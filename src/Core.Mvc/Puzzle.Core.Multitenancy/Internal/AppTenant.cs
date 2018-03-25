using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Puzzle.Core.Multitenancy.Internal
{
    public class AppTenant
    {
        public string Name { get; set; }

        public string Id => GenerateSlug(Name);
        public string[] Hostnames { get; set; }
        public string Theme { get; set; }
        public string ConnectionString { get; set; }

        /// <summary>
        /// Credit for this method goes to http://stackoverflow.com/questions/2920744/url-slugify-alrogithm-in-cs
        /// </summary>
        private static string GenerateSlug(string value, int? maxLength = null)
        {
            // prepare string, remove accents, lower case and convert hyphens to whitespace
            var result = RemoveDiacritics(value).Replace("-", " ").ToLowerInvariant();

            result = Regex.Replace(result, @"[^a-z0-9\s-]", string.Empty); // remove invalid characters
            result = Regex.Replace(result, @"\s+", " ").Trim(); // convert multiple spaces into one space

            return Regex.Replace(result, @"\s", "-"); // replace all spaces with hyphens
        }

        private static string RemoveDiacritics(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
    }

    /*
    public class AppTenant : IEquatable<AppTenant>
    {
        public string Name { get; set; }
        public string[] Hostnames { get; set; }

        public string Id { get => Name.Replace(" ", "").ToLower(); }

        public bool Equals(AppTenant other)
        {
            if (other == null)
            {
                return false;
            }

            return other.Name.Equals(Name);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AppTenant);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
    */
}