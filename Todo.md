### Bugs and Needed Features
* [x] Drag and Drop from Document Outline
* [x] Update Emoji Picker (use Html and color icons)
* [x] Add Normalize Whitespace Extended Html Formatter
* [x] Add Checkbox List Extended Html Formatter
* [ ] Remove internal HtmlPackager and Replace with NuGet Package
* [ ] Allow saving documents as admin when permissions fail
* [ ] mmCLI: Enable Windows Long Path Support (win10)

#### Enhancements
* [ ] Edit Code In Editor on Context Menu (like Edit Image/Edit HREF)
* [x] Move some dialogs (like new version) to Windows Notifications

### Nice to Have
* Conversion .NET Core 6.0  
	* Issues:
		* `ExcecuteScript()` functionality
		* use of `dynamic` fixes
		* Figure out Runtime Distribution

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

	* Wpf.FontAwesome  
	  *(apparently works with net462 binary)*
	* HUnspell  
	  *(apparently works with net462 binary)*
* Pathing Resolution issues 
	* Weird File not Found errors  
	*(file not found errors in Folder Browser and other places)*
	* Addins and binary files dump into output folder
	* ~~GoUrl() with filename breaking on paths with spaces (fixed)  
	  *(due to change in default of ShellExecute in ProcessInfo)*~~
	 