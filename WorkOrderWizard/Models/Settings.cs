using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Text;
using System.Data.OleDb;


namespace WorkOrderWizard.Models
{
    public class MP2_DataBaseSettings
    {

        public MP2_DataBaseSettings()
        {
            //string strSQLConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["mp250db"].ConnectionString;
            OleDBConnection = new OleDbConnection(ConnectionString);
        }

        //private static string mstrSQLConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["mp250db"].ConnectionString;
        //public static string ConnectionString { get { return mstrSQLConnectionString.Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory").ToString()); } }
        public static string ConnectionString { get { return System.Configuration.ConfigurationManager.ConnectionStrings["mp250db"].ConnectionString; } }
        public OleDbConnection OleDBConnection { get; set; }
    }

    public class MP2WriteDb_DataBaseSettings
    {

        public MP2WriteDb_DataBaseSettings()
        {
            //string strSQLConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["mp250db"].ConnectionString;
            OleDBConnection = new OleDbConnection(ConnectionString);
        }

        //private static string mstrSQLConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["mp250db"].ConnectionString;
        //public static string ConnectionString { get { return mstrSQLConnectionString.Replace("|DataDirectory|", AppDomain.CurrentDomain.GetData("DataDirectory").ToString()); } }
        public static string ConnectionString { get { return System.Configuration.ConfigurationManager.ConnectionStrings["mp250dbWrite"].ConnectionString; } }
        public OleDbConnection OleDBConnection { get; set; }
    }

    public class QueryDefinitions
    {
        static System.Resources.ResourceManager objResourceManager = new System.Resources.ResourceManager("WorkOrderWizard.Models.QueryDefs", System.Reflection.Assembly.GetExecutingAssembly());
        private StringBuilder strSQL = new StringBuilder();


        public string GetQuery(string strQueryName)
        {
            return objResourceManager.GetString(strQueryName);
        }

        public string GetQuery(string strQueryName, string[] strParams)
        {
            strSQL.Clear();
            strSQL.Append(objResourceManager.GetString(strQueryName));
            //strSQL.Append(QueryDefs.DeleteOldestItemUnitWeightHistory);

            //for (int intCounter = 0; intCounter < strParams.Length; intCounter++)
            for (int intCounter = strParams.Length - 1; intCounter > -1; intCounter--)
            {
                string strTemp = "~p" + intCounter;
                strSQL.Replace(strTemp, strParams[intCounter]);
            }

            return strSQL.ToString();
        }
    }

    public static class SharedVariables
    {
        public static DateTime MINDATE = new DateTime(1900, 1, 1);
        public static DateTime MAXDATE = new DateTime(2999, 1, 1);
    }

    public static class Settings
    {
        public static string WorkOrderPrefix { get { return System.Configuration.ConfigurationManager.AppSettings["WorkOrderPrefix"]; } }
        public static string ReportDirectory { get { return System.Configuration.ConfigurationManager.AppSettings["ReportDirectory"]; } }
    }
}