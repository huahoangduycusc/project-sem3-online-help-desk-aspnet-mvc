using eProjectHDO.Models;
using PagedList;
using Rotativa;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace eProjectHDO.Controllers
{
    public class ArticleController : Controller
    {
        ohdEntities db = new ohdEntities();
        public ActionResult View(int id)
        {
            var result = (from a in db.Articles
                          join u in db.Users on a.UserId equals u.UserId
                          where a.ArticleId == id
                          select new eProjectHDO.Models.Baiviet
                          {
                              id = a.ArticleId,
                              title = a.title,
                              description = a.description,
                              thumbnail = a.thumbnail,
                              author = u.username,
                              userId = u.UserId,
                              created_at = a.created_at.ToString(),
                              view = (int)a.view
                          }).FirstOrDefault();
            if (result == null)
            {
                return RedirectToAction("Index", "Home", "");
            }
            ViewData["demCmt"] = db.comments.Where(m => m.article_id == id).Count();
            var addView = db.Articles.Where(m => m.ArticleId == id).FirstOrDefault();
            addView.view += 1;
            db.SaveChanges();
            return View(result);
        }
        // comment
        [Authorize]
        [HttpPost]
        public ActionResult View(int id, comment c)
        {
            int user_id = (int)Session["uid"]; // session
            var selfBv = db.Articles.Where(m => m.ArticleId == id).FirstOrDefault();
            try
            {
                if (ModelState.IsValid)
                {
                    var userSelf = db.Users.Where(m => m.UserId == user_id).FirstOrDefault();
                    comment obj = new comment();
                    obj.message = c.message;
                    obj.created_at = DateTime.Now;
                    obj.article_id = id;
                    obj.userId = user_id;
                    db.comments.Add(obj);
                    db.SaveChanges();
                    return RedirectToAction("View", "Article", new { id = id });
                }
                else
                {
                    ViewBag.Msg = "There is something wrong";
                    return RedirectToAction("View", "Article", new { id = id});
                }
            }
            catch (Exception ex)
            {
                ViewBag.Msg = ex.Message;
            }
            return RedirectToAction("View", "Article", new { id = id});
        }
        // list
        [Authorize(Roles = "admin")]
        public ActionResult List(int? page)
        {
            var result = (from a in db.Articles
                          join u in db.Users on a.UserId equals u.UserId
                          select new eProjectHDO.Models.Baiviet
                          {
                              id = a.ArticleId,
                              title = a.title,
                              thumbnail = a.thumbnail,
                              author = u.username,
                              userId = u.UserId,
                              created_at = a.created_at.ToString(),
                              view = (int)a.view
                          }).ToList().OrderByDescending(m => m.id).ToPagedList(page ?? 1, 10);
            return View(result);
        }
        // display article index
        public ActionResult displayArticle(int id) // category id
        {
            var result = db.Articles.Where(m => m.categoryId == id).OrderByDescending(m => m.ArticleId).Take(4).ToList();
            return PartialView("_DisplayArticle",result);
        }
        // display new article index
        public ActionResult displayNewsArticle() // category id
        {
            var result = db.Articles.Where(m => m.categoryId != 20).OrderByDescending(m => m.ArticleId).Take(4).ToList();
            return PartialView("_DisplayArticle", result);
        }
        // create article
		[Authorize(Roles ="admin")]
        public ActionResult Create()
        {
            TempData["category"] = db.Categories.ToList();
            TempData.Keep("category");
            return View();
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult Create(Article a, HttpPostedFileBase thumbnail)
        {
            TempData["category"] = db.Categories.ToList();
            TempData.Keep("category");
            string filename = "";
            string extension = "";
            int uid = (int)Session["uid"];
            string name = "";
            try
            {
                if (ModelState.IsValid)
                {
                    if (thumbnail != null && thumbnail.ContentLength > 0)
                    {
                        filename = Path.GetFileName(thumbnail.FileName);
                        extension = Path.GetExtension(thumbnail.FileName).ToLower();
                        name = DateTime.Now.ToFileTime().ToString();
                        name += extension;
                        var allowExtension = new[] { ".png", ".jpg", ".jpeg" };
                        if (allowExtension.Contains(extension))
                        {
                            string foldername = Path.Combine(Server.MapPath("~/images/thumbnail"), name);
                            thumbnail.SaveAs(foldername);
                            Article obj = new Article();
                            obj.title = a.title;
                            obj.description = a.description;
                            obj.categoryId = a.categoryId;
                            obj.view = 0;
                            obj.UserId = (int)Session["uid"];
                            obj.thumbnail = name;
                            obj.created_at = DateTime.Now;
                            db.Articles.Add(obj);
                            db.SaveChanges();
                            int a_id = (int)obj.ArticleId;
                            ViewBag.Success = "New topic created successfully, you can see the article at !<a href=\"" + Url.Action("View", "Article", new { id = a_id }) + "\">"+a.title+" !</a>";
                        }
                        else
                        {
                            ViewBag.Msg = "The file format is invalid, the image extension must be .PNG, .JPEG and .JPG !";
                        }
                    }
                }
                else
                {
                    ViewBag.Msg = "There is something wrong";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Msg = ex.Message;
            }
            return View();
        }
        // edit article
        [Authorize(Roles = "admin")]
        public ActionResult editArticle(int id)
        {
            TempData["category"] = db.Categories.ToList();
            TempData.Keep("category");
            var result = db.Articles.Where(m => m.ArticleId == id).FirstOrDefault();
            return View(result);
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult editArticle(int id, Article a, HttpPostedFileBase thumbnail)
        {
            TempData["category"] = db.Categories.ToList();
            TempData.Keep("category");
            var result = db.Articles.Where(m => m.ArticleId == id).FirstOrDefault();
            result.title = a.title;
            result.description = a.description;
            result.categoryId = a.categoryId;
            if (thumbnail != null && thumbnail.ContentLength > 0)
            {
                var allowExtension = new[] { ".png", ".jpg", ".jpeg" };
                string extension = Path.GetExtension(thumbnail.FileName).ToLower();
                string name = DateTime.Now.ToFileTime().ToString();
                name += extension;
                if (!allowExtension.Contains(extension))
                {
                    ViewBag.Msg = "The file format is invalid, the image extension must be .PNG, .JPEG and .JPG !";
                }
                else
                {
                    string foldername = Path.Combine(Server.MapPath("~/images/thumbnail"), name);
                    thumbnail.SaveAs(foldername);
                    result.thumbnail = name;
                }
            }
            ViewBag.Success = "Update success !";
            db.SaveChanges();
            return View(result);
        }
        // delete article
        [Authorize(Roles = "admin")]
        public ActionResult deleteArticle(int id)
        {
            var result = db.Articles.Where(m => m.ArticleId == id).FirstOrDefault();
            db.Articles.Remove(result);
            db.SaveChanges();
            return RedirectToAction("List");
        }
        // view comment
        public ActionResult getComment(int id, int skip = 0)
        {
            System.Threading.Thread.Sleep(1000);
            var result = (from c in db.comments
                          join u in db.Users on c.userId equals u.UserId
                          where c.article_id == id
                          orderby c.cmt_id descending
                          select new eProjectHDO.Models.binhluan
                          {
                              id = c.cmt_id,
                              uid = u.UserId,
                              name = u.username,
                              img = u.avatar,
                              day = c.created_at.ToString(),
                              msg = c.message
                          }).Skip(skip).Take(5).ToList();
            return PartialView("_Comment", result);
        }
        // del comment
        [Authorize]
        public ActionResult DelComment(int id)
        {
            var result = db.comments.Where(m => m.cmt_id == id).FirstOrDefault();
            int art = (int)result.article_id;
            if (result == null)
            {
                return RedirectToAction("Index", "Home", "");
            }
            else
            {
                if (User.IsInRole("admin") || Session["uid"] != null && Session["uid"].ToString() == result.userId.ToString())
                {
                    db.comments.Remove(result);
                    db.SaveChanges();
                    return RedirectToAction("View", "Article", new { id = art });
                }
                else
                {
                    return RedirectToAction("Login", "Home", "");
                }
            }
        }
        // xoa comment
        [Authorize(Roles ="admin")]
        public ActionResult xoaComment(int id)
        {
            var result = db.comments.Where(m => m.cmt_id == id).FirstOrDefault();
            if (result == null)
            {
                return RedirectToAction("Index", "Home", "");
            }
            else
            {
                db.comments.Remove(result);
                db.SaveChanges();
            }
            return RedirectToAction("Comment", "Article", "");
        }
        // comment list
        [Authorize(Roles ="admin")]
        public ActionResult Comment(int? page)
        {
            var result = (from c in db.comments
                          join a in db.Articles
                          on c.article_id equals a.ArticleId
                          join u in db.Users
                          on c.userId equals u.UserId
                          orderby c.cmt_id descending
                          select new eProjectHDO.Models.baocaoCmt
                          {
                              cid = c.cmt_id,
                              user_id = u.UserId,
                              aid = a.ArticleId,
                              title = a.title,
                              nickname = u.username,
                              msg = c.message,
                              created_at = c.created_at.ToString()
                          }).ToList().ToPagedList(page ?? 1, 10);
            return View(result);
        }
        // report
        [Authorize(Roles ="admin")]
        public ActionResult Report()
        {
            var result = (from a in db.Articles
                          join u in db.Users on a.UserId equals u.UserId
                          select new eProjectHDO.Models.Baiviet
                          {
                              id = a.ArticleId,
                              title = a.title,
                              thumbnail = a.thumbnail,
                              author = u.username,
                              userId = u.UserId,
                              created_at = a.created_at.ToString(),
                              view = (int)a.view
                          }).ToList().OrderByDescending(m => m.id);
            var report = new ViewAsPdf(result);
            return report;
        }
    }
}