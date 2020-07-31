using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace PI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize("Admin")]
    public class NoticeController : Controller
    {

        private ITopicRepository _topic;
        public UserManager<User> UserManager { get; }

        public NoticeController(ITopicRepository topic, UserManager<User> userManager)
        {
            _topic = topic;
            UserManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddNotice(Topic topic)
        {
             try
                {
                    string tname = "Admin";
                    var u = UserManager.FindByNameAsync(tname).Result;
                    string userid = u.Id;
                    topic.UserId = userid;
                    topic.LastReplyTime = DateTime.Now;
                    topic.LastReplyUserId = userid;
                    topic.UserName = u.UserName;
                    topic.Email = u.Email;
                    topic.CreateOn = DateTime.Now;
                    topic.Type = TopicType.Notice;
                    topic.Suggest = "Notice";
                    topic.ReplyCount = 0;
                    topic.HasUpload = "0";
                     _topic.Add(topic);
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    return Content("数据更新异常，发布提案失败");
                }
        }
    }
}