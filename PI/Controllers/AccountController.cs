using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ApplicationCore.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PI.ViewModels;

namespace PI.Controllers
{
    public class AccountController : Controller
    {
        public UserManager<User> UserManager { get; }
        public SignInManager<User> SignInManager { get; }
        public AccountController(UserManager<User> userManager,SignInManager<User> signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }
        public IActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "用户名或密码错误");
                return View(model);
            }
        }


        public async Task<ActionResult> LogOff()
        {
            //var userName = HttpContext.User.Identity.Name;
            await SignInManager.SignOutAsync();
            //_logger.LogInformation("{userName} logged out.", userName);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> ForgotPassword(ForgetViewModel model)
        {
            if (ModelState.IsValid)
            {
                var u = UserManager.GetUserAsync(User).Result;
                string password = model.Password.ToString();
                u.PasswordHash = UserManager.PasswordHasher.HashPassword(u, password);
                IdentityResult result = await UserManager.UpdateAsync(u);
                if (result.Succeeded)
                {
                    return RedirectToAction("Login");
                }
            }
            return Content("信息错误");
        }

        [HttpPost]
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
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return Content("注册失败");
                }

            }
            return Content("注册信息填写不完整");
        }


    }
}