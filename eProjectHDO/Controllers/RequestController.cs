using eProjectHDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eProjectHDO.Controllers
{
    public class RequestController : Controller
    {
        ohdEntities db = new ohdEntities();
        // create request
        [Authorize]
        public ActionResult Create()
        {
            try
            {
                TempData["facility"] = db.Facilities.Where(m => m.status == 0).ToList();
                TempData["severity"] = db.Severities.ToList();
                // get first facility to show on index
                var first = db.Facilities.Where(m => m.status == 0).FirstOrDefault(); // facility is active
                if (first != null)
                {
                    TempData["typeOfFacility"] = db.TypeOfFacilities.Where(m => m.Facility == first.FacilityId && m.Status == 0).ToList();
                }
            }
            catch(Exception ex)
            {
                ViewBag.msg = ex.Message;
            }
            return View();
        }
        [Authorize]
        [HttpPost]
        public ActionResult Create(Request r)
        {
            int userId = (int)Session["uid"];
            TempData["facility"] = db.Facilities.Where(m => m.status == 0).ToList();
            TempData["severity"] = db.Severities.ToList();
            TempData.Keep("facility");
            TempData.Keep("severity");
            // get first facility to show on index
            var first = db.Facilities.FirstOrDefault();
            if (first != null)
            {
                TempData["typeOfFacility"] = db.TypeOfFacilities.Where(m => m.Facility == first.FacilityId && m.Status == 0).ToList();
            }
            TempData.Keep("typeOfFacility");
            try
            {
                Request obj = new Request();
                obj.title = r.title;
                obj.description = HttpUtility.HtmlEncode(r.description.Replace(Environment.NewLine,"<br/>"));
                obj.severity = r.severity;
                obj.facility = r.facility;
                obj.typefacility = r.typefacility;
                obj.requestor = userId;
                obj.assigned = 0;
                obj.status = 3; // unassigned
                obj.block = 0;
                obj.created_at = DateTime.Now;
                db.Requests.Add(obj);
                // add status of request
                RequestUpdated status = new RequestUpdated();
                status.requestStatus = "Pending";
                status.remarks = "Unassigned";
                status.RequestId = obj.RequestId;
                status.created_at = DateTime.Now;
                db.RequestUpdateds.Add(status);
                // send noti to email
                var faci = db.Users.Where(m => m.facility == r.facility && m.role == 3).ToList();
                foreach(var item in faci)
                {
                    // add notification to requestor
                    Notification noti = new Notification();
                    noti.userId = item.UserId;
                    string msg1 = "<b>New request : </b>" + getName(userId) + " just sent the request to the facility.</a>";
                    noti.message = msg1;
                    noti.status = 0;
                    noti.requestId = obj.RequestId;
                    noti.created_at = DateTime.Now;
                    db.Notifications.Add(noti);
                    sendMail.mailNotification("You have a notification from CUSC Online Help Desk System", msg1, getEmail(item.UserId));
                }
                db.SaveChanges();
                ViewBag.success = "Create new request success ! <br/>A notification email was sent to the facility head";
            }
            catch(Exception ex)
            {
                ViewBag.msg = ex.Message;
            }
            return View();
        }
        // view request
        [Authorize]
        public ActionResult View(int id)
        {
            int UserId = (int)Session["uid"];
            var result = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join u in db.Users on r.requestor equals u.UserId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.RequestId == id && r.requestor == UserId
                          select new eProjectHDO.Models.RequestUser
                          {
                              requestId = r.RequestId,
                              requestTitle = r.title,
                              description = r.description,
                              facility = f.FacilityName,
                              type = t.TypeName,
                              status = s.StatusName,
                              username = u.username,
                              userId = u.UserId,
                              created_at = r.created_at.ToString()
                          }).FirstOrDefault();
            return View(result);
        }
        // open request
        [Authorize]
        public ActionResult Open(int id)
        {
            int userId = (int)Session["uid"];
            var result = db.Requests.Where(m => m.RequestId == id).FirstOrDefault();
            if(result.requestor != userId)
            {
                return Redirect("/");
            }
            else
            {
                result.block = 0; // open
                db.SaveChanges();
            }
            return RedirectToAction("Requests", "User", null);
        }
        // close request
        [Authorize]
        public ActionResult Close(int id)
        {
            int userId = (int)Session["uid"];
            var result = db.Requests.Where(m => m.RequestId == id).FirstOrDefault();
            if (result.requestor != userId)
            {
                return Redirect("/");
            }
            else
            {
                if(result.status == 3) // unassigned, chua chi dinh
                {
                    result.block = 1; // close
                    db.SaveChanges();
                }
                else
                {
                    return RedirectToAction("Requests", "User", new {close="error" });
                }
            }
            return RedirectToAction("Requests", "User", null);
        }
        // request status partial view
        public ActionResult Status(int id)
        {
            var result = db.RequestUpdateds.Where(m => m.RequestId == id).ToList();
            return PartialView("_StatusRequest", result);
        }
        // get type of facility ajax
        public ActionResult getTypeofFacility(int id)
        {
            db.Configuration.ProxyCreationEnabled = false; // end loop
            var result = db.TypeOfFacilities.Where(m => m.Facility == id && m.Status == 0);
            return Json(result.ToList(),JsonRequestBehavior.AllowGet);
        }
        // get name of user
        public string getName(int id)
        {
            return db.Users.Where(m => m.UserId == id).FirstOrDefault().username;
        }
        // get email of user id
        public string getEmail(int id)
        {
            return db.Users.Where(m => m.UserId == id).FirstOrDefault().email;
        }
        // view request
        [Authorize(Roles = "admin,head,assigned")]
        public ActionResult See(int id)
        {
            try
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
                              requestId = r.RequestId,
                              requestTitle = r.title,
                              description = r.description,
                              facility = f.FacilityName,
                              type = t.TypeName,
                              severity = se.SeverityName,
                              status = s.StatusName,
                              username = u.username,
                              userId = u.UserId,
                              assigned = (int)r.assigned,
                              assignedPerson = db.Users.Where(m => m.UserId == r.assigned).FirstOrDefault().username,
                              created_at = r.created_at.ToString()
                          }).FirstOrDefault();
                return View(result);
            }
            catch(Exception ex)
            {
                ViewBag.msg = ex.Message;
                return View();
            }
        }
    }
}