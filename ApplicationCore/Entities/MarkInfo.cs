using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.Entities
{
    public class MarkInfo:BaseEntity
    {

        public int TopicId { set; get; }

        public string MarkBy { set; get; }

        public string MarkUserName { set; get; }

        public DateTime? CreateDate { set; get; }

        public DateTime? MarkDate { set; get; }

        public DateTime? LastEmailDate { set; get; }

        public int Mark { set; get; }

        public int SubMark1 { set; get; }
        public int SubMark2 { set; get; }
        public int SubMark3 { set; get; }
        public int SubMark4 { set; get; }
        public int SubMark5 { set; get; }
        public int SubMark6 { set; get; }

        //1为基地，2为总部
        public int MarkType { set; get; }
    }
}
