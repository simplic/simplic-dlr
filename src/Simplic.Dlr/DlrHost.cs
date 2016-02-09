using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Simplic.Dlr
{
    /// <summary>
    /// Host environment for the dynamic language runtime
    /// </summary>
    public class DlrHost<T> : IDlrHost where T : IDlrLanguage
    {
        #region Private Member
        private ScriptEngine scriptEngine;
        private ScriptRuntimeSetup runtimeSetup;
        private ScriptRuntime runtime;
        private IList<IDlrImportResolver> resolver;
        private T language;

        private HashSet<string> loadedAssemblies;

        private DlrScriptScope defaultScope;
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new dlr host environment
        /// </summary>
        public DlrHost(T language)
        {
            this.language = language;
            loadedAssemblies = new HashSet<string>();
            resolver = new List<IDlrImportResolver>();

            // Create runtime
            this.runtimeSetup = language.CreateRuntime();
            this.runtime = new ScriptRuntime(this.runtimeSetup);

            // Create engine
            scriptEngine = language.CreateEngine(this.runtime);

            //TODO: Refactor this:
            GenericImportModule.Host = (this as DlrHost<IronPythonLanguage>);

            // create default scope
            defaultScope = new DlrScriptScope(this);
        }
        #endregion

        #region Private Methods

        #endregion

        #region Public Methods
        /// <summary>
        /// Create new default scope. One default scope will be created automatically when creating a Host-Instance.
        /// </summary>
        /// <returns>Scope instance</returns>
        public DlrScriptScope CreateNewDefaultScope()
        {
            defaultScope = new DlrScriptScope(this);
            return defaultScope;
        }

        /// <summary>
        /// Load an assembly into the runtime
        /// </summary>
        /// <param name="asm">Instance of an assembly</param>
        public void LoadAssembly(Assembly asm)
        {
            if (asm == null)
            {
                throw new ArgumentNullException("asm");
            }

            if (!this.loadedAssemblies.Contains(asm.FullName))
            {
                this.runtime.LoadAssembly(asm);
                this.loadedAssemblies.Add(asm.FullName);
            }
        }

        /// <summary>
        /// Precompile all available scripts
        /// </summary>
        public void PreCompile()
        {

        }

        /// <summary>
        /// Add additional search path to look for modules/packages in the filesystem
        /// </summary>
        /// <param name="path">Path to the modules</param>
        public void AddSearchPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Invalid path information passed to AddSearchPath. Path must not be null or white space");
            }

            var paths = scriptEngine.GetSearchPaths();
            paths.Add(path);
            scriptEngine.SetSearchPaths(paths);
        }

        /// <summary>
        /// Remove search path from the list of module/package paths
        /// </summary>
        /// <param name="path">Path to remove</param>
        /// <returns>True if the path was found and removed, else false</returns>
        public bool RemoveSearchPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Invalid path information passed to AddSearchPath. Path must not be null or white space");
            }

            foreach (var exPath in scriptEngine.GetSearchPaths())
            {
                if (exPath.ToLower() == path.ToLower())
                {
                    scriptEngine.GetSearchPaths().Remove(path);
                    return true;
                }
            }

            return false;
        }   
        
        /// <summary>
        /// Add a resolver instance to the resolver list
        /// </summary>
        /// <param name="resolver"></param>
        public void AddImportResolver(IDlrImportResolver resolver)
        {
            RemoveSearchPath(GenericImportModule.RESOLVER_PATH_NAME);
            AddSearchPath(GenericImportModule.RESOLVER_PATH_NAME);

            this.resolver.Add(resolver);
        }
        #endregion

        #region Public Member
        /// <summary>
        /// Default scope for the current dlr host instance
        /// </summary>
        public DlrScriptScope DefaultScope
        {
            get
            {
                return defaultScope;
            }
        }

        /// <summary>
        /// Instance of the ScriptEngine connected with the host
        /// </summary>
        public ScriptEngine ScriptEngine
        {
            get
            {
                return scriptEngine;
            }
        }

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
