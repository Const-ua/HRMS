using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hrms.Models
{
    public class Message
    {
        public int Id { get; set; }
        
        public int ProfileId { get; set; }
        public string Mesg { get; set; }
        public DateTime Date { get; set; }
        public Profile Profile { get; set; }
    }
}