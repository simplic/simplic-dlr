using Simplic.Dlr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sample.Wrapper
{
 /// <summary>
 /// Simple wrapper class
 /// </summary>
 public class MathClass : DlrClass
 {
     /// <summary>
     /// Create math class
     /// </summary>
     /// <param name="scriptScope"></param>
     /// <param name="className"></param>
     /// <param name="parameter"></param>
     internal MathClass(DlrScriptScope scriptScope, params object[] parameter)
         : base(scriptScope, "MathClass", parameter)
     {

     }

     /// <summary>
     /// Add data
     /// </summary>
     /// <param name="x">x to add</param>
     /// <param name="y">y to add</param>
     /// <returns>returns x + y</returns>
     public int Add(int x, int y)
     {
         return CallFunction("add", x, y);
     }

     /// <summary>
     /// Subtract data
     /// </summary>
     /// <param name="x">x value</param>
     /// <param name="y">y to substract from x</param>
     /// <returns>returns x - y</returns>
     public int Sub(int x, int y)
     {
         return CallFunction("sub", x, y);
     }
 }

 class Program
 {
     static void Main(string[] args)
     {
         // Create simple host environment
         var host = new DlrHost<IronPythonLanguage>(new IronPythonLanguage());

         // Define class
         host.DefaultScope.Execute("class MathClass(object):\r\n"
             + ""
             + "    def add(self, x, y):\r\n"
             + "        return x + y\r\n"
             + ""
             + "    def sub(self, x, y):\r\n"
             + "        return x - y\r\n");

         // Use Math-class
         var cl = new MathClass(host.DefaultScope);

         // Call c# -> python
         Console.WriteLine("Add: 5 + 100 = " + cl.Add(5, 100));
         Console.WriteLine("Sub: 90 - 14 = " + cl.Sub(90, 40));

         Console.ReadLine();
     }
 }
}
