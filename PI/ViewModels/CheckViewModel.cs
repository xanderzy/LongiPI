using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PI.ViewModels
{
    public class CheckViewModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string RealName { get; set; }

        [Required]
        public string Department { get; set; }

        [Required]
        public int ReplyId { get; set; }
    }
}
