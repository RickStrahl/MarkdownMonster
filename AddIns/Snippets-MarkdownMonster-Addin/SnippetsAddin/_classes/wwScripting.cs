
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
		protected ICodeCompiler Compiler = null;

		/// <summary>
		/// Reference to the Compiler Parameter object
		/// </summary>
		protected CompilerParameters Parameters = null;

		/// <summary>
		/// Reference to the final assembly
		/// </summary>
		protected Assembly Assembly = null;

		/// <summary>
		/// The compiler results object used to figure out errors.
		/// </summary>
		protected CompilerResults CompilerResults = null;
		protected string OutputAssembly = null;
		protected string Namespaces = "";
		protected bool FirstLoad = true;



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
			SetLanguage(language);
		}
		
		

		/// <summary>
		/// Specifies the language that is used. Supported languages include
		/// CSHARP C# VB
		/// </summary>
		/// <param name="language"></param>
		public void SetLanguage(string language) 
		{
			ScriptingLanguage = language;

			if (ScriptingLanguage == "CSharp" || ScriptingLanguage == "C#") 
			{
#pragma warning disable CS0618 // Type or member is obsolete
                Compiler = new CSharpCodeProvider().CreateCompiler();
#pragma warning restore CS0618 // Type or member is obsolete
                ScriptingLanguage = "CSharp";
			}	
			else if (ScriptingLanguage == "VB")	
			{
#pragma warning disable CS0618 // Type or member is obsolete
                Compiler = new VBCodeProvider().CreateCompiler();
#pragma warning restore CS0618 // Type or member is obsolete
            }										   
			// else throw(Exception ex);

			Parameters = new CompilerParameters();
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
				Parameters.ReferencedAssemblies.Clear();
				Namespaces = "";
				return;
			}
			
			if (assemblyDll != null)
				Parameters.ReferencedAssemblies.Add(assemblyDll);
		
			if (nameSpace != null) 
				if (ScriptingLanguage == "CSharp")
					Namespaces = Namespaces + "using " + nameSpace + ";\r\n";
				else
					Namespaces = Namespaces + "imports " + nameSpace + "\r\n";
		}

		/// <summary>
		/// Adds an assembly to the compiled code.
		/// </summary>
		/// <param name="assemblyDll">DLL assembly file name</param>
		public void AddAssembly(string assemblyDll) 
		{
			AddAssembly(assemblyDll,null);
		}
		public void AddNamespace(string nameSpace)
		{
			AddAssembly(null,nameSpace);
		}
		public void AddDefaultAssemblies()
		{
			AddAssembly("System.dll","System");
		    AddAssembly("System.Core.dll");
		    AddAssembly("Microsoft.CSharp.dll");
			AddNamespace("System.Reflection");
			AddNamespace("System.IO");

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
			
			if (ObjRef == null) 
			{
				if (FirstLoad)
				{
					if (DefaultAssemblies) 
					{
						AddDefaultAssemblies();
					}
					//this.AddAssembly(this.SupportAssemblyPath + "RemoteLoader.dll","Westwind.RemoteLoader");
					//this.AddAssembly(this.SupportAssemblyPath + "wwScripting.dll","Westwind.wwScripting");
					FirstLoad = false;
				}

				StringBuilder sb = new StringBuilder("");

				//*** Program lead in and class header
				sb.Append(Namespaces);
				sb.Append("\r\n");

				if (ScriptingLanguage == "CSharp") 
				{
					// *** Namespace headers and class definition
					sb.Append("namespace " + AssemblyNamespace + "{\r\npublic class " + ClassName + ":MarshalByRefObject {\r\n");	
				
					// *** Generic Invoke method required for the remote call interface
					sb.Append(
						"public object Invoke(string lcMethod,object[] parms)\r\n{ " + 
						"return this.GetType().InvokeMember(lcMethod,BindingFlags.InvokeMethod,null,this,parms );\r\n" +
						"}\r\n\r\n" );

					//*** The actual code to run in the form of a full method definition.
					sb.Append(code);

					sb.Append("\r\n} }");  // Class and namespace closed
				}
				else if (ScriptingLanguage == "VB") 
				{
					// *** Namespace headers and class definition
					sb.Append("Namespace " + AssemblyNamespace + "\r\npublic class " + ClassName + "\r\n");
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

				if (SaveSourceCode)
				{
					SourceCode = sb.ToString();
					//MessageBox.Show(this.cSourceCode);
				}

				if (!CompileAssembly(sb.ToString()) )
					return null;

				object loTemp = CreateInstance();
				if (loTemp == null)
					return null;
			}

			return CallMethod(ObjRef,methodName,parameters);
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
			if (ScriptingLanguage == "CSharp")
				return ExecuteMethod("public object ExecuteCode(params object[] Parameters) \r\n{\r\n" + 
						code + 
						"\r\n}",
						"ExecuteCode",parameters);
			else if (ScriptingLanguage == "VB")
				return ExecuteMethod("public function ExecuteCode(ParamArray Parameters() As Object) as object\r\n" + 
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

			if (AppDomain == null && OutputAssembly == null)
				Parameters.GenerateInMemory = true;
			else if (AppDomain != null && OutputAssembly == null)
			{
				// *** Generate an assembly of the same name as the domain
				OutputAssembly = "wws_" + Guid.NewGuid().ToString() + ".dll";
				Parameters.OutputAssembly = OutputAssembly;
			}
			else {
				  Parameters.OutputAssembly = OutputAssembly;
			}
		
			CompilerResults = Compiler.CompileAssemblyFromSource(Parameters,source);

			if (CompilerResults.Errors.HasErrors) 
			{
				Error = true;

				// *** Create Error String
				ErrorMessage = CompilerResults.Errors.Count.ToString() + " Errors:";
				for (int x=0;x<CompilerResults.Errors.Count;x++) 
					ErrorMessage = ErrorMessage  + "\r\nLine: " + CompilerResults.Errors[x].Line.ToString() + " - " + 
						                               CompilerResults.Errors[x].ErrorText;				
				return false;
			}

			if (AppDomain == null)
				Assembly = CompilerResults.CompiledAssembly;
			
			return true;
		}

		public object CreateInstance() 
		{
			if (ObjRef != null) 
			{
				return ObjRef;
			}
			
			// *** Create an instance of the new object
			try 
			{
				if (AppDomain == null)
					try 
					{
						ObjRef =  Assembly.CreateInstance(AssemblyNamespace + "." + ClassName);
						return ObjRef;
					}
					catch(Exception ex) 
					{
						Error = true;
						ErrorMessage = ex.Message;
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
				Error = true;
				ErrorMessage = ex.Message;
				return null;
			}
				
		}

		public object CallMethod(object loObject,string lcMethod, params object[] loParameters) 
		{
			// *** Try to run it
			try 
			{
				if (AppDomain == null)
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
				Error = true;
				ErrorMessage = ex.Message;
			}
			return null;
		}

		public bool CreateAppDomain(string lcAppDomain) 
		{
			if (lcAppDomain == null)
				lcAppDomain = "wwscript";

			AppDomainSetup loSetup = new AppDomainSetup();
			loSetup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;

			AppDomain = AppDomain.CreateDomain(lcAppDomain,null,loSetup);
			return true;
		}

		public bool UnloadAppDomain()
		{
			if (AppDomain != null)
			   AppDomain.Unload(AppDomain);

			AppDomain = null;

			if (OutputAssembly != null) 
			{
				try 
				{
					File.Delete(OutputAssembly);
				}
				catch(Exception) {;}
			}

			return true;
		}
		public void Release() 
		{
			ObjRef = null;
		}

		public void Dispose() 
		{
			Release();
			UnloadAppDomain();
		}

		~wwScripting() 
		{
			Dispose();
		}
	}


}
