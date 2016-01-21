using IronPython.Runtime;
using IronPython.Runtime.Operations;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Simplic.Dlr
{
    public static class GenericImportModule
    {
        /// <summary>
        /// IronPython language context.
        /// </summary>
        public static DlrHost<IronPythonLanguage> Host
        {
            get;
            internal set;
        }

        public class genericimporter
        {
            [SpecialName]
            public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
            {

            }

            #region Private Member
            private IDlrImportResolver resolver;
            #endregion

            #region [Static]
            /// <summary>
            /// zip_searchorder defines how we search for a module in the Zip
            /// archive: we first search for a package __init__, then for
            /// non-package .pyc, .pyo and .py entries. The .pyc and .pyo entries
            /// are swapped by initzipimport() if we run in optimized mode. Also,
            /// '/' is replaced by SEP there.
            /// </summary>
            private static readonly Dictionary<string, GenericModuleCodeType> _search_order;

            #region [Constructor]
            /// <summary>
            /// Create generic importer
            /// </summary>
            static genericimporter()
            {
                // Create search order:
                //   1. /__init__.py
                //   2. .py
                _search_order = new Dictionary<string, GenericModuleCodeType>()
                    {
                            { "/__init__.py", GenericModuleCodeType.Package | GenericModuleCodeType.Source },
                            { ".py", GenericModuleCodeType.Source }
                    };
            }
            #endregion

            #endregion

            public genericimporter()
            {

            }

            public genericimporter(CodeContext/*!*/ context, object pathObj, [Microsoft.Scripting.ParamDictionary] IDictionary<object, object> kwArgs)
            {
                // Can only be used, if a host is set
                if (Host == null || Host.Resolver.Count == 0)
                {
                    throw new Exception("No generic importer is registered");
                }

                if (pathObj == null)
                {
                    throw PythonOps.TypeError("must be string, not None");
                }

                if (!(pathObj is string))
                {
                    throw PythonOps.TypeError("must be string, not {0}", pathObj.GetType());
                }

                if (kwArgs.Count > 0)
                {
                    throw PythonOps.TypeError("genericimporter() does not take keyword arguments");
                }

                string path = pathObj.ToString();
                Guid resolverId = Guid.Empty;

                // Try to find resolver
                if (path.StartsWith("resolver:"))
                {
                    path = path.Replace("resolver:", "");

                    int pipeIndex = path.IndexOf("|");

                    if (pipeIndex > 0)
                    {
                        path = path.Substring(0, pipeIndex);

                        if (Guid.TryParse(path, out resolverId))
                        {
                            lastPath = pathObj.ToString().Replace("resolver:", "");
                            lastPath = lastPath.Remove(0, Guid.Empty.ToString().Length + 1);

                            resolver = Host.Resolver.Where(item => item.UniqueResolverId == resolverId).FirstOrDefault();
                        }
                    }
                }
            }

            private static string lastPath = "";

            public string __repr__()
            {
                return "";
            }

            #region [find_module]
            /// <summary>
            /// Find module for importing. If a resolver is set, the current module should be used
            /// </summary>
            /// <param name="context"></param>
            /// <param name="fullname"></param>
            /// <param name="args"></param>
            /// <returns>If resolver is not null, this will be returned, else null</returns>
            public object find_module(CodeContext/*!*/ context, string fullname, params object[] args)
            {
                if (resolver == null)
                {
                    return null;
                }

                return this;
            }
            #endregion

            public object load_module(CodeContext/*!*/ context, string fullname)
            {
                fullname = fullname.Replace("<module>.", lastPath);

                string code = null;
                GenericModuleCodeType moduleType;
                bool ispackage = false;
                string modpath = null;
                PythonModule mod;
                PythonDictionary dict = null;

                // Go through available import types by search-order
                foreach (var order in _search_order)
                {
                    string tempCode = resolver.GetScriptSource(fullname + order.Key);

                    if (tempCode != null)
                    {
                        moduleType = order.Value;
                        code = tempCode;
                        modpath = fullname + order.Key;

                        if ((order.Value & GenericModuleCodeType.Package) == GenericModuleCodeType.Package)
                        {
                            ispackage = true;
                        }

                        break;
                    }
                }

                // of no code was loaded
                if (code == null)
                {
                    return null;
                }

                var scriptCode = context.ModuleContext.Context.CompileSourceCode
                    (
                        new SourceUnit(context.LanguageContext, new SourceStringContentProvider(code), modpath, SourceCodeKind.AutoDetect),
                        new IronPython.Compiler.PythonCompilerOptions() { },
                        ErrorSink.Default
                    );

                // initialize module
                mod = context.ModuleContext.Context.InitializeModule(modpath, context.ModuleContext, scriptCode, ModuleOptions.None);

                dict = mod.Get__dict__();

                // we do these here because we don't want CompileModule to initialize the module until we've set 
                // up some additional stuff
                dict.Add("__name__", fullname);
                dict.Add("__loader__", this);
                dict.Add("__package__", null);

                if (ispackage)
                {
                    // Add path
                    string fullpath = fullname.Replace(".", "/");

                    List pkgpath = PythonOps.MakeList("resolver:" + resolver.UniqueResolverId + "|" + fullpath);
                    dict.Add("__path__", pkgpath);
                }

                var scope = context.ModuleContext.GlobalScope;
                scriptCode.Run(scope);

                return mod;
            }

            public string get_filename(CodeContext context, string fullname)
            {
                return null;
            }

            public bool is_package(CodeContext context, string fullname)
            {
                return true;
            }

            public string get_data(CodeContext context, string path)
            {
                return null;
            }

            public string get_code(CodeContext context, string fullname)
            {
                return "";
            }

            public string get_source(CodeContext context, string fullname)
            {
                return null;
            }
        }
    }
}