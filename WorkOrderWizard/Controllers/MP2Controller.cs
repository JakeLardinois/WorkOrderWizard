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
        //private WorkOrders mWorkOrders
        //{
        //    get { return Session["mWorkOrders"] as WorkOrders; }
        //    set { Session["mWorkOrders"] = value; }
        //}

        //protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        //{
        //    base.Initialize(requestContext);
        //}

        [HttpGet]
        public JsonResult WorkOrderTypes()
        {
            //JavaScriptSerializer serializer;
            var objWOTypeList = new WorkOrderTypes();


            //return JsonConvert.SerializeObject(WOTypeList);


            var result = new JsonResult();
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            result.Data = objWOTypeList;

            return result;
        }

        [HttpGet]
        public JsonResult EquipmentList()
        {
            var objEquipmentList = new EquipmentList();
            var result = new JsonResult();

            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            result.Data = objEquipmentList;

            return result;
        }

        [HttpPost]
        public JsonResult GetSLEmployee(string EmployeeID)
        {
            var db = new SytelineDbEntities();

            var strSQL = QueryDefinitions.GetQuery("SelectSLEmployeeByID", new string[] { EmployeeID.PadLeft(7) });
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
            WorkOrder objWorkOrder;
            var result = new JsonResult();
            var objPostedData = Request.InputStream;
            var strPostedData = new StreamReader(objPostedData).ReadToEnd();


            objWorkOrder = new WorkOrder();
            try
            {
                objWorkOrder = JsonConvert.DeserializeObject<WorkOrder>(strPostedData);
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
        public JsonResult GetWorkOrders(int MaxRecordCount, JQueryDataTablesModel jQueryDataTablesModel)
        {
            int totalRecordCount, searchRecordCount;
            //JQueryDataTablesModel jQueryDataTablesModel;
            var result = new JsonResult();

            
            //Populate my jQueryDataTablesModel by using the static method..
            //jQueryDataTablesModel = JQueryDataTablesModel.CreateFromPostData(Request.InputStream);

            //InMemoryWorkOrdersRepository.AllWorkOrders = new WorkOrders();
            InMemoryWorkOrdersRepository.AllWorkOrders = new WorkOrders(jQueryDataTablesModel.sSearch);
            

            var objItems = InMemoryWorkOrdersRepository.GetWorkOrders(MaxRecordCount,
                searchRecordCount: out searchRecordCount, DataTablesModel: jQueryDataTablesModel);

            result.Data = new
            {
                iTotalRecords = InMemoryWorkOrdersRepository.AllWorkOrders.TotalRecordCount,
                jQueryDataTablesModel.sEcho,
                iTotalDisplayRecords = searchRecordCount,
                aaData = objItems
            };

            return result;
        }

        [HttpGet]
        public ActionResult GetWorkOrders(JQueryDataTablesModel jQueryDataTablesModel)
        {
            int totalRecordCount, searchRecordCount;

            InMemoryWorkOrdersRepository.AllWorkOrders = new WorkOrders(jQueryDataTablesModel.sSearch);

            var objItems = InMemoryWorkOrdersRepository.GetWorkOrders(0, //MaxRecordCount get ignored when isDownloadReport is True...
                searchRecordCount: out searchRecordCount, DataTablesModel: jQueryDataTablesModel, isDownloadReport: true);

            RenderWorkOrderReport(objItems);
            return View();  //note that this is never reached since RenderWorkOrderReport writes to the response stream
        }

        private void RenderWorkOrderReport(IList<WorkOrder> objItems)
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
            WorkOrderDataSource = new ReportDataSource("WorkOrders", objItems);

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

        [HttpPost]
        public JsonResult UpdateWONote(string WONUM, string CloseDate, string NoteContent)
        {
            WorkOrder objWorkOrder;
            bool blnResult;

            var CLOSEDATE = CloseDate.GetDateTimeFromJSON();

            objWorkOrder = new WorkOrder() { WONUM = WONUM, CLOSEDATE = CLOSEDATE, WONotes = NoteContent };
            blnResult = objWorkOrder.UpdateWONote();

            objWorkOrder = new WorkOrder(WONUM, CLOSEDATE);

            return Json(new { Success = blnResult, objWorkOrder.HTMLWONotes });
            //return new JsonResult();
        }
    }
}
