namespace PuzzleCMS.Core.Multitenancy.Internal.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog;
    using PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog.LogProviders;

    /// <summary>
    /// Represents a way to get a <see cref="ILog"/>.
    /// </summary>
    /// <typeparam name="TTenant"></typeparam>
    public abstract class TenantLoggerProviderBase<TTenant> : /*LogProviderBase,*/ITenantLoggerProvider<TTenant>
    {
        private static readonly IDisposable NoopDisposableInstance = new DisposableAction();

        private readonly Lazy<OpenNdc> lazyOpenNdcMethod;
        private readonly Lazy<OpenMdc> lazyOpenMdcMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantLoggerProviderBase{TTenant}"/> class.
        /// </summary>
        protected TenantLoggerProviderBase()
        {
            lazyOpenNdcMethod = new Lazy<OpenNdc>(GetOpenNdcMethod);
            lazyOpenMdcMethod = new Lazy<OpenMdc>(GetOpenMdcMethod);
        }

        /// <summary>
        /// Nested Diagnostic Context.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected delegate IDisposable OpenNdc(string message);

        /// <summary>
        /// Mapped Diagnostic Context.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected delegate IDisposable OpenMdc(string key, string value);

        /// <summary>
        /// Get the logger by name.
        /// </summary>
        /// <param name="name">The name of the logger.</param>
        /// <returns></returns>
        public abstract Logger GetLogger(string name);

        /// <summary>
        /// Open Nested Diagnostic Context.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public IDisposable OpenNestedContext(string message) => lazyOpenNdcMethod.Value(message);


        /// <summary>
        /// Open Mapped Diagnostic Context.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public IDisposable OpenMappedContext(string key, string value) => lazyOpenMdcMethod.Value(key, value);


        /// <summary>
        /// 
        /// </summary>
        /// <returns>Nested Diagnostic Context.</returns>
        protected virtual OpenNdc GetOpenNdcMethod() => _ => NoopDisposableInstance;

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Mapped Diagnostic Context.</returns>
        protected virtual OpenMdc GetOpenMdcMethod() => (_, __) => NoopDisposableInstance;
    }
}
