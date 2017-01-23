using IronPython.Runtime;
using Simplic.Dlr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Debugging
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                // Create host
                var language = new IronPythonLanguage();
                language.Argv = args.ToList();

                var host = new DlrHost<IronPythonLanguage>(language);

                host.AddSearchPath(@"C:\Program Files (x86)\IronPython 2.7\Lib\");
                host.AddSearchPath(System.IO.Path.GetDirectoryName(args[0]));
                
                try
                {
                    host.DefaultScope.ExecuteScript(args.First());
                }
                catch (Exception ex)
                {
                    Environment.Exit(1);
                }
            }
        }
    }
}
