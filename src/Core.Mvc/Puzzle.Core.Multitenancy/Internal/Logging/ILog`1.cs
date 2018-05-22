namespace Puzzle.Core.Multitenancy.Internal.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Puzzle.Core.Multitenancy.Internal.Logging.LibLog;

    /// <summary>
    /// A generic interface for logging where the category name is derived from the specified
    /// <typeparamref name="TCategoryName"/> type name.
    /// Generally used to enable activation of a named <see cref="ILog"/> from dependency injection.
    /// </summary>
    /// <typeparam name="TCategoryName">The type who's name is used for the logger category name.</typeparam>
    [ExcludeFromCodeCoverage]
    public interface ILog<out TCategoryName> : ILog
    {
    }
}
