using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hrms.Models
{
    public class Department :IComparable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Budget { get; set; }

        public List<Profile> Profiles { get; set; }
        public List<Vacancy> Vacancies { get; set; }

        public int CompareTo(object o)
        {
            Department d = o as Department;
            if (d != null)
                return this.Name.CompareTo(d.Name);
            else
                throw new Exception("Невозможно сравнить два объекта");
        }
    }
}