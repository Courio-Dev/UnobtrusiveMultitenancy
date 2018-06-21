namespace Puzzle.Core.Multitenancy
{
    using System;

    /// <summary>
    /// Attribute to exclude from code coverage.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
    public class ExcludeFromCodeCoverageAttribute : Attribute { }
}
