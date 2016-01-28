using Simplic.Dlr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting.Hosting;
using System.Reflection;
using System.IO;

namespace Sample.ImportResolver
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Create simple host environment
            var host = new DlrHost<IronPythonLanguage>(new IronPythonLanguage());

            host.AddSearchPath(@"C:\Program Files (x86)\IronPython 2.7\Lib\");

            // Add some resolver for embedded scripts/modules
            host.AddImportResolver(new EmbeddedModuleResolver());

            // Execute script by path
            var val = host.DefaultScope.ExecuteScript("FileSample/FileSample.py");
            Console.WriteLine("Script value: " + val.ToString());

            // Import some classes and use them
            host.DefaultScope.Execute(""
                //+ "import sys" + "\r\n"
                + "import Math.MathImpl" + "\r\n"
                //+ "from System import Console" + "\r\n"
                + "" + "\r\n"
                + "" + "\r\n"
                //+ "Console.WriteLine(str(Math.add(1, 2)))" + "\r\n"
                + "" + "\r\n"
                + "" + "\r\n"
                + "" + "\r\n");

            Console.ReadLine();
        }
    }

    /// <summary>
    /// Resolver definitions to look for modules which gets compiled into some .net assembly.
    /// Custom resolvers can be writte to load scripts from databases or other sources
    /// </summary>
    public class EmbeddedModuleResolver : IDlrImportResolver
    {
        /// <summary>
        /// Get the script source. If the script was not found, null has to be returned
        /// </summary>
        /// <param name="path">Path to the script</param>
        /// <returns>Null if no code was found, else the script code</returns>
        public string GetScriptSource(string path)
        {
            return GetEmbedded("Sample.ImportResolver." + path.Replace("/", "."));
        }

        /// <summary>
        /// Resolve the type of the path, for example this may contains some package, module or nothing
        /// </summary>
        /// <param name="path">Path to the package, module</param>
        /// <returns>Type of the destination</returns>
        public ResolvedType GetModuleInformation(string path)
        {
            string dottedPath = path.Replace("/", ".");

            if (path.EndsWith(".py"))
            {
                if (GetEmbedded("Sample.ImportResolver." + dottedPath) != null)
                {
                    return ResolvedType.Module;
                }
            }
            else
            {
                if (GetEmbedded("Sample.ImportResolver." + dottedPath + ".__init__.py") != null)
                {
                    return ResolvedType.Package;
                }
                else if(GetEmbedded("Sample.ImportResolver." + dottedPath + ".py") != null)
                {
                    return ResolvedType.Module;
                }
            }

            return ResolvedType.None;
        }

        private string GetEmbedded(string module)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = module;

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
