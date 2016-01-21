using Microsoft.Scripting;
using Microsoft.Scripting.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Simplic.Dlr
{
    /// <summary>
    /// Provides a StreamContentProvider for a stream of content backed by a file on disk.
    /// Source-Code from IronPython project
    /// </summary>
    [Serializable]
    internal sealed class SourceStringContentProvider : TextContentProvider
    {
        private readonly string _code;

        internal SourceStringContentProvider(string code)
        {
            ContractUtils.RequiresNotNull(code, "code");
            _code = NormalizeLineEndings(code);
        }

        public override SourceCodeReader GetReader()
        {
            return new SourceCodeReader(new StringReader(_code), null);
        }

        private string NormalizeLineEndings(string input)
        {
            return input.Replace("\r\n", "\n") + "\n";
        }
    }
}
