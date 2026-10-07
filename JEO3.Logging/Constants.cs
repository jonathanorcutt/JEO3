namespace JEO3.Logging
{
    internal class Constants
    {
        internal static string GenericExceptionPrefix = "Exception encountered. Detail: ";

        internal static string[] ExceptionIgnoreMessageFragments = new string[]
        {
            "A task was canceled.",
            "The operation was canceled.",
            "JavaScript interop calls cannot be issued at this time"
        }.Select(v => v.ToLower()).ToArray();

    }
}
