using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Simplic.Dlr
{
    /// <summary>
    /// Set of internal helper methods
    /// </summary>
    internal class Helper
    {
        /// <summary>
        /// Hash simple string to SHA1
        /// </summary>
        /// <param name="input">String which should be hashed</param>
        /// <returns>String to hash</returns>
        internal static string Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }
    }
}
