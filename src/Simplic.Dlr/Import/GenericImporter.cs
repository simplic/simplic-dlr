using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
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

        /// <summary>
        /// Generic importer, bases on PEP 302 and the IronPython zipimporter module
        /// </summary>
        public class genericimporter
        {
            [SpecialName]
            public static void PerformModuleReload(PythonContext context, PythonDictionary dict)
            {
                InitModuleExceptions(context, dict);
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

                Console.WriteLine("    Path: " + pathObj.ToString());

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

            #region [__repr__]
            /// <summary>
            /// OBject representation
            /// </summary>
            /// <returns>__repr__ as string</returns>
            public string __repr__()
            {
                if (resolver == null)
                {
                    return "<genericimporter object \"Invalid-Resolver\">";
                }

                return "<genericimporter object \"" + resolver.UniqueResolverId.ToString() + "\">";
            }
            #endregion

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

            #region [load_module]
            public object load_module(CodeContext/*!*/ context, string fullname)
            {
                fullname = fullname.Replace("<module>.", lastPath + ".").Replace(".", "/");

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

                        Console.WriteLine("     IMPORT: " + modpath);
                        
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
                dict.Add("__name__", fullname.Split(new char[] { '/' }).Last());
                dict.Add("__loader__", this);
                dict.Add("__package__", null);
                
                if (ispackage)
                {
                    // Add path
                    string fullpath = fullname.Replace(".", "/");

                    List pkgpath = PythonOps.MakeList("resolver:" + resolver.UniqueResolverId + "|" + fullpath);
                    dict.Add("__path__", pkgpath);
                }
                else
                {
                    StringBuilder packageName = new StringBuilder();
                    string[] packageParts = fullname.Split(new char[] { '/' });
                    for (int i = 0; i < packageParts.Length - 1; i++)
                    {
                        if (i > 0)
                        {
                            packageName.Append(".");
                        }

                        packageName.Append(packageParts[i]);
                    }

                    dict.Add("__package__", packageName.ToString());
                }

                var scope = context.ModuleContext.GlobalScope;
                scriptCode.Run(scope);

                return mod;
            }
            #endregion

            public string get_filename(CodeContext context, string fullname)
            {
                return null;
            }

            /// <summary>
            /// Return True if the module specified by 'fullname' is a package and False if it isn't.
            /// </summary>
            /// <param name="context">CodeContext - Automatically passed by the IP-Core</param>
            /// <param name="fullname">Full path to the module</param>
            /// <returns>True if the module is a package</returns>
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

            #region [GenericImporterException]
            public static PythonType GenericImporterError;

            /// <summary>
            /// Create new throwable exception
            /// </summary>
            /// <param name="args">Exception parameter</param>
            /// <returns>Exception instance</returns>
            internal static Exception MakeError(params object[] args)
            {
                return PythonOps.CreateThrowable(GenericImporterError, args);
            }

            /// <summary>
            /// Initialize exception
            /// </summary>
            /// <param name="context"></param>
            /// <param name="dict"></param>
            private static void InitModuleExceptions(PythonContext context, PythonDictionary dict)
            {
                GenericImporterError = context.EnsureModuleException(
                    "genericimporter.GenericImporterError",
                    PythonExceptions.ImportError,
                    typeof(PythonExceptions.BaseException),
                    dict, "GenericImporterError", "genericimporter",
                    msg => new ImportException(msg));
            }
            #endregion
        }
    }
}