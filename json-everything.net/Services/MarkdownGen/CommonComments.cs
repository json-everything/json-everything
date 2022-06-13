namespace JsonEverythingNet.Services.MarkdownGen
{
    /// <summary>
    /// Base class for comments classes
    /// </summary>
    public class CommonComments
    {
        /// <summary>
        /// "summary" comment
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// "remarks" comment
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// "example" comment
        /// </summary>
        public string Example { get; set; }

        /// <summary>
        /// Inheritdoc tag. Null if missing in comments.
        /// </summary>
        public InheritdocTag Inheritdoc { get; set; }
    }
}
