using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationCore.Interfaces;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace PI.Controllers
{
    public class ChartController : Controller
    {
        private readonly DataContext _context;
        private ITopicRepository _topic;
        public ChartController(DataContext context,ITopicRepository topic)
        {
            _context = context;
            _topic = topic;
        }
         public IActionResult Index()
        {
            return View();
        }

        public IActionResult XqView(int depindex)
        {
            string[] deparr =new string[] { "IE运营部", "财务部", "采购履行部", "仓储物流部", "动力部", "计划物控部", "技术部", "人力资源部", "设备部", "生产二组", "生产三组", "生产一组", "质量部", "总经理办公室" };
            string depname = deparr[depindex];
           
            ViewBag.Depname = depname;
            return View();
        }

        public IActionResult GetTwoCharts()
        {
            //var trenddata = _context.MonthTrends;
            var depalldata = _context.Depalldatas;
            var depstatusdata = _context.Depstatusdatas;
            string resstr = JsonConvert.SerializeObject(new { code = 0, msg = "查询成功",ddata=depalldata,sdata= depstatusdata });
            return Content(resstr);
        }

        public IActionResult GetXqCharts()
        {
            var depalldata = _context.Depalldatas;
            var depstatusdata = _context.Depstatusdatas;
            var depmark = _context.DepMark;
            var goodtopic = _topic.List(r => r.ZongbuMark > 0&&r.CreateOn > Convert.ToDateTime("2020-01-01 00:00:00")).Select(r => new
            {
                r.Id,
                r.Title,
                r.CreateOn,
                r.TopicMark,
                r.ZongbuMark
            }).OrderByDescending(r => r.ZongbuMark).Take(7).ToList();
            string resstr = JsonConvert.SerializeObject(new { code = 0, msg = "查询成功", ddata = depalldata, sdata = depstatusdata,mdata= depmark,goodtopic= goodtopic });
            return Content(resstr);
        }
    }
}