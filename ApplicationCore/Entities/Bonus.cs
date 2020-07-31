using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ApplicationCore.Entities
{
    public class Bonus : BaseEntity
    {

        public DateTime? FinishTime { set; get; }

        public int TopicId { set; get; }
        public string Title { set; get; }
        public int Mark { set; get; }
        public string UserName { set; get; }
        public string RealName { set; get; }
        public int  Role { set; get; }

        public double Ratio { set; get; }
        public double Amount { set; get; }

        public DateTime? TransDate { set; get; }

        public string Attr2 { set; get; }
    }
}
