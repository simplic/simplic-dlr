using Simplic.Dlr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sample.Base
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create simple host environment
            var host = new DlrHost<IronPythonLanguage>(new IronPythonLanguage());

            host.DefaultScope.Execute("import sys\r\n"
                + "class TestClass(object):\r\n"
                + "    var1 = ''\r\n"
                + "    def doPrint(self, txt):\r\n"
                + "        print txt\r\n"
                + ""
                + "    def doPrintVar(self):\r\n"
                + "        print self.var1\r\n");

            // Call via name of the method
            var instance = host.DefaultScope.CreateClassInstance("TestClass");
            instance.CallMethod("doPrint", "Text to print?");

            // Call directly over the embedded dynamic keyword
            dynamic dynInstance = instance;
            dynInstance.doPrint("Text 2 to print!");

            // Set variable an print out
            instance.SetMember("var1", "Variable content 1.");
            instance.CallMethod("doPrintVar");

            dynInstance.var1 = "Variable content 2.";
            dynInstance.doPrintVar();

            // Cache
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            host.DefaultScope.Execute("print 'Hello World'");
            watch.Stop();
            Console.WriteLine("No cached: " + watch.ElapsedMilliseconds.ToString());

            watch.Start();
            host.DefaultScope.Execute("print 'Hello World'");
            watch.Stop();
            Console.WriteLine("Cached: " + watch.ElapsedMilliseconds.ToString());

            Console.ReadLine();
        }
    }
}
