
//#define useRemoteLoader 

using System;
using System.IO;
using System.Text;
//using System.Collections.Specialized;
//using System.Collections;

using Microsoft.CSharp;
using Microsoft.VisualBasic;
using System.Reflection;
using System.Runtime.Remoting;
using System.CodeDom.Compiler;


namespace Westwind.wwScripting
{
	/// <summary>
	/// Deletgate for the Completed Event
	/// </summary>
	public delegate void DelegateCompleted(object sender,EventArgs e);

	/// <summary>
	/// Class that enables running of code dynamcially created at runtime.
	/// Provides functionality for evaluating and executing compiled code.
	/// </summary>
    public class wwScripting
	{
		/// <summary>
		/// Compiler object used to compile our code
		/// </summary>
		protected ICodeCompiler oCompiler = null;

		/// <summary>
		/// Reference to the Compiler Parameter object
		/// </summary>
		protected CompilerParameters oParameters = null;

		/// <summary>
		/// Reference to the final assembly
		/// </summary>
		protected Assembly oAssembly = null;

		/// <summary>
		/// The compiler results object used to figure out errors.
		/// </summary>
		protected CompilerResults oCompiled = null;
		protected string cOutputAssembly = null;
		protected string cNamespaces = "";
		protected bool lFirstLoad = true;



		/// <summary>
		/// The object reference to the compiled object available after the first method call.
		/// You can use this method to call additional methods on the object.
		/// For example, you can use CallMethod and pass multiple methods of code each of
		/// which can be executed indirectly by using CallMethod() on this object reference.
		/// </summary>
		public object ObjRef = null;

		/// <summary>
		/// If true saves source code before compiling to the cSourceCode property.
		/// </summary>
		public bool SaveSourceCode = false;

		/// <summary>
		/// Contains the source code of the entired compiled assembly code.
		/// Note: this is not the code passed in, but the full fixed assembly code.
		/// Only set if lSaveSourceCode=true.
		/// </summary>
		public string SourceCode = "";

		/// <summary>
		/// Line where the code that runs starts
		/// </summary>
		protected int StartCodeLine = 0;

		/// <summary>
		/// Namespace of the assembly created by the script processor. Determines
		/// how the class will be referenced and loaded.
		/// </summary>
		public string AssemblyNamespace = "WestWindScripting";

		/// <summary>
		/// Name of the class created by the script processor. Script code becomes methods in the class.
		/// </summary>
		public string ClassName = "WestWindScript";

		/// <summary>
		/// Determines if default assemblies are added. System, System.IO, System.Reflection
		/// </summary>
		public bool DefaultAssemblies = true;

		protected AppDomain AppDomain = null;

		public string ErrorMessage = "";
		public bool Error = false;

		/// <summary>
		/// Path for the support assemblies wwScripting and RemoteLoader.
		/// By default this can be blank but if you're using this functionality
		/// under ASP.Net specify the bin path explicitly. Should include trailing
		/// dash.
		/// </summary>
		//[Description("Path for the support assemblies wwScripting and RemoteLoader. Blank by default. Include trailing dash.")]
		public string SupportAssemblyPath = "";

		/// <summary>
		/// The scripting language used. CSharp, VB, JScript
		/// </summary>
		public string ScriptingLanguage = "CSharp";
		
		/// <summary>
		/// The language to be used by this scripting class. Currently only C# is supported 
		/// with VB syntax available but not tested.
		/// </summary>
		/// <param name="language">CSharp or VB</param>
		public wwScripting(string language = "CSharp")
		{			
			this.SetLanguage(language);
		}
		
		

		/// <summary>
		/// Specifies the language that is used. Supported languages include
		/// CSHARP C# VB
		/// </summary>
		/// <param name="language"></param>
		public void SetLanguage(string language) 
		{
			this.ScriptingLanguage = language;

			if (this.ScriptingLanguage == "CSharp" || this.ScriptingLanguage == "C#") 
			{
				this.oCompiler = new CSharpCodeProvider().CreateCompiler();
				this.ScriptingLanguage = "CSharp";
			}	
			else if (this.ScriptingLanguage == "VB")	
			{
				this.oCompiler = new VBCodeProvider().CreateCompiler();
			}										   
			// else throw(Exception ex);

			this.oParameters = new CompilerParameters();
		}


		/// <summary>
		/// Adds an assembly to the compiled code
		/// </summary>
		/// <param name="assemblyDll">DLL assembly file name</param>
		/// <param name="nameSpace">Namespace to add if any. Pass null if no namespace is to be added</param>
		public void AddAssembly(string assemblyDll,string nameSpace) 
		{
			if (assemblyDll==null && nameSpace == null) 
			{
				// *** clear out assemblies and namespaces
				this.oParameters.ReferencedAssemblies.Clear();
				this.cNamespaces = "";
				return;
			}
			
			if (assemblyDll != null)
				this.oParameters.ReferencedAssemblies.Add(assemblyDll);
		
			if (nameSpace != null) 
				if (this.ScriptingLanguage == "CSharp")
					this.cNamespaces = this.cNamespaces + "using " + nameSpace + ";\r\n";
				else
					this.cNamespaces = this.cNamespaces + "imports " + nameSpace + "\r\n";
		}

		/// <summary>
		/// Adds an assembly to the compiled code.
		/// </summary>
		/// <param name="assemblyDll">DLL assembly file name</param>
		public void AddAssembly(string assemblyDll) 
		{
			this.AddAssembly(assemblyDll,null);
		}
		public void AddNamespace(string nameSpace)
		{
			this.AddAssembly(null,nameSpace);
		}
		public void AddDefaultAssemblies()
		{
			this.AddAssembly("System.dll","System");
		    this.AddAssembly("System.Core.dll");
		    this.AddAssembly("Microsoft.CSharp.dll");
			this.AddNamespace("System.Reflection");
			this.AddNamespace("System.IO");

		}


		/// <summary>
		/// Executes a complete method by wrapping it into a class.
		/// </summary>
		/// <param name="code">One or more complete methods.</param>
		/// <param name="methodName">Name of the method to call.</param>
		/// <param name="parameters">any number of variable parameters</param>
		/// <returns></returns>
		public object ExecuteMethod(string code, string methodName, params object[] parameters) 
		{
			
			if (this.ObjRef == null) 
			{
				if (this.lFirstLoad)
				{
					if (this.DefaultAssemblies) 
					{
						this.AddDefaultAssemblies();
					}
					//this.AddAssembly(this.SupportAssemblyPath + "RemoteLoader.dll","Westwind.RemoteLoader");
					//this.AddAssembly(this.SupportAssemblyPath + "wwScripting.dll","Westwind.wwScripting");
					this.lFirstLoad = false;
				}

				StringBuilder sb = new StringBuilder("");

				//*** Program lead in and class header
				sb.Append(this.cNamespaces);
				sb.Append("\r\n");

				if (this.ScriptingLanguage == "CSharp") 
				{
					// *** Namespace headers and class definition
					sb.Append("namespace " + this.AssemblyNamespace + "{\r\npublic class " + this.ClassName + ":MarshalByRefObject {\r\n");	
				
					// *** Generic Invoke method required for the remote call interface
					sb.Append(
						"public object Invoke(string lcMethod,object[] parms)\r\n{ " + 
						"return this.GetType().InvokeMember(lcMethod,BindingFlags.InvokeMethod,null,this,parms );\r\n" +
						"}\r\n\r\n" );

					//*** The actual code to run in the form of a full method definition.
					sb.Append(code);

					sb.Append("\r\n} }");  // Class and namespace closed
				}
				else if (this.ScriptingLanguage == "VB") 
				{
					// *** Namespace headers and class definition
					sb.Append("Namespace " + this.AssemblyNamespace + "\r\npublic class " + this.ClassName + "\r\n");
					sb.Append("Inherits MarshalByRefObject\r\nImplements IRemoteInterface\r\n\r\n");	
				
					// *** Generic Invoke method required for the remote call interface
					sb.Append(
						"Public Overridable Overloads Function Invoke(ByVal lcMethod As String, ByVal Parameters() As Object) As Object _\r\n" +
						"Implements IRemoteInterface.Invoke\r\n" + 
						"return me.GetType().InvokeMember(lcMethod,BindingFlags.InvokeMethod,nothing,me,Parameters)\r\n" +
						"End Function\r\n\r\n" );

					//*** The actual code to run in the form of a full method definition.
					sb.Append(code);

					sb.Append("\r\n\r\nEnd Class\r\nEnd Namespace\r\n");  // Class and namespace closed
				}

				if (this.SaveSourceCode)
				{
					this.SourceCode = sb.ToString();
					//MessageBox.Show(this.cSourceCode);
				}

				if (!this.CompileAssembly(sb.ToString()) )
					return null;

				object loTemp = this.CreateInstance();
				if (loTemp == null)
					return null;
			}

			return this.CallMethod(this.ObjRef,methodName,parameters);
		}

		/// <summary>
		///  Executes a snippet of code. Pass in a variable number of parameters
		///  (accessible via the loParameters[0..n] array) and return an object parameter.
		///  Code should include:  return (object) SomeValue as the last line or return null
		/// </summary>
		/// <param name="code">The code to execute</param>
		/// <param name="parameters">The parameters to pass the code</param>
		/// <returns></returns>
		public object ExecuteCode(string code, params object[] parameters) 
		{	
			if (this.ScriptingLanguage == "CSharp")
				return this.ExecuteMethod("public object ExecuteCode(params object[] Parameters) \r\n{\r\n" + 
						code + 
						"\r\n}",
						"ExecuteCode",parameters);
			else if (this.ScriptingLanguage == "VB")
				return this.ExecuteMethod("public function ExecuteCode(ParamArray Parameters() As Object) as object\r\n" + 
					code + 
					"\r\nend function\r\n",
					"ExecuteCode",parameters);

			return null;
		}
	    
		/// <summary>
		/// Compiles and runs the source code for a complete assembly.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public bool CompileAssembly(string source) 
		{
			//this.oParameters.GenerateExecutable = false;

			if (this.AppDomain == null && this.cOutputAssembly == null)
				this.oParameters.GenerateInMemory = true;
			else if (this.AppDomain != null && this.cOutputAssembly == null)
			{
				// *** Generate an assembly of the same name as the domain
				this.cOutputAssembly = "wws_" + Guid.NewGuid().ToString() + ".dll";
				this.oParameters.OutputAssembly = this.cOutputAssembly;
			}
			else {
				  this.oParameters.OutputAssembly = this.cOutputAssembly;
			}
		
			this.oCompiled = this.oCompiler.CompileAssemblyFromSource(this.oParameters,source);

			if (oCompiled.Errors.HasErrors) 
			{
				this.Error = true;

				// *** Create Error String
				this.ErrorMessage = oCompiled.Errors.Count.ToString() + " Errors:";
				for (int x=0;x<oCompiled.Errors.Count;x++) 
					this.ErrorMessage = this.ErrorMessage  + "\r\nLine: " + oCompiled.Errors[x].Line.ToString() + " - " + 
						                               oCompiled.Errors[x].ErrorText;				
				return false;
			}

			if (this.AppDomain == null)
				this.oAssembly = oCompiled.CompiledAssembly;
			
			return true;
		}

		public object CreateInstance() 
		{
			if (this.ObjRef != null) 
			{
				return this.ObjRef;
			}
			
			// *** Create an instance of the new object
			try 
			{
				if (this.AppDomain == null)
					try 
					{
						this.ObjRef =  oAssembly.CreateInstance(this.AssemblyNamespace + "." + this.ClassName);
						return this.ObjRef;
					}
					catch(Exception ex) 
					{
						this.Error = true;
						this.ErrorMessage = ex.Message;
						return null;
					}
				else 
				{
#if useRemoteLoader
                    // create the factory class in the secondary app-domain
                    RemoteLoaderFactory factory = (RemoteLoaderFactory) this.oAppDomain.CreateInstance( "RemoteLoader", "Westwind.RemoteLoader.RemoteLoaderFactory" ).Unwrap();

					// with the help of this factory, we can now create a real 'LiveClass' instance
					this.oObjRef = factory.Create( this.cOutputAssembly, this.cAssemblyNamespace + "." + this.cClassName, null );

					return this.oObjRef;			
#endif
				    return null;
				}	
			}
			catch(Exception ex) 
			{
				this.Error = true;
				this.ErrorMessage = ex.Message;
				return null;
			}
				
		}

		public object CallMethod(object loObject,string lcMethod, params object[] loParameters) 
		{
			// *** Try to run it
			try 
			{
				if (this.AppDomain == null)
					// *** Just invoke the method directly through Reflection
					return loObject.GetType().InvokeMember(lcMethod,BindingFlags.InvokeMethod,null,loObject,loParameters );
				else 
				{
#if useRemoteLoader
					// *** Invoke the method through the Remote interface and the Invoke method
					object loResult;
					try 
					{
						// *** Cast the object to the remote interface to avoid loading type info
						IRemoteInterface loRemote = (IRemoteInterface) loObject;

						// *** Indirectly call the remote interface
						loResult = loRemote.Invoke(lcMethod,loParameters);
					}
					catch(Exception ex) 
					{
						this.bError = true;
						this.cErrorMsg = ex.Message;
						return null;
					}
					return loResult;
#endif  
                }	

            }
            catch (Exception ex) 
			{
				this.Error = true;
				this.ErrorMessage = ex.Message;
			}
			return null;
		}

		public bool CreateAppDomain(string lcAppDomain) 
		{
			if (lcAppDomain == null)
				lcAppDomain = "wwscript";

			AppDomainSetup loSetup = new AppDomainSetup();
			loSetup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;

			this.AppDomain = AppDomain.CreateDomain(lcAppDomain,null,loSetup);
			return true;
		}

		public bool UnloadAppDomain()
		{
			if (this.AppDomain != null)
			   AppDomain.Unload(this.AppDomain);

			this.AppDomain = null;

			if (this.cOutputAssembly != null) 
			{
				try 
				{
					File.Delete(this.cOutputAssembly);
				}
				catch(Exception) {;}
			}

			return true;
		}
		public void Release() 
		{
			this.ObjRef = null;
		}

		public void Dispose() 
		{
			this.Release();
			this.UnloadAppDomain();
		}

		~wwScripting() 
		{
			this.Dispose();
		}
	}


}