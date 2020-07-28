# Registry Inventory Provider

### Summary
WMI Provider developed using the .NET WMI Provider Extension Framework.  This will take a list of rules from an xml definition file.

### Installation
You must have .NET 4.5 or higher installed on the system.  Use the built in installutil.exe application to install the provider into your WMI repository.

Example: `"C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe" "RegistryInventoryProvider.dll"`

Uninstalling is just as easy, add `/U` to the above command to run the uninstall routine.

While in preview releases, the install routine will create a definition xml in the location: `"C:\ProgramData\RegistryInventoryProvider\definition.xml"`. This will contain one test rule for the windows run (64-Bit) key.  Use this as a template to create new rules.  Please remember that XML is case sensitive so all attributes need to be lowercase.

To view results, enumerate the inventory class called `Win32_RegistryInventory`.  It is located in the `ROOT\cimv2` namespace.  This can be done with a simple powershell command 
`Get-CimInstance -ClassName Win32_RegistryInventory` or using one of my favorite utilities [WMIExplorer](https://github.com/vinaypamnani/wmie2/releases).


More information to come.  Please feel free to reach out to me with suggestions, comments.  I'm not a professional developer so I look forward to improving my skills while working on this project.
