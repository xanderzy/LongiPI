using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationCore.Entities
{
    public class UserCollection : BaseEntity
    {
        public string UserId { get; set; }
        public int TopicId { get; set; }
        public Topic Topic { get; set; }
        public int State { get; set; }
        public DateTime CreateOn { get; set; }
    }
}
