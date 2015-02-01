using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blakar.Tools.LinkDb
{
    public class DuplicateTagEventArgs : EventArgs
    {
        public DuplicateTagEventArgs(string tagName)
        {
            this.TagName = tagName;
        }

        public string TagName { get; private set; }
    }

    public class DuplicateLinkEventArgs : EventArgs
    {
        public DuplicateLinkEventArgs(string link)
        {
            this.Link = link;
        }

        public string Link { get; private set; }
    }

    public class GenericMessageEventArgs : EventArgs
    {
        public GenericMessageEventArgs(string message)
        {
            this.Message = message;
        }

        public string Message { get; private set; }
    }
}
