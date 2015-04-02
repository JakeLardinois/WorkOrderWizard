using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using WorkOrderWizard.Models;
using Newtonsoft.Json;
using System.IO;
using System.Text;


namespace WorkOrderWizard.Controllers
{
    public class MP2Controller : Controller
    {


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

            var objSLEmployee = db.Database.SqlQuery<Employee>(QueryDefinitions.GetQuery("SelectSLEmployeeByID", new string[] { EmployeeID.PadLeft(7) }))
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
        public JsonResult GetWorkOrders(int MaxRecordCount)
        {
            int totalRecordCount, searchRecordCount, intMaxRecordCount;
            JQueryDataTablesModel jQueryDataTablesModel;
            var result = new JsonResult();


            //Populate my jQueryDataTablesModel by using the static method..
            jQueryDataTablesModel = JQueryDataTablesModel.CreateFromPostData(Request.InputStream);

            InMemoryWorkOrdersRepository.AllWorkOrders = new WorkOrders();

            var objItems = InMemoryWorkOrdersRepository.GetWorkOrders(MaxRecordCount,
                totalRecordCount: out totalRecordCount, searchRecordCount: out searchRecordCount, DataTablesModel: jQueryDataTablesModel);


            result.Data = new
            {
                iTotalRecords = totalRecordCount,
                jQueryDataTablesModel.sEcho,
                iTotalDisplayRecords = searchRecordCount,
                aaData = objItems
            };

            return result;
        }

        [HttpPost]
        public JsonResult GetWorkOrdersByParams()
        {
            var result = new JsonResult();
            return result;
        }



        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(int id)
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Edit(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Delete(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
