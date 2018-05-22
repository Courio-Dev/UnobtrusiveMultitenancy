namespace Puzzle.Core.Multitenancy.Internal.Logging.LibLog
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// DisposableAction.
    /// </summary>
    [ExcludeFromCoverage]
    internal class DisposableAction : IDisposable
    {
        private readonly Action onDispose;

        public DisposableAction(Action onDispose = null)
        {
            this.onDispose = onDispose;
        }

        public void Dispose()
        {
            onDispose?.Invoke();
        }
    }
}
