using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hrms.Models
{
    public class Goal
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public int LmId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Comments { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int Status { get; set; }

        public Profile Profile { get; set; }

    }
}