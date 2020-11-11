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
        public UserController(DataContext context, UserManager<User> userManager, ITopicRepository topic, IMarkInfoRepository markinfo)
        {
            _context = context;
            UserManager = userManager;
            _topic = topic;
            _markinfo = markinfo;
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
            return Content("信息错误");
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
            var markbylist = (
                from c in _context.Topics
                join s in _context.MarkInfos
                on c.Id equals s.TopicId
                where c.Type == TopicType.Marking && s.MarkBy == u.UserName&&s.Mark==1001
                select new
                {
                    c.Id,
                    c.Title,
                    s.CreateDate,
                });
            var mcss = markbylist.OrderBy(r => r.CreateDate).ToList();
            int count = mcss.Count();
            var list = mcss.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            string str = JsonConvert.SerializeObject(new { total = count, rows = list });
            return Content(str);
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
    }
}