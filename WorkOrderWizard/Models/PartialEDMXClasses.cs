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
        private StringBuilder objStrBldr = new StringBuilder();
        private List<WOEQLIST> mWOEQLIST { get; set; }
        public List<WOEQLIST> WOEQToAdd { get; set; }

        public WO()
            :base()
        {
            CLOSEDATE = SharedVariables.MINDATE;
            REQUESTTIME = new DateTime(SharedVariables.MINDATE.Year, SharedVariables.MINDATE.Month, SharedVariables.MINDATE.Day, 
                DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);
            REQUESTDATE = DateTime.Now.Date;

        }

        public virtual List<WOEQLIST> WOEQLIST
        { 
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

        public virtual string Equipment
        {
            get
            {
                objStrBldr.Clear();
                if (WOEQLIST != null)
                    foreach (var objEquipment in WOEQLIST)
                        objStrBldr.Append(objEquipment.EQNUM + ", ");

                return objStrBldr.Length > 2 ? objStrBldr.ToString().Substring(0, objStrBldr.Length - 2) : string.Empty;

            }
        }

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

        public virtual int? DaysOpen { //the int is made nullable so that it can be excluded from the pivot table calculation when invalid data is encountered
            get
            {
                DateTime dtmRequestDate, dtmCompletionDate;
                int intDaysOpen;


                if (STATUS != null && STATUS == 'C') //all Work Orders that go through the 'Close' process get thier status updated to 'Completed' regardless of what thier original status was
                {
                    dtmRequestDate = REQUESTDATE ?? SharedVariables.MINDATE; //uses the null-coalescing operator to get the nullable REQUESTDATE to a DateTime object
                    if (dtmRequestDate == SharedVariables.MINDATE)
                        return (int?)null;

                    dtmCompletionDate = COMPLETIONDATE ?? SharedVariables.MINDATE; //uses the null-coalescing operator to get the nullable COMPLETIONDATE to a DateTime object
                    if (dtmCompletionDate == SharedVariables.MINDATE)
                        return (int?)null;

                    intDaysOpen = (dtmCompletionDate - dtmRequestDate).Days;
                    return intDaysOpen >= 0 ? intDaysOpen : (int?)null;   //if the CLOSEDATE occurred before the REQUESTDATE (resulting in a negative) then the data is invalid and so a null is returned
                }
                else if (STATUS != null && (STATUS == 'O' || STATUS == 'R'))
                {
                    dtmRequestDate = REQUESTDATE ?? SharedVariables.MINDATE; //uses the null-coalescing operator to get the nullable REQUESTDATE to a DateTime object
                    if (dtmRequestDate == SharedVariables.MINDATE)
                        return (int?)null;

                    dtmCompletionDate = DateTime.Now.Date;

                    intDaysOpen = (dtmCompletionDate - dtmRequestDate).Days;
                    return intDaysOpen > 0 ? intDaysOpen : (int?)null;   //if the CLOSEDATE occurred before the REQUESTDATE (resulting in a negative) then the data is invalid and so a null is returned
                }
                else
                    return (int?)null;
            }
        }
        //POST variables...
        public virtual string[] WOPriority { get; set; }
        public virtual string[] WOType { get; set; }
        public virtual string[] WOEquipment { get; set; }
        public virtual string EmployeeInfo { get; set; }

        public string GetNextWorkOrderNum()
        {
            var strWorkOrderPrefix = Settings.WorkOrderPrefix;
            MP2_DataBaseSettings objDb = new MP2_DataBaseSettings();
            objStrBldr.Clear();
            objStrBldr.Append(QueryDefinitions.GetQuery("SelectCurrentWONum", new string[] { strWorkOrderPrefix }));
            int intTemp, intWorkOrderNum;

            using (objDb.OleDBConnection)
            {
                OleDbCommand objOleDbCommand = new OleDbCommand(objStrBldr.ToString(), objDb.OleDBConnection);
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

                objStrBldr.Clear();
                objStrBldr.Append((strWorkOrderPrefix + intWorkOrderNum.ToString().PadLeft(10 - strWorkOrderPrefix.Length, '0')));

                objDb.OleDBConnection.Close();
            }

            return objStrBldr.ToString().Substring(objStrBldr.ToString().Length - 10); //MP2 WONUM field can only be 10 characters long...
        }

        public void PopulateFromPostVariables()
        {
            int intTemp;


            PRIORITY = int.TryParse(WOPriority[0], out intTemp) ? intTemp : 5; //5 is the lowest priority...
            ORIGINATOR = EmployeeInfo.Split(':')[0]; //MP2 'Short Text' fields only allow 25 characters...
            WOTYPE = WOType[0];

            WOEQToAdd = new List<WOEQLIST>();
            if (WOEquipment != null)
                foreach (var strEquipment in WOEquipment)
                    using (var db = new mp250dbDB())
                    {
                        WOEQToAdd.Add(db.EQUIPs
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
                objStrBldr.Clear();
                objStrBldr.Append(QueryDefinitions.GetQuery("InsertIntoWO", new string[] { WONUM, CLOSEDATE.ToString("d"), TASKDESC.EscapeSingleQuotes(), NOTES.EscapeSingleQuotes(),
                WOTYPE, ORIGINATOR, PRIORITY.ToString(), string.Format("{0:G}", REQUESTTIME), string.Format("{0:d}", REQUESTDATE)}));

                OleDbCommand objOleDbCommand = new OleDbCommand(objStrBldr.ToString(), objDb.OleDBConnection);
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
                foreach (var objWOEquip in WOEQToAdd)
                    QueryStatuses.Add(objWOEquip.Insert());

            return QueryStatuses;
        }

        public bool UpdateWONote()
        {
            MP2_DataBaseSettings db = new MP2_DataBaseSettings();
            int intRecordsAffected = 0;


            //make sure to replace the single quotes with double single quotes in order to escape them
            objStrBldr.Clear();
            objStrBldr.Append(QueryDefinitions.GetQuery("UpdateWONotes", new string[] { NOTES.EscapeSingleQuotes(), WONUM, CLOSEDATE.ToString("d") }));

            using (db.OleDBConnection)
            {
                OleDbCommand objOleDbCommand = new OleDbCommand(objStrBldr.ToString(), db.OleDBConnection);
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
        private WOC mWOC { get; set; }


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

        public virtual string HTMLTEXTS
        {
            get
            {
                if (!string.IsNullOrEmpty(TEXTS))
                    return TEXTS
                        .Replace(Environment.NewLine, "<br />")
                        .Replace("\n", "<br />");
                else
                    return string.Empty;
            }
        }

        public virtual string TEXTS
        { 
            get {
                if (WOC != null)
                    return WOC.TEXTS;
                else
                    return string.Empty;
            } 
        }

        public virtual WOC WOC
        {
            get { 
                if (!string.IsNullOrEmpty(EQNUM))
                    using (var db = new mp250dbDB())
                    {
                        mWOC = db.WOCs
                            .Where(w => w.WONUM.Equals(WONUM) && 
                                (w.CLOSEDATE == CLOSEDATE) &&
                                w.EQNUM == EQNUM && 
                                w.LOCATION == LOCATION && 
                                w.SUBLOCATION1 == SUBLOCATION1 &&
                                w.SUBLOCATION2 == SUBLOCATION2 &&
                                w.SUBLOCATION3 == SUBLOCATION3)
                            .DefaultIfEmpty(new WOC { TEXTS = string.Empty})
                            .SingleOrDefault();
                    }
                return mWOC;
            }
            set { mWOC = value; }
        }
    }

    public partial class EQUIP
    {
        public virtual string DescriptionDisplay
        { 
            get { 
                return EQNUM + ": " + DESCRIPTION; 
            }
        }
    }
}