﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using PHYSIOCARE.Models;
using SHERIA.Helpers;
using SHERIA.Models;
using static SHERIA.Controllers.ClientManagementController;
using static SHERIA.Helpers.CryptoHelper;
using System.Collections;
using System.Data;

namespace PHYSIOCARE.Controllers
{
    public class TreatmentPlanController : Controller
    {
        private IWebHostEnvironment ihostingenvironment;
        private ILoggerManager iloggermanager;
        private DBHandler dbhandler;

        public TreatmentPlanController(ILoggerManager logger, IWebHostEnvironment environment, DBHandler mydbhandler)
        {
            iloggermanager = logger;
            ihostingenvironment = environment;
            dbhandler = mydbhandler;
        }

        public class treatment_plan_record
        {
            public Int64 id { set; get; }
            public Int64 client_id { get; set; }
            public DateTime treatment_plan_date { get; set; }
            public string? remarks { get; set; }
        }

        public IActionResult Treatment()
        {
            if (HttpContext.Session.GetString("name") == null)
                return RedirectToAction("AdminLogin", "AppAuth");
            else
            {
                ViewBag.MenuLayout = HttpContext.Session.GetString("menulayout");
                MenuHandler menuhandler = new MenuHandler(dbhandler);
                IEnumerable<MenuModel> menu = menuhandler.GetMenu(Convert.ToInt16(HttpContext.Session.GetString("profileid")), HttpContext.Request.Path);
                return View(menu);
            }
        }


        [HttpPost]
        public ActionResult CreateTreatmentPlan(treatment_plan_record record)
        {
            processingresponse response = new processingresponse
            {
                system_ref = DateTime.Now.ToString("yyyyMMddHHmmssfff")
            };

            if (HttpContext.Session.GetString("name") == null)
                return RedirectToAction("AdminLogin", "AppAuth");
            else
            {
                if (record.client_id == 0)
                    return Content("Invalid client");

                try
                {
                    TreatmentPlanModel existingrecord = dbhandler.GetTreatmentPlan().Find(mymodel => mymodel.id == record.id)!;
                    if (existingrecord != null)
                    {
                        TreatmentPlanModel mymodel = new TreatmentPlanModel
                        {
                            id = existingrecord.id,
                            client_id = record.client_id,
                            treatment_plan_date = record.treatment_plan_date,
                            remarks = record.remarks,
                        };

                        if (dbhandler.UpdateTreatmentPlan(mymodel))
                        {
                            CaptureAuditTrail("Updated treatment plan", "Updated treatment plan: " + mymodel.client_id);
                            ModelState.Clear();
                            response.error_code = "00";
                            response.error_desc = "Updated treatment plan successfully ";
                        }
                        else
                        {
                            response.error_code = "01";
                            response.error_desc = "Could not Update treatment plan, kindly contact system admin ";
                        }
                    }
                    else
                    {
                        TreatmentPlanModel mymodel = new TreatmentPlanModel
                        {
                            client_id = record.client_id,
                            treatment_plan_date = record.treatment_plan_date,
                            remarks = record.remarks,
                            created_by = Convert.ToInt16(HttpContext.Session.GetString("userid"))
                        };

                        if (dbhandler.AddTreatmentPlan(mymodel))
                        {
                            CaptureAuditTrail("Created treatment plan", "Created traetment plan: " + mymodel.client_id);
                            ModelState.Clear();
                            response.error_code = "00";
                            response.error_desc = "treatment plan successfully created";
                        }
                        else
                        {
                            response.error_code = "01";
                            response.error_desc = "Could not Create treatment plan, kindly contact system admin";
                        }

                    }
                }
                catch
                {
                    response.error_code = "01";
                    response.error_desc = "Could not Create diagnosis, kindly contact system admin";
                }
            }
            return Content(JsonConvert.SerializeObject(response, Formatting.Indented), "application/json");
        }




        [HttpGet]
        public ContentResult GetRecords(string module, string param = "normal")
        {
            FinpayiSecurity.CryptoFactory CryptographyFactory = new FinpayiSecurity.CryptoFactory();
            FinpayiSecurity.ICrypto Cryptographer = CryptographyFactory.MakeCryptographer("rijndael");
            ArrayList details = new ArrayList();
            DataTable datatable = new DataTable();
            DataTable datatableI = new DataTable();
            //System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            JObject jobject = new JObject();
            JArray jarray = new JArray();
            JArray option_array = new JArray();

            switch (module)
            {

                default:
                    datatable = dbhandler.GetRecords(module);
                    break;
            }

            if (datatable.Rows.Count > 0)
            {
                foreach (DataRow dr in datatable.Rows)
                {
                    row = new Dictionary<string, object>();
                    foreach (DataColumn col in datatable.Columns)
                    {
                        row.Add(col.ColumnName, dr[col]);
                    }
                    rows.Add(row);
                }
            }
            return Content(JsonConvert.SerializeObject(rows, Formatting.Indented) /*serializer.Serialize(rows)*/, "application/json");
        }

        public bool CaptureAuditTrail(string action_type, string action_description)
        {
            AuditTrailModel audittrailmodel = new AuditTrailModel
            {
                user_name = HttpContext.Session.GetString("email")!.ToString(),
                action_type = action_type,
                action_description = action_description,
                page_accessed = String.Format("{0}://{1}{2}{3}", HttpContext.Request.Scheme, HttpContext.Request.Host, HttpContext.Request.Path, HttpContext.Request.QueryString), /*Request.Url.ToString(),*/
                //client_ip_address = GetIPAddress(HttpContext), //Request.HttpContext.Connection.RemoteIpAddress.ToString(), /*Request.UserHostAddress,*/
                session_id = HttpContext.Session.Id //HttpContext.Session.GetString("userid") /*Session.SessionID*/
            };
            return dbhandler.AddAuditTrail(audittrailmodel);
        }

        [RBAC]
        public ActionResult Delete(/*[FromBody] JObject jobject*/ int id, string module)
        {

            if (HttpContext.Session.GetString("name") == null)
                return RedirectToAction("AdminLogin", "AppAuth");
            else
            {
                switch (module)
                {
                    case "treatment_plan_record":
                        TreatmentPlanModel treatment_plan_model = dbhandler.GetTreatmentPlan().Find(mymodel => mymodel.id == id)!;
                        if (treatment_plan_model != null)
                        {
                            dbhandler.DeleteRecord(id, Convert.ToInt16(HttpContext.Session.GetString("userid")), module);
                            CaptureAuditTrail("Deleted treatmant plan record", "Deleted treatment record: " + treatment_plan_model.client_id);
                        }
                        break;


                    default:
                        break;
                }

                return GetRecords(module);
            }
        }
    }
}