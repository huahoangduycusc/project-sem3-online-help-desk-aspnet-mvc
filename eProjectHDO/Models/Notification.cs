//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace eProjectHDO.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Notification
    {
        public int NotiId { get; set; }
        public string message { get; set; }
        public Nullable<int> status { get; set; }
        public Nullable<int> userId { get; set; }
        public Nullable<System.DateTime> created_at { get; set; }
        public Nullable<int> requestId { get; set; }
    
        public virtual Request Request { get; set; }
        public virtual User User { get; set; }
    }
}
