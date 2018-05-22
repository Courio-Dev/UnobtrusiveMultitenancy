namespace PuzzleCMS.UnitsTests.Base
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Puzzle.Core.Multitenancy;
    using Puzzle.Core.Multitenancy.Extensions;
    using Puzzle.Core.Multitenancy.Internal;
    using Puzzle.Core.Multitenancy.Internal.Options;
    using Puzzle.Core.Multitenancy.Internal.Resolvers;
    using Xunit;
    using static PuzzleCMS.UnitsTests.Base.MultitenancyBaseFixture;

    /// <summary>
    /// The base test for multitenant.
    /// </summary>
    public class MultitenancyBaseTest : IClassFixture<MultitenancyBaseFixture>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultitenancyBaseTest"/> class.
        /// </summary>
        /// <param name="fixture">The base fixture.</param>
        public MultitenancyBaseTest(MultitenancyBaseFixture fixture)
        {
            Fixture = fixture;
        }

        /// <summary>
        /// gets the base fixture.
        /// </summary>
        protected MultitenancyBaseFixture Fixture { get; private set; }

        internal TestHarness CreateTestHarness(bool disposeOnEviction = true, int cacheExpirationInSeconds = 10, bool evictAllOnExpiry = true)
        {
            return MultitenancyBaseFixture.CreateTestHarness(
                disposeOnEviction: disposeOnEviction,
                cacheExpirationInSeconds: cacheExpirationInSeconds,
                evictAllOnExpiry: evictAllOnExpiry);
        }

        /// <summary>
        /// Build WebHostBuilder for test.
        /// </summary>
        protected WebHostBuilder CreateWebHostBuilder<TStartup, TTenant, TResolver>()
           where TStartup : class
           where TTenant : class
           where TResolver : class, ITenantResolver<TTenant>
        {
            return MultitenancyBaseFixture.CreateWebHostBuilder<TStartup, TTenant, TResolver>();
        }
    }
}
