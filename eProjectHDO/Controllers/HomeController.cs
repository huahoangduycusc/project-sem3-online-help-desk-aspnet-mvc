using eProjectHDO.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace eProjectHDO.Controllers
{
    public class HomeController : Controller
    {
        ohdEntities db = new ohdEntities();
        public ActionResult Index()
        {
            return View();
        }

        // login
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(User u)
        {
            string pass = "";
            pass += u.password;
            if (pass != "")
            {
                pass = encryptPassword.encrypt(pass);
            }
            var result = db.Users.FirstOrDefault(m => m.username == u.username && m.password == pass);
            if (result == null)
            {
                ViewBag.Msg = "These credentials do not match our records.";
            }
            else
            {
                if(result.status == 1)
                {
                    ViewBag.Msg = "Oops: Your account is disabled, you cannot login at this time!";
                }
                else
                {
                    FormsAuthentication.SetAuthCookie(u.username, false);
                    User obj = db.Users.FirstOrDefault(m => m.username == u.username);
                    Session["username"] = u.username;
                    Session["uid"] = obj.UserId;
                    Session["avatar"] = obj.avatar;
                    return RedirectToAction("Index");
                }
            }
            return View();
        }
        // register
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(User u)
        {
            var result = db.Users.SingleOrDefault(m => m.username == u.username);
            try
            {
                if (ModelState.IsValid)
                {
                    if (result != null)
                    {
                        ViewBag.Msg = "The username has already exists!, Please use different username";
                    }
                    else
                    {
                        User obj = new User();
                        obj.username = u.username;
                        obj.password = encryptPassword.encrypt(u.password);
                        obj.fullname = u.fullname;
                        obj.gender = u.gender;
                        obj.email = u.email;
                        obj.role = 1;
                        obj.timeJoin = DateTime.Now;
                        obj.avatar = "default.jpg";
                        obj.status = 0;
                        db.Users.Add(obj);
                        db.SaveChanges();
                        FormsAuthentication.SetAuthCookie(u.username, false);
                        Session["username"] = u.username;
                        Session["uid"] = obj.UserId;
                        Session["avatar"] = "default.jpg";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    ViewBag.Msg = "There something went wrong, try again !";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Msg = ex.Message;
            }
            return View();
        }
        // Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Remove("username");
            Session.Remove("uid");
            Session.Remove("avatar");
            return Redirect("/");
        }
        // help page format question and answer
        public ActionResult Help(int? page)
        {
            var result = db.Questionnaires.ToList().ToPagedList(page ?? 1, 10);
            return View(result);
        }
        // category display
        public ActionResult displayCategory()
        {
            var result = (from c in db.Categories
                          select new eProjectHDO.Models.countCategory
                          {
                              id = c.CategoryId,
                              title = c.categoryName,
                              article = db.Articles.Where(m => m.categoryId == c.CategoryId).Count()
                          }).ToList();
            return PartialView("_Category",result);
        }
        //  category
        public ActionResult Category(int id, int ? page)
        {
            ViewBag.Category = db.Categories.Where(m => m.CategoryId == id).FirstOrDefault().categoryName;
            var result = db.Articles.Where(m => m.categoryId == id).ToList().ToPagedList(page ?? 1, 10);
            return View(result);
        }
        // contact
        public ActionResult About()
        {
            return View();
        }
        // render category
        public ActionResult sideCategory()
        {
            try
            {
                var result = db.Categories.ToList();
                return PartialView("_sideCategory", result);
            }
            catch (Exception ex)
            {
                var error = ex.Message.ToString();
                return Content("Error");
            }
        }
    }
}