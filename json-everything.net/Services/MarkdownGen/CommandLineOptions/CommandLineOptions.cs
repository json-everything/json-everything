namespace JsonEverythingNet.Services.MarkdownGen.CommandLineOptions
{
    /// <summary>
    /// Mddox command line definition
    /// </summary>
    public class CommandLineOptions
    {
        public string AssemblyName { get; set; }
        public string OutputFile { get; set; }
        public string Format { get; set; }
        public bool AllRecursive { get; set; }
        public IEnumerable<string> RecursiveAssemblies { get; set; }
        public bool IgnoreMethods { get; set; }
        public bool DocumentMethodDetails { get; set; }
        public IEnumerable<string> IgnoreAttributes { get; set; }
        public string TypeName { get; set; }
        public string MsdnLinkViewParameter { get; set; }
        public string DocumentTitle { get; set; }
        public bool DoNotShowDocumentDateTime { get; set; }
        public bool Verbose { get; set; }
    }
}
