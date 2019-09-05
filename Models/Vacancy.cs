using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Hrms.Models
{
    public class Vacancy
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PositionId { get; set; }
        public int DepartmentId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string Skills { get; set; }
        public int Priority { get; set; }
        public int Salary { get; set; }
        public int Experience { get; set; }
        [AllowHtml] public string Comments { get; set; }


        public List<Aspirant> Aspirants { get; set; }
        public Position Position { get; set; }
        public Department Department { get; set; }
    }
}