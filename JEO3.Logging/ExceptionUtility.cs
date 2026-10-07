using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace JEO3.Logging
{
    public static class ExceptionUtility
    {
        #region Properties
        private static int _isRegistered = 0;
        private static bool _logFirstChance = true;
        private static bool _logUnhandled = true;
        private static string _connectionString = string.Empty;
        private static readonly ConcurrentDictionary<MethodBase, CachedMethodMetadata> _methodCache = new();
        #endregion

        #region Exposed
        public static void Register(string connectionString, bool logFirstChance = true, bool logUnhandled = true)
        {
            try
            {
                // Thread-safe lock-free registration check
                if (Interlocked.CompareExchange(ref _isRegistered, 1, 0) == 1) return;

                _connectionString = connectionString;
                _logFirstChance = logFirstChance;
                _logUnhandled = logUnhandled;

                if (_logFirstChance)
                {
                    AppDomain.CurrentDomain.FirstChanceException += new EventHandler<System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs>((o, e) =>
                    {
                        LogException(e.Exception);
                    });
                }

                if (_logUnhandled)
                {
                    AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((o, e) =>
                    {
                        if (e != null && e.ExceptionObject != null && typeof(Exception).IsAssignableFrom(e.ExceptionObject.GetType()))
                        {
                            LogException((Exception)e.ExceptionObject);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(Constants.GenericExceptionPrefix + ex);
            }
        }
        public static void LogException(Exception ex)
        {
            try
            {
                if (ex == null) { return; }

                var codeException = LoadException(ex);
                if (codeException != null)
                {
                    Trace.WriteLine($"Exception: {codeException.ClassFullName} - {codeException.Method} - {codeException.AttemptedMethod}: {codeException.Message}");

                    CodeException.Insert(codeException, _connectionString);
                }
            }
            catch (Exception exMisc)
            {
                Trace.WriteLine(Constants.GenericExceptionPrefix + exMisc);
            }
        }
        public static async Task LogExceptionAsync(Exception ex)
        {
            try
            {
                if (ex == null) { return; }

                var codeException = LoadException(ex);
                if (codeException != null)
                {
                    // Revisit Later - When Free Time To Implement

                    Trace.WriteLine($"Exception: {codeException.ClassFullName} - {codeException.Method} - {codeException.AttemptedMethod}: {codeException.Message}");

                    await CodeException.InsertAsync(codeException, _connectionString);
                }
            }
            catch (Exception exMisc)
            {
                Trace.WriteLine(Constants.GenericExceptionPrefix + exMisc);
            }
        }

        #endregion

        #region Load
        private static CodeException LoadException(Exception ex)
        {
            // Validation
            if (ex.Message != null)
            {
                string exMessage = ex.Message.ToLower();
                if (Constants.ExceptionIgnoreMessageFragments.Any(v => exMessage.Contains(v))) { return null; }
            }

            if (ex.TargetSite?.DeclaringType?.Namespace == "System.Runtime.ExceptionServices" ||
                ex.TargetSite?.DeclaringType?.Namespace == "System.Runtime.CompilerServices")
            {
                return null;
            }

            CodeException codeException = new CodeException(ex)
            {
                Exception = ex,
                Type = ex.GetType().Name,
                TimeStamp = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("America/New_York")),
                Message = ex.Message,
                StackTrace = (ex.StackTrace != null) ? ex.StackTrace : string.Empty,
                InnerException = ex.InnerException?.ToString() ?? string.Empty,
                UserName = System.Security.Principal.WindowsIdentity.GetCurrent()?.Name ?? Environment.UserName
            };

            // Loop Stack Trace Methods
            StackTrace stack = new StackTrace(ex, 0, true);
            for (int index = 0; index < stack.FrameCount; index++)
            {
                // Load Reflection Information
                StackFrame stackFrame = (stack != null && index >= 0) ? stack.GetFrame(index) : null;
                MethodBase methodBase = (stackFrame != null) ? stackFrame.GetMethod() : null;
                if (methodBase == null || methodBase.Name == nameof(LogException) || methodBase.Name == nameof(LoadException)) continue;

                // Fetch from cache
                var cachedData = _methodCache.GetOrAdd(methodBase, BuildMethodMetadata);

                // Apply the cached strings
                if (cachedData != null)
                {
                    codeException.ClassFullName = cachedData.ClassFullName;
                    codeException.Namespace = cachedData.Namespace;
                    codeException.AssemblyVersion = cachedData.AssemblyVersion;
                    codeException.AssemblyFilePath = cachedData.AssemblyFilePath;
                }

                // Load Method Properties (Untouched!)
                LoadMethodProperties(codeException, ex, stackFrame);
            }

            return codeException;
        }
        private static void LoadMethodProperties(CodeException codeException, Exception ex, StackFrame stackFrame)
        {
            if (ex == null || ex.TargetSite == null) { return; }

            if (typeof(MethodInfo).IsAssignableFrom(ex.TargetSite.GetType()))
            {
                // Reflection Variables
                MethodBase methodBase = stackFrame.GetMethod();
                MethodBody methodBody = methodBase.GetMethodBody();
                MethodInfo methodInfo = (methodBase != null && typeof(MethodInfo).IsAssignableFrom(methodBase.GetType()) == true) ? (MethodInfo)methodBase : null;
                MethodInfo targetSiteInfo = (MethodInfo)ex.TargetSite;

                // Attempted Method
                if (string.IsNullOrEmpty(codeException.AttemptedMethod))
                {
                    codeException.AttemptedMethod = (targetSiteInfo != null && targetSiteInfo.ReturnParameter != null) ? targetSiteInfo.Name : string.Empty;
                }

                codeException.AttemptedMethodReturnType = (targetSiteInfo != null && targetSiteInfo.ReturnType != null) ? targetSiteInfo.ReturnType.FullName : string.Empty;

                // Get Line Number
                int intLineNumber = GetExceptionLineNumber(codeException, ex);
                intLineNumber = (intLineNumber == 0) ? stackFrame.GetFileLineNumber() : intLineNumber;
                codeException.LineNumber = (codeException.LineNumber == "0") ? stackFrame.GetFileLineNumber().ToString() : intLineNumber.ToString();
                codeException.Class = methodBase.ReflectedType.FullName.Split('.').Last();
                codeException.Method = stackFrame.GetMethod().Name;
                codeException.ClassFullName = methodBase.ReflectedType.FullName;
                codeException.AttemptedMethodSignature = (targetSiteInfo != null) ? "(" + string.Join(", ", targetSiteInfo.GetParameters().Select(v => v.ParameterType.Name + " " + v.Name)) + ")" : string.Empty;
            }
        }
        private static int GetExceptionLineNumber(CodeException codeException, Exception ex)
        {
            if (ex != null && ex.StackTrace != null)
            {
                int lineNumber = 0;
                string[] values = ex.StackTrace.Split(':');

                // if (strValues.Length > 0 && strValues[strValues.Length - 1].Contains("line") == true)
                if (values.Length > 0)
                {
                    var matches = values.Where(match => match.Contains("line")).ToList();
                    if (matches.Count > 0)
                    {
                        var lastMatch = matches.Last();
                        string strValue = matches.Last().Replace("line ", string.Empty); // strValues[strValues.Length - 1].Replace("line ", "");
                        lineNumber = (strValue.Contains(" ") == true && int.TryParse(strValue.Substring(0, strValue.IndexOf(" ")).Trim(), out lineNumber) == true) ? int.Parse(strValue.Substring(0, strValue.IndexOf(" ")).Trim()) : 0;
                        return lineNumber;
                    }
                }

                return lineNumber;
            }
            else
            {
                return -1;
            }
        }
        private static CachedMethodMetadata BuildMethodMetadata(MethodBase methodBase)
        {
            var meta = new CachedMethodMetadata();
            MethodInfo methodInfo = (methodBase != null && typeof(MethodInfo).IsAssignableFrom(methodBase.GetType())) ? (MethodInfo)methodBase : null;

            if (methodInfo != null && methodInfo.ReflectedType != null)
            {
                var aqn = methodInfo.ReflectedType.AssemblyQualifiedName;
                meta.ClassFullName = methodInfo.ReflectedType.FullName;
                meta.Namespace = methodInfo.ReflectedType.Namespace;
                meta.AssemblyVersion = aqn.Replace(aqn.Substring(0, aqn.IndexOf("=") + 1), string.Empty);
                meta.AssemblyVersion = (meta.AssemblyVersion.Contains(",") == true) ? meta.AssemblyVersion.Substring(0, meta.AssemblyVersion.IndexOf(",")) : string.Empty;

                Assembly assembly = Assembly.GetExecutingAssembly();
                if (assembly != null && assembly.CodeBase != string.Empty)
                {
                    meta.AssemblyFilePath = "Assembly File Path: " + assembly.CodeBase + "\n";
                }
            }

            return meta;
        }
        #endregion
    }
}