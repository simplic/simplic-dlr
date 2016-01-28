using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simplic.Dlr
{
    /// <summary>
    /// Resolve path
    /// </summary>
    public enum ResolvedType
    {
        /// <summary>
        /// Could not detect/find script
        /// </summary>
        None = 0,

        /// <summary>
        /// Package resolved
        /// </summary>
        Package = 1,

        /// <summary>
        /// Module resolved
        /// </summary>
        Module = 2
    }

    /// <summary>
    /// Resolver for importing scripts and modules
    /// </summary>
    public interface IDlrImportResolver
    {
        /// <summary>
        /// Import path
        /// </summary>
        /// <param name="path">Returns the script-source</param>
        /// <returns></returns>
        string GetScriptSource(string path);

        /// <summary>
        /// Detect the type of the script. Returns None-Type if not found
        /// </summary>
        /// <param name="path">Module name, package path or script path (ends with .py)</param>
        /// <returns>Type of the script</returns>
        ResolvedType GetType(string path);

        /// <summary>
        /// Unique resolver name
        /// </summary>
        Guid UniqueResolverId
        {
            get;
        }
    }
}
