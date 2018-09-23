namespace PuzzleCMS.Core.Multitenancy
{
    /// <summary>
    /// Used to retreive configured TTenant instances.
    /// </summary>
    /// <typeparam name="TTenant">The type of tenant being requested.</typeparam>
    public interface ITenant<out TTenant>
    {
        /// <summary>
        /// Gets tenant object.
        /// </summary>
        TTenant Value { get; }
    }
}
