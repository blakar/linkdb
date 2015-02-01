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

            parser.CheckConsistency();
        }

        [ArgActionMethod, ArgDescription("Calculate dates with respect to Unix epoch")]
        public void Epoch(EpochArguments args)
        {
            if (args.Date == null)
            {
                args.Date = DateTime.Today;
            }

            var epochTime = GetEpochTime(args.Date.Value);

            Console.WriteLine("Epoch time for {0} is {1}", args.Date.Value.ToString("yyyy-MM-dd"), epochTime);
        }

        void OnGenericMessageEvent(object sender, GenericMessageEventArgs e)
        {
            this.Write(e.Message, e.Severity);
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
            Write(message, Severity.Error);
        }

        private void Write(string message, Severity severity)
        {
            string prefix = "";

            switch (severity)
            {
                case Severity.Information:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    prefix = "INFO: ";
                    break;
                case Severity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    prefix = "WARN: ";
                    break;
                case Severity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    prefix = "ERROR: ";
                    break;
            }

            Console.WriteLine(prefix + message);
            Console.ResetColor();
        }

        private int GetEpochTime(DateTime dateTime)
        {
            TimeSpan t = dateTime - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;
            return secondsSinceEpoch;
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

    internal class EpochArguments
    {
        [ArgShortcut("-d"), ArgPosition(1)]
        public DateTime? Date { get; set; }
    }
}
