using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Configuration;
using AdaniCall.Models;
using AdaniCall.Entity.Enums;
using Azure.Core;
using AdaniCall.Utility.Common;
using AdaniCall.Entity;
using AdaniCall.Business.BusinessFacade;
using AdaniCall.Business.DataAccess.Constants;

namespace AdaniCall.Controllers
{
    [Localisation] //<<< ADD THIS TO ALL CONTROLLERS (OR A BASE CONTROLLER)
    //[CustomAuthorizeAttribute(false, Roles = RoleEnums.Agent + "," + RoleEnums.Kiosk)]
    public class HomeController : Controller
    {
        TokenHelper objTokenHelper = new TokenHelper();
        private readonly string _module = "AdaniCall.Controllers.HomeController";

        [CustomAuthorizeAttribute(false, Roles = RoleEnums.Kiosk)]
        public ActionResult Call()
        {
            string _cacheKey = "CallToken_" + Session.SessionID;
            HttpContext.Cache.Remove(_cacheKey);
            Int64 UserID = 0;
            string CallerID = "";
            string KioskID = "";
            if (HttpContext.Session[KeyEnums.SessionKeys.UserId.ToString()] != null)
            {
                UserID = Convert.ToInt64(HttpContext.Session[KeyEnums.SessionKeys.UserId.ToString()]);
                if (HttpContext.Session[KeyEnums.SessionKeys.CallerID.ToString()] != null)
                    CallerID = HttpContext.Session[KeyEnums.SessionKeys.CallerID.ToString()].ToString();
                if (HttpContext.Session[KeyEnums.SessionKeys.KioskID.ToString()] != null)
                    KioskID = HttpContext.Session[KeyEnums.SessionKeys.KioskID.ToString()].ToString();
            }

            try
            {
                ViewBag.Title = "Traveller";

                CommonData objCD = new CommonData();
                AccessToken objAT = new AccessToken();
                
                if (!string.IsNullOrWhiteSpace(CallerID))
                {
                    if (HttpContext.Cache[_cacheKey] != null)
                    {
                        objAT = objCD.GetFromCache(_cacheKey);
                        DateTime objDTNow = DateTime.Now;
                        if (objDTNow > objAT.ExpiresOn.DateTime)
                        {
                            objAT = objTokenHelper.RefreshTokenAsync(CallerID);
                            objCD.AddToCache(_cacheKey, objAT, objAT.ExpiresOn.DateTime);
                        }
                    }
                    else
                    {
                        objAT = objTokenHelper.RefreshTokenAsync(CallerID);
                        objCD.AddToCache(_cacheKey, objAT, objAT.ExpiresOn.DateTime);
                    }
                }
                else
                    new UserController().Logout();

                ViewBag.CallToken = objAT.Token;
            }
            catch (Exception ex)
            {
                Log.WriteLog(_module, "Accept(UserId=" + UserID + ")", ex.Source, ex.Message);
            }
            
            return View();
        }

        [CustomAuthorizeAttribute(false, Roles = RoleEnums.Agent)]
        public ActionResult Accept()
        {
            string _cacheKey = "AcceptToken_" + Session.SessionID;
            HttpContext.Cache.Remove(_cacheKey);
            Int64 UserID = 0;
            string CallerID = "";
            if (HttpContext.Session[KeyEnums.SessionKeys.UserId.ToString()] != null) 
            {
                UserID = Convert.ToInt64(HttpContext.Session[KeyEnums.SessionKeys.UserId.ToString()]);
                if (HttpContext.Session[KeyEnums.SessionKeys.CallerID.ToString()] != null)
                    CallerID = HttpContext.Session[KeyEnums.SessionKeys.CallerID.ToString()].ToString();
                UsersBusinessFacade objUsers = new UsersBusinessFacade();
                objUsers.ChangeAvailabilityStatus(UserID, "1");
            }

            try
            {
                ViewBag.Title = "Help Desk";

                CommonData objCD = new CommonData();
                AccessToken objAT = new AccessToken();

                if (!string.IsNullOrWhiteSpace(CallerID))
                {
                    if (HttpContext.Cache[_cacheKey] != null)
                    {
                        objAT = objCD.GetFromCache(_cacheKey);
                        DateTime objDTNow = DateTime.Now;
                        if (objDTNow > objAT.ExpiresOn.DateTime)
                        {
                            objAT = objTokenHelper.RefreshTokenAsync(CallerID);
                            objCD.AddToCache(_cacheKey, objAT, objAT.ExpiresOn.DateTime);
                        }
                    }
                    else
                    {
                        objAT = objTokenHelper.RefreshTokenAsync(CallerID);
                        objCD.AddToCache(_cacheKey, objAT, objAT.ExpiresOn.DateTime);
                    }
                }
                else
                    new UserController().Logout();

                ViewBag.AcceptToken = objAT.Token;
                ViewBag.AgentCallerID = CallerID;
            }
            catch (Exception ex)
            {
                Log.WriteLog(_module, "Accept(UserId=" + UserID + ")", ex.Source, ex.Message);
            }
            
            return View();
        }

        [HttpPost]
        public JsonResult MakeCallTransaction(string UniqueCallID, string IncomingCallID)
        {
            Int64 UserID = 0;
            CallTransactions objCT = new CallTransactions();
            try
            {
                CallTransactionsBusinessFacade objBF = new CallTransactionsBusinessFacade();
                if (HttpContext.Session[KeyEnums.SessionKeys.UserId.ToString()] != null)
                {
                    UserID = Convert.ToInt64(HttpContext.Session[KeyEnums.SessionKeys.UserId.ToString()]);
                    objCT.AgentUserID = UserID;
                    objCT.TravellerCallID = IncomingCallID;
                    objCT.UniqueCallID = UniqueCallID;
                    objBF.Save(objCT);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(_module, "MakeCallTransaction(UniqueCallID:" + UniqueCallID + ",IncomingCallID:" + IncomingCallID + ")", ex.Source, ex.Message);
            }
            return Json("");
        }

        [HttpPost]
        public JsonResult UpdateCallTransactionsEndTime(string UniqueCallID, string CallLanguage = "3")
        {
            CallTransactions objCT = new CallTransactions();
            try
            {
                CallTransactionsBusinessFacade objBF = new CallTransactionsBusinessFacade();
                if (HttpContext.Session[KeyEnums.SessionKeys.UserId.ToString()] != null)
                {
                    int callLanguage;
                    int.TryParse(CallLanguage, out callLanguage);
                    objBF.UpdateCallTransactions(CallTransactionsDBFields.CallEndTime + "=CONVERT(VARCHAR, GETDATE(), 120)," + CallTransactionsDBFields.LanguageId + "=" + callLanguage, CallTransactionsDBFields.UniqueCallID + "='" + UniqueCallID + "'");
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(_module, "UpdateCallTransactionsEndTime(UniqueCallID:" + UniqueCallID + ",CallLanguage:" + CallLanguage + ")", ex.Source, ex.Message);
            }
            return Json("");
        }

        [HttpPost]
        public JsonResult InsertAM(string UniqueCallID, string CallStatus)
        {
            Int64 UserID = 0;
            string _role = "";
            Users objUsers = new Users();
            AccessMember objAM = new AccessMember();
            Helper _helper = new Helper();
            objAM = _helper.GetAccessMember();

            try
            {
                if (HttpContext.Session[KeyEnums.SessionKeys.UserId.ToString()] != null) {
                    UserID = Convert.ToInt64(HttpContext.Session[KeyEnums.SessionKeys.UserId.ToString()]);
                    if (HttpContext.Session[KeyEnums.SessionKeys.UserRole.ToString()] != null)
                    {
                        _role = HttpContext.Session[KeyEnums.SessionKeys.UserRole.ToString()].ToString();
                        objAM.UniqueCallID = UniqueCallID;
                        if (_role == Convert.ToString((byte)RoleEnums.Role.Agent))
                        {
                            objAM.CallerID = "";
                        }
                        else if (_role == Convert.ToString((byte)RoleEnums.Role.Kiosk))
                        {
                            objAM.CallerID = "";
                            if (HttpContext.Session[KeyEnums.SessionKeys.KioskID.ToString()] != null && HttpContext.Session[KeyEnums.SessionKeys.KioskID.ToString()].ToString() != "")
                                objAM.KioskID = Convert.ToInt64(HttpContext.Session[KeyEnums.SessionKeys.KioskID.ToString()]);
                        }
                    }
                }

                if (UserID > 0)
                {
                    objUsers.ID = UserID;
                    if (CallStatus.ToLower() == "connected")
                    {
                        string AMID = InsertAccessMember(objUsers, objAM, "Call");
                        return Json(AMID);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(_module, "InsertAM(UniqueCallID:" + UniqueCallID + ",UserID:" + UserID + ",CallStatus=" + CallStatus + ")", ex.Source, ex.Message);
            }
            return Json("");
        }

        [HttpPost]
        public JsonResult InsertAMPing(string pingFrom = "")
        {
            Int64 UserID = 0;
            string _role = "";
            Users objUsers = new Users();
            AccessMember objAM = new AccessMember();
            Helper _helper = new Helper();
            objAM = _helper.GetAccessMember();

            try
            {
                if (HttpContext.Session[KeyEnums.SessionKeys.UserId.ToString()] != null)
                {
                    UserID = Convert.ToInt64(HttpContext.Session[KeyEnums.SessionKeys.UserId.ToString()]);
                    if (HttpContext.Session[KeyEnums.SessionKeys.UserRole.ToString()] != null)
                    {
                        _role = HttpContext.Session[KeyEnums.SessionKeys.UserRole.ToString()].ToString();
                        objAM.UniqueCallID = "";
                        if (_role == Convert.ToString((byte)RoleEnums.Role.Agent))
                        {
                            objAM.CallerID = "";
                        }
                        else if (_role == Convert.ToString((byte)RoleEnums.Role.Kiosk))
                        {
                            objAM.CallerID = "";
                            if (HttpContext.Session[KeyEnums.SessionKeys.KioskID.ToString()] != null && HttpContext.Session[KeyEnums.SessionKeys.KioskID.ToString()].ToString() != "")
                                objAM.KioskID = Convert.ToInt64(HttpContext.Session[KeyEnums.SessionKeys.KioskID.ToString()]);
                        }
                    }
                }
                string strPing = "Ping";
                if (!string.IsNullOrWhiteSpace(pingFrom))
                    strPing = pingFrom;
                if (UserID > 0 && (_role == Convert.ToString((byte)RoleEnums.Role.Kiosk) || _role == Convert.ToString((byte)RoleEnums.Role.Agent)))
                {
                    objUsers.ID = UserID;
                    string AMID = InsertAccessMember(objUsers, objAM, strPing);
                    return Json(AMID);
                }
                else
                {
                    string AMID = InsertAccessMember(objUsers, objAM, strPing);
                    return Json(AMID);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(_module, "InsertAMPing(pingFrom:" + pingFrom + ")", ex.Source, ex.Message);
            }
            return Json("");
        }

        private string InsertAccessMember(Users objUsers, AccessMember objAccessMember, string ClickedBy)
        {
            try
            {
                objAccessMember.UserID = objUsers.ID;
                objAccessMember.ClickedBy = ClickedBy;
                AccessMemberBusinessFacade objAccessMemberBusinessFacade = new AccessMemberBusinessFacade();
                var AMID = objAccessMemberBusinessFacade.Save(objAccessMember);
                return AMID.ToString();
            }
            catch (Exception ex)
            {
                Log.WriteLog(_module, "InsertAccessMember(UserID: " + objUsers.ID + ", clickedby:" + ClickedBy + ")", ex.Source, ex.Message);
            }
            return "0";
        }
    }
}
