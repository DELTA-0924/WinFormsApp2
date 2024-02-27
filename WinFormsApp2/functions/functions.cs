using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
namespace WinFormsApp2.functions
{
    public  class Functions
    {
        static private string _name = String.Empty;
        static public readonly string _path= "C:\\Users\\Scorp\\OneDrive\\Рабочий стол\\";
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern void RtlCopyMemory(IntPtr destination, IntPtr source, uint length);

        const uint PAGE_READWRITE = 0x04;
        const uint FILE_MAP_ALL_ACCESS = 0x0002;
        const int BUF_SIZE = 4096;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateFileMapping(
            IntPtr hFile,
            IntPtr lpFileMappingAttributes,
            uint flProtect,
            uint dwMaximumSizeHigh,
            uint dwMaximumSizeLow,
            string lpName
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr MapViewOfFile(
            IntPtr hFileMappingObject,
            uint dwDesiredAccess,
            uint dwFileOffsetHigh,
            uint dwFileOffsetLow,
            uint dwNumberOfBytesToMap
        );


        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess fileAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            FileAttributes flags,
            IntPtr template
            );
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool VirtualQuery(
                IntPtr lpAddress,
                out MEMORY_BASIC_INFORMATION lpBuffer,
                uint dwLength
                );

        [StructLayout(LayoutKind.Sequential)]
        struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        static public void FileCreate(string name)
        {
            _name=name;
            //FileStream context=File.Create($"{name}.txt");
            //if (context.CanWrite && context.CanRead)
            //{
            //    MessageBox.Show("your file has been created");
            //    //FileWrite();
            //}
            //else MessageBox.Show("Error:File has not created");
        }
        static public void FileWrite()
        {
            string content = "ZZZBBDDDCCAA";
            File.WriteAllText($"{_path}{_name}.txt",content);

        }

        public static byte[] ReadDrive( int sizeToRead)
        {
            
            if (sizeToRead < 1)
                throw new System.ArgumentException("Size parameter cannot be null or 0 or less than 0!");
            SafeFileHandle drive = CreateFile(
                                    fileName:$"{_name}.txt" ,
                                    fileAccess: FileAccess.Read,
                                    fileShare: FileShare.Write | FileShare.Read | FileShare.Delete,
                                    securityAttributes: IntPtr.Zero,
                                    creationDisposition: FileMode.OpenOrCreate,
                                    flags: FileAttributes.Normal,
                                    template: IntPtr.Zero
                                );
            if (drive.IsInvalid)
            {
                throw new IOException("Unable to access drive. Win32 Error Code " +
                Marshal.GetLastWin32Error());
                //if get windows error code 5 this means access denied.
                //You must try to run the program as admin privileges.
            }
 

            //чтение данных по дескриптору файла
            byte[] buf = new byte[512];
            using (FileStream diskStreamToRead = new FileStream(drive, FileAccess.Read)) { 
            diskStreamToRead.Read(buf, 0, 512);
            try { diskStreamToRead.Close(); } catch { }//закрытие файлового потока
            try { drive.Close(); } catch { }//закрытие дескриптора
            }

            return buf;
        }
        public static void writeToDisk(byte[] dataToWrite,Encoding encoding)
        {
            if (dataToWrite == null)
                throw new System.ArgumentException("dataToWrite parameter cannot be null!");
            
            

            SafeFileHandle drive = CreateFile(
                fileName: $"{_name}.txt",
                fileAccess: FileAccess.Write,
                fileShare: FileShare.Write | FileShare.Read | FileShare.Delete,
                securityAttributes: IntPtr.Zero,
                creationDisposition: FileMode.OpenOrCreate,
                flags: FileAttributes.Normal,
                template: IntPtr.Zero
            );

            using (StreamWriter writer = new StreamWriter(new FileStream(drive, FileAccess.Write), encoding))
            {
                // Записываем данные в файл с использованием указанной кодировки
                writer.Write(encoding.GetString(dataToWrite));
            }

            // Опционально, закрываем файлы
            try { drive.Close(); }
            catch { }
        }



        static public  string MapView()
        {
            const string szName = "MyMemoryMappedFile"; // Имя файла отображения

            IntPtr hMapFile = CreateFileMapping(
                IntPtr.Zero,
                IntPtr.Zero,
                PAGE_READWRITE,
                0,
                BUF_SIZE,
                szName
            );

            if (hMapFile == IntPtr.Zero)
            {
                Debug.WriteLine($"Could not create file mapping object ({Marshal.GetLastWin32Error()}).");
                return "Error";
            }

            IntPtr pBuf = MapViewOfFile(
                hMapFile,
                FILE_MAP_ALL_ACCESS,
                0,
                0,
                BUF_SIZE
            );

            if (pBuf == IntPtr.Zero)
            {
                Debug.WriteLine($"Could not map view of file ({Marshal.GetLastWin32Error()}).");
                CloseHandle(hMapFile);
                return "Error";
            }

            // Здесь вы можете использовать pBuf для доступа к памяти
            MEMORY_BASIC_INFORMATION mbi;
            string? res;
            if (VirtualQuery(pBuf, out mbi, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION))) != false)
            {
                long baseAddress = (long)pBuf;
                res= $"Базовый адрес выделенной области виртуальной памяти: 0x{ baseAddress:X}\n";

                res += $"Размер отображенной области: {mbi.RegionSize.ToInt32()} байт";
            }
            else
            {
                 res=$"Could not get size of mapped view ({Marshal.GetLastWin32Error()}).";
            }

                CloseHandle(hMapFile);
            return res ?? "Error";
        }
        public static void CopyMemory(byte[]bufers,out byte[]Copybuf)
        {
            // Пример использования функции CopyMemory
            byte[] sourceData = bufers;
            byte[] destinationData = new byte[sourceData.Length];

            // Выделение памяти для копирования
            IntPtr sourcePtr = Marshal.AllocHGlobal(sourceData.Length);
            IntPtr destinationPtr = Marshal.AllocHGlobal(destinationData.Length);

            try
            {
                // Копирование данных из массива sourceData в выделенную память
                Marshal.Copy(sourceData, 0, sourcePtr, sourceData.Length);

                // Вызов функции CopyMemory для копирования данных
                RtlCopyMemory(destinationPtr, sourcePtr, (uint)sourceData.Length);

                // Копирование данных из выделенной памяти в массив destinationData
                Marshal.Copy(destinationPtr, destinationData, 0, destinationData.Length);           
                Copybuf = destinationData;
            }
            finally
            {
                // Освобождение выделенной памяти
                Marshal.FreeHGlobal(sourcePtr);
                Marshal.FreeHGlobal(destinationPtr);
            }
        }
    }
}

    
