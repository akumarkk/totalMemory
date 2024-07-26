
using System.Runtime.InteropServices;
using System.Management;

namespace Program
{
    class Program
    {
        public static void Main()
        {
            Console.WriteLine($"Hello world - agent {DateTime.UtcNow} {GetPhysicallyInstalledSystemMemory()}");
        }

        [DllImport("kernel32.dll")]
        static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_INFO
        {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public IntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        }



        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX(bool init)
            {
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
                dwMemoryLoad = 0;
                ullTotalPhys = 0;
                ullAvailPhys = 0;
                ullTotalPageFile = 0;
                ullAvailPageFile = 0;
                ullTotalVirtual = 0;
                ullAvailVirtual = 0;
                ullAvailExtendedVirtual = 0;
            }
        }

        static void PrintMemoryStatus()
        {
            MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX(true);

            if (GlobalMemoryStatusEx(ref memStatus))
            {
                Console.WriteLine("Total Physical Memory (RAM): " + (memStatus.ullTotalPhys / 1024 / 1024) + " MB");
                Console.WriteLine("Available Physical Memory: " + (memStatus.ullAvailPhys / 1024 / 1024) + " MB");
                Console.WriteLine("Total Virtual Memory: " + (memStatus.ullTotalVirtual / 1024 / 1024) + " MB");
                Console.WriteLine("Available Virtual Memory: " + (memStatus.ullAvailVirtual / 1024 / 1024) + " MB");
            }
            else
            {
                Console.WriteLine("Unable to retrieve memory status. Error code: " + Marshal.GetLastWin32Error());
            }
        }

        internal static ulong GetPhysicallyInstalledSystemMemory()
        {
            ulong totalMemoryInKilobytes;
            bool result = GetPhysicallyInstalledSystemMemory(out totalMemoryInKilobytes);
            
            if (!result)
            {
                // Retrieve the last error code
                int errorCode = Marshal.GetLastWin32Error();

                // Display the error code
                Console.WriteLine("Win32 Error Code: " + errorCode);

                // Optionally, get the error message
                string errorMessage = new System.ComponentModel.Win32Exception(errorCode).Message;
                Console.WriteLine("Error Message: " + errorMessage);
                Console.WriteLine($"Hello world - agent {DateTime.UtcNow} {totalMemoryInKilobytes} result = {result}");

                using (var searcher = new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        totalMemoryInKilobytes += Convert.ToUInt64(obj["Capacity"]) / 1024;
                    }
                }
                Console.WriteLine("Total memory Win32_PhysicalMemory: " + (totalMemoryInKilobytes / 1024 / 1024) + " GB");

                PrintMemoryStatus();
            }
            return totalMemoryInKilobytes;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out ulong totalMemoryInKilobytes);
    }
}
