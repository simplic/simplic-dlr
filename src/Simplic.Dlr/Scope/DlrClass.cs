using IronPython.Runtime.Types;
using System;
using System.Dynamic;

namespace Simplic.Dlr
{
    /// <summary>
    /// Class which represents an instance of a dlr class for simple using in c#
    /// </summary>
    /// <typeparam name="T">Type of IDlrLanguage</typeparam>
    public class DlrClass : DynamicObject
    {
        #region Private Member
        private dynamic instance;
        private dynamic type;
        private string className;
        private DlrScriptScope scriptScope;
        #endregion

        #region [Constructor]
        /// <summary>
        /// Create new dlr class
        /// </summary>
        /// <param name="scriptScope">Scope which contains the class definition and in which the instance will be created</param>
        /// <param name="className">name of the class</param>
        /// <param name="parameter">Arguments for the constructor</param>
        public DlrClass(DlrScriptScope scriptScope, string className, params object[] parameter)
        {
            this.className = className;
            this.scriptScope = scriptScope;

            // create class instance
            type = scriptScope.ScriptScope.GetVariable(className);
            instance = scriptScope.Host.ScriptEngine.Operations.CreateInstance(type, parameter);
        }

        /// <summary>
        /// Create new dlr class
        /// </summary>
        /// <param name="scriptScope">Scope which contains the class definition and in which the instance will be created</param>
        /// <param name="pythonType">Python-Type instance</param>
        /// <param name="parameter">Arguments for the constructor</param>
        public DlrClass(DlrScriptScope scriptScope, PythonType pythonType, params object[] parameter)
        {
            this.scriptScope = scriptScope;

            // create class instance
            instance = scriptScope.Host.ScriptEngine.Operations.CreateInstance(pythonType, parameter);
        }
        #endregion

        #region Public Methods

        #region [Try Get/Set Member]
        /// <summary>
        /// Get member over dlr/meta class
        /// </summary>
        /// <param name="binder">Binder to the dynamic member</param>
        /// <param name="result">Value which should be returned</param>
        /// <returns>True if the member exists</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            try
            {
                result = GetMember(binder.Name);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Set member over dlr/meta class
        /// </summary>
        /// <param name="binder">Binder to the dynamic member</param>
        /// <param name="value">Value which should be set</param>
        /// <returns>True if the value was set success full</returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            try
            {
                SetMember(binder.Name, value);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region [Private TryInvokeMember]
        /// <summary>
        /// Invoke a method
        /// </summary>
        /// <param name="binder">Binder to the method</param>
        /// <param name="args">Arguments for the method</param>
        /// <param name="result">Return value of the method</param>
        /// <returns>True if calling the method was successfull</returns>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            try
            {
                result = CallFunction(binder.Name, args);
                return true;
            }
            catch(Exception ex)
            {
                result = null;
                return false;
            }
        }
        #endregion

        public void CallMethod(string method, params dynamic[] arguments)
        {
            scriptScope.Host.ScriptEngine.Operations.InvokeMember(instance, method, arguments);
        }

        public void SetMember(string name, object value)
        {
            scriptScope.Host.ScriptEngine.Operations.SetMember(instance, name, value);
        }

        public dynamic GetMember(string member)
        {
            return scriptScope.Host.ScriptEngine.Operations.GetMember(instance, member);
        }

        public dynamic CallFunction(string method, params dynamic[] arguments)
        {
            return scriptScope.Host.ScriptEngine.Operations.InvokeMember(instance, method, arguments);
        }

        public dynamic CallFunctionIgnoreCase(string method, params dynamic[] arguments)
        {
            return scriptScope.Host.ScriptEngine.Operations.InvokeMember(instance, method, arguments);
        }
        #endregion

        #region Public Member
        /// <summary>
        /// Scope containing the class defn. / instance
        /// </summary>
        public DlrScriptScope ScriptScope
        {
            get
            {
                return scriptScope;
            }
        }

        /// <summary>
        /// Represents the dynamic class instance
        /// </summary>
        public dynamic Instance
        {
            get
            {
                return instance;
            }
        }
        #endregion
    }
}
