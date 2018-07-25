using Simplic.Dlr;
using System;
using System.IO;

namespace Sample.StdLib
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                IronPythonLanguage ironpython = new IronPythonLanguage();
                var host = new DlrHost<IronPythonLanguage>(ironpython);
                host.AddSearchPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Lib"));
                host.ScriptEngine.Runtime.IO.RedirectToConsole();

                host.DefaultScope.ExecuteScript("Scripts/TestScript.py");

                Console.ReadLine();
            }
            catch (Exception e)
            {
            }
        }
    }
}
