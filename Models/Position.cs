using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hrms.Models
{
    public class Position : IComparable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DepId { get; set; }
        public int SalaryFrom { get; set; }
        public int SalaryTo { get; set; }
        public int Experience { get; set; }
        public string FullTime { get; set; }
        public List<Profile> Profiles { get; set; }
        public List<Vacancy> Vacancies { get; set; }

        public int CompareTo(object o)
        {
            Position p = o as Position;
            if (p != null)
                return this.Name.CompareTo(p.Name);
            else
                throw new Exception("Невозможно сравнить два объекта");
        }

    }
}