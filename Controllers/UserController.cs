using LoginRegistrationApp.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace LoginRegistrationApp.Controllers
{
    public class UserController : Controller
    {
        // Registration Action
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        // Registration POST action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified, Activationcode")] UserData user)
        {
            bool Status = false;
            string message = "";

            // Model Validation
            if (ModelState.IsValid)
            {
                #region Check if Email exists
                var isEmail =  IfEmailExist(user.EmailID);
                if (isEmail)
                {
                    ModelState.AddModelError("EmailExist", "Email Exists");
                    return View(user);
                }
                #endregion

                #region Generate Activation Code

                user.Activationcode = Guid.NewGuid();
                #endregion

                #region Password Hashing
                user.Password = PasswordEncryption.Hash(user.Password);
                user.ConfirmPassword = PasswordEncryption.Hash(user.ConfirmPassword);
                #endregion
                user.IsEmailVerified = false;

                #region Save data to Database
                using (UserInfoEntities ud = new UserInfoEntities())
                {
                    ud.UserDatas.Add(user);
                    ud.SaveChanges();
                }
                #endregion

                #region Send Email to user
                SendVerificationEmail(user.EmailID, user.Activationcode.ToString());
                message = "Registration successfully done. Account activation link has been sent your email id:" +
                    user.EmailID;
                Status = true;
                #endregion
            }
            else
            {
                message = "Invalid Request";
            }

            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(user);
        }

        // Verify Email
        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;
            using (UserInfoEntities ud = new UserInfoEntities())
            {
                ud.Configuration.ValidateOnSaveEnabled = false;

                var valid = ud.UserDatas.Where(e => e.Activationcode == new Guid(id)).FirstOrDefault();
                if (valid != null)
                {
                    valid.IsEmailVerified = true;
                    ud.SaveChanges();
                    Status = true;
                }
                else
                {
                    ViewBag.Message = "Invaliid Request";
                }             
            }
            ViewBag.Status = Status;
            return View();
        }

        // Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // Login POST action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin login, string ReturnUrl)
        {
            string message = "";
            using (UserInfoEntities ud = new UserInfoEntities())
            {
                var validate = ud.UserDatas.Where(e => e.EmailID == login.EmailID).FirstOrDefault();
                if (validate != null)
                {
                    if (string.Compare(PasswordEncryption.Hash(login.Password), validate.Password) == 0)
                    {
                        int timeout = login.RememberMe ? 525600 : 20; // 525600 min = 1 year
                        var ticket = new FormsAuthenticationTicket(login.EmailID, login.RememberMe, timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);


                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        message = "Invalid Email or Password";

                    }
                }
                else
                {
                    message = "Invalid Email or Password";
                }
            }

            ViewBag.Message = message;
            return View();
        }

        // Logout
        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }


        [NonAction]
        public bool IfEmailExist(string emailID)
        {
            using (UserInfoEntities userInfoEntities = new UserInfoEntities() )
            {
                var check = userInfoEntities.UserDatas.Where(ud => ud.EmailID == emailID).FirstOrDefault();
                return check != null;
            }

        }

        [NonAction]
        public void SendVerificationEmail(string emailID, string activationCode)
        {
            var verifyUrl = "/User/VerifyAccount/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var senderEmail = new MailAddress("nenefrank41@gmail.com", "Learn with Marv");
            var senderPassword = "m4crystfs1998";
            var receiverEmail = new MailAddress(emailID);
            string subject = "Account Creation Successfully!";
            string body = "<br/><br/>We are glad to inform you that your account has successfully been created." +
                " Please click on the link below to verify your account.<br/><br/>" +
                $"<a href = {link}>{link}</a>";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderEmail.Address, senderPassword)
            };

            using (var message = new MailMessage(senderEmail, receiverEmail) { Subject = subject, Body = body, IsBodyHtml = true})
            {
                smtp.Send(message);
            }
        }
    }
}