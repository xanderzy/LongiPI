using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationCore.Entities
{
    public class TopicReply : BaseEntity
    {
        public int TopicId { get; set; }
        public string ReplyUserId { get; set; }
        public string ReplyUserName { get; set; }
        public User ReplyUser { get; set; }
        public string ReplyEmail { get; set; }
        public string ReplyContent { get; set; }
        public DateTime CreateOn { get; set; }

        public TopicReplyType ReplyType { get; set; }
    }

    public enum TopicReplyType
    {
        Answer = 0,  //代表参与提案回复
        BakcInfo = 1, //代表驳回提案回复 
        BreakInfo=2 //代表中止提案回复
    }
}
