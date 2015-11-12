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
            return new DlrClass(this, className);
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
