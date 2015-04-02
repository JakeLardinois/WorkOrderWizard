using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.IO;
using System.Data.OleDb;
using System.Text;


namespace WorkOrderWizard.Models
{
    public class WorkOrder
    {
        public WorkOrder()
        {
            CLOSEDATE = SharedVariables.MINDATE;
            REQUESTTIME = new DateTime(SharedVariables.MINDATE.Year, SharedVariables.MINDATE.Month, SharedVariables.MINDATE.Day, 
                DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);
            REQUESTDATE = DateTime.Now.Date;
        }

        public string WONUM { get; set; }
        public DateTime CLOSEDATE { get; set; }
        public string TASKDESC { get; set; }
        public string WOTYPE { get; set; }
        public string ORIGINATOR { get; set; }
        public int PRIORITY { get; set; }
        public DateTime REQUESTTIME { get; set; }
        public DateTime REQUESTDATE { get; set; }
        public string STATUS { get; set; }

        public List<WorkOrderEquipment> WOEQLIST { get; set; }

        //POST variables...
        public string[] WOPriority { get; set; }
        public string[] WOType { get; set; }
        public string[] WOEquipment { get; set; }
        public string EmployeeInfo { get; set; }
        
        public string GetNextWorkOrderNum() 
        {
            var strWorkOrderPrefix = Settings.WorkOrderPrefix;
            MP2_DataBaseSettings objDb = new MP2_DataBaseSettings();
            var strSQL = QueryDefinitions.GetQuery("SelectCurrentWONum", new string[] { strWorkOrderPrefix });
            int intTemp, intWorkOrderNum;
            string strNewWO;

            using (objDb.OleDBConnection)
            {
                OleDbCommand objOleDbCommand = new OleDbCommand(strSQL, objDb.OleDBConnection);
                objDb.OleDBConnection.Open();
                OleDbDataReader objOleDbDataReader = objOleDbCommand.ExecuteReader();

                if (objOleDbDataReader.Read()) //in case there are no records with the given prefix...
                {
                    var strCurrentWO = objOleDbDataReader["WONUM"].ToString();
                    var strCurrentWONum = strCurrentWO.Remove(0, strWorkOrderPrefix.Length);
                    intWorkOrderNum = (int.TryParse(strCurrentWONum, out intTemp) ? intTemp : 0);
                    intWorkOrderNum++;
                }
                else
                    intWorkOrderNum = 0;

                strNewWO = (strWorkOrderPrefix + intWorkOrderNum.ToString().PadLeft(10 - strWorkOrderPrefix.Length, '0'));

                objDb.OleDBConnection.Close();
            }

            return strNewWO.Substring(strNewWO.Length - 10); //MP2 WONUM field can only be 10 characters long...
        }

        public void PopulateFromPostVariables()
        {
            int intTemp;

            PRIORITY = int.TryParse(WOPriority[0], out intTemp) ? intTemp : 5; //5 is the lowest priority...
            ORIGINATOR = "A" + EmployeeInfo.Split(':')[0]; //MP2 'Short Text' fields only allow 25 characters...
            WOTYPE = WOType[0];

            var objEquipmentList = new EquipmentList();
            WOEQLIST = new List<WorkOrderEquipment>();
            if (WOEquipment != null)
                foreach (var strEquipment in WOEquipment)
                {
                    var TempEquip = objEquipmentList
                        .Where(e => e.EQNUM.Equals(strEquipment))
                        .Select(e => new WorkOrderEquipment
                        {
                            WONUM = this.WONUM,
                            EQNUM = e.EQNUM,
                            EQDESC = e.DESCRIPTION,
                            DEPARTMENT = e.DEPT,
                            LOCATION = e.LOCATION,
                            SUBLOCATION1 = e.SUBLOCATION1,
                            SUBLOCATION2 = e.SUBLOCATION2,
                            SUBLOCATION3 = e.SUBLOCATION3
                        })
                        .DefaultIfEmpty(new WorkOrderEquipment { EQNUM = "NULL" })
                        .SingleOrDefault();

                    WOEQLIST.Add(TempEquip);
                }
            
        }

        public QueryStatus Insert()
        {
            int intRecordsAffected = 0;
            MP2_DataBaseSettings objDb;

            try
            {
                objDb = new MP2_DataBaseSettings();
                var strSQL = QueryDefinitions.GetQuery("InsertIntoWO", new string[] { WONUM, CLOSEDATE.ToString("d"), TASKDESC.EscapeSingleQuotes(),
                WOTYPE, ORIGINATOR, PRIORITY.ToString(), REQUESTTIME.ToString("G"), REQUESTDATE.ToString("d")});

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

        public IEnumerable<QueryStatus> AddWorkOrder()
        {
            List<QueryStatus> QueryStatuses = new List<QueryStatus>();

            QueryStatuses.Add(this.Insert());
            if (QueryStatuses[0].RecordsAffected > 0)
                foreach (var objWOEquip in WOEQLIST)
                    QueryStatuses.Add(objWOEquip.Insert());

            return QueryStatuses;
        }
    }

    public class WorkOrders : List<WorkOrder>
    {
        public WorkOrders()
            : base()
        {
            MP2_DataBaseSettings db = new MP2_DataBaseSettings();
            List<WorkOrderEquipment> objWorkOrderEquipmentList;
            DateTime dtmTemp;
            int intTemp;
            string strSQL;
            StringBuilder objStrBldr;
            


            strSQL = QueryDefinitions.GetQuery("SelectAllWorkOrders");

            objStrBldr = new StringBuilder();
            using (db.OleDBConnection)
            {
                OleDbCommand objOleDbCommand = new OleDbCommand(strSQL, db.OleDBConnection);
                db.OleDBConnection.Open();
                OleDbDataReader objOleDbDataReader = objOleDbCommand.ExecuteReader();

                objStrBldr.Clear();
                while (objOleDbDataReader.Read())
                {
                    var objWorkOrder = new WorkOrder { 
                        WONUM = objOleDbDataReader["WONUM"].ToString(),
                        CLOSEDATE = DateTime.TryParse(objOleDbDataReader["CLOSEDATE"].ToString(), out dtmTemp) ? dtmTemp : SharedVariables.MINDATE,
                        ORIGINATOR = objOleDbDataReader["ORIGINATOR"].ToString(),
                        PRIORITY = int.TryParse(objOleDbDataReader["PRIORITY"].ToString(), out intTemp) ? intTemp : 0,
                        REQUESTDATE = DateTime.TryParse(objOleDbDataReader["REQUESTDATE"].ToString(), out dtmTemp) ? dtmTemp : SharedVariables.MINDATE,
                        REQUESTTIME = DateTime.TryParse(objOleDbDataReader["REQUESTTIME"].ToString(), out dtmTemp) ? dtmTemp : SharedVariables.MINDATE,
                        TASKDESC = objOleDbDataReader["TASKDESC"].ToString(),
                        WOTYPE = objOleDbDataReader["WOTYPE"].ToString(),
                        STATUS = objOleDbDataReader["STATUS"].ToString()
                    };
                    objStrBldr.Append(objWorkOrder.WONUM + ",");
                    this.Add(objWorkOrder);
                }
                db.OleDBConnection.Close();


                strSQL = QueryDefinitions.GetQuery("SelectWOEQLISTByWONUMList", new string[] { objStrBldr.ToString().AddSingleQuotes() });
                objOleDbCommand = new OleDbCommand(strSQL, db.OleDBConnection);
                db.OleDBConnection.Open();
                objOleDbDataReader = objOleDbCommand.ExecuteReader();

                objWorkOrderEquipmentList = new List<WorkOrderEquipment>();
                while (objOleDbDataReader.Read())
                {
                    var objWorkOrderEquipment = new WorkOrderEquipment
                    {
                        WONUM = objOleDbDataReader["WONUM"].ToString(),
                        CLOSEDATE = DateTime.TryParse(objOleDbDataReader["CLOSEDATE"].ToString(), out dtmTemp) ? dtmTemp : SharedVariables.MINDATE,
                        EQNUM = objOleDbDataReader["EQNUM"].ToString(),
                        LOCATION = objOleDbDataReader["LOCATION"].ToString(),
                        SUBLOCATION1 = objOleDbDataReader["SUBLOCATION1"].ToString(),
                        SUBLOCATION2 = objOleDbDataReader["SUBLOCATION2"].ToString(),
                        SUBLOCATION3 = objOleDbDataReader["SUBLOCATION3"].ToString(),
                        DEPARTMENT = objOleDbDataReader["DEPARTMENT"].ToString(),
                        EQDESC = objOleDbDataReader["EQDESC"].ToString()
                    };
                    objWorkOrderEquipmentList.Add(objWorkOrderEquipment);
                }
                db.OleDBConnection.Close();

            }
            foreach (var objWorkOrder in this)
                objWorkOrder.WOEQLIST = objWorkOrderEquipmentList
                    .Where(w => w.WONUM.Equals(objWorkOrder.WONUM))
                    .ToList();

        }
    }
}