using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hrms.Models
{
    public class Interview
    {
        public int Id { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int AspirantId { get; set; }
        public string  Comments { get; set; }
        public int ResultId  { get; set; } // 1-не проводилось, 2-одобрен, 3-резерв, 4-решить позже, 5-отклонен, 6-перенесено

        public Result Result { get; set; }
        public Aspirant Aspirant { get; set; }
    }
}