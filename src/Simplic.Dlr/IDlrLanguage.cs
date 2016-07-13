
using Microsoft.Scripting.Hosting;
using System.Collections.Generic;

namespace Simplic.Dlr
{
    public delegate void SetupRuntime(ScriptRuntime runtime);

    /// <summary>
    /// Base class which must be inherited in all Language implementations (e.g. IronPythonLanguage)
    /// </summary>
    public interface IDlrLanguage
    {
        /// <summary>
        /// Create runtime settings
        /// </summary>
        /// <returns>ScriptRuntimeSetup containing all settings</returns>
        ScriptRuntimeSetup CreateRuntime();

        /// <summary>
        /// Create scripting engine
        /// </summary>
        /// <param name="runtime">Instance of the used runtime, created by the DlrHost</param>
        /// <returns>Instance if the script engine (e.g. for IronPython, IronRuby, ...)</returns>
        ScriptEngine CreateEngine(ScriptRuntime runtime);

        /// <summary>
        /// Gets or sets a set of language options. For example Arguments for passing argv
        /// </summary>
        IDictionary<string, object> LanguageOptionSet
        {
            get;
            set;
        }
    }
}
