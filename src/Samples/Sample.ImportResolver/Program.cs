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

            // Add some resolver for embedded scripts/modules
            host.AddImportResolver(new EmbeddedModuleResolver());

            // Import some classes and use them
            host.DefaultScope.Execute(""
                //+ "import sys" + "\r\n"
                + "import Sample.ImportResolver.Math" + "\r\n" // <- Import which will be resolved by using Simplic.Dlr resolving
                //+ "from System import Console" + "\r\n"
                //+ "from System.IO import File" + "\r\n"
                + "" + "\r\n"
                + "" + "\r\n"
                //+ "Console.WriteLine('Hello World')" + "\r\n"
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
        public ScriptSource[] GetScripts(ScriptEngine engine)
        {
            return new ScriptSource[] { };
        }

        public ScriptSource GetScriptSource(string moudleName, ScriptEngine engine)
        {
            var module = GetEmbedded(moudleName + ".__init__.py");

            if (module != null)
            {
                return engine.CreateScriptSourceFromString(module, Microsoft.Scripting.SourceCodeKind.AutoDetect);
            }

            var script = GetEmbedded(moudleName + ".py");

            if (script != null)
            {
                return engine.CreateScriptSourceFromString(module, Microsoft.Scripting.SourceCodeKind.Expression);
            }

            return null;
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
