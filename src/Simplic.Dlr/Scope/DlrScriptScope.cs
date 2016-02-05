using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simplic.Dlr
{
    /// <summary>
    /// Represents a scope for the dlr hosting system. A scope descriptes an area, where stuff is available and accessable
    /// </summary>
    public class DlrScriptScope
    {
        #region Private Member
        private ScriptScope scriptScope;
        private IDlrHost host;
        private IDictionary<string, CompiledCode> cachedExpressions;
        private IDictionary<string, CompiledCode> compiledScripts;
        private IList<string> executedScripts;
        #endregion

        #region Constructor
        /// <summary>
        /// Create new DlrScope for executing base stuff
        /// </summary>
        /// <param name="host">Instance of the hosting component</param>
        public DlrScriptScope(IDlrHost host)
        {
            this.host = host;
            cachedExpressions = new Dictionary<string, CompiledCode>();
            executedScripts = new List<string>();

            scriptScope = host.ScriptEngine.CreateScope();
        }
        #endregion

        #region Private Methods

        #endregion

        #region Public Methods
        /// <summary>
        /// Execute a simple script expression
        /// </summary>
        /// <param name="expression">Script expression as a string</param>
        /// <param name="cache">True if the expression should be cached</param>
        /// <returns>Result of the script expression as dynamic</returns>
        public dynamic Execute(string expression, bool cache = true)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(expression);
            }

            if (cache == false)
            {
                return host.ScriptEngine.CreateScriptSourceFromString(expression).Execute(scriptScope);
            }
            else
            {
                string hash = Helper.Hash(expression);

                if (cachedExpressions.ContainsKey(hash))
                {
                    CompiledCode cc = cachedExpressions[hash];
                    return cc.Execute(scriptScope);
                }
                else
                {
                    ScriptSource source = host.ScriptEngine.CreateScriptSourceFromString(expression);
                    CompiledCode cc = source.Compile();
                    cachedExpressions.Add(hash, cc);

                    return cc.Execute(scriptScope);
                }
            }
        }

        /// <summary>
        /// Clear cached expressions
        /// </summary>
        public void ClearCache()
        {
            cachedExpressions.Clear();
            cachedExpressions = new Dictionary<string, CompiledCode>();
        }

        /// <summary>
        /// Create an instance of an IronPython class
        /// </summary>
        /// <param name="className">Name of the class</param>
        /// <param name="parameter">List of parameter which will be passed to the constructor</param>
        /// <returns>Instance of a dlr class containing dlr meta class</returns>
        public DlrClass CreateClassInstance(string className, params object[] parameter)
        {
            return new DlrClass(this, className, parameter);
        }

        #region [Variable]
        /// <summary>
        /// Set the value of a variable in the current scope
        /// </summary>
        /// <param name="variable">Name of the variable</param>
        /// <param name="value">Value of the variable</param>
        public void SetVariable(string variable, dynamic value)
        {
            if (string.IsNullOrWhiteSpace(variable))
            {
                throw new ArgumentException("Variable name can not be null or white space", "variable");
            }
            scriptScope.SetVariable(variable, value);
        }

        /// <summary>
        /// Get variable value by name
        /// </summary>
        /// <param name="variable">Name of the variable</param>
        /// <returns>Content of the variable</returns>
        public dynamic GetVariable(string variable)
        {
            if (string.IsNullOrWhiteSpace(variable))
            {
                throw new ArgumentException("Variable name can not be null or white space", "variable");
            }

            return scriptScope.GetVariable(variable);
        }
        #endregion

        #region [Call functions]
        /// <summary>
        /// Call a function within the current scope
        /// </summary>
        /// <param name="name">Name of the function</param>
        /// <param name="parameter">List of parameter</param>
        /// <returns>Return value of the function</returns>
        public dynamic CallFunction(string name, params object[] parameter)
        {
            var method = GetVariable(name);
            return method(parameter);
        }
        #endregion

        #region [PreCompile Code]
        /// <summary>
        /// Precompile code and store it in a dictioanry
        /// </summary>
        /// <param name="name">Unique name of the script, to access it later</param>
        /// <param name="code">Code to compile</param>
        /// <param name="overrideExisting">True if existing scripts with the same name can be overriden. If set to false and the
        /// script alredy exists, an exception will be thrown</param>
        /// <param name="isModule">Register also as module, so it can be load with the `import` command. !!!Not yet implemented!!!</param>
        public void PreCompileCode(string name, string code, bool overrideExisting = true, bool isModule = false)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new Exception("Could not precompile script where name is null or white-space");
            }
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new Exception("Could not precompile script where code is null or white-space");
            }

            ScriptSource source = host.ScriptEngine.CreateScriptSourceFromString(code);
            CompiledCode cc = source.Compile();

            if (compiledScripts.ContainsKey(name))
            {
                if (overrideExisting)
                {
                    compiledScripts[name] = cc;
                }
                else
                {
                    throw new Exception(string.Format("Script is already precompiled {0}"));
                }
            }
            else
            {
                compiledScripts.Add(name, cc);
            }
        }

        /// <summary>
        /// Execute precompiled code and return result
        /// </summary>
        /// <param name="name">Unique name of the scipt</param>
        /// <param name="otherScope">Deriving scope in which the script should be executed in</param>
        /// <returns>Result of the script/statement</returns>
        public dynamic ExecutePreCompiledCode(string name, ScriptScope otherScope = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new Exception("Could not get precompiled script where name is null or white-space");
            }

            if (compiledScripts.ContainsKey(name))
            {
                // Execute compiled
                var cc = compiledScripts[name];
                return cc.Execute(otherScope ?? this.scriptScope);
            }
            else
            {
                throw new Exception(string.Format("Could not find precompiled code with the name {0}", name));
            }
        }
        #endregion

        #region [ExecuteScript]
        /// <summary>
        /// Execute a script. The script will be searched in all resolvers and the default search paths
        /// </summary>
        /// <param name="path">Script path</param>
        /// <param name="alwaysExecute">If set to true, the script will be executed event if it was before. If set to 
        /// false and the path was already executed, null will be returned</param>
        /// <returns>Result of the script execution</returns>
        public dynamic ExecuteScript(string path, bool alwaysExecute = false)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path can not be null or white-space in DlrScriptScope.ExecuteScript.");
            }

            // Cancel if alwaysExecute is fals and the script was already executed
            if (!alwaysExecute && executedScripts.Any(item => item.Equals(path, StringComparison.OrdinalIgnoreCase)))
            {
                return null;
            }

            foreach (var resolver in host.Resolver)
            {
                try
                {
                    string result = resolver.GetScriptSource(path);
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        executedScripts.Add(path);
                        return Execute(result, true);
                    }
                }
                catch
                {
                    if (executedScripts.Contains(path))
                    {
                        executedScripts.Remove(path);
                    }

                    throw;
                }
            }

            var source = host.ScriptEngine.CreateScriptSourceFromFile(path);
            return source.Execute(scriptScope);
        }

        /// <summary>
        /// Returns whether a script was executed over `ExecuteScript`
        /// </summary>
        /// <param name="path">Path to the script</param>
        /// <returns>True if the script was executed</returns>
        public bool IsScriptPathCached(string path)
        {
            return executedScripts.Any(item => item.Equals(path, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Remove path from cache, so that a script from ExecuteScript will be executed the next time without using alwaysExecute = true
        /// </summary>
        /// <param name="path">Path to the script</param>
        public void RemoveCachedScriptPath(string path)
        {
            if (executedScripts.Any(item => item.Equals(path, StringComparison.OrdinalIgnoreCase)))
            {
                executedScripts.Remove(path);
            }
        }
        #endregion

        #endregion

        #region Public Member
        /// <summary>
        /// Instance of the dlr script scope
        /// </summary>
        public ScriptScope ScriptScope
        {
            get
            {
                return scriptScope;
            }
        }

        /// <summary>
        /// Instance of the connected host
        /// </summary>
        public IDlrHost Host
        {
            get
            {
                return host;
            }
        }

        /// <summary>
        /// Get current dnymic script scope
        /// </summary>
        public dynamic DynamicScriptScope
        {
            get
            {
                return scriptScope;
            }
        }
        #endregion
    }
}
