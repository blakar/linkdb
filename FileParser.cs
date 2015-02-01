namespace Blakar.Tools.LinkDb
{
    using System.IO;
    using System;
    using System.Collections.Generic;

    internal class FileParser
    {
        private string fileName;

        private HashSet<string> tags;
        private HashSet<string> links;
        private LinkInfo currentLinkInfo;
        private List<LinkInfo> linkDetails;

        public event EventHandler<DuplicateTagEventArgs> OnDuplicateTagEvent;
        public event EventHandler<DuplicateLinkEventArgs> OnDuplicateLinkEvent;
        public event EventHandler<GenericMessageEventArgs> OnGenericMessageEvent;

        public FileParser(string fileName)
        {
            this.fileName = fileName;
            this.tags = new HashSet<string>();
            this.links = new HashSet<string>();
            this.linkDetails = new List<LinkInfo>();
        }

        public int TagCount
        {
            get
            {
                return tags.Count;
            }
        }

        public int LinkCount
        {
            get
            {
                return links.Count;
            }
        }

        public void Parse()
        {
            using (var sr = new StreamReader(fileName))
            {
                string line = null;
                do
                {
                    line = sr.ReadLine();
                    if (line != null && line.TrimEnd().EndsWith(" \\") && !line.TrimStart().StartsWith("%%"))
                    {
                        // continuation character, we need to read next line as well
                        bool continueReading = true;
                        line = line.Substring(0, line.Length - 1); // remove marker
                        while (continueReading)
                        {
                            var nextLine = sr.ReadLine();
                            continueReading = nextLine == null || nextLine.TrimEnd().EndsWith(" \\");
                            if (nextLine != null)
                            {
                                nextLine = nextLine.Substring(0, nextLine.Length - 1);
                                line += nextLine;
                            }
                        }
                    }

                    this.ProcessFileLine(line);
                }
                while (line != null);
            }
        }

        private void ProcessFileLine(string line)
        {
            if (line != null)
            {
                if (line.StartsWith("%%") || string.IsNullOrEmpty(line))
                {
                    // this is a comment; ignore it
                }
                else if (line.StartsWith("."))
                {
                    var command = SplitDotCommand(line);
                    this.ProcessCommand(command);
                }
            }
        }

        private void ProcessCommand(Tuple<string, string> command)
        {
            var commandType = command.Item1;
            var commandArg = command.Item2;

            switch (commandType.ToLowerInvariant())
            {
                case ".tag":
                    if (this.tags.Contains(commandArg))
                    {
                        if (OnDuplicateTagEvent != null)
                        {
                            OnDuplicateTagEvent(this, new DuplicateTagEventArgs(commandArg));
                        }
                    }
                    this.tags.Add(commandArg);
                    break;

                case ".link":
                    if (this.tags.Contains(commandArg))
                    {
                        if (this.OnDuplicateLinkEvent != null)
                        {
                            this.OnDuplicateLinkEvent(this, new DuplicateLinkEventArgs(commandArg));
                        }
                    }
                    this.links.Add(commandArg);
                    this.currentLinkInfo = new LinkInfo { Link = commandArg };
                    this.linkDetails.Add(this.currentLinkInfo);
                    break;

                case ".added":
                    {
                        var date = ConvertCompactDateStringToDateTime(commandArg);
                        if (date != DateTime.MinValue)
                        {
                            this.currentLinkInfo.Added = date;
                        }
                        else
                        {
                            if (this.OnGenericMessageEvent != null)
                            {
                                this.OnGenericMessageEvent(this, 
                                    new GenericMessageEventArgs("Added command contains invalid date: " + commandArg));
                            }
                        }
                    }
                    break;

                case ".title":
                    this.currentLinkInfo.Title = commandArg;
                    break;

                case ".tags":
                    var tags = commandArg.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var tag in tags)
                    {
                        this.currentLinkInfo.Tags.Add(tag);
                    }
                    break;

                case ".quote":
                    this.currentLinkInfo.Quotes.Add(commandArg);
                    break;

                case ".reddit":
                case ".related":
                    this.currentLinkInfo.Related.Add(commandArg);
                    break;

                case ".read-request":
                    this.currentLinkInfo.ReadRequest = true;
                    break;

                case ".published":
                    {
                        var date = ConvertCompactDateStringToDateTime(commandArg);
                        if (date != DateTime.MinValue)
                        {
                            this.currentLinkInfo.Published = date;
                        }
                        else
                        {
                            if (this.OnGenericMessageEvent != null)
                            {
                                this.OnGenericMessageEvent(this, 
                                    new GenericMessageEventArgs("Published command contains invalid date: " + commandArg));
                            }
                        }
                    }
                    break;

                default:
                    if (this.OnGenericMessageEvent != null)
                    {
                        this.OnGenericMessageEvent(this, new GenericMessageEventArgs("Unknown command type: " + commandType));
                    }
                    break;
            }
        }

        private Tuple<string, string> SplitDotCommand(string line)
        {
            // assumes line starts with a dot, find next blank line
            int pos = line.IndexOf(" ");
            if (pos > -1)
            {
                return new Tuple<string, string>(
                    line.Substring(0, pos), 
                    line.Substring(pos + 1));
            }
            else
            {
                return new Tuple<string, string>(line.Trim(), null);
            }
        }

        private static DateTime ConvertCompactDateStringToDateTime(string date)
        {
            // date is in format YYYYMMDD
            if (string.IsNullOrWhiteSpace(date))
            {
                return DateTime.MinValue;
            }

            date = date.Trim();
            if (date.Length != 8)
            {
                return DateTime.MinValue;
            }

            int year = ConvertToInt(date.Substring(0, 4));
            int month = ConvertToInt(date.Substring(4, 2));
            int day = ConvertToInt(date.Substring(6, 2));

            return new DateTime(year, month, day);
        }

        private static int ConvertToInt(string text, int defaultValue = 0)
        {
            int result;
            return int.TryParse(text, out result) ? result : defaultValue;
        }
    }

    internal class LinkInfo
    {
        public string Link { get; set; }
        public string Title { get; set; }
        public DateTime? Added { get; set; }
        public DateTime? Published { get; set; }
        public List<string> Tags { get; set; }
        public List<string> Quotes { get; set; }
        public bool ReadRequest { get; set; }
        public List<string> Related { get; set; }

        public LinkInfo()
        {
            this.Quotes = new List<string>();
            this.Related = new List<string>();
            this.Tags = new List<string>();
        }
    }
}
