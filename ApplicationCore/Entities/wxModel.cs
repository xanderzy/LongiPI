using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationCore.Entities
{
     public class wxModel
    {
        public string openid { get; set; }
        public string session_key { get; set; }

        public string expires_in { get; set; }

        public string errcode { get; set; }

        public string errmsg { get; set; }
    }
}
