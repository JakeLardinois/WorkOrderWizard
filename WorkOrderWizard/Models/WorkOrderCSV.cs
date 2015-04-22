using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using LINQtoCSV;
using System.IO;
using System.Text;


namespace WorkOrderWizard.Models
{
    public class WorkOrderCSVList : List<WorkOrderCSV>
    {
        public WorkOrderCSVList(IList<WO> WorkOrderCollection)
            : base()
        {
            this.AddRange(WorkOrderCollection
                .Select(w => new WorkOrderCSV
                {
                    WONUM = w.WONUM,
                    CLOSEDATE = w.CLOSEDATE,
                    STATUS = w.STATUS,
                    PRIORITY = w.PRIORITY,
                    WOTYPE = w.WOTYPE,
                    ORIGINATOR = w.ORIGINATOR,
                    REQUESTDATE = w.REQUESTDATE,
                    REQUESTTIME = w.REQUESTTIME,
                    COMPLETIONDATE = w.COMPLETIONDATE,
                    COMPLETIONTIME = w.COMPLETIONTIME,
                    TASKDESC = w.TASKDESC,
                    NOTES = w.NOTES,
                    DaysOpen = w.DaysOpen
                }));
        }

        //http://www.codeproject.com/Articles/25133/LINQ-to-CSV-library
        public string ToCSVString()
        {
            Stream objStream;


            CsvFileDescription outputFileDescription = new CsvFileDescription
            {
                SeparatorChar = '\t', // tab delimited
                FirstLineHasColumnNames = true, // column names in first record
                FileCultureName = "en-US" // use US style dates and numbers
            };

            using (var objMemoryStream = new MemoryStream())
            {
                using (TextWriter objTextWriter = new StreamWriter(objMemoryStream))
                {
                    CsvContext cc = new CsvContext();
                    cc.Write(
                        this,
                        objTextWriter,
                        outputFileDescription);
                    objTextWriter.Flush();
                    objMemoryStream.Position = 0;

                    return Encoding.ASCII.GetString(objMemoryStream.ToArray());
                }
            }
        }
    }

    public class WorkOrderEquipCSVList : List<WorkOrderEquipmentCSV>
    {
        public WorkOrderEquipCSVList(IList<WO> WorkOrderCollection)
            :base()
        {
            this.AddRange(WorkOrderCollection
                .SelectMany(w => w.WOEQLIST
                .Select(e => new WorkOrderEquipmentCSV
                {
                    WONUM = w.WONUM,
                    CLOSEDATE = w.CLOSEDATE,
                    STATUS = w.STATUS,
                    PRIORITY = w.PRIORITY,
                    WOTYPE = w.WOTYPE,
                    ORIGINATOR = w.ORIGINATOR,
                    REQUESTDATE = w.REQUESTDATE,
                    REQUESTTIME = w.REQUESTTIME,
                    COMPLETIONDATE = w.COMPLETIONDATE,
                    COMPLETIONTIME = w.COMPLETIONTIME,
                    TASKDESC = w.TASKDESC,
                    NOTES = w.NOTES,
                    DaysOpen = w.DaysOpen,
                    EQNUM = e.EQNUM,
                    LOCATION = e.LOCATION,
                    SUBLOCATION1 = e.SUBLOCATION1,
                    SUBLOCATION2 = e.SUBLOCATION2,
                    SUBLOCATION3 = e.SUBLOCATION3,
                    DEPARTMENT = e.DEPARTMENT,
                    COSTCENTER = e.COSTCENTER,
                    WOEquipmentNotes = e.TEXTS
                })));
        }

        //http://www.codeproject.com/Articles/25133/LINQ-to-CSV-library
        public string ToCSVString()
        {
            Stream objStream;


            CsvFileDescription outputFileDescription = new CsvFileDescription
            {
                SeparatorChar = '\t', // tab delimited
                FirstLineHasColumnNames = true, // column names in first record
                FileCultureName = "en-US" // use US style dates and numbers
            };

            using (var objMemoryStream = new MemoryStream())
            {
                using (TextWriter objTextWriter = new StreamWriter(objMemoryStream))
                {
                    CsvContext cc = new CsvContext();
                    cc.Write(
                        this,
                        objTextWriter,
                        outputFileDescription);
                    objTextWriter.Flush();
                    objMemoryStream.Position = 0;

                    return Encoding.ASCII.GetString(objMemoryStream.ToArray());
                }
            }
        }
    }


    public class WorkOrderCSV
    {
        private string mTASKDESC { get; set; }
        private string mNOTES { get; set; }
        private string mWOEquipmentNotes { get; set; }


        #region WO_Fields
        [CsvColumn(Name = "WONUM", FieldIndex = 1)]
        public string WONUM { get; set; }

        [CsvColumn(Name = "CLOSEDATE", FieldIndex = 2, OutputFormat = "MM/dd/yyyy")]
        public DateTime CLOSEDATE { get; set; }

        [CsvColumn(Name = "STATUS", FieldIndex = 3)]
        public char? STATUS { get; set; }

        [CsvColumn(Name = "PRIORITY", FieldIndex = 4)]
        public double? PRIORITY { get; set; }

        [CsvColumn(Name = "WOTYPE", FieldIndex = 5)]
        public string WOTYPE { get; set; }

        [CsvColumn(Name = "ORIGINATOR", FieldIndex = 6)]
        public string ORIGINATOR { get; set; }

        [CsvColumn(Name = "REQUESTDATE", FieldIndex = 7, OutputFormat = "MM/dd/yyyy")]
        public DateTime? REQUESTDATE { get; set; }

        [CsvColumn(Name = "REQUESTTIME", FieldIndex = 8, OutputFormat = "h:MM tt")]
        public DateTime? REQUESTTIME { get; set; }

        [CsvColumn(Name = "COMPLETIONDATE", FieldIndex = 9, OutputFormat = "MM/dd/yyyy")]
        public DateTime? COMPLETIONDATE { get; set; }

        [CsvColumn(Name = "COMPLETIONTIME", FieldIndex = 10, OutputFormat = "h:MM tt")]
        public DateTime? COMPLETIONTIME { get; set; }

        [CsvColumn(Name = "TASKDESC", FieldIndex = 11)]
        public string TASKDESC
        {
            get { return string.IsNullOrEmpty(mTASKDESC) ? string.Empty : mTASKDESC.RemoveTabAndNewLine(); }
            set { mTASKDESC = value; }
        }

        [CsvColumn(Name = "WONotes", FieldIndex = 12)]
        public string NOTES
        {
            get { return string.IsNullOrEmpty(mNOTES) ? string.Empty : mNOTES.RemoveTabAndNewLine(); }
            set { mNOTES = value; }
        }
        #endregion

        [CsvColumn(Name = "DaysOpen", FieldIndex = 21)]
        public int? DaysOpen { get; set; }
    }

    public class WorkOrderEquipmentCSV
    {
        private string mTASKDESC { get; set; }
        private string mNOTES { get; set; }
        private string mWOEquipmentNotes { get; set; }


        #region WO_Fields
        [CsvColumn(Name = "WONUM", FieldIndex = 1)]
        public string WONUM { get; set; }

        [CsvColumn(Name = "CLOSEDATE", FieldIndex = 2, OutputFormat = "MM/dd/yyyy")]
        public DateTime CLOSEDATE { get; set; }

        [CsvColumn(Name = "STATUS", FieldIndex = 3)]
        public char? STATUS { get; set; }

        [CsvColumn(Name = "PRIORITY", FieldIndex = 4)]
        public double? PRIORITY { get; set; }

        [CsvColumn(Name = "WOTYPE", FieldIndex = 5)]
        public string WOTYPE { get; set; }

        [CsvColumn(Name = "ORIGINATOR", FieldIndex = 6)]
        public string ORIGINATOR { get; set; }

        [CsvColumn(Name = "REQUESTDATE", FieldIndex = 7, OutputFormat = "MM/dd/yyyy")]
        public DateTime? REQUESTDATE { get; set; }

        [CsvColumn(Name = "REQUESTTIME", FieldIndex = 8, OutputFormat = "h:MM tt")]
        public DateTime? REQUESTTIME { get; set; }

        [CsvColumn(Name = "COMPLETIONDATE", FieldIndex = 9, OutputFormat = "MM/dd/yyyy")]
        public DateTime? COMPLETIONDATE { get; set; }

        [CsvColumn(Name = "COMPLETIONTIME", FieldIndex = 10, OutputFormat = "h:MM tt")]
        public DateTime? COMPLETIONTIME { get; set; }

        [CsvColumn(Name = "TASKDESC", FieldIndex = 11)]
        public string TASKDESC {
            get { return string.IsNullOrEmpty(mTASKDESC) ? string.Empty : mTASKDESC.RemoveTabAndNewLine(); }
            set { mTASKDESC = value; }
        }

        [CsvColumn(Name = "WONotes", FieldIndex = 12)]
        public string NOTES
        {
            get { return string.IsNullOrEmpty(mNOTES) ? string.Empty : mNOTES.RemoveTabAndNewLine(); }
            set { mNOTES = value; }
        }
        #endregion


        #region WOEQLIST_fields
        [CsvColumn(Name = "EQNUM", FieldIndex = 13)]
        public string EQNUM { get; set; }

        [CsvColumn(Name = "LOCATION", FieldIndex = 14)]
        public string LOCATION { get; set; }

        [CsvColumn(Name = "SUBLOCATION1", FieldIndex = 15)]
        public string SUBLOCATION1 { get; set; }

        [CsvColumn(Name = "SUBLOCATION2", FieldIndex = 16)]
        public string SUBLOCATION2 { get; set; }

        [CsvColumn(Name = "SUBLOCATION3", FieldIndex = 17)]
        public string SUBLOCATION3 { get; set; }

        [CsvColumn(Name = "DEPARTMENT", FieldIndex = 18)]
        public string DEPARTMENT { get; set; }

        [CsvColumn(Name = "COSTCENTER", FieldIndex = 19)]
        public string COSTCENTER { get; set; }

        [CsvColumn(Name = "WOEquipmentNotes", FieldIndex = 20)]
        public string WOEquipmentNotes //the Texts property from WOC
        {
            get { return string.IsNullOrEmpty(mWOEquipmentNotes) ? string.Empty : mWOEquipmentNotes.RemoveTabAndNewLine(); }
            set { mWOEquipmentNotes = value; }
        }
        #endregion

        [CsvColumn(Name = "DaysOpen", FieldIndex = 21)]
        public int? DaysOpen { get; set; }
    }
}