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
            string strSQLConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MP2_DataBaseConnectionString"].ConnectionString;
            OleDBConnection = new OleDbConnection(strSQLConnectionString);
        }

        public OleDbConnection OleDBConnection { get; set; }
    }

    public static class QueryDefinitions
    {
        static System.Resources.ResourceManager objResourceManager = new System.Resources.ResourceManager("WorkOrderWizard.Models.QueryDefs", System.Reflection.Assembly.GetExecutingAssembly());
        static StringBuilder strSQL;


        public static string GetQuery(string strQueryName)
        {
            strSQL = new StringBuilder();
            strSQL.Append(objResourceManager.GetString(strQueryName));

            return strSQL.ToString();
        }

        public static string GetQuery(string strQueryName, string[] strParams)
        {
            strSQL = new StringBuilder();
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