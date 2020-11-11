using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ApplicationCore;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OfficeOpenXml;
using PI.Models;
using PI.ViewModels;


namespace PI.Controllers
{
    public class HomeController : Controller
    {
        private ITopicRepository _topic;
        public UserManager<User> UserManager { get; }
        private readonly DataContext _context;
        private IMyFileRepository _myfile;
        private IHostingEnvironment _hostingEnv;
        private ITopicReplyRepository _reply;
        private IStatusLogRepository _statuslog;
        private IMailQueueService _mailservice;
        private IReferkeyRepository _referkey;


        public HomeController(ITopicRepository topic, UserManager<User> userManager, IMyFileRepository myfile, IHostingEnvironment hostingEnv, DataContext context, ITopicReplyRepository reply,IStatusLogRepository statuslog, IMailQueueService mailservice, IReferkeyRepository referkey)
        {
            _topic = topic;
            UserManager = userManager;
            _myfile = myfile;
            _hostingEnv = hostingEnv;
            _context = context;
            _reply = reply;
            _statuslog = statuslog;
            _mailservice = mailservice;
            _referkey = referkey;

        }

        //获取首页所有信息
        public IActionResult Index(string looks, string topicname)
        {
            var thisuser= UserManager.GetUserAsync(User).Result;
            var gnotice = _topic.List(r => r.Type == TopicType.Notice && r.NodeId == 5).OrderByDescending(r => r.CreateOn)
               .Select(r => new NoticeViewModel
               {
                   Id = r.Id,
                   Title = r.Title,
                   CreateOn = r.CreateOn,
                   UserName = r.UserName
               }).FirstOrDefault();
           
            var unotice = _topic.List(r => r.Type == TopicType.Notice && r.NodeId == 4).OrderByDescending(r => r.CreateOn)
              .Select(r => new NoticeViewModel
              {
                  Id = r.Id,
                  Title = r.Title,
                  CreateOn = r.CreateOn,
                  UserName = r.UserName
              }).OrderByDescending(r => r.CreateOn).Take(5).ToList();

            if (!string.IsNullOrEmpty(topicname))
            {
                var stm = _topic.List(r => r.Title.Contains(topicname)).Select(r => new TopicViewModel
                {
                    Id = r.Id,
                    NodeId = r.NodeId,
                    NodeName = r.Node.Name,
                    UserName = r.User.UserName,
                    RealName = r.User.RealName,
                    Department = r.User.Department,
                    Title = r.Title,
                    Top = r.Top,
                    Type = r.Type,
                    ReplyCount = r.ReplyCount,
                    LastReplyTime = r.LastReplyTime,
                    CreateOn = r.CreateOn,
                    TopicMark = r.TopicMark
                });
                ViewBag.UNotice = unotice;
                ViewBag.GNotice = gnotice;
                ViewBag.Topics = stm.ToList();
                ViewBag.User = thisuser;
                return View();
            }
            else
            { 
            int pageindex = 1;
            int pagesize = 10;
            Page<Topic> result = null;
            ViewBag.Topics = null;
            if (string.IsNullOrEmpty(looks))
                result = _topic.PageList(r => r.Type == TopicType.Good, pagesize, pageindex);
            if (looks == "check")
                result = _topic.PageList(r => r.Type == TopicType.TeamLeaderCheck||r.Type==TopicType.AdminCheck, pagesize, pageindex);
            if (looks == "good")
                result = _topic.PageList(r => r.Type == TopicType.Good, pagesize, pageindex);
            if (looks == "perfect")
                result = _topic.PageList(r => r.Type == TopicType.Perfect || r.Type == TopicType.Top, pagesize, pageindex);
            var tvm = result.List.Select(r => new TopicViewModel
            {
                Id = r.Id,
                NodeId = r.NodeId,
                NodeName = r.Node.Name,
                UserName = r.User.UserName,
                RealName = r.User.RealName,
                Department = r.User.Department,
                Title = r.Title,
                Top = r.Top,
                Type = r.Type,
                ReplyCount = r.ReplyCount,
                LastReplyTime = r.LastReplyTime,
                CreateOn = r.CreateOn,
                TopicMark = r.TopicMark
            });
            ViewBag.UNotice = unotice;
            ViewBag.GNotice = gnotice;
            ViewBag.Topics = tvm.ToList();
            ViewBag.User = thisuser;
            return View();
            }
        }

        /*public IActionResult Draft(int id)
        {
            ViewBag.TopicId = id;
            return View();
        }

        public IActionResult GetDraftInfo(int id)
        {
            var topic = _topic.GetById(id);
            var ffiles = _myfile.List(r => r.TopicId == id).ToList();
            string resstr = JsonConvert.SerializeObject(new { code = "0", msg = "发布成功",topicdata=topic,files=ffiles });
            return Content(resstr);
        }

        public IActionResult DraftAdd(string node, string title, string content, string suggest, string fileid, string type, string truser,int topicid)
        {

            string teamleader = "";
            string resstr = "";
            int rcount = 0;
            var retype = TopicType.TeamLeaderCheck;
            var nu = UserManager.GetUserAsync(User).Result;
            if (type == "2")
            {
                if (truser == null)
                    //直接去部门对接人那里
                    switch (nu.Department)
                    {
                         
                    }
                else
                {
                    teamleader = truser;
                }
            }
            else
            {
                retype = TopicType.AdminCheck;
                teamleader = "Admin";
                rcount = truser.Split(',').Length;
            }
            try
            {
                var topic = _topic.GetById(topicid);
                topic.LastReplyTime = DateTime.Now;
                topic.LastReplyUserId = nu.Id;
                topic.UserName = nu.UserName;
                topic.UserId = nu.Id;
                topic.CreateOn = DateTime.Now;
                topic.Type = retype;
                topic.ReplyCount = rcount;
                topic.TeamLeader = teamleader;
                topic.NodeId = Convert.ToInt16(node);
                topic.Title = title;
                topic.Content = content;
                topic.Suggest = suggest;
                topic.HasUpload = fileid;
                _topic.Edit(topic);

                _statuslog.Add(new StatusLog
                {
                    TopicId = topicid,
                    TransDate = DateTime.Now,
                    TransBy = nu.UserName,
                    TransName = "DraftAddTopic",
                    PreStaus = 2,
                    NowStatus = Convert.ToInt16(retype)
                });
                //发送审核邮件
                if (teamleader != "" && type == "2")
                {
                    MailBox mymail = new MailBox();
                    mymail.Subject = "请审核提案-" + title;
                    mymail.Body = @"<p>" + truser + "您好：</p>" +
                               "<p>请审核提案：<a href=\"http://10.6.6.199/User/Mycheck \" target=\"_blank\">" + title + "</a></p>";
                    mymail.IsHtml = true;
                    var tu = UserManager.FindByNameAsync(teamleader).Result;
                    mymail.To = tu.Email.Split(',');
                    _mailservice.Enqueue(mymail);
                }
                //判断是否有附件
                if (fileid == "0")
                {
                    resstr = JsonConvert.SerializeObject(new { code = "0", msg = "发布成功" });
                    return Content(resstr);
                }
                else
                {
                    //刷新文件数据
                    string[] idsplit = fileid.Split(new Char[] { ',' });
                    for (int j = 0; j < idsplit.Length; j++)
                    {
                        int aa = Convert.ToInt32(idsplit[j]);
                        var attachfile = _myfile.List(r => r.Id == aa).FirstOrDefault();
                        attachfile.TopicId = topicid;
                        attachfile.Uploader = nu.UserName;
                        if (type == "3") { attachfile.FileIcon = "W"; }
                        _myfile.Edit(attachfile);
                    }
                }
                //直接添加实施人
                if (type == "3")
                {
                    int xhlength = 1;
                    if (truser.IndexOf(',') > 0)
                    {
                        string[] rp = truser.Split(',');
                        xhlength = rp.Length;
                    }
                    for (int k = 0; k < xhlength; k++)
                    {
                        string rusername = truser;
                        if (xhlength > 1)
                        {
                            rusername = truser.Split(',')[k];
                        }
                        var ru = UserManager.FindByNameAsync(rusername).Result;
                        _reply.Add(new TopicReply
                        {
                            ReplyUserId = ru.Id,
                            CreateOn = DateTime.Now,
                            ReplyEmail = ru.Email,
                            ReplyUserName = ru.UserName,
                            ReplyContent = "我参与此提案并进行改善",
                            TopicId = topicid,
                            ReplyType = TopicReplyType.Answer
                        });
                    }
                }
                resstr = JsonConvert.SerializeObject(new { code = "0", msg = "发布成功" });
                return Content(resstr);
            }
            catch (Exception ex)
            {
                resstr = JsonConvert.SerializeObject(new { code = "1", msg = ex.ToString() });
                return Content(resstr);
            }
        }*/




        public IActionResult AddTopic()
        {
            return View();
        } 

        //新添加提案功能
        public IActionResult AddTopicNew(string node,string title,string content,string suggest,string fileid,string type,string truser,string sugdep,string tusername)
        {
            string teamleader = "";
            string attrname = "";
            string resstr = "";
            int rcount = 0;
            var retype = TopicType.TeamLeaderCheck;
            //var nu = UserManager.GetUserAsync(User).Result;
            //为了小程序-将用户名抛过来
            var nu = UserManager.FindByNameAsync(tusername).Result;
            if (type == "2")
            {
                if (!string.IsNullOrEmpty(sugdep)) {
                    //直接去部门对接人那里
                    var temskey = _referkey.List(r => r.Keys == "teamleader" && r.StrVal3 == sugdep).FirstOrDefault();
                    teamleader = temskey.StrVal1;
                    attrname = temskey.StrVal2;
                    /* switch (sugdep)
                {
                    case "生产一组":teamleader = "252381"; attrname = "赵洋"; break;    
                    case "生产二组": teamleader = "252381"; attrname = "赵洋"; break;
                    case "生产三组": teamleader = "252381"; attrname = "赵洋"; break;
                    case "生产管理组": teamleader = "252381"; attrname = "赵洋"; break;
                    case "质量部": teamleader = "245626"; attrname = "张卓然"; break;
                    case "总经理办公室": teamleader = "251616"; attrname = "田方"; break;
                    case "计划物控部": teamleader = "245213"; attrname = "陈芳"; break;
                        case "技术部": teamleader = "194695"; attrname = "严重菲"; break;
                        case "设备部": teamleader = "194696"; attrname = "高歌"; break;
                        case "动力部": teamleader = "245208"; attrname = "马腾"; break;
                        case "仓储物流部": teamleader = "249943"; attrname = "李真真"; break;
                        case "人力资源部": teamleader = "264373"; attrname = "段苇"; break;
                        case "采购部": teamleader = "164060"; attrname = "徐宾宏"; break;
                        case "财务部": teamleader = "194695"; attrname = "严重菲"; break;
                        case "IE运营部": teamleader = "194695"; attrname = "严重菲"; break;
                    }*/
                }
                else
                {
                    teamleader = "Admin";
                    attrname = "管理员";
                }
            }
            else 
            {
                retype = TopicType.AdminCheck;
                teamleader = "Admin";
                attrname = "管理员";
                rcount = truser.Split(',').Length;
            }
            try
            {
               int tid=_topic.Add(new Topic
                {
                LastReplyTime = DateTime.Now,
                LastReplyUserId = nu.Id,
                UserName = nu.UserName,
                UserId=nu.Id,
                CreateOn = DateTime.Now,
                Type = retype,
                ReplyCount = rcount,
                TeamLeader = teamleader,
                NodeId = Convert.ToInt16(node),
                Title=title,
                Content=content,
                Suggest=suggest,
                HasUpload=fileid,
                TopicMark=0,
                ZongbuMark=0
                });
            
                _statuslog.Add(new StatusLog
                {
                    TopicId=tid,
                    TransDate=DateTime.Now,
                    TransBy=nu.UserName,
                    TransName="CreateTopic",
                    PreStaus=-1,
                    NowStatus= Convert.ToInt16(retype),
                    Attr2= attrname
                });
                //发送审核邮件
                if (teamleader !="" && type == "2") { 
                MailBox mymail = new MailBox();
                mymail.Subject = "请审核提案-" +title;
                mymail.Body = @"<p>" + truser + "您好：</p>" +
                           "<p>请审核提案：<a href=\"http://10.6.6.193/User/Mycheck \" target=\"_blank\">" + title + "</a></p>";
                mymail.IsHtml = true;
                var tu= UserManager.FindByNameAsync(teamleader).Result;
                mymail.To = tu.Email.Split(',');
                _mailservice.Enqueue(mymail);
                }
                //判断是否有附件
                if (fileid == "0") {
                    resstr = JsonConvert.SerializeObject(new { code = "0", msg = "发布成功" });
                    return Content(resstr);
                }else {
                //刷新文件数据
                string[] idsplit = fileid.Split(new Char[] { ',' });
                for (int j = 0; j < idsplit.Length; j++)
                  {
                    int aa = Convert.ToInt32(idsplit[j]);
                    var attachfile = _myfile.List(r => r.Id == aa).FirstOrDefault();
                    attachfile.TopicId = tid;
                    attachfile.Uploader =nu.UserName;
                    if (type == "3") { attachfile.FileIcon ="W"; }
                   _myfile.Edit(attachfile);
                    }

                    //直接添加实施人
                    if (type == "3")
                    {
                        int xhlength = 1;
                        if (truser.IndexOf(',') > 0)
                        {
                            string[] rp = truser.Split(',');
                            xhlength = rp.Length;
                        }
                        for (int k = 0; k < xhlength; k++)
                        {
                            string rusername = truser;
                            if (xhlength > 1)
                            {
                                rusername = truser.Split(',')[k];
                            }
                            int ceshi = tid;
                            var ru = UserManager.FindByNameAsync(rusername).Result;
                            _reply.Add(new TopicReply
                            {
                                ReplyUserId = ru.Id,
                                CreateOn = DateTime.Now,
                                ReplyEmail = ru.Email,
                                ReplyUserName = ru.UserName,
                                ReplyContent = "我参与此提案并进行改善",
                                TopicId = tid,
                                ReplyType = TopicReplyType.Answer
                            });
                        }
                    }
                }
                resstr = JsonConvert.SerializeObject(new { code = "0", msg = "发布成功" });
                return Content(resstr);
            }
            catch (Exception ex)
            {
                resstr = JsonConvert.SerializeObject(new { code = "1", msg = ex.ToString() });
                return Content(resstr);
            }
        }
        public IActionResult NoReg()
        {
            return View();
        }



        public IActionResult AllTopics()
        {
            return View();
        }
        //查看所有提案
        public IActionResult ShowAllTopics(string looks, int pageIndex, int pageSize)
        {
            var result = _topic.List();
            if (string.IsNullOrEmpty(looks))
            { result = _topic.List(r => r.Type == TopicType.Normal); }
            if (looks == "normal")
            { result = _topic.List(r => r.Type == TopicType.Normal); }
            if (looks == "good")
            { result = _topic.List(r => r.Type == TopicType.Good); }
            if (looks == "perfect")
            { result = _topic.List(r => r.Type == TopicType.Top || r.Type == TopicType.Perfect); }
            var tvm = result.Select(r => new TopicViewModel
            {
                Id = r.Id,
                NodeName = r.Node.Name,
                UserName = r.User.UserName,
                RealName = r.User.RealName,
                Title = r.Title,
                Type = r.Type,
                ReplyCount = r.ReplyCount,
                Department = r.User.Department,
                LastReplyTime = r.LastReplyTime,
                CreateOn = r.CreateOn,
                TopicMark = r.TopicMark
            });
            var nopagetopics = tvm.ToList();
            int count = nopagetopics.Count();
            var list = nopagetopics.Skip((pageIndex-1)* pageSize).Take(pageSize).OrderByDescending(r => r.CreateOn).ToList();
            string str = JsonConvert.SerializeObject(new { total = count, rows = list });
            return Content(str);
        }

        //报表
        public IActionResult Report()
        {
            return View();
        }
        //提案报表功能
        public IActionResult GetReport(int limit, int page, string title, string username, string status, string startdate, string enddate, string department, string acdepartment,int setimef)
        {
             var result = from a in _context.Topics
                         join ar in _context.Users
                         on a.UserId equals ar.Id
                         join c in _context.Users
                         on a.TeamLeader equals c.UserName
                         into t
                         from ac in t.DefaultIfEmpty()
                         where a.UserName != "admin"
                         select new
                         {
                             a.Id,
                             a.NodeId,
                             ar.RealName,
                             ar.Department,
                             a.TeamLeader,
                             a.PassTime,
                             a.FinishTime,
                             AcRealName=ac.RealName,
                             Acdepartment = ac.Department,
                             a.Type,
                             a.UserName,
                             a.Title,
                             a.ReplyCount,
                             a.CreateOn,
                             a.TopicMark,
                             a.ZongbuMark
                         };
            //这里的result为IQueryable类型 不能直接tolist是因为result前面默认为IQueryable类型,不使用tolist是为了拼接查询条件
            if (setimef != 0)
            {
                switch (setimef)
                {
                    case 1:
                        if (!string.IsNullOrEmpty(startdate)) { result = result.Where(r => r.CreateOn > Convert.ToDateTime(startdate)); }
                        if (!string.IsNullOrEmpty(enddate)) { result = result.Where(r => r.CreateOn < Convert.ToDateTime(enddate)); }
                        break;
                    case 2:
                        if (!string.IsNullOrEmpty(startdate)) { result = result.Where(r => r.PassTime > Convert.ToDateTime(startdate)); }
                        if (!string.IsNullOrEmpty(enddate)) { result = result.Where(r => r.PassTime < Convert.ToDateTime(enddate)); }
                        break;
                    case 3:
                        if (!string.IsNullOrEmpty(startdate)) { result = result.Where(r => r.FinishTime > Convert.ToDateTime(startdate)); }
                        if (!string.IsNullOrEmpty(enddate)) { result = result.Where(r => r.FinishTime < Convert.ToDateTime(enddate)); }
                        break;

                } 
            }
            if (!string.IsNullOrEmpty(title)) { result = result.Where(r => r.Title.Contains(title)); }
            if (!string.IsNullOrEmpty(username)) { result = result.Where(r => r.RealName == username); }
            if (!string.IsNullOrEmpty(department)) {
                String[] depArr = department.Split(",");
                result = result.Where(r => depArr.Contains(r.Department)); }
            if (!string.IsNullOrEmpty(acdepartment)) { result = result.Where(r => r.Acdepartment == acdepartment); }
            var shangci = result;
            if (!string.IsNullOrEmpty(status))
            {
                //这里的处理类似于冒泡排序-用一个临时的值来进行中转
                String[] statusArr = status.Split(",");
                for (int aa=0;aa< statusArr.Length; aa++) {
                 var fresult = result;
                  switch (statusArr[aa])
                  {
                    case "1":fresult = result.Where(r => r.Type == TopicType.Normal); break;
                    case "2":fresult = result.Where(r => r.Type == TopicType.TeamLeaderCheck); break;
                    case "3": fresult = result.Where(r => r.Type == TopicType.AdminCheck); break;
                    case "4":fresult = result.Where(r => r.Type == TopicType.Good); break;
                    case "5":fresult = result.Where(r => r.Type == TopicType.Top); break;
                    case "6":fresult = result.Where(r => r.Type == TopicType.Perfect); break;
                    case "7": fresult = result.Where(r => r.Type == TopicType.Marking); break;
                    case "0":fresult = result.Where(r => r.Type == TopicType.Delete); break;
                    case "11": fresult = result.Where(r => r.Type == TopicType.Breakup); break;
                    }
                    if (aa != 0)
                    {
                        var ccresult= shangci.Concat(fresult);
                        shangci = ccresult;
                    }
                    else
                    {
                        shangci = fresult;
                    }
                }
            }
            var list = shangci.OrderByDescending(r => r.CreateOn).ToList();
            var total = list.Count();
            var rows = list.Skip((page - 1) * limit).Take(limit).ToList();
            string str = JsonConvert.SerializeObject(new { code = 0, msg = "查询成功", count = total, data = rows });
            return Content(str);
        }

        public IActionResult MergeReport()
        {
            return View();
        }

        public IActionResult GetMergeReport(int page,int limit,string musername,string mtitle,string mdepartment,string mstarttime,string mendtime,int mtype,string mstatus)
        { 

            var mergeresult = _context.MergeDatas.Where(r=>r.Type!=0);
            if (mtype != 0)
            {
                switch (mtype)
                {
                    case 1:
                        if (!string.IsNullOrEmpty(mstarttime)) { mergeresult = mergeresult.Where(r => r.CreateOn > Convert.ToDateTime(mstarttime)); }
                        if (!string.IsNullOrEmpty(mendtime)) { mergeresult = mergeresult.Where(r => r.CreateOn < Convert.ToDateTime(mendtime)); }
                        break;
                    case 2:
                        if (!string.IsNullOrEmpty(mstarttime)) { mergeresult = mergeresult.Where(r => r.PassTime > Convert.ToDateTime(mstarttime)); }
                        if (!string.IsNullOrEmpty(mendtime)) { mergeresult = mergeresult.Where(r => r.PassTime < Convert.ToDateTime(mendtime)); }
                        break;
                    case 3:
                        if (!string.IsNullOrEmpty(mstarttime)) { mergeresult = mergeresult.Where(r => r.FinishTime > Convert.ToDateTime(mstarttime)); }
                        if (!string.IsNullOrEmpty(mendtime)) { mergeresult = mergeresult.Where(r => r.FinishTime < Convert.ToDateTime(mendtime)); }
                        break;
                }
            }
            if (!string.IsNullOrEmpty(mtitle)) { mergeresult = mergeresult.Where(r => r.Title.Contains(mtitle)); }
            if (!string.IsNullOrEmpty(mstatus)) {
                  mergeresult = mergeresult.Where(r => r.Type == Convert.ToInt16(mstatus)); 
            }
            if (!string.IsNullOrEmpty(musername)) { mergeresult = mergeresult.Where(r => r.TRealName == musername); }
            if (!string.IsNullOrEmpty(mdepartment)) { mergeresult = mergeresult.Where(r => r.TDepartment == mdepartment); }
            int rescount = mergeresult.Count();
            var resdata = mergeresult.OrderByDescending(r => r.CreateOn).Skip((page - 1) * limit).Take(limit).ToList();
            string resstr = JsonConvert.SerializeObject(new { code = 0, msg = "查询成功", count = rescount, data = resdata });
            return Content(resstr);
        }



        public IActionResult TrReport()
        {
            return View();
        }
        //改善报表功能
        public IActionResult GetTrReport(int limit, int page, string title, string username, int status, string startdate, string enddate)
        {
            var result = from a in _context.TopicReplys
                         join ar in _context.Users
                         on a.ReplyUserId equals ar.Id
                       
                         join c in _context.Topics
                         on a.TopicId equals c.Id
                         
                         where a.ReplyType==0
                         select new
                         {
                             a.Id,
                             ar.UserName,
                             ar.RealName,
                             c.Type,
                             c.Title,
                             a.CreateOn,
                             a.TopicId,
                             a.ReplyContent
                         };
            
            if (!string.IsNullOrEmpty(startdate)) { result = result.Where(r => r.CreateOn > Convert.ToDateTime(startdate)); }
            if (!string.IsNullOrEmpty(enddate)) { result = result.Where(r => r.CreateOn < Convert.ToDateTime(enddate)); }
            if (!string.IsNullOrEmpty(title)) { result = result.Where(r => r.Title.Contains(title)); }
            if (!string.IsNullOrEmpty(username)) { result = result.Where(r => r.RealName == username); }
            if (status != 0)
            {
                switch (status)
                {
                    case 4: result = result.Where(r => r.Type == TopicType.Good); break;
                    case 5: result = result.Where(r => r.Type == TopicType.Top); break;
                    case 6: result = result.Where(r => r.Type == TopicType.Perfect); break;
                }
            }
            var list = result.ToList();
            var total = list.Count();
            var rows = list.Skip((page - 1) * limit).Take(limit).ToList();
            string str = JsonConvert.SerializeObject(new { code = 0, msg = "查询成功", count = total, data = rows });
            return Content(str);
        }



        public IActionResult UserInfoReport()
        {
            return View();
        }
        //人员信息功能
        public IActionResult GetUserInfoReport(int limit, int page, string username)
        {
            var uresult = from u in _context.Users
                          where u.UserName!="admin"
                          select new
                          {
                              u.UserName,
                              u.RealName,
                              u.Email,
                              u.CreateOn,
                              u.TopicCount,
                              u.TopicReplyCount,
                              u.Department
                          };
            string aa = username;
            if (!string.IsNullOrEmpty(username)) { uresult = uresult.Where(r => r.RealName == username); }
            var list1 = uresult.ToList();
            var total = list1.Count();
            var rows= list1.Skip((page - 1) * limit).Take(limit).OrderByDescending(r => r.CreateOn).ToList();
            string str = JsonConvert.SerializeObject(new { code = 0, msg = "查询成功", count = total, data = rows });
            return Content(str);
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        //进入公告-公告主页内容
        public IActionResult ShowNotice(int id)
        {
            var showdetail = _topic.List(r=>r.Id== id).FirstOrDefault();
            ViewBag.ShowNotice = showdetail;
            return View();
        }

        //批量上传功能
        public async Task<IActionResult> TMFileImport(string uploaduser,string userimport)
        {
            var files = Request.Form.Files;
            string filePath = "";
            string realfilename = "";
            foreach (var formFile in files)
            {
                    string webRootPath = _hostingEnv.WebRootPath;
                    DateTime dt = DateTime.Now;
                    string filedate = dt.ToString("yyyyMMddHHmmss");
                    string fileExt = Path.GetExtension(formFile.FileName); //文件扩展名，不含“.”
                    realfilename = Path.GetFileNameWithoutExtension(formFile.FileName);
                    long fileSize = formFile.Length; //获得文件大小，以字节为单位 我将Long改为Int                    
                    string newFileName = filedate + realfilename + fileExt;
                    filePath = webRootPath + "/import/" + newFileName;
                    FileInfo file = new FileInfo(filePath);
                    try
                    {
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {

                            await formFile.CopyToAsync(stream);
                            stream.Flush();
                        }
                        if (System.IO.File.Exists(filePath))
                        {
                            _myfile.Add(new MyFile
                            {
                                CreateDt = dt,
                                ModifyDt = dt,
                                FileExt = fileExt,
                                FileName = realfilename,
                                FilePath = filePath,
                                FileSize = (int)fileSize,
                                FileIcon = "TM",
                                TopicId = 0,
                                Uploader = uploaduser
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        string ss = JsonConvert.SerializeObject(new { Success = false, exinfo = "文件上传异常" + ex.ToString() });
                        return Content(ss);
                    }
                    try
                    {
                        using (ExcelPackage package = new ExcelPackage(file))
                        {

                            ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                            int rowCount = worksheet.Dimension.Rows;
                            int ColCount = worksheet.Dimension.Columns;
                            string[] array = null;
                            string[] brray = null;

                            //brray主要是用来判断用户是否已经注册
                            brray = new string[rowCount - 1];
                            for (int row = 2; row <= rowCount; row++)
                            {
                                //空值不能赋值给数组。。。
                                if (worksheet.Cells[row, 1].Value is null)
                                {
                                    string ex = "第" + row + "行用户名为空！！！用户名不准为空";
                                    string ss = JsonConvert.SerializeObject(new { Success = false, exinfo = ex });
                                    return Content(ss);
                                }
                                brray[row - 2] = worksheet.Cells[row, 1].Value.ToString();
                            }
                            string[] checkuser = brray.Distinct().ToArray();
                            for (int c = 0; c < checkuser.Length; c++)
                            {
                                //如何用in查询一次性查完。。。一次次去读感觉怪怪的- - 现在是完全不考虑性能。。。
                                if (checkuser[c] != null)
                                {
                                    var bu = UserManager.FindByNameAsync(checkuser[c]).Result;
                                    if (bu is null)
                                    {
                                    //这里重新读取下行号
                                    for (int row = 2; row <= rowCount; row++)
                                    {
                                        if (worksheet.Cells[row, 1].Value.ToString() == checkuser[c])
                                        {
                                            string runame = worksheet.Cells[row, 1].Value.ToString();
                                            string rrname = worksheet.Cells[row, 2].Value.ToString();
                                            string rdep = worksheet.Cells[row, 3].Value.ToString();
                                            string reamil = "pi@longi-silicon.com";
                                            if (userimport == "userimport")
                                            {
                                                reamil = worksheet.Cells[row,4].Value.ToString(); ;
                                            }
                                            string rpass = "123456";
                                            var user = new User { UserName = runame, Email = reamil, Department = rdep, CreateOn = DateTime.Now, LastTime = DateTime.Now, RealName = rrname };
                                            var result = await UserManager.CreateAsync(user, rpass);
                                            if (result.Succeeded==false)
                                            {
                                                string ex = checkuser[c] + "用户自动注册失败，请联系管理员";
                                                string ss = JsonConvert.SerializeObject(new { Success = false, exinfo = ex });
                                                return Content(ss);
                                            }
                                            break;
                                        }
                                    }
                                    }
                                }
                            }
                            if(userimport == "userimport")
                            {
                              string ss = JsonConvert.SerializeObject(new { Success = true, exinfo ="批量导入成功" });
                              return Content(ss);
                            }

                        //判断是否有空内容，防止一半上传-这里实在太多了。。。
                        for (int row = 2; row <= rowCount; row++)
                            {
                                for (int col = 1; col <= ColCount; col++)
                                {
                                    if (worksheet.Cells[row, col].Value is null)
                                    {
                                        string ex = "第" + row + "行存在空内容，请检查上传文件";
                                        string ss = JsonConvert.SerializeObject(new { Success = false, exinfo = ex });
                                        return Content(ss);
                                    }
                                }
                            }

                            for (int row = 2; row <= rowCount; row++)
                            {
                                array = new string[ColCount];
                                for (int col = 1; col <= ColCount; col++)
                                {
                                    array[col - 1] = worksheet.Cells[row, col].Value.ToString();
                                }
                                string username = array[0];
                                string title = array[3];
                                string content = array[4];
                                string suggest = array[5];
                                string teamlead = array[7];
                                int node = array[6] == "技术类" ? 3 : 2;
                                var ou = UserManager.FindByNameAsync(username).Result;
                                var tu = UserManager.FindByNameAsync(teamlead).Result;
                                if (tu == null) { teamlead = "120776"; }
                               int ptid=_topic.Add(new Topic
                                {
                                    Content = content,
                                    Suggest = suggest,
                                    UserId = ou.Id,
                                    Title = title,
                                    Email = ou.Email,
                                    NodeId = node,
                                    ReplyCount = 0,
                                    LastReplyTime = DateTime.Now,
                                    LastReplyUserId = ou.Id,
                                    UserName = username,
                                    TeamLeader= teamlead,
                                    CreateOn = DateTime.Now,
                                    Type = TopicType.TeamLeaderCheck,
                                    HasUpload = "0"
                                });
                               _statuslog.Add(new StatusLog
                               {
                                TopicId = ptid,
                                TransDate = DateTime.Now,
                                TransBy = username,
                                TransName = "CreateTopic",
                                PreStaus = -1,
                                NowStatus = 2
                                });
                        }
                        }
                    }
                    catch (Exception ex)
                    {
                        string ss = JsonConvert.SerializeObject(new { Success = false, exinfo = "模板格式异常" + ex.ToString() });
                        return Content(ss);
                    }
            }
            string str = JsonConvert.SerializeObject(new { Success = true, FName = realfilename });
            return Content(str);
        }
    }
}
