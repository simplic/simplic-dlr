using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simplic.Dlr
{
    /// <summary>
    /// Type of module
    /// </summary>
    [Flags]
    internal enum GenericModuleCodeType
    {
        /// <summary>
        /// Is source code
        /// </summary>
        Source = 0,

        /// <summary>
        /// Is Byte-Code
        /// </summary>
        ByteCode = 1,

        /// <summary>
        /// Is package (__init__.py)
        /// </summary>
        Package = 2,
    }
}
