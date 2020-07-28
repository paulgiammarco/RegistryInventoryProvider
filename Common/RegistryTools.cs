using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using RegistryInventoryProvider.WinAPI;

namespace RegistryInventoryProvider
{
    public class RegistryTools
    {
        /// <summary>
        /// Returns a RegistryHive and String of a subkey path from a full path.
        /// </summary>
        /// <param name="fullpath"></param>
        /// <param name="hive"></param>
        /// <param name="path"></param>
        /// <returns>Return boolean value if the parse was successful.</returns>
        public static bool TryGetRegistryHiveAndPath(string fullpath, ref RegistryHive hive, ref string path)
        {
            path = path.TrimEnd('\\');
            string[] RegistryPathSplit = fullpath.Split(new char[] { '\\' }, 2);

            //Guard Clauses
            if ((string.IsNullOrEmpty(fullpath)) || (RegistryPathSplit.Count() < 2)) { return false; }

            //Convert Hive Root String to Registry Hive Class
            string HiveString = RegistryPathSplit[0];
            path = RegistryPathSplit[1];

            switch (HiveString.ToLowerInvariant())
            {
                case "hkcu":
                case "hkey_current_user":
                    hive = RegistryHive.CurrentUser;
                    break;
                case "hkcr":
                case "hkey_classes_root":
                    hive = RegistryHive.ClassesRoot;
                    break;
                case "hklm":
                case "hkey_local_machine":
                    hive = RegistryHive.LocalMachine;
                    break;
                case "hku":
                case "hkey_users":
                    hive = RegistryHive.Users;
                    break;
            }

            if (hive != 0 && !string.IsNullOrEmpty(path))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Returns string of full kernel key path from a key handle.
        /// </summary>
        /// <param name="HKey"></param>
        /// <returns></returns>
        public static string GetKeyNameFromPtr(IntPtr HKey)
        {

            int KeyInformationBufferSize = 0;
            string KeyName = string.Empty;

            // Run method initially as query to determine the necessary buffer size
            int NTStatus = NativeMethods.NtQueryKey(HKey, NativeMethods.KEY_INFORMATION_CLASS.KeyNameInformation, IntPtr.Zero, 0, out KeyInformationBufferSize);

            // Declare a pointer and allocate the correct size of our buffer.  This pinning is needed to ensure stability of the routine.
            IntPtr KeyInformationPtr = Marshal.AllocHGlobal(KeyInformationBufferSize);

            NTStatus = NativeMethods.NtQueryKey(HKey, NativeMethods.KEY_INFORMATION_CLASS.KeyNameInformation, KeyInformationPtr, KeyInformationBufferSize, out KeyInformationBufferSize);

            if (NTStatus == 0)
            {
                //Copy data from pointer address to byte array 
                byte[] bytes = new byte[KeyInformationBufferSize];
                Marshal.Copy(KeyInformationPtr, bytes, 0, KeyInformationBufferSize);

                // Convert byte array to unicode string. Offset start and length by 4 to compensate for the typedef LONG (C# equivalent int32) size of 4 bytes in structure
                KeyName = Encoding.Unicode.GetString(bytes, 4, bytes.Length - 4);

            }

            Marshal.FreeHGlobal(KeyInformationPtr);

            return KeyName;

        }

        public static bool LoadKey(RegistryHive registryHive, string Subkey, string HiveFilePath)
        {
            int ReturnValue = NativeMethods.RegLoadKey((uint)registryHive, Subkey, HiveFilePath);
            if (ReturnValue != 0)
            {
                Log.Write($@"RegLoadKey Error: {Marshal.GetLastWin32Error()}");
                return false;
            }

            Log.Write($@"RegLoadKey Successful");
            return true;
        }

        public static bool UnloadKey(RegistryHive registryHive, string Subkey)
        {

            int ReturnValue = NativeMethods.RegUnLoadKey((uint)registryHive, Subkey);
            if (ReturnValue != 0)
            {
                Log.Write($@"RegUnloadKey Error Code: {Marshal.GetLastWin32Error()}");
                return false;
            }

            Log.Write($@"RegUnloadKey Successful");
            return true;
        }


        /// <summary>
        /// Returns the last modified date of the key represented by the handle.
        /// </summary>
        /// <param name="HKey"></param>
        /// <returns></returns>
        public static DateTime GetRegistryKeyLastModifiedDate(IntPtr HKey)
        {
            NativeMethods.FILETIME LastWriteFileTime = new NativeMethods.FILETIME();
            
            var RESULT = NativeMethods.RegQueryInfoKey(HKey, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, out LastWriteFileTime);

            if(RESULT == 0)
            {
                long FullFileTime = (long)LastWriteFileTime.dwHighDateTime << 32 | (uint)LastWriteFileTime.dwLowDateTime;
                return DateTime.FromFileTime(FullFileTime);
            }
            else
            {
                return default;
            }
           
        }

    }

}
