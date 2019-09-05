using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hrms.Models
{
    public class NewsItem
    {
        public int Id { get; set; }
        public string Header { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public string Priority { get; set; }//{ "Ordinar", "Important", "Urgent" };
        public byte[] Photo { get; set; }
    }
}