using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace PuzzleCMS.Core.Multitenancy.Internal.Options
{
    /// <summary>
    /// 
    /// </summary>
    public static class OptionsMonitorExtensions
    {
        private static readonly ConcurrentDictionary<object, CancellationTokenSource> Tokens
            = new ConcurrentDictionary<object, CancellationTokenSource>();

        private const int DefaultDelay = 1000;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="monitor"></param>
        /// <param name="listener"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static IDisposable OnChangeDelayed<T>(this IOptionsMonitor<T> monitor, Action<T> listener, int delay = DefaultDelay)
        {
            return monitor.OnChangeDelayed(
                (obj, _) => listener(obj),
                delay);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="monitor"></param>
        /// <param name="listener"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static IDisposable OnChangeDelayed<T>(this IOptionsMonitor<T> monitor, Action<T, string> listener, int delay = DefaultDelay)
        {
            return monitor.OnChange((obj, name) => ChangeHandler(monitor, listener, obj, name));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="monitor"></param>
        /// <param name="listener"></param>
        /// <param name="obj"></param>
        /// <param name="name"></param>
        private static void ChangeHandler<T>(IOptionsMonitor<T> monitor, Action<T, string> listener, T obj, string name)
        {
            CancellationTokenSource tokenSource = GetCancellationTokenSource(monitor);
            CancellationToken token = tokenSource.Token;
            Task delay = Task.Delay(DefaultDelay, token);

            delay.ContinueWith(
                _ => ListenerInvoker(monitor, listener, obj, name),
                token
                );
        }

        private static CancellationTokenSource GetCancellationTokenSource<T>(IOptionsMonitor<T> monitor)
        {
            return Tokens.AddOrUpdate(monitor, CreateTokenSource, ReplaceTokenSource);
        }

        private static CancellationTokenSource CreateTokenSource(object key)
        {
            return new CancellationTokenSource();
        }

        private static CancellationTokenSource ReplaceTokenSource(object key, CancellationTokenSource existing)
        {
            existing.Cancel();
            existing.Dispose();
            return new CancellationTokenSource();
        }

        private static void ListenerInvoker<T>(IOptionsMonitor<T> monitor, Action<T, string> listener, T obj, string name)
        {
            listener(obj, name);
            if (Tokens.TryRemove(monitor, out CancellationTokenSource tokenSource))
            {
                tokenSource.Dispose();
            }
        }
    }
}
