using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using log4net;
using System.Security;

namespace Perrich.RunAsService.Process
{
    /// <summary>
    /// Manage the processes using kernel32.dll
    /// </summary>
    public class ProcessManager : IProcessManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProcessManager));

        public IProcessWrapper GetProcess(String executable, String parameters)
        {
            var fileName = Path.GetFileName(executable);
            var dirName = Path.GetDirectoryName(executable);

            if (fileName == null || dirName == null)
            {
                Log.Error(string.Format("executable {0} not found!", executable));
                throw new FileNotFoundException(string.Format("executable {0} not found!", executable));
            }

            Log.Debug("Starting...");

            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = parameters,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                WorkingDirectory = dirName
            };


            return new ProcessWrapper(new System.Diagnostics.Process { StartInfo = startInfo, EnableRaisingEvents = true });
        }

        public void KillProcess(IProcessWrapper process, bool killChildren)
        {
            if (killChildren)
            {
                KillChildren((uint)process.Id);
            }
            else
            {
                process.Kill();
            }
        }

        private static void KillChildren(uint pid)
        {
            var processes = System.Diagnostics.Process.GetProcesses();
            foreach (var p in processes)
            {
                try
                {
                    if (pid == GetParentProcess((uint) p.Id))
                    {
                        KillChildren((uint)p.Id);
                        if (Log.IsDebugEnabled)
                        {
                            Log.Debug(string.Format("Kill {0} child process.", p.ProcessName));
                        }
                        p.Kill();
                    }
                }
                catch (Exception e)
                {
                    Log.Error(string.Format("Exception when trying to kill children of {0} pid: {1}", pid, e.Message));
                }

                p.Dispose();
            }
        }

        /// <summary>
        /// Get the parent process PID
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        private static uint GetParentProcess(uint pid)
        {
            //entry of process information
            var pe = new Processentry32
                          {
                              dwSize = ((uint)Marshal.SizeOf(typeof(Processentry32)))
                          };


            //Handle to get process information
            var handleProcess = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);

            uint parentPid = 0;

            try
            {
                //Get the process entry for the first Handle
                var rv = Process32First(handleProcess, ref pe);

                if (!rv)
                {
                    CloseHandle(handleProcess);
                    throw new SecurityException("Cannot iterate on process list");
                }

                while (rv)
                {
                    if (pid == pe.th32ProcessID) //is it the current process?
                    {
                        parentPid = pe.th32ParentProcessID;
                        break;
                    }

                    //Get the process entry for the next Handle
                    rv = Process32Next(handleProcess, ref pe);
                }
            }
            finally
            {
                CloseHandle(handleProcess);
            }

            return parentPid;
        }

        #region kernel32.dll methods import

        private const int TH32CS_SNAPPROCESS = 2;

        /// <summary>
        /// process entry
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct Processentry32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public uint th32ModuleID;
            public uint cntThreads;
            public uint th32ParentProcessID;
            public int pcPriClassBase;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExeFile;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags,
                                                              uint th32ProcessID);


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool Process32First(IntPtr hSnapshot, ref Processentry32 lppe);


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool Process32Next(IntPtr hSnapshot, ref Processentry32 lppe);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        #endregion
    }
}