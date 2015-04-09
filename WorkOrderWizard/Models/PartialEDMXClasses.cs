using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Text;
using System.Data.OleDb;
using System.Data;

namespace WorkOrderWizard.Models
{
    public partial class WO : IEquatable<WO>
    {
        private StringBuilder objStrBldrSQL = new StringBuilder();
        private List<WOEQLIST> mWOEQLIST { get; set; }

        public WO()
            :base()
        {
            CLOSEDATE = SharedVariables.MINDATE;
            REQUESTTIME = new DateTime(SharedVariables.MINDATE.Year, SharedVariables.MINDATE.Month, SharedVariables.MINDATE.Day, 
                DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);
            REQUESTDATE = DateTime.Now.Date;


        }

        public WO(string WONUM, DateTime CLOSEDATE)
            : base()
        {
            MP2_DataBaseSettings db = new MP2_DataBaseSettings();
            List<WOEQLIST> objWorkOrderEquipmentList;
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
                    NOTES = recordset.First().Field<string>("NOTES");
                    WOTYPE = recordset.First().Field<string>("WOTYPE");
                    STATUS = recordset.First().Field<char?>("STATUS");

                    WOEQLIST = recordset
                        .DefaultIfEmpty(objDataTable.NewRow()) //Handles the problem where MP2 has data orphans...
                        .Select(g => new WOEQLIST
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

        public List<WOEQLIST> WOEQLIST { 
            get {
                if (!string.IsNullOrEmpty(WONUM))
                    using (var db = new mp250dbDB())
                    {
                        mWOEQLIST = new List<WOEQLIST>();
                        mWOEQLIST.AddRange(db.WOEQLISTs
                            .Where(w => w.WONUM.Equals(WONUM) && w.CLOSEDATE == CLOSEDATE)
                            .ToList());
                    }
                return mWOEQLIST;
            }
            set
            {
                mWOEQLIST = value;
            }
        }
        //public List<WorkOrderEquipment> WOEQLIST
        //{
        //    get
        //    {
        //        using (var db = new mp250dbDB())
        //        {
        //            return db.WOEQLISTs
        //                .Where(w => w.WONUM.Equals(WONUM))
        //                .Select(w => new WorkOrderEquipment {
        //                    WONUM = w.WONUM,
        //                    CLOSEDATE = w.CLOSEDATE,
        //                    EQNUM = w.EQNUM,
        //                    LOCATION = w.LOCATION,
        //                    SUBLOCATION1 = w.SUBLOCATION1,
        //                    SUBLOCATION2 = w.SUBLOCATION2,
        //                    SUBLOCATION3 = w.SUBLOCATION3,
        //                    DEPARTMENT = w.DEPARTMENT,
        //                    EQDESC = w.EQDESC
        //                })
        //                .ToList();
        //        }
        //    }
        //}
       
        public virtual string HTMLWONotes
        {
            get
            {
                if (!string.IsNullOrEmpty(NOTES))
                    return NOTES
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

            WOEQLIST = new List<WOEQLIST>();
            if (WOEquipment != null)
                foreach (var strEquipment in WOEquipment)
                    using (var db = new mp250dbDB())
                    {
                        WOEQLIST.Add(db.EQUIPs
                        .Where(e => e.EQNUM.Equals(strEquipment))
                        .Select(e => new WOEQLIST
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
                        .DefaultIfEmpty(new WOEQLIST { EQNUM = "NULL" })
                        .SingleOrDefault());
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
                objStrBldrSQL.Append(QueryDefinitions.GetQuery("InsertIntoWO", new string[] { WONUM, CLOSEDATE.ToString("d"), TASKDESC.EscapeSingleQuotes(), NOTES.EscapeSingleQuotes(),
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
            objStrBldrSQL.Append(QueryDefinitions.GetQuery("UpdateWONotes", new string[] { NOTES.EscapeSingleQuotes(), WONUM, CLOSEDATE.ToString("d") }));

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

        #region IEquatableMethods
        public bool Equals(WO other)
        {

            //Check whether the compared object is null.
            if (Object.ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data.
            if (Object.ReferenceEquals(this, other)) return true;

            //Check whether the products' properties are equal.
            return WONUM.Equals(other.WONUM) && CLOSEDATE.Equals(other.CLOSEDATE);
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.

        public override int GetHashCode()
        {

            //Get hash code for the Name field if it is not null.
            int hashWONUM = WONUM == null ? 0 : WONUM.GetHashCode();

            //Get hash code for the Code field.
            int hashCLOSEDATE = CLOSEDATE.GetHashCode();

            //Calculate the hash code for the product.
            return hashWONUM ^ hashCLOSEDATE;
        }
        #endregion
    }

    public partial class WOes
    {
        public static int TotalRecordCount
        {
            get
            {
                int intTemp;

                using (var db = new mp250dbDB())
                    intTemp = db.WOes.Count();

                return intTemp;
            }
        }
    }

    public partial class WOEQLIST
    {
        public WOEQLIST()
            : base()
        {
            CLOSEDATE = SharedVariables.MINDATE;
        }

        public QueryStatus Insert()
        {
            int intRecordsAffected = 0;
            MP2_DataBaseSettings objDb;


            try
            {
                objDb = new MP2_DataBaseSettings();
                var strSQL = QueryDefinitions.GetQuery("InsertIntoWOEQLIST", new string[] { WONUM, string.Format("{0:d}", CLOSEDATE), EQNUM,
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

    public partial class EQUIP
    {
        public string DescriptionDisplay { 
            get { 
                return EQNUM + ": " + DESCRIPTION; 
            }
        }
    }
}