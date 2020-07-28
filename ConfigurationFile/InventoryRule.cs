using Microsoft.Win32;
using RegistryInventoryProvider.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RegistryInventoryProvider.ConfigurationFile
{
    public class InventoryRule
    {

        public enum DefaultValueOption
        {
            [XmlEnum("neverinventory")] NeverInventory,
            [XmlEnum("onlyifvalueisset")] OnlyIfValueIsSet,
            [XmlEnum("onlyifkeyisempty")] OnlyIfKeyIsEmpty,
            [XmlEnum("onlyifvalueissetorkeyisempty")] OnlyIfValueIsSetOrKeyIsEmpty
        }

        [XmlAttribute("key")]
        public string Key { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; } = "*";
        [XmlAttribute("defaultvalue")]
        public DefaultValueOption DefaultValue { get; set; } = DefaultValueOption.OnlyIfValueIsSet;
        [XmlAttribute("inventoryemptyvalues")]
        public bool InventoryEmptyValues { get; set; } = false;
        [XmlAttribute("showmodifiedtime")]
        public bool ShowModifiedTime { get; set; } = false;
        [XmlAttribute("showkernelpath")]
        public bool ShowKernelPath { get; set; } = false;
    
    }
}
