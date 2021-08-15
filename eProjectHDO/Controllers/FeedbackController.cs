using eProjectHDO.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eProjectHDO.Controllers
{
    public class FeedbackController : Controller
    {
        ohdEntities db = new ohdEntities();
        // GET: Feedback
        public ActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Index(Feedback obj)
        {
            string msg = "";
            if (ModelState.IsValid)
            {
                Feedback fb = new Feedback();
                fb.email = obj.email;
                fb.title = obj.title;
                fb.description = obj.description;
                fb.created_at = DateTime.Now;
                fb.status = 0;
                db.Feedbacks.Add(fb);
                db.SaveChanges();
                msg = "Gửi feedback thành công, cảm ơn phản hồi của bạn !";
            }
            else
            {
                msg = "Có lỗi xảy ra trong quá trình gửi Feedback.";
            }
            return Json(new { msg = msg }, JsonRequestBehavior.AllowGet);
        }
        // list feedback
        [Authorize(Roles = "admin,btv")]
        public ActionResult List(int? page, int type = 0)
        {
            if(type == 0)
            {
                var result = db.Feedbacks.OrderByDescending(m => m.fbId).ToList().ToPagedList(page ?? 1, 10);
                return View(result);
            }
            else if (type == 1)
            {
                var result = db.Feedbacks.Where(m => m.status == 0).OrderByDescending(m => m.fbId).ToList().ToPagedList(page ?? 1, 10);
                return View(result);
            }
            else
            {
                var result = db.Feedbacks.Where(m => m.status == 1).OrderByDescending(m => m.fbId).ToList().ToPagedList(page ?? 1, 10);
                return View(result);
            }
        }
        // delete
        [Authorize(Roles = "admin,btv")]
        public ActionResult Delete(int id)
        {
            var result = db.Feedbacks.Where(m => m.fbId == id).FirstOrDefault();
            db.Feedbacks.Remove(result);
            db.SaveChanges();
            return RedirectToAction("List");
        }
        // view feedback
        [Authorize(Roles = "admin,btv")]
        public ActionResult View(int id)
        {
            var result = db.Feedbacks.Where(m => m.fbId == id).FirstOrDefault();
            string fb_title = result.title;
            string fb_email = result.email;
            string fb_msg = result.description;
            string fb_time = result.created_at.ToString();
            result.status = 1;
            db.SaveChanges();
            return Json(new { title = fb_title, email = fb_email, msg = fb_msg, dates = fb_time }, JsonRequestBehavior.AllowGet);
        }
    }
}