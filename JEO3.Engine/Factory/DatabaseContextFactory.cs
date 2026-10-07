using System.Diagnostics;
using JEO3.Engine.Entities;
using JEO3.Engine.Entities.JEO3.Engine.Models;
using JEO3.Engine.Models;
using JEO3.Logging;
using JEO3.Providers;
using JEO3.Schema;

namespace JEO3.Engine
{
    /// <summary>
    /// Hydrates provider-specific flat metadata DTOs into the provider-agnostic JEO3.Schema object model.
    /// The Flat* layer is intentionally optimized for retrieval. This class is the normalization boundary
    /// between the flat database metadata representation and the metadata domain model.
    /// </summary>
    public static class DatabaseContextFactory
    {
        #region Entry Point

        public static async Task<DatabaseContext> GetContext(IDatabaseProvider provider)
        {
            var flatCtx = await StagingContextFactory.GetFlatContext(provider, true);
            var ctx = GetContext(flatCtx, provider);

            return ctx;
        }

        public static DatabaseContext GetContext(IFlatDatabaseContext ctx, IDatabaseProvider provider)
        {
            try
            {
                DateTime now = DateTime.Now;
                var columns = ctx.Columns.OrderBy(v => v.SchemaName).ThenBy(v => v.Table).ThenBy(v => v.ColumnId).ToList();
                var relations = ctx.Relations.OrderBy(v => v.PrimaryColumnPath).ThenBy(v => v.ForeignColumnPath).ToList();
                var triggerModels = ctx.Triggers.OrderBy(v => v.Name).ToList();
                var indexes = ctx.Indexes.OrderBy(v => v.SchemaName).ThenBy(v => v.TableName).ThenBy(v => v.ColumnId).ToList();
                var procedures = ctx.Procedures.OrderBy(v => v.SchemaName).ThenBy(v => v.Name).ToList();

                var flatViews = ctx.Views?.ToList() ?? [];
                var flatFunctions = ctx.Functions?.ToList() ?? [];
                var flatUserDefinedTypes = ctx.UserDefinedTypes?.ToList() ?? [];
                var checkConstraintModels = ctx.CheckConstraints?.ToList() ?? [];
                var checkConstraintsByTable = checkConstraintModels.GroupBy(x => x.ParentObjectId).ToDictionary(x => x.Key, x => (IReadOnlyList<ICheckConstraint>)x.ToList());
                var missingIndexModels = ctx.MissingIndexes?.ToList() ?? [];

                // COLUMNS
                var columnsByObjectId = columns.GroupBy(x => x.ObjectId ?? 0).ToDictionary(x => x.Key, x => (IReadOnlyList<Schema.IColumn>)x.ToList());

                // INDEXES
                var indexHydration = HydrateIndexes(indexes);
                var indexModels = indexHydration.Select(x => x.Index).ToList();
                var indexesByTable = indexHydration.GroupBy(x => x.TableKey).ToDictionary(x => x.Key, x => (IReadOnlyList<IIndex>)x.Select(y => y.Index).ToList(), StringComparer.OrdinalIgnoreCase);

                // TABLES
                var tableList = ctx.Tables.OrderBy(v => v.DatabaseName).ThenBy(v => v.TablePath).ToList();

                // Database Set
                foreach (var db in ctx.Databases)
                {
                    db.Schemas = ctx.Schemas;
                }

                // Schemas Set
                foreach (var schema in ctx.Schemas)
                {
                    schema.Database = ctx.Databases.FirstOrDefault();
                    schema.Tables = tableList.Where(v => v.SchemaName == schema.Name).ToList();
                    schema.Parent = ctx.Databases.FirstOrDefault();
                }

                // Tables Set
                foreach (var table in tableList)
                {
                    MapTable(table, columnsByObjectId, indexesByTable);
                    table.Database = ctx.Databases.FirstOrDefault();
                    table.Schema = ctx.Schemas.FirstOrDefault(v => v.Name == table.SchemaName);
                    table.Parent = table.Database;
                }
                var tablesByKey = tableList.ToDictionary(x => TableKey(x.SchemaName, x.Name), StringComparer.OrdinalIgnoreCase);
                var schemasByName = ctx.Schemas.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);

                // RELATIONS / FOREIGN KEYS
                var relationModels = HydrateRelations(relations, tablesByKey);

                // RELATIONS BY TABLE
                var relationsByTable = relationModels
                    .SelectMany(relation => GetRelationTableKeys(relation).Select(tableKey => new { TableKey = tableKey, Relation = relation }))
                    .GroupBy(x => x.TableKey, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(x => x.Key, x => (IReadOnlyList<Schema.IRelation>)x.Select(y => y.Relation).ToList(), StringComparer.OrdinalIgnoreCase);

                // MISSING INDEXES BY TABLE
                var missingIndexesByTable = missingIndexModels.Where(x => x.ObjectId.HasValue).GroupBy(x => x.ObjectId!.Value).ToDictionary(x => x.Key, x => (IReadOnlyList<MissingIndex>)x.ToList());

                // CHECK CONSTRAINTS BY COLUMN PATH
                var constraintsByPath = checkConstraintModels.Where(cc => !string.IsNullOrEmpty(cc.Path)).GroupBy(cc => cc.Path!).ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

                // STATISTICS, TRIGGERS & EXTENDED PROPERTIES HYDRATION
                var statisticModels = ctx.Statistics?.ToList() ?? [];
                var statisticsByTable = statisticModels.GroupBy(x => x.ParentObjectId).ToDictionary(x => x.Key, x => (IReadOnlyList<IStatistic>)x.ToList());

                var triggersByTable = triggerModels.GroupBy(x => x.ParentObjectId).ToDictionary(x => x.Key, x => (IReadOnlyList<ITrigger>)x.ToList());

                var extendedPropertyModels = ctx.ExtendedProperties?.ToList() ?? [];
                var extendedPropertiesByTable = extendedPropertyModels.Where(v => v.ParentObjectId != null).GroupBy(x => x.ParentObjectId!.Value).ToDictionary(x => x.Key, x => (IReadOnlyList<IExtendedProperty>)x.ToList());

                // TABLE NAVIGATION WIRING
                foreach (var table in tableList)
                {
                    var objectId = table.ObjectId.GetValueOrDefault();
                    var tableKey = TableKey(table.SchemaName, table.Name);

                    // TABLE -> MISSING INDEXES
                    var tableMissingIndexes = missingIndexesByTable.TryGetValue(objectId, out var missing) ? missing : Array.Empty<MissingIndex>();
                    table.MissingIndexes = tableMissingIndexes;

                    // TABLE -> STATISTICS
                    table.Statistics = statisticsByTable.TryGetValue(objectId, out var stats) ? stats : Array.Empty<IStatistic>();

                    // TABLE -> TRIGGERS
                    table.Triggers = triggersByTable.TryGetValue(objectId, out var trig) ? trig : Array.Empty<ITrigger>();

                    // TABLE -> EXTENDED PROPERTIES
                    table.ExtendedProperties = extendedPropertiesByTable.TryGetValue(objectId, out var extProps) ? extProps : Array.Empty<IExtendedProperty>();

                    // TABLE -> RELATIONS / FOREIGN KEYS
                    var tableRelations = relationsByTable.TryGetValue(tableKey, out var relationsForTable) ? relationsForTable : Array.Empty<Schema.IRelation>();
                    table.Relations = tableRelations;

                    // TABLE -> SCHEMA (Parent)
                    if (schemasByName.TryGetValue(table.SchemaName ?? string.Empty, out var schema))
                    {
                        table.Parent = schema;
                    }

                    // TABLE -> CHECK CONSTRAINTS
                    table.CheckConstraints = checkConstraintsByTable.TryGetValue(objectId, out var constraints) ? constraints : Array.Empty<ICheckConstraint>();

                    // COLUMN -> CHECK CONSTRAINTS (exact column Path match: Schema.Table.Column)
                    foreach (var column in table.Columns)
                    {
                        if (column is Schema.Column concreteColumn && !string.IsNullOrEmpty(concreteColumn.Path))
                            concreteColumn.CheckConstraints = constraintsByPath.TryGetValue(concreteColumn.Path, out var colConstraints) ? colConstraints.Where(v => v != null).ToList() : [];

                        if (table.MissingIndexes.Count > 0)
                        {
                            ((Column)column).MissingIndexes = tableMissingIndexes.Where(v => v.EqualityColumns != null && v.EqualityColumnsString?.Contains(column.Name) == true).ToList();
                        }
                    }

                    // ForeignKeys on this table (this table is the Child holding the FK constraint)
                    table.ForeignKeys = tableRelations.OfType<IForeignKey>().Where(fk => fk.ReferencedTable != null && string.Equals(fk.ReferencedTable.SchemaName, table.SchemaName, StringComparison.OrdinalIgnoreCase) && string.Equals(fk.ReferencedTable.Name, table.Name, StringComparison.OrdinalIgnoreCase)).ToList();

                    // ReferencedByForeignKeys on this table (this table is the Parent / PK table being pointed to)
                    table.ReferencedByForeignKeys = tableRelations.OfType<IForeignKey>().Where(fk => fk.ParentTable != null && string.Equals(fk.ParentTable.SchemaName, table.SchemaName, StringComparison.OrdinalIgnoreCase) && string.Equals(fk.ParentTable.Name, table.Name, StringComparison.OrdinalIgnoreCase)).ToList();

                    // TABLE -> COLUMNS
                    if (columnsByObjectId.TryGetValue(objectId, out var tableColumns))
                    {
                        table.Columns = tableColumns;
                        foreach (var column in tableColumns)
                            if (column is Schema.Column metadataColumn) { metadataColumn.Table = table; metadataColumn.Parent = table; }
                    }
                    // TABLE -> INDEXES
                    if (indexesByTable.TryGetValue(tableKey, out var tableIndexes))
                    {
                        table.Indexes = tableIndexes;
                        foreach (var index in tableIndexes)
                            if (index is Schema.Index metadataIndex) { metadataIndex.Table = table; metadataIndex.Parent = table; }
                    }

                    // WIRE MISSING INDEX COLUMNS
                    foreach (var index in table.MissingIndexes)
                    {
                        var eqColNames = index.EqualityColumnsString?.Split('|').ToList() ?? [];
                        var ineqColNames = index.InequalityColumnsString?.Split('|').ToList() ?? [];
                        var inclColNames = index.IncludedColumnsString?.Split('|').ToList() ?? [];
                        var suggColNames = index.SuggestedKeyColumnsString?.Split('|').ToList() ?? [];
                        ((MissingIndex)index).IncludedColumns = table.Columns.Where(v => inclColNames.Contains(v.Name)).ToList();
                        ((MissingIndex)index).InequalityColumns = table.Columns.Where(v => ineqColNames.Contains(v.Name)).ToList();
                        ((MissingIndex)index).SuggestedKeyColumns = table.Columns.Where(v => suggColNames.Contains(v.Name)).ToList();
                        ((MissingIndex)index).EqualityColumns = table.Columns.Where(v => eqColNames.Contains(v.Name)).ToList();
                    }
                }

                // COLUMN INDEX LINKS
                var columnIndexLinks = indexes.Select(MapColumnIndexLink).ToList();

                // COLUMN INDEX LINK LOOKUPS
                var columnLinksByColumn = columnIndexLinks.GroupBy(x => new { x.ObjectName, x.ColumnId }).ToDictionary(x => x.Key, x => (IReadOnlyList<IColumnIndexLink>)x.ToList());

                var indexByTableAndId = tableList
                    .SelectMany(table => table.Indexes.Where(index => index.IndexId.HasValue).Select(index => new { Key = $"{table.SchemaName}.{table.Name}.{index.IndexId!.Value}", Index = index }))
                    .ToDictionary(x => x.Key, x => x.Index, StringComparer.OrdinalIgnoreCase);

                // COLUMN NAVIGATION WIRING
                foreach (var column in columns)
                {
                    // COLUMN -> INDEX LINKS
                    var columnTableName = column.TableName ?? string.Empty;
                    if (column.ColumnId.HasValue && columnLinksByColumn.TryGetValue(new { ObjectName = columnTableName, ColumnId = column.ColumnId }, out var columnLinks))
                        column.IndexLinks = columnLinks;

                    // COLUMN -> INDEXES
                    column.Indexes = column.IndexLinks
                        .Select(link => { var indexKey = $"{column.SchemaName}.{column.TableName}.{link.IndexId}"; return indexByTableAndId.TryGetValue(indexKey, out var index) ? index : null; })
                        .Where(index => index != null)
                        .Cast<IIndex>()
                        .Distinct()
                        .ToList();
                }

                // COLUMN <-> FOREIGN KEY NAVIGATION
                var parentColumnForeignKeys = new Dictionary<Schema.Column, List<IForeignKey>>();
                var referencedColumnForeignKeys = new Dictionary<Schema.Column, List<IForeignKey>>();

                foreach (var foreignKey in relationModels)
                {
                    if (foreignKey.ParentTable is null || foreignKey.ReferencedTable is null) continue;

                    foreach (var pair in foreignKey.ColumnPairs)
                    {
                        // PARENT / FOREIGN KEY COLUMN
                        var parentColumn = foreignKey.ParentTable.Columns.FirstOrDefault(column => string.Equals(column.Name, pair.ParentColumnName, StringComparison.OrdinalIgnoreCase));
                        if (parentColumn is Schema.Column parentMetadataColumn)
                        {
                            if (!parentColumnForeignKeys.TryGetValue(parentMetadataColumn, out var fks)) parentColumnForeignKeys[parentMetadataColumn] = fks = [];
                            if (!fks.Contains(foreignKey)) fks.Add(foreignKey);
                        }
                        pair.ParentColumn = parentColumn as Schema.IColumn;

                        // REFERENCED COLUMN
                        var referencedColumn = foreignKey.ReferencedTable.Columns.FirstOrDefault(column => string.Equals(column.Name, pair.ReferencedColumnName, StringComparison.OrdinalIgnoreCase));
                        if (referencedColumn is Schema.Column referencedMetadataColumn)
                        {
                            if (!referencedColumnForeignKeys.TryGetValue(referencedMetadataColumn, out var fks)) referencedColumnForeignKeys[referencedMetadataColumn] = fks = [];
                            if (!fks.Contains(foreignKey)) fks.Add(foreignKey);
                        }
                        pair.ReferencedColumn = referencedColumn as Schema.IColumn;
                    }
                }

                foreach (var (col, fks) in parentColumnForeignKeys) col.ForeignKeys = fks;
                foreach (var (col, fks) in referencedColumnForeignKeys) col.ReferencedByForeignKeys = fks;

                // VIEW MODELS
                var viewModels = MapView(flatViews, columnsByObjectId).ToList();

                // VIEW COLUMN NAVIGATION (prevent view columns from thinking they belong to a Table)
                foreach (var view in viewModels)
                {
                    if (columnsByObjectId.TryGetValue(view.ObjectId.GetValueOrDefault(), out var viewColumns))
                        foreach (var column in viewColumns)
                            if (column is Schema.Column metadataColumn) metadataColumn.Table = null;
                }

                // PROCEDURES / FUNCTIONS / USER DEFINED TYPES
                var procedureModels = MapProcedure(procedures).ToList();
                var functionModels = MapFunction(flatFunctions).ToList();

                // SYNONYM & TARGET TABLE NAVIGATION PASS
                var synonymsByTargetTable = ctx.Synonyms.ToLookup(syn => TableKey(syn.TargetSchemaName, syn.TargetTableName), StringComparer.OrdinalIgnoreCase);

                foreach (var table in tableList)
                {
                    var tableKey = TableKey(table.SchemaName, table.Name);
                    if (synonymsByTargetTable.Contains(tableKey))
                    {
                        var matchedSynonyms = synonymsByTargetTable[tableKey].ToList();
                        foreach (var syn in matchedSynonyms) syn.TargetTable = table;
                        table.Synonyms = matchedSynonyms;
                    }
                }

                var finalColumns = tableList.SelectMany(v => v.Columns).Cast<Schema.IColumn>().ToList();
                var userDefinedTypeModels = MapUserDefinedType(flatUserDefinedTypes, finalColumns).ToList();

                var database = ctx.Databases.First();

                // FINAL CONTEXT
                // At this point every canonical object collection has been created and the graph
                // navigation has been wired. The same instances are exposed through every navigation path.
                var newCtx = new DatabaseContext
                {
                    Provider = provider,
                    Database = database,
                    Schemas = ctx.Schemas.ToList(),
                    Tables = tableList,
                    Views = viewModels,
                    Procedures = procedureModels,
                    Functions = functionModels,
                    UserDefinedTypes = userDefinedTypeModels,
                    Relations = relationModels.Cast<Schema.IRelation>().ToList(),
                    ForeignKeys = relationModels.Cast<IForeignKey>().ToList(),
                    Indexes = indexModels,
                    ColumnIndexLinks = columnIndexLinks,
                    Columns = finalColumns,
                    CheckConstraints = checkConstraintModels,
                    MissingIndexes = missingIndexModels,
                    Statistics = statisticModels,
                    Triggers = triggerModels,
                    Synonyms = synonymsByTargetTable.SelectMany(g => g).ToList(),
                    ExtendedProperties = extendedPropertyModels
                };

                var log = Math.Round((DateTime.Now - now).TotalSeconds, 2);
                Trace.WriteLine($"GetContext - Elapsed {log}sec");

                return newCtx;
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex);
                throw;
            }
        }

        public static DatabaseContext GetContext_Original(IFlatDatabaseContext ctx, IDatabaseProvider provider)
        {
            try
            {
                DateTime now = DateTime.Now;
                //IEnumerable<Table> tables = ctx.Tables.OrderBy(v => v.DatabaseName).ThenBy(v => v.TablePath).ToList();
                var columns = ctx.Columns.OrderBy(v => v.SchemaName).ThenBy(v => v.Table).ThenBy(v => v.ColumnId).ToList();
                var relations = ctx.Relations.OrderBy(v => v.PrimaryColumnPath).ThenBy(v => v.ForeignColumnPath).ToList();
                var triggers = ctx.Triggers.OrderBy(v => v.Name).ToList();
                var indexes = ctx.Indexes.OrderBy(v => v.SchemaName).ThenBy(v => v.TableName).ThenBy(v => v.ColumnId).ToList();
                var procedures = ctx.Procedures.OrderBy(v => v.SchemaName).ThenBy(v => v.Name).ToList();
                var views = ctx.Views.OrderBy(v => v.SchemaName).ThenBy(v => v.Name).ToList();

                var flatIndexes = indexes?.ToList() ?? [];
                var flatRelations = relations?.ToList() ?? [];
                var flatProcedures = procedures?.ToList() ?? [];
                var flatViews = ctx.Views?.ToList() ?? [];
                var flatFunctions = ctx.Functions?.ToList() ?? [];
                var flatUserDefinedTypes = ctx.UserDefinedTypes?.ToList() ?? [];
                var checkConstraintModels = ctx.CheckConstraints?.ToList() ?? []; //HydrateCheckConstraints(checkConstraints.ToList());
                var checkConstraintsByTable = checkConstraintModels.GroupBy(x => x.ParentObjectId).ToDictionary(x => x.Key, x => (IReadOnlyList<ICheckConstraint>)x.ToList());
                var missingIndexModels = ctx.MissingIndexes?.ToList() ?? []; // HydrateMissingIndexes(missingIndexes.ToList());

                // COLUMNS
                var columnsByObjectId = columns.GroupBy(x => x.ObjectId ?? 0).ToDictionary(x => x.Key, x => (IReadOnlyList<Schema.IColumn>)x.ToList());

                // INDEXES
                var indexHydration = HydrateIndexes(flatIndexes);
                var indexModels = indexHydration.Select(x => x.Index).ToList();
                var indexesByTable = indexHydration.GroupBy(x => x.TableKey).ToDictionary(x => x.Key, x => (IReadOnlyList<IIndex>)x.Select(y => y.Index).ToList(), StringComparer.OrdinalIgnoreCase);

                // TABLES
                var tableList = ctx.Tables.OrderBy(v => v.DatabaseName).ThenBy(v => v.TablePath).ToList();

                // Database Set
                foreach (var db in ctx.Databases)
                {
                    //db.Tables = tableList.Where(v => v.DatabaseName == db.Name).ToList();
                    db.Schemas = ctx.Schemas;
                }

                // Schemas Set
                foreach (var schema in ctx.Schemas)
                {
                    schema.Database = ctx.Databases.FirstOrDefault();
                    schema.Tables = tableList.Where(v => v.SchemaName == schema.Name).ToList();
                    schema.Parent = ctx.Databases.FirstOrDefault();
                }

                // Tables Set
                foreach (var table in tableList)
                {
                    MapTable(table, columnsByObjectId, indexesByTable);
                    table.Database = ctx.Databases.FirstOrDefault();
                    table.Schema = ctx.Schemas.FirstOrDefault(v => v.Name == table.SchemaName);
                    table.Parent = table.Database;
                }
                var tablesByKey = tableList.ToDictionary(x => TableKey(x.SchemaName, x.Name), StringComparer.OrdinalIgnoreCase);
                var schemasByName = ctx.Schemas.ToDictionary(s => s.Name, StringComparer.OrdinalIgnoreCase);

                // RELATIONS / FOREIGN KEYS
                var relationModels = HydrateRelations(flatRelations, tablesByKey);

                // RELATIONS BY TABLE
                var relationsByTable = relationModels
                    .SelectMany(relation => GetRelationTableKeys(relation).Select(tableKey => new { TableKey = tableKey, Relation = relation }))
                    .GroupBy(x => x.TableKey, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(x => x.Key, x => (IReadOnlyList<Schema.IRelation>)x.Select(y => y.Relation).ToList(), StringComparer.OrdinalIgnoreCase);

                // MISSING INDEXES BY TABLE
                var missingIndexesByTable = missingIndexModels.Where(x => x.ObjectId.HasValue).GroupBy(x => x.ObjectId!.Value).ToDictionary(x => x.Key, x => (IReadOnlyList<MissingIndex>)x.ToList());

                // CHECK CONSTRAINTS BY COLUMN PATH
                var constraintsByPath = checkConstraintModels.Where(cc => !string.IsNullOrEmpty(cc.Path)).GroupBy(cc => cc.Path!).ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

                // STATISTICS, TRIGGERS & EXTENDED PROPERTIES HYDRATION
                var statisticModels = ctx.Statistics?.ToList() ?? []; //HydrateStatistics(statistics);
                var statisticsByTable = statisticModels.GroupBy(x => x.ParentObjectId).ToDictionary(x => x.Key, x => (IReadOnlyList<IStatistic>)x.ToList());

                var triggerModels = triggers?.ToList() ?? []; // HydrateTriggers(triggers);
                var triggersByTable = triggerModels.GroupBy(x => x.ParentObjectId).ToDictionary(x => x.Key, x => (IReadOnlyList<ITrigger>)x.ToList());

                var extendedPropertyModels = ctx.ExtendedProperties?.ToList() ?? []; // HydrateExtendedProperties(extendedProperties);
                var extendedPropertiesByTable = extendedPropertyModels.Where(v => v.ParentObjectId != null).GroupBy(x => x.ParentObjectId!.Value).ToDictionary(x => x.Key, x => (IReadOnlyList<IExtendedProperty>)x.ToList());

                // TABLE NAVIGATION WIRING
                foreach (var table in tableList)
                {
                    var objectId = table.ObjectId.GetValueOrDefault();
                    var tableKey = TableKey(table.SchemaName, table.Name);

                    // TABLE -> MISSING INDEXES
                    var tableMissingIndexes = missingIndexesByTable.TryGetValue(objectId, out var missing) ? missing : Array.Empty<MissingIndex>();
                    table.MissingIndexes = tableMissingIndexes;

                    // TABLE -> STATISTICS
                    table.Statistics = statisticsByTable.TryGetValue(objectId, out var stats) ? stats : Array.Empty<IStatistic>();

                    // TABLE -> TRIGGERS
                    table.Triggers = triggersByTable.TryGetValue(objectId, out var trig) ? trig : Array.Empty<ITrigger>();

                    // TABLE -> EXTENDED PROPERTIES
                    table.ExtendedProperties = extendedPropertiesByTable.TryGetValue(objectId, out var extProps) ? extProps : Array.Empty<IExtendedProperty>();

                    // TABLE -> RELATIONS / FOREIGN KEYS
                    var tableRelations = relationsByTable.TryGetValue(tableKey, out var relationsForTable) ? relationsForTable : Array.Empty<Schema.IRelation>();
                    table.Relations = tableRelations;

                    // TABLE -> SCHEMA (Parent)
                    if (schemasByName.TryGetValue(table.SchemaName ?? string.Empty, out var schema))
                    {
                        table.Parent = schema;
                    }

                    // TABLE -> CHECK CONSTRAINTS
                    table.CheckConstraints = checkConstraintsByTable.TryGetValue(objectId, out var constraints) ? constraints : Array.Empty<ICheckConstraint>();

                    // COLUMN -> CHECK CONSTRAINTS (exact column Path match: Schema.Table.Column)
                    foreach (var column in table.Columns)
                    {
                        if (column is Schema.Column concreteColumn && !string.IsNullOrEmpty(concreteColumn.Path))
                            concreteColumn.CheckConstraints = constraintsByPath.TryGetValue(concreteColumn.Path, out var colConstraints) ? colConstraints.Where(v => v != null).ToList() : [];

                        if (table.MissingIndexes.Count > 0)
                        {
                            //var parts = table.MissingIndexes.Where(v => v.EqualityColumns != null).SelectMany(v => v.EqualityColumns.Split(',').Select(v => v.Trim().TrimStart('[').TrimEnd(']'))).ToList(); 
                            ((Column)column).MissingIndexes = tableMissingIndexes.Where(v => v.EqualityColumns != null && v.EqualityColumnsString?.Contains(column.Name) == true).ToList();
                        }
                    }

                    // ForeignKeys on this table (this table is the Child holding the FK constraint)
                    table.ForeignKeys = tableRelations.OfType<IForeignKey>().Where(fk => fk.ReferencedTable != null && string.Equals(fk.ReferencedTable.SchemaName, table.SchemaName, StringComparison.OrdinalIgnoreCase) && string.Equals(fk.ReferencedTable.Name, table.Name, StringComparison.OrdinalIgnoreCase)).ToList();

                    // ReferencedByForeignKeys on this table (this table is the Parent / PK table being pointed to).
                    // NOTE: this is the ONLY place this gets set now - a later block used to overwrite it with an
                    // incorrect computation that effectively duplicated table.ForeignKeys. That block is removed.
                    table.ReferencedByForeignKeys = tableRelations.OfType<IForeignKey>().Where(fk => fk.ParentTable != null && string.Equals(fk.ParentTable.SchemaName, table.SchemaName, StringComparison.OrdinalIgnoreCase) && string.Equals(fk.ParentTable.Name, table.Name, StringComparison.OrdinalIgnoreCase)).ToList();
                    foreach (var refFk in table.ReferencedByForeignKeys)
                    {
                    }

                    // TABLE -> COLUMNS
                    if (columnsByObjectId.TryGetValue(objectId, out var tableColumns))
                    {
                        table.Columns = tableColumns;
                        foreach (var column in tableColumns)
                            if (column is Schema.Column metadataColumn) { metadataColumn.Table = table; metadataColumn.Parent = table; }
                    }
                    // TABLE -> INDEXES
                    if (indexesByTable.TryGetValue(tableKey, out var tableIndexes))
                    {
                        table.Indexes = tableIndexes;
                        foreach (var index in tableIndexes)
                            if (index is Schema.Index metadataIndex) { metadataIndex.Table = table; metadataIndex.Parent = table; }
                    }

                    // WIRE MISSING INDEX COLUMNS
                    foreach (var index in table.MissingIndexes)
                    {
                        var eqColNames = index.EqualityColumnsString?.Split('|').ToList() ?? [];
                        var ineqColNames = index.InequalityColumnsString?.Split('|').ToList() ?? [];
                        var inclColNames = index.IncludedColumnsString?.Split('|').ToList() ?? [];
                        var suggColNames = index.SuggestedKeyColumnsString?.Split('|').ToList() ?? [];
                        ((MissingIndex)index).IncludedColumns = table.Columns.Where(v => inclColNames.Contains(v.Name)).ToList();
                        ((MissingIndex)index).InequalityColumns = table.Columns.Where(v => ineqColNames.Contains(v.Name)).ToList();
                        ((MissingIndex)index).SuggestedKeyColumns = table.Columns.Where(v => suggColNames.Contains(v.Name)).ToList();
                        ((MissingIndex)index).EqualityColumns = table.Columns.Where(v => eqColNames.Contains(v.Name)).ToList();
                    }
                }

                // COLUMN INDEX LINKS
                var columnIndexLinks = flatIndexes.Select(MapColumnIndexLink).ToList();

                // COLUMN INDEX LINK LOOKUPS
                var columnLinksByColumn = columnIndexLinks.GroupBy(x => new { x.ObjectName, x.ColumnId }).ToDictionary(x => x.Key, x => (IReadOnlyList<IColumnIndexLink>)x.ToList());

                var indexByTableAndId = tableList
                    .SelectMany(table => table.Indexes.Where(index => index.IndexId.HasValue).Select(index => new { Key = $"{table.SchemaName}.{table.Name}.{index.IndexId!.Value}", Index = index }))
                    .ToDictionary(x => x.Key, x => x.Index, StringComparer.OrdinalIgnoreCase);

                // COLUMN NAVIGATION WIRING
                foreach (var column in columns)
                {
                    // COLUMN -> INDEX LINKS
                    var columnTableName = column.TableName ?? string.Empty;
                    if (column.ColumnId.HasValue && columnLinksByColumn.TryGetValue(new { ObjectName = columnTableName, ColumnId = column.ColumnId }, out var columnLinks))
                        column.IndexLinks = columnLinks;

                    // COLUMN -> INDEXES
                    column.Indexes = column.IndexLinks
                        .Select(link => { var indexKey = $"{column.SchemaName}.{column.TableName}.{link.IndexId}"; return indexByTableAndId.TryGetValue(indexKey, out var index) ? index : null; })
                        .Where(index => index != null)
                        .Cast<IIndex>()
                        .Distinct()
                        .ToList();
                }

                // COLUMN <-> FOREIGN KEY NAVIGATION
                foreach (var foreignKey in relationModels)
                {
                    if (foreignKey.ParentTable is null || foreignKey.ReferencedTable is null) continue;

                    foreach (var pair in foreignKey.ColumnPairs)
                    {
                        // PARENT / FOREIGN KEY COLUMN
                        var parentColumn = foreignKey.ParentTable.Columns.FirstOrDefault(column => string.Equals(column.Name, pair.ParentColumnName, StringComparison.OrdinalIgnoreCase));
                        if (parentColumn is Schema.Column parentMetadataColumn)
                            parentMetadataColumn.ForeignKeys = parentMetadataColumn.ForeignKeys.Append(foreignKey).Distinct().ToList();
                        pair.ParentColumn = parentColumn as Schema.IColumn;

                        // REFERENCED COLUMN
                        var referencedColumn = foreignKey.ReferencedTable.Columns.FirstOrDefault(column => string.Equals(column.Name, pair.ReferencedColumnName, StringComparison.OrdinalIgnoreCase));
                        if (referencedColumn is Schema.Column referencedMetadataColumn)
                            referencedMetadataColumn.ReferencedByForeignKeys = referencedMetadataColumn.ReferencedByForeignKeys.Append(foreignKey).Distinct().ToList();
                        pair.ReferencedColumn = referencedColumn as Schema.IColumn;
                    }
                }

                // VIEW MODELS
                var viewModels = MapView(flatViews, columnsByObjectId).ToList();

                // VIEW COLUMN NAVIGATION (prevent view columns from thinking they belong to a Table)
                foreach (var view in viewModels)
                {
                    if (columnsByObjectId.TryGetValue(view.ObjectId.GetValueOrDefault(), out var viewColumns))
                        foreach (var column in viewColumns)
                            if (column is Schema.Column metadataColumn) metadataColumn.Table = null;
                }

                // PROCEDURES / FUNCTIONS / USER DEFINED TYPES
                var procedureModels = MapProcedure(flatProcedures).ToList();
                var functionModels = MapFunction(flatFunctions).ToList();

                // SYNONYM & TARGET TABLE NAVIGATION PASS
                // Group all synonyms into a fast lookup by their target table key strings
                var synonymsByTargetTable = ctx.Synonyms
                    .ToLookup(syn => TableKey(syn.TargetSchemaName, syn.TargetTableName), StringComparer.OrdinalIgnoreCase);

                // Multi-map the navigation links in a single loop pass over tables
                foreach (var table in tableList)
                {
                    var tableKey = TableKey(table.SchemaName, table.Name);

                    if (synonymsByTargetTable.Contains(tableKey))
                    {
                        // Materialize the read-only list for the table
                        var matchedSynonyms = synonymsByTargetTable[tableKey].ToList();

                        // Set the backlink on each synonym pointing to this Table instance
                        foreach (var syn in matchedSynonyms)
                        {
                            syn.TargetTable = table;
                        }

                        // Assign the immutable list directly to the table asset
                        table.Synonyms = matchedSynonyms;
                    }
                }

                var finalColumns = tableList.SelectMany(v => v.Columns).Cast<Schema.IColumn>().ToList();
                var userDefinedTypeModels = MapUserDefinedType(flatUserDefinedTypes, finalColumns).ToList();


                var database = ctx.Databases.First();
                // FINAL CONTEXT
                // At this point every canonical object collection has been created and the graph
                // navigation has been wired. The same instances are exposed through every navigation path.
                var newCtx = new DatabaseContext
                {
                    Provider = provider,
                    Database = database,
                    Schemas = ctx.Schemas.ToList(),
                    Tables = tableList,
                    Views = viewModels,
                    Procedures = procedureModels,
                    Functions = functionModels,
                    UserDefinedTypes = userDefinedTypeModels,
                    Relations = relationModels.Cast<Schema.IRelation>().ToList(),
                    ForeignKeys = relationModels.Cast<IForeignKey>().ToList(),
                    Indexes = indexModels,
                    ColumnIndexLinks = columnIndexLinks,
                    Columns = finalColumns,
                    CheckConstraints = checkConstraintModels.ToList(),
                    MissingIndexes = missingIndexModels,
                    Statistics = statisticModels,
                    Triggers = triggerModels,
                    //Principals = ctx.Principals.Select(v => new  ).ToList(),
                    //Users = principalList.OfType<DatabaseUser>().ToList(),
                    //Roles = principalList.OfType<DatabaseRole>().ToList(),
                    //ApplicationRoles = principalList.OfType<ApplicationRole>().ToList(),
                    Synonyms = synonymsByTargetTable.SelectMany(g => g).ToList(),
                    ExtendedProperties = extendedPropertyModels
                };

                var log = Math.Round((DateTime.Now - now).TotalSeconds, 2);
                Trace.WriteLine($"PollOnce - Elapsed {log}sec");

                return newCtx;
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex);
                throw;
            }
        }

        #endregion

        #region Tables

        private static void MapTable(Table source, IReadOnlyDictionary<int, IReadOnlyList<Schema.IColumn>> columnsByObjectId, IReadOnlyDictionary<string, IReadOnlyList<IIndex>> indexesByTable)
        {
            columnsByObjectId.TryGetValue(source.ObjectId.GetValueOrDefault(), out var columns);
            var tableKey = TableKey(source.SchemaName, source.Name);
            indexesByTable.TryGetValue(tableKey, out var indexes);

            source.Columns = columns ?? Array.Empty<Schema.IColumn>();
            source.Indexes = indexes ?? Array.Empty<IIndex>();
            source.Relations = Array.Empty<Schema.IRelation>();
            source.ForeignKeys = Array.Empty<IForeignKey>();
            source.ColumnIndexLinks = Array.Empty<IColumnIndexLink>();
        }

        #endregion

        #region Indexes

        private sealed record HydratedIndex(string TableKey, Schema.Index Index);

        private static IReadOnlyList<HydratedIndex> HydrateIndexes(IReadOnlyList<FlatIndex> source)
        {
            return source
                .GroupBy(x => new { x.SchemaName, x.TableName, x.IndexId })
                .Select(group =>
                {
                    var first = group.First();
                    var indexColumns = group.OrderBy(x => x.IndexOrdinalPosition).ThenBy(x => x.IndexColumnId).Select(MapIndexColumn).ToList();
                    var tableKey = TableKey(first.SchemaName, first.TableName);

                    var index = new Schema.Index
                    {
                        ObjectId = null,
                        SchemaName = first.SchemaName,
                        IndexId = first.IndexId,
                        Name = first.Name,
                        IndexType = MapIndexType(first.IndexType),
                        ProviderIndexType = first.IndexType,
                        IsUnique = first.IsUnique,
                        IsPrimaryKey = false,
                        IsUniqueConstraint = first.IsUniqueConstraint,
                        IsDisabled = first.IsDisabled,
                        IsClustered = first.IsClustered,
                        IsCovering = group.Any(x => x.IsIncluded),
                        FragmentationPercentage = first.FragmentationPercentage,
                        PageCount = first.PageCount,
                        Columns = indexColumns,
                        KeyColumns = group.Where(x => !x.IsIncluded).OrderBy(x => x.IndexOrdinalPosition).Select(MapIndexColumnAsColumn).Cast<Schema.IColumn>().ToList(),
                        IncludedColumns = group.Where(x => x.IsIncluded).OrderBy(x => x.IndexOrdinalPosition).Select(MapIndexColumnAsColumn).Cast<Schema.IColumn>().ToList()
                    };

                    return new HydratedIndex(tableKey, index);
                })
                .ToList();
        }

        private static IndexColumn MapIndexColumn(FlatIndex source)
        {
            return new IndexColumn
            {
                ObjectId = null,
                IndexId = source.IndexId,
                ColumnId = source.ColumnId,
                IndexName = source.Name,
                ColumnName = source.ColumnName,
                Ordinal = source.IndexOrdinalPosition,
                IsIncluded = source.IsIncluded,
                IsKeyColumn = !source.IsIncluded,
                SortDirection = IndexColumnSortDirection.Unknown,
                IsDescending = false
            };
        }

        private static Schema.Column MapIndexColumnAsColumn(FlatIndex source)
        {
            return new Schema.Column
            {
                ObjectId = null,
                ColumnId = source.ColumnId,
                Name = source.ColumnName,
                DataType = source.DataType,
                ProviderDataType = source.DataType,
                TableName = source.TableName,
                SchemaName = source.SchemaName,
                IsIndexedInDatabase = true
            };
        }

        private static ColumnIndexLink MapColumnIndexLink(FlatIndex source)
        {
            return new ColumnIndexLink
            {
                ObjectId = null,
                ColumnId = source.ColumnId,
                IndexId = source.IndexId,
                ObjectName = source.TableName,
                ColumnName = source.ColumnName,
                IndexName = source.Name,
                IsIncludedColumn = source.IsIncluded,
                OrdinalPosition = source.IsIncluded ? null : source.IndexOrdinalPosition,
                IncludedOrdinal = source.IsIncluded ? source.IndexOrdinalPosition : null,
                SortDirection = IndexColumnSortDirection.Unknown,
                IsDescending = false,
                IsIndexDisabled = source.IsDisabled,
                IsUnique = source.IsUnique,
                IsPrimaryKey = false,
                IsUniqueConstraint = source.IsUniqueConstraint
            };
        }

        private static IndexType MapIndexType(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? IndexType.Unknown : value.Trim().ToLowerInvariant() switch
            {
                "heap" => IndexType.Heap,
                "clustered" => IndexType.Clustered,
                "nonclustered" => IndexType.NonClustered,
                "non-clustered" => IndexType.NonClustered,
                "hash" => IndexType.Hash,
                "columnstore" => IndexType.ColumnStore,
                _ => IndexType.Other
            };
        }

        #endregion

        #region Relations

        private static IReadOnlyList<ForeignKey> HydrateRelations(IReadOnlyList<FlatRelation> source, IReadOnlyDictionary<string, Table> tablesByKey)
        {
            return source
                .GroupBy(x => new { x.ObjectId, x.KeyName })
                .Select(group =>
                {
                    var first = group.First();
                    var primaryKey = TableKey(first.PrimarySchema, first.PrimaryTableName);
                    var foreignKey = TableKey(first.ForeignSchema, first.ForeignTableName);
                    tablesByKey.TryGetValue(primaryKey, out var primaryTable);
                    tablesByKey.TryGetValue(foreignKey, out var foreignTable);
                    var pairs = group.OrderBy(x => x.PrimaryColumnPath).ThenBy(x => x.ForeignColumnPath).Select((x, ordinal) => MapRelationColumnPair(x, ordinal + 1)).ToList();

                    return new ForeignKey
                    {
                        ObjectId = group.Key.ObjectId,
                        RelationId = first.ObjectId,
                        Name = first.KeyName,
                        RelationType = MapRelationType(first.KeyType),
                        KeyName = first.KeyName,
                        // ParentTable = PK side (e.g. Product), ReferencedTable = FK-holding side (e.g. ProductInventory).
                        // This naming is intentional in this codebase - opposite of the "Parent = FK holder" convention
                        // some other schema tools use. Keep consistent with RelationColumnPair below.
                        ParentTable = primaryTable,
                        ParentObjectId = primaryTable?.ObjectId,
                        ReferencedTable = foreignTable,
                        ReferencedObjectId = foreignTable?.ObjectId,
                        ColumnPairs = pairs,
                        IsDisabled = first.IsDisabled,
                        DeleteAction = first.DeleteAction,
                        UpdateAction = first.UpdateAction,
                        IsNullable = group.Any(x => x.IsNullable),
                        IsSelfReferencing = string.Equals(first.PrimarySchema, first.ForeignSchema, StringComparison.OrdinalIgnoreCase) && string.Equals(first.PrimaryTableName, first.ForeignTableName, StringComparison.OrdinalIgnoreCase),
                        IsComposite = first.IsComposite || group.Count() > 1
                    };
                })
                .ToList();
        }

        private static RelationColumnPair MapRelationColumnPair(FlatRelation source, int ordinal)
        {
            return new RelationColumnPair
            {
                RelationId = source.ObjectId,
                Ordinal = ordinal,
                // Parent = PK column (Product.ProductID)
                ParentColumnName = source.PrimaryColumnName,
                ParentTableName = source.PrimaryTableName,
                // Referenced = FK column (ProductInventory.ProductID)
                ReferencedColumnName = source.ForeignColumnName,
                ReferencedTableName = source.ForeignTableName
            };
        }

        private static RelationType MapRelationType(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? RelationType.ForeignKey : value.Trim().ToLowerInvariant() switch
            {
                "foreign key" => RelationType.ForeignKey,
                "foreignkey" => RelationType.ForeignKey,
                "reference" => RelationType.Reference,
                "dependency" => RelationType.Dependency,
                _ => RelationType.ForeignKey
            };
        }

        private static IEnumerable<string> GetRelationTableKeys(Schema.IRelation relation)
        {
            if (relation.ParentTable != null) yield return TableKey(relation.ParentTable.SchemaName, relation.ParentTable.Name);
            if (relation.ReferencedTable != null) yield return TableKey(relation.ReferencedTable.SchemaName, relation.ReferencedTable.Name);
        }

        #endregion

        #region The Rest

        #region Procedures

        internal static IEnumerable<Procedure> MapProcedure(IEnumerable<FlatStoredProcedure> sources)
        {
            return sources
                .GroupBy(p => p.ObjectId)
                .Select(group =>
                {
                    var header = group.First();

                    return new Procedure
                    {
                        ObjectId = header.ObjectId,
                        DatabaseName = header.DatabaseName,
                        SchemaName = header.SchemaName,
                        Name = header.DefinitionName, // Or header.DefinitionName depending on property name
                        FullName = header.FullName,
                        Identifier = header.Identifier,
                        Definition = header.Definition,

                        // Map parameters directly from the collapsed group
                        Parameters = group
                            .Where(p => !string.IsNullOrEmpty(p.Name)) // Guard against definitions with zero parameters
                            .OrderBy(p => p.OrdinalPosition)
                            .Select(p => new ProcedureParameter
                            {
                                Ordinal = p.OrdinalPosition,
                                Name = p.Name,
                                DataType = p.DataType,
                                ProviderDataType = p.ProviderDataType,
                                MaxLength = p.MaxLength,
                                Precision = p.Precision,
                                Scale = p.Scale,
                                IsOutput = p.IsOutput,
                                IsNullable = p.IsNullable,
                                //IsReadOnly = p.IsReadOnly,
                                DefaultValue = p.DefaultValue
                            })
                            .ToList()
                    };
                });
        }

        #endregion

        #region Functions

        internal static IEnumerable<Schema.Function> MapFunction(IEnumerable<FlatFunction> sources)
        {
            return sources
                .GroupBy(f => f.ObjectId)
                .Select(group =>
                {
                    var header = group.First();

                    return new Schema.Function
                    {
                        ObjectId = header.ObjectId,
                        DatabaseName = header.DatabaseName,
                        SchemaName = header.SchemaName,
                        Name = header.DefinitionName, // Or header.DefinitionName depending on property
                        FullName = header.FullName,
                        Definition = header.Definition,
                        ReturnType = header.ReturnType,
                        ProviderReturnType = header.ProviderReturnType,

                        // Map parameters directly from the collapsed group
                        Parameters = group
                            .Where(f => !string.IsNullOrEmpty(f.Name)) // Guard against functions with zero parameters
                            .OrderBy(f => f.OrdinalPosition)
                            .Select(f => new ProcedureParameter
                            {
                                Ordinal = f.OrdinalPosition,
                                Name = f.Name,
                                DataType = f.DataType,
                                ProviderDataType = f.ProviderDataType,
                                MaxLength = f.MaxLength,
                                Precision = f.Precision,
                                Scale = f.Scale,
                                IsOutput = f.IsOutput,
                                IsNullable = f.IsNullable,
                                DefaultValue = f.DefaultValue
                            })
                            .ToList()
                    };
                });
        }

        #endregion

        #region User Defined Types

        internal static IEnumerable<UserDefinedType> MapUserDefinedType(IEnumerable<FlatUserDefinedType> sources, IEnumerable<IColumn> columns)
        {
            return sources
                .GroupBy(udt => udt.ObjectId)
                .Select(group =>
                {
                    var header = group.First();
                    var typeKind = header.FunctionType switch
                    {
                        "ALIAS_UDT" => UserDefinedTypeKind.Alias,
                        "TABLE_TYPE" => UserDefinedTypeKind.Structured,
                        "CLR_UDT" => UserDefinedTypeKind.ClrType,
                        _ => UserDefinedTypeKind.Unknown
                    };
                    return new UserDefinedType
                    {
                        ObjectId = header.ObjectId,
                        DatabaseName = header.DatabaseName,
                        SchemaName = header.SchemaName,
                        Name = header.DefinitionName, // Or header.DefinitionName depending on property name
                        FullName = header.FullName,
                        TypeKind = typeKind,
                        BaseType = header.DataType,
                        ProviderBaseType = header.ProviderDataType,
                        MaxLength = header.MaxLength,
                        Precision = header.Precision,
                        Scale = header.Scale,
                        CreateDate = header.CreateDate,
                        ModifyDate = header.ModifyDate,
                        IsSystemObject = header.IsSystemObject,
                        Definition = header.Definition,
                        DefinitionHash = header.DefinitionHash,
                        SchemaId = header.SchemaId,
                        IsNullable = header.IsNullable,
                        Identifier = header.Identifier,
                        IsDeterministic = header.IsDeterministic,
                        IsReplicated = header.IsReplicated,
                        WithCheckOption = header.WithCheckOption,

                        // Map columns directly from the collapsed group (for Table Types)
                        Columns = group
                            .Where(u => !string.IsNullOrEmpty(u.Name)) // Guard for scalar UDTs that have no table columns
                            .OrderBy(u => u.OrdinalPosition)
                            .Select(u => new UserDefinedTypeColumn
                            {
                                OrdinalPosition = u.OrdinalPosition,
                                Name = u.Name,
                                DataType = u.DataType,
                                ProviderDataType = u.ProviderDataType,
                                MaximumLength = u.MaxLength,
                                Precision = u.Precision,
                                Scale = u.Scale,
                                IsNullable = u.IsNullable,
                                DefaultValue = u.DefaultValue
                            })
                            .Cast<IUserDefinedTypeColumn>()
                            .ToList()
                    };
                });
        }

        #endregion

        #region Views

        internal static IEnumerable<Schema.View> MapView(IEnumerable<FlatView> sources, IReadOnlyDictionary<int, IReadOnlyList<Schema.IColumn>> columnsByObjectId)
        {
            return sources
                .GroupBy(v => v.ObjectId)
                .Select(group =>
                {
                    var definition = group.First();
                    columnsByObjectId.TryGetValue(definition.ObjectId, out var viewColumns);

                    return new Schema.View
                    {
                        ObjectId = definition.ObjectId,
                        DatabaseName = definition.DatabaseName,
                        SchemaName = definition.SchemaName,
                        Name = definition.DefinitionName,
                        FullName = $"{definition.SchemaName}.{definition.Name}",
                        Definition = definition.Definition,

                        // Project from canonical columns instead of instantiating new duplicates
                        Columns = viewColumns ?? Array.Empty<Schema.IColumn>()
                    };
                });
        }

        #endregion

        #endregion

        #region Principals & Permissions

        internal static IReadOnlyList<IDatabasePrincipal> HydratePrincipals(
                    IEnumerable<FlatDatabasePrincipal> principalSources,
                    IEnumerable<FlatDatabasePermission> permissionSources,
                    IEnumerable<FlatRoleMembership> roleMembershipSources)
        {
            var permissions = permissionSources?.ToList() ?? [];
            var memberships = roleMembershipSources?.ToList() ?? [];

            // Group permissions by grantee principal ID
            var permissionsByPrincipal = permissions
                .GroupBy(p => p.GranteePrincipalId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(p => (IPrincipalPermission)new PrincipalPermission
                    {
                        PermissionName = p.PermissionName,
                        StateDescription = p.StateDesc,
                        ClassDescription = p.ClassDesc,
                        SecurableSchemaName = p.SecurableSchemaName,
                        SecurableName = p.SecurableName
                    }).ToList() as IReadOnlyList<IPrincipalPermission>
                );

            // 2. First Pass: Instantiate all base principal objects
            var principalList = new List<IDatabasePrincipal>();

            foreach (var group in principalSources.GroupBy(p => p.PrincipalId))
            {
                var header = group.First();
                var assignedPerms = permissionsByPrincipal.TryGetValue(header.PrincipalId, out var perms)
                    ? perms
                    : Array.Empty<IPrincipalPermission>();

                IDatabasePrincipal principalModel = header.PrincipalType switch
                {
                    "R" => new DatabaseRole
                    {
                        PrincipalId = header.PrincipalId,
                        Name = header.Name,
                        TypeDescription = header.TypeDescription,
                        IsFixedRole = header.IsFixedRole,
                        Permissions = assignedPerms
                    },
                    "A" => new ApplicationRole
                    {
                        PrincipalId = header.PrincipalId,
                        Name = header.Name,
                        TypeDescription = header.TypeDescription,
                        Permissions = assignedPerms
                    },
                    _ => new DatabaseUser
                    {
                        PrincipalId = header.PrincipalId,
                        Name = header.Name,
                        TypeDescription = header.TypeDescription,
                        DefaultSchemaName = header.DefaultSchemaName,
                        Permissions = assignedPerms
                    }
                };

                principalList.Add(principalModel);
            }

            // 3. Second Pass: Wire up Role Memberships using actual object references
            var membersByRole = memberships
                .GroupBy(rm => rm.RolePrincipalId)
                .ToDictionary(g => g.Key, g => g.Select(rm => rm.MemberPrincipalId).ToHashSet());

            foreach (var principal in principalList)
            {
                if (principal is DatabaseRole role && membersByRole.TryGetValue(role.PrincipalId, out var memberIds))
                {
                    role.Members = principalList
                        .Where(p => memberIds.Contains(p.PrincipalId))
                        .ToList();
                }
            }

            return principalList;
        }

        #endregion

        #region Helpers

        private static string TableKey(string? schema, string? table)
        {
            return $"{schema ?? string.Empty}.{table ?? string.Empty}";
        }

        #endregion
    }
}