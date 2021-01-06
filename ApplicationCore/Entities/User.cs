using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace ApplicationCore.Entities
{
    public class User:IdentityUser
    {
        public string Department { get; set; }
        public string Avatar { get; set; }
        public string Profile { get; set; }
        public string RealName { get; set; }
        public string GitHub { get; set; }
        public int TopicCount { get; set; }
        public int TopicReplyCount { get; set; }
        public int Score { get; set; }
        public DateTime CreateOn { get; set; }
        public DateTime LastTime { get; set; }

     }
}
