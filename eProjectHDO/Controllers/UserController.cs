using eProjectHDO.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eProjectHDO.Controllers
{
    public class UserController : Controller
    {
        ohdEntities db = new ohdEntities();
        // GET: User
        [Authorize]
        public ActionResult Index(int id)
        {
            var result = db.Users.Where(m => m.UserId == id).FirstOrDefault();
            return View(result);
        }
        [Authorize]
        // change password
        public ActionResult changePassword()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public ActionResult changePassword(string opw, string npw, string cfpw)
        {
            int id = (int)Session["uid"];
            bool flag = true;
            if (npw != cfpw)
            {
                ViewBag.Msg = "new password and confirm password are not the same !";
                flag = false;
            }
            var result = db.Users.Where(m => m.UserId == id).FirstOrDefault();
            string ePw = encryptPassword.encrypt(opw);
            if (result.password != ePw)
            {
                ViewBag.Msg = "old password is not correct, please try again !";
                flag = false;
            }
            if (flag)
            {
                string newPass = encryptPassword.encrypt(npw);
                result.password = newPass;
                ViewBag.Success = "Change password success !";
                db.SaveChanges();
            }
            return View();
        }
        [Authorize]
        // setting
        public ActionResult Setting(int id)
        {
            var result = db.Users.Where(m => m.UserId == id).FirstOrDefault();
            if (result != null)
            {
                if (User.IsInRole("admin") || Session["uid"] != null && (int)Session["uid"] == result.UserId)
                {
                    return View(result);
                }
                else
                {
                    return RedirectToAction("Index", "Home", "");
                }
            }
            else
            {
                return RedirectToAction("Index", "Home", "");
            }
        }
        [Authorize]
        [HttpPost]
        public ActionResult Setting(int id, User u, HttpPostedFileBase avatar)
        {
            var result = db.Users.Where(m => m.UserId == id).FirstOrDefault();
            var allowExtension = new[] { ".png", ".jpg", ".jpeg" };
            try
            {
                result.fullname = u.fullname;
                result.gender = u.gender;
                result.birthday = u.birthday;
                result.address = u.address;
                result.phone = u.phone;
                result.favorite = u.favorite;
                result.email = u.email;
                if (avatar != null && avatar.ContentLength > 0)
                {
                    string extension;
                    string name;
                    extension = Path.GetExtension(avatar.FileName).ToLower();
                    if (!allowExtension.Contains(extension))
                    {
                        TempData["msg"] = "The file format is invalid, the image extension must be .PNG, .JPEG and .JPG !";
                    }
                    else
                    {
                        name = result.UserId.ToString();
                        name += extension;
                        Session["avatar"] = name;
                        string foldername = Path.Combine(Server.MapPath("~/images/profile"), name);
                        avatar.SaveAs(foldername);
                        result.avatar = name;
                    }
                }
                db.SaveChanges();
                ViewBag.Msg = "Update information success";
            }
            catch (Exception ex)
            {
                ViewBag.Msg = ex.Message;
            }
            return View(result);
        }
        // roles
        [Authorize(Roles ="admin")]
        public ActionResult Roles(int id)
        {
            var result = db.Users.Where(m => m.UserId == id).FirstOrDefault();
            TempData["facility"] = db.Facilities.ToList();
            TempData["role"] = db.Roles.ToList();
            return View(result);
        }
        [Authorize(Roles ="admin")]
        [HttpPost]
        public ActionResult Roles(int id, User u)
        {
            TempData["facility"] = db.Facilities.ToList();
            TempData["role"] = db.Roles.ToList();
            var result = db.Users.Where(m => m.UserId == id).FirstOrDefault();
            TempData.Keep("facility");
            TempData.Keep("role");
            result.facility = u.facility;
            result.role = u.role;
            db.SaveChanges();
            ViewBag.Msg = "Update success !";
            return View(result);
        }
        // request of user
        [Authorize]
        public ActionResult Requests(int? page, int type = 0)
        {
            int userId = (int)Session["uid"];
            if(type == 0)
            {
                var result = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.requestor == userId
                          select new eProjectHDO.Models.RequestUser
                          {
                              requestId = r.RequestId,
                              requestTitle = r.title,
                              facility = f.FacilityName,
                              block = (int)r.block,
                              type = t.TypeName,
                              status = s.StatusName,
                              created_at = r.created_at.ToString()
                          }).OrderByDescending(m => m.requestId).ToList().ToPagedList(page ?? 1, 10);
            return View(result);
            }
            else if(type == 1)
            {
                var result = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.requestor == userId && r.block == 0
                          select new eProjectHDO.Models.RequestUser
                          {
                              requestId = r.RequestId,
                              requestTitle = r.title,
                              facility = f.FacilityName,
                              type = t.TypeName,
                              block = (int)r.block,
                              status = s.StatusName,
                              created_at = r.created_at.ToString()
                          }).OrderByDescending(m => m.requestId).ToList().ToPagedList(page ?? 1, 10);
            return View(result);
            }
            else if(type == 2)
            {
                var result = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.requestor == userId && r.block == 1
                          select new eProjectHDO.Models.RequestUser
                          {
                              requestId = r.RequestId,
                              requestTitle = r.title,
                              facility = f.FacilityName,
                              type = t.TypeName,
                              block = (int)r.block,
                              status = s.StatusName,
                              created_at = r.created_at.ToString()
                          }).OrderByDescending(m => m.requestId).ToList().ToPagedList(page ?? 1, 10);
            return View(result);
            }
            else if(type == 5)
            {
                var result = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.requestor == userId && r.status == 5
                          select new eProjectHDO.Models.RequestUser
                          {
                              requestId = r.RequestId,
                              requestTitle = r.title,
                              facility = f.FacilityName,
                              type = t.TypeName,
                              block = (int)r.block,
                              status = s.StatusName,
                              created_at = r.created_at.ToString()
                          }).OrderByDescending(m => m.requestId).ToList().ToPagedList(page ?? 1, 10);
            return View(result);
            }
            else if(type == 7)
            {
                var result = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.requestor == userId && r.status == 7
                          select new eProjectHDO.Models.RequestUser
                          {
                              requestId = r.RequestId,
                              requestTitle = r.title,
                              facility = f.FacilityName,
                              type = t.TypeName,
                              block = (int)r.block,
                              status = s.StatusName,
                              created_at = r.created_at.ToString()
                          }).OrderByDescending(m => m.requestId).ToList().ToPagedList(page ?? 1, 10);
            return View(result);
            }
            else if(type == 8)
            {
                var result = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.requestor == userId && r.status == 8
                          select new eProjectHDO.Models.RequestUser
                          {
                              requestId = r.RequestId,
                              requestTitle = r.title,
                              facility = f.FacilityName,
                              type = t.TypeName,
                              block = (int)r.block,
                              status = s.StatusName,
                              created_at = r.created_at.ToString()
                          }).OrderByDescending(m => m.requestId).ToList().ToPagedList(page ?? 1, 10);
            return View(result);
            }
            else
            {
                var result = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.requestor == userId
                          select new eProjectHDO.Models.RequestUser
                          {
                              requestId = r.RequestId,
                              requestTitle = r.title,
                              facility = f.FacilityName,
                              type = t.TypeName,
                              block = (int)r.block,
                              status = s.StatusName,
                              created_at = r.created_at.ToString()
                          }).OrderByDescending(m => m.requestId).ToList().ToPagedList(page ?? 1, 10);
            return View(result);
            }
        }
        // notification , thong bao
        [Authorize]
        public ActionResult Notification(int? page)
        {
            int userId = (int)Session["uid"];
            var result = db.Notifications.Where(m => m.userId == userId).OrderByDescending(m => m.NotiId).ToList().ToPagedList(page ?? 1, 10);
            db.Notifications.Where(m => m.userId == userId).ToList().ForEach(m => m.status = 1);
            db.SaveChanges();
            return View(result);
        }
        // count notification not seen
        public ActionResult countNoti()
        {
            int user_id = (int)Session["uid"];
            var result = db.Notifications.Where(m => m.userId == user_id && m.status == 0).ToList();
            return PartialView("_newNotification", result);
        }
        // ban user
        [Authorize(Roles ="admin,head")]
        public ActionResult Block(int id =-1)
        {
            var result = db.Users.Where(m => m.UserId == id).FirstOrDefault();
            if(result == null)
            {
                return Redirect("/");
            }
            else
            {
                if(result.status == 0)
                {
                    result.status = 1;
                }
                else
                {
                    result.status = 0;
                }
                db.SaveChanges();
                return RedirectToAction("Index",new {id=id });
            }
        }
    }
}