//#define useRemoteLoader 

using System;
using System.IO;
using System.Text;

using Microsoft.CSharp;
using System.Reflection;

using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MarkdownMonster;
using Westwind.Utilities;


namespace Westwind.Scripting
{
	
	/// <summary>
	/// Class that enables running of code dynamically created at runtime.
	/// Provides functionality for evaluating and executing compiled code.
	/// </summary>
    public class ScriptRunnerRoslyn : IDisposable
	{
		/// <summary>
		/// Compiler object used to compile our code
		/// </summary>
		protected CodeDomProvider Compiler = null;

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

        /// <summary>
        /// Flag uus
        /// </summary>
		protected bool FirstLoad = true;

        /// <summary>
        /// Namespaces added to generated code
        /// </summary>
        protected StringBuilder Namespaces { get; } = new StringBuilder(200);


        /// <summary>
        /// The object reference to the compiled object available after the first method call.
        /// You can use this method to call additional methods on the object.
        /// For example, you can use InvokeMethod and pass multiple methods of code each of
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
		public string AssemblyNamespace = "__ScriptRunner_Ns";

		/// <summary>
		/// Name of the class created by the script processor. Script code becomes methods in the class.
		/// </summary>
		public string ClassName = "__ScriptRunner";

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


        public ScriptRunnerRoslyn()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            Compiler = CodeDomProvider.CreateProvider("CSharp");
            SetCompilerServerTimeToLive(Compiler as CSharpCodeProvider, new TimeSpan(0, 20, 0));
#pragma warning restore CS0618 // Type or member is obsolete

            Parameters = new CompilerParameters();
        }

        #region Execution Setup

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
                return;
			}
			
			if (assemblyDll != null)
				Parameters.ReferencedAssemblies.Add(assemblyDll);

            if (nameSpace != null)
                Namespaces.AppendLine("using " + nameSpace + ";");
        }

		/// <summary>
		/// Adds an assembly to the compiled code.
		/// </summary>
		/// <param name="assemblyDll">DLL assembly file name</param>
		public void AddAssembly(string assemblyDll) 
		{
			AddAssembly(assemblyDll,null);
		}

        /// <summary>
        /// Adds an assembly reference from a type
        /// </summary>
        /// <param name="type"></param>
        public void AddAssembly(Type type)
        {
            AddAssembly(type.Assembly.Location);
        }

		public void AddNamespace(string nameSpace)
        {
            if (nameSpace != null)
                Namespaces.AppendLine("using " + nameSpace + ";");
        }
		public void AddDefaultAssemblies()
		{
			AddAssembly("System.dll","System");
		    AddAssembly("System.Core.dll");
		    AddAssembly("Microsoft.CSharp.dll");
			AddNamespace("System.Reflection");
			AddNamespace("System.IO");

		}

        #endregion

        #region Execution

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
					FirstLoad = false;
				}

				var sb = new StringBuilder(300);

				//*** Program lead in and class header
				sb.Append(Namespaces);
				sb.Append("\r\n");

					// *** Namespace headers and class definition
					sb.Append("namespace " + AssemblyNamespace + "{\r\npublic class " + ClassName + " {\r\n");	
				
					// *** Generic Invoke method required for the remote call interface
					sb.Append(
						"public object Invoke(string lcMethod,object[] parms)\r\n{ " + 
						"return this.GetType().InvokeMember(lcMethod,BindingFlags.InvokeMethod,null,this,parms );\r\n" +
						"}\r\n\r\n" );

					//*** The actual code to run in the form of a full method definition.
					sb.Append(code);

					sb.Append("\r\n} }");  // Class and namespace closed
				
				if (SaveSourceCode)
                    SourceCode = sb.ToString();

                if (!CompileAssembly(sb.ToString()) )
					return null;

				object instance = CreateInstance();
				if (instance == null)
					return null;
			}

			return InvokeMethod(ObjRef,methodName,parameters);
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
            return ExecuteMethod("public object ExecuteCode(params object[] Parameters) \r\n{\r\n" +
                                 code +
                                 "\r\n}",
                "ExecuteCode", parameters);
        }

       


        /// <summary>
        /// Invoke a method on an object generically. Used to dynamically invoke
        /// the generated method on the instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="lcMethod"></param>
        /// <param name="loParameters"></param>
        /// <returns></returns>
        public object InvokeMethod(object instance, string lcMethod, params object[] loParameters)
        {
            // *** Try to run it
            try
            {
                if (AppDomain == null)
                    // *** Just invoke the method directly through Reflection
                    return instance.GetType().InvokeMember(lcMethod, BindingFlags.InvokeMethod, null, instance, loParameters);
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


        /// <summary>
        /// Invokes a method on the previously generated and compiled instance
        /// stored in ObjRef.
        /// </summary>
        /// <param name="lcMethod"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public object InvokeMethod(string lcMethod, params object[] parameters)
        {
            return InvokeMethod(ObjRef, lcMethod, parameters);
        }

        #endregion

        #region Compilation

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



        /// <summary>
        /// Force the compiler to live for longer than 10 seconds which is the default for Roslyn
        /// </summary>
        private static void SetCompilerServerTimeToLive(CSharpCodeProvider codeProvider, TimeSpan timeToLive)
        {
            const BindingFlags privateField = BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance;

            var compilerSettingField = codeProvider.GetType().GetField("_compilerSettings", privateField);
            var compilerSettings = compilerSettingField.GetValue(codeProvider);

            var timeToLiveField = compilerSettings.GetType().GetField("_compilerServerTimeToLive", privateField);
            timeToLiveField.SetValue(compilerSettings, (int)timeToLive.TotalSeconds);
        }

        #endregion

        #region App Domains

#if NETFULL
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
#endif

        #endregion


        public void Release() 
		{
			ObjRef = null;
            Compiler.Dispose();
        }

		public void Dispose() 
		{
			Release();

#if NETFULL
            UnloadAppDomain();
#endif
        }

        ~ScriptRunnerRoslyn() 
		{
			Dispose();
		}


        #region Warmup and ShutDown
        /// <summary>
        /// Run a script execution asynchronously in the background to warm up Roslyn.
        /// Call this during application startup or anytime before you run the first
        /// script to ensure scripts execute quickly.
        /// </summary>
        public static void WarmupRoslyn()
        {
            // warm up Roslyn
            Task.Run(() =>
            {
                using (var script = new ScriptRunnerRoslyn())
                {
                    script.ExecuteCode("int x = 1; return x;", null);
                }
            });
        }

        /// <summary>
        /// Call this method to shut down the VBCSCompiler if our
        /// application started it.
        /// </summary>
        public static void ShutdownRoslyn()
        {
            var processes = Process.GetProcessesByName("VBCSCompiler");
            if (processes != null)
            {
                foreach (var process in processes)
                {
                    // only shut down 'our' VBCSCompiler
                    var fn = GetMainModuleFileName(process);
                    if (fn.Contains(App.InitialStartDirectory, StringComparison.InvariantCultureIgnoreCase))
                        LanguageUtils.IgnoreErrors(() => process.Kill());
                }
            }
        }


        [DllImport("Kernel32.dll")]
        private static extern bool QueryFullProcessImageName(
            [In] IntPtr hProcess,
            [In] uint dwFlags,
            [Out] StringBuilder lpExeName,
            [In, Out] ref uint lpdwSize);

        public static string GetMainModuleFileName(Process process)
        {
            var fileNameBuilder = new StringBuilder(1024);
            uint bufferLength = (uint)fileNameBuilder.Capacity + 1;
            return QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength)
                ? fileNameBuilder.ToString()
                : null;
        }
#endregion
    }


}
