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
    [Authorize(Roles ="admin")]
    public class AdminController : Controller
    {
        ohdEntities db = new ohdEntities();

        // GET: Admin
        [Obsolete]
        public ActionResult Index()
        {
            var cUser = db.Users.Count();
            var cRequest = db.Requests.Count();
            var cFacility = db.Facilities.Count();
            var cCategory = db.Categories.Count();
            var cArticle = db.Articles.Count();
            var cQuestion = db.Questionnaires.Count();
            var cCmt = db.comments.Count();
            DateTime times = DateTime.Now; // user is ban
            var cBan = db.Feedbacks.Count();
            var result = new adminStatistic
            {
                users = cUser,
                requests = cRequest,
                facilities = cFacility,
                categories = cCategory,
                articles = cArticle,
                question = cQuestion,
                fb = cBan,
                cmt = cCmt
            };
            DateTime past = DateTime.Now.AddDays(-30);
            // pending
            var request = (from r in db.Requests
                           where r.block != 1 && r.created_at > past
                           group r by EntityFunctions.TruncateTime(r.created_at) into grp
                           select new
                           {
                               key = grp.Key,
                               value = grp.Count()
                           });
            // success
            var request1 = (from r in db.Requests
                            where r.block != 1 && r.status == 8 && r.created_at > past
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
            return View(result);
        }
        // facility
        public ActionResult cFacility(int? page)
        {
            var result = db.Facilities.ToList().ToPagedList(page ?? 1, 10);
            return View(result);
        }
        // create new facility
        [Authorize(Roles ="admin")]
        public ActionResult createFacility()
        {
            return View();
        }
        [HttpPost]
        public ActionResult createFacility(Facility f)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    Facility obj = new Facility();
                    obj.FacilityName = f.FacilityName;
                    obj.Description = f.Description;
                    obj.status = f.status;
                    db.Facilities.Add(obj);
                    db.SaveChanges();
                    ViewBag.Success = "Create new facility success !";
                }
            }
            catch(Exception ex)
            {
                ViewBag.msg = ex.Message;
            }
            return View();
        }
        // edit facility
        public ActionResult editFacility(int id)
        {
            var result = db.Facilities.Where(m => m.FacilityId == id).FirstOrDefault();
            return View(result);
        }
        [HttpPost]
        public ActionResult editFacility(Facility f, int id)
        {
            var result = db.Facilities.Where(m => m.FacilityId == id).FirstOrDefault();
            result.FacilityName = f.FacilityName;
            result.Description = f.Description;
            result.status = f.status;
            db.SaveChanges();
            ViewBag.Success = "Update success !";
            return View(result);
        }
        // delete facility
        public ActionResult deleteFacility(int id)
        {
            try
            {
                int result = db.Requests.Where(m => m.facility == id).Count();
                if (result != 0)
                {
                    int error = 1;
                    return RedirectToAction("cFacility", new { conflict = error });
                }
                else
                {
                    var allUser = db.Users.Where(m => m.facility == id).ToList();
                    foreach(var item in allUser)
                    {
                        item.facility = 0;
                        item.role = 1;
                        db.SaveChanges();
                    }
                    var obj = db.Facilities.Where(m => m.FacilityId == id).FirstOrDefault();
                    db.Facilities.Remove(obj);
                    db.SaveChanges();
                    return RedirectToAction("cFacility");
                }
            }
            catch(Exception ex)
            {
                ViewBag.msg = ex.Message;
                return RedirectToAction("cFacility", new { conflict = 1 });
            }
        }
        // view people in facility
        [Authorize(Roles = "admin")]
        public ActionResult userInFacility(int? page, int id)
        {
            var result = db.Users.Where(m => m.facility == id && (m.role == 2 || m.role == 3)).ToList().ToPagedList(page ?? 1, 10);
            return View(result);
        }
        // type of facility
        public ActionResult typeFacility(int id, int ? page)
        {
            var result = db.TypeOfFacilities.Where(m => m.Facility == id).ToList().ToPagedList(page ?? 1, 10);
            return View(result);
        }
        // add type of facility
        [HttpPost]
        public ActionResult typeFacility(int id, int? page, TypeOfFacility t)
        {
            try
            {
                TypeOfFacility obj = new TypeOfFacility();
                obj.TypeName = t.TypeName;
                obj.Status = t.Status;
                obj.Facility = id;
                db.TypeOfFacilities.Add(obj);
                db.SaveChanges();
                ViewBag.Success = "Create success new type of facility";
            }
            catch(Exception ex)
            {
                ViewBag.Msg = ex.Message;
            }
            var result = db.TypeOfFacilities.Where(m => m.Facility == id).ToList().ToPagedList(page ?? 1, 10);
            return View(result);
        }
        // edit type of facility
        public ActionResult editType(int id)
        {
            var result = db.TypeOfFacilities.Where(m => m.TypeId == id).FirstOrDefault();
            return View(result);
        }
        [HttpPost]
        public ActionResult editType(int id, TypeOfFacility t)
        {
            var result = db.TypeOfFacilities.Where(m => m.TypeId == id).FirstOrDefault();
            result.TypeName = t.TypeName;
            result.Status = t.Status;
            db.SaveChanges();
            ViewBag.Success = "Update success";
            return View(result);
        }
        // detele type of facility
        public ActionResult deleteType(int id, int fid)
        {
            int request = db.Requests.Where(m => m.typefacility == id).Count();
            if(request != 0)
            {
                int error = 1;
                return RedirectToAction("typeFacility", new { conflict = error,id=fid });
            }
            else
            {
                var result = db.TypeOfFacilities.Where(m => m.TypeId == id).FirstOrDefault();
                db.TypeOfFacilities.Remove(result);
                db.SaveChanges();
                return RedirectToAction("typeFacility", new { id = fid });
            }
        }
        // category
        public ActionResult cCategory(int? page)
        {
            var result = db.Categories.ToList().ToPagedList(page ?? 1, 10);
            return View(result);
        }
        // create new category
        public ActionResult createCategory()
        {
            return View();
        }
        [HttpPost]
        public ActionResult createCategory(Category c)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Category obj = new Category();
                    obj.categoryName = c.categoryName;
                    obj.description = c.description;
                    db.Categories.Add(obj);
                    db.SaveChanges();
                    ViewBag.Success = "Create new category success !";
                }
            }
            catch (Exception ex)
            {
                ViewBag.msg = ex.Message;
            }
            return View();
        }
        // edit category
        public ActionResult editCategory(int id)
        {
            var result = db.Categories.Where(m => m.CategoryId == id).FirstOrDefault();
            return View(result);
        }
        [HttpPost]
        public ActionResult editCategory(Category c, int id)
        {
            var result = db.Categories.Where(m => m.CategoryId == id).FirstOrDefault();
            result.categoryName = c.categoryName;
            result.description = c.description;
            db.SaveChanges();
            ViewBag.Success = "Update category name success !";
            return View(result);
        }
        // delete category
        public ActionResult deleteCategory(int id)
        {
            int dem = db.Articles.Where(m => m.categoryId == id).Count();
           if(dem != 0)
            {
                int error = 1;
                return RedirectToAction("cCategory", new { conflict = error});
            }
            else
            {
                var result = db.Categories.Where(m => m.CategoryId == id).FirstOrDefault();
                db.Categories.Remove(result);
                db.SaveChanges();
                return RedirectToAction("cCategory");
            }
        }
        // questionnaire
        public ActionResult cQandA(int ? page)
        {
            var result = db.Questionnaires.ToList().OrderByDescending(m => m.qId).ToPagedList(page ?? 1,10);
            return View(result);
        }
        // create new question
        public ActionResult createQandA()
        {
            return View();
        }
        [HttpPost]
        public ActionResult createQandA(Questionnaire q)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Questionnaire obj = new Questionnaire();
                    obj.question = q.question;
                    obj.answer = q.answer;
                    obj.created_at = DateTime.Now;
                    db.Questionnaires.Add(obj);
                    db.SaveChanges();
                    ViewBag.Success = "Create new question and answer success !";
                }
            }
            catch(Exception ex)
            {
                ViewBag.Msg = ex.Message;
            }
            return View();
        }
        // edit q and a
        public ActionResult editQandA(int id)
        {
            var result = db.Questionnaires.Where(m => m.qId == id).FirstOrDefault();
            return View(result);
        }
        [HttpPost]
        public ActionResult editQandA(Questionnaire q, int id)
        {
            var result = db.Questionnaires.Where(m => m.qId == id).FirstOrDefault();
            result.answer = q.answer;
            result.question = q.question;
            result.created_at = DateTime.Now;
            db.SaveChanges();
            ViewBag.Success = "Update success !";
            return View(result);
        }
        // delete q and a
        public ActionResult deleteQandA(int id)
        {
            var result = db.Questionnaires.Where(m => m.qId == id).FirstOrDefault();
            db.Questionnaires.Remove(result);
            db.SaveChanges();
            return RedirectToAction("cQandA");
        }
        // severity manage
        public ActionResult cSeverity(int? page)
        {
            var result = db.Severities.ToList().ToPagedList(page ?? 1, 10);
            return View(result);
        }
        // create severity
        public ActionResult createSeverity()
        {
            return View();
        }
        [HttpPost]
        public ActionResult createSeverity(Severity s)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Severity obj = new Severity();
                    obj.SeverityName = s.SeverityName;
                    obj.Description = s.Description;
                    db.Severities.Add(obj);
                    db.SaveChanges();
                    ViewBag.Success = "Create new severity success !";
                }
                else
                {
                    ViewBag.msg = "There is something wrong happened !";
                }
            }
            catch(Exception ex)
            {
                ViewBag.msg = ex.Message;
            }
            return View();
        }
        // edit severity
        public ActionResult editSeverity(int id)
        {
            var result = db.Severities.Where(m => m.SeverityId == id).FirstOrDefault();
            if(result == null)
            {
                return Redirect("/");
            }
            return View(result);
        }
        [HttpPost]
        public ActionResult editSeverity(int id, Severity s)
        {

            var result = db.Severities.Where(m => m.SeverityId == id).FirstOrDefault();
            result.SeverityName = s.SeverityName;
            result.Description = s.Description;
            db.SaveChanges();
            ViewBag.Success = "Update success !";
            return View(result);
        }
        // delete severity
        public ActionResult delSeverity(int id)
        {
            int result = db.Requests.Where(m => m.severity == id).Count();
            if(result != 0)
            {
                int error = 1;
                return RedirectToAction("cSeverity",new {conflict=error});
            }
            else
            {
                var obj = db.Severities.Where(m => m.SeverityId == id).FirstOrDefault();
                db.Severities.Remove(obj);
                db.SaveChanges();
                return RedirectToAction("cSeverity");
            }
        }
        // acount
        public ActionResult cAccount(int? page, int type = 0)
        {
            if(type == 0)
            {
                var result = db.Users.ToList().OrderByDescending(m => m.UserId).ToPagedList(page ?? 1, 10);
                return View(result);
            }
            else
            {
                var result = db.Users.Where(m => m.role == type).ToList().OrderByDescending(m => m.UserId).ToPagedList(page ?? 1, 10);
                return View(result);
            }
        }
        // create an account
        public ActionResult createUser()
        {
            TempData["coso"] = db.Facilities.ToList();
            TempData["role"] = db.Roles.ToList();
            TempData.Keep("coso");
            TempData.Keep("role");
            return View();
        }
        [HttpPost]
        public ActionResult createUser(User u)
        {
            TempData["coso"] = db.Facilities.ToList();
            TempData["role"] = db.Roles.ToList();
            TempData.Keep("coso");
            TempData.Keep("role");
            try
            {
                if (ModelState.IsValid)
                {
                    User obj = new User();
                    obj.username = u.username;
                    obj.password = encryptPassword.encrypt(u.password); // ma hoa 
                    obj.fullname = u.fullname;
                    obj.gender = u.gender;
                    obj.email = u.email;
                    obj.role = u.role;
                    obj.facility = u.facility;
                    obj.birthday = DateTime.Now.Date;
                    obj.timeJoin = DateTime.Now;
                    obj.status = 0;
                    obj.avatar = "default.jpg";
                    db.Users.Add(obj);
                    db.SaveChanges();
                    ViewBag.Success = "Create new account success !";
                }
                else
                {
                    ViewBag.Msg = "There is something wrong happening ?";
                }
            }
            catch(Exception ex)
            {
                ViewBag.Msg = ex.Message;
            }
            return View();
        }
        // delete
        public ActionResult deleteUser(int id)
        {
            var result = db.Users.Where(m => m.UserId == id).FirstOrDefault();
            db.Users.Remove(result);
            db.SaveChanges();
            return RedirectToAction("cAccount");
        }
        // manage request
        public ActionResult cRequest(int? page)
        {

            TempData["facility"] = db.Facilities.Where(m => m.status == 0).ToList();
            // get first facility to show on index
            var first = db.Facilities.Where(m => m.status == 0).FirstOrDefault(); // facility is active
            if(first != null)
            {
                TempData["typeOfFacility"] = db.TypeOfFacilities.Where(m => m.Facility == first.FacilityId && m.Status == 0).ToList();
            }
            var requests = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join se in db.Severities on r.severity equals se.SeverityId
                          join u in db.Users on r.requestor equals u.UserId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.block == 0
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
        // delete request
        public ActionResult delRequest(int id)
        {
            var result = db.Requests.Where(m => m.RequestId == id).FirstOrDefault();
            if(result != null)
            {
                db.Requests.Remove(result);
                db.SaveChanges();
            }
            return RedirectToAction("cRequest");
        }
        // filter request
        public ActionResult filterRequest(DateTime tu, DateTime den, int facility = 0, int type = 0)
        {
            System.Threading.Thread.Sleep(1000);
            if (facility == 0)
            {
                var requests = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join se in db.Severities on r.severity equals se.SeverityId
                          join u in db.Users on r.requestor equals u.UserId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.block == 0
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
                return PartialView("_filterRequest", requests);
            }
            else
            {
                if(type == 0)
                {
                    var requests = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join se in db.Severities on r.severity equals se.SeverityId
                          join u in db.Users on r.requestor equals u.UserId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.block == 0 && r.facility == facility
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
                return PartialView("_filterRequest", requests);
                }
                else
                {
                    var requests = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join se in db.Severities on r.severity equals se.SeverityId
                          join u in db.Users on r.requestor equals u.UserId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.block == 0 && r.facility == facility && r.typefacility == type
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
                return PartialView("_filterRequest", requests);
                }
            }
        }
        // search user
        public ActionResult cSearch()
        {
            return View();
        }
        // tra ve ket qua cho partial view
        public ActionResult GetUser(string name, int type = 0)
        {
            System.Threading.Thread.Sleep(1000);
            if (type == 0)
            {
                var result = db.Users.Where(m => m.username.Contains(name)).ToList();
                return PartialView("_seachUser", result);
            }
            else
            {
                var result = db.Users.Where(m => m.fullname.Contains(name)).ToList();
                return PartialView("_seachUser", result);
            }
        }
        // print report
        public ActionResult PrintReport(DateTime tu, DateTime den, int facility = 0, int type = 0)
        {
            if (facility == 0) // if type = 0, it means get all types of facility
            {   
                var requests = (from r in db.Requests join f in db.Facilities
                        on r.facility equals f.FacilityId
                        join s in db.Status on r.status equals s.StatusId
                        join se in db.Severities on r.severity equals se.SeverityId
                        join u in db.Users on r.requestor equals u.UserId
                        join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                        where r.block == 0
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
                if(type == 0)
                {
                    var requests = (from r in db.Requests join f in db.Facilities
                          on r.facility equals f.FacilityId
                          join s in db.Status on r.status equals s.StatusId
                          join se in db.Severities on r.severity equals se.SeverityId
                          join u in db.Users on r.requestor equals u.UserId
                          join t in db.TypeOfFacilities on r.typefacility equals t.TypeId
                          where r.facility == facility && r.block == 0
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
                          where r.facility == facility && r.block == 0 && r.typefacility == type
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
        //end
        //print user
        public ActionResult printUser()
        {
            var result = db.Users.ToList().OrderByDescending(m => m.UserId);
            var report = new ViewAsPdf(result);
            return report;
        }
    }
}