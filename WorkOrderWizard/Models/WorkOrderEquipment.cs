using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.OleDb;


namespace WorkOrderWizard.Models
{
    public class WorkOrderEquipment
    {
        public WorkOrderEquipment()
        {
            CLOSEDATE = SharedVariables.MINDATE;
        }

        public string WONUM { get; set; }
        public DateTime CLOSEDATE { get; set; }
        public string EQNUM { get; set; }
        public string LOCATION { get; set; }
        public string SUBLOCATION1 { get; set; }
        public string SUBLOCATION2 { get; set; }
        public string SUBLOCATION3 { get; set; }
        public string DEPARTMENT { get; set; }
        public string EQDESC { get; set; }

        public QueryStatus Insert()
        {
            int intRecordsAffected = 0;
            MP2_DataBaseSettings objDb;
            

            try
            {
                objDb = new MP2_DataBaseSettings();
                var strSQL = QueryDefinitions.GetQuery("InsertIntoWOEQLIST", new string[] { WONUM, CLOSEDATE.ToString("d"), EQNUM,
                LOCATION, SUBLOCATION1, SUBLOCATION2, SUBLOCATION3, DEPARTMENT, EQDESC.EscapeSingleQuotes()});
                OleDbCommand objOleDbCommand = new OleDbCommand(strSQL, objDb.OleDBConnection);

                using (objDb.OleDBConnection)
                {
                    objDb.OleDBConnection.Open();

                    intRecordsAffected = objOleDbCommand.ExecuteNonQuery();

                    objDb.OleDBConnection.Close();
                }
            }
            catch (Exception objEx)
            {
                return new QueryStatus { RecordsAffected = intRecordsAffected, Message = objEx.Message };
            }

            return new QueryStatus { RecordsAffected = intRecordsAffected };
        }
    }
}