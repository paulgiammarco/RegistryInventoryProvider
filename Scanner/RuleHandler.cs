using Microsoft.Win32;
using RegistryInventoryProvider.ConfigurationFile;
using RegistryInventoryProvider.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static RegistryInventoryProvider.ConfigurationFile.InventoryRule;

namespace RegistryInventoryProvider.Scanner
{
    public class RuleHandler
    {
        private InventoryRule Rule;

        public RuleHandler(InventoryRule inventoryRule)
        {
            Rule = inventoryRule;
        }

        public RegistryInventory[] GetResults()
        {

            List<RegistryInventory> Results = new List<RegistryInventory>();
            RegistryHive Hive = new RegistryHive();
            string Path = string.Empty;

            // Extract the Hive and Subkey path from the key string
            if (!RegistryTools.TryGetRegistryHiveAndPath(Rule.Key, ref Hive, ref Path))
            {
                return Results.ToArray();
            }

            // Get redirection options

            // Get target registry key while validating existence and accurate case for path
            RegistryKey TargetKey = GetTargetKey(Hive, Path);

            // Get values from registry key
            if (TargetKey != null)
            {
                GetKeyValues(Results, TargetKey, Rule.Name);
                TargetKey.Dispose();
            }
            else
            {
                Log.Write($@"Key doesn't exist {Rule.Key}");
            }

            return Results.ToArray();

        }

        private void GetKeyValues(List<RegistryInventory> Results, RegistryKey TargetKey, string TargetValue)
        {

            // Get Values of registry items that match search criteria in current key
            string[] RegistryNameCollection = GetTargetNameCollection(TargetKey, TargetValue);

            // Get advanced properties if needed
            IntPtr HKey = TargetKey.Handle.DangerousGetHandle();

            string KernelKeyPath = Rule.ShowKernelPath ? RegistryTools.GetKeyNameFromPtr(HKey) : null;
            string LastModifiedDate = Rule.ShowModifiedTime ? RegistryTools.GetRegistryKeyLastModifiedDate(HKey).ToString() : null;

            // If RegistryNameCollection doesn't contain an empty string then the default value isn't set.  Add a blank result depending on the DefaultValue property
            if (!RegistryNameCollection.Contains(string.Empty))
            {
                if (RegistryNameCollection.Count() == 0 && (Rule.DefaultValue == DefaultValueOption.OnlyIfKeyIsEmpty || Rule.DefaultValue == DefaultValueOption.OnlyIfValueIsSetOrKeyIsEmpty))
                {
                    // Insert a blank (default) value if it is not set but requested
                    Results.Add(new RegistryInventory(TargetKey.ToString(), "(Default)", null, GetRegistryType(RegistryValueKind.String), LastModifiedDate, KernelKeyPath));
                }
            }

            // Get keys that match the search criteria
            foreach (string CurrentValueName in RegistryNameCollection)
            {

                // Handle (default) value. If NeverInventory or OnlyIfKeyIsEmpty (and key is not empty) continue to next ValueName
                if (string.IsNullOrEmpty(CurrentValueName))
                {
                    if (Rule.DefaultValue == DefaultValueOption.NeverInventory) { continue; }
                    if (Rule.DefaultValue == DefaultValueOption.OnlyIfKeyIsEmpty && RegistryNameCollection.Length > 0) { continue; }
                }

                string ValueData = ConvertRegistryValueToString(TargetKey.GetValue(CurrentValueName, "", RegistryValueOptions.DoNotExpandEnvironmentNames));
                string ValueType = GetRegistryType(TargetKey.GetValueKind(CurrentValueName));
                string ValueName = string.IsNullOrEmpty(CurrentValueName) ? "(Default)" : CurrentValueName;

                // Handle (default) value based on data.
                if (string.IsNullOrEmpty(CurrentValueName))
                {
                    if (Rule.DefaultValue == DefaultValueOption.OnlyIfValueIsSetOrKeyIsEmpty && (string.IsNullOrEmpty(ValueName) || RegistryNameCollection.Length > 1)) { continue; }
                    if (Rule.DefaultValue == DefaultValueOption.OnlyIfValueIsSet && string.IsNullOrEmpty(ValueName)) { continue; }
                }

                // Do not add result if the value is null and InventoryEmptyValue is false
                if (string.IsNullOrEmpty(ValueData) && !Rule.InventoryEmptyValues) { continue; }

                Results.Add(new RegistryInventory(TargetKey.ToString(), ValueName, ValueData, ValueType, LastModifiedDate, KernelKeyPath));

            }

        }

        private string ConvertRegistryValueToString(object value)
        {
            // Handles any possible registry data types and returns them similar to how regedit would display them.

            switch (value)
            {
                case string str:
                    return string.IsNullOrEmpty(str) ? null : str;

                case byte[] bytearray:
                    return BitConverter.ToString(bytearray).Replace('-', ' ').ToLowerInvariant();

                case int dword:
                    return $"0x{dword.ToString("X8").ToLowerInvariant()} ({(UInt32)dword})";

                case long qword:
                    return $"0x{qword.ToString("X16").ToLowerInvariant().TrimStart('0').PadLeft(8, '0')} ({(UInt64)qword})";

                case string[] strarray:
                    return $"{{{String.Join(", ", strarray)}}}";

                default:
                    return string.Empty;
            }

        }

        private string GetRegistryType(RegistryValueKind registryValueKind)
        {

            switch (registryValueKind)
            {
                case RegistryValueKind.String:
                    return "REG_SZ";
                case RegistryValueKind.ExpandString:
                    return "REG_EXPAND_SZ";
                case RegistryValueKind.Binary:
                    return "REG_BINARY";
                case RegistryValueKind.DWord:
                    return "REG_DWORD";
                case RegistryValueKind.MultiString:
                    return "REG_MULTI_SZ";
                case RegistryValueKind.QWord:
                    return "REG_QWORD";
                default:
                    return string.Empty;
            }

        }

        private static string[] GetTargetNameCollection(RegistryKey TargetKey, string TargetValue)
        {

            // Handle wildcards
            if (!string.IsNullOrEmpty(TargetValue) && TargetValue.Contains("*"))
            {
                string Pattern = TargetValue.Replace("*", ".*");
                Regex regex = new Regex(Pattern, RegexOptions.IgnoreCase);
                return TargetKey.GetValueNames().Where(x => regex.IsMatch(x)).ToArray<string>();
            }
            else if (!string.IsNullOrEmpty(TargetValue))
            {
                return TargetKey.GetValueNames().Where(x => string.Equals(x, TargetValue, StringComparison.InvariantCultureIgnoreCase)).ToArray<string>();
            }
            else
            {
                return TargetKey.GetValueNames().ToArray<string>();
            }

        }

        private RegistryKey GetTargetKey(RegistryHive hive, string path)
        {
            // Validates that the key exists and walks the subkeys to get correct case.
            RegistryKey KeyWalker = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);

            foreach (string key in path.Split('\\'))
            {

                var nextkeyname = KeyWalker.GetSubKeyNames().Where(x => string.Equals(x, key, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                if (!string.IsNullOrEmpty(nextkeyname))
                {
                    try
                    {
                        RegistryKey NextKey = KeyWalker.OpenSubKey(nextkeyname);
                        KeyWalker.Dispose();
                        KeyWalker = NextKey;
                    }
                    catch (Exception ex)
                    {
                        if (ex is SecurityException)
                        {
                            Log.Write($"Access Denied on {KeyWalker.ToString()}\\{nextkeyname}");
                        }
                        else
                        {
                            Log.Write($"{System.Reflection.MethodInfo.GetCurrentMethod().Name}: {(ex.Message)}");
                        }
                        return default;
                    }
                }
                else
                {
                    return default;
                }

            }

            return KeyWalker;

        }

    }
}
