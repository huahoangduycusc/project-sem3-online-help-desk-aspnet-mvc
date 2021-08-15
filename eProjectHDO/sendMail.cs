using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace eProjectHDO
{
    public static class sendMail
    {
        public static void mailNotification(string subject, string msg,string email)
        {
            try
            {
                //string admin = "huahoangduy.cusc@gmail.com";
                //string password = "lanhuthedo";
                //MailMessage mail = new MailMessage("huahoangduy.cusc@gmail.com", email);
                //mail.Subject = subject;
                //mail.Body = "<html><body>Hi you !<br/><br/>" + msg + "<br> Regard !<br /><br/><font color='red'>Online Help Desk</font></body></html>";
                //mail.IsBodyHtml = true;
                //SmtpClient smtp = new SmtpClient();
                //smtp.Host = "smtp.gmail.com";
                //smtp.Port = 587;
                //smtp.EnableSsl = true;
                //NetworkCredential nc = new NetworkCredential(admin, password);
                //smtp.UseDefaultCredentials = false;
                //smtp.Credentials = nc;
                //Task.Factory.StartNew(() => smtp.Send(mail));
            }
            catch(Exception ex)
            {
                string error = ex.Message;
            }
        }
    }
}