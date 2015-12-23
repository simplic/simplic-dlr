# Simplic.Dlr

Simplic.Dlr is a library to use the Microsoft Dlr in a very simple and efficient way, without loosing flexibility.
The library provides the following functions:

* Very easy usage of the Microsoft Dlr and IronPython without loosing flexibility: `DlrHost`
* Integrated Dlr-Class to interact between C# and Python class: `DlrClass`
  * Easily write wrappers of IronPython classes to use them as a .Net class
* Access dlr variables easily over the integrated `DlrScriptScope`
* Easily write your own script import resolver, to load scripts and even package from the database or any other source: `IDlrImportResolver` [Not implemented yet, but very soon]
* Precompile code for faster usage [Not implemented yet, but very soon]
* Cache scripts/statements for faster execution

## Installation

### Compile

Just clone the current repository and open the *Simplic.Dlr* solution in the *src* directory. After compiling 
just copy all needed assemblies (Simplic.Dlr, IronPython.dll, Microsoft.Scripting, ...).

### Nuget

You can find the newest and stable version at nuget: [![NuGet Status](http://img.shields.io/nuget/v/Simplic.Dlr.svg?style=flat)](https://www.nuget.org/packages/Simplic.Dlr/)

## Samples

A list of samples can be found in the `src/Samples` directory of the repository.

## Getting started:

Install `Simplic.Dlr` by compiling on your own or using [nuget](https://www.nuget.org/packages/Simplic.Dlr/).

### Initialize Simplic.Dlr

To use Simplic.Dlr you always need to create a `DlrHost`. A `DlrHost` will always be initialized with a specific language:

    var host = new DlrHost<IronPythonLanguage>(new IronPythonLanguage());
    
Tha's all you need to initialize the following component:

1. ScriptEngine
2. ScriptRuntime
3. Default scope

### Execute IronPython code

To execute a line of IronPython code just use default scope and execute the script directly:

```csharp
host.DefaultScope.Execute("print 'Hello World'");
```

### Set search paths for modules

The host provides methods to add and remove search paths very easily

*Add search path to the host*

```csharp
host.AddSearchPath("C:\\Python\\lib")
```

*Remove search path from the host*

```csharp
if (host.RemoveSearchPath('C:\\Python\\lib'))
{
    // Removes successfully        
}
```

### Create an instance of an IronPython class in C# and access it's member

```csharp
host.DefaultScope.Execute("class TestClass(object):\r\n"
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
```

### Create a c# wrapper for IronPython classes

```csharp
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
```
