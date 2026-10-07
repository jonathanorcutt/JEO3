namespace JEO3.Extensions.Generic
{
    public static class EnumExtensions
    {
        public static string GetEnumDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            return field?.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false)
                is System.ComponentModel.DescriptionAttribute[] { Length: > 0 } attrs
                ? attrs[0].Description
                : value.ToString();
        }
    }
}
