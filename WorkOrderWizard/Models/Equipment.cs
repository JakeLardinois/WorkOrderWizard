using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.OleDb;


namespace WorkOrderWizard.Models
{
    public class Equipment
    {
        public string EQNUM { get; set; }
        public string EQTYPE { get; set; }
        public string DESCRIPTION { get; set; }
        public string LOCATION { get; set; }
        public string SUBLOCATION1 { get; set; }
        public string SUBLOCATION2 { get; set; }
        public string SUBLOCATION3 { get; set; }
        public string DEPT { get; set; }

        public string DescriptionDisplay { get { return EQNUM + ": " + DESCRIPTION; } }
    }

    public class EquipmentList : List<Equipment>
    {
        public EquipmentList()
            : base()
        {
            MP2_DataBaseSettings objDb = new MP2_DataBaseSettings();
            var strSQL = QueryDefinitions.GetQuery("SelectInServiceEquipment");

            OleDbCommand objOleDbCommand = new OleDbCommand(strSQL, objDb.OleDBConnection);
            using (objDb.OleDBConnection)
            {
                objDb.OleDBConnection.Open();
                OleDbDataReader objOleDbDataReader = objOleDbCommand.ExecuteReader();

                while (objOleDbDataReader.Read())
                {
                    var objEquipment = new Equipment
                    {
                        EQNUM = objOleDbDataReader["EQNUM"].ToString(),
                        EQTYPE = objOleDbDataReader["EQTYPE"].ToString(),
                        DESCRIPTION = objOleDbDataReader["DESCRIPTION"].ToString(),
                        LOCATION = objOleDbDataReader["LOCATION"].ToString(),
                        SUBLOCATION1 = objOleDbDataReader["SUBLOCATION1"].ToString(),
                        SUBLOCATION2 = objOleDbDataReader["SUBLOCATION2"].ToString(),
                        SUBLOCATION3 = objOleDbDataReader["SUBLOCATION3"].ToString(),
                        DEPT = objOleDbDataReader["DEPT"].ToString()
                    };
                    this.Add(objEquipment);
                }
                objOleDbDataReader.Close();
            }
            
        }
    }
}