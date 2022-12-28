using System;
using System.Web; 
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Diagnostics;
using System.Configuration;
namespace AdaniCall.Business.DataAccess.DataAccessLayer
{
    public static class dbClass
    {
        public static long SqlConnectionCounter = 0;
        public static long OleDbConnectionCounter = 0;
        public static string ConnectString()
        {
            string sConnectString = "";
            sConnectString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
            return sConnectString;
        }

        public static string SqlConnectString()
        {
            string sConnectString = "";
            sConnectString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
            return sConnectString;
        }

        public static SqlConnection GetConnection()
        {
            SqlConnection dbConnection = new SqlConnection();
            dbConnection.ConnectionString = dbClass.ConnectString();
            dbConnection.Open();
            SqlConnectionCounter++;
            Debug.Print("Sql Connection Number - " + SqlConnectionCounter);
            return dbConnection;
        }

        public static SqlConnection GetConnectionSqlConnection()
        {
            SqlConnection dbConnection = new SqlConnection();

            dbConnection.ConnectionString = dbClass.SqlConnectString();
            dbConnection.Open();
            SqlConnectionCounter++;
            Debug.Print("Sql Connection Number - " + SqlConnectionCounter);
            return dbConnection;
        }

        public static SqlConnection GetSqlConnection()
        {
            SqlConnection dbConnection = new SqlConnection();

            dbConnection.ConnectionString = dbClass.SqlConnectString();
            dbConnection.Open();
            SqlConnectionCounter++;
            Debug.Print("Sql Connection Number - " + SqlConnectionCounter);
            return dbConnection;
        }



        public static void GetSqlConnection(ref SqlConnection dbConnection)
        {
            dbConnection = new SqlConnection();

            dbConnection.ConnectionString = dbClass.SqlConnectString();
            dbConnection.Open();
            SqlConnectionCounter++;
            Debug.Print("Sql Connection Number - " + SqlConnectionCounter);
        }

        public static void CloseSqlConnection(ref SqlConnection dbConnection)
        {
            if (dbConnection != null)
            {
                if (dbConnection.State == ConnectionState.Open)
                {
                    dbConnection.Close();
                }
                dbConnection.Dispose();
                dbConnection = null;
                System.GC.Collect();
                SqlConnectionCounter--;
            }
        }
        public static string OleDbConnectString()
        {
            string sConnectString = "";
            sConnectString = ConfigurationManager.ConnectionStrings["FlipBook"].ConnectionString;
            return sConnectString;
        }

        public static OleDbConnection GetOledbConnection()
        {
            OleDbConnection dbConnection = new OleDbConnection();
            dbConnection.ConnectionString = dbClass.ConnectString();
            dbConnection.Open();
            OleDbConnectionCounter++;
            Debug.Print("OLEDB Connection Number - " + OleDbConnectionCounter);
            return dbConnection;
        }

        public static OleDbConnection GetOleDbConnection()
        {
            OleDbConnection dbConnection = new OleDbConnection();

            dbConnection.ConnectionString = dbClass.OleDbConnectString();
            dbConnection.Open();
            OleDbConnectionCounter++;
            Debug.Print("OLEDB Connection Number - " + OleDbConnectionCounter);
            return dbConnection;
        }

        public static void GetOleDbConnection(ref OleDbConnection dbConnection)
        {
            dbConnection = new OleDbConnection();

            dbConnection.ConnectionString = dbClass.OleDbConnectString();
            dbConnection.Open();
            OleDbConnectionCounter++;
            Debug.Print("OLEDB Connection Number - " + OleDbConnectionCounter);
        }

        public static void CloseOleDbConnection(ref OleDbConnection dbConnection)
        {
            if (dbConnection != null)
            {
                if (dbConnection.State == ConnectionState.Open)
                {
                    dbConnection.Close();
                }
                dbConnection.Dispose();
                dbConnection = null;
                System.GC.Collect();
                OleDbConnectionCounter--;
            }
        }
    }
}