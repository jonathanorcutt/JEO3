using System.Diagnostics;
using JEO3.Engine.Entities;
using JEO3.Engine.Entities.JEO3.Engine.Models;
using JEO3.Engine.Models;
using JEO3.Providers;
using JEO3.Providers.Catalogs;
using JEO3.Schema;

namespace JEO3.Engine
{
    /// <summary>
    /// Staging Context Retrieval - Only Exposed Because FlatDatabaseContext Can Be Serialized! (e.g. Caching)
    /// </summary>
    public static class StagingContextFactory
    {
        public static async Task<FlatDatabaseContext> GetFlatContext(IDatabaseProvider prov, bool runInParallel = true)
        {
            DateTime now = DateTime.Now;
            if (prov == null)
            {
                throw new InvalidOperationException($"Could not resolve a database provider for engine type: {prov.Connection.ProviderName}");
            }

            // Switch For Debugging
            if (runInParallel == false)
            {
                var flatDatabaseTask = await prov.GetInstances<Database>(SchemaQueryType.DatabaseQuery);
                var flatSchemasTask = await prov.GetInstances<JEO3.Schema.Schema>(SchemaQueryType.SchemaQuery);
                var flatTablesTask = await prov.GetInstances<Table>(SchemaQueryType.TableQuery);
                var flatColumnsTask = await prov.GetInstances<Column>(SchemaQueryType.ColumnQuery);
                var flatRelationsTask = await prov.GetInstances<FlatRelation>(SchemaQueryType.RelationQuery);
                var flatIndexesTask = await prov.GetInstances<FlatIndex>(SchemaQueryType.IndexQuery);

                var flatProcedureDefinitions = await prov.GetInstances<FlatStoredProcedureDefinition>(SchemaQueryType.StoredProcedureDefinitionQuery);
                var flatViewDefinitions = await prov.GetInstances<FlatViewDefinition>(SchemaQueryType.ViewDefinitionQuery);
                var flatFunctionDefinitions = await prov.GetInstances<FlatFunctionDefinition>(SchemaQueryType.FunctionDefinitionQuery);
                var flatUdtDefinitions = await prov.GetInstances<FlatUserDefinedTypeDefinition>(SchemaQueryType.UserDefinedTypeDefinitionQuery);

                var flatProcedureParameters = await prov.GetInstances<FlatStoredProcedureParameter>(SchemaQueryType.StoredProcedureParameterQuery);
                var flatFunctionParameters = await prov.GetInstances<FlatFunctionParameter>(SchemaQueryType.FunctionParameterQuery);
                var flatViewColumns = await prov.GetInstances<FlatViewColumn>(SchemaQueryType.ViewColumnQuery);
                var flatUdColumns = await prov.GetInstances<FlatUserDefinedTypeColumn>(SchemaQueryType.UserDefinedTypeColumnQuery);

                var flatCheckConstraints = await prov.GetInstances<CheckConstraint>(SchemaQueryType.CheckConstraintQuery);
                var flatUExtendedProperties = await prov.GetInstances<ExtendedProperty>(SchemaQueryType.ExtendedPropertyQuery);
                var flatMissingIndexes = await prov.GetInstances<MissingIndex>(SchemaQueryType.MissingIndexQuery);
                var flatStatistics = await prov.GetInstances<Statistic>(SchemaQueryType.StatisticQuery);
                var flatTriggers = await prov.GetInstances<Trigger>(SchemaQueryType.TriggerQuery);
                //var flatSynonyms = await prov.GetInstances<Synonym>(SchemaQueryType.SynonymQuery);

                //var flatPrincipals = await prov.GetInstances<FlatDatabasePrincipal>(SchemaQueryType.DatabasePrincipalQuery);
                //var flatPermissions = await prov.GetInstances<FlatDatabasePermission>(SchemaQueryType.DatabasePermissionQuery);
                //var flatRoleMemberships = await prov.GetInstances<FlatRoleMembership>(SchemaQueryType.DatabaseRoleMembershipQuery);


                return new FlatDatabaseContext()
                {
                    ProviderName = prov.Connection.ProviderName,
                    DatabaseName = prov.Connection.Database,
                    ConnectionString = prov.Connection.ConnectionString,
                    Databases = flatDatabaseTask.ToList() ?? [],
                    Schemas = flatSchemasTask.ToList() ?? [],
                    Tables = flatTablesTask.ToList() ?? [],
                    Columns = flatColumnsTask.ToList() ?? [],
                    Relations = flatRelationsTask.ToList() ?? [],
                    Indexes = flatIndexesTask.ToList() ?? [],
                    Procedures = FlatStoredProcedure.Create(flatProcedureDefinitions.ToList() ?? [], flatProcedureParameters ?? []),
                    Views = FlatView.Create(flatViewDefinitions.ToList() ?? [], flatViewColumns ?? []),
                    Functions = FlatFunction.Create(flatFunctionDefinitions.ToList() ?? [], flatFunctionParameters ?? []),
                    UserDefinedTypes = FlatUserDefinedType.Create(flatUdtDefinitions.ToList() ?? [], flatUdColumns ?? []),
                    CheckConstraints = flatCheckConstraints.ToList() ?? [],
                    Statistics = flatStatistics.ToList() ?? [],
                    MissingIndexes = flatMissingIndexes.ToList() ?? [],
                    Triggers = flatTriggers.ToList() ?? [],
                    ExtendedProperties = flatUExtendedProperties.ToList() ?? [],
                    //Principals = flatPrincipals,
                    //Permissions = flatPermissions,
                    //Synonyms = flatSynonyms
                };
            }
            else
            {
                // 1. Kick off all tasks simultaneously
                var flatDatabaseTask = prov.GetInstances<Database>(SchemaQueryType.DatabaseQuery);
                var flatSchemasTask = prov.GetInstances<JEO3.Schema.Schema>(SchemaQueryType.SchemaQuery);
                var flatTablesTask = prov.GetInstances<Table>(SchemaQueryType.TableQuery);
                var flatColumnsTask = prov.GetInstances<Column>(SchemaQueryType.ColumnQuery);
                var flatRelationsTask = prov.GetInstances<FlatRelation>(SchemaQueryType.RelationQuery);
                var flatIndexesTask = prov.GetInstances<FlatIndex>(SchemaQueryType.IndexQuery);

                var flatProcedureDefinitionsTask = prov.GetInstances<FlatStoredProcedureDefinition>(SchemaQueryType.StoredProcedureDefinitionQuery);
                var flatViewDefinitionsTask = prov.GetInstances<FlatViewDefinition>(SchemaQueryType.ViewDefinitionQuery);
                var flatFunctionDefinitionsTask = prov.GetInstances<FlatFunctionDefinition>(SchemaQueryType.FunctionDefinitionQuery);
                var flatUdtDefinitionsTask = prov.GetInstances<FlatUserDefinedTypeDefinition>(SchemaQueryType.UserDefinedTypeDefinitionQuery);

                var flatProcedureParametersTask = prov.GetInstances<FlatStoredProcedureParameter>(SchemaQueryType.StoredProcedureParameterQuery);
                var flatFunctionParametersTask = prov.GetInstances<FlatFunctionParameter>(SchemaQueryType.FunctionParameterQuery);
                var flatViewColumnsTask = prov.GetInstances<FlatViewColumn>(SchemaQueryType.ViewColumnQuery);
                var flatUdColumnsTask = prov.GetInstances<FlatUserDefinedTypeColumn>(SchemaQueryType.UserDefinedTypeColumnQuery);

                var flatCheckConstraintsTask = prov.GetInstances<CheckConstraint>(SchemaQueryType.CheckConstraintQuery);
                var flatUExtendedPropertiesTask = prov.GetInstances<ExtendedProperty>(SchemaQueryType.ExtendedPropertyQuery);
                var flatMissingIndexesTask = prov.GetInstances<MissingIndex>(SchemaQueryType.MissingIndexQuery);
                var flatStatisticsTask = prov.GetInstances<Statistic>(SchemaQueryType.StatisticQuery);
                var flatTriggersTask = prov.GetInstances<Trigger>(SchemaQueryType.TriggerQuery);
                var flatSynonymsTask = prov.GetInstances<Synonym>(SchemaQueryType.SynonymQuery); // Renamed to denote it's a Task

                var flatPrincipalsTask = prov.GetInstances<FlatDatabasePrincipal>(SchemaQueryType.DatabasePrincipalQuery);
                var flatPermissionsTask = prov.GetInstances<FlatDatabasePermission>(SchemaQueryType.DatabasePermissionQuery);
                var flatRoleMembershipsTask = prov.GetInstances<FlatRoleMembership>(SchemaQueryType.DatabaseRoleMembershipQuery);

                List<Task> tasksToExecute = new List<Task>()
                {
                     flatDatabaseTask, flatSchemasTask, flatTablesTask, flatColumnsTask,
                    flatRelationsTask, flatIndexesTask, flatProcedureDefinitionsTask, flatViewDefinitionsTask,
                    flatFunctionDefinitionsTask, flatUdtDefinitionsTask, flatProcedureParametersTask, flatFunctionParametersTask,
                    flatViewColumnsTask, flatUdColumnsTask, flatCheckConstraintsTask, flatUExtendedPropertiesTask,
                    flatMissingIndexesTask, flatStatisticsTask, flatTriggersTask, 
                    // flatSynonymsTask,
                    // flatPrincipalsTask, flatPermissionsTask, flatRoleMembershipsTask
                };

                // 2. Await ALL tasks (added flatSynonymsTask here)
                await Task.WhenAll(tasksToExecute);

                // 3. Construct Context safely with all mapping fields fulfilled
                var newCtx = new FlatDatabaseContext()
                {
                    ProviderName = prov.Connection.ProviderName,
                    DatabaseName = prov.Connection.Database,
                    ConnectionString = prov.Connection.ConnectionString,
                    Databases = flatDatabaseTask.Result?.ToList() ?? [], // Added missing mapping
                    Schemas = flatSchemasTask.Result?.ToList() ?? [],     // Added missing mapping
                    Tables = flatTablesTask.Result?.ToList() ?? [],
                    Columns = flatColumnsTask.Result?.ToList() ?? [],
                    Relations = flatRelationsTask.Result?.ToList() ?? [],
                    Indexes = flatIndexesTask.Result?.ToList() ?? [],
                    Procedures = FlatStoredProcedure.Create(flatProcedureDefinitionsTask.Result?.ToList() ?? [], flatProcedureParametersTask.Result?.ToList() ?? []),
                    Views = FlatView.Create(flatViewDefinitionsTask.Result?.ToList() ?? [], flatViewColumnsTask.Result?.ToList() ?? []),
                    Functions = FlatFunction.Create(flatFunctionDefinitionsTask.Result?.ToList() ?? [], flatFunctionParametersTask.Result?.ToList() ?? []),
                    UserDefinedTypes = FlatUserDefinedType.Create(flatUdtDefinitionsTask.Result?.ToList() ?? [], flatUdColumnsTask.Result?.ToList() ?? []),
                    CheckConstraints = flatCheckConstraintsTask.Result?.ToList() ?? [],
                    Statistics = flatStatisticsTask.Result?.ToList() ?? [],
                    MissingIndexes = flatMissingIndexesTask.Result?.ToList() ?? [],
                    Triggers = flatTriggersTask.Result?.ToList() ?? [],
                    ExtendedProperties = flatUExtendedPropertiesTask.Result?.ToList() ?? [],
                    //Principals = flatPrincipalsTask.Result?.ToList() ?? [],
                    //Permissions = flatPermissionsTask.Result?.ToList() ?? [],
                    //Synonyms = flatSynonymsTask.Result?.ToList() ?? [] // Safe to call .Result now
                };
                var log = Math.Round((DateTime.Now - now).TotalSeconds, 2);
                Trace.WriteLine($"JEO3.Site - GetFlatContext - Tables: {(newCtx?.Tables?.Count ?? 0)}. Elapsed {log}sec");
                
                return newCtx;
            }
        }
    }
}
