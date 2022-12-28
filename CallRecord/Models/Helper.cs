using AdaniCall.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AdaniCall.Entity.Enums;
using System.Web.Mvc;
using AdaniCall.Utility.Common;
using AdaniCall.Utility;
using Newtonsoft.Json;

namespace AdaniCall.Models
{
    public class Helper
    {
        private readonly string _module = "AdaniCall.Models.Helper";

        private bool _IsRegisterProcess = false;

        public bool IsRegisterProcess
        {
            get { return _IsRegisterProcess; }
            set { _IsRegisterProcess = value; }
        }
        public LoginVM GetSession()
        {
            LoginVM loginVM = new LoginVM();

            try
            {
                if (HttpContext.Current.Session[KeyEnums.SessionKeys.UserSession.ToString()] != null)
                {
                    loginVM = JsonConvert.DeserializeObject<LoginVM>(HttpContext.Current.Session[KeyEnums.SessionKeys.UserSession.ToString()].ToString());
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(_module, "GetSession()", ex.Source, ex.Message, ex);
            }
            return loginVM;
        }

        public LoginVM UpdateSession(LoginVM _LoginVM)
        {
            LoginVM loginVM = new LoginVM();
            try
            {
                HttpContext.Current.Session[KeyEnums.SessionKeys.UserSession.ToString()] = JsonConvert.SerializeObject(_LoginVM);
            }
            catch (Exception ex)
            {
                Log.WriteLog(_module, "UpdateSession()", ex.Source, ex.Message, ex);
            }
            return loginVM;
        }

        public void IsFirstTimeLoginChangeStatus()
        {
            if (HttpContext.Current.Session[KeyEnums.SessionKeys.UserSession.ToString()] != null)
            {
                LoginVM LM = GetSession();
                LM.IsFirstTimeLogin = false;
                HttpContext.Current.Session[KeyEnums.SessionKeys.UserSession.ToString()] = JsonConvert.SerializeObject(LM);
            }
        }
        public void SetUserLanguage(long LanguageId)
        {
            HttpContext.Current.Session[KeyEnums.SessionKeys.LanguageId.ToString()] = LanguageId;
        }

        public void SetSession(Users user)
        {
            try
            {
                if (user != null)
                {
                    LoginVM loginVM = new LoginVM();
                    loginVM.SessionId = HttpContext.Current.Session.SessionID;

                    loginVM.Id = user.ID;
                    loginVM.UserName = user.UserName;
                    loginVM.Password = user.Password;
                    loginVM.FirstName = user.FirstName;
                    loginVM.LastName = user.LastName;
                    loginVM.RoleId = user.RoleId;
                    loginVM.UserRole = (int)user.RoleId;
                    loginVM.ParentId = user.ParentId;

                    loginVM.IsEmailVerified = user.IsEmailVerified;
                    loginVM.EmailVerficationCode = user.EmailVerficationCode;
                    loginVM.EmailVerificationDate = user.EmailVerificationDate;
                    loginVM.StatusId = user.StatusId;
                    loginVM.CreatedDate = user.CreatedDate;
                    loginVM.LanguageId = user.LanguageId;
                    loginVM.LocationID = user.AgentLocationID;
                    loginVM.CallerID = user.AgentCallID;
                    if (IsRegisterProcess)
                    {
                        loginVM.IsFirstTimeLogin = true;
                    }

                    if (!string.IsNullOrWhiteSpace(loginVM.FirstName))
                        loginVM.Initial = StringFilter.GetInitialChar(loginVM.FirstName);

                    if ((!string.IsNullOrWhiteSpace(loginVM.FirstName)) && (!string.IsNullOrWhiteSpace(loginVM.LastName)))
                        loginVM.InitialChars = StringFilter.GetInitials(loginVM.FirstName);

                    loginVM.ParentId = user.ParentId;

                    CommonData _CommonData = new CommonData();
                    loginVM.ProfileLanguage = _CommonData.LanguageNameFromId(loginVM.LanguageId.ToString());
                    loginVM.SelectedLanguage = _CommonData.LanguageNameFromId(loginVM.LanguageId.ToString());
                    HttpContext.Current.Session[KeyEnums.SessionKeys.UserId.ToString()] = loginVM.Id.ToString();
                    HttpContext.Current.Session[KeyEnums.SessionKeys.FirstName.ToString()] = loginVM.FirstName.ToString();
                    HttpContext.Current.Session[KeyEnums.SessionKeys.LastName.ToString()] = loginVM.LastName.ToString();
                    HttpContext.Current.Session[KeyEnums.SessionKeys.UserRole.ToString()] = loginVM.UserRole.ToString();
                    HttpContext.Current.Session[KeyEnums.SessionKeys.CallerID.ToString()] = !string.IsNullOrWhiteSpace(loginVM.CallerID) ? loginVM.CallerID.ToString() : "";
                    HttpContext.Current.Session[KeyEnums.SessionKeys.KioskID.ToString()] = !string.IsNullOrWhiteSpace(loginVM.KioskID) ? loginVM.KioskID.ToString() : "";
                    HttpContext.Current.Session[KeyEnums.SessionKeys.LocationID.ToString()] = loginVM.LocationID.ToString();

                    if(!string.IsNullOrWhiteSpace(loginVM.UserName))
                        HttpContext.Current.Session[KeyEnums.SessionKeys.UserEmailID.ToString()] = loginVM.UserName.ToString();
                    HttpContext.Current.Session[KeyEnums.SessionKeys.UserLogo.ToString()] = loginVM.Profile_Logo.ToString();
                    HttpContext.Current.Session[KeyEnums.SessionKeys.UserSession.ToString()] = JsonConvert.SerializeObject(loginVM);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(_module, "SetSession(User user)", ex.Source, ex.Message, ex);
            }
        }

        public AccessMember GetAccessMember()
        {
            AccessMember objAM = new AccessMember();
            try
            {
                objAM.Url = HttpContext.Current.Request.Url != null ? HttpContext.Current.Request.Url.ToString() : "";
                objAM.ReferrerURL = HttpContext.Current.Request.UrlReferrer != null ? HttpContext.Current.Request.UrlReferrer.ToString() : "";
                objAM.port = HttpContext.Current.Request.Url.Port != 0 ? HttpContext.Current.Request.Url.Port.ToString() : "";
                objAM.Host = HttpContext.Current.Request.Url.Host != null ? HttpContext.Current.Request.Url.Host.ToString() : "";

                var browser = HttpContext.Current.Request.Browser;

                if (HttpContext.Current.Request.ServerVariables["REMOTE_HOST"] != null)
                    objAM.REMOTE_HOST = HttpContext.Current.Request.ServerVariables["REMOTE_HOST"];
                else
                    objAM.REMOTE_HOST = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ?? HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

                objAM.REMOTE_ADDR_IP = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"] != null ? HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"] : "";
                objAM.Useragent = (HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"] != null ? HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].ToString() : "");
                objAM.BrowserType = (browser.Type != null ? browser.Type.ToString() : "");
                objAM.BrowserVersion = (browser.Browser != null ? browser.Browser.ToString() : "");
                objAM.Platform = (browser.Platform != null ? browser.Platform.ToString() : "");
            }
            catch (Exception ex)
            {
                Log.WriteLog(_module, "getAccessMember()", ex.Source, ex.Message, ex);
            }
            return objAM;
        }
    }
}
