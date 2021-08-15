using eProjectHDO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace eProjectHDO.Controllers
{
    public class EmailController : Controller
    {
        ohdEntities db = new ohdEntities();
        public ActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Index(string user, string email)
        {
            try
            {
                string msg = ""; // return text for client 
                var result = db.Users.Where(m => m.username == user && m.email == email).FirstOrDefault();
                if (result == null) // no result
                {
                    msg = "These credentials do not match our records.";
                    return Json(new { msg = msg, success = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    MailMessage mail = new MailMessage("huahoangduy.cusc@gmail.com", email);
                    mail.Subject = "Reset Password - CUSC";
                    string password = GetRandomPassword();
                    mail.Body = "Your new password is <b>" + password + "</b> <br>Please do not disclose your password to anyone.";
                    mail.IsBodyHtml = true;
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    NetworkCredential nc = new NetworkCredential("huahoangduy.cusc@gmail.com", "lanhuthedo");
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = nc;
                    Task.Factory.StartNew(() => smtp.Send(mail));
                    result.password = encryptPassword.encrypt(password); // ma hoa 
                    db.SaveChanges(); // save new password
                    msg = "We have sent to your email address a new password, please check your mailbox !";
                    return Json(new { success = msg, msg = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
                return Json(new { msg = msg, success = "" }, JsonRequestBehavior.AllowGet);
            }
            // end if
        }
        // tao mat khau ngau nhien, random password
        public string GetRandomPassword()
        {
            int length = 8;
            const string chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            StringBuilder sb = new StringBuilder();
            Random rnd = new Random();
            for (int i = 0; i < length; i++)
            {
                int index = rnd.Next(chars.Length);
                sb.Append(chars[index]);
            }
            return sb.ToString();
        }
    }
}