using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationCore;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PI.ViewModels;

namespace PI.Controllers
{
    public class UserController : Controller
    {
        private ITopicRepository _topic;
        public UserManager<User> UserManager { get; }
        private readonly DataContext _context;
        private IMarkInfoRepository _markinfo;
        private IReferkeyRepository _referkey;
        private IStatusLogRepository _statuslog;


        public UserController(DataContext context, UserManager<User> userManager, ITopicRepository topic, IMarkInfoRepository markinfo, IReferkeyRepository referkey, IStatusLogRepository statuslog)
        {
            _context = context;
            UserManager = userManager;
            _topic = topic;
            _markinfo = markinfo;
            _referkey = referkey;
            _statuslog = statuslog;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult MyTR()
        {
            return View();
        }

         
        public IActionResult UserInfo()
        {
            ViewBag.User = UserManager.GetUserAsync(User).Result;
            return View();
        }


        //更改用户信息
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User usermodel)
        {
            var user = UserManager.GetUserAsync(User).Result;
            if (ModelState.IsValid)
            {
                user.Email = usermodel.Email;
                user.RealName = usermodel.RealName;
                user.Department = usermodel.Department;
                await UserManager.UpdateAsync(user);
                return RedirectToAction("UserInfo");
            }
            return Content("信息格式异常");
        }

        
        public IActionResult MyCheck()
        {
            var hasu = UserManager.GetUserAsync(User).Result;
            if (hasu == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return  View();
        }


        //我审核的提案
        public IActionResult GetCheckTopic(int pageIndex, int pageSize,string username,string skey)
        {
            string tlusername = "";
            if (username == null)
            {
                var u = UserManager.GetUserAsync(User).Result;
                tlusername = u.UserName;
            }
            else
            {
                tlusername = username;
            }
            var backchecklist = _topic.List(r => r.Type == TopicType.TeamLeaderCheck && r.TeamLeader == tlusername).Select(r => new
            {
                r.Id,
                r.Title,
                r.CreateOn,
                r.NodeId,
                r.User.RealName,
                r.User.Department
            });
            if (skey != null&& skey !="")
            {
                backchecklist = backchecklist.Where(r => r.Title.Contains(skey));
            }
            var ctss = backchecklist.OrderBy(r => r.CreateOn).ToList();
            int count = ctss.Count();
            var list = ctss.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            string str = JsonConvert.SerializeObject(new { total = count, rows = list });
            return Content(str);
        }

        public IActionResult MyMark()
        {
            var hasu= UserManager.GetUserAsync(User).Result;
            if (hasu == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        //我打分的提案
        public IActionResult GetMarkTopic(int pageIndex, int pageSize)
        {
            var u = UserManager.GetUserAsync(User).Result;
            bool  duijieren = _referkey.List(r => r.Keys == "teamleader" && r.StrVal1 == u.UserName).Any();
            if (duijieren)
            {
               var markbylist = (
               from c in _context.Topics
               join s in _context.Users
               on c.UserName equals s.UserName
               where c.Type == TopicType.Top && s.Department==u.Department&&c.TopicMark==0
               select new
               {
                   c.Id,
                   c.Title,
                   c.FinishTime,
               });
                var mcss = markbylist.OrderBy(r => r.FinishTime).ToList();
                int count = mcss.Count();
                var list = mcss.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                string str = JsonConvert.SerializeObject(new {code=0,total = count, rows = list });
                return Content(str);
            }
            else
            {
                string str = JsonConvert.SerializeObject(new {code=1});
                return Content(str);
            }
            
        }


        //我的主页
        public IActionResult MyHome()
        {
            var u= UserManager.GetUserAsync(User).Result;
            var mytforten = _topic.List(r => r.UserName==u.UserName).Select(r => new SimpleTopic
                {
                   Id=r.Id,
                   Title=r.Title,
                   CreateOn=r.CreateOn
                }).OrderByDescending(r=>r.CreateOn);
           
            var myrforr=(from a in _context.Topics
                         join c in _context.TopicReplys
                         on a.Id equals c.TopicId
                         where a.Type==TopicType.Good&&c.ReplyUserName==u.UserName
                         select new SimpleTopic
                         {
                            Id=a.Id,
                            Title=a.Title,
                           CreateOn=c.CreateOn
                         });
            ViewBag.RTopics = mytforten.Take(10).ToList();
            ViewBag.RsReplys = myrforr.Take(10).ToList();
            ViewBag.User = u;
            return View();
        }


        //对接人打分
        public IActionResult MarkTopic(int maid, int mark, string submark, int marktype)
        {
            string exin = "";
            string ss = "";
            int[] submarkdefault = new int[] { 0, 0, 0, 0, 0, 0 };
            if (submark != null)
            {
                string[] allmark = submark.Split(",");
                for (int i = 0; i < 6; i++)
                {
                    submarkdefault[i] = Convert.ToInt32(allmark[i]);
                }
            }
            try
            {
                var u = UserManager.GetUserAsync(User).Result;
                //添加分数信息
                _markinfo.Add(new MarkInfo
                {
                    TopicId = maid,
                    MarkBy = u.UserName,
                    MarkUserName = u.RealName,
                    Mark = mark == 0 ? 1 : mark,
                    CreateDate = DateTime.Now,
                    MarkDate = DateTime.Now,
                    MarkType = marktype,
                    SubMark1 = submarkdefault[0],
                    SubMark2 = submarkdefault[1],
                    SubMark3 = submarkdefault[2],
                    SubMark4 = submarkdefault[3],
                    SubMark5 = submarkdefault[4],
                    SubMark6 = submarkdefault[5],
                }); ;
                //添加打分日志
                string logtransname = "FirstMark";
                int preval = 5;
                int nowval = mark > 20 ? 5 : 7;
                _statuslog.Add(new StatusLog
                {
                    TopicId = maid,
                    TransDate = DateTime.Now,
                    TransBy = u.UserName,
                    TransName = logtransname,
                    PreStaus = preval,
                    NowStatus = nowval,
                    Attr2 = mark.ToString()
                });

                var topic = _topic.GetById(maid);
                topic.LastReplyTime = System.DateTime.Now;
                if (mark <=20)
                {
                    topic.Type = TopicType.Perfect;
                }
                topic.TopicMark = mark;
                _topic.Edit(topic);
                exin = "评分成功";
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
    }
}