using RegistryInventoryProvider;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Management.Instrumentation;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace RegistryInventory_WMIProvider
{

    [System.ComponentModel.RunInstaller(true)]
    public class ProviderInstaller : DefaultManagementInstaller
    {

        public override void Install(IDictionary stateSaver)
        {

            base.Install(stateSaver);

            try
            {
                //Install current assembly in to the GAC (This is needed for WMI Extension Providers to function)
                new System.EnterpriseServices.Internal.Publish().GacInstall(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            catch { }



        }

        public override void Commit(IDictionary savedState)
        {

            base.Commit(savedState);

            try
            {
                // Copy InstallState file to the regsitered GAC Location so InstallUtil can find it later.
                var InstallStateLocation = Path.ChangeExtension(System.Reflection.Assembly.GetExecutingAssembly().Location, ".InstallState");
                if (File.Exists(InstallStateLocation))
                {
                    GacUtilities GacInformation = new GacUtilities(Assembly.GetExecutingAssembly());
                    File.Move(InstallStateLocation, $@"{Path.GetDirectoryName(GacInformation.LocationInGAC)}\{Path.GetFileName(InstallStateLocation)}");
                }
            }
            catch { }

            string DefinitionPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\RegistryInventoryProvider\definition.xml";
            if (!File.Exists(DefinitionPath))
            {
                if (!Directory.Exists(Path.GetDirectoryName(DefinitionPath)))
                {
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(DefinitionPath));
                    }
                    catch { }
                }

                using (StreamWriter streamWriter = new StreamWriter(DefinitionPath, true))
                {
                    //write sample definition file
                    streamWriter.Write("<scannersettings>\r\n	<registryrule key=\"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run\" redirectionoption=\"bothlocations\" showkernelpath=\"true\" showmodifiedtime=\"true\"/>\r\n</scannersettings>");
                }

            }

        }

        public override void Uninstall(IDictionary savedState)
        {

            try
            {
                base.Uninstall(savedState);
            }
            catch { }

            try
            {
                new System.EnterpriseServices.Internal.Publish().GacRemove(System.Reflection.Assembly.GetExecutingAssembly().Location);
            }
            catch { }

        }

    }

}
