using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PI.Controllers
{
    public class WeChatController : Controller
    {
        private ITopicRepository _topic;
        private UserManager<User> UserManager;
        public SignInManager<User> SignInManager { get; }

        private ITopicReplyRepository _reply;
        private IStatusLogRepository _statuslog;
        private IMyFileRepository _myfile;
        //private IHostingEnvironment _hostingEnv;
        private readonly DataContext _context;
        public WeChatController(ITopicRepository topic, ITopicReplyRepository reply, UserManager<User> userManager,IMyFileRepository myfile, DataContext context, IStatusLogRepository statuslog, SignInManager<User> signInManager)
        {
            _topic = topic;
            _reply = reply;
            _myfile = myfile;
             UserManager = userManager;
            SignInManager = signInManager;
            // _hostingEnv = hostingEnv;
            _context = context;
            _statuslog = statuslog;

        }
        // GET: /<controller>/
        public IActionResult Index()
        {
            string str = JsonConvert.SerializeObject(new { code = 0, msg = "查询成功", count = 100 });
            return Content(str);

        }

       [HttpPost]
         public async Task<IActionResult> WxLogin(string username, string password)
        {
            string str = "";
           var result = await SignInManager.PasswordSignInAsync(username, password, false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
               str = JsonConvert.SerializeObject(new { code = 1, msg = "工号密码错误" });
               return Content(str);
            }
            else
            {
                str = JsonConvert.SerializeObject(new { code = 0, msg = "登陆成功", data = username });
                return Content(str);
            }
             
        }

        //[HttpPost]
        //跟微信服务器对接
        /*public async Task<IActionResult> WxLogin(string code, string username, string password, int type)
        {
            //暂时不加密了，没啥影响
            string str = "";
            string thirdsession = "";
            //验证username跟password是否OK type为1则是从登陆页面发出的请求，type为2则是重新获取thirdsession
            if (type == 1)
            {
                var result = await SignInManager.PasswordSignInAsync(username, password, false, lockoutOnFailure: false);
                if (!result.Succeeded)
                {
                    str = JsonConvert.SerializeObject(new { code = 1, msg = "工号密码错误" });
                    return Content(str);
                }
            }
            if (code != "")
            {
                //用户信息获取
                string apiUrl = string.Format("https://api.weixin.qq.com/sns/jscode2session?appid={0}&secret={1}&js_code={2}&grant_type=authorization_code", "wx9e9aaf96a95acf6e", "ee3d2606b83ed3ecada8c8c1048f2fac", code);
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(apiUrl);
                myRequest.Method = "GET";
                HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
                StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
                string content = reader.ReadToEnd();
                myResponse.Close();
                reader.Close();
                reader.Dispose();
                wxModel ReMsg = JsonConvert.DeserializeObject<wxModel>(content);
                if (ReMsg.openid==null)
                {
                    str = JsonConvert.SerializeObject(new { code = 1, msg = "与微信服务器连接失败" });
                    return Content(str);
                }
                //用openid去数据库读取下有无数据
                bool haswxuser = _context.UserMessages.Where(r => r.OpenId == ReMsg.openid).Any();
                //微信跟系统用户是否存在关联-没有存在关联-看是否username跟password为空，空的话返回code=2，跳转到login页面
                if (!haswxuser)
                {
                    if (type == 2)
                    {
                        str = JsonConvert.SerializeObject(new { code = 2, msg = "请进行用户登录" });
                        return Content(str);
                    }
                    //生成thirdsession 
                     MD5 md5 = MD5.Create();
                     byte[] bs = Encoding.UTF8.GetBytes(ReMsg.openid + ReMsg.session_key);
                     byte[] hs = md5.ComputeHash(bs);
                     StringBuilder sb = new StringBuilder();
                     foreach (byte b in hs)
                     {
                         sb.Append(b.ToString("x2"));
                     }
                     thirdsession = sb.ToString();
                    //向数据库插入记录
                    thirdsession = username;
                    _context.Set<UserMessage>().Add(new UserMessage
                    {
                        ThirdSession ="",
                        UserName = username,
                        OpenId = ReMsg.openid,
                        Sessionkey = ReMsg.session_key,
                        Attr2 = "",
                        CreateOn = DateTime.Now
                    });
                    _context.SaveChanges();
                }
                //重新获取下数据库内的用户名
                else
                {
                    thirdsession = _context.UserMessages.Where(r => r.OpenId == ReMsg.openid).Select(r => r.UserName).FirstOrDefault();
                    if (thirdsession == null || thirdsession == "")
                    {
                        str = JsonConvert.SerializeObject(new { code = 1, msg = "解析用户失败" });
                        return Content(str);
                    }
                }
                str = JsonConvert.SerializeObject(new { code = 0, msg = "登陆成功", data = thirdsession });
                return Content(str);

            }
            else
            {
                str = JsonConvert.SerializeObject(new { code = 1, msg = "登陆失败，无法获取微信code" });
                return Content(str);
            }
        }*/

        public IActionResult CheckByTeamLeader(int tid)
        {
            var ctopicdata = _topic.GetById(tid);
            string str = JsonConvert.SerializeObject(new { code = 0, msg = "查询成功", data = ctopicdata });
            return Content(str);
        }

        //用户提案列表
        public IActionResult GetUserTRs(int type, int pageIndex, int pageSize, string username, string skey)
        {
            string searchname = "";
            if (username == null)
            {
                var u = UserManager.GetUserAsync(User).Result;
                searchname = u.UserName;
            }
            else
            {
                searchname = username;
            }
            //type代表查询状态，1代表实施中，2代表审核中，3代表已完结，4代表全部
            
            //所有发布的提案
            var topiclst = (
            from c in _context.Topics
            where c.UserName == searchname
            select new
            {
                c.Id,
                c.Title,
                c.CreateOn,
                c.ReplyCount,
                c.User.RealName,
                c.NodeId,
                c.Type,
                c.Top,
                c.TopicMark,
                c.ZongbuMark
            });
            var replylist = (
                from c in _context.Topics
                join s in _context.TopicReplys
                on c.Id equals s.TopicId
                where s.ReplyUserName == searchname && s.ReplyType == 0&&c.UserName!= searchname
                select new
                {
                    c.Id,
                    c.Title,
                    c.CreateOn,
                    c.ReplyCount,
                    c.User.RealName,
                    c.NodeId,
                    c.Type,
                    c.Top,
                    c.TopicMark,
                    c.ZongbuMark
                });
            var alllist = topiclst.Concat(replylist);
            if (skey != "" && skey != null)
            {
                alllist = alllist.Where(r => r.Title.Contains(skey));
            }
            else
            {
                switch (type)
                {
                    case 1: alllist = alllist.Where(r => r.Type == TopicType.Good); break;
                    case 2: alllist = alllist.Where(r => r.Type == TopicType.TeamLeaderCheck || r.Type == TopicType.AdminCheck); break;
                    case 3: alllist = alllist.Where(r => r.Type == TopicType.Perfect || r.Type == TopicType.Top || r.Type == TopicType.Marking); break;
                    case 5: alllist = alllist.Where(r => r.Type == TopicType.Delete || r.Type == TopicType.Breakup); break;
                    default: break;
                }
            }
            int curcount = alllist.Count();
            var backlist = alllist.OrderByDescending(r => r.CreateOn).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            string str = JsonConvert.SerializeObject(new { code = 0, msg = "成功", total = curcount, rows = backlist });
            return Content(str);
        }

        //提案页面数据
        public IActionResult GetTopicInfo(int tid)
        {
            var topicdetail = _topic.GetById(tid);
            var filelist = _myfile.List(r => r.TopicId == tid && r.IsDelete==0&&r.FileIcon!="P").Select(r=>new { 
               r.Id,
               r.FileName,
               r.FileExt
            }).ToList();
            string str = JsonConvert.SerializeObject(new { code = 0, msg = "成功", total = 1, rows = topicdetail,files= filelist });
            return Content(str);
        }

        //提案流程数据
        public IActionResult GetStatusLog(int tid)
        {
            var statuslog = from a in _context.StatusLogs
                            join ar in _context.Users
                            on a.TransBy equals ar.UserName
                            orderby a.TransDate
                            where a.TopicId == tid
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
            int scount = statuslog.Count();
            string str = JsonConvert.SerializeObject(new { code = 0, msg = "成功", total = scount, rows = statuslog.ToList() });
            return Content(str);
        }

        //添加实施人-实施人员验证
        public IActionResult AddReplybyWeChat(string replyers, int topicid, string addbys)
        {
            string exinfo = "";
            string str = "";
            string[] susername = replyers.Split('，');
            int arrlength = susername.Length;
            bool isrepeat = false;
            if (arrlength > 1)
            {
                Hashtable ht = new Hashtable();
                for (int i = 0; i < arrlength; i++)
                {
                    if (ht.Contains(susername[i]))
                    {
                        isrepeat = true;
                        break;
                    }
                    else
                    {
                        ht.Add(susername[i], susername[i]);
                    }
                }
            }
            if (isrepeat)
            {
                exinfo = "脑子有坑么加两个同样的名字？？";
                str = JsonConvert.SerializeObject(new { success = false, ex = exinfo });
                return Content(str);
            }
            //首先 将username进行序列化，然后进行循环验证-验证所有用户名没有问题 -没问题进行实施人添加，然后进行通过
            //var users = _context.Users.AsQueryable();

            for (int i = 0; i < arrlength; i++)
            {
                var isuserexist = _context.Users.Where(r => r.RealName == susername[i]).Select(r => new
                {
                    r.UserName
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
            }

            for (int i = 0; i < arrlength; i++)
            {
                var replyinfos = _context.Users.Where(r => r.RealName == susername[i]).Select(r => new
                {
                    r.Id,
                    r.Email,
                    r.UserName
                }).ToList();
                try
                {
                    _reply.Add(new TopicReply
                    {
                        ReplyUserId = replyinfos[0].Id,
                        CreateOn = DateTime.Now,
                        ReplyEmail = replyinfos[0].Email,
                        ReplyUserName = replyinfos[0].UserName,
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
                            Attr2 = replyers
                        });
                    }
                }
                catch (Exception ex)
                {
                    exinfo = "添加失败，出现未知异常,请联系管理员";
                    str = JsonConvert.SerializeObject(new { success = false, ex = ex.ToString() });
                    return Content(str);
                }
            }
            var topic = _topic.GetById(topicid);
            topic.LastReplyTime = DateTime.Now;
            topic.ReplyCount += arrlength;
            _topic.Edit(topic);
            exinfo = "添加成功";
            str = JsonConvert.SerializeObject(new { success = true, ex = exinfo });
            return Content(str);
        }

        //获取userinfo数据
        public IActionResult GetUserInfo(string username)
        {
            var userinfos = _context.Users.Where(r => r.UserName == username).Select(r => new
            {
                r.RealName,
                r.Department,
                r.Email,
                r.TopicCount,
                r.TopicReplyCount
            }).ToList();
             string  str = JsonConvert.SerializeObject(new { code = 0, msg="查询成功",data=userinfos });
            return Content(str);
        }

        [HttpPost]
        //用户信息修改
        public async Task<IActionResult> UserEdit(string username,string realname,string dep,string email)
        {
            var user = UserManager.FindByNameAsync(username).Result;
            user.Email = email;
            user.RealName = realname;
            user.Department = dep;
            await UserManager.UpdateAsync(user);
            string str = JsonConvert.SerializeObject(new { code = 0, msg = "操作成功", data="" });
            return Content(str);

        }
    }
}
