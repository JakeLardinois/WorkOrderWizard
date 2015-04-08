using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.IO;
using System.Data.OleDb;
using System.Text;
using System.Data;




namespace WorkOrderWizard.Models
{
    public class WorkOrder
    {
        private StringBuilder objStrBldrSQL = new StringBuilder();


        public WorkOrder()
        {
            CLOSEDATE = SharedVariables.MINDATE;
            REQUESTTIME = new DateTime(SharedVariables.MINDATE.Year, SharedVariables.MINDATE.Month, SharedVariables.MINDATE.Day, 
                DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);
            REQUESTDATE = DateTime.Now.Date;
        }

        public WorkOrder(string WONUM, DateTime CLOSEDATE)
        {
            MP2_DataBaseSettings db = new MP2_DataBaseSettings();
            List<WorkOrderEquipment> objWorkOrderEquipmentList;
            DataTable objDataTable;
            int intTemp;


            objStrBldrSQL.Clear();
            objStrBldrSQL.Append(QueryDefinitions.GetQuery("SelectWOAndEQLISTByWONumAndCloseDate", new[] { WONUM, CLOSEDATE.ToString("d") }));

            using (db.OleDBConnection)
            {
                OleDbCommand objOleDbCommand = new OleDbCommand(objStrBldrSQL.ToString(), db.OleDBConnection);
                db.OleDBConnection.Open();
                OleDbDataReader objOleDbDataReader = objOleDbCommand.ExecuteReader();
                objDataTable = new DataTable();
                objDataTable.Load(objOleDbDataReader);
                db.OleDBConnection.Close();

                var recordset = objDataTable.AsEnumerable();
                if (recordset.Count() != 0)
                {
                    WONUM = recordset.First().Field<string>("WONUM");
                    CLOSEDATE = recordset.First().Field<DateTime>("CLOSEDATE");
                    ORIGINATOR = recordset.First().Field<string>("ORIGINATOR");
                    PRIORITY = recordset.First().Field<double?>("PRIORITY");
                    REQUESTDATE = recordset.First().Field<DateTime?>("REQUESTDATE");
                    REQUESTTIME = recordset.First().Field<DateTime?>("REQUESTTIME");
                    TASKDESC = recordset.First().Field<string>("TASKDESC");
                    WONotes = recordset.First().Field<string>("NOTES");
                    WOTYPE = recordset.First().Field<string>("WOTYPE");
                    STATUS = recordset.First().Field<string>("STATUS");

                    WOEQLIST = recordset
                        .DefaultIfEmpty(objDataTable.NewRow()) //Handles the problem where MP2 has data orphans...
                        .Select(g => new WorkOrderEquipment
                        {
                            WONUM = g.Field<string>("WOEQWONUM"),
                            CLOSEDATE = g.Field<DateTime>("WOEQCLOSEDATE"),
                            EQNUM = g.Field<string>("WOEQWONUM"),
                            LOCATION = g.Field<string>("LOCATION"),
                            SUBLOCATION1 = g.Field<string>("SUBLOCATION1"),
                            SUBLOCATION2 = g.Field<string>("SUBLOCATION2"),
                            SUBLOCATION3 = g.Field<string>("SUBLOCATION3"),
                            DEPARTMENT = g.Field<string>("DEPARTMENT"),
                            EQDESC = g.Field<string>("EQDESC")
                        })
                    .ToList();
                }

            }
        }

        public string WONUM { get; set; }
        public DateTime CLOSEDATE { get; set; }
        public string TASKDESC { get; set; }
        public string WOTYPE { get; set; }
        public string ORIGINATOR { get; set; }
        public double? PRIORITY { get; set; }
        public DateTime? REQUESTTIME { get; set; }
        public DateTime? REQUESTDATE { get; set; }
        public string STATUS { get; set; }

        public List<WorkOrderEquipment> WOEQLIST { get; set; }
        public string WONotes { get; set; }
        public virtual string HTMLWONotes
        {
            get
            {
                if (!string.IsNullOrEmpty(WONotes))
                    return WONotes
                        .Replace(Environment.NewLine, "<br />")
                        .Replace("\n", "<br />");
                else
                    return string.Empty;
            }
        }

        //POST variables...
        public string[] WOPriority { get; set; }
        public string[] WOType { get; set; }
        public string[] WOEquipment { get; set; }
        public string EmployeeInfo { get; set; }
        
        public string GetNextWorkOrderNum() 
        {
            var strWorkOrderPrefix = Settings.WorkOrderPrefix;
            MP2_DataBaseSettings objDb = new MP2_DataBaseSettings();
            objStrBldrSQL.Clear();
            objStrBldrSQL.Append(QueryDefinitions.GetQuery("SelectCurrentWONum", new string[] { strWorkOrderPrefix }));
            int intTemp, intWorkOrderNum;

            using (objDb.OleDBConnection)
            {
                OleDbCommand objOleDbCommand = new OleDbCommand(objStrBldrSQL.ToString(), objDb.OleDBConnection);
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

                objStrBldrSQL.Clear();
                objStrBldrSQL.Append((strWorkOrderPrefix + intWorkOrderNum.ToString().PadLeft(10 - strWorkOrderPrefix.Length, '0')));

                objDb.OleDBConnection.Close();
            }

            return objStrBldrSQL.ToString().Substring(objStrBldrSQL.ToString().Length - 10); //MP2 WONUM field can only be 10 characters long...
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
                objStrBldrSQL.Clear();
                objStrBldrSQL.Append(QueryDefinitions.GetQuery("InsertIntoWO", new string[] { WONUM, CLOSEDATE.ToString("d"), TASKDESC.EscapeSingleQuotes(), WONotes.EscapeSingleQuotes(),
                WOTYPE, ORIGINATOR, PRIORITY.ToString(), string.Format("{0:G}", REQUESTTIME), string.Format("{0:d}", REQUESTDATE)}));

                OleDbCommand objOleDbCommand = new OleDbCommand(objStrBldrSQL.ToString(), objDb.OleDBConnection);
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

        public bool UpdateWONote()
        {
            MP2_DataBaseSettings db = new MP2_DataBaseSettings();
            int intRecordsAffected = 0;


            //make sure to replace the single quotes with double single quotes in order to escape them
            objStrBldrSQL.Clear();
            objStrBldrSQL.Append(QueryDefinitions.GetQuery("UpdateWONotes", new string[] {WONotes.EscapeSingleQuotes(), WONUM, CLOSEDATE.ToString("d") }));

            using (db.OleDBConnection)
            {
                OleDbCommand objOleDbCommand = new OleDbCommand(objStrBldrSQL.ToString(), db.OleDBConnection);
                db.OleDBConnection.Open();
                try
                {
                    intRecordsAffected = objOleDbCommand.ExecuteNonQuery();
                }
                catch
                {
                    db.OleDBConnection.Close();
                    return false;
                }
                db.OleDBConnection.Close();
            }

            return intRecordsAffected == 0 ? false : true;
        }

    }

    public class WorkOrders : List<WorkOrder>
    {
        private StringBuilder objStrBldrSQL = new StringBuilder();
        public static int TotalRecordCount
        {
            get
            {
                MP2_DataBaseSettings db = new MP2_DataBaseSettings();
                OleDbCommand objOleDbCommand;
                OleDbDataReader objOleDbDataReader;
                int intTemp;
                

                using (db.OleDBConnection)
                {
                    var strSQL = QueryDefinitions.GetQuery("SelectTotalWOCount");

                    objOleDbCommand = new OleDbCommand(strSQL, db.OleDBConnection);
                    db.OleDBConnection.Open();
                    objOleDbDataReader = objOleDbCommand.ExecuteReader();
                    objOleDbDataReader.Read();
                    db.OleDBConnection.Close();
                    intTemp = int.TryParse(objOleDbDataReader["TotalWorkOrders"].ToString(), out intTemp) ? intTemp : 0;
                }

                return intTemp;
            }
        }

        public WorkOrders(string strWOStatuses)
            : base()
        {
            MP2_DataBaseSettings db = new MP2_DataBaseSettings();
            List<WorkOrderEquipment> objWorkOrderEquipmentList;
            DataTable objDataTable;
            int intTemp;


            objStrBldrSQL.Clear();
            objStrBldrSQL.Append(QueryDefinitions.GetQuery("SelectWOAndEQLISTByStatusList", new[] { strWOStatuses.AddSingleQuotes() }));

            using (db.OleDBConnection)
            {
                OleDbCommand objOleDbCommand = new OleDbCommand(objStrBldrSQL.ToString(), db.OleDBConnection);
                db.OleDBConnection.Open();
                OleDbDataReader objOleDbDataReader = objOleDbCommand.ExecuteReader();
                objDataTable = new DataTable();
                objDataTable.Load(objOleDbDataReader);
                db.OleDBConnection.Close();


                this.AddRange(objDataTable.AsEnumerable()
                    .GroupBy(w => new { WONUM = w.Field<string>("WONUM"), CLOSEDATE = w.Field<DateTime>("CLOSEDATE") })
                    .Select(g => new WorkOrder
                    {
                        WONUM = g.Key.WONUM,
                        CLOSEDATE = g.Key.CLOSEDATE,
                        ORIGINATOR = g.Min(w => w.Field<string>("ORIGINATOR")),
                        PRIORITY = g.Min(w => w.Field<double?>("PRIORITY")),
                        REQUESTDATE = g.Min(w => w.Field<DateTime?>("REQUESTDATE")),
                        REQUESTTIME = g.Min(w => w.Field<DateTime?>("REQUESTTIME")),
                        TASKDESC = g.Min(w => w.Field<string>("TASKDESC")),
                        WONotes = g.Min(w => w.Field<string>("NOTES")),
                        WOTYPE = g.Min(w => w.Field<string>("WOTYPE")),
                        STATUS = g.Min(w => w.Field<string>("STATUS"))
                    })
                    .ToList());

                foreach (var objWorkOrder in this)
                {
                    objWorkOrder.WOEQLIST = objDataTable.AsEnumerable()
                        .Where(w => w.Field<string>("WOEQWONUM").Equals(objWorkOrder.WONUM) && w.Field<DateTime>("WOEQCLOSEDATE").Equals(objWorkOrder.CLOSEDATE))
                        .DefaultIfEmpty(objDataTable.NewRow()) //Handles the problem where MP2 has data orphans...
                        .Select(g => new WorkOrderEquipment {
                            WONUM = g.Field<string>("WOEQWONUM"),
                            CLOSEDATE = g.Field<DateTime>("WOEQCLOSEDATE"),
                            EQNUM = g.Field<string>("EQNUM"),
                            LOCATION = g.Field<string>("LOCATION"),
                            SUBLOCATION1 = g.Field<string>("SUBLOCATION1"),
                            SUBLOCATION2 = g.Field<string>("SUBLOCATION2"),
                            SUBLOCATION3 = g.Field<string>("SUBLOCATION3"),
                            DEPARTMENT = g.Field<string>("DEPARTMENT"),
                            EQDESC = g.Field<string>("EQDESC")
                        })
                        .ToList();
                }

            }

        }
        #region OldInitializer
       /* public WorkOrders()
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
                    var objWorkOrder = new WorkOrder
                    {
                        WONUM = objOleDbDataReader["WONUM"].ToString(),
                        CLOSEDATE = DateTime.TryParse(objOleDbDataReader["CLOSEDATE"].ToString(), out dtmTemp) ? dtmTemp : SharedVariables.MINDATE,
                        ORIGINATOR = objOleDbDataReader["ORIGINATOR"].ToString(),
                        PRIORITY = int.TryParse(objOleDbDataReader["PRIORITY"].ToString(), out intTemp) ? intTemp : 0,
                        REQUESTDATE = DateTime.TryParse(objOleDbDataReader["REQUESTDATE"].ToString(), out dtmTemp) ? dtmTemp : SharedVariables.MINDATE,
                        REQUESTTIME = DateTime.TryParse(objOleDbDataReader["REQUESTTIME"].ToString(), out dtmTemp) ? dtmTemp : SharedVariables.MINDATE,
                        TASKDESC = objOleDbDataReader["TASKDESC"].ToString(),
                        WONotes = objOleDbDataReader["NOTES"].ToString(),
                        WOTYPE = objOleDbDataReader["WOTYPE"].ToString(),
                        STATUS = objOleDbDataReader["STATUS"].ToString()
                    };
                    objStrBldr.Append(objWorkOrder.WONUM + ",");
                    this.Add(objWorkOrder);
                }
                db.OleDBConnection.Close();


                //strSQL = QueryDefinitions.GetQuery("SelectWOEQLISTByWONUMList", new string[] { objStrBldr.ToString().AddSingleQuotes() });
                strSQL = QueryDefinitions.GetQuery("SelectAllWOEQLIST", new string[] { objStrBldr.ToString().AddSingleQuotes() });
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

        }*/
        #endregion
        
    }
}