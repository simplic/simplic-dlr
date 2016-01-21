using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting.Hosting;
using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Types;

namespace Simplic.Dlr
{
    /// <summary>
    /// Implementation of the IronPython language to use in the simplic dlr implementation
    /// </summary>
    public class IronPythonLanguage : IDlrLanguage
    {
        #region [Const]
        /// <summary>
        /// Character for package separation
        /// </summary>
        public const char PACKAGE_SEPARATOR = '/';

        /// <summary>
        /// Default extension of a python file
        /// </summary>
        public const string PYTHON_FILE_EXTENSION = ".py";

        /// <summary>
        /// File which defines a package
        /// </summary>
        public const string PACKAGE_DEFINITION_FILE = "__init__" + PYTHON_FILE_EXTENSION;
        #endregion

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
        private string absolutePath;
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
            loadedAssemblyCount = 0;
            absolutePath = "";

            EnableZipImporter = false;

            // Default built in modules
            buildInModules.Add("sys");
            buildInModules.Add("clr");
            buildInModules.Add("wpf");
            buildInModules.Add("random");
            buildInModules.Add("md5");
            buildInModules.Add("ssl");
            buildInModules.Add("array");
        }
        #endregion

        #region Private Methods

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

            var sysScope = scriptEngine.GetSysModule();
            List path_hooks = sysScope.GetVariable("path_hooks");

            // Disable zipimporter if needed
            for (int i = 0; i < path_hooks.Count; i++)
            {
                if (path_hooks.ElementAt(i) != null)
                {
                    PythonType type = path_hooks.ElementAt(i) as PythonType;
                    string name = PythonType.Get__name__(type);

                    if (name == "zipimporter" && EnableZipImporter == false)
                    {
                        path_hooks.RemoveAt(i);
                    }
                    
                }
            }

            PythonType genericimporter = DynamicHelpers.GetPythonType(new GenericImportModule.genericimporter());
            path_hooks.Add(genericimporter);

            sysScope.SetVariable("path_hooks", path_hooks);
            
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
        /// Enable or disable zip-importer. By default it is disabled
        /// </summary>
        public bool EnableZipImporter
        {
            get;
            set;
        }
        #endregion
    }
}
