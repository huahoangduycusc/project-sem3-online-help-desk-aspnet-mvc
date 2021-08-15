using eProjectHDO.Models;
using PagedList;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eProjectHDO.Controllers
{
    public class PanelController : Controller
    {
        ohdEntities db = new ohdEntities();
        // dashbroad index
        [Authorize(Roles ="head")]
        [Obsolete]
        public ActionResult Index(int? page)
        {
            int facili = getFacilityofUser();     
            TempData["toDo"] = db.Requests.Where(m => m.facility == facili).Count();
            TempData["inProcess"] = db.Requests.Where(m => m.facility == facili && m.status == 5).Count();
            TempData["completed"] = db.Requests.Where(m => m.facility == facili && m.status == 8).Count();
            TempData["issues"] = db.Requests.Where(m => m.facility == facili && m.status == 7 || m.status == 6).Count();
            TempData["facility"] = db.Facilities.Where(m => m.FacilityId == facili).FirstOrDefault().FacilityName;
            TempData["assigned"] = db.Users.Where(m => m.facility == facili && m.role == 2).ToList(); // role = 2 (Assigned person)
            TempData.Keep("assigned");
            DateTime past = DateTime.Now.AddDays(-30);
            // pending
            var request = (from r in db.Requests
                           where r.block != 1 && r.facility == facili && r.created_at > past
                           group r by EntityFunctions.TruncateTime(r.created_at) into grp
                           select new
                           {
                               key = grp.Key,
                               value = grp.Count()
                           });
            // success
            var request1 = (from r in db.Requests
                            where r.block != 1 && r.facility == facili && r.status == 8 && r.created_at > past
                            group r by EntityFunctions.TruncateTime(r.created_at) into grp
                            select new
                            {
                                key = grp.Key,
                                value = grp.Count()
                            });
            // date
            List<string> date = new List<string>();
            request.ToList().ForEach(m => date.Add(m.key.ToString()));
            // request 
            List<int> pending = new List<int>();
            request.ToList().ForEach(m => pending.Add(m.value));
            // request is work in success
            List<int> success = new List<int>();
            request1.ToList().ForEach(m => success.Add(m.value));
            ViewBag.pending = pending;
            ViewBag.success = success;
            ViewBag.date = date; 
            return View();
        }
        // list of request
        [Authorize(Roles ="head")]
        public ActionResult List(int? page, int type = 0)
        {
            int facili = getFacilityofUser();
            TempData["assigned"] = db.Users.Where(m => m.facility == facili && m.role == 2).ToList(); // role = 2 (Assigned person)
            TempData.Keep("assigned");
            TempData["facility"] = db.Facilities.Where(m => m.FacilityId == facili).FirstOrDefault().FacilityName;
            if(type == 0)
            {
                var requests = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join se in db.Severities on r.severity equals se.SeverityId
                          join u in db.Users on r.requestor equals u.UserId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.facility == facili && r.block == 0
                          select new eProjectHDO.Models.RequestUser
                          {
                              requestId = r.RequestId,
                              requestTitle = r.title,
                              facility = f.FacilityName,
                              type = t.TypeName,
                              username = u.fullname,
                              userId = u.UserId,
                              severity = se.SeverityName,
                              assigned = (int)r.assigned,
                              assignedPerson = db.Users.Where(m => m.UserId == r.assigned).FirstOrDefault().fullname,
                              status = s.StatusName,
                              created_at = r.created_at.ToString()
                          }).OrderByDescending(m => m.requestId).ToList().ToPagedList(page ?? 1, 10); // get list of request 
                return View(requests);
            }
            else
            {
                var requests = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join se in db.Severities on r.severity equals se.SeverityId
                          join u in db.Users on r.requestor equals u.UserId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.facility == facili && r.block == 0 && r.status == type
                          select new eProjectHDO.Models.RequestUser
                          {
                              requestId = r.RequestId,
                              requestTitle = r.title,
                              facility = f.FacilityName,
                              type = t.TypeName,
                              username = u.fullname,
                              userId = u.UserId,
                              severity = se.SeverityName,
                              assigned = (int)r.assigned,
                              assignedPerson = db.Users.Where(m => m.UserId == r.assigned).FirstOrDefault().fullname,
                              status = s.StatusName,
                              created_at = r.created_at.ToString()
                          }).OrderByDescending(m => m.requestId).ToList().ToPagedList(page ?? 1, 10); // get list of request 
                return View(requests);
            }
        }
        // facility of user
        public int getFacilityofUser()
        {
            int userId = (int)Session["uid"];
            var result = db.Users.Where(m => m.UserId == userId).FirstOrDefault(); // get facility of user
            return (int)result.facility;
        }
        // view request ajax
        [Authorize(Roles ="head,assigned,admin")]
        public ActionResult viewRequest(int id)
        {
            var result = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join u in db.Users on r.requestor equals u.UserId
                          join se in db.Severities on r.severity equals se.SeverityId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.RequestId == id
                          select new eProjectHDO.Models.RequestUser
                          {
                              requestTitle = r.title,
                              description = r.description,
                              facility = f.FacilityName,
                              type = t.TypeName,
                              severity = se.SeverityName,
                              status = s.StatusName,
                              username = u.fullname,
                              userId = u.UserId,
                              assigned = (int)r.assigned,
                              assignedPerson = db.Users.Where(m => m.UserId == r.assigned).FirstOrDefault().fullname,
                              created_at = r.created_at.ToString()
                          }).FirstOrDefault();
            string title = result.requestTitle;
            string type = result.type;
            string des = HttpUtility.HtmlDecode((result.description));
            string facility = result.facility;
            string severity = result.severity;
            string status = result.status;
            string username = result.username;
            int userId = result.userId;
            int assigned = result.assigned;
            string person = result.assignedPerson;
            string create = result.created_at; // result json to client
            return Json(new { title = title,description=des,facility=facility,severity=severity,status=status,username,person=person,times=create,type=type }, JsonRequestBehavior.AllowGet);
        }
        // reject request
        [Authorize(Roles ="head")]
        public ActionResult RejectedRequest(int id, string remarks)
        {
            var result = db.Requests.Where(m => m.RequestId == id).FirstOrDefault(); // find request
            result.status = 7; // rejected
            // add status of request
            RequestUpdated status = new RequestUpdated();
            status.requestStatus = "Rejected";
            status.RequestId = result.RequestId;
            status.remarks = remarks;
            status.created_at = DateTime.Now;
            db.RequestUpdateds.Add(status);
            // notification
            Notification objTb = new Notification();
            objTb.userId = result.requestor;
            string mesTb = "Your request '" + result.title + "' - Rejected by facility-head !";
            objTb.message = mesTb;
            objTb.requestId = result.RequestId;
            objTb.status = 0;
            objTb.created_at = DateTime.Now;
            db.Notifications.Add(objTb);
            string user1 = getEmail((int)result.requestor); // email requestor
            sendMail.mailNotification("You have a notification from Online Help Desk", mesTb, user1);
            db.SaveChanges();
            return Json(new {success="update success" }, JsonRequestBehavior.AllowGet);
        }
        // assigned person
        [Authorize(Roles ="head")]
        public ActionResult assignedPerson(int id, int assigned)
        {
            string Domain = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
            var result = db.Requests.Where(m => m.RequestId == id).FirstOrDefault();
            result.assigned = assigned; // assigned person
            result.status = 4; // assigned
            // add status of request
            RequestUpdated status = new RequestUpdated();
            status.requestStatus = "Assigned";
            status.RequestId = id;
            status.created_at = DateTime.Now;
            db.RequestUpdateds.Add(status);
            // add notification to requestor
            Notification noti = new Notification();
            noti.userId = result.requestor;
            string msg1 = "Your request '" + result.title+ "' has been assigned !";
            noti.message = msg1;
            noti.status = 0;
            noti.requestId = result.RequestId;
            noti.created_at = DateTime.Now;
            // add notification to assigned person
            Notification noti1 = new Notification();
            noti1.userId = assigned;
            string msg2 = "You receive a request as assigned by the facility's head .";
            noti1.message = msg2;
            noti1.status = 0;
            noti1.created_at = DateTime.Now;
            db.Notifications.Add(noti);
            db.Notifications.Add(noti1);
            // send notification to mail
            string user1 = getEmail((int)result.requestor); // email requestor
            string user2 = getEmail((int)result.assigned); // email assigned-person
            sendMail.mailNotification("You have a notification from System Online Help Desk CUSC", msg1, user1);
            sendMail.mailNotification("You have a notification from facility-head", msg2, user2);
            // end send email
            db.SaveChanges();
            return Json(new { success = "update success" }, JsonRequestBehavior.AllowGet);
        }
        // list user in facility
        [Authorize(Roles ="head,assigned")]
        public ActionResult userInFacility(int? page)
        {
            int facili = getFacilityofUser();
            var result = db.Users.Where(m => m.facility == facili && (m.role == 2 || m.role == 3)).ToList().ToPagedList(page ?? 1, 10);
            return View(result);
        }
        // search user
        [Authorize(Roles = "head,assigned")]
        public ActionResult Search(int? page, string s, int type = 0)
        {
            int facili = getFacilityofUser();
            if(type == 0)
            {
                var result = db.Users.Where(m => m.facility == facili && m.username.Contains(s) && (m.role == 2 || m.role == 3)).ToList().ToPagedList(page ?? 1, 10);
                return View(result);
            }
            else
            {
                var result = db.Users.Where(m => m.facility == facili && m.fullname.Contains(s) && (m.role == 2 || m.role == 3)).ToList().ToPagedList(page ?? 1, 10);
                return View(result);
            }
        }
        // render partial view 
        public ActionResult RenderTool()
        {
            int facili = getFacilityofUser();
            TempData["assigned"] = db.Users.Where(m => m.facility == facili && m.role == 2).ToList(); // role = 2 (Assigned person)
            TempData.Keep("assigned");
            return PartialView("_ToolRequest", null);
        }
        // search request
        [Authorize(Roles ="head")]
        public ActionResult searchRequest(int? page, int query = 0, int type = 0)
        {
            int facili = getFacilityofUser();
            TempData["facility"] = db.Facilities.Where(m => m.FacilityId == facili).FirstOrDefault().FacilityName;
            if(type == 0)
            {
                var result = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join se in db.Severities on r.severity equals se.SeverityId
                          join u in db.Users on r.requestor equals u.UserId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.facility == facili && r.block == 0 && r.requestor == query
                          select new eProjectHDO.Models.RequestUser
                          {
                              requestId = r.RequestId,
                              requestTitle = r.title,
                              facility = f.FacilityName,
                              type = t.TypeName,
                              username = u.fullname,
                              userId = u.UserId,
                              severity = se.SeverityName,
                              assigned = (int)r.assigned,
                              assignedPerson = db.Users.Where(m => m.UserId == r.assigned).FirstOrDefault().fullname,
                              status = s.StatusName,
                              created_at = r.created_at.ToString()
                          }).OrderByDescending(m => m.requestId).ToList().ToPagedList(page ?? 1, 10); // get list of request 
                return View(result);
            }
            else
            {
                var result = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join se in db.Severities on r.severity equals se.SeverityId
                          join u in db.Users on r.requestor equals u.UserId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.facility == facili && r.block == 0 && r.assigned == query
                          select new eProjectHDO.Models.RequestUser
                          {
                              requestId = r.RequestId,
                              requestTitle = r.title,
                              facility = f.FacilityName,
                              type = t.TypeName,
                              username = u.fullname,
                              userId = u.UserId,
                              severity = se.SeverityName,
                              assigned = (int)r.assigned,
                              assignedPerson = db.Users.Where(m => m.UserId == r.assigned).FirstOrDefault().fullname,
                              status = s.StatusName,
                              created_at = r.created_at.ToString()
                          }).OrderByDescending(m => m.requestId).ToList().ToPagedList(page ?? 1, 10); // get list of request 
                return View(result);
            }
        }
        // report
        [Authorize(Roles ="head")]
        public ActionResult Report()
        {
            int facili = getFacilityofUser();
            TempData["facility"] = db.Facilities.Where(m => m.FacilityId == facili).FirstOrDefault().FacilityName;
            TempData["typeOfFacility"] = db.TypeOfFacilities.Where(m => m.Facility == facili && m.Status == 0).ToList();
            TempData["status"] = db.Status.Where(m => m.StatusId > 2).ToList();
            return View();
        }
        // return report via partial view
        [Authorize(Roles ="head")]
        public ActionResult resultReport(DateTime tu, DateTime den, int status = 0, int type = 0)
        {
            System.Threading.Thread.Sleep(1000);
            int facili = getFacilityofUser();
            if (type == 0)
            {
                if(status == 0)
                {
                    var requests = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join se in db.Severities on r.severity equals se.SeverityId
                          join u in db.Users on r.requestor equals u.UserId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.facility == facili && r.block == 0
                          && r.created_at >= tu && r.created_at <= den
                          select new eProjectHDO.Models.RequestUser
                          {
                              requestId = r.RequestId,
                              requestTitle = r.title,
                              facility = f.FacilityName,
                              type = t.TypeName,
                              username = u.fullname,
                              userId = u.UserId,
                              severity = se.SeverityName,
                              assigned = (int)r.assigned,
                              assignedPerson = db.Users.Where(m => m.UserId == r.assigned).FirstOrDefault().fullname,
                              status = s.StatusName,
                              created_at = r.created_at.ToString()
                          }).OrderByDescending(m => m.requestId).ToList(); // get list of request 
                    return PartialView("_ReportRequest", requests);
                }
                else
                {
                    var requests = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join se in db.Severities on r.severity equals se.SeverityId
                          join u in db.Users on r.requestor equals u.UserId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.facility == facili && r.block == 0 && r.status == status
                          && r.created_at >= tu && r.created_at <= den
                          select new eProjectHDO.Models.RequestUser
                          {
                              requestId = r.RequestId,
                              requestTitle = r.title,
                              facility = f.FacilityName,
                              type = t.TypeName,
                              username = u.fullname,
                              userId = u.UserId,
                              severity = se.SeverityName,
                              assigned = (int)r.assigned,
                              assignedPerson = db.Users.Where(m => m.UserId == r.assigned).FirstOrDefault().fullname,
                              status = s.StatusName,
                              created_at = r.created_at.ToString()
                          }).OrderByDescending(m => m.requestId).ToList(); // get list of request 
                    return PartialView("_ReportRequest", requests);
                }
            }
            else
            {
                if(status == 0)
                {
                    var requests = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join se in db.Severities on r.severity equals se.SeverityId
                          join u in db.Users on r.requestor equals u.UserId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.facility == facili && r.block == 0 && r.typefacility == type
                          && r.created_at >= tu && r.created_at <= den
                          select new eProjectHDO.Models.RequestUser
                          {
                              requestId = r.RequestId,
                              requestTitle = r.title,
                              facility = f.FacilityName,
                              type = t.TypeName,
                              username = u.fullname,
                              userId = u.UserId,
                              severity = se.SeverityName,
                              assigned = (int)r.assigned,
                              assignedPerson = db.Users.Where(m => m.UserId == r.assigned).FirstOrDefault().fullname,
                              status = s.StatusName,
                              created_at = r.created_at.ToString()
                          }).OrderByDescending(m => m.requestId).ToList(); // get list of request 
                    return PartialView("_ReportRequest", requests);
                }
                else
                {
                    var requests = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join se in db.Severities on r.severity equals se.SeverityId
                          join u in db.Users on r.requestor equals u.UserId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.facility == facili && r.block == 0 && r.status == status && r.typefacility == type
                          && r.created_at >= tu && r.created_at <= den
                          select new eProjectHDO.Models.RequestUser
                          {
                              requestId = r.RequestId,
                              requestTitle = r.title,
                              facility = f.FacilityName,
                              type = t.TypeName,
                              username = u.fullname,
                              userId = u.UserId,
                              severity = se.SeverityName,
                              assigned = (int)r.assigned,
                              assignedPerson = db.Users.Where(m => m.UserId == r.assigned).FirstOrDefault().fullname,
                              status = s.StatusName,
                              created_at = r.created_at.ToString()
                          }).OrderByDescending(m => m.requestId).ToList(); // get list of request 
                    return PartialView("_ReportRequest", requests);
                }
            }
        }
        // print report
        public ActionResult PrintReport(DateTime tu, DateTime den, int status = 0, int type = 0)
        {
            int facili = getFacilityofUser(); // id facility of user
            if (type == 0) // if type = 0, it means get all types of facility
            {
                if(status == 0) // if status = 0, it means get all requests with any status
                {
                    var requests = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join se in db.Severities on r.severity equals se.SeverityId
                          join u in db.Users on r.requestor equals u.UserId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.facility == facili && r.block == 0
                          && r.created_at >= tu && r.created_at <= den
                          select new eProjectHDO.Models.RequestUser
                          {
                              requestId = r.RequestId,
                              requestTitle = r.title,
                              facility = f.FacilityName,
                              type = t.TypeName,
                              username = u.fullname,
                              userId = u.UserId,
                              severity = se.SeverityName,
                              assigned = (int)r.assigned,
                              assignedPerson = db.Users.Where(m => m.UserId == r.assigned).FirstOrDefault().fullname,
                              status = s.StatusName,
                              created_at = r.created_at.ToString()
                          }).OrderByDescending(m => m.requestId).ToList(); // get list of request 
                    var report = new ViewAsPdf(requests);
                    return report;
                }
                else
                {
                    var requests = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join se in db.Severities on r.severity equals se.SeverityId
                          join u in db.Users on r.requestor equals u.UserId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.facility == facili && r.block == 0 && r.status == status
                          && r.created_at >= tu && r.created_at <= den
                          select new eProjectHDO.Models.RequestUser
                          {
                              requestId = r.RequestId,
                              requestTitle = r.title,
                              facility = f.FacilityName,
                              type = t.TypeName,
                              username = u.fullname,
                              userId = u.UserId,
                              severity = se.SeverityName,
                              assigned = (int)r.assigned,
                              assignedPerson = db.Users.Where(m => m.UserId == r.assigned).FirstOrDefault().fullname,
                              status = s.StatusName,
                              created_at = r.created_at.ToString()
                          }).OrderByDescending(m => m.requestId).ToList(); // get list of request 
                    var report = new ViewAsPdf(requests);
                    return report;
                }
            }
            else
            {
                if(status == 0)
                {
                    var requests = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join se in db.Severities on r.severity equals se.SeverityId
                          join u in db.Users on r.requestor equals u.UserId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.facility == facili && r.block == 0 && r.typefacility == type
                          && r.created_at >= tu && r.created_at <= den
                          select new eProjectHDO.Models.RequestUser
                          {
                              requestId = r.RequestId,
                              requestTitle = r.title,
                              facility = f.FacilityName,
                              type = t.TypeName,
                              username = u.fullname,
                              userId = u.UserId,
                              severity = se.SeverityName,
                              assigned = (int)r.assigned,
                              assignedPerson = db.Users.Where(m => m.UserId == r.assigned).FirstOrDefault().fullname,
                              status = s.StatusName,
                              created_at = r.created_at.ToString()
                          }).OrderByDescending(m => m.requestId).ToList(); // get list of request 
                    var report = new ViewAsPdf(requests);
                    return report;
                }
                else
                {
                    var requests = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join se in db.Severities on r.severity equals se.SeverityId
                          join u in db.Users on r.requestor equals u.UserId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.facility == facili && r.block == 0 && r.status == status && r.typefacility == type
                          && r.created_at >= tu && r.created_at <= den
                          select new eProjectHDO.Models.RequestUser
                          {
                              requestId = r.RequestId,
                              requestTitle = r.title,
                              facility = f.FacilityName,
                              type = t.TypeName,
                              username = u.fullname,
                              userId = u.UserId,
                              severity = se.SeverityName,
                              assigned = (int)r.assigned,
                              assignedPerson = db.Users.Where(m => m.UserId == r.assigned).FirstOrDefault().fullname,
                              status = s.StatusName,
                              created_at = r.created_at.ToString()
                          }).OrderByDescending(m => m.requestId).ToList(); // get list of request 
                    var report = new ViewAsPdf(requests);
                    return report;
                }
            }
        }
        // get email of user id
        public string getEmail(int id)
        {
            return db.Users.Where(m => m.UserId == id).FirstOrDefault().email;
        }
    }
}