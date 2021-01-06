using System;
using System.Collections.Generic;
 using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
 using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
 using Newtonsoft.Json;
using OfficeOpenXml;
 using PI.ViewModels;

namespace PI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize("Admin")]
    public class AdminUserController : Controller
    {
        private readonly DataContext _context;
        private IMyFileRepository _myfile;
        private IHostingEnvironment _hostingEnv;

        public UserManager<User> UserManager { get; }
        public AdminUserController(DataContext context, UserManager<User> userManager, IHostingEnvironment hostingEnv, IMyFileRepository myfile)
        {
            _context = context;
            UserManager = userManager;
            _hostingEnv = hostingEnv;
            _myfile = myfile;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult FileList()
        {
            return View();
        }

        public IActionResult GetDelFileList(int page, int limit)
        {
            var delfielist = _myfile.List(r => r.IsDelete == 1).ToList();
            int total = delfielist.Count();
            var rows = delfielist.Skip((page - 1) * limit).Take(limit).OrderByDescending(r => r.ModifyDt).ToList();
            string str = JsonConvert.SerializeObject(new { code = 0, msg = "查询成功", count = total, data = rows });
            return Content(str);
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User { UserName = model.UserName.Trim(), Email = model.Email, Department = model.Department, CreateOn = DateTime.Now, LastTime = DateTime.Now, RealName = model.RealName.Trim() };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (user.UserName.ToLower().Equals("admin"))
                    {
                        await UserManager.AddClaimAsync(user, new Claim("Admin", "Allowed"));
                    }
                    return RedirectToAction("Index", "AdminUser", new { area = "Admin" });
                }
                else
                {
                    return Content("注册失败");
                }

            }
            return Content("注册信息填写不完整");
        }

        public IActionResult GetUserIndex(int limit, int page, string username)
        {
            var uresult = from u in _context.Users
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
            if (!string.IsNullOrEmpty(username)) { uresult = uresult.Where(r => r.RealName == username || r.UserName == username); }
            var list1 = uresult.ToList();
            var total = list1.Count();
            var rows = list1.Skip((page - 1) * limit).Take(limit).OrderByDescending(r => r.CreateOn).ToList();
            string str = JsonConvert.SerializeObject(new { code = 0, msg = "查询成功", count = total, data = rows });
            return Content(str);
        }

        //密码重置
        public async Task<IActionResult> ResetPassword(string username,string newpass)
        {
            if (!string.IsNullOrEmpty(username))
            {
                var u = UserManager.FindByNameAsync(username).Result;
                 u.PasswordHash = UserManager.PasswordHasher.HashPassword(u, newpass);
                IdentityResult result = await UserManager.UpdateAsync(u);
                string str = JsonConvert.SerializeObject(new { code = 0, msg = "重置成功" });
                return Content(str);
            }
            else
            {
                string str = JsonConvert.SerializeObject(new { code = 1, msg = "重置密码失败" });
                return Content(str);
            }
        }
        public async Task<IActionResult> UserEditSave(string Email, string Department, string RealName, string UserName)
        {
            string ustr = "";
            var user = UserManager.FindByNameAsync(UserName).Result;
            if (ModelState.IsValid)
            {
                user.Email = Email;
                user.RealName = RealName;
                user.Department = Department;
                await UserManager.UpdateAsync(user);
                ustr = JsonConvert.SerializeObject(new { success = true, msg = "更新成功" });
                return Content(ustr);
            }
            ustr = JsonConvert.SerializeObject(new { success = false, msg = "状态异常，更新失败" });
            return Content(ustr);
        }
    }

       
}