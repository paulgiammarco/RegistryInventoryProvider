using Microsoft.Win32;
using RegistryInventoryProvider.ConfigurationFile;
using RegistryInventoryProvider.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace RegistryInventoryProvider.Scanner
{

    class ResultCollector
    {

        public static RegistryInventory[] GetResultsFromRuleset(InventorySettings Settings)
        {
            
            List<RegistryInventory> tmpResults = new List<RegistryInventory>();
            
            foreach (InventoryRule rule in Settings.InventoryRules)
            {
                var results = new RuleHandler(rule).GetResults();
                if (!(results is null))
                {
                    tmpResults.AddRange(results);
                }
            }

            // Remove Duplicate Results
            tmpResults = tmpResults.GroupBy(x => new { x.KeyPath, x.Name }).Select(p => p.First()).ToList();

            return tmpResults.ToArray();

        }

    }

}
