using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ApplicationCore.Entities
{
        public class Topic : BaseEntity
        {
            [Required]
            public int NodeId { get; set; }
            public TopicNode Node { get; set; }
            [Required]
            public string UserId { get; set; }
            public string UserName { get; set; }
            public User User { get; set; }
            public string Email { get; set; }
            [Required(ErrorMessage = "不能为空")]
            [MaxLength(20)]
            public string Title { get; set; }
            [Required]
            public string Content { get; set; }
            [Required]
            public string Suggest { get; set; }
            public string TeamLeader { get; set; }
            /// <summary>
            /// 置顶权重
            /// </summary>
            public int Top { get; set; }
            public TopicType Type { get; set; }
            public int ViewCount { get; set; }
            public int ReplyCount { get; set; }
            public string LastReplyUserId { get; set; }
            public User LastReplyUser { get; set; }
            public DateTime LastReplyTime { get; set; }
            public DateTime CreateOn { get; set; }
            public DateTime? PassTime { get; set; }
            public DateTime? FinishTime { get; set; }

            public string HasUpload { get; set; }

            public int TopicMark { get; set; }

            public int ZongbuMark { get; set; }
        public virtual List<TopicReply> Replys { get; set; }
        }
        public enum TopicType
        {
            Delete = 0,  //代表驳回状态
            Normal = 1,//代表未审核,发布以后 
            TeamLeaderCheck = 2, //处于派发状态
            AdminCheck = 3,//代表管理员审核状态
            Good = 4,    //代表未打分-实施中
            Top = 5,   //代表已完结未打分
            Marking=7,   //代表已派发打分中
            Perfect = 6, //代表已完结已打分
            Notice=10,//代表公告
            Breakup=11//代表中止状态
        }
}
