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
    public class AssignedController : Controller
    {
        ohdEntities db = new ohdEntities();
        // GET: Assigned
        [Authorize(Roles ="assigned")]
        public ActionResult Index(int? page, int type = 0)
        {
            int facili = getFacilityofUser(); // get facility
            int userId = (int)Session["uid"]; // get user id
            TempData["toDo"] = db.Requests.Where(m => m.facility == facili && m.assigned == userId).Count();
            TempData["inProcess"] = db.Requests.Where(m => m.facility == facili && m.status == 5 && m.assigned == userId).Count();
            TempData["completed"] = db.Requests.Where(m => m.facility == facili && m.status == 8 && m.assigned == userId).Count();
            TempData["issues"] = db.Requests.Where(m => m.facility == facili && m.status == 7 || m.status == 6 && m.assigned == userId).Count();
            TempData["facility"] = db.Facilities.Where(m => m.FacilityId == facili).FirstOrDefault().FacilityName;
            if(type == 0)
            {
                var requests = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join se in db.Severities on r.severity equals se.SeverityId
                          join u in db.Users on r.requestor equals u.UserId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.facility == facili && r.assigned == userId && r.block != 1
                          select new eProjectHDO.Models.RequestUser
                          {
                              requestId = r.RequestId,
                              requestTitle = r.title,
                              facility = f.FacilityName,
                              type = t.TypeName,
                              username = u.fullname,
                              userId = u.UserId,
                              severity = se.SeverityName,
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
                          where r.facility == facili && r.assigned == userId && r.block != 1 && r.status == type
                          select new eProjectHDO.Models.RequestUser
                          {
                              requestId = r.RequestId,
                              requestTitle = r.title,
                              facility = f.FacilityName,
                              type = t.TypeName,
                              username = u.fullname,
                              userId = u.UserId,
                              severity = se.SeverityName,
                              status = s.StatusName,
                              created_at = r.created_at.ToString()
                          }).OrderByDescending(m => m.requestId).ToList().ToPagedList(page ?? 1, 10); // get list of request 
                return View(requests);
            }
        }
        // update status of request by assigned person
        [HttpPost]
        public ActionResult updateStatus(int id,int status,string remarks)
        {
            string Domain = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
            int facility = getFacilityofUser();
            int userId = (int)Session["uid"];
            // check request assigned
            var result = db.Requests.Where(m => m.RequestId == id && m.facility == facility && m.assigned == userId).FirstOrDefault();
            result.status = status;
            // add status of request
            var eStatus = db.Status.Where(m => m.StatusId == status).FirstOrDefault();
            RequestUpdated objStatus = new RequestUpdated();
            objStatus.requestStatus = eStatus.StatusName; // get name of status
            objStatus.RequestId = result.RequestId;
            if (remarks != "")
            {
                objStatus.remarks = remarks;
            }
            objStatus.created_at = DateTime.Now;
            db.RequestUpdateds.Add(objStatus);
            // add notification to requestor
            Notification noti = new Notification();
            noti.userId = result.requestor;
            string msg = "Your request '" + result.title + "' - " + eStatus.StatusName + " !";
            noti.message = msg;
            noti.status = 0;
            noti.created_at = DateTime.Now;
            db.Notifications.Add(noti);
            var thongbao = db.Users.Where(m => m.facility == facility && m.role == 3).ToList(); // facility heads role = 3
            string mesTb = getNickname(userId) + " updated the status of the '<a href=\"" +Url.Action("See","Request",new {id=id})+"\"><b>" + result.title + "</b></a>' request !";
            foreach (var item in thongbao) // gui thong bao den facility heads
            {
                Notification objTb = new Notification();
                objTb.userId = item.UserId;
                objTb.message = mesTb;
                objTb.requestId = result.RequestId;
                objTb.status = 0;
                objTb.created_at = DateTime.Now;
                db.Notifications.Add(objTb);
                db.SaveChanges();
            }
            // send mail
            sendMail.mailNotification("You have a notification from Online Help Desk",msg,getEmail((int)result.requestor));
            db.SaveChanges();
            return Json(new { success = "update success" }, JsonRequestBehavior.AllowGet);
        }
        // facility of user
        public int getFacilityofUser()
        {
            int userId = (int)Session["uid"];
            var result = db.Users.Where(m => m.UserId == userId).FirstOrDefault(); // get facility of user
            return (int)result.facility;
        }
        // get username of user
        public string getNickname(int id)
        {
            var result = db.Users.Where(m => m.UserId == id).FirstOrDefault();
            return result.username;
        }
        // get email of user id
        public string getEmail(int id)
        {
            return db.Users.Where(m => m.UserId == id).FirstOrDefault().email;
        }
        // report
        [Authorize(Roles = "assigned")]
        public ActionResult Report()
        {
            int facili = getFacilityofUser();
            TempData["facility"] = db.Facilities.Where(m => m.FacilityId == facili).FirstOrDefault().FacilityName;
            TempData["typeOfFacility"] = db.TypeOfFacilities.Where(m => m.Facility == facili && m.Status == 0).ToList();
            TempData["status"] = db.Status.Where(m => m.StatusId > 3).ToList();
            return View();
        }
        // result report
        [Authorize(Roles ="assigned")]
        public ActionResult resultReport(DateTime tu, DateTime den, int status = 0, int type = 0)
        {
            System.Threading.Thread.Sleep(1000);
            int facili = getFacilityofUser();
            int userId = (int)Session["uid"];
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
                          where r.facility == facili && r.block == 0 && r.assigned == userId
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
                          where r.facility == facili && r.block == 0 && r.status == status && r.assigned == userId
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
                          where r.facility == facili && r.block == 0 && r.typefacility == type && r.assigned == userId
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
                          where r.facility == facili && r.block == 0 && r.status == status && r.typefacility == type && r.assigned == userId
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
            int facili = getFacilityofUser();
            int userId = (int)Session["uid"];
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
                          where r.facility == facili && r.block == 0 && r.assigned == userId
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
                          where r.facility == facili && r.block == 0 && r.status == status && r.assigned == userId
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
                          where r.facility == facili && r.block == 0 && r.typefacility == type && r.assigned == userId
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
                          where r.facility == facili && r.block == 0 && r.status == status && r.typefacility == type && r.assigned == userId
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
    }
}