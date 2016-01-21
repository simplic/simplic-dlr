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
        /// Import path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        string GetScriptSource(string path);

        /// <summary>
        /// Unique resolver name
        /// </summary>
        Guid UniqueResolverId
        {
            get;
        }
    }
}
