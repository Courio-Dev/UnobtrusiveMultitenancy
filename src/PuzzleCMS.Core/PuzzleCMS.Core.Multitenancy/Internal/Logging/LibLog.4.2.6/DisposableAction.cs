namespace PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog
{
    using System;

    /// <summary>
    /// DisposableAction.
    /// </summary>
    [ExcludeFromCodeCoverage]
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
