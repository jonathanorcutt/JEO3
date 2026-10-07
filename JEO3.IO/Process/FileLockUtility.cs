using System.Diagnostics;
using System.Runtime.InteropServices;

namespace JEO3.IO.Processes
{
    public static class FileLockUtility
    {
        // --- Windows Restart Manager API P/Invoke Declarations ---

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct RM_UNIQUE_PROCESS
        {
            public int dwProcessId;
            public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
        }

        private enum RM_APP_TYPE
        {
            RmUnknownApp = 0,
            RmMainWindow = 1,
            RmOtherWindow = 2,
            RmService = 3,
            RmExplorer = 4,
            RmConsole = 5,
            RmCritical = 1000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct RM_PROCESS_INFO
        {
            public RM_UNIQUE_PROCESS Process;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string strAppName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string strServiceShortName;
            public RM_APP_TYPE ApplicationType;
            public uint AppStatus;
            public uint TSSessionId;
            [MarshalAs(UnmanagedType.Bool)]
            public bool bRestartable;
        }

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        private static extern int RmStartSession(out uint pSessionHandle, uint dwFlags, string strSessionKey);

        [DllImport("rstrtmgr.dll")]
        private static extern int RmEndSession(uint dwSessionHandle);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        private static extern int RmRegisterResources(uint dwSessionHandle, uint nFiles, string[] rgsFileNames,
                                                     uint nApplications, RM_UNIQUE_PROCESS[] rgApplications,
                                                     uint nServices, string[] rgsServiceNames);

        [DllImport("rstrtmgr.dll")]
        private static extern int RmGetList(uint dwSessionHandle, out uint pnProcInfoNeeded,
                                            ref uint pnProcInfo, [In, Out] RM_PROCESS_INFO[] rgAffectedApps,
                                            ref uint lpdwRebootReasons);

        private const int ERROR_MORE_DATA = 234;
        private const int ERROR_SUCCESS = 0;

        /// <summary>
        /// Finds all processes that are currently locking a given file.
        /// </summary>
        public static List<System.Diagnostics.Process> GetLockingProcesses(string filePath)
        {
            var processes = new List<System.Diagnostics.Process>();
            string sessionKey = Guid.NewGuid().ToString();

            // 1. Start a Restart Manager Session
            if (RmStartSession(out uint sessionHandle, 0, sessionKey) != ERROR_SUCCESS)
                return processes;

            try
            {
                // 2. Register the file resource we want to check
                string[] resources = { filePath };
                if (RmRegisterResources(sessionHandle, (uint)resources.Length, resources, 0, null, 0, null) != ERROR_SUCCESS)
                    return processes;

                // 3. Fetch the size/count of processes locking the file
                uint pnProcInfoNeeded = 0;
                uint pnProcInfo = 0;
                uint rebootReasons = 0;

                int res = RmGetList(sessionHandle, out pnProcInfoNeeded, ref pnProcInfo, null, ref rebootReasons);

                if (res == ERROR_MORE_DATA)
                {
                    // Create an array to hold the process data
                    var processInfo = new RM_PROCESS_INFO[pnProcInfoNeeded];
                    pnProcInfo = pnProcInfoNeeded;

                    // 4. Get the actual list of processes
                    if (RmGetList(sessionHandle, out pnProcInfoNeeded, ref pnProcInfo, processInfo, ref rebootReasons) == ERROR_SUCCESS)
                    {
                        for (int i = 0; i < pnProcInfo; i++)
                        {
                            try
                            {
                                // Try to bind to the live .NET Process object via PID
                                processes.Add(Process.GetProcessById(processInfo[i].Process.dwProcessId));
                            }
                            catch (ArgumentException)
                            {
                                // Catch if the process terminated after the list was gathered
                            }
                        }
                    }
                }
            }
            finally
            {
                // 5. Always close the Restart Manager session
                RmEndSession(sessionHandle);
            }

            return processes;
        }

        /// <summary>
        /// Finds and terminates any process locking the target file.
        /// </summary>
        public static void KillLockingProcesses(string filePath)
        {
            List<System.Diagnostics.Process> lockingProcesses = GetLockingProcesses(filePath);

            foreach (var process in lockingProcesses)
            {
                try
                {
                    Console.WriteLine($"Killing locking process: {process.ProcessName} (PID: {process.Id})");
                    process.Kill(true); // Kills the process and optionally all child processes
                    process.WaitForExit(); // Wait to guarantee the lock is fully dropped
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not kill process {process.Id}: {ex.Message}");
                }
            }
        }
    }
}
