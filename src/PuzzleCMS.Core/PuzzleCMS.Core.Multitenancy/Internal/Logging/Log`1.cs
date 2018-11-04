namespace PuzzleCMS.Core.Multitenancy.Internal.Logging
{
    using System;
    using PuzzleCMS.Core.Multitenancy.Internal.Logging.LibLog;

    /// <summary>
    /// A generic interface for logging where the category name is derived from the specified
    /// <typeparamref name="T"/> type name.
    /// Generally used to enable activation of a named <see cref="ILog"/> from dependency injection.
    /// </summary>
    /// <typeparam name="T">The type who's name is used for the logger category name.</typeparam>
    [ExcludeFromCodeCoverage]
    internal class Log<T> : ILog<T>
    {
        private readonly ILog log;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logProvider"></param>
        public Log(ILogProvider logProvider)
        {
            log =(logProvider!=null)? GetLogger(logProvider,typeof(T)) : LogProvider.For<T>();
        }

        bool ILog.Log(LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
        {
            return log.Log(logLevel, messageFunc, exception, formatParameters);
        }

        private static ILog GetLogger(ILogProvider logProvider,Type type, string fallbackTypeName = "System.Object")
        {
            // If the type passed in is null then fallback to the type name specified
            return GetLogger(logProvider,type != null ? type.FullName : fallbackTypeName);
        }

        /// <summary>
        /// Gets a logger with the specified name.
        /// </summary>
        /// <param name="logProvider">the provided log provider</param>
        /// <param name="name">The name.</param>
        /// <returns>An instance of. <see cref="ILog"/></returns>
        private static ILog GetLogger(ILogProvider logProvider,string name)
        {
            bool isDisabled = false;
            return  (ILog)new LoggerExecutionWrapper(logProvider.GetLogger(name), () => isDisabled);
        }
    }
}
