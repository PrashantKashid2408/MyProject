using AdaniCall.Business.BusinessFacade;
using AdaniCall.Entity;
using AdaniCall.Entity.Common;
using AdaniCall.Entity.Enums;
using AdaniCall.Models;
using AdaniCall.Utility.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using AdaniCall.Resources;
using AdaniCall.Controllers;
using AdaniCall.Business.DataAccess.Wrapper;
using AdaniCall.Business.DataAccess.Constants;
using System.Configuration;

namespace AdaniCall.Controllers
{
    [Localisation]
    public class UserController : Controller
    {
        // GET: User
        private readonly string _module = "AdaniCall.Controllers.UserController";
        JsonMessage _jsonMessage = null;
        
        UsersBusinessFacade objUserBusinessFacade = new UsersBusinessFacade();
        Users objUserEntity = new Users();
        LoginVM _loginVM = null;
        Helper _helper = null;

        public UserController()
        {
            _helper = new Helper();
            _loginVM = _helper.GetSession();
        }

        private string GetUrl(string actionName, string controllerName)
        {
            return "/" + controllerName + "/" + actionName;
        }

        [HttpGet]
        public ActionResult SessionEnd()
        {
            return Redirect(URLDetails.GetSiteRootUrl());

        }

        [HttpGet]
        public string GetCurrentSession()
        {
            string UID = string.Empty;
            try
            {
                if (_loginVM != null)
                    UID = _loginVM.Id.ToString();
            }
            catch (Exception ex)
            {
                Log.WriteLog(_module, "GetCurrentSession()", ex.Source, ex.Message, ex);
            }
            return UID;
        }

        public ActionResult Login()
        {
            _helper = new Helper();
            _loginVM = _helper.GetSession();
            //if (_loginVM != null && _loginVM.Id > 0)
            //    return RedirectToAction(AdaniCallConstants.DefaultView, AdaniCallConstants.DefaultController);

            //if (AdaniCallConstants.DefaultController.ToLower() == "home")
            //{
                Session[KeyEnums.SessionKeys.UserLanguage.ToString()] = "EN";
                Session[KeyEnums.SessionKeys.LandingLanguage.ToString()] = "EN";
            //}
            string strCookieUserName = "";
            //if (objUserEntity.RoleId == (byte)RoleEnums.Role.Kiosk || objUserEntity.RoleId == (byte)RoleEnums.Role.Agent)
            strCookieUserName = CookieStore.GetDecryptedCookieByKey("AGCXWebLoginUser");


            if (!string.IsNullOrWhiteSpace(strCookieUserName))
            {
                _jsonMessage = IsLoginValid(strCookieUserName, "", LoginMode.COOKIE);
                if (_jsonMessage.IsSuccess)
                    objUserEntity = (Users)_jsonMessage.Data;
            }
            if (Session[KeyEnums.SessionKeys.UserId.ToString()] != null)
            {
                return Redirect(GetRedirectUrl());
            }
            else if (!string.IsNullOrWhiteSpace(strCookieUserName))
            {
                if (_jsonMessage.IsSuccess)
                {
                    if (objUserEntity != null)
                    {
                        _helper.SetSession(objUserEntity);
                        if (objUserEntity.ID > 0)
                        {
                            UsersBusinessFacade objUsers = new UsersBusinessFacade();
                            objUsers.ChangeAvailabilityStatus(objUserEntity.ID, "1");
                        }
                    }

                    _helper.SetUserLanguage(objUserEntity.LanguageId);
                    InsertAccessMember(objUserEntity, _helper.GetAccessMember(), "Login");
                    CommonData _CommonData = new CommonData();

                    return Redirect(GetRedirectUrl());
                }
            }
            else
            {
                Session.Abandon();
                FormsAuthentication.SignOut();
                //ViewBag.IsTokenAuthenticated = false;
                TempData["IsUserLoggedOut"] = "1";
            }

            return View();
        }

        public ActionResult Logout()
        {
            try
            {
                string UserName = string.Empty;
                if (_loginVM != null)
                    UserName = _loginVM.UserName;
                Int64 UserID = 0;
                if (HttpContext.Session[KeyEnums.SessionKeys.UserId.ToString()] != null)
                    UserID = Convert.ToInt64(HttpContext.Session[KeyEnums.SessionKeys.UserId.ToString()]);

                if (UserID > 0) {
                    UsersBusinessFacade objUsers = new UsersBusinessFacade();
                    objUsers.ChangeAvailabilityStatus(UserID, "2");
                }

                UserLogOut();

                string StrUrl = string.Empty;
                StrUrl = URLDetails.GetSiteRootUrl().TrimEnd('/');
                return Redirect(StrUrl);
            }
            catch (Exception ex)
            {
                Log.WriteLog(_module, "Logout()", ex.Source, ex.Message, ex);
                return RedirectToAction("Index", "Home");
            }
        }

        private void UserLogOut()
        {
            ClearCache();//Delete the user details from cache.
            Session.Abandon();

            if (Request.Cookies["LoginData"] != null)
            {
                Response.Cookies["LoginData"].Expires = DateTime.Now.AddDays(-1);
            }
            if (Response.Cookies["UserLanguage"] != null)
            {
                Response.Cookies["UserLanguage"].Expires = DateTime.Now.AddDays(-1);
            }

            FormsAuthentication.SignOut(); //Delete the authentication ticket and sign out.

            CookieStore.RemoveCookie("AGCXWebLoginUser");
        }

        private void ClearCache()
        {
            HttpContext.Cache.Remove("_userId" + Session.SessionID + "_" + "true");
            HttpContext.Cache.Remove("AcceptToken_" + Session.SessionID);
            HttpContext.Cache.Remove("CallToken_" + Session.SessionID);
        }

        private string GetRedirectUrl()
        {
            //if (objUserEntity.RoleId == (byte)RoleEnums.Role.Kiosk)
            //        return GetUrl("Call", "Home");
            //else
            if (objUserEntity.RoleId == (byte)RoleEnums.Role.Agent)
                return GetUrl("Accept", "Home");
            else
                return GetUrl(AdaniCallConstants.DefaultView, AdaniCallConstants.DefaultController);
        }

        [HttpPost]
        public JsonResult Login(string username, string password, string queryString, bool isRemember, bool autologin)
        {
            try
            {
                HttpCookie isRemembercookie = new HttpCookie("isRemembercookie");
                HttpCookie cookie = new HttpCookie("LoginData");

                if (isRemember)
                {
                    isRemembercookie.Values.Add("RememberMe", "true");
                    int cookieDays = Convert.ToInt32(AdaniCallConstants.LoginCookie);
                    isRemembercookie.Expires = DateTime.Now.AddDays(cookieDays);
                    Response.Cookies.Add(isRemembercookie);
                }
                else
                {
                    isRemembercookie.Values.Add("RememberMe", "false");
                    int cookieDays = Convert.ToInt32(AdaniCallConstants.LoginCookie);
                    isRemembercookie.Expires = DateTime.Now.AddDays(cookieDays);
                    Response.Cookies.Add(isRemembercookie);
                }

                LoginVM loginVM = new LoginVM();

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    _jsonMessage = new JsonMessage(false, Resource.lbl_error, Resource.lbl_msg_emailPasswordRequired, KeyEnums.JsonMessageType.ERROR);
                }
                else
                {
                    password = new Encription().Encrypt(password);
                    _jsonMessage = IsLoginValid(username, password, LoginMode.CMS);

                    if (loginVM != null)
                    {
                        if (_jsonMessage.IsSuccess)
                        {
                            if (isRemember)
                            {
                                cookie.Values.Add("UserName", username);
                                cookie.Values.Add("ReturnURL", _jsonMessage.ReturnUrl);
                                cookie.HttpOnly = true;
                                int cookieDays = Convert.ToInt32(AdaniCallConstants.LoginCookie);
                                cookie.Expires = DateTime.Now.AddDays(cookieDays);
                                Response.Cookies.Add(cookie);
                            }
                            objUserEntity = (Users)_jsonMessage.Data;
                            if (isRemember)
                            {
                                cookie.Values.Add("UserRoleID", objUserEntity.RoleId.ToString());
                            }

                            if (objUserEntity != null)
                            {
                                _helper.SetSession(objUserEntity);
                                if (objUserEntity.ID > 0)
                                {
                                    UsersBusinessFacade objUsers = new UsersBusinessFacade();
                                    objUsers.ChangeAvailabilityStatus(objUserEntity.ID, "1");
                                    if(objUserEntity.RoleId == (byte)RoleEnums.Role.Kiosk || objUserEntity.RoleId == (byte)RoleEnums.Role.Agent)
                                        CookieStore.SetEncryptedCookieKey("AGCXWebLoginUser", username, TimeSpan.MaxValue);
                                }
                            }

                            _helper.SetUserLanguage(objUserEntity.LanguageId);
                            InsertAccessMember(objUserEntity, _helper.GetAccessMember(), "Login");
                            CommonData _CommonData = new CommonData();

                            //if (objUserEntity.RoleId == (byte)RoleEnums.Role.Kiosk)
                            //    _jsonMessage.ReturnUrl = URLDetails.GetSiteRootUrl().TrimEnd('/') + "/Home/Call";
                            //else if (objUserEntity.RoleId == (byte)RoleEnums.Role.Agent)
                            //    _jsonMessage.ReturnUrl = URLDetails.GetSiteRootUrl().TrimEnd('/') + "/Home/Accept";
                            //else 
                            if (objUserEntity.RoleId == (byte)RoleEnums.Role.SuperAdmin || objUserEntity.RoleId == (byte)RoleEnums.Role.Admin || objUserEntity.RoleId == (byte)RoleEnums.Role.LocationAdmin)
                                _jsonMessage.ReturnUrl = URLDetails.GetSiteRootUrl().TrimEnd('/') + "/Transactions/List";
                            else if (objUserEntity.RoleId == (byte)RoleEnums.Role.Agent)
                                _jsonMessage.ReturnUrl = URLDetails.GetSiteRootUrl().TrimEnd('/') + "/Home/Accept";
                            else
                                _jsonMessage.ReturnUrl = URLDetails.GetSiteRootUrl().TrimEnd('/') + "/" + AdaniCallConstants.DefaultController + "/" + AdaniCallConstants.DefaultView + "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(_module, "Login(username:" + username + ",password:" + password + ")", ex.Source, ex.Message, ex);
                _jsonMessage = new JsonMessage(false, Resource.lbl_error, Resource.lbl_msg_internalServerErrorOccurred, KeyEnums.JsonMessageType.DANGER, "", "username", string.Format("Method : Login(), Source : {0}, Message {1}", ex.Source, ex.Message));
            }
            return Json(_jsonMessage);
        }

        public JsonMessage IsLoginValid(string username, string password, string LoginMode = "")
        {
            Users objUserEntity = new Users();
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                    _jsonMessage = new JsonMessage(false, Resource.lbl_msg_invalidEmailAddress, Resource.lbl_msg_invalidEmailAddress, KeyEnums.JsonMessageType.DANGER);
                else if (string.IsNullOrWhiteSpace(password) && LoginMode == "")
                    _jsonMessage = new JsonMessage(false, Resource.lbl_msg_invalidPassowrd, Resource.lbl_msg_invalidPassowrd, KeyEnums.JsonMessageType.DANGER);
                else
                {
                    string[] Fieldsname = new string[2];
                    string[] Values = new string[2];
                    Fieldsname[0] = username;
                    Fieldsname[1] = password;
                    Values[0] = username;
                    Values[1] = password;
                    string StrUrl = URLDetails.GetSiteRootUrl().TrimEnd('/');
                    UsersBusinessFacade objUsersBusinessFacade = new UsersBusinessFacade();
                    objUserEntity = objUsersBusinessFacade.Authenticate(username, password, LoginMode);

                    if (objUserEntity != null)
                    {
                        if (objUserEntity.StatusId == (byte)StateEnums.Statuses.Active || objUserEntity.StatusId == (byte)StateEnums.Statuses.Pending)
                        {
                            _jsonMessage = new JsonMessage(true, Resource.lbl_Cap_success, Resource.lbl_msg_dataSavedSuccessfully, KeyEnums.JsonMessageType.SUCCESS, StrUrl, "true", objUserEntity);
                        }
                        else if (objUserEntity.StatusId == (byte)StateEnums.Statuses.InActive)
                        {
                            _jsonMessage = new JsonMessage(false, Resource.lbl_error, Resource.lbl_accountDisabled, KeyEnums.JsonMessageType.FAILURE, "/User/Login");
                        }
                        else if (objUserEntity.StatusId == (byte)StateEnums.Statuses.Deleted)
                        {
                            _jsonMessage = new JsonMessage(false, Resource.lbl_error, Resource.lbl_accountDeleted, KeyEnums.JsonMessageType.FAILURE, "/User/Login");
                        }
                        else if (objUserEntity.StatusId == (byte)StateEnums.Statuses.Active && objUserEntity.IsEmailVerified == true)
                        {
                            _jsonMessage = new JsonMessage(true, Resource.lbl_Cap_success, Resource.lbl_msg_dataSavedSuccessfully, KeyEnums.JsonMessageType.SUCCESS, StrUrl, "true", objUserEntity);
                        }
                        else
                        {
                            _jsonMessage = new JsonMessage(false, Resource.lbl_error, Resource.lbl_msg_loginFailed, KeyEnums.JsonMessageType.ERROR);
                        }
                    }
                    else
                        _jsonMessage = new JsonMessage(false, Resource.lbl_error, Resource.lbl_msg_invalidEmailAddressPassword, KeyEnums.JsonMessageType.ERROR);
                }
            }
            catch (Exception ex)
            {
                _jsonMessage = new JsonMessage(false, Resource.lbl_msg_internalServerErrorOccurred, Resource.lbl_msg_internalServerErrorOccurred, KeyEnums.JsonMessageType.ERROR, ex.Message);
                Log.WriteLog(_module, "IsLoginValid(username=" + username + ", password=" + password + ")", ex.Source, ex.Message, ex);
            }

            return _jsonMessage;
        }

        private void InsertAccessMember(Users objUsers, AccessMember objAccessMember, string ClickedBy)
        {
            try
            {
                objAccessMember.UserID = objUsers.ID;
                objAccessMember.ClickedBy = ClickedBy;
                objAccessMember.LocationID = objUsers.AgentLocationID;
                objAccessMember.Role = objUsers.RoleId;
                AccessMemberBusinessFacade objAccessMemberBusinessFacade = new AccessMemberBusinessFacade();
                var AMID = objAccessMemberBusinessFacade.Save(objAccessMember);
            }
            catch (Exception ex)
            {
                Log.WriteLog(_module, "InsertAccessMember(UserID: " + objUsers.ID + ", clickedby:" + ClickedBy + ")", ex.Source, ex.Message);
            }
        }

        [HttpPost]
        public JsonResult IsRefreshRequired()
        {
            string IsRefreshRequired = ConfigurationManager.AppSettings["IsRefreshNeeded"].ToString();
            JsonMessage _jsonMessage = null;
            Users objUsers = new Users();
            objUsers.ID = Convert.ToInt32(IsRefreshRequired);
            _jsonMessage = new JsonMessage(true, Resources.Resource.lbl_Cap_success, "", KeyEnums.JsonMessageType.SUCCESS, objUsers);
            return Json(_jsonMessage);
        }
        
        [HttpPost]
        public JsonResult GetAvailableAgent()
        {
            JsonMessage _jsonMessage = null;
            UsersBusinessFacade objBF = new UsersBusinessFacade();
            Users objUsers = new Users();
            Int64 UserID = 0;
            try
            {
                if (HttpContext.Session[KeyEnums.SessionKeys.UserId.ToString()] != null)
                    UserID = Convert.ToInt64(HttpContext.Session[KeyEnums.SessionKeys.UserId.ToString()]);

                objUsers = objBF.GetAvailableAgent(UserID);
                _jsonMessage = new JsonMessage(true, Resources.Resource.lbl_Cap_success, "", KeyEnums.JsonMessageType.SUCCESS, objUsers);
            }
            catch (Exception ex)
            {
                Log.WriteLog(_module, "GetAvailableAgent", ex.Source, ex.Message, ex);
            }
            return Json(_jsonMessage);
        }

        [HttpPost]
        public JsonResult GetKioskDetails(string TravellerCallerID)
        {
            JsonMessage _jsonMessage = null;
            KioskMasterBusinessFacade objBF = new KioskMasterBusinessFacade();
            KioskMaster objKioskMaster = new KioskMaster();
            try
            {
                objKioskMaster = objBF.GetKioskDetails(TravellerCallerID);
                _jsonMessage = new JsonMessage(true, Resources.Resource.lbl_Cap_success, "", KeyEnums.JsonMessageType.SUCCESS, objKioskMaster);
            }
            catch (Exception ex)
            {
                Log.WriteLog(_module, "GetKioskDetails(TravellerCallerID:" + TravellerCallerID + ")", ex.Source, ex.Message, ex);
            }
            return Json(_jsonMessage);
        }

        [HttpPost]
        public JsonResult ChangeAvailabilityStatus(string AvailabilityStatus, string AgentCallerID = "", string tranID = "")
        {
            Int64 UserID = 0;
            JsonMessage _jsonMessage = null;
            try
            {
                if (HttpContext.Session[KeyEnums.SessionKeys.UserId.ToString()] != null)
                    UserID = Convert.ToInt64(HttpContext.Session[KeyEnums.SessionKeys.UserId.ToString()]);

                if (AgentCallerID != "")
                {
                    UsersBusinessFacade objUsers = new UsersBusinessFacade();
                    _jsonMessage = objUsers.ChangeAvailabilityStatus(UserID, AvailabilityStatus, AgentCallerID);
                }
                else if (UserID > 0)
                {
                    UsersBusinessFacade objUsers = new UsersBusinessFacade();
                    _jsonMessage = objUsers.ChangeAvailabilityStatus(UserID, AvailabilityStatus, "");
                }

                if (AvailabilityStatus == "2")
                {
                    //CallTransactionsBusinessFacade objBF = new CallTransactionsBusinessFacade();
                    //objBF.UpdateCallTransactions(CallTransactionsDBFields.CallEndTime + "=getDate()," + CallTransactionsDBFields.UpdatedDate + "=getDate()", CallTransactionsDBFields.ID + "=" + tranID);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(_module, "ChangeAvailabilityStatus(UserID:" + UserID + ", strStatus:" + AvailabilityStatus + ",tranID=" + tranID + ")", ex.Source, ex.Message, ex);
            }
            return Json(_jsonMessage);
        }

        [HttpPost]
        public JsonResult MakeAgentActive()
        {
            Int64 UserID = 0;
            JsonMessage _jsonMessage = null;
            try
            {
                if (HttpContext.Session[KeyEnums.SessionKeys.UserId.ToString()] != null)
                    UserID = Convert.ToInt64(HttpContext.Session[KeyEnums.SessionKeys.UserId.ToString()]);

                if (UserID > 0)
                {
                    UsersBusinessFacade objUsers = new UsersBusinessFacade();
                    _jsonMessage = objUsers.ChangeAvailabilityStatus(UserID, "1");
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(_module, "MakeAgentActive(UserID:" + UserID + ")", ex.Source, ex.Message, ex);
            }
            return Json(_jsonMessage);
        }
    }
}