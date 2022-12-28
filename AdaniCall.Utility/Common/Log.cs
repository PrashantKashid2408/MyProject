using System;
using System.Data;
using System.Configuration;
using System.IO;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;
using System.Web;
using DataSecurity;
using System.ComponentModel;
using System.Globalization;

namespace AdaniCall.Utility.Common
{
    public static class Log
    {
        //static string Errorpath = System.AppDomain.CurrentDomain.BaseDirectory + @"\Error\";
        static string Errorpath;

        static Log()
        {
            Errorpath = AdaniCallConstants.PhyPath + @"\Error\";
            DirectoryInfo Dir = new DirectoryInfo(Path.GetDirectoryName(Errorpath));
            if (!Dir.Exists)
            {
                Dir.Create();
            }
        }

        public static void WriteLog(string fromModule, string fromMethod, string errSource, string errMessage, Exception pex = null)
        {
            var line = 0;
            try
            {
                string sDate = DateTime.Now.Date.ToString("yyyyMMMdd");
                FileStream logFile = new FileStream(Errorpath + "[" + sDate + "].log", FileMode.Append);
                StreamWriter streamWriter = new StreamWriter(logFile);

                // Write to the file using StreamWriter class 
                streamWriter.BaseStream.Seek(0, SeekOrigin.End);
                streamWriter.Write("LogTime : ");
                streamWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), "");
                streamWriter.WriteLine("Module Name : " + fromModule + " (" + fromMethod + ")\n");
                streamWriter.WriteLine("Error Source : " + errSource + "\n ");
                streamWriter.WriteLine("Error Message : " + errMessage + "\n ");

                if (pex != null)
                {
                    StackTrace st = new StackTrace(pex, true);
                    // Get the top stack frame
                    if (st.FrameCount > 0)
                    {

                        var frame = st.GetFrame(0);
                        streamWriter.WriteLine("Error Message GetMethod FullName : " + frame.GetMethod().Name.ToString() + "\n ");
                        streamWriter.WriteLine("Error Message GetFileName : " + Path.GetFileName(frame.GetFileName()) + "\n ");
                        streamWriter.WriteLine("Error Message GetFileLineNumber : " + frame.GetFileLineNumber() + "\n ");
                        // Get the line number from the stack frame
                        line = frame.GetFileLineNumber();
                    }

                }

                streamWriter.WriteLine(" \n ");
                streamWriter.WriteLine("======================================== \n ");
                streamWriter.Flush();
                streamWriter.Close();
                logFile.Close();
                logMail("[" + sDate + "].log");
            }
            catch (IOException exio)
            {
                Debug.Print(exio.ToString());
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
            }
            finally
            {

            }

            
        }
        public static void WriteLogWithoutMail(string fromModule, string fromMethod, string errSource, string errMessage, Exception pex = null)
        {
            try
            {
                string sDate = DateTime.Now.Date.ToString("yyyyMMMdd");
                // string Errorpath = System.AppDomain.CurrentDomain.BaseDirectory + @"\Error\";
                FileStream logFile = new FileStream(Errorpath + "[" + sDate + "]_Console.log", FileMode.Append);
                StreamWriter streamWriter = new StreamWriter(logFile);

                // Write to the file using StreamWriter class 
                streamWriter.BaseStream.Seek(0, SeekOrigin.End);
                streamWriter.Write("LogTime : ");
                streamWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), "");
                streamWriter.WriteLine("Module Name : " + fromModule + " (" + fromMethod + ")\n");
                streamWriter.WriteLine("Error Source : " + errSource + "\n ");
                streamWriter.WriteLine("Error Message : " + errMessage + "\n ");
                streamWriter.WriteLine(" \n ");
                streamWriter.WriteLine("======================================== \n ");
                streamWriter.Flush();
                streamWriter.Close();
                logFile.Close();
                //logMail("[" + sDate + "]_NoMail.log");
            }
            catch (IOException exio)
            {
                Debug.Print(exio.ToString());
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
            }
            finally
            {

            }

        }

        public static void logMail(string logFile)
        {

            string Sender_Email = ConfigurationManager.AppSettings.Get("DefaultmailID").ToString();
            string Receiver_Email = ConfigurationManager.AppSettings.Get("LOG_EMAIL_RECEIVER").ToString();

            string logPath = Errorpath + logFile + "";
            try
            {
                MailMessage objEmail = new MailMessage(Sender_Email, Receiver_Email); 
                //string FileUrl = ConfigurationManager.AppSettings.Get("Book2LookAdminDomain").ToString() + @"/Bug/DownLoadFile?Id=" + HttpUtility.UrlEncode(EncryptDecrypt.EncryptNew(logFile));
                string FileUrl = AdaniCallConstants.AdaniCallQRDomain + @"/Bug/DownLoadFile?Id=" + HttpUtility.UrlEncode(EncryptDecrypt.EncryptNew(logFile));
                objEmail.IsBodyHtml = true;
                objEmail.Subject = ConfigurationManager.AppSettings.Get("LOG_EMAIL_SUBJECT").ToString();
                objEmail.Priority = MailPriority.Normal;
                string Body = "<a href=\"" + FileUrl + "\" target=\"_blank\">Click here</a> for open error file"; 
                objEmail.Body = Body;
                SmtpClient smail = new SmtpClient();
                smail.Send(objEmail);
                objEmail.Dispose(); 
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static void WriteInfoLogWithoutMail(string fromModule, string fromMethod, string errSource, string errMessage)
        {
            try
            {
                string sDate = DateTime.Now.Date.ToString("yyyyMMMdd");
                // string Errorpath = System.AppDomain.CurrentDomain.BaseDirectory + @"\Error\";
                FileStream logFile = new FileStream(Errorpath + "[" + sDate + "].Infolog", FileMode.Append);
                StreamWriter streamWriter = new StreamWriter(logFile);

                // Write to the file using StreamWriter class 
                streamWriter.BaseStream.Seek(0, SeekOrigin.End);
                streamWriter.Write("LogTime : ");
                streamWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), "");
                streamWriter.WriteLine("Module Name : " + fromModule + " (" + fromMethod + ")\n");
                streamWriter.WriteLine("Error Source : " + errSource + "\n ");
                streamWriter.WriteLine("Error Message : " + errMessage + "\n ");
                streamWriter.WriteLine(" \n ");
                streamWriter.WriteLine("======================================== \n ");
                streamWriter.Flush();
                streamWriter.Close();
                logFile.Close();
            }
            catch (IOException exio)
            {
                Debug.Print(exio.ToString());
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
            }
            finally
            {

            }

        }

    }
    public static class DataRecordExtensions
    {
        public static bool HasColumn(this IDataRecord dr, string columnName)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (dr.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
