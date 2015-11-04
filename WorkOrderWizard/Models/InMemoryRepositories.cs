using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Collections.ObjectModel;
using System.Data.OleDb;
using System.Text;
using System.Linq.Dynamic;
using System.Data.Objects.SqlClient;
using System.Text.RegularExpressions;


namespace WorkOrderWizard.Models
{
    public static class InMemoryWorkOrdersRepository
    {
        public static IList<WO> GetWorkOrders(out int searchRecordCount, JQueryDataTablesModel DataTablesModel, bool isDownloadReport = false)
        {
            ReadOnlyCollection<SortedColumn> sortedColumns = DataTablesModel.GetSortedColumns();
            IEnumerable<WO> workorders;
            DateTime dtmTemp;
            int intTemp;
            string[] objResults;
            WorkOrderSearch objWorkOrderSearch;
            string strEmptyString = "EMPTY";
            StringBuilder objStrBldr = new StringBuilder();


            objWorkOrderSearch = new WorkOrderSearch();
            for (int intCounter = 0; intCounter < DataTablesModel.iColumns; intCounter++)
            {

                if (DataTablesModel.bSearchable_[intCounter] == true && !string.IsNullOrEmpty(DataTablesModel.sSearch_[intCounter]))
                {
                    /*For some reason when I implemented resizable movable columns and would then move the columns in the application, the application would send tilde's in the 'checkbox' column types sSearch field which was wierd
                     since the checkbox column types are delimited by the pipe | character and the 'range' column types are delimited by the tilde...  The resolution that I came up with was to check if the only value passed in sSearch
                     was a tilde and if it was then skip the loop so that the respective VendorRequestSearch field was left null.*/
                    if (DataTablesModel.sSearch_[intCounter].Equals("~"))
                        continue;

                    /*Notice that i had to use mDataProp2_ due to datatables multi-column filtering not placing sSearch into proper array position when columns are reordered; See VendorRequestsController.cs Search method for details...*/
                    switch (DataTablesModel.mDataProp2_[intCounter])
                    {
                        case "WONUM":
                            objStrBldr.Clear();
                            objStrBldr.Append(DataTablesModel.sSearch_[intCounter]);
                            objWorkOrderSearch.WONUM = string.IsNullOrEmpty(objStrBldr.ToString()) ? strEmptyString : DataTablesModel.sSearch_[intCounter];
                            break;
                        case "CLOSEDATE":
                            objResults = DataTablesModel.sSearch_[intCounter].Split('~');//results returned from a daterange are delimited by the tilde char
                            objWorkOrderSearch.CLOSEDATEGT = DateTime.TryParse(objResults[0], out dtmTemp) ? dtmTemp : DateTime.MinValue;
                            objWorkOrderSearch.CLOSEDATELT = DateTime.TryParse(objResults[1], out dtmTemp) ? dtmTemp : DateTime.MinValue;
                            break;
                        case "TASKDESC":
                            objWorkOrderSearch.TASKDESC = DataTablesModel.sSearch_[intCounter];
                            break;
                        case "WOTYPE":
                            objWorkOrderSearch.WOTYPES = DataTablesModel.sSearch_[intCounter].Split('|');//results returned from a checklist are delimited by the pipe char
                            break;
                        case "ORIGINATOR":
                            objWorkOrderSearch.ORIGINATOR = DataTablesModel.sSearch_[intCounter];
                            break;
                        case "PRIORITY":
                            objWorkOrderSearch.PRIORITIES = DataTablesModel.sSearch_[intCounter].Split('|');//results returned from a checklist are delimited by the pipe char   
                            break;
                        case "REQUESTDATE":
                            objResults = DataTablesModel.sSearch_[intCounter].Split('~');//results returned from a daterange are delimited by the tilde char
                            objWorkOrderSearch.REQUESTDATEGT = DateTime.TryParse(objResults[0], out dtmTemp) ? dtmTemp : DateTime.MinValue;
                            objWorkOrderSearch.REQUESTDATELT = DateTime.TryParse(objResults[1], out dtmTemp) ? dtmTemp : DateTime.MinValue;
                            break;
                        case "STATUS":
                            objWorkOrderSearch.STATUSES = DataTablesModel.sSearch_[intCounter].Split('|');//results returned from a checklist are delimited by the pipe char
                            break;
                        case "WOEQLIST":
                            objWorkOrderSearch.EQNUMS = DataTablesModel.sSearch_[intCounter].Split('|');//results returned from a checklist are delimited by the pipe char
                            break;
                        case "COMPLETIONDATE":
                            objResults = DataTablesModel.sSearch_[intCounter].Split('~');//results returned from a daterange are delimited by the tilde char
                            objWorkOrderSearch.COMPLETIONDATEGT = DateTime.TryParse(objResults[0], out dtmTemp) ? dtmTemp : DateTime.MinValue;
                            objWorkOrderSearch.COMPLETIONDATELT = DateTime.TryParse(objResults[1], out dtmTemp) ? dtmTemp : DateTime.MinValue;
                            break;
                    }
                }
            }


            /*The Below was created because the Entity Framework had a problem doing a filter of a list with a list because of the difficulty it had using deferred execution and the corresponding sql creation*/
            var WOTYPEList = objWorkOrderSearch.WOTYPES == null ? new[] { strEmptyString } : objWorkOrderSearch.WOTYPES.ToArray<string>();
            var STATUSList = objWorkOrderSearch.STATUSES == null ? new[] { strEmptyString } : objWorkOrderSearch.STATUSES.ToArray<string>();
            var EQNUMList = objWorkOrderSearch.EQNUMS == null ? new[] { strEmptyString } : objWorkOrderSearch.EQNUMS.ToArray<string>();
            //var PRIORITYList = objWorkOrderSearch.PRIORITIES == null ? new[] { (double?)null } : Array.ConvertAll(objWorkOrderSearch.PRIORITIES.ToArray<string>(), MySharedFunctions.TryParseNullableDouble);
            var PRIORITYList = objWorkOrderSearch.PRIORITIES == null ? new[] { 0.0 } : Array.ConvertAll(objWorkOrderSearch.PRIORITIES.ToArray<string>(), double.Parse);

            using (var db = new mp250dbDB())
            {
                workorders = db.WOes.SelectMany(
                    w => db.WOEQLISTs
                    .Where(we => w.WONUM == we.WONUM && w.CLOSEDATE == we.CLOSEDATE)
                    .DefaultIfEmpty(),
                    (w, we) => new
                    {
                        WONUM = w.WONUM,
                        CLOSEDATE = w.CLOSEDATE,
                        ORIGINATOR = w.ORIGINATOR,
                        PRIORITY = w.PRIORITY,
                        REQUESTDATE = w.REQUESTDATE,
                        REQUESTTIME = w.REQUESTTIME,
                        TASKDESC = w.TASKDESC,
                        DELAYDESC = w.DELAYDESC,
                        NOTES = w.NOTES,
                        WOTYPE = w.WOTYPE,
                        STATUS = w.STATUS,
                        COMPLETIONDATE = w.COMPLETIONDATE,
                        COMPLETIONTIME = w.COMPLETIONTIME,
                        EQNUM = we.EQNUM
                    })
                    .Where(c => string.IsNullOrEmpty(objWorkOrderSearch.WONUM) || c.WONUM.ToUpper().Contains(objWorkOrderSearch.WONUM.ToUpper()))
                    .Where(c => c.CLOSEDATE >= objWorkOrderSearch.CLOSEDATEGT || objWorkOrderSearch.CLOSEDATEGT == DateTime.MinValue)
                    .Where(c => c.CLOSEDATE <= objWorkOrderSearch.CLOSEDATELT || objWorkOrderSearch.CLOSEDATELT == DateTime.MinValue)
                    .Where(c => string.IsNullOrEmpty(objWorkOrderSearch.TASKDESC) || c.TASKDESC.ToUpper().Contains(objWorkOrderSearch.TASKDESC.ToUpper()))
                    .Where(c => WOTYPEList.Contains(strEmptyString) || WOTYPEList.Contains(c.WOTYPE))
                    .Where(c => string.IsNullOrEmpty(objWorkOrderSearch.ORIGINATOR) || c.ORIGINATOR.ToUpper().Contains(objWorkOrderSearch.ORIGINATOR.ToUpper()))
                    //.Where(c => string.IsNullOrEmpty(objWorkOrderSearch.PRIORITY) || (int)c.w.PRIORITY == (int.TryParse(objWorkOrderSearch.PRIORITY, out intTemp) ? intTemp : 0))
                    //.Where(c => PRIORITYList.Contains(strEmptyString) || PRIORITYList.Contains(((int)c.w.PRIORITY).ToString()))
                    .Where(c => c.REQUESTDATE >= objWorkOrderSearch.REQUESTDATEGT || objWorkOrderSearch.REQUESTDATEGT == DateTime.MinValue)
                    .Where(c => c.REQUESTDATE <= objWorkOrderSearch.REQUESTDATELT || objWorkOrderSearch.REQUESTDATELT == DateTime.MinValue)
                    .Where(c => STATUSList.Contains(strEmptyString) || STATUSList.Contains(c.STATUS + string.Empty))
                    .Where(c => c.COMPLETIONDATE >= objWorkOrderSearch.COMPLETIONDATEGT || objWorkOrderSearch.COMPLETIONDATEGT == DateTime.MinValue)
                    .Where(c => c.COMPLETIONDATE <= objWorkOrderSearch.COMPLETIONDATELT || objWorkOrderSearch.COMPLETIONDATELT == DateTime.MinValue)
                    .Where(c => EQNUMList.Contains(strEmptyString) || EQNUMList.Contains(c.EQNUM))
                    .OrderBy(sortedColumns[0].PropertyName + " " + sortedColumns[0].Direction) //Uses Dynamic Linq to have sorting occur in the query
                    .Where(c => PRIORITYList.Contains(0) || PRIORITYList.Contains(Math.Truncate(c.PRIORITY.GetValueOrDefault())))
                    .Select(g => new WO
                    {
                        WONUM = g.WONUM,
                        CLOSEDATE = g.CLOSEDATE,
                        ORIGINATOR = g.ORIGINATOR,
                        PRIORITY = g.PRIORITY,
                        REQUESTDATE = g.REQUESTDATE,
                        REQUESTTIME = g.REQUESTTIME,
                        TASKDESC = g.TASKDESC,
                        DELAYDESC = g.DELAYDESC,
                        NOTES = g.NOTES,
                        WOTYPE = g.WOTYPE,
                        STATUS = g.STATUS,
                        COMPLETIONDATE = g.COMPLETIONDATE,
                        COMPLETIONTIME = g.COMPLETIONTIME
                    })
                    .Distinct();


                /*workorders = db.WOes
                    .Join(db.WOEQLISTs,
                    w => new { w.WONUM, CloseDate = w.CLOSEDATE },
                    we => new { we.WONUM, CloseDate = we.CLOSEDATE }, //needed to alter WOEQLIST table with "ALTER TABLE WOEQLIST ALTER COLUMN CLOSEDATE DATETIME CONSTRAINT ConditionRequired NOT NULL"
                    (w, we) => new { w, we })
                    .Where(c => string.IsNullOrEmpty(objWorkOrderSearch.WONUM) || c.w.WONUM.ToUpper().Contains(objWorkOrderSearch.WONUM.ToUpper()))
                    .Where(c => c.w.CLOSEDATE >= objWorkOrderSearch.CLOSEDATEGT || objWorkOrderSearch.CLOSEDATEGT == DateTime.MinValue)
                    .Where(c => c.w.CLOSEDATE <= objWorkOrderSearch.CLOSEDATELT || objWorkOrderSearch.CLOSEDATELT == DateTime.MinValue)
                    .Where(c => string.IsNullOrEmpty(objWorkOrderSearch.TASKDESC) || c.w.TASKDESC.ToUpper().Contains(objWorkOrderSearch.TASKDESC.ToUpper()))
                    .Where(c => WOTYPEList.Contains(strEmptyString) || WOTYPEList.Contains(c.w.WOTYPE.ToUpper()))
                    .Where(c => string.IsNullOrEmpty(objWorkOrderSearch.ORIGINATOR) || c.w.ORIGINATOR.ToUpper().Contains(objWorkOrderSearch.ORIGINATOR.ToUpper()))
                    //.Where(c => string.IsNullOrEmpty(objWorkOrderSearch.PRIORITY) || (int)c.w.PRIORITY == (int.TryParse(objWorkOrderSearch.PRIORITY, out intTemp) ? intTemp : 0))
                    //.Where(c => PRIORITYList.Contains(strEmptyString) || PRIORITYList.Contains(((int)c.w.PRIORITY).ToString()))
                    .Where(c => c.w.REQUESTDATE >= objWorkOrderSearch.REQUESTDATEGT || objWorkOrderSearch.REQUESTDATEGT == DateTime.MinValue)
                    .Where(c => c.w.REQUESTDATE <= objWorkOrderSearch.REQUESTDATELT || objWorkOrderSearch.REQUESTDATELT == DateTime.MinValue)
                    .Where(c => STATUSList.Contains(strEmptyString) || STATUSList.Contains(c.w.STATUS + string.Empty))
                    .Where(c => c.w.COMPLETIONDATE >= objWorkOrderSearch.COMPLETIONDATEGT || objWorkOrderSearch.COMPLETIONDATEGT == DateTime.MinValue)
                    .Where(c => c.w.COMPLETIONDATE <= objWorkOrderSearch.COMPLETIONDATELT || objWorkOrderSearch.COMPLETIONDATELT == DateTime.MinValue)
                    .Where(e => EQNUMList.Contains(strEmptyString) || EQNUMList.Contains(e.we.EQNUM.ToUpper()))
                    //.Where(e => EQNUMList.Contains(strEmptyString) || e.we.Select(n => n.EQNUM).Intersect(EQNUMList).Any())
                    //.Where(e => EQNUMList.Contains(strEmptyString) || EQNUMList.Intersect(e.we.Select(n => n.EQNUM)).Any())
                    //.Where(e => EQNUMList.Contains(strEmptyString) || e.we.Where(x => EQNUMList.Contains(x.EQNUM)).Any())
                    //.Where(e => EQNUMList.Contains(strEmptyString) || e.we.Any(l => EQNUMList.Contains(l.EQNUM)))
                    .Select(c => c.w)
                    .OrderBy(sortedColumns[0].PropertyName + " " + sortedColumns[0].Direction) //Uses Dynamic Linq to have sorting occur in the query
                    .Select(g => new WO
                    {
                        WONUM = g.WONUM,
                        CLOSEDATE = g.CLOSEDATE,
                        ORIGINATOR = g.ORIGINATOR,
                        PRIORITY = g.PRIORITY,
                        REQUESTDATE = g.REQUESTDATE,
                        REQUESTTIME = g.REQUESTTIME,
                        TASKDESC = g.TASKDESC,
                        DELAYDESC = g.DELAYDESC,
                        NOTES = g.NOTES,
                        WOTYPE = g.WOTYPE,
                        STATUS = g.STATUS,
                        COMPLETIONDATE = g.COMPLETIONDATE,
                        COMPLETIONTIME = g.COMPLETIONTIME
                    })
                    //.Distinct() //no longer necessary with GroupJoin...
                    //.Where(c => Math.Truncate(c.PRIORITY.GetValueOrDefault()) == 1)
                    .Where(c => PRIORITYList.Contains(0) || PRIORITYList.Contains(Math.Truncate(c.PRIORITY.GetValueOrDefault())));
                    //.ToList()//my pagination didn't work properly without this; some problem with .skip call directly to the database...                                                         //couldn't use the query created by dynamic linq and instead had to use the List<t> linq...
                */

                //needed this to get the proper pagination values. by adding it here, i was hoping to optomize performance and still leverage deferred execution with the above queries
                // and the take values below...
                searchRecordCount = workorders.Count();


                IEnumerable<WO> obj;
                if (isDownloadReport)
                    obj = workorders
                        .ToList();
                else
                    obj = workorders
                        .Skip(DataTablesModel.iDisplayStart)
                        .Take(DataTablesModel.iDisplayLength)
                        .ToList();

                /*It turns out that when access is queried using any sort of aggregation operator http://allenbrowne.com/ser-63.html. I noticed this because the long notes were being stored
                 * in the database, but were truncated when accessed. I created the .RefreshNote() method which queries the database for the specific wo note so that I could retrieve the proper
                 * notes after my above complex query*/
                foreach (var wo in obj)
                    wo.RefreshNote();


                if (obj.Count() > 0)
                    AddOriginatorNames(obj);

                return obj.ToList();
            }
        }
        private static void AddOriginatorNames(IEnumerable<WO> obj)
        {
            var objQueryDefs = new QueryDefinitions();
            StringBuilder objStrBldr = new StringBuilder();


            var Emps = obj
                .GroupBy(e => e.ORIGINATOR)
                .Select(e => new Employee { 
                    emp_num = string.IsNullOrEmpty(e.Key) ? string.Empty : e.Key, 
                    OrigEmpNum = string.IsNullOrEmpty(e.Key) ? string.Empty : e.Key, 
                    name = string.Empty })
                .ToList();
            Emps //removes A or Z from the begining of the employee number...
                .ForEach(e => e.emp_num = string.IsNullOrEmpty(e.emp_num) ? string.Empty : Regex.Replace(e.emp_num, "^A|^Z", string.Empty));
            Emps //removes leading zeros from the employee number
                .ForEach(e => e.emp_num = string.IsNullOrEmpty(e.emp_num) ? string.Empty : Regex.Replace(e.emp_num, "^0+(?!$)", string.Empty));
            Emps //pad left for lookup in Syteline
                .ForEach(e => e.emp_num = string.IsNullOrEmpty(e.emp_num) ? string.Empty : e.emp_num.PadLeft(7, ' '));

            objStrBldr.Clear();
            foreach (var objEmp in Emps)
                objStrBldr.Append("'" + objEmp.emp_num + "', ");

            var strSQL = objQueryDefs.GetQuery("SelectSLEmployeesByList", new string[] { objStrBldr.Remove(objStrBldr.Length - 2, 2).ToString() });
            using (var SLDb = new SytelineDbEntities())
            {
                var SLEmps = SLDb.Database
                    .SqlQuery<Employee>(strSQL);

                foreach (var objEmp in Emps)
                    objEmp.name = SLEmps
                        .Where(e => e.emp_num.Equals(objEmp.emp_num))
                        .DefaultIfEmpty(new Employee { name = "Not Found"})
                        .SingleOrDefault()
                        .name;
            }

            foreach (var objWO in obj)
            {
                if (string.IsNullOrEmpty(objWO.ORIGINATOR))
                    objWO.ORIGINATORName = "Originator Not Listed";
                else
                    objWO.ORIGINATORName = Emps
                    .Where(e => e.OrigEmpNum.Equals(objWO.ORIGINATOR))
                    .DefaultIfEmpty(new Employee { name = "Not Found" })
                    .SingleOrDefault().name;

            }
                
                
        }
    }
}