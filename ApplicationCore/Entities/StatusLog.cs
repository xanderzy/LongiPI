using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ApplicationCore.Entities
{
    public class StatusLog : BaseEntity
    {
      
        public DateTime TransDate { set; get; }

        public int TopicId { set; get; }
        public int PreStaus { set; get; }
        public int NowStatus { set; get; }
        public string TransBy { set; get; }
        public string TransName { set; get; }
        public string Attr2 { set; get; }
    }
}
