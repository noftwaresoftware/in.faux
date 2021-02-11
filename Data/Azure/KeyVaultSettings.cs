using System;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Noftware.In.Faux.Data.Extensions;
using Noftware.In.Faux.Core.Services;

namespace Noftware.In.Faux.Data.Azure
{
    /// <summary>
    /// Wrapper to access settings from Azure KeyVault.
    /// </summary>
    public class KeyVaultSettings : IKeyVaultSettings
    {
        // Azure SecretClient
        private static SecretClient _client;

        // Tenant Id
        private readonly string _tenantId;

        // KeyVault address
        private readonly string _vaultAddress;

        // KeyVault client Id
        private readonly string _clientId;

        // KeyVault secret
        private readonly string _secret;

        /// <summary>
        /// Default constructor when tenant id, client Id and secret are known.
        /// </summary>
        /// <param name="vaultAddress">KeyVault address.</param>
        /// <param name="clientId">KeyVault client Id.</param>
        /// <param name="clientId">KeyVault client Id.</param>
        /// <param name="secret">KeyVault secret.</param>
        public KeyVaultSettings(string vaultAddress, string tenantId, string clientId, string secret)
        {
            // Vault details
            _vaultAddress = vaultAddress;
            _clientId = clientId;
            _secret = secret;
            _tenantId = tenantId;

            _client = new SecretClient(new Uri(_vaultAddress), new ClientSecretCredential(_tenantId, _clientId, _secret), GetOptions());
        }

        /// <summary>
        /// Default constructor when az login or environment variables are used.
        /// </summary>
        /// <param name="vaultAddress">KeyVault address.</param>
        public KeyVaultSettings(string vaultAddress)
        {
            // Vault details
            _vaultAddress = vaultAddress;

            _client = new SecretClient(new Uri(_vaultAddress), new DefaultAzureCredential(), GetOptions());
        }

        /// <summary>
        /// Retry operations.
        /// </summary>
        /// <returns><see cref="SecretClientOptions"/></returns>
        private static SecretClientOptions GetOptions()
        {
            var options = new SecretClientOptions()
            {
                Retry =
                {
                    Delay = TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(16),
                    MaxRetries = 5,
                    Mode = RetryMode.Exponential
                }
            };

            return options;
        }

        /// <summary>
        /// Get secret value based on key.
        /// </summary>
        /// <param name="key">Key. Example: secret:KeyName/KeyVersion</param>
        /// <returns>Value, if found. Null, if otherwise.</returns>
        public string GetSecret(string key)
        {
            if (string.IsNullOrEmpty(key) == true)
            {
                return null;
            }

            KeyVaultSecret secret;

            // Example: secret:KeyName/KeyVersion
            var parsed = new ParsedKey(key);
            if (parsed.HasVersion == true)
            {
                secret = _client.GetSecretAsync(parsed.KeyName, parsed.Version).GetAwaiter().GetResult().Value;
            }
            else
            {
                secret = _client.GetSecretAsync(parsed.KeyName).GetAwaiter().GetResult().Value;
            }

            return secret?.Value;
        }

        /// <summary>
        /// Parses a KV string in the following formats:
        /// secret:KeyName/KeyVersion
        /// KeyName/KeyVersion
        /// KeyName
        /// </summary>
        private class ParsedKey
        {
            /// <summary>
            /// Constructor to accept the KV string.
            /// </summary>
            /// <param name="input">KV string.</param>
            public ParsedKey(string input)
            {
                if (input != null)
                {
                    string output = input;
                    if (output.StartsWith("secret:", StringComparison.InvariantCultureIgnoreCase) == true)
                    {
                        output = output.ReplaceIgnoreCase("secret:", string.Empty);
                    }

                    // Does it contain a forward slash? If so, treat it as a version
                    if (output.Contains("/") == true)
                    {
                        var items = output.Split('/');
                        this.KeyName = items[0];
                        this.Version = items[1];
                    }
                    else
                    {
                        this.KeyName = output;
                    }
                }
            }

            /// <summary>
            /// Name of key.
            /// </summary>
            public string KeyName { get; set; }

            /// <summary>
            /// True if key is defined, false if otherwise.
            /// </summary>
            public bool HasKey
            {
                get
                {
                    return !string.IsNullOrWhiteSpace(this.KeyName);
                }
            }

            /// <summary>
            /// [optional] Version.
            /// </summary>
            public string Version { get; set; }

            /// <summary>
            /// True if version is defined, false if otherwise.
            /// </summary>
            public bool HasVersion
            {
                get
                {
                    return !string.IsNullOrWhiteSpace(this.Version);
                }
            }

            /// <summary>
            /// Rebuilds the entire string.
            /// </summary>
            public override string ToString()
            {
                if (this.HasKey == true)
                {
                    if (this.HasVersion == true)
                    {
                        return $"secret:{this.KeyName}/{this.Version}";
                    }
                    else
                    {
                        return $"secret:{this.KeyName}";
                    }
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
