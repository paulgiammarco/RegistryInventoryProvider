using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RegistryInventoryProvider
{
    // Temporary Class to get information from the GAC until I can replace with native fusion.dll data

    class GacUtilities
    {
        private string _name;
        private Version _version;
        private string _cultureName;
        private string _publicKeyToken;
        private string _architecture;

        public string LocationInGAC { get; }
        public string Location { get; }

        public bool IsAssemblyRegistered()
        {
            if (!string.IsNullOrEmpty(Location) && IsAssemblyInRegistry())
            {
                return true;
            }
            return false;
        }

        public GacUtilities(Assembly assembly)
        {

            if (assembly is null) return;

            Location = assembly.Location;

            AssemblyName assemblyName = AssemblyName.GetAssemblyName(assembly.Location);

            _name = assemblyName.Name;
            _version = assemblyName.Version;
            _cultureName = assemblyName.CultureName;
            _publicKeyToken = GetPublicKeyTokenString(assemblyName);
            _architecture = assemblyName.ProcessorArchitecture.ToString().ToLowerInvariant();

            LocationInGAC = GetGacLocation();

        }

        private string GetPublicKeyTokenString(AssemblyName assemblyName)
        {
            StringBuilder tokenString = new StringBuilder();
            foreach (byte b in assemblyName.GetPublicKeyToken())
            {
                tokenString.Append(b.ToString("x2"));
            }

            return tokenString.ToString();
        }

        private string GetGacLocation()
        {
            string GacPathIdentifier = $@"_{_version}_{_cultureName}_{_publicKeyToken}";
            string GacRootPath = $@"C:\windows\Microsoft.NET\assembly\{GetGacPathArchitecture()}";

            string AssemblyLocation = Directory.GetDirectories(GacRootPath, _name, SearchOption.TopDirectoryOnly).FirstOrDefault();
            string FullAssemblyLocation = Directory.GetDirectories(AssemblyLocation, $@"*{GacPathIdentifier}", SearchOption.TopDirectoryOnly).FirstOrDefault();
            string FullAssemblyPath = $@"{FullAssemblyLocation}\{_name}.dll";

            if (System.IO.File.Exists(FullAssemblyPath))
            {
                return FullAssemblyPath;
            }

            return "";
        }

        private string GetGacPathArchitecture()
        {
            switch (_architecture.ToLowerInvariant())
            {
                case "x86":
                    return "GAC_32";
                case "amd64":
                    return "GAC_64";
                case "msil":
                    return "GAC_MSIL";
            }

            return "";
        }

        private bool IsAssemblyInRegistry()
        {

            string GacRegistryIdentifier = $@"{_name},{_version},{_cultureName},{_publicKeyToken},{_architecture}";

            RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

            if (registryKey.OpenSubKey(@"SOFTWARE\Microsoft\Fusion\GACChangeNotification\Default")?.GetValueNames()?.Where(x => x.ToLowerInvariant() == GacRegistryIdentifier.ToLowerInvariant()).ToArray().Count() > 0)
            {
                return true;
            }

            return false;

        }

    }
}
