using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hrms.Models
{
    public class Profile
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string Passport { get; set; }
        public string Phone { get; set; }
        public string Skype { get; set; }
        public DateTime BirthDay { get; set; }
        public string TaxCode { get; set; }
        public int PositionId { get; set; }
        public int DepartmentId { get; set; }
        public byte[] Photo { get; set; }
        public Position Position { get; set; }
        public string HashPwd { get; set; }
        public int RoleId { get; set; }


        public Role Role { get; set; }
        public Department Department { get; set; }
        public List<Message> Messages { get; set; }    
        public List<Active> Actives { get; set; }
        public List<Goal> Goals { get; set; }

        public void GetProperties(Profile usr)
        {
            this.Id = usr.Id;
            this.Email = usr.Email;
            this.HashPwd = usr.HashPwd;
            this.RoleId = usr.RoleId;
            this.Role = usr.Role;
        }

    }

}