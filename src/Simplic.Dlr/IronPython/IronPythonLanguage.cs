using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting.Hosting;
using IronPython.Hosting;
using IronPython.Runtime;

namespace Simplic.Dlr
{
    /// <summary>
    /// Implementation of the IronPython language to use in the simplic dlr implementation
    /// </summary>
    public class IronPythonLanguage : IDlrLanguage
    {
        #region Events & Delegates
        /// <summary>
        /// Import delegate to overwrite IP import-system
        /// </summary>
        /// <param name="context">Script context</param>
        /// <param name="moduleName">name of the module</param>
        /// <param name="globals">Global settings</param>
        /// <param name="locals">Local settings</param>
        /// <param name="tuple"></param>
        /// <returns></returns>
        delegate object ImportDelegate(CodeContext context, string moduleName, PythonDictionary globals, PythonDictionary locals, PythonTuple fromlist);
        #endregion

        #region Private Member
        private ScriptEngine scriptEngine;
        private HashSet<string> buildInModules;
        private HashSet<string> clrModules;
        private HashSet<string> loadedAssemblies;
        private int loadedAssemblyCount;
        private IList<IDlrImportResolver> resolver;
        string absolutePath;
        #endregion

        #region Constructor
        /// <summary>
        /// Create IronPythonLanguage instance for using in a DlrHost
        /// </summary>
        public IronPythonLanguage()
        {
            // Create language
            buildInModules = new HashSet<string>();
            clrModules = new HashSet<string>();
            loadedAssemblies = new HashSet<string>();
            resolver = new List<IDlrImportResolver>();
            loadedAssemblyCount = 0;
            absolutePath = "";

            // Default built in modules
            buildInModules.Add("sys");
            buildInModules.Add("clr");
            buildInModules.Add("wpf");
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// This method will be called, if imports should be resolved
        /// </summary>
        /// <param name="context"></param>
        /// <param name="moduleName"></param>
        /// <param name="globals"></param>
        /// <param name="locals"></param>
        /// <param name="fromlist"></param>
        /// <returns></returns>
        private object ResolveImports(CodeContext context, string moduleName, PythonDictionary globals, PythonDictionary locals, PythonTuple fromlist)
        {
            #region [Import .Net Namespaces]
            // Add not loaded assemblies to the import list
            if (loadedAssemblyCount != AppDomain.CurrentDomain.GetAssemblies().Length)
            {
                loadedAssemblyCount = AppDomain.CurrentDomain.GetAssemblies().Length;

                foreach (System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (!loadedAssemblies.Contains(asm.FullName))
                    {
                        // add as loaded assembly for later usage
                        loadedAssemblies.Add(asm.FullName);

                        // add all types in an assembly as clr module
                        foreach (var type in asm.GetTypes())
                        {
                            if (!clrModules.Contains(type.Namespace))
                            {
                                clrModules.Add(type.Namespace);
                            }
                        }
                    }
                }
            }

            // Check if is build in module or clr module (type in assembly)
            if (buildInModules.Contains(moduleName) || clrModules.Contains(moduleName))
            {
                return IronPython.Modules.Builtin.__import__(context, moduleName, globals, locals, fromlist, -1);
            }
            #endregion

            // Import python namespaces using the custom script resolving system

            // Split module name, because sub.modules also needs to be imported
            string[] modulePath = moduleName.Split(new char[] { '.' });

            foreach (var mp in modulePath)
            {
                // Generate the absolte path, to know where to import from
                if (!string.IsNullOrWhiteSpace(absolutePath))
                {
                    absolutePath += ".";
                }
                absolutePath += mp;

                // Try to import from current absolt path
                Console.WriteLine("Import from" + absolutePath);                
            }


            var mod = context.ModuleContext.Module;

            ScriptSource source = null;

            foreach (var res in resolver)
            {
                source = res.GetScriptSource(moduleName, scriptEngine);

                if (source != null)
                {
                    break;
                }
            }

            // Module was found by an external resolver
            if (source != null)
            {
                CompiledCode compiled = source.Compile();

                ScriptScope scope = scriptEngine.CreateScope();
                compiled.Execute(scope);
                Microsoft.Scripting.Runtime.Scope ret = Microsoft.Scripting.Hosting.Providers.HostingHelpers.GetScope(scope);

                return ret;
            }

            // In case that no rule could resolve the import, let's try IronPython to resolve it on his own
            return IronPython.Modules.Builtin.__import__(context, moduleName, globals, locals, fromlist, -1);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Create new scripting engine
        /// </summary>
        /// <param name="runtime">Instance of the current runtime enviroement</param>
        /// <returns>Instance of IP script engine</returns>
        public ScriptEngine CreateEngine(ScriptRuntime runtime)
        {
            scriptEngine = runtime.GetEngineByTypeName(typeof(PythonContext).AssemblyQualifiedName);

            return scriptEngine;
        }

        /// <summary>
        /// Create new runtime setup
        /// </summary>
        /// <returns></returns>
        public ScriptRuntimeSetup CreateRuntime()
        {
            var runtimeSetup = Python.CreateRuntimeSetup(null);
            runtimeSetup.DebugMode = false;
            runtimeSetup.Options["Frames"] = true;
            runtimeSetup.Options["FullFrames"] = true;

            return runtimeSetup;
        }

        /// <summary>
        /// Initialize the system for resolving script imports
        /// </summary>
        public void InitializeResolver()
        {
            ScriptScope scope = IronPython.Hosting.Python.GetBuiltinModule(scriptEngine);
            scope.SetVariable("__import__", new ImportDelegate(this.ResolveImports));
        }

        /// <summary>
        /// Add Build-In module
        /// </summary>
        /// <param name="module">Module name, like sys or clr, ...</param>
        public void AddBuildInModule(string module)
        {
            if (!buildInModules.Contains(module))
            {
                buildInModules.Add(module);
            }
        }
        #endregion

        #region Public Member
        /// <summary>
        /// Import resolver list
        /// </summary>
        public IList<IDlrImportResolver> Resolver
        {
            get
            {
                return resolver;
            }
        }
        #endregion
    }
}
