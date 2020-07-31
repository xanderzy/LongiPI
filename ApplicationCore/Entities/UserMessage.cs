using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationCore.Entities
{
    public class UserMessage : BaseEntity
    {
        public string ThirdSession { get; set; }
         
        public string UserName { get; set; }
        
        public string OpenId { get; set; }
        public string Sessionkey { get; set; }
        public string Attr2 { get; set; }
        public DateTime CreateOn { get; set; }
    }
}
