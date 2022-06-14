namespace JsonEverythingNet.Services.MarkdownGen
{
    /// <summary>
    /// Comment of one enum value
    /// </summary>
    public class EnumValueComment : CommonComments
    {
        /// <summary>
        /// The name of the enum value
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Integer value of the enum
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Debugging-friendly text.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{(Name??"")}={Value}" + (Summary != null ? $" {Summary}" : "");
        }
    }
}
