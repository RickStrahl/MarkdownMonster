### Bugs and Needed Features

* [ ] Gist Addin - Fix Refresh in Previewer
* [ ] Allow saving documents as admin when permissions fail

#### Enhancements
* [ ] Open Files List from Command Palette

### Consideration
* [ ] Markup (Publishing) support
* [ ] reveal.js presentations as an Addin
* [ ] FTP Upload Addin (started)


### Markdown Monster .NET CORE 6.0 Migration

**Issues**

* .NET Framework Assemblies that require updates
	* XmlRpc  
	  *(fails to load in Core)*
	* Windows Speech Engine (not supported for .NET Core)  
	  *(conditionally removed for Core)*
	  *Found explicit library that can be shipped from NuGet*

	* Wpf.FontAwesome  
	  *(apparently works with net462 binary)*
	* HUnspell (fixed with ported version)
	  *(apparently works with net462 binary)*
	  
* Pathing Resolution issues 
	* Weird File not Found errors  
	*(file not found errors in Folder Browser and other places)*
	* Addins and binary files dump into output folder
	* ~~GoUrl() with filename breaking on paths with spaces (fixed)  
	  *(due to change in default of ShellExecute in ProcessInfo)*~~
	 