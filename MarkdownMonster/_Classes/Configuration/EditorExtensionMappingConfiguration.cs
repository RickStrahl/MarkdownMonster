using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownMonster
{
	public class EditorExtensionMappingConfiguration
	{
		public Dictionary<string, string> Extensions = new Dictionary<string, string>
		{
			{"md", "markdown" },
			{"cs", "csharp"},
			{"js","javascript" },
			{"ts", "typescript" },
			{"css", "css" },
			{"less", "less" },
			{"sass", "sass" },
			{"sql", "sqlserver" },
			{"prg", "foxpro" },
			{"vb", "vb" },
			{"py", "python" },
			{"c", "c_cpp" },
			{"cpp", "c_cpp" },
			{"ps1", "powershell" },
			{"ini", "ini" },
			{"sh", "bash" },
			{"bat", "batchfile" },
			{"cmd", "batchfile" },
			
			{"html", "html" },
			{"asp", "html" },
			{"aspx", "html" },
			{ "jsx", "jsx" },
			{"php", "php" },
			{"go", "golang" },
			{"cshtml", "razor" },
			{"r","r" },
			{"mak","makefile" },
			
			{"xml", "xml" },
			{"xaml", "xml" },
			{"wsdl", "xml" },
			{"config", "xml" },
			{ "csproj", "xml" },
			{ "nuspec", "xml" },

			{ "yaml", "yaml" },
			{"txt","text" },
			{"log", "text" }
		};
	}
}
