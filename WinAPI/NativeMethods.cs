using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RegistryInventoryProvider.WinAPI
{
    internal static class NativeMethods
    {
        #region "PInvoke Methods"

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int RegLoadKey(uint hKey, string lpSubKey, string lpFile);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern int RegUnLoadKey(uint hKey, string lpSubKey);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int RegQueryInfoKey(IntPtr hKey, IntPtr lpClass, IntPtr lpcchClass, IntPtr lpReserved, IntPtr lpcSubKeys, IntPtr lpcbMaxSubKeyLen, IntPtr lpcbMaxClassLen, IntPtr lpcValues, IntPtr lpcbMaxValueNameLen, IntPtr lpcbMaxValueLen, IntPtr lpcbSecurityDescriptor, out FILETIME lpftLastWriteTime);

        [DllImport("ntdll.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int NtQueryKey(IntPtr KeyHandle, KEY_INFORMATION_CLASS KeyInformationClass, IntPtr KeyInformation, int Length, out int ResultLength);

        #endregion

        #region "Enums"

        public enum KEY_INFORMATION_CLASS
        {
            KeyBasicInformation,
            KeyNodeInformation,
            KeyFullInformation,
            KeyNameInformation,
            KeyCachedInformation,
            KeyFlagsInformation,
            KeyVirtualizationInformation,
            KeyHandleTagsInformation,
            MaxKeyInfoClass
        }

        #endregion

        #region "Structures"

        [StructLayout(LayoutKind.Sequential)]
        public struct FILETIME
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        }

        #endregion
    
    }
}