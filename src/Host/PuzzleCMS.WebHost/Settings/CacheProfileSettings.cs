namespace PuzzleCMS.WebHost.Settings
{
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;

    /// <summary>
    /// The caching settings for the application.
    /// </summary>
    public class CacheProfileSettings
    {
        /// <summary>
        /// Gets or sets the cache profiles (How long to cache things for).
        /// </summary>
        public Dictionary<string, CacheProfile> CacheProfiles { get; set; }
    }
}