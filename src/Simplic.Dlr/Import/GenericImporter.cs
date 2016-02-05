using IronPython.Runtime;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Types;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Simplic.Dlr
{
    public static class GenericImportModule
    {
        #region Const
        /// <summary>
        /// Name which will be used in resolver
        /// </summary>
        public const string RESOLVER_PATH_NAME = "<simplic-dlr-resolver>";
        #endregion

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
            private string _rel_path;
            private string _prefix;
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
                PlatformAdaptationLayer pal = context.LanguageContext.DomainManager.Platform;
                
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

                if (string.IsNullOrWhiteSpace(path))
                {
                    throw new Exception("Could not resolve empty, whitespace or null path");
                }

                // Only use for resolvable paths
                if (!path.StartsWith(GenericImportModule.RESOLVER_PATH_NAME))
                {
                    throw new ImportException("Not a generic source path.");
                }

                string buf = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                string input = buf;

                _prefix = input.Replace(path, string.Empty);
                // add trailing SEP
                if (!string.IsNullOrEmpty(_prefix) && !_prefix.EndsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    _prefix = _prefix.Substring(1);
                    _prefix += Path.DirectorySeparatorChar;
                }
            }

            #region [__repr__]
            /// <summary>
            /// OBject representation
            /// </summary>
            /// <returns>__repr__ as string</returns>
            public string __repr__()
            {
                return "<genericimporter object \"" + this.GetType().ToString() + "\">";
            }
            #endregion

            #region [MakePath]
            /// <summary>
            /// Create complete relative path
            /// </summary>
            /// <param name="fullname"></param>
            /// <returns></returns>
            public string MakeValidPath(string fullname)
            {
                if (string.IsNullOrWhiteSpace(_rel_path))
                {
                    return fullname;
                }
                else if (fullname.StartsWith(_rel_path + "."))
                {
                    return fullname;
                }
                else
                {
                    return _rel_path + "." + fullname;
                }
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
                try
                {
                    fullname = MakeValidPath(fullname);

                    // Find resolver
                    foreach (var resolver in Host.Resolver)
                    {
                        var res = resolver.GetModuleInformation(fullname);

                        // If this script could be resolved by some resolver
                        if (res != ResolvedType.None)
                        {
                            this.resolver = resolver;
                            return this;
                        }
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    throw new ImportException("Error in generic importer.", ex);
                }
            }
            #endregion

            #region [load_module]
            public object load_module(CodeContext/*!*/ context, string fullname)
            {
                fullname = MakeValidPath(fullname);

                string code = null;
                GenericModuleCodeType moduleType;
                bool ispackage = false;
                string modpath = null;
                string fullFileName = null;
                PythonModule mod;
                PythonDictionary dict = null;

                // Go through available import types by search-order
                foreach (var order in _search_order)
                {
                    string tempCode = this.resolver.GetScriptSource(fullname + order.Key);

                    if (tempCode != null)
                    {
                        moduleType = order.Value;
                        code = tempCode;
                        modpath = fullname;
                        fullFileName = fullname.Replace(".", "/") + order.Key;

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

                ScriptCode scriptCode = null;

                mod = context.LanguageContext.CompileModule(fullFileName, fullname,
                    new SourceUnit(context.LanguageContext, new SourceStringContentProvider(code), modpath, SourceCodeKind.File),
                    ModuleOptions.None, out scriptCode);

                dict = mod.Get__dict__();

                // Set values before execute script
                dict.Add("__name__", fullname.Split('.').Last());
                dict.Add("__loader__", this);
                dict.Add("__package__", null);

                if (ispackage)
                {
                    // Add path
                    string fullpath = string.Format(fullname.Replace("/", "."));
                    _rel_path = fullpath;
                    List pkgpath = PythonOps.MakeList(fullpath);

                    if (dict.ContainsKey("__path__"))
                    {
                        dict["__path__"] = pkgpath;
                    }
                    else
                    {
                        dict.Add("__path__", pkgpath);
                    }
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

                    dict["__package__"] = packageName.ToString();
                }

                scriptCode.Run(mod.Scope);
                return mod;
            }
            #endregion

            public string get_filename(CodeContext context, string fullname)
            {
                return null;
            }

            #region [is_package]
            /// <summary>
            /// Return True if the module specified by 'fullname' is a package and False if it isn't.
            /// </summary>
            /// <param name="context">CodeContext - Automatically passed by the IP-Core</param>
            /// <param name="fullname">Full path to the module</param>
            /// <returns>True if the module is a package</returns>
            public bool is_package(CodeContext context, string fullname)
            {
                foreach (var resolver in Host.Resolver)
                {
                    var res = resolver.GetModuleInformation(fullname);

                    // If this script could be resolved by some resolver
                    if (res == ResolvedType.Package)
                    {
                        return true;
                    }
                }

                return false;
            }
            #endregion

            #region [get_data]
            public string get_data(CodeContext context, string path)
            {
                return null;
            }
            #endregion

            #region [get_code/get_source]
            public string get_code(CodeContext context, string fullname)
            {
                return "";
            }

            /// <summary>
            /// Get source ccode
            /// </summary>
            /// <param name="context"></param>
            /// <param name="fullname"></param>
            /// <returns></returns>
            public string get_source(CodeContext context, string fullname)
            {
                foreach (var resolver in Host.Resolver)
                {
                    var res = resolver.GetModuleInformation(fullname);

                    // If this script could be resolved by some resolver
                    if (res != ResolvedType.None)
                    {
                        return resolver.GetScriptSource(fullname);
                    }
                }

                return null;
            }
            #endregion

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