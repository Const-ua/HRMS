using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hrms.Models
{
    public class MainFormModel
    {
        public List<NewsItem> News { get; set; }
        public List<Message> Chat { get; set; }
        public Profile UserProfile { get; set; }

    }
}