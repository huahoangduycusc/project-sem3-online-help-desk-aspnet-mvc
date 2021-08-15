using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eProjectHDO.Models
{
    public class FacilityHead
    {
        public int toDo { get; set; }
        public int inProcess { get; set; }
        public int completed { get; set; }
        public int issues { get; set; }
        public IEnumerable<RequestUser> listRequest { get; set; }
    }
}