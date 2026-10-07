namespace JEO3.Schema
{
    public sealed class DescribeFirstResultSet
    {
        public bool is_hidden { get; }
        public int? column_ordinal { get; init; }
        public string? name { get; init; }
        public bool? is_nullable { get; init; }
        public int? system_type_id { get; internal init; }
        public string? system_type_name { get; init; }
        public short? max_length { get; internal init; }
        public byte? precision { get; internal init; }
        public byte? scale { get; internal init; }
        public string? collation_name { get; internal init; }
        public int? user_type_id { get; internal init; }
        public string? user_type_database { get; internal init; }
        public string? user_type_schema { get; internal init; }
        public string? user_type_name { get; internal init; }
        public string? assembly_qualified_type_name { get; internal init; }
        public int? xml_collection_id { get; internal init; }
        public string? xml_collection_database { get; internal init; }
        public string? xml_collection_schema { get; internal init; }
        public string? xml_collection_name { get; internal init; }
        public bool? is_xml_document { get; internal init; }
        public bool? is_case_sensitive { get; internal init; }
        public bool? is_fixed_length_clr_type { get; internal init; }
        public string? source_server { get; internal init; }
        public string? source_database { get; internal init; }
        public string? source_schema { get; internal init; }
        public string? source_table { get; internal init; }
        public string? source_column { get; internal init; }
        public bool? is_identity_column { get; internal init; }
        public bool? is_part_of_unique_key { get; internal init; }
        public bool? is_updateable { get; internal init; }
        public bool? is_computed_column { get; internal init; }
        public bool? is_sparse_column_set { get; internal init; }
        public short? ordinal_in_order_by_list { get; internal init; }
        public bool? order_by_is_descending { get; internal init; }
        public short? order_by_list_length { get; internal init; }
        public int? tds_type_id { get; internal init; }
        public int? tds_length { get; internal init; }
        public int? tds_collation_id { get; internal init; }
        public byte? tds_collation_sort_id { get; internal init; }
    }
}
