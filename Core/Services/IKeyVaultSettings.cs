using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noftware.In.Faux.Core.Services
{
    /// <summary>
    /// Interface to access settings from Azure KeyVault.
    /// </summary>
    public interface IKeyVaultSettings
    {
        /// <summary>
        /// Get secret value based on key.
        /// </summary>
        /// <param name="key">Key. Example: secret:KeyName/KeyVersion</param>
        /// <returns>Value, if found. Null, if otherwise.</returns>
        string GetSecret(string key);
    }
}
