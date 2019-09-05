using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hrms.Models
{
    public class Role : IComparable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Profile> Profiles { get; set; }

        public int CompareTo(object o)
        {
            Role r = o as Role;
            if (r != null)
                return this.Name.CompareTo(r.Name);
            else
                throw new Exception("Невозможно сравнить два объекта");
        }
    }
}