namespace NP
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;

    public class Mem
    {
        public static int BaseAddress;
        public static int m_iNumberOfBytesRead;
        public static int m_iNumberOfBytesWritten;
        public static IntPtr m_pProcessHandle;
        public static Process m_Process;
        private const int PROCESS_VM_OPERATION = 8;
        private const int PROCESS_VM_READ = 0x10;
        private const int PROCESS_VM_WRITE = 0x20;

        private static T ByteArrayToStructure<T>(byte[] bytes) where T: struct
        {
            T local;
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                local = (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
            return local;
        }

        public static float[] ConvertToFloatArray(byte[] bytes)
        {
            if ((bytes.Length % 4) > 0)
            {
                throw new ArgumentException();
            }
            float[] numArray = new float[bytes.Length / 4];
            for (int i = 0; i < numArray.Length; i++)
            {
                numArray[i] = BitConverter.ToSingle(bytes, i * 4);
            }
            return numArray;
        }

        public static int GetModuleAdress(string ModuleName)
        {
            try
            {
                foreach (ProcessModule module in m_Process.Modules)
                {
                    if (!ModuleName.Contains(".dll"))
                    {
                        ModuleName = ModuleName.Insert(ModuleName.Length, ".dll");
                    }
                    if (ModuleName == module.ModuleName)
                    {
                        return (int) module.BaseAddress;
                    }
                }
            }
            catch
            {
            }
            return -1;
        }

        public static void Initialize(string ProcessName)
        {
            if (Process.GetProcessesByName(ProcessName).Length > 0)
            {
                m_Process = Process.GetProcessesByName(ProcessName)[0];
                BaseAddress = Process.GetProcessesByName(ProcessName)[0].MainModule.BaseAddress.ToInt32();
            }
            else
            {
                //MessageBox.Show("Open Ros.exe first");
                Environment.Exit(1);
            }
            m_pProcessHandle = OpenProcess(0x38, false, m_Process.Id);
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        public static float[] ReadMatrix<T>(int Adress, int MatrixSize) where T: struct
        {
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T)) * MatrixSize];
            ReadProcessMemory((int) m_pProcessHandle, Adress, buffer, buffer.Length, ref m_iNumberOfBytesRead);
            return ConvertToFloatArray(buffer);
        }

        public static T ReadMemory<T>(int Adress) where T: struct
        {
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
            ReadProcessMemory((int) m_pProcessHandle, Adress, buffer, buffer.Length, ref m_iNumberOfBytesRead);
            return ByteArrayToStructure<T>(buffer);
        }

        public static byte[] ReadMem(int addr, int size)
        {
            byte[] array = new byte[size];
            Mem.ReadProcessMemory((int)Mem.m_pProcessHandle, addr, array, size, ref Mem.m_iNumberOfBytesRead);
            return array;
        }

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] buffer, int size, ref int lpNumberOfBytesRead);
        public static string ReadString(int address, int _Size)
        {
            byte[] buffer = new byte[_Size];
            ReadProcessMemory((int) m_pProcessHandle, address, buffer, _Size, ref m_iNumberOfBytesRead);
            return Encoding.ASCII.GetString(buffer);
        }

        private static byte[] StructureToByteArray(object obj)
        {
            int cb = Marshal.SizeOf(obj);
            byte[] destination = new byte[cb];
            IntPtr ptr = Marshal.AllocHGlobal(cb);
            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, destination, 0, cb);
            Marshal.FreeHGlobal(ptr);
            return destination;
        }

        public static void WriteMemory<T>(int Adress, object Value) where T: struct
        {
            byte[] buffer = StructureToByteArray(Value);
            WriteProcessMemory((int) m_pProcessHandle, Adress, buffer, buffer.Length, out m_iNumberOfBytesWritten);
        }

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] buffer, int size, out int lpNumberOfBytesWritten);
    }
}

