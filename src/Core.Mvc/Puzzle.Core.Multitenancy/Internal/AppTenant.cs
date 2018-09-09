namespace Puzzle.Core.Multitenancy.Internal
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The tenant object.
    /// </summary>
    public class AppTenant
    {
        /// <summary>
        /// Gets or sets the name of tenant.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the id of tenant, thid must be unique.
        /// </summary>
        public string Id => GenerateId(Name).ToLowerInvariant();

        /// <summary>
        /// Gets or sets list of hostname hosted by tenant.
        /// </summary>
        public string[] Hostnames { get; set; }

        /// <summary>
        /// Gets or sets theme of tenant.
        /// </summary>
        public string Theme { get; set; }

        /// <summary>
        /// Gets or sets connection string of tenant.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Credit for this method goes to http://stackoverflow.com/questions/2920744/url-slugify-alrogithm-in-cs.
        /// </summary>
        /// <param name="value">value.</param>
        /// <returns>string.</returns>
        private static string GenerateId(string value)
        {
            // prepare string, remove accents, lower case and convert hyphens to whitespace
            string result = RemoveDiacritics(value).Replace("-", " ").ToLowerInvariant();

            result = Regex.Replace(result, @"[^a-z0-9\s-]", string.Empty); // remove invalid characters
            result = Regex.Replace(result, @"\s+", " ").Trim(); // convert multiple spaces into one space

            return Regex.Replace(result, @"\s", "-"); // replace all spaces with hyphens
        }

        private static string RemoveDiacritics(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            string normalizedString = text.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
