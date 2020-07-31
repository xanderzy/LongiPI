using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationCore.Entities
{
    public class MailBox
    {
        //public IEnumerable<IAttachment> Attachments { get; set; }
        public string Body { get; set; }
        public IEnumerable<string> Cc { get; set; }
        public bool IsHtml { get; set; }
        public string Subject { get; set; }
        public IEnumerable<string> To { get; set; }
    }
}
