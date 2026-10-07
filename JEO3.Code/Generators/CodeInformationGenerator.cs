using System.Text;
using JEO3.IO.Directory;
using JEO3.IO.File;


namespace JEO3.Code
{
    public readonly record struct CodeTreeMetrics(int FileCount, int TotalLines, long TotalCharacters);
    public static class CodeInformationGenerator
    {
        #region Generate
        public static string GenerateCSharpFileTree(string path, string pattern = "*.cs", bool recurse = true, bool ignoreForDev = true, string basePathRemove = "")
        {
            var dir = new DirectoryObject(path);
            if (dir.Exists == false) { return string.Empty; }

            var files = dir.GetFiles(pattern, recurse);
            if (files == null) { return string.Empty; }

            var fs = ignoreForDev == false ? files.ToList() : files.Where(v => v.Extension.Contains("g.") == false && v.FilePath.Contains(@"\bin") == false && v.FilePath.Contains(@"\obj") == false).ToList();

            var sb = new StringBuilder();
            for (int i = 0; i < fs.Count; i++)
            {
                if (i > 0) sb.Append("\r\n");
                sb.Append($"// Path: {(basePathRemove.Length > 0 ? fs[i].FilePath.Replace(basePathRemove, "") : fs[i].FilePath)} - Lines: {fs[i].Content?.Lines?.Count ?? 0} - Len: {fs[i].Content?.Value?.Length ?? 0}");
            }
            return sb.ToString();
        }
        public static string GenerateCSharpFileTree(string path, string[] patterns, bool recurse = true, bool ignoreForDev = true, string basePathRemove = "")
        {
            var dir = new DirectoryObject(path);
            if (dir.Exists == false) { return string.Empty; }

            var files = patterns.SelectMany(v => dir.GetFiles(v, recurse)).ToList();
            if (files == null) { return string.Empty; }

            files = ignoreForDev == false ? files.ToList() : files.Where(v => v.Extension.Contains("g.") == false && v.FilePath.Contains(@"\bin") == false && v.FilePath.Contains(@"\obj") == false).ToList();

            var sb = new StringBuilder();
            for (int i = 0; i < files.Count; i++)
            {
                if (i > 0) sb.Append("\r\n");
                sb.Append($"// Path: {files[i].FilePath.Replace(basePathRemove, "")} - Lines: {files[i].Content?.Lines?.Count ?? 0} - Len: {files[i].Content?.Value?.Length ?? 0}");
            }
            return sb.ToString();
        }
        public static string GenerateCSharpFileTree(List<FileObject> files, bool ignoreForDev = true)
        {
            if (files == null) { return string.Empty; }

            var fs = ignoreForDev == false ? files.ToList() : files.Where(v => v.Extension.Contains("g.") == false && v.FilePath.Contains(@"\bin") == false && v.FilePath.Contains(@"\obj") == false).ToList();

            var sb = new StringBuilder();
            for (int i = 0; i < fs.Count; i++)
            {
                if (i > 0) sb.Append("\r\n");
                sb.Append($"// Path: {fs[i].FilePath} - Lines: {fs[i].Content?.Lines?.Count ?? 0} - Len: {fs[i].Content?.Value?.Length ?? 0}");
            }
            return sb.ToString();
        }
        public static string GenerateFileContentWithAnnotation(string path, string[] patterns, bool recurse = true, bool ignoreForDev = true, string basePathRemove = "")
        {
            var dir = new DirectoryObject(path);
            var files = patterns.SelectMany(v => dir.GetFiles(v, recurse)).ToList();
            files = ignoreForDev == false ? files.ToList() : files.Where(v => v.Extension.Contains("g.") == false && v.FilePath.Contains(@"\bin") == false && v.FilePath.Contains(@"\obj") == false).ToList();

            return string.Join("\r\n\r\n",
                files.Select(v => "// ".PadRight(5, '*') + (basePathRemove.Length > 0 ? v.FilePath.Replace(basePathRemove, "") : v.FilePath)
                    + "\r\n" + string.Join("\r\n", v.Content.Lines.Where(v => v.TrimStart().StartsWith("using ") == false))));
        }

        public static CodeTreeMetrics GetTreeMetrics(string path, string pattern = "*.cs", bool recurse = true)
        {
            var dir = new DirectoryObject(path);
            if (dir.Exists == false) { return new CodeTreeMetrics(0, 0, 0); }

            var files = dir.GetFiles(pattern, recurse)?.ToList();
            if (files == null || !files.Any()) { return new CodeTreeMetrics(0, 0, 0); }

            int totalFiles = files.Count;
            int totalLines = files.Sum(v => v.Content?.Lines?.Count ?? 0);
            long totalChars = files.Sum(v => (long)(v.Content?.Value?.Length ?? 0));

            return new CodeTreeMetrics(totalFiles, totalLines, totalChars);
        }
        #endregion

        #region Helpers
        public static Dictionary<string, int> ScanKeywordOccurrences(string path, string keyword, string pattern = "*.cs", bool recurse = true)
        {
            var dir = new DirectoryObject(path);
            var results = new Dictionary<string, int>();

            if (dir.Exists == false) { return results; }

            var files = dir.GetFiles(pattern, recurse);
            if (files == null) { return results; }

            foreach (var file in files)
            {
                var content = file.Content?.Value;
                if (string.IsNullOrEmpty(content)) continue;

                // Simple fast ordinal match counting
                int count = 0;
                int index = 0;
                while ((index = content.IndexOf(keyword, index, System.StringComparison.OrdinalIgnoreCase)) != -1)
                {
                    count++;
                    index += keyword.Length;
                }

                if (count > 0)
                {
                    results[file.Name] = count;
                }
            }

            return results;
        }
        #endregion
    }

}