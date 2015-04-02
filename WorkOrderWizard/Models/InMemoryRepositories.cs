using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Collections.ObjectModel;
using System.Data.OleDb;


namespace WorkOrderWizard.Models
{
    public static class InMemoryWorkOrdersRepository
    {
        public static IList<WorkOrder> AllWorkOrders { get; set; }

        public static IList<WorkOrder> GetWorkOrders(int MaxRecordCount, out int totalRecordCount, out int searchRecordCount, JQueryDataTablesModel DataTablesModel, bool isDownloadReport = false)
        {
            //MP2_DataBaseSettings db = new MP2_DataBaseSettings();

            ReadOnlyCollection<SortedColumn> sortedColumns = DataTablesModel.GetSortedColumns();
            IList<WorkOrder> workorders;
            DateTime dtmTemp;
            int intTemp;
            string[] objResults;
            WorkOrderSearch objWorkOrderSearch;


            totalRecordCount = AllWorkOrders.Count;

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
                            objWorkOrderSearch.WONUM = DataTablesModel.sSearch_[intCounter];
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
                            objWorkOrderSearch.PRIORITY = DataTablesModel.sSearch_[intCounter];
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
                    }
                }
            }


            /*The Below was created because the Entity Framework had a problem doing a filter of a list with a list because of the difficulty it had using deferred execution and the corresponding sql creation*/
            var strEmptyString = "EMPTY";
            var WOTYPEList = objWorkOrderSearch.WOTYPES == null ? new[] { strEmptyString } : objWorkOrderSearch.WOTYPES.ToArray<string>();
            var STATUSList = objWorkOrderSearch.STATUSES == null ? new[] { strEmptyString } : objWorkOrderSearch.STATUSES.ToArray<string>();
            var EQNUMList = objWorkOrderSearch.EQNUMS == null ? new[] { strEmptyString } : objWorkOrderSearch.EQNUMS.ToArray<string>();

            //bool blnProcessed = false;
            //if (objVendorRequestSearch.Processed != null && !objVendorRequestSearch.Processed.ToUpper().Equals("BOTH"))
            //    blnProcessed = bool.TryParse(objVendorRequestSearch.Processed, out blnProcessed) ? blnProcessed : false;

            if (isDownloadReport)
                workorders = AllWorkOrders
                    .Where(c => string.IsNullOrEmpty(objWorkOrderSearch.WONUM) || c.WONUM.ToUpper().Contains(objWorkOrderSearch.WONUM.ToUpper()))
                    .Where(c => c.CLOSEDATE >= objWorkOrderSearch.CLOSEDATEGT || objWorkOrderSearch.CLOSEDATEGT == DateTime.MinValue)
                    .Where(c => c.CLOSEDATE <= objWorkOrderSearch.CLOSEDATELT || objWorkOrderSearch.CLOSEDATELT == DateTime.MinValue)
                    .Where(c => string.IsNullOrEmpty(objWorkOrderSearch.TASKDESC) || c.TASKDESC.ToUpper().Contains(objWorkOrderSearch.TASKDESC.ToUpper()))
                    .Where(c => WOTYPEList.Contains(strEmptyString) || WOTYPEList.Contains(c.WOTYPE))
                    .Where(c => string.IsNullOrEmpty(objWorkOrderSearch.ORIGINATOR) || c.ORIGINATOR.ToUpper().Contains(objWorkOrderSearch.ORIGINATOR.ToUpper()))
                    .Where(c => string.IsNullOrEmpty(objWorkOrderSearch.PRIORITY) ||  c.PRIORITY == (int.TryParse(objWorkOrderSearch.PRIORITY, out intTemp) ? intTemp: 0))
                    .Where(c => c.REQUESTDATE >= objWorkOrderSearch.REQUESTDATEGT || objWorkOrderSearch.REQUESTDATEGT == DateTime.MinValue)
                    .Where(c => c.REQUESTDATE <= objWorkOrderSearch.REQUESTDATELT || objWorkOrderSearch.REQUESTDATELT == DateTime.MinValue)
                    .Where(c => STATUSList.Contains(strEmptyString) || STATUSList.Contains(c.STATUS))
                    .Where(c => EQNUMList.Contains(strEmptyString) || c.WOEQLIST.Select(n => n.EQNUM).Intersect(EQNUMList).Any())
                    .ToList();
            else
                workorders = AllWorkOrders
                    .Where(c => string.IsNullOrEmpty(objWorkOrderSearch.WONUM) || c.WONUM.ToUpper().Contains(objWorkOrderSearch.WONUM.ToUpper()))
                    .Where(c => c.CLOSEDATE >= objWorkOrderSearch.CLOSEDATEGT || objWorkOrderSearch.CLOSEDATEGT == DateTime.MinValue)
                    .Where(c => c.CLOSEDATE <= objWorkOrderSearch.CLOSEDATELT || objWorkOrderSearch.CLOSEDATELT == DateTime.MinValue)
                    .Where(c => string.IsNullOrEmpty(objWorkOrderSearch.TASKDESC) || c.TASKDESC.ToUpper().Contains(objWorkOrderSearch.TASKDESC.ToUpper()))
                    .Where(c => WOTYPEList.Contains(strEmptyString) || WOTYPEList.Contains(c.WOTYPE))
                    .Where(c => string.IsNullOrEmpty(objWorkOrderSearch.ORIGINATOR) || c.ORIGINATOR.ToUpper().Contains(objWorkOrderSearch.ORIGINATOR.ToUpper()))
                    .Where(c => string.IsNullOrEmpty(objWorkOrderSearch.PRIORITY) || c.PRIORITY == (int.TryParse(objWorkOrderSearch.PRIORITY, out intTemp) ? intTemp : 0))
                    .Where(c => c.REQUESTDATE >= objWorkOrderSearch.REQUESTDATEGT || objWorkOrderSearch.REQUESTDATEGT == DateTime.MinValue)
                    .Where(c => c.REQUESTDATE <= objWorkOrderSearch.REQUESTDATELT || objWorkOrderSearch.REQUESTDATELT == DateTime.MinValue)
                    .Where(c => STATUSList.Contains(strEmptyString) || STATUSList.Contains(c.STATUS))
                    .Where(c => EQNUMList.Contains(strEmptyString) || c.WOEQLIST.Select(n => n.EQNUM).Intersect(EQNUMList).Any())
                    .OrderByDescending(c => c.WONUM)
                    .Take(MaxRecordCount)
                    .ToList();

            searchRecordCount = workorders.Count;

            IOrderedEnumerable<WorkOrder> sortedList = null;
            foreach (var sortedColumn in sortedColumns)
            {
                switch (sortedColumn.PropertyName)
                {
                    case "WONUM":
                        sortedList = sortedList == null ? workorders.CustomSort(sortedColumn.Direction, i => i.WONUM)
                            : sortedList.CustomSort(sortedColumn.Direction, i => i.WONUM);
                        break;
                    case "CLOSEDATE":
                        sortedList = sortedList == null ? workorders.CustomSort(sortedColumn.Direction, i => i.CLOSEDATE)
                            : sortedList.CustomSort(sortedColumn.Direction, i => i.CLOSEDATE);
                        break;
                    case "TASKDESC":
                        sortedList = sortedList == null ? workorders.CustomSort(sortedColumn.Direction, i => i.TASKDESC)
                            : sortedList.CustomSort(sortedColumn.Direction, i => i.TASKDESC);
                        break;
                    case "WOTYPE":
                        sortedList = sortedList == null ? workorders.CustomSort(sortedColumn.Direction, i => i.WOTYPE)
                            : sortedList.CustomSort(sortedColumn.Direction, i => i.WOTYPE);
                        break;
                    case "ORIGINATOR":
                        sortedList = sortedList == null ? workorders.CustomSort(sortedColumn.Direction, i => i.ORIGINATOR)
                            : sortedList.CustomSort(sortedColumn.Direction, i => i.ORIGINATOR);
                        break;
                    case "PRIORITY":
                        sortedList = sortedList == null ? workorders.CustomSort(sortedColumn.Direction, i => i.PRIORITY)
                            : sortedList.CustomSort(sortedColumn.Direction, i => i.PRIORITY);
                        break;
                    case "REQUESTTIME":
                        sortedList = sortedList == null ? workorders.CustomSort(sortedColumn.Direction, i => i.REQUESTTIME)
                            : sortedList.CustomSort(sortedColumn.Direction, i => i.REQUESTTIME);
                        break;
                    case "REQUESTDATE":
                        sortedList = sortedList == null ? workorders.CustomSort(sortedColumn.Direction, i => i.REQUESTDATE)
                            : sortedList.CustomSort(sortedColumn.Direction, i => i.REQUESTDATE);
                        break;
                    case "STATUS":
                        sortedList = sortedList == null ? workorders.CustomSort(sortedColumn.Direction, i => i.STATUS)
                            : sortedList.CustomSort(sortedColumn.Direction, i => i.STATUS);
                        break;
                    default://This took care of the below scenario where the default sorted column was 0 which is my drill down image. I could have used 'case "0":' but just made a default case instead...
                        sortedList = sortedList == null ? workorders.CustomSort(sortedColumn.Direction, i => i.WONUM)
                            : sortedList;
                        break;
                }
            }


            if (isDownloadReport)
                return sortedList.ToList();
            else
                if (DataTablesModel.iDisplayLength == -1)
                    return sortedList.Skip(DataTablesModel.iDisplayStart).ToList();
                else
                    return sortedList.Skip(DataTablesModel.iDisplayStart).Take(DataTablesModel.iDisplayLength).ToList();

        }
    }
}