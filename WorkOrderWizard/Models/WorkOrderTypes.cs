using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using WorkOrderWizard.Models;
using System.Data.OleDb;


namespace WorkOrderWizard.Models
{
    public class WorkOrderType
    {
        public string WOTYPE { get; set; }
        public string DESCRIPTION { get; set; }
    }

    public class WorkOrderTypes : List<WorkOrderType>
    {
        public WorkOrderTypes()
            : base()
        {//https://msdn.microsoft.com/en-us/library/dw70f090(v=vs.110).aspx
            MP2_DataBaseSettings objDb = new MP2_DataBaseSettings();
            var strSQL = QueryDefinitions.GetQuery("SelectWOTypes");

            OleDbCommand objOleDbCommand = new OleDbCommand(strSQL, objDb.OleDBConnection);
            using (objDb.OleDBConnection)
            {
                objDb.OleDBConnection.Open();
                OleDbDataReader objOleDbDataReader = objOleDbCommand.ExecuteReader();

                while (objOleDbDataReader.Read())
                {
                    var objWorkOrderType = new WorkOrderType
                    {
                        WOTYPE = objOleDbDataReader["WOTYPE"].ToString(),
                        DESCRIPTION = objOleDbDataReader["DESCRIPTION"].ToString()
                    };
                    this.Add(objWorkOrderType);
                }
                objOleDbDataReader.Close();
            }
            
        }
    }
}