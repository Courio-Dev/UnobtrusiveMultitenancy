namespace Puzzle.Core.Multitenancy
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Attribute to exclude from code coverage.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
    public class ExcludeFromCoverageAttribute : Attribute { }
}
