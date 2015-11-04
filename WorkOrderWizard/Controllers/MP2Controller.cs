using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using WorkOrderWizard.Models;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using Microsoft.Reporting.WebForms;


namespace WorkOrderWizard.Controllers
{
    public class MP2Controller : Controller
    {

        private IList<WO> WorkOrderCollection { get; set; }


        [HttpGet]
        public JsonResult WorkOrderTypes()
        {
            List<WOTYPE> objWOTypeList;


            using (var db = new mp250dbDB())
            {
                objWOTypeList = db.WOTYPEs
                    .Where(w => !w.DESCRIPTION.Contains("do not use!"))
                    .ToList();
            }

            var result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            result.Data = objWOTypeList;

            return result;
        }

        [HttpGet]
        public JsonResult EquipmentList()
        {
            List<EQUIP> objEquipmentList;


            using (var db = new mp250dbDB())
            {
                objEquipmentList = db.EQUIPs
                    .Where(e => e.INSERVICE.Equals("Y"))
                    .ToList();
            }

            var result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            result.Data = objEquipmentList;

            return result;
        }

        [HttpPost]
        public JsonResult GetSLEmployee(string EmployeeID)
        {
            var db = new SytelineDbEntities();

            var objQueryDefs = new QueryDefinitions();
            var strSQL = objQueryDefs.GetQuery("SelectSLEmployeeByID", new string[] { EmployeeID.PadLeft(7) });
            var objSLEmployee = db.Database.SqlQuery<Employee>(strSQL)
                .DefaultIfEmpty(new Employee { EmployeeFound = false })
                .FirstOrDefault();

            var result = new JsonResult();
            result.Data = objSLEmployee;

            return result;
        }

        [HttpPost]
        public JsonResult CreateWorkOrder()
        {
            WO objWorkOrder;
            var result = new JsonResult();
            var objPostedData = Request.InputStream;
            var strPostedData = new StreamReader(objPostedData).ReadToEnd();


            try
            {
                objWorkOrder = JsonConvert.DeserializeObject<WO>(strPostedData);
                objWorkOrder.WONUM = objWorkOrder.GetNextWorkOrderNum(); //The work order number must be set before populating from post data,  
                objWorkOrder.PopulateFromPostVariables();                //else the WONUM won't beavailable when adding the equipment to the WO from the post stream
            }
            catch (Exception objEx)
            {
                result.Data = new { Success = false, Message = objEx.Message };
                return result;
            }


            
            var objQueryResultList = objWorkOrder.AddWorkOrder();

            var objStrBldr = new StringBuilder();
            foreach (var objQueryResult in objQueryResultList)
                objStrBldr.Append(objQueryResult.Message);

            if (string.IsNullOrEmpty(objStrBldr.ToString()))
                result.Data = new { Success = true, Message = "Work Order " + objWorkOrder.WONUM + " was successfully created." };
            else
                result.Data = new { Success = false, Message = objStrBldr.ToString() };
            

            return result;
        }

        [HttpPost]
        public JsonResult GetWorkOrders(JQueryDataTablesModel jQueryDataTablesModel)
        {

            int totalRecordCount, searchRecordCount;
            var result = new JsonResult();


            var objItems = InMemoryWorkOrdersRepository.GetWorkOrders(searchRecordCount: out searchRecordCount, DataTablesModel: jQueryDataTablesModel);

            

            //result.MaxJsonLength = Int32.MaxValue;  //took care of the error "Error during serialization or deserialization using the JSON JavaScriptSerializer. The length of the string exceeds the value set on the maxJsonLength property"
            result.Data = new                       //that occurred when pagination was disabled. I reenabled pagination 
            {
                iTotalRecords = WOes.TotalRecordCount, // iTotalRecords = InMemoryWorkOrdersRepository.AllWorkOrders.TotalRecordCount,
                jQueryDataTablesModel.sEcho,
                iTotalDisplayRecords = searchRecordCount,
                aaData = objItems
            };

            return result;
        }

        [HttpGet]
        public void GetWorkOrders(JQueryDataTablesModel jQueryDataTablesModel, string Format = "Excel")
        {
            int totalRecordCount, searchRecordCount;


            WorkOrderCollection = InMemoryWorkOrdersRepository.GetWorkOrders(searchRecordCount: out searchRecordCount, DataTablesModel: jQueryDataTablesModel, isDownloadReport: true);

            switch (Format)
            {
                case "WOExcel":
                    RenderWorkOrdeExcelReport();
                    break;
                case "WOEquipExcel":
                    RenderWorkOrderEquipExcelReport();
                    break;
                case "WOCSV":
                    RenderWorkOrderCSVReport();
                    break;
                case "WOEquipCSV":
                    RenderWorkOrderEquipCSVReport();
                    break;
                default:
                    RenderWorkOrderCSVReport();
                    break;
            }
        }

        private void RenderWorkOrderEquipCSVReport()
        {
            var objWorkOrderCSVList = new WorkOrderEquipCSVList(WorkOrderCollection);

            Response.Clear();
            Response.ContentType = "text/csv";
            Response.AddHeader("content-disposition", "attachment; filename=WorkOrderEquip" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + ".csv");
            Response.Write(objWorkOrderCSVList.ToCSVString());
            Response.End();
        }

        private void RenderWorkOrderCSVReport()
        {
            var objWorkOrderCSVList = new WorkOrderCSVList(WorkOrderCollection);

            Response.Clear();
            Response.ContentType = "text/csv";
            Response.AddHeader("content-disposition", "attachment; filename=WorkOrders" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + ".csv");
            Response.Write(objWorkOrderCSVList.ToCSVString());
            Response.End();
        }

        private void RenderWorkOrdeExcelReport()
        {
            string strReportType = "Excel";
            LocalReport objLocalReport;
            ReportDataSource WorkOrderDataSource;
            string mimeType;
            string encoding;
            string fileNameExtension;
            string deviceInfo = "";
            Warning[] warnings;
            string[] streams;

            objLocalReport = new LocalReport { ReportPath = Server.MapPath(Settings.ReportDirectory + "WorkOrders.rdlc") };

            //Give the reportdatasource a name so that we can reference it in our report designer
            WorkOrderDataSource = new ReportDataSource("WorkOrders", WorkOrderCollection);

            objLocalReport.DataSources.Add(WorkOrderDataSource);
            objLocalReport.Refresh();

            //The DeviceInfo settings should be changed based on the reportType
            //http://msdn2.microsoft.com/en-us/library/ms155397.aspx
            deviceInfo = string.Format(
                        "<DeviceInfo>" +
                        "<OmitDocumentMap>True</OmitDocumentMap>" +
                        "<OmitFormulas>True</OmitFormulas>" +
                        "<SimplePageHeaders>True</SimplePageHeaders>" +
                        "</DeviceInfo>", strReportType);

            //Render the report
            var renderedBytes = objLocalReport.Render(
                strReportType,
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);

            //Clear the response stream and write the bytes to the outputstream
            //Set content-disposition to "attachment" so that user is prompted to take an action
            //on the file (open or save)
            Response.Clear();
            Response.ClearHeaders();
            Response.ClearContent();
            Response.ContentType = mimeType;
            Response.AddHeader("content-disposition", "attachment; filename=WorkOrders" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + "." + fileNameExtension);
            Response.BinaryWrite(renderedBytes);
            Response.End();
        }

        private void RenderWorkOrderEquipExcelReport()
        {
            string strReportType = "Excel";
            LocalReport objLocalReport;
            ReportDataSource WorkOrderDataSource;
            string mimeType;
            string encoding;
            string fileNameExtension;
            string deviceInfo = "";
            Warning[] warnings;
            string[] streams;

            objLocalReport = new LocalReport { ReportPath = Server.MapPath(Settings.ReportDirectory + "WorkOrderEquipment.rdlc") };

            objLocalReport.SubreportProcessing += new SubreportProcessingEventHandler(MySubreportEventHandler);

            //Give the reportdatasource a name so that we can reference it in our report designer
            WorkOrderDataSource = new ReportDataSource("WorkOrders", WorkOrderCollection);

            objLocalReport.DataSources.Add(WorkOrderDataSource);
            objLocalReport.Refresh();

            //The DeviceInfo settings should be changed based on the reportType
            //http://msdn2.microsoft.com/en-us/library/ms155397.aspx
            deviceInfo = string.Format(
                        "<DeviceInfo>" +
                        "<OmitDocumentMap>True</OmitDocumentMap>" +
                        "<OmitFormulas>True</OmitFormulas>" +
                        "<SimplePageHeaders>True</SimplePageHeaders>" +
                        "</DeviceInfo>", strReportType);

            //Render the report
            var renderedBytes = objLocalReport.Render(
                strReportType,
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings);

            //Clear the response stream and write the bytes to the outputstream
            //Set content-disposition to "attachment" so that user is prompted to take an action
            //on the file (open or save)
            Response.Clear();
            Response.ClearHeaders();
            Response.ClearContent();
            Response.ContentType = mimeType;
            Response.AddHeader("content-disposition", "attachment; filename=WorkOrderEquip" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + "." + fileNameExtension);
            Response.BinaryWrite(renderedBytes);
            Response.End();
        }

        private void MySubreportEventHandler(object sender, SubreportProcessingEventArgs e)
        {
            DateTime dtmTemp;


            var objWONumParam = e.Parameters.Where(p => p.Name.Equals("WONum"))
                .SingleOrDefault()
                .Values[0];
            var objCloseDateParam = e.Parameters.Where(p => p.Name.Equals("CloseDate"))
                .SingleOrDefault()
                .Values[0];

            var dtmCloseDate = DateTime.TryParse(objCloseDateParam, out dtmTemp) ? dtmTemp : SharedVariables.MINDATE;


            //WorkOrderCollection
            var WOEquipment = WorkOrderCollection
                .Where(w => w.WONUM == objWONumParam)
                .Where(w => w.CLOSEDATE == dtmCloseDate)
                .SingleOrDefault();

            e.DataSources.Add(new ReportDataSource("WorkOrderEquipment", WOEquipment.WOEQLIST));
        }

        [HttpPost]
        public JsonResult UpdateWONote(string WONUM, string CloseDate, string NoteContent)
        {
            WO objWorkOrder;
            bool blnResult;


            var CLOSEDATE = CloseDate.GetDateTimeFromJSON();

            objWorkOrder = new WO() { WONUM = WONUM, CLOSEDATE = CLOSEDATE, NOTES = NoteContent };
            blnResult = objWorkOrder.UpdateWONote();

            using (var db = new mp250dbDB())
            {
                objWorkOrder = db.WOes
                    .Where(w => w.WONUM == WONUM && w.CLOSEDATE == CLOSEDATE)
                    .Single();
            }

            return Json(new { Success = blnResult, objWorkOrder.HTMLWONotes });
        }
    }
}
