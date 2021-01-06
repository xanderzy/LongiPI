using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using PI.Utils;
using PI.ViewModels;

namespace PI.Controllers
{
    public class TopicController : Controller
    {
        private ITopicRepository _topic;
        private UserManager<User> UserManager;
        private ITopicReplyRepository _reply;
        private IStatusLogRepository _statuslog;
        private IMyFileRepository _myfile;
        private IHostingEnvironment _hostingEnv;
        private readonly DataContext _context;
        private IMailQueueService _mailservice;
        private IMarkInfoRepository _markinfo;

        public TopicController(ITopicRepository topic, ITopicReplyRepository reply, UserManager<User> userManager, IMyFileRepository myfile, IHostingEnvironment hostingEnv, DataContext context, IMailQueueService mailservice, IMarkInfoRepository markinfo, IStatusLogRepository statuslog)
        {
            _topic = topic;
            _reply = reply;
            _myfile = myfile;
            UserManager = userManager;
            _hostingEnv = hostingEnv;
            _context = context;
            _markinfo = markinfo;
            _statuslog = statuslog;
            _mailservice = mailservice;
        }

        //Topic的主信息页面
        public IActionResult Index(int id)
        {
            if (id <= 0) return Redirect("/");
            var topicdetail = _topic.GetById(id);
            //string testtype = topicdetail.Type.ToString();
            if (topicdetail == null) return Redirect("/");
            var replys = _reply.List(r => r.TopicId == id).ToList();
            var ffiles = _myfile.List(r => r.TopicId == id&&r.FileIcon=="F"&&r.IsDelete==0).ToList();
            var wfiles = _myfile.List(r => r.TopicId == id&&r.FileIcon == "W"&&r.IsDelete==0).ToList();
            ViewBag.Replys = replys;
            ViewBag.FFiles = ffiles;
            ViewBag.WFiles = wfiles;
            return View(topicdetail);
        }


        public IActionResult detail(int id)
        {
            var topic = _topic.GetById(id);
            string str = JsonConvert.SerializeObject(new { code = 0, msg = "查询成功", data = topic });
            return Content(str);
        }

        //获取流程数据
        public IActionResult GetStatusLog(int slid)
        {
            var lcdata = from a in _context.StatusLogs
                         join ar in _context.Users
                         on a.TransBy equals ar.UserName
                         orderby a.TransDate
                         where a.TopicId == slid
                         select new
                         {
                             a.Id,
                             a.NowStatus,
                             a.TopicId,
                             ar.RealName,
                             a.TransName,
                             a.TransDate,
                             a.Attr2
                         };

            var sllist = lcdata.ToList();
            var total = sllist.Count();
            string str = JsonConvert.SerializeObject(new { code = 0, msg = "查询成功", count = total, data = sllist });
            return Content(str);


        }

            //审核页面的内容
            public IActionResult CheckByTeamLeader(int id)
            {
            if (id <= 0) return Redirect("/");
            var checktopicdetail = _topic.GetById(id);
            if (checktopicdetail == null) return Redirect("/");
            var checktopicfiles = _myfile.List(r => r.TopicId == id).ToList();
            ViewBag.CFiles = checktopicfiles;
            return View(checktopicdetail);
            }

        //审核页面-之前添加的用户信息读取
        public IActionResult ReplyUserInfo(int id)
        {
            try 
            { 
                var checkreplys = (from t in _context.TopicReplys
                               join u in _context.Users
                               on t.ReplyUserId equals u.Id
                               orderby t.CreateOn descending
                               where t.TopicId == id
                               select new
                               {
                                   username = u.UserName,
                                   realname = u.RealName,
                                   dep = u.Department,
                                   rid = t.Id
                               }).ToList();
            string str = JsonConvert.SerializeObject(new { success = true, data = checkreplys });
            return Content(str);
            }
            catch(Exception ex)
            {
                string str = JsonConvert.SerializeObject(new { success = false, data = ex.ToString() });
                return Content(str);
            }
        }




        //添加实施人-审核/实施人添加实施人
        public IActionResult AddReplybyTeamLeader(string username, int topicid, string addbys)
        {
            string exinfo = "";
            string str = "";
            int rid = 0;
            //首先 将username进行序列化，然后进行循环验证-验证所有用户名没有问题 -没问题进行实施人添加，然后进行通过
            //var users = _context.Users.AsQueryable();
                var isuserexist = _context.Users.Where(r => r.RealName == username).Select(r => new
                {
                    r.Id,
                    r.UserName,
                    r.RealName,
                    r.Email,
                    r.Department
                }).ToList();
                int sss = isuserexist.Count();
                if (topicid == 0)
                {
                    exinfo = "提案ID异常，无法添加";
                    str = JsonConvert.SerializeObject(new { success = false, ex = exinfo });
                    return Content(str);
                }
                if (sss > 1)
                {
                    exinfo = "系统内存在同名用户超过两个人，请联系管理员修改";
                    str = JsonConvert.SerializeObject(new { success = false, ex = exinfo });
                    return Content(str);
                }
                if (sss == 0)
                {
                    exinfo = "此用户不存在,请检查输入的用户";
                    str = JsonConvert.SerializeObject(new { success = false, ex = exinfo });
                    return Content(str);
                }
                bool isreply = _reply.List(r => r.TopicId == topicid && r.ReplyUserName == isuserexist[0].UserName).Any();
                if (isreply)
                {
                    exinfo = "此实施人已添加，请勿重复添加！";
                    str = JsonConvert.SerializeObject(new { success = false, ex = exinfo });
                    return Content(str);
                }
                try
                {
                    rid = _reply.Add(new TopicReply
                    {
                        ReplyUserId = isuserexist[0].Id,
                        CreateOn = DateTime.Now,
                        ReplyEmail = isuserexist[0].Email,
                        ReplyUserName = isuserexist[0].UserName,
                        ReplyContent = "我参与此提案并进行改善",
                        TopicId = topicid,
                        ReplyType = TopicReplyType.Answer
                    });
                    if (addbys != null)
                    {
                        _statuslog.Add(new StatusLog
                        {
                            TopicId = topicid,
                            TransDate = DateTime.Now,
                            TransBy = addbys,
                            TransName = "AddNewRUser",
                            PreStaus = Convert.ToInt16(TopicType.TeamLeaderCheck),
                            NowStatus = Convert.ToInt16(TopicType.TeamLeaderCheck),
                            Attr2 = username
                        });
                    }
                }
                catch (Exception ex)
                {
                    exinfo = "添加失败，出现未知异常,请联系管理员";
                    str = JsonConvert.SerializeObject(new { success = false, ex = ex.ToString() });
                    return Content(str);
                }
            
            var topic = _topic.GetById(topicid);
            topic.LastReplyTime = DateTime.Now;
            topic.ReplyCount += 1;
            _topic.Edit(topic);
            exinfo = "添加成功";
            str = JsonConvert.SerializeObject(new { success = true, ex = exinfo, username = isuserexist[0].UserName, realname = isuserexist[0].RealName, dep = isuserexist[0].Department, rid = rid });
            return Content(str);
        }

        //更改审核人
        public IActionResult ChangeTeamLeader(string newtl, int tid)
        {
            string resstr = "";
            try {

            var topic = _topic.GetById(tid);
            string oldtl = topic.TeamLeader;
            topic.TeamLeader = newtl;
            _topic.Edit(topic);
                //状态添加
             _statuslog.Add(new StatusLog
                {
                    TopicId = tid,
                    TransDate = DateTime.Now,
                    TransBy = oldtl,
                    TransName = "ChangeTL",
                    PreStaus = Convert.ToInt16(TopicType.TeamLeaderCheck),
                    NowStatus = Convert.ToInt16(TopicType.TeamLeaderCheck),
                    Attr2=newtl
              });
            resstr = JsonConvert.SerializeObject(new { code = 0,msg="转办成功"});
            return Content(resstr);
            }
            catch (Exception ex)
            {
                resstr = JsonConvert.SerializeObject(new { code = 1, msg =ex.ToString() });
                return Content(resstr);
            }
        }

         //删除添加的人员
         public IActionResult DeleteReply(int replyid)
        {
            string exinfo = "";
            string str = "";
            var trtodel = _reply.GetById(replyid);
            if (trtodel == null)
            {
                exinfo = "数据不存在";
                str = JsonConvert.SerializeObject(new { success = false, ex = exinfo });
                return Content(str);
            }
            _reply.Delete(trtodel);
            int tid = trtodel.TopicId;
            var dtopic = _topic.GetById(tid);
            dtopic.ReplyCount -= 1;
            _topic.Edit(dtopic);
            exinfo = "删除成功";
            str = JsonConvert.SerializeObject(new { success = true, ex = exinfo });
            return Content(str);
        }

        //中止提案
        public IActionResult BreakTian(int bid,string breakreason,string bkusername)
        {
            string exin = "";
            string ss = "";
            var topic = _topic.GetById(bid);
            if (topic == null)
            {
                exin = "出现异常，提案为空";
                ss = JsonConvert.SerializeObject(new { success = false, exinfo = exin });
                return Content(ss);
            }
            var u = UserManager.FindByNameAsync(bkusername).Result;
            topic.LastReplyUserId = u.Id;
            topic.Type = TopicType.Breakup;
            topic.LastReplyTime = DateTime.Now;
            _topic.Edit(topic);
            var replys = _context.TopicReplys.Where(r => r.TopicId == bid).ToList();
            _context.TopicReplys.RemoveRange(replys);
            _context.SaveChanges();
            _reply.Add(
                   new TopicReply
                   {
                       ReplyUserId = u.Id,
                       CreateOn = DateTime.Now,
                       ReplyEmail = u.Email,
                       ReplyUserName = u.UserName,
                       ReplyContent = breakreason,
                       TopicId = bid,
                       ReplyType = TopicReplyType.BreakInfo
                  });
            _statuslog.Add(new StatusLog
            {
                TopicId = bid,
                TransDate = DateTime.Now,
                TransBy = bkusername,
                TransName = "BreakTL",
                PreStaus = Convert.ToInt16(TopicType.Good),
                NowStatus = Convert.ToInt16(TopicType.Breakup),
                Attr2= breakreason
            });
            exin = "中止成功";
            ss = JsonConvert.SerializeObject(new { success = true, exinfo = exin });
            return Content(ss);
        }

        //通过提案审核-添加完人员后-通过提案进入实施状态-发送邮件提醒
        public IActionResult UpdateToTop(int id)
        {
            string exin = "";
            string ss = "";
            var topic = _topic.GetById(id);
            if (topic == null)
            {
                exin = "出现异常，提案为空";
                ss = JsonConvert.SerializeObject(new { success = false, exinfo = exin });
                return Content(ss);
            }
            var reply = _reply.List(r => r.TopicId == id).ToList();
            string temaillist = "";
            string tonamelist = "";
            string adduserlist = "";
            foreach (var item in reply)
            {
                var u = UserManager.FindByNameAsync(item.ReplyUserName).Result;
                adduserlist = adduserlist == "" ? u.RealName : adduserlist + "," + u.RealName;
                if (u.Email == "pi@longi-silicon.com") { continue; }
                if (temaillist == "")
                {
                    temaillist = u.Email;
                    tonamelist = u.RealName;
                }
                else
                {
                    temaillist = temaillist + "," + u.Email;
                    tonamelist = tonamelist + "," + u.RealName;
                }
            }
            //如果没有邮件的人-就不发送邮件了
            if (temaillist != "") {
            MailBox mymail = new MailBox();
            mymail.Subject = "您参与了提案：" + topic.Title;
            
            mymail.Body = @"<p>" + tonamelist + "您好：</p>" +
                        "<p>您参与了此提案：<a href=\"http://10.12.0.154/Topic/Index/" + id + "\"  target=\"_blank\">" + topic.Title + "</a></p>" +
                        "<p>请尽快完成改善，完成后请上传报告进行提案完结。</p>";
            mymail.IsHtml = true;
            mymail.To = temaillist.Split(',');
            _mailservice.Enqueue(mymail);
            }
            topic.Type = TopicType.Good;
            topic.PassTime = DateTime.Now;
            topic.LastReplyTime = DateTime.Now;
            _topic.Edit(topic);
            _statuslog.Add(new StatusLog
            {
                TopicId = id,
                TransDate = DateTime.Now,
                TransBy = topic.TeamLeader,
                TransName = "PassTL",
                PreStaus = Convert.ToInt16(TopicType.TeamLeaderCheck),
                NowStatus = Convert.ToInt16(TopicType.Good),
                Attr2= adduserlist
            });
            exin = "通过成功";
            ss = JsonConvert.SerializeObject(new { success = true, exinfo = exin });
            return Content(ss);
        }


        //驳回提案
        public IActionResult HideTopic(string hidereason, int htid,string opeuser)
        {
            string exin = "";
            string ss = "";
            if (opeuser =="admin")
            {
                var delreply = _context.TopicReplys.Where(r =>r.TopicId==htid).ToList();
                _context.TopicReplys.RemoveRange(delreply);
                _context.SaveChanges();
            }
            var ru=UserManager.FindByNameAsync(opeuser).Result; 
            try
            {
                _reply.Add(
                    new TopicReply
                    {
                        ReplyUserId = ru.Id,
                        CreateOn = DateTime.Now,
                        ReplyEmail = ru.Email,
                        ReplyUserName = ru.UserName,
                        ReplyContent = hidereason,
                        TopicId = htid,
                        ReplyType = TopicReplyType.BakcInfo
                    });
                _statuslog.Add(new StatusLog
                {
                    TopicId = htid,
                    TransDate = DateTime.Now,
                    TransBy = ru.UserName,
                    TransName = "HideTopic",
                    PreStaus = Convert.ToInt16(TopicType.TeamLeaderCheck),
                    NowStatus = Convert.ToInt16(TopicType.Delete),
                    Attr2=hidereason
                });
                var topic = _topic.GetById(htid);

                var topicuser = UserManager.FindByNameAsync(topic.UserName).Result;


                //发送驳回邮件
                MailBox mymail = new MailBox();
                mymail.Subject = "您的提案：" + topic.Title;
                mymail.Body = @"<p>" + topicuser.RealName + "您好：</p>" +
                            "<p>您的提案：<a href=\"http://10.6.6.199/Topic/Index/" + htid + "\"  target=\"_blank\">" + topic.Title + "</a>已被驳回。</p>" +
                            "<p>驳回理由：" + hidereason + "</p>" +
                            "<p>审核人：" + hidereason + "</p>";
                mymail.IsHtml = true;
                mymail.To = topicuser.Email.Split(',');
                _mailservice.Enqueue(mymail);


                topic.LastReplyUserId = ru.Id;
                topic.LastReplyTime = DateTime.Now;
                topic.Type = TopicType.Delete;
                _topic.Edit(topic);
                exin = "驳回成功";
                ss = JsonConvert.SerializeObject(new { success = true, exinfo = exin });
                return Content(ss);
            }
            catch (Exception ex)
            {
                ss = JsonConvert.SerializeObject(new { success = false, exinfo = ex.ToString() });
                return Content(ss);
            }
        }

        //完结提案
        public async Task<IActionResult> FinishTa(int wid,string transby,string wfileid,string deluser,string delusername)
        {
            string ss = "";
            string exin = "";
            var topic = _topic.GetById(wid);
            var pretype = TopicType.Good;
            if (topic is null)
            {
                exin = "出现未知异常，请联系管理员";
                ss = JsonConvert.SerializeObject(new { success = false, exinfo = exin });
                return Content(ss);
            }
            try
            {
                //先删除人员
                int shanchucount = 0;
                if (deluser!="0"&&deluser!=null)
                {
                    string[] delarr = deluser.Split(',');
                    int rlistcount = _reply.List(r => r.TopicId == wid).ToList().Count();
                    //判断删除的数量
                    if(delarr.Length== rlistcount)
                    {
                        exin = "不能删除所有参与人员！！！！";
                        ss = JsonConvert.SerializeObject(new { success = false, exinfo = exin });
                        return Content(ss);
                    }
                    else {
                        shanchucount = delarr.Length;
                        List<int> dellist = new List<int>();
                        for (int i = 0; i < delarr.Length; i++)
                        {
                        dellist.Add(Convert.ToInt32(delarr[i]));
                        }
                        var delreply = _context.TopicReplys.Where(r => dellist.Contains(r.Id)).ToList();
                        _context.TopicReplys.RemoveRange(delreply);
                        _context.SaveChanges();

                        _statuslog.Add(new StatusLog
                        {
                            TopicId = wid,
                            TransDate = DateTime.Now,
                            TransBy = transby,
                            TransName = "DeleteRuser",
                            PreStaus = Convert.ToInt16(pretype),
                            NowStatus = Convert.ToInt16(pretype),
                            Attr2= delusername
                        });
                    }
                }

                //更新提案人员
                var topicuser = UserManager.FindByNameAsync(topic.UserName).Result;
                topicuser.TopicCount += 1;
                await UserManager.UpdateAsync(topicuser);
                //更新人员改善数量 replycount
                var list = _reply.List(r => r.TopicId == wid).ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    var replyuser = UserManager.FindByNameAsync(item.ReplyUserName).Result;
                    replyuser.TopicReplyCount += 1;
                    await UserManager.UpdateAsync(replyuser);
                }

                //更新提案Topic数据
                if (wfileid != "0"){ 
                topic.HasUpload = topic.HasUpload == "0" ? wfileid : topic.HasUpload + "," + wfileid;
                    //更新完结文件绑定的topicid
                    string[] fileidlist = wfileid.Split(',');
                    for (int j = 0; j < fileidlist.Length; j++)
                    {
                        var editfile = _myfile.List(r => r.Id == Convert.ToInt32(fileidlist[j])).First();
                        editfile.TopicId = wid;
                        _myfile.Edit(editfile);
                    }
                }
                else
                {
                    pretype = TopicType.AdminCheck;
                }
                topic.Type = TopicType.Top;
                //删除提案参与人数
                topic.ReplyCount -= shanchucount;
                if(topic.PassTime is null)
                {
                    topic.PassTime = DateTime.Now;
                }
                topic.FinishTime = DateTime.Now;
                topic.LastReplyTime = DateTime.Now;
                _topic.Edit(topic);


                //添加日志
                _statuslog.Add(new StatusLog
                {
                    TopicId = wid,
                    TransDate = DateTime.Now,
                    TransBy = transby,
                    TransName = "FinishTopic",
                    PreStaus = Convert.ToInt16(pretype),
                    NowStatus = Convert.ToInt16(TopicType.Top)
                });
                exin = "完结成功,2秒后自动刷新页面";
                ss = JsonConvert.SerializeObject(new { success = true, exinfo = exin });
                return Content(ss);
            }
            catch (Exception ex)
            {
                exin = ex.ToString();
                ss = JsonConvert.SerializeObject(new { success = false, exinfo = exin });
                return Content(ss);
            }
        }
        
        //删除未审核的提案
        public IActionResult DeleteWet(int deleteid)
        {
            string ss = "";
            string exin = "";
            var topic = _topic.GetById(deleteid);
            if (topic != null)
            {
                try
                {
                    _topic.Delete(topic);
                    var replys = _context.TopicReplys.Where(r => r.TopicId == deleteid).ToList();
                    if (replys != null) {
                    _context.TopicReplys.RemoveRange(replys);
                    _context.SaveChanges();
                    }
                    exin = "删除成功";
                    ss = JsonConvert.SerializeObject(new { success = true, exinfo = exin });
                    return Content(ss);
                }
                catch (Exception ex)
                {
                    exin = "删除失败：" + ex.ToString();
                    ss = JsonConvert.SerializeObject(new { success = false, exinfo = exin });
                    return Content(ss);
                }
            }
            else
            { 
                exin = "未找到相关提案";
                  ss = JsonConvert.SerializeObject(new { success = false, exinfo = exin });
                return Content(ss);
            }
        }
        //部门对接人打分页面信息显示
        public IActionResult FirstMark(int id)
        {
            if (id <= 0) return Redirect("/");
            var marktopicdetail = _topic.GetById(id);
            if (marktopicdetail == null) return Redirect("/");
             var mffiles = _myfile.List(r => r.TopicId == id && r.FileIcon == "F").ToList();
            var mwfiles = _myfile.List(r => r.TopicId == id && r.FileIcon == "W").ToList();
            ViewBag.MFFiles = mffiles;
            ViewBag.MWFiles = mwfiles;
             return View(marktopicdetail);
        }


        //打分页面信息显示
        public IActionResult MarkByLeader(int id)
        {
            if (id <= 0) return Redirect("/");
            var marktopicdetail = _topic.GetById(id);
            if (marktopicdetail == null) return Redirect("/");
            //var markreplys = _reply.List(r => r.TopicId == id).ToList();
            var mffiles = _myfile.List(r => r.TopicId == id && r.FileIcon == "F").ToList();
            var mwfiles = _myfile.List(r => r.TopicId == id && r.FileIcon == "W").ToList();
            ViewBag.MFFiles = mffiles;
            ViewBag.MWFiles = mwfiles;
            //ViewBag.MReplys = markreplys;
            return View(marktopicdetail);
        }
        /*
        //评分人员评分
        public IActionResult MakeMark(int id,int markv)
        {
            try {
            var nu= UserManager.GetUserAsync(User).Result;
            //先直接评分
            var usermark = _markinfo.List(r => r.TopicId == id && r.MarkBy == nu.UserName).FirstOrDefault();
            usermark.Mark = markv;
            usermark.MarkDate = DateTime.Now;
            _markinfo.Edit(usermark);
            //然后判断是否是最后一个
            var pdtopic = _topic.GetById(id);
            if (pdtopic.TopicMark == 0) {
             pdtopic.TopicMark = markv;
            }else {
             double d =(pdtopic.TopicMark + markv) / 2;
             pdtopic.TopicMark = Convert.ToInt16(Math.Round(d, 0));
             }
            var indexmark = _markinfo.List(r => r.Mark == 1001 && r.TopicId == id).Any();
            if (indexmark is false)
            {
                //如果没有等于0的了，说明全部打分完毕
                if (pdtopic.TopicMark <= 60) {
                       pdtopic.Type = TopicType.Perfect;
                    }
                /*
                    else
                    {
                        //发送邮件-提醒走流程
                        MailBox mymail = new MailBox();
                        mymail.Subject = pdtopic.Title + "提案总部打分流程提醒";
                    }
            }
            _topic.Edit(pdtopic);
            string backinfo = JsonConvert.SerializeObject(new { success = true, exinfo ="打分成功" });
            return Content(backinfo);
            }
            catch(Exception ex)
            {
                string ebackinfo = JsonConvert.SerializeObject(new { success = false, exinfo = ex.ToString()});
                return Content(ebackinfo);
            }
        }*/


    }
}