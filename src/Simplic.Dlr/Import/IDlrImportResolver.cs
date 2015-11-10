using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simplic.Dlr
{
    /// <summary>
    /// Resolver for importing scripts and modules
    /// </summary>
    public interface IDlrImportResolver
    {
        /// <summary>
        /// Resove a specific script/module
        /// </summary>
        /// <param name="module">Module or script name</param>
        /// <param name="engine">Script engine instance</param>
        /// <returns>Instance of the created script source</returns>
        ScriptSource GetScriptSource(string module, ScriptEngine engine);

        /// <summary>
        /// Get all script sources for a specific resolver
        /// </summary>
        /// <param name="engine">Script engine instance</param>
        /// <returns>List of script sources</returns>
        ScriptSource[] GetScripts(ScriptEngine engine);
    }
}
