namespace ManagedBass.Dsd
{
    /// <summary>
    /// DSD Comment Type to be used with <see cref="DSDComment.CommentType"/>.
    /// </summary>
    public enum DSDCommentType : short
    {
        /// <summary>
        /// General
        /// </summary>
        General,

        /// <summary>
        /// Channel
        /// </summary>
        Channel,

        /// <summary>
        /// Sound Source
        /// </summary>
        SoundSource,

        /// <summary>
        /// File History
        /// </summary>
        FileHistory
    }
}