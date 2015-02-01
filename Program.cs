namespace Blakar.Tools.LinkDb
{
    using System;
    using PowerArgs;

    /// <summary>
    /// Entry point to the LinkDb application
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Starts executing application with user-specified arguments 
        /// </summary>
        /// <param name="args">List of arguments passed from external environment</param>
        public static void Main(string[] args)
        {
            Args.InvokeAction<LinkDbArgs>(args);
        }
    }

    /// <summary>
    /// Represents complete list of arguments supported by LinkDB console application
    /// </summary>
    [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
    internal class LinkDbArgs
    {
        /// <summary>
        /// Handle help request
        /// </summary>
        [HelpHook, ArgShortcut("-?"), ArgDescription("Show this help")]
        public bool Help { get; set; }

        /// <summary>
        /// Handle 'check' action.
        /// </summary>
        /// <param name="args">Check action arguments</param>
        [ArgActionMethod, ArgDescription("Check linkdb file syntax")]
        public void Check(CheckArguments args)
        {
            if (args.CheckAll)
            {
                args.CheckDuplicateLinks = true;
                args.CheckDuplicateTags = true;
            }

            var parser = new FileParser(args.FileName);
            parser.OnDuplicateLinkEvent += new EventHandler<DuplicateLinkEventArgs>(OnDuplicateLinkEvent);
            parser.OnDuplicateTagEvent += new EventHandler<DuplicateTagEventArgs>(OnDuplicateTagEvent);
            parser.OnGenericMessageEvent += new EventHandler<GenericMessageEventArgs>(OnGenericMessageEvent);
            parser.Parse();
            Console.WriteLine("Counts: Tags={0}; Links={1}", parser.TagCount, parser.LinkCount);
        }

        void OnGenericMessageEvent(object sender, GenericMessageEventArgs e)
        {
            this.WriteError(e.Message);
        }

        private void OnDuplicateLinkEvent(object sender, DuplicateLinkEventArgs e)
        {
            this.WriteError("Duplicate link: " + e.Link);
        }

        private void OnDuplicateTagEvent(object sender, DuplicateTagEventArgs e)
        {
            this.WriteError("Duplicate tag: " + e.TagName);
        }

        private void WriteError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR: " + message);
            Console.ResetColor();
        }
    }

    /// <summary>
    /// Arguments for the 'check' action.
    /// </summary>
    internal class CheckArguments
    {
        /// <summary>
        /// Input file to process
        /// </summary>
        [ArgRequired, ArgShortcut("-f"), ArgDescription("Path to linkdb file"), ArgPosition(1)]
        public string FileName{ get; set; }

        [ArgShortcut("-a"), ArgDescription("Perform all checks")]
        public bool CheckAll { get; set; }

        [ArgShortcut("-dt"), ArgDescription("Check duplicate tags")]
        public bool CheckDuplicateTags { get; set; }

        [ArgShortcut("-dl"), ArgDescription("Check duplicate links")]
        public bool CheckDuplicateLinks { get; set; }

    }
}
