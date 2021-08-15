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
    
    public partial class Request
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Request()
        {
            this.RequestUpdateds = new HashSet<RequestUpdated>();
            this.Notifications = new HashSet<Notification>();
        }
    
        public int RequestId { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public Nullable<int> severity { get; set; }
        public Nullable<int> requestor { get; set; }
        public Nullable<int> facility { get; set; }
        public Nullable<int> assigned { get; set; }
        public Nullable<int> status { get; set; }
        public string remarks { get; set; }
        public Nullable<System.DateTime> created_at { get; set; }
        public Nullable<int> block { get; set; }
        public Nullable<int> typefacility { get; set; }
    
        public virtual Facility Facility1 { get; set; }
        public virtual User User { get; set; }
        public virtual Severity Severity1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RequestUpdated> RequestUpdateds { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual Status Status1 { get; set; }
        public virtual TypeOfFacility TypeOfFacility { get; set; }
    }
}
