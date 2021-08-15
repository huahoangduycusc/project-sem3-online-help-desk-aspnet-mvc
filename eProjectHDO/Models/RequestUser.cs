using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eProjectHDO.Models
{
    public class RequestUser
    {
        public int requestId { get; set; }
        public string requestTitle { get; set; }
        public string description { get; set; }
        public string facility { get; set; }
        public string type { get; set; }
        public string severity { get; set; }
        public string status { get; set; }
        public string username { get; set; }
        public int userId { get; set; }
        public string created_at { get; set; }
        public int assigned { get; set; }
        public string assignedPerson { get; set; }
        public int block { get; set; }
    }
}