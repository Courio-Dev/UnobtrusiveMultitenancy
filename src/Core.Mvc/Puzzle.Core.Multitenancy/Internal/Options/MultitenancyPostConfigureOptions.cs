namespace Puzzle.Core.Multitenancy.Internal.Options
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Represents something that post configures the MultitenancyOptions type.
    /// </summary>
    public class MultitenancyPostConfigureOptions : IPostConfigureOptions<MultitenancyOptions>
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
        public void PostConfigure(string name, MultitenancyOptions options)
        {
            if (options == null)
            {
                throw new InvalidOperationException(nameof(options));
            }

            AddAdditionnalKeys(options, TokenList);
            options = ReplaceTokenString(options, TokenList);
        }

        private void AddAdditionnalKeys(MultitenancyOptions options, IDictionary<string, string> tokenList)
        {
            // Add other tokens.
            foreach (KeyValuePair<string, string> item in options.OtherTokens)
            {
                tokenList[string.Format(FormatReplacement, item.Key)] = item.Value;
            }
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

        private string ReplaceWithStringBuilder(string value, IEnumerable<Tuple<string, string>> toReplace)
        {
            StringBuilder result = new StringBuilder(value);
            foreach (Tuple<string, string> item in toReplace)
            {
                result.Replace($"{OpenTokenReplacement}{item.Item1}{CloseTokenReplacement}", item.Item2);
            }

            return result.ToString();
        }

        private MultitenancyOptions ReplaceTokenString(MultitenancyOptions options, IDictionary<string, string> tokenList)
        {
            string tmpString = DumpToJsonString(options.Tenants);
            string tenantListAsString = ReplaceWithStringBuilder(tmpString, tokenList);

            options.Tenants = JsonConvert.DeserializeObject<Collection<AppTenant>>(tenantListAsString);
            return options;
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

        /*
        static string Invariant(FormattableString formattableString, object[] formatArguments)
        {
            string resultString = string.Format(formattableString.Format, formatArguments).ToString(CultureInfo.InvariantCulture);
            return resultString;
        }

        class FormattableStringImpl : FormattableString
        {
            private readonly string format;
            private readonly object[] arguments;

            internal FormattableStringImpl(string format,params object[] arguments)
            {
                this.format = format;
                this.arguments = arguments;
            }

            public override string Format => format;
            public override object[] GetArguments() => arguments;
            public override int ArgumentCount => arguments.Length;
            public override object GetArgument(int index) => arguments[index];

            public override string ToString(IFormatProvider formatProvider)
            {
                return string.Format(formatProvider, format, arguments);
            }
        }
        */
    }
}
