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

### How to configure the definition XML
The simplest rule you can create would be just providing the key name you wish to inventory.  
`<registryrule key="HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion" />`

To further pinpoint your results you can add a name attribute and specify a value's name that you wish to inventory. This will only return the result if a registry value matches the name specified.  
`'<registryrule key="HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion" name="ProgramFilesDir"/>'`

Using the wildcard (\*) character will let you partially match value names.  
`'<registryrule key="HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion" name="ProgramFiles*"/>'`

Additional attributes currently available are:
**defaultvalueoption** - How to handle the default value of a key. (onlyifvalueisset is default)
- neverinventory
- onlyifvalueisset (default)
- onlyifkeyisempty
- onlyifvalueissetorkeyisempty

**inventoryemptyvalues** - Set true/false to indicate if you want to return results where the value is null/empty. (false is default)

**showmodifiedtime** - Set true/false to indicate if you want to show the last time the key was written to. (false is default)

**showkernelpath** - Set true/false to indicate if you want to return the kernel path of the current key. (false is default)


## Thank You

More information to come.  Please feel free to reach out to me with suggestions, comments.  I'm not a professional developer so I look forward to improving my skills while working on this project.

