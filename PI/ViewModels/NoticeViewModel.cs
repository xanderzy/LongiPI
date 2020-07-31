using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PI.ViewModels
{
    public class NoticeViewModel
    {
        public int Id { get; set; }

        public string  Title { get; set; }
        public  DateTime CreateOn{ get; set; }
        public string UserName { get; set; }
    }
}
