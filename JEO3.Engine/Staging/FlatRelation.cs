using System.Text.Json.Serialization;

namespace JEO3.Engine.Models
{
    public sealed class FlatRelation
    {
        #region Properties

        [JsonInclude]
        public int ObjectId { get; private set; }

        [JsonInclude]
        public string KeyName { get; private set; } = string.Empty;

        [JsonInclude]
        public string KeyType { get; private set; } = string.Empty;

        [JsonInclude]
        public DateTime CreatedDate { get; private set; }
        [JsonInclude]
        public DateTime ModifiedDate { get; private set; }

        [JsonInclude]
        public string DeleteAction { get; private set; } = string.Empty;
        [JsonInclude]
        public string UpdateAction { get; private set; } = string.Empty;

        // Table Keys
        [JsonInclude]
        public string PrimaryTablePath { get; private set; } = string.Empty; // CONCAT(SP.name,'.',TP.name,'.',CP.name) AS PrimaryColumnPath,
        [JsonInclude]
        public string ForeignTablePath { get; private set; } = string.Empty; //  (CASE WHEN SF.name = 'dbo' THEN TF.name ELSE SF.name + '.' + TF.name END) AS ForeignTablePathDisplay,

        // Column Keys
        [JsonInclude]
        public string PrimaryColumnPath { get; private set; } = string.Empty; // CONCAT(SP.name,'.',TP.name,'.',CP.name) AS PrimaryColumnPath,
        [JsonInclude]
        public string ForeignColumnPath { get; private set; } = string.Empty; // CONCAT(SF.name,'.',TF.name,'.',CF.name) AS ForeignColumnPath,

        // Schema Name
        [JsonInclude]
        public string PrimarySchema { get; private set; } = string.Empty;
        [JsonInclude]
        public string ForeignSchema { get; private set; } = string.Empty;

        // Table Name
        [JsonInclude]
        public string PrimaryTableName { get; private set; } = string.Empty;
        [JsonInclude]
        public string ForeignTableName { get; private set; } = string.Empty;

        // Column Name
        [JsonInclude]
        public string PrimaryColumnName { get; private set; } = string.Empty;
        [JsonInclude]
        public string ForeignColumnName { get; private set; } = string.Empty;

        // DataType -> Delete Later
        [JsonInclude]
        public string PrimaryDataType { get; private set; } = string.Empty;
        [JsonInclude]
        public string ForeignDataType { get; private set; } = string.Empty;

        // Aux Data
        [JsonInclude]
        public bool IsComposite { get; private set; }
        [JsonInclude]
        public bool IsNullable { get; private set; }
        [JsonInclude]
        public bool IsDisabled { get; private set; }
        [JsonInclude]
        public bool IsComputed { get; private set; }

        public string Key => $"{PrimarySchema}.{PrimaryTableName}.{PrimaryColumnName}._{ForeignSchema}.{ForeignTableName}.{ForeignColumnName}";

        #endregion
    }
}
