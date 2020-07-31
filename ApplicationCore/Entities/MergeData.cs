using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ApplicationCore.Entities
{
    public class MergeData 
    {
        public int TopicId { get; set; }
        public string TrUserName { get; set; }

        public string TrDep { get; set; }
        public string TrRealName { get; set; }

        public string Title { get; set; }

        public string TDepartment { get; set; }

        public int TopicMark { get; set; }

        public int ZongbuMark { get; set; }

        public int NodeId { get; set; }

        public int Type { get; set; }

        public string UserName { get; set; }

        public string TRealName { get; set; }

        public int ReplyCount { get; set; }

        public DateTime? CreateOn { get; set; }

        public DateTime? PassTime { get; set; }

        public DateTime? FinishTime { get; set; }

    }
   
}
