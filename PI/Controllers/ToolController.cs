using System;
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

namespace PI.Controllers
{
    public class ToolController : Controller
    {
        private ITopicRepository _topic;
        public UserManager<User> UserManager { get; }
        private readonly DataContext _context;
        private IMyFileRepository _myfile;
        private IHostingEnvironment _hostingEnv;
       

        public ToolController(ITopicRepository topic, UserManager<User> userManager, IMyFileRepository myfile, IHostingEnvironment hostingEnv, DataContext context)
        {
            _topic = topic;
            UserManager = userManager;
            _myfile = myfile;
            _hostingEnv = hostingEnv;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }


        //所有的文件图片上传接口
        public async Task<IActionResult> FileUpload(string uploadtype,int tid)
        {
            var files = Request.Form.Files;
            int fileid = 0;
            string filePath = "";
            string result = "";
            string realfilename = "";
            string backfilename = "";
            string imgurl = "";

            //这一段用来判断所有的文件上传-uploadtype直接代表fileicon好了
            string cunfangfile = "/upload2020/";
            if (uploadtype == "P")
            {
                cunfangfile = "/TopicImg2020/";
            }
            foreach (var formFile in files)
            {
                var nu = UserManager.GetUserAsync(User).Result;
                string username = "noone";
                if (nu != null)
                {
                    username=nu.UserName;
                }
                //默认，如果是noone的人员代码上传取消了
                string webRootPath = _hostingEnv.WebRootPath;
                DateTime dt = DateTime.Now;
                string filedate = dt.ToString("yyyyMMddHHmmss");
                string fileExt = Path.GetExtension(formFile.FileName);
                realfilename = Path.GetFileNameWithoutExtension(formFile.FileName);
                long fileSize = formFile.Length;
                backfilename = realfilename + fileExt;
                string newFileName = filedate + realfilename + fileExt;
                filePath = webRootPath + cunfangfile + newFileName;
                imgurl = "https://jxta.longi-silicon.com:9515" + cunfangfile+ newFileName;
                //imgurl= cunfangfile + newFileName;
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                        stream.Flush();
                        if (System.IO.File.Exists(filePath))
                        {
                           fileid=_myfile.Add(new MyFile
                            {
                                CreateDt = dt,
                                ModifyDt = dt,
                                FileExt = fileExt,
                                FileName = realfilename,
                                FilePath = filePath,
                                FileSize = (int)fileSize,
                                FileIcon = uploadtype,
                                TopicId = tid,
                                Uploader = username
                            });
                        }
                        else
                        {
                            result = JsonConvert.SerializeObject(new { Success = false, errno = 1, Errormsg = "服务器IO错误,请重新上传" });
                            return Content(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = JsonConvert.SerializeObject(new { Success = false, errno = 1, Errormsg = ex.ToString() });
                    return Content(result);
                }
            }
            string[] pathback = { imgurl };
            result = JsonConvert.SerializeObject(new { Success = true,errno=0,Fid = fileid, FName = realfilename, data = pathback});
            return Content(result);
        }

        //删除文件
        public IActionResult DeleteFile(int dfid, int tid)
        {
            string resstr = "";
            var fileinfo = _myfile.GetById(dfid);
            string fileFullPath = fileinfo.FilePath;
            if (System.IO.File.Exists(fileFullPath))
            {
                try
                {
                    //删除成功后，要把topic里面的hasupload更新掉。。。
                    System.IO.File.Delete(fileFullPath);
                    fileinfo.IsDelete = 1;
                    _myfile.Edit(fileinfo);
                    if (tid != 0)
                    {
                        var ttopic = _topic.GetById(tid);
                        string[] result = ttopic.HasUpload.Split(',');
                        string newhasupload = "";
                        for (int i = 0; i < result.Length; i++)
                        {
                            if (result[i] != dfid.ToString() && result[i] != "")
                            {
                                if (i == 0)
                                {
                                    newhasupload = result[i];
                                }
                                else
                                {
                                    newhasupload = newhasupload + "," + result[i];
                                }
                            }
                        }
                        if (newhasupload == "") { newhasupload = "0"; }
                        ttopic.HasUpload = newhasupload;
                        _topic.Edit(ttopic);
                    }
                    resstr = JsonConvert.SerializeObject(new { success = true, exinfo = "删除成功" });
                    return Content(resstr);
                }
                catch (Exception ex)
                {
                    resstr = JsonConvert.SerializeObject(new { success = false, exinfo = ex.ToString() });
                    return Content(resstr);
                }
            }
            else
            {
                resstr = JsonConvert.SerializeObject(new { success = false, exinfo = "找不到相关文件，请联系管理员" });
                return Content(resstr);
            }

        }
        //文件下载接口
        //下载附件功能
        public IActionResult DownLoad(int fiid)
        {
            var thefile = _myfile.GetById(fiid);
            if (thefile is null)
            {
                return Content("文件下载出现异常，请联系管理员");
            }
            var addUrl = thefile.FilePath;
            string fileExt = thefile.FileExt;
            var realname = thefile.FileName + fileExt;
            try
            {
                var stream = System.IO.File.OpenRead(addUrl);
                var provider = new FileExtensionContentTypeProvider();
                var memi = provider.Mappings[fileExt];
                return File(stream, memi, realname);
            }
            catch (Exception ex)
            {
                return Content("文件下载出现异常，请联系管理员，异常如下:" + ex.ToString());
            }
        }

        //根据姓名获取用户数据接口
        public IActionResult GetUserInfo(string realname)
        {
            string msg = "";
            string resstr = "";
            var users = _context.Users.AsQueryable();
            var isuserexist = users.Where(r => r.RealName == realname).Select(r => r.UserName).ToList();
            int userconut = isuserexist.Count();
            if (userconut > 1)
            {
                msg = "系统内存在同名用户超过两个人，请联系管理员修改";
                resstr = JsonConvert.SerializeObject(new { code = 1, msg = msg });
                return Content(resstr);
            }
            if (userconut == 0)
            {
                msg = "此用户不存在";
                resstr = JsonConvert.SerializeObject(new { code = 1, msg = msg });
                return Content(resstr);
            }
            string tname = isuserexist.FirstOrDefault();
            var u = UserManager.FindByNameAsync(tname).Result;
            resstr = JsonConvert.SerializeObject(new { code = 0, msg = "获取成功", data = u });
            return Content(resstr);
        }


        public IActionResult GetFileInfo(string filehasupload)
        {
            string[] filearry = filehasupload.Split(',');
            List<int> flist = new List<int>();
            for (int i = 0; i < filearry.Length; i++)
            {
                flist.Add(Convert.ToInt32(filearry[i]));
            }
            var filelist = _myfile.List(r => flist.Contains(r.Id)).Select(r=> new { r.FileName,r.Id}).ToList();
            string resfileinfo = JsonConvert.SerializeObject(new { code = 0, msg = "获取成功", data = filelist });
            return Content(resfileinfo);
        }

        public IActionResult GetTopicFile(int tid)
        {   
            var filelist=_myfile.List(r=>r.TopicId==tid&&r.FileIcon == "W"&&r.IsDelete==0).Select(r => new { r.Id}).ToList();
            string resfileinfo = JsonConvert.SerializeObject(new { code = 0, msg = "获取成功", data = filelist });
            return Content(resfileinfo);
        }

    }
}