using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Simplic.Dlr
{
    /// <summary>
    /// Represents a simplic Dlr host
    /// </summary>
    public interface IDlrHost
    {
        /// <summary>
        /// Load an assembly into the runtime
        /// </summary>
        /// <param name="asm">Instance of an assembly</param>
        void LoadAssembly(Assembly asm);

        /// <summary>
        /// Precompile all available scripts
        /// </summary>
        void PreCompile();

        /// <summary>
        /// Add a simple resolver
        /// </summary>
        /// <param name="resolver">Resolver instance</param>
        void AddImportResolver(IDlrImportResolver resolver);

        /// <summary>
        /// Default scope for the current dlr host instance
        /// </summary>
        DlrScriptScope DefaultScope
        {
            get;
        }

        /// <summary>
        /// Instance of the ScriptEngine connected with the host
        /// </summary>
        ScriptEngine ScriptEngine
        {
            get;
        }

        /// <summary>
        /// Import resolver list
        /// </summary>
        IList<IDlrImportResolver> Resolver
        {
            get;
        }
    }
}
