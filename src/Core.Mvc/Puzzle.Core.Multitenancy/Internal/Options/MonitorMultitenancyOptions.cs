using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Puzzle.Core.Multitenancy.Internal.Options
{
    /// <summary>
    /// Implementation of IOptionsMonitor.
    /// https://raw.githubusercontent.com/aspnet/Options/dev/src/Microsoft.Extensions.Options/OptionsMonitor.cs
    /// http://www.cnblogs.com/RainingNight/p/strongly-typed-options-ioptions-monitor-in-asp-net-core.html
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public class MonitorMultitenancyOptions : IOptionsMonitor<MultitenancyOptions>
    {

        private readonly IOptionsMonitorCache<MultitenancyOptions> cache;
        private readonly IOptionsFactory<MultitenancyOptions> factory;
        private readonly IEnumerable<IOptionsChangeTokenSource<MultitenancyOptions>> sources;
        internal event Action<MultitenancyOptions, string> onChange;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="factory">The factory to use to create options.</param>
        /// <param name="sources">The sources used to listen for changes to the options instance.</param>
        /// <param name="cache">The cache used to store options.</param>
        public MonitorMultitenancyOptions(
            IOptionsFactory<MultitenancyOptions> factory,
            IEnumerable<IOptionsChangeTokenSource<MultitenancyOptions>> sources,
            IOptionsMonitorCache<MultitenancyOptions> cache
            /*ILogger<MultitenancyOptions> logger*/)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.sources = sources ?? throw new ArgumentNullException(nameof(sources));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));

            foreach (var source in sources)
            {
                ChangeToken.OnChange<string>(() => source.GetChangeToken(), (name) => InvokeChanged(name), source.Name);
            }
        }

        private void InvokeChanged(string name)
        {
            name = name ?? Microsoft.Extensions.Options.Options.DefaultName;
            cache.TryRemove(name);
            var options = Get(name);
            if (onChange != null)
            {
                onChange?.Invoke(options, name);
            }
        }

        /// <summary>
        /// The present value of the options.
        /// </summary>
        public MultitenancyOptions CurrentValue
        {
            get => Get(Microsoft.Extensions.Options.Options.DefaultName);
        }

        public virtual MultitenancyOptions Get(string name)
        {
            name = name ?? Microsoft.Extensions.Options.Options.DefaultName;
            return cache.GetOrAdd(name, () => factory.Create(name));
        }

        /// <summary>
        /// Registers a listener to be called whenever TOptions changes.
        /// </summary>
        /// <param name="listener">The action to be invoked when TOptions has changed.</param>
        /// <returns>An IDisposable which should be disposed to stop listening for changes.</returns>
        public IDisposable OnChange(Action<MultitenancyOptions, string> listener)
        {
            var disposable = new ChangeTrackerDisposable(this, listener);
            onChange += disposable.OnChange;
            return disposable;
        }

        internal class ChangeTrackerDisposable : IDisposable
        {
            private readonly Action<MultitenancyOptions, string> listener;
            private readonly MonitorMultitenancyOptions monitor;

            public ChangeTrackerDisposable(MonitorMultitenancyOptions monitor, Action<MultitenancyOptions, string> listener)
            {
                this.listener = listener;
                this.monitor = monitor;
            }

            public void OnChange(MultitenancyOptions options, string name) => listener.Invoke(options, name);

            public void Dispose() => monitor.onChange -= OnChange;
        }
    }
}
