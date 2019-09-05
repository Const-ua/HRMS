using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Hrms.Models
{
    public class Aspirant :ICloneable
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Skype { get; set; }
        public int Salary { get; set; }
        public string Education { get; set; }
        public int Sex { get; set; }
        public DateTime BirthDay { get; set; }
        public int VacancyId { get; set; }
        public int Experience { get; set; }
        public byte[] Photo { get; set; }
        [AllowHtml] public string CvText { get; set; }
        public string Skills { get; set; }
        public DateTime Date { get; set; }
        public int Status { get; set; } //1 - новый; 2-сделан оффер; 3-принять оффер; 4- принят на работу.
        public int ProfileId { get; set; }

        public Profile Profile { get; set; } // Профиль рекрутера
        public Vacancy Vacancy { get; set; }
        public List<Interview> Interviews { get; set; }

        public object Clone()
        {
           return this.MemberwiseClone();
        }
    }
}