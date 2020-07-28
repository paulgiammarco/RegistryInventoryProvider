using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RegistryInventoryProvider.ConfigurationFile
{
    [XmlRoot("scannersettings")]
    public class InventorySettings
    {
        [XmlAttribute("enabled")]
        public bool Enabled { get; set; } = true;
        [XmlElement("registryrule")]
        public List<InventoryRule> InventoryRules { get; set; } = new List<InventoryRule>();

        private InventorySettings() { }
      
        /// <summary>
        /// Loads settings from an XML definition file
        /// </summary>
        /// <param name="filename">Location of the definition XML file</param>
        /// <returns></returns>
        public static InventorySettings LoadFromFile(string filename)
        {
            if (File.Exists(filename))
            {
                XmlSerializer xml = new XmlSerializer(typeof(InventorySettings));

                using (TextReader reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    
                    try
                    {
                        //Load from XML
                        InventorySettings tmpInventorySettings = (InventorySettings)xml.Deserialize(reader);

                        // Remove duplicate inventory rules (take the first key/name combo)
                        tmpInventorySettings.InventoryRules = tmpInventorySettings.InventoryRules
                            .GroupBy(x => new { x.Key, x.Name })
                            .Select(z => z.First())
                            .ToList();

                        // Remove inventory rules without key information
                        tmpInventorySettings.InventoryRules = tmpInventorySettings.InventoryRules
                            .Where(x => !string.IsNullOrEmpty(x.Key))
                            .ToList();

                        //Return definition
                        return tmpInventorySettings;
                    }

                    catch (Exception ex)
                    {
                        Log.Write($"{System.Reflection.MethodInfo.GetCurrentMethod().Name}: {(ex.Message)}");
                    }
                }
            }
            else
            {
                Log.Write($"{System.Reflection.MethodInfo.GetCurrentMethod().Name}: Definition file was not found.");
            }

            return null;

        }



    }
}
