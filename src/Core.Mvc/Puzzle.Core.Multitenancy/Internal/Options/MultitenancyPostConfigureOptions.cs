namespace Puzzle.Core.Multitenancy.Internal.Options
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Text;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Represents something that post configures the MultitenancyOptions type.
    /// </summary>
    /// <typeparam name="TTenant">Tenant object.</typeparam>
    public class MultitenancyPostConfigureOptions<TTenant> : IPostConfigureOptions<MultitenancyOptions<TTenant>>
    {
        private const string OpenTokenReplacement = "{";
        private const string CloseTokenReplacement = "}";

        private static readonly char DirectorySeparator = Path.DirectorySeparatorChar;
        private static readonly string FormatReplacement = string.Format("{0}{1}{2}", OpenTokenReplacement, 0, CloseTokenReplacement);

        private readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new JsonConverter[]
            {
               new StringEnumConverter(),
            },
            StringEscapeHandling = StringEscapeHandling.EscapeHtml,
            Formatting = Newtonsoft.Json.Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects,
        };

        /// <summary>
        /// Gets list of tokens replacement.
        /// </summary>
        public Dictionary<string, string> TokenList { get; } = new Dictionary<string, string>()
        {
            // Default tokens.
            [string.Format(FormatReplacement, "DS")] = new string(DirectorySeparator, 2),
            [string.Format(FormatReplacement, "Env")] = string.Empty,
            [string.Format(FormatReplacement, "TenantFolder")] = "App_Tenants",
        };

        /// <summary>
        /// Invoked to configure a MultitenancyOptions instance.
        /// </summary>
        /// <param name="name">The name of the options instance being configured.</param>
        /// <param name="options">The MultitenancyOptions instance to configured.</param>
        public void PostConfigure(string name, MultitenancyOptions<TTenant> options)
        {
            AddAdditionnalKeys(options, TokenList);
            options = ReplaceTokenString(options, TokenList);
        }

        private void AddAdditionnalKeys(MultitenancyOptions<TTenant> options, IDictionary<string, string> tokenList)
        {
            // Add other tokens.
            foreach (KeyValuePair<string, string> item in options.OtherTokens)
            {
                tokenList[string.Format(FormatReplacement, item.Key)] = item.Value;
            }
        }

        private MultitenancyOptions<TTenant> ReplaceTokenString(MultitenancyOptions<TTenant> options, IDictionary<string, string> tokenList)
        {
            string tmpString = DumpToJsonString(options.Tenants);
            string tenantListAsString = ReplaceWithStringBuilder(tmpString, tokenList);

            options.Tenants = JsonConvert.DeserializeObject<Collection<TTenant>>(tenantListAsString);
            return options;
        }

        private string ReplaceWithStringBuilder(string value, IDictionary<string, string> tokenList)
        {
            StringBuilder result = new StringBuilder(value);
            foreach (KeyValuePair<string, string> item in tokenList)
            {
                result.Replace($"{OpenTokenReplacement}{item.Key}{CloseTokenReplacement}", item.Value);
            }

            return result.ToString();
        }

        /// <summary>
        /// Convert object To Json String.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The json as string.</returns>
        private string DumpToJsonString(object obj)
        {
            return JsonConvert.SerializeObject(obj, settings);
        }
    }
}
