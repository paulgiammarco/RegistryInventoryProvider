using Microsoft.Win32;
using RegistryInventoryProvider.ConfigurationFile;
using RegistryInventoryProvider.Scanner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;

[assembly: WmiConfiguration("root\\cimv2", HostingModel = ManagementHostingModel.LocalSystem)]
namespace RegistryInventoryProvider.Provider
{

    [ManagementEntity(Name = "Win32_RegistryInventory")]
    [ManagementQualifier("Description", Value = "Return registry inventory based on a set of rules defined in XML.")]
    public class RegistryInventory
    {

        // Class properties

        [ManagementKey(Name = "KeyPath")]
        public string KeyPath { get; set; }

        [ManagementKey(Name = "Name")]
        public string Name { get; set; }

        [ManagementProbe(Name = "Value")]
        public string Value { get; set; }

        [ManagementProbe(Name = "Type")]
        public string Type { get; set; }

        [ManagementProbe(Name = "KeyLastWriteTime")]
        public string KeyLastWriteTime { get; set; }

        [ManagementProbe(Name = "KernelPath")]
        public string KernelPath { get; set; }

        // Options
        private static string definitionPath;
        private static string logPath;
        private static bool logEnabled;
        private static bool providerEnabled;

        public RegistryInventory(string keyPath, string name, string value, string type, string keylastwritetime = "", string kernelpath = "")
        {
            KeyPath = keyPath;
            Name = name;
            Value = value;
            Type = type;
            KeyLastWriteTime = string.IsNullOrEmpty(keylastwritetime) ? null : keylastwritetime;
            KernelPath = string.IsNullOrEmpty(kernelpath) ? null : kernelpath;
        }

        [ManagementEnumerator]
        public static IEnumerable<RegistryInventory> EnumerateInstances()
        {

            LoadOptions();
            Log.LogLocation = logPath;
            Log.LogEnabled = logEnabled;

            Log.Write("Beginning enumeration of WMI Class");

            if (!providerEnabled)
            {
                Log.Write("Provider has been disabled by provider settings in the registry. Exiting without returning any instances.");
                yield break;
            }

            if (!System.IO.File.Exists(definitionPath))
            {
                Log.Write($"Definition file could not be found at \"{definitionPath}\". Exiting without returning any instances.");
                yield break;
            }

            //Load Definition
            InventorySettings RegistryInventorySetting = InventorySettings.LoadFromFile(definitionPath);

            if (RegistryInventorySetting is null)
            {
                Log.Write($"Definition file was unable to load. Exiting without returning any instances.");
                yield break;
            }

            if (!RegistryInventorySetting.Enabled)
            {
                Log.Write($"Provider has been disabled by provider settings in the definition file. Exiting without returning any instances.");
                yield break;
            }

            // Yield Results
            foreach (RegistryInventory Result in ResultCollector.GetResultsFromRuleset(RegistryInventorySetting))
            {
                yield return Result;
            }

            Log.Write("Sucessfully finished enumeration. Exiting.");

        }

        private static void LoadOptions()
        {

            //Set default values
            definitionPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\RegistryInventoryProvider\definition.xml";
            logPath = string.Empty;
            logEnabled = false;
            providerEnabled = true;

            RegistryKey OptionsKey = null;

            try
            {
                OptionsKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey("SOFTWARE\\RegistryInventoryProvider");
            }
            catch { }

            if (!(OptionsKey is null))
            {
                foreach (string ValueName in OptionsKey.GetValueNames())
                {
                    string Value = OptionsKey.GetValue(ValueName).ToString();

                    switch (ValueName.ToLowerInvariant())
                    {
                        case "definitionlocation":
                            definitionPath = Value;
                            break;
                        case "loglocation":
                            logPath = Value;
                            break;
                        case "logenabled":
                            logEnabled = Value == "0" ? false : true;
                            break;
                        case "providerenabled":
                            providerEnabled = Value == "0" ? false : true;
                            break;
                        default:
                            break;
                    }

                }

            }
        }
    }
}