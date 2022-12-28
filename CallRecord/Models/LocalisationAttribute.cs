using AdaniCall.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using AdaniCall.Models;
using AdaniCall.Entity;
using AdaniCall.Utility.Common;

// Based on: http://geekswithblogs.net/shaunxu/archive/2010/05/06/localization-in-asp.net-mvc-ndash-3-days-investigation-1-day.aspx
public class LocalisationAttribute : ActionFilterAttribute
{
    public const string LangParam = "lang";
    public const string CookieName = "UserLanguage";

    // List of allowed languages in this app (to speed up check)
    private const string Cultures = "en-GB en-US de-DE fr-FR es-ES ro-RO ";

    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {

        var culture = "en";
        var name = "";
        var value = "";

        if (HttpContext.Current.Session["AutoLoginURL"] == null)
        {
            string strController = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            string strAction = filterContext.ActionDescriptor.ActionName;
            //if (strAction.ToLower() != "login")
            //{
            var queryParameters = filterContext.ActionDescriptor.GetParameters();
            var paramValues = queryParameters.Select(s => new
            {
                Name = s.ParameterName,
                Value = filterContext.HttpContext.Request[s.ParameterName]
            });

            string strNewUrl = "";
            foreach (var list in paramValues)
            {
                name = list.Name;
                value = list.Value;
                if (strNewUrl == "")
                {
                    strNewUrl = "?" + name + "=" + value;
                }
                else
                {
                    strNewUrl = strNewUrl + "&" + name + "=" + value;
                }
            }
            if (queryParameters == null)
            {
                HttpContext.Current.Session["AutoLoginURL"] = "/" + strController + "/" + strAction;
            }
            else
            {
                HttpContext.Current.Session["AutoLoginURL"] = "/" + strController + "/" + strAction + strNewUrl;

            }
            //}
        }

        if (HttpContext.Current.Session[KeyEnums.SessionKeys.UserSession.ToString()] != null)
        {
            Helper _Helper = new Helper();
            LoginVM _LoginVM = _Helper.GetSession();
            culture = _LoginVM.ProfileLanguage;
        }
        else if (HttpContext.Current.Session[KeyEnums.SessionKeys.UserLanguage.ToString()] != null)
        {
            culture = Convert.ToString(HttpContext.Current.Session[KeyEnums.SessionKeys.UserLanguage.ToString()]);
        }
        else if (HttpContext.Current.Session[KeyEnums.SessionKeys.LandingLanguage.ToString()] != null && AdaniCallConstants.DefaultController.ToLower() == "home")
        {
            culture = Convert.ToString(HttpContext.Current.Session[KeyEnums.SessionKeys.LandingLanguage.ToString()]);
        }
        if (culture != "")
        {
            var language = culture.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries)[0];
            filterContext.RouteData.Values[LangParam] = language;

            try
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(language);
            }
            catch (Exception)
            {
            }
        }
        // HttpContext.Current.Session[KeyEnums.SessionKeys.UserLanguage.ToString()] = LangParam;
        base.OnActionExecuting(filterContext);
    }
}
public class LandingLocalisationAttribute : ActionFilterAttribute
{
    public const string LangParam = "lang";
    public const string CookieName = "UserLanguage";

    // List of allowed languages in this app (to speed up check)
    private const string Cultures = "en-GB en-US de-DE fr-FR es-ES ro-RO ";

    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var culture = "de";

        try
        {
            if (HttpContext.Current.Session[KeyEnums.SessionKeys.LandingLanguage.ToString()] == null)
            {
                var userLanguages = filterContext.HttpContext.Request.UserLanguages;
                CultureInfo ci;
                if (userLanguages.Count() > 0)
                {
                    try
                    {
                        ci = new CultureInfo(userLanguages[0]);
                    }
                    catch (CultureNotFoundException)
                    {
                        ci = CultureInfo.InvariantCulture;
                    }
                }
                else
                {
                    ci = CultureInfo.InvariantCulture;
                }

                if (ci.IetfLanguageTag.ToString().ToLower().Contains("de"))
                {
                    filterContext.HttpContext.Session[AdaniCall.Entity.Enums.KeyEnums.SessionKeys.LandingLanguage.ToString()] = "DE";
                }
                else
                {
                    filterContext.HttpContext.Session[AdaniCall.Entity.Enums.KeyEnums.SessionKeys.LandingLanguage.ToString()] = "EN";
                }
            }
        }
        catch (Exception)
        {
        }
        if (HttpContext.Current.Session[KeyEnums.SessionKeys.LandingLanguage.ToString()] != null)
        {
            Helper _Helper = new Helper();
            culture = Convert.ToString(HttpContext.Current.Session[KeyEnums.SessionKeys.LandingLanguage.ToString()]);
        }
        var language = culture.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries)[0];
        filterContext.RouteData.Values[LangParam] = language;
        try
        {

            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(language);
        }
        catch (Exception)
        {
        }
        base.OnActionExecuting(filterContext);
    }
}