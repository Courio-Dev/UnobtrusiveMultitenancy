namespace Puzzle.Core.Multitenancy.Internal.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Puzzle.Core.Multitenancy.Internal.Logging.LibLog;

    /// <summary>
    /// Represents a way to get a <see cref="ILog"/>
    /// </summary>
    /// <typeparam name="TTenant">TTenant.</typeparam>
    public interface  ITenantLoggerProvider<TTenant>:ILogProvider
    {
    }
}
