# Simplic.Dlr

Simplic.Dlr is a library to use the Microsoft Dlr in a very simple and efficient way, without any lost of flexibility.
The library provides the following functions:

* Very easy usage of the Microsoft Dlr and IronPython without loosing flexibility
* Integrated Dlr-Class to interact between C# and Python class
  * Easily write wrappers of IronPython classes to use them as a .Net class
* Easily write your own script import resolver, to load scripts and even package from the database or any other source

## Installation

### Compile

Just clone the current repository and open the *Simplic.Dlr* solution in the *src* directory. After compiling 
just copy all needed assemblies (Simplic.Dlr, IronPython.dll, Microsoft.Scripting, ...).

### Nuget

You can find the newest and stable version at nuget: [Simplic.Dlr](https://www.nuget.org/packages/Simplic.Dlr/)

## Samples

A list of samples can be found in the `src/Samples` directory of the repository.

## Getting started:

1. Install `Simplic.Dlr` by compiling on your own or using nuget.

#### Initialie Simplic.Dlr

To use Simplic.Dlr you always need to create a `DlrHost`. A `DlrHost` will always be initialized with a specific language:

    var host = new DlrHost<IronPythonLanguage>(new IronPythonLanguage());
    
Tha's all you need to initialize the following component:

1. ScriptEngine
2. ScriptRuntime
3. Default scope

#### Execute IronPython code

To execute a line of IronPython code just use default scope and execute the script directly:

```
host.DefaultScope.Execute("class TestClass(object):\r\n"
  + "    var1 = ''\r\n"
  + "    def doPrint(self, txt):\r\n"
  + "        print txt\r\n"
  + ""
  + "    def doPrintVar(self):\r\n"
  + "        print self.var1\r\n");
```
