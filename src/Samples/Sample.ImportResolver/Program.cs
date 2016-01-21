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
            host.DefaultScope.ExecuteExpression(""
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
        public Guid UniqueResolverId
        {
            get
            {
                return Guid.Parse("124ec66d-ffc7-48c3-a9d5-bc15de97b540");
            }
        }

        public string GetScriptSource(string path)
        {
            return GetEmbedded("Sample.ImportResolver." + path.Replace("/", "."));
        }

        public string GetEmbedded(string module)
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
