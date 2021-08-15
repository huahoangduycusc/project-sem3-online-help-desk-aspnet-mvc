using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eProjectHDO.Models
{
    public class Baiviet
    {
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string thumbnail { get; set; }
        public int userId { get; set; }
        public string author { get; set; }
        public int view { get; set; }
        public string created_at { get; set; }
    }
}