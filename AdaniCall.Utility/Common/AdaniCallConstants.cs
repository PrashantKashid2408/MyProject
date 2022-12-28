using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace AdaniCall.Utility.Common
{
    public class AdaniCallConstants
    {
        public static string GetAppSetting(string appSettingKey)
        {
            return ConfigurationManager.AppSettings.AllKeys.Contains(appSettingKey) ? ConfigurationManager.AppSettings.Get(appSettingKey).ToString() : string.Empty;
        }

        public static readonly string OnAuthorizationController = GetAppSetting("OnAuthorizationController");
        public static readonly string OnAuthorizationAction = GetAppSetting("OnAuthorizationAction");

        public static readonly string RootPath = GetAppSetting("RootPath");
        public static readonly string PhyPath = GetAppSetting("PhyPath");
        public static readonly string AdaniCallQRDomain = GetAppSetting("AdaniCallQRDomain");

        public static readonly string LOG_FOLDER_PATH = GetAppSetting("LOG_FOLDER_PATH");
        public static readonly string LOG_EMAIL_SENDER = GetAppSetting("LOG_EMAIL_SENDER");
        public static readonly string LOG_EMAIL_RECEIVER = GetAppSetting("LOG_EMAIL_RECEIVER");
        public static readonly string LOG_EMAIL_SUBJECT = GetAppSetting("LOG_EMAIL_SUBJECT");
        public static readonly string LOG_EMAIL_IS_SEND = GetAppSetting("LOG_EMAIL_IS_SEND");
        public static readonly string LOG_EMAIL_CC = GetAppSetting("LOG_EMAIL_CC");
        public static readonly string LOG_EMAIL_BCC = GetAppSetting("LOG_EMAIL_BCC");

        public static readonly string shortURL = GetAppSetting("shortURL");
        public static readonly string Cachedate = GetAppSetting("Cachedate");

        public static readonly string DefaultmailID = GetAppSetting("DefaultmailID");
        public static readonly string Account_Confirmation = GetAppSetting("Account_Confirmation");
        public static readonly string ReplyToEmail = GetAppSetting("ReplyToEmail");

        public static readonly string DefaultController = GetAppSetting("DefaultController");
        public static readonly string DefaultView = GetAppSetting("DefaultView");

        public static readonly string LoginCookie = GetAppSetting("LoginCookie");

        public static readonly string CareEmail = GetAppSetting("CareEmail");

        public static readonly string AESUserEncrryptKey = GetAppSetting("AESUserEncrryptKey");
        public static readonly string AESUserVector = GetAppSetting("AESUserVector");
        public static readonly string AESUserSalt = GetAppSetting("AESUserSalt");

        public static readonly string MSG91Key = GetAppSetting("MSG91Key");
        public static readonly string MSG91SenderId = GetAppSetting("MSG91SenderId");
        public static readonly string MSG91Route = GetAppSetting("MSG91Route");
        public static readonly string MSG91APIUrl = GetAppSetting("MSG91APIUrl");

        public static readonly string AdaniCallRequestToken = GetAppSetting("AdaniCallRequestToken");
        public static readonly string AdaniCallUserRequest = GetAppSetting("AdaniCallUserRequest");
        public static readonly string AdaniCallRequestByID = GetAppSetting("AdaniCallRequestByID");

        public static readonly string GeocodingAPIURL = GetAppSetting("GeocodingAPIURL");
        public static readonly string GeocodingAPIKey = GetAppSetting("GeocodingAPIKey");

        public static readonly string AdaniCallQRDomainSiteRUL = URLDetails.GetSiteRootUrl().TrimEnd('/');

        public static readonly string ErrorLogEmailSubject = GetAppSetting("ErrorLogEmailSubject");

        public static readonly string TriggerTemperatureDegree = GetAppSetting("TriggerTemperatureDegree");
        public static readonly string TriggerTemperatureFarenheit = GetAppSetting("TriggerTemperatureFarenheit");
        public static readonly string TriggerSPO2 = GetAppSetting("TriggerSPO2");
        public static readonly string TriggerStatus = GetAppSetting("TriggerStatus");
        public static readonly string TriggerMobileNo = GetAppSetting("TriggerMobileNo");
        public static readonly int SQLCommandTimeOut = GetAppSetting("SQLCommandTimeOut") != "" ? Convert.ToInt32(GetAppSetting("SQLCommandTimeOut")) : 0;

        //Paths
        public static readonly string UploadFetchVir = @"/ALLContent/User/[UserID]/UploadFetch/";
        public static readonly string UploadFetchPhy = @"\ALLContent\User\[UserID]\UploadFetch\";
        public static readonly string UploadFetchTemplateVir = @"/Templates/Excel/UploadFetch.xls";
        public static readonly string UploadFetchTemplatePhy = @"\Templates\Excel\UploadFetch.xls";

        public static readonly string UploadFetchTempVir = @"/ALLContent/Temp/";
        public static readonly string UploadFetchTempPhy = @"\ALLContent\Temp\";

        public static readonly string ContentNoLogoPathVir = "/Content/images/no-logo.jpg";
    }
}
