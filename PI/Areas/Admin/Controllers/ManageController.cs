using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using PI.Utils;
using PI.ViewModels;

namespace PI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize("Admin")]
    public class ManageController : Controller
    {
        private ITopicRepository _topic;
        private ITopicReplyRepository _reply;
        private IMarkInfoRepository _markinfo;
        private IMyFileRepository _myfile;
        private IBonusRepository _bonus;
        public UserManager<User> UserManager { get; }
        private readonly DataContext _context;
        private IMailQueueService _mailservice;
        private IStatusLogRepository _statuslog;
        public ManageController(ITopicRepository topic, UserManager<User> userManager, DataContext context, ITopicReplyRepository reply, IMarkInfoRepository markinfo, IMailQueueService mailservice, IMyFileRepository myfile, IStatusLogRepository statuslog, IBonusRepository bonus)
        {
            _topic = topic;
            UserManager = userManager;
            _context = context;
            _reply = reply;
            _bonus = bonus;
            _markinfo = markinfo;
            _mailservice = mailservice;
            _myfile = myfile;
            _statuslog = statuslog;
        }

        public IActionResult Index()
        {
            return View();
        }

        //获取派发列表
        public IActionResult GetPaifaRpt(int page, int limit)
        {
            var result = from a in _context.Topics
                         join b in _context.Users
                         on a.UserName equals b.UserName
                         where (a.Type ==TopicType.AdminCheck||a.Type==TopicType.TeamLeaderCheck)&& a.TeamLeader=="Admin"
                         orderby a.CreateOn descending
                         select new
                         {
                             a.Id,
                             a.UserName,
                             b.Department,
                             b.RealName,
                             a.Title,
                             a.Type,
                             a.CreateOn,
                             a.Content,
                             a.Suggest,
                             a.HasUpload
                         };
            var list = result.ToList();
            var total = list.Count();
            var rows = list.Skip((page - 1) * limit).Take(limit).OrderByDescending(r => r.CreateOn).ToList();
            string str = JsonConvert.SerializeObject(new { code = 0, msg = "查询成功", count = total, data = rows });
            return Content(str);
        }


        //派发功能-管理员派发
        public IActionResult DistributeTeamLeader(int TopicId, string TeamLeader)
        {
            string dtexin = "";
            string dtss = "";
            string aa = TeamLeader;
            if (!string.IsNullOrEmpty(TeamLeader))
            {
                var users = _context.Users.AsQueryable();
                var isuserexist = users.Where(r => r.RealName == TeamLeader).Select(r => r.UserName).ToList();
                int sss = isuserexist.Count();
                if (sss == 0)
                {
                    dtexin = "此用户不存在";
                    dtss = JsonConvert.SerializeObject(new { success = false, exinfo = dtexin });
                    return Content(dtss);
                }
                if (sss > 1)
                {
                    dtexin = "此用户在用户列表里有两个人名存在,请修改";
                    dtss = JsonConvert.SerializeObject(new { success = false, exinfo = dtexin });
                    return Content(dtss);
                }
                string tname = isuserexist.FirstOrDefault();
                var u = UserManager.FindByNameAsync(tname).Result;
                var topic = _topic.GetById(TopicId);
                //邮件功能-将邮件入队
                MailBox mymail = new MailBox();
                mymail.Subject= "请审核提案-" + topic.Title;
                mymail.Body= @"<p>" + u.RealName + "您好：</p>" +
                           "<p>请审核提案：<a href=\"http://10.6.6.199/User/Mycheck \" target=\"_blank\">" + topic.Title + "</a></p>";
                mymail.IsHtml = true;
                mymail.To = u.Email.Split(',');
                _mailservice.Enqueue(mymail);
                topic.TeamLeader = tname;
                topic.Type = TopicType.TeamLeaderCheck;
                _topic.Edit(topic);
                dtexin = "派发成功";

                _statuslog.Add(new StatusLog
                {
                    TopicId = TopicId,
                    TransDate = DateTime.Now,
                    TransBy = "Admin",
                    TransName = "ChangeTL",
                    PreStaus = Convert.ToInt16(TopicType.AdminCheck),
                    NowStatus = Convert.ToInt16(TopicType.TeamLeaderCheck),
                    Attr2 = TeamLeader
                });

                dtss = JsonConvert.SerializeObject(new { success = true, exinfo = dtexin });
                return Content(dtss);
            }
            else
            {
                dtexin = "请输入正确的审核人名字";
                dtss = JsonConvert.SerializeObject(new { success = false, exinfo = dtexin });
                return Content(dtss);
            }
        }
 
        //驳回页面
        public IActionResult HideIndex()
        {
            return View();
        }

        //获取驳回页面
        public IActionResult GetHideIndex(int page, int limit)
        {
            var hresult = from a in _context.Topics
                          join b in _context.Users
                          on a.UserName equals b.UserName
                          orderby a.CreateOn descending
                          where a.Type == TopicType.Delete
                          select new
                          {
                              a.Id,
                              a.UserName,
                              b.Department,
                              b.RealName,
                              a.Title,
                              a.CreateOn,
                              a.HasUpload
                          };
            var hlist = hresult.ToList();
            var total = hlist.Count();
            var rows = hlist.Skip((page - 1) * limit).Take(limit).OrderByDescending(r => r.CreateOn).ToList();
            string hstr = JsonConvert.SerializeObject(new { code = 0, msg = "查询成功", count = total, data = rows });
            return Content(hstr);
        }

        //查看驳回理由0 0 贼麻烦
        public IActionResult HideReason(int id)
        {
            var replys = _reply.List(r => r.TopicId == id).Select(r=>new {
             r.ReplyUserName,
             BohuiReal=r.ReplyUser.RealName,
             r.ReplyContent
            }).ToList();
            var backreason = JsonConvert.SerializeObject(new { code = 0, msg = "查询成功", data = replys });
            return Content(backreason);
        }

        //OA流程
        public IActionResult OAChange(int oaid)
        {
            var oatopic = _topic.GetById(oaid);
            oatopic.Top = 1;
            _topic.Edit(oatopic);
            string backreason = JsonConvert.SerializeObject(new { code = 0, msg = "更新成功" });
            return Content(backreason);
        }


        //恢复提案
        public IActionResult BackToNormal(int hideid)
        {
            string bexin = "";
            string bss = "";
            var topic = _topic.GetById(hideid);
            if (topic.Type == TopicType.Delete)
            {
                try
                {
                    topic.Type = TopicType.Normal;
                    topic.ReplyCount = 0;
                    _topic.Edit(topic);
                    var replys = _context.TopicReplys.Where(r => r.TopicId == hideid).ToList();
                    _context.TopicReplys.RemoveRange(replys);
                    _context.SaveChanges();
                    bexin = "恢复成功";
                    bss = JsonConvert.SerializeObject(new { success = true, exinfo = bexin });
                    return Content(bss);
                }
                catch (Exception)
                {
                    bexin = "更新失败";
                    bss = JsonConvert.SerializeObject(new { success = false, exinfo = bexin });
                    return Content(bss);
                }
            }
            bexin = "此提案状态异常，请联系管理员";
            bss = JsonConvert.SerializeObject(new { success = false, exinfo = bexin });
            return Content(bss);
        }


        //永久删除提案
        public IActionResult DeleteMust(int deleteid)
        {
            string dexin = "";
            string dss = "";
            var topic = _topic.GetById(deleteid);
            if (topic != null)
            {
                try
                {
                    _topic.Delete(topic);
                    var replys = _context.TopicReplys.Where(r => r.TopicId == deleteid).ToList();
                    _context.TopicReplys.RemoveRange(replys);
                    _context.SaveChanges();
                    dexin = "删除成功";
                    dss = JsonConvert.SerializeObject(new { success = true, exinfo = dexin });
                    return Content(dss);
                }
                catch (Exception)
                {
                    dexin = "删除失败";
                    dss = JsonConvert.SerializeObject(new { success = false, exinfo = dexin });
                    return Content(dss);
                }
            }
            dexin = "提案状态异常";
            dss = JsonConvert.SerializeObject(new { success = false, exinfo = dexin });
            return Content(dss);
        }
       

        //转移实施人页面
        public IActionResult ChangeRuser()
        {
            return View();
        }


        //获取所有实施人提案
        public IActionResult GetAllRuser(int limit, int page, string title, string username)
        {
            var result = from a in _context.TopicReplys
                         join ar in _context.Users
                         on a.ReplyUserId equals ar.Id
                         join c in _context.Topics
                         on a.TopicId equals c.Id
                         where c.Type==TopicType.Good
                         select new
                         {
                             a.Id,
                             a.ReplyUserName,
                             ar.RealName,
                             c.CreateOn,
                             c.PassTime,
                             c.Title,
                             tid=c.Id
                         };
            if (!string.IsNullOrEmpty(title)) { result = result.Where(r => r.Title.Contains(title)); }
            if (!string.IsNullOrEmpty(username)) { result = result.Where(r => r.RealName == username); }
            var list = result.OrderByDescending(r => r.CreateOn).ToList();
            var total = list.Count();
            var rows = list.Skip((page - 1) * limit).Take(limit).ToList();
            string str = JsonConvert.SerializeObject(new { code = 0, msg = "查询成功", count = total, data = rows });
            return Content(str);
        }

        //更換實施人信息
        public IActionResult ChangeUserInfo(int rid,string newruser)
        {
            string str = "";
            var newuser = _context.Users.Where(r => r.RealName == newruser).FirstOrDefault();
            if(newuser is null)
            {
                str = JsonConvert.SerializeObject(new { success = true, msg = "未查到此人員" });
                return Content(str);
            }
            var cnreply = _reply.GetById(rid);
            if(cnreply is null)
            {
                str = JsonConvert.SerializeObject(new { success = true, msg = "數據異常，請聯係管理員" });
                return Content(str);
            }
            cnreply.ReplyEmail = newuser.Email;
            cnreply.ReplyUserId = newuser.Id;
            cnreply.ReplyUserName = newuser.UserName;
            cnreply.CreateOn = DateTime.Now;
            _reply.Edit(cnreply);

            str = JsonConvert.SerializeObject(new { success = true, msg = "转移实施人成功"});
            return Content(str);

        }

        //完结提案驳回-需接触完结报告的绑定
        public IActionResult BackToGood(int tid, string backreason)
        {
            try {
                //循环解除绑定
                var wfileinfo = _myfile.List(r => r.TopicId == tid).ToList();
                foreach(var item in wfileinfo)
                {
                    item.TopicId = 0;
                    _myfile.Edit(item);
                }
 

            var topic = _topic.GetById(tid);
            topic.Type = TopicType.Good;
            _topic.Edit(topic);
                _statuslog.Add(new StatusLog
                {
                    TopicId = tid,
                    TransDate = DateTime.Now,
                    TransBy ="Admin",
                    TransName = "Chehui",
                    PreStaus =5,
                    NowStatus=4,
                    Attr2 = backreason
                });
            string str = JsonConvert.SerializeObject(new { success = true, msg = "驳回成功" });
            return Content(str);
            }
            catch(Exception ex)
            {
                string str = JsonConvert.SerializeObject(new { success = false, msg = ex.ToString() });
                return Content(str);
            }
        }

        //获取文件列表
        public IActionResult GetTopicFilelist(int topicid)
        {
            int a = topicid;
            var ffiles = _myfile.List(r => r.TopicId == topicid && r.FileIcon == "F" && r.IsDelete == 0).ToList();
            var wfiles = _myfile.List(r => r.TopicId == topicid && r.FileIcon == "W" && r.IsDelete == 0).ToList();
            string resstr = JsonConvert.SerializeObject(new { code = "0", msg = "查询成功", ffdata = ffiles,wfdata= wfiles });
            return Content(resstr);

        }

        //奖金报表页面
        public IActionResult BonusReport()
        {
            return View();
        }

        //获取奖金报表页面
        public IActionResult GetBonusReport(int page, int limit,string title,string starttime,string endtime)
        {
            var allbonusresult = _bonus.List().AsQueryable();
            if (!string.IsNullOrEmpty(title))
            {
                allbonusresult = allbonusresult.Where(r => r.Title.Contains(title));
            }
            if (!string.IsNullOrEmpty(starttime))
            {
                allbonusresult = allbonusresult.Where(r => r.TransDate > Convert.ToDateTime(starttime));
            }
            if (!string.IsNullOrEmpty(endtime))
            {
                allbonusresult = allbonusresult.Where(r => r.TransDate <= Convert.ToDateTime(endtime));
            }

            int bonuscount = allbonusresult.ToList().Count();
            var bonusresult = allbonusresult.OrderByDescending(r => r.TransDate).Skip((page - 1) * limit).Take(limit).ToList();
            string resstr = JsonConvert.SerializeObject(new { code = 0, msg = "查询成功", count = bonuscount, data = bonusresult });
            return Content(resstr);
        }

        //总部打分页面
        public IActionResult MarkIndex()
        {
            return View();
        }

        //基地打分页面
        public IActionResult JidiMark()
        {
            return View();
        }

        //获取基地打分列表
        public IActionResult GetJiDiMarkRpt(int page, int limit,string title)
        {
            var result = from a in _context.Topics
                         join b in _context.Users
                         on a.UserName equals b.UserName
                         where a.Type == TopicType.Top
                         orderby a.FinishTime  
                         select new
                         {
                             a.Id,
                             a.UserName,
                             b.Department,
                             b.RealName,
                             a.Title,
                             a.FinishTime,
                             a.HasUpload
                         };
            var list = result.ToList();
            if (title != null)
            {
                list = list.Where(r => r.Title.Contains(title)).ToList();
            }
            var total = list.Count();
            var rows = list.Skip((page - 1) * limit).Take(limit).ToList();
            string str = JsonConvert.SerializeObject(new { code = 0, msg = "查询成功", count = total, data = rows });
            return Content(str);
        }


        //获取总部打分列表
        public IActionResult GetMarkRpt(int page, int limit,string title,string oastatus)
        {
            var result = from a in _context.Topics
                         join b in _context.Users
                         on a.UserName equals b.UserName
                         where a.Type == TopicType.Marking
                         orderby a.CreateOn descending
                         select new
                         {
                             a.Id,
                             a.Top,
                             a.UserName,
                             b.Department,
                             b.RealName,
                             a.Title,
                             a.CreateOn,
                             a.HasUpload,
                             a.TopicMark
                         };
            
            var list = result.ToList();
            
            if (title != null)
            {
                list = list.Where(r => r.Title.Contains(title)).ToList();
            }
            if (oastatus != ""&& oastatus !=null)
            {
                list = list.Where(r => r.Top==Convert.ToInt16(oastatus)).ToList();
            }
            var total = list.Count();
            var rows = list.Skip((page - 1) * limit).Take(limit).ToList();
            string str = JsonConvert.SerializeObject(new { code = 0, msg = "查询成功", count = total, data = rows });
            return Content(str);
        }
        //驳回的评分
        public IActionResult BohuiMark(int maid, int mark,int marktype)
        {
            _markinfo.Add(new MarkInfo
            {
                TopicId = maid,
                MarkBy = "Admin",
                MarkUserName = "Admin",
                Mark = mark,
                CreateDate = DateTime.Now,
                MarkDate = DateTime.Now,
                MarkType = marktype,
                SubMark1 = 0,
                SubMark2 =0,
                SubMark3 =0,
                SubMark4 =0,
                SubMark5 =0,
                SubMark6 =0
            });
            var topic = _topic.GetById(maid);

            var getreplyer = from a in _context.TopicReplys
                             join b in _context.Users
                            on a.ReplyUserName equals b.UserName
                             where a.TopicId == topic.Id
                             select new
                             {
                                 a.ReplyUserName,
                                 b.RealName
                             };
            //判断实际到底有多少人
            int allcount = getreplyer.Count() + 1;
            var replyerlist = getreplyer.ToList();
            for (int i = 0; i < replyerlist.Count; i++)
            {
                if (replyerlist[i].ReplyUserName == topic.UserName)
                {
                    replyerlist.Remove(replyerlist[i]);
                    //去除list中的replyusername
                    allcount -= 1;
                }
            }
            //计算百分比跟金额
            double ratio = 100 / allcount;

            double sumprice = 0.00;

            if (mark >= 0 && mark <= 9)
            {
                sumprice = 0.00;
            }
            else if (mark >= 10 && mark <= 20)
            {
                sumprice = 10.00;
            }
            else if (mark >= 21 && mark <= 30)
            {
                sumprice = 30.00;
            }
            else if (mark >= 31 && mark <= 40)
            {
                sumprice = 50.00;
            }
            else if (mark >= 41 && mark <= 50)
            {
                sumprice = 100.00;
            }
            else if (mark >= 51 && mark <= 60)
            {
                sumprice = 200.00;
            }

            double amount = sumprice / allcount;
            //roleval 1 代表改善人，2代表实施人，3代表提案人/实施人
            int roleval = 1;
            if (allcount == 1)
            {
                roleval = 3;
            }
            else
            {
                //进行实施人员的添加
                for (int i = 0; i < replyerlist.Count; i++)
                {
                    _bonus.Add(new Bonus
                    {
                        FinishTime = topic.FinishTime,
                        TransDate = DateTime.Now,
                        TopicId = topic.Id,
                        Title = topic.Title,
                        Mark = mark,
                        UserName = replyerlist[i].ReplyUserName,
                        RealName = replyerlist[i].RealName,
                        Role = 2,
                        Ratio = ratio,
                        Amount = amount
                    });
                }
            }
            //进行提案人的添加
            _bonus.Add(new Bonus
            {
                FinishTime = topic.FinishTime,
                TransDate=DateTime.Now,
                TopicId = topic.Id,
                Title = topic.Title,
                Mark = mark,
                UserName = topic.UserName,
                RealName = topic.User.RealName,
                Role = roleval,
                Ratio = ratio,
                Amount = amount
            });

            topic.LastReplyTime = System.DateTime.Now;
            topic.TopicMark = mark;
            topic.ZongbuMark = 0;
            topic.Type = TopicType.Perfect;
            _topic.Edit(topic);

            _statuslog.Add(new StatusLog
            {
                TopicId = maid,
                TransDate = DateTime.Now,
                TransBy = "Admin",
                TransName ="BohuiMark",
                PreStaus = 7,
                NowStatus = 6,
                Attr2 = mark.ToString()
            });
            string ss = JsonConvert.SerializeObject(new { success = true, exinfo ="打分成功" });
            return Content(ss);
        }


        //打分功能
        public IActionResult MarkTopic(int maid, int mark,string submark,int marktype)
        {
            string exin = "";
            string ss = "";
            int[] submarkdefault= new int[] { 0, 0, 0, 0, 0 ,0 };
            if (submark != null)
            {
                string[] allmark = submark.Split(",");
                for (int i = 0; i <6; i++)
                {
                    submarkdefault[i] =Convert.ToInt32(allmark[i]);
                }
            }
            try
            {
                //添加分数信息
                _markinfo.Add(new MarkInfo
                {
                    TopicId = maid,
                    MarkBy = "Admin",
                    MarkUserName = "Admin",
                    Mark = mark, 
                    CreateDate = DateTime.Now,
                    MarkDate=DateTime.Now,
                    MarkType= marktype,
                    SubMark1 = submarkdefault[0],
                    SubMark2= submarkdefault[1],
                    SubMark3 = submarkdefault[2],
                    SubMark4 = submarkdefault[3],
                    SubMark5 = submarkdefault[4],
                    SubMark6 = submarkdefault[5],
                });
                //添加打分日志
                string logtransname = marktype == 1 ? "JiMark" : "ZonbugMark";
                int preval = marktype == 1 ? 5 : 7;
                int nowval = marktype == 1 ? 7 : 6;
                _statuslog.Add(new StatusLog
                {
                    TopicId = maid,
                    TransDate = DateTime.Now,
                    TransBy = "Admin",
                    TransName = logtransname,
                    PreStaus = preval,
                    NowStatus = nowval,
                    Attr2 = mark.ToString()
                });

                var topic = _topic.GetById(maid);
                topic.LastReplyTime = System.DateTime.Now;
                //判断是否是基地打分
                if (topic.Type == TopicType.Top)
                {
                    topic.TopicMark = mark;
                    if (mark >= 61)
                    {
                        //发送邮件走总部流程
                        MailBox mymail = new MailBox();
                        mymail.Subject = "提案：" + topic.Title+"，总部打分邮件提醒";
                        string temaillist = "";
                        string tonamelist = "";
                        //部门对接人+上传文件的人
                        string fileupload = _myfile.List(r => r.TopicId == maid && r.FileIcon == "W").Select(r => r.Uploader).FirstOrDefault();
                        string duijieren = "";
                        var nu = UserManager.FindByNameAsync(topic.UserName).Result;
                        switch (nu.Department)
                        {
                            case "生产一组": duijieren = "167699"; break;   //曹燕
                            case "生产二组": duijieren = "167699"; break;
                            case "生产三组": duijieren = "167699"; break;
                            case "技术部": duijieren = "119065"; break;//邵余婷
                            case "设备部": duijieren = "186644"; break;//陈吉如
                            case "质量部": duijieren = "119701"; break;//郑倩
                            case "计划物控部": duijieren = "116882"; break;//陈丽
                            case "仓储物流部": duijieren = "118092"; break;//封吟吟
                            case "动力部": duijieren = "118881"; break;//杨欢
                            case "采购履行部": duijieren = "116684"; break;//俞红
                            case "财务部": duijieren = "111507"; break;//王佳
                            case "IE运营部": duijieren = "122304"; break;//柴兆龙
                            case "总经理办公室": duijieren = "118730"; break;//徐建英
                            case "人力资源部": duijieren = "118359"; break;//吴春霞
                        }

                        var fu = UserManager.FindByNameAsync(fileupload).Result;
                        var du = UserManager.FindByNameAsync(duijieren).Result;
                        temaillist = fu.Email + "," + du.Email;
                        tonamelist = fu.RealName + "," + du.RealName;
                        mymail.Body = @"<p>" + tonamelist + "您好：</p>" +
                                    "<p>您的完结提案：<a href=\"http://10.6.6.199/Topic/Index/" + maid + "\"  target=\"_blank\">" + topic.Title + "</a></p>" +
                                    "<p>基地评分为"+topic.TopicMark+"。根据总部规定，大于等于61分的提案需要重新走OA流程由总部审批</p>" +
                                    "请在OA中填写相关信息:首页》流程管理》管理支持与服务类》企业管理类》组件事业部-提案改善流程";
                        mymail.IsHtml = true;
                        mymail.To = temaillist.Split(',');
                        _mailservice.Enqueue(mymail);
                        topic.Type = TopicType.Marking;
                    }
                    else
                    {
                        //如果小于60分，就直接算完结了，然后添加分数信息
                        var getreplyer = from a in _context.TopicReplys
                                         join b in _context.Users
                                        on a.ReplyUserName equals b.UserName
                                         where a.TopicId == topic.Id
                                         select new
                                         {
                                             a.ReplyUserName,
                                             b.RealName
                                         };
                        //判断实际到底有多少人
                        int allcount = getreplyer.Count()+1;
                        var replyerlist = getreplyer.ToList();
                        for (int i = 0; i < replyerlist.Count; i++)
                        {
                            if (replyerlist[i].ReplyUserName == topic.UserName)
                            {
                                replyerlist.Remove(replyerlist[i]);
                                //去除list中的replyusername
                                allcount -= 1;
                            }
                        }
                        //计算百分比跟金额
                        double ratio = 100 / allcount;

                        double sumprice = 0.00;

                        if (mark >=0 && mark <= 9)
                        {
                            sumprice = 0.00;
                        }else if (mark >= 10 && mark <= 20)
                        {
                            sumprice = 10.00;
                        }
                        else if (mark >= 21 && mark <=30)
                        {
                            sumprice = 30.00;
                        }
                        else if (mark >= 31 && mark <= 40)
                        {
                            sumprice = 50.00;
                        }
                        else if (mark >= 41 && mark <= 50)
                        {
                            sumprice = 100.00;
                        }
                        else if (mark >= 51 && mark <= 60)
                        {
                            sumprice = 200.00;
                        }

                        double amount = sumprice / allcount;
                        //roleval 1 代表改善人，2代表实施人，3代表提案人/实施人
                        int roleval = 1;
                        if (allcount == 1)
                        {
                            roleval = 3;
                        }
                        else
                        {
                            //进行实施人员的添加
                            for (int i = 0; i < replyerlist.Count; i++)
                            {
                                _bonus.Add(new Bonus
                                {
                                    FinishTime = topic.FinishTime,
                                    TransDate = DateTime.Now,
                                    TopicId = topic.Id,
                                    Title = topic.Title,
                                    Mark = mark,
                                    UserName = replyerlist[i].ReplyUserName,
                                    RealName = replyerlist[i].RealName,
                                    Role = 2,
                                    Ratio = ratio,
                                    Amount = amount
                                });
                            }
                        }
                        //进行提案人的添加
                        _bonus.Add(new Bonus
                        {
                            FinishTime = topic.FinishTime,
                            TransDate = DateTime.Now,
                            TopicId = topic.Id,
                            Title = topic.Title,
                            Mark = mark,
                            UserName = topic.UserName,
                            RealName = topic.User.RealName,
                            Role = roleval,
                            Ratio = ratio,
                            Amount = amount
                        });                     
                      //将提案状态更新为完结
                        topic.Type = TopicType.Perfect;
                    }
                }
                else
                {
                    topic.ZongbuMark = mark;
                    topic.Type = TopicType.Perfect;
                }
                _topic.Edit(topic);
                exin = "评分成功";
                ss = JsonConvert.SerializeObject(new { success = true, exinfo = exin });
                return Content(ss);
            }
            catch (Exception ex)
            {
                exin =ex.ToString();
                ss = JsonConvert.SerializeObject(new { success = false, exinfo = exin });
                return Content(ss);
            }
        }

        
    }
}