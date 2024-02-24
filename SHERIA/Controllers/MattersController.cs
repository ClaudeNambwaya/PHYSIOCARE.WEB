﻿ using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SHERIA.Helpers;
using SHERIA.Models;
using static SHERIA.Helpers.CryptoHelper;
using System.Collections;
using System.Data;
using static SHERIA.Controllers.ClientManagementController;

namespace SHERIA.Controllers
{
    public class MattersController : Controller
    {
        private IWebHostEnvironment ihostingenvironment;
        private ILoggerManager iloggermanager;
        private DBHandler dbhandler;

        public MattersController(ILoggerManager logger, IWebHostEnvironment environment, DBHandler mydbhandler)
        {
            iloggermanager = logger;
            ihostingenvironment = environment;
            dbhandler = mydbhandler;
        }

        public class matters_record
        {
            public Int64 id { set; get; }
            public string? matter_name { set; get; }
            public string? matter_number { set; get; }
            public Int64 assigned_to { set; get; }
            public Int64 client_id { set; get; }
            public DateTime start_date { set; get; }
            public DateTime close_date { set; get; }
            public string? practice_area { set; get; }
            public string? matter_status { set; get; }
            public string? matter_billing { set; get; }
            public string? description { set; get; }
        }

        public IActionResult OpenMatters()
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
        public IActionResult ClosedMatters()
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
        public ActionResult CreateMatters(matters_record record)
        {
            processingresponse response = new processingresponse
            {
                system_ref = DateTime.Now.ToString("yyyyMMddHHmmssfff")
            };

            if (HttpContext.Session.GetString("name") == null)
                return RedirectToAction("AdminLogin", "AppAuth");
            else
            {
                if (record.matter_name == null)
                    return Content("Invalid value");

                if (record.matter_number == null)
                    return Content("Invalid value");

                if (record.matter_status == null)
                    return Content("Invalid value");

                try
                {
                    MattersModel existingrecord = dbhandler.GetMattersRecord().Find(mymodel => mymodel.id == record.id)!;
                    if (existingrecord != null)
                    {
                        MattersModel mymodel = new MattersModel
                        {
                            id = existingrecord.id,
                            matter_name = record.matter_name,
                            matter_number = record.matter_number,
                            assigned_to = record.assigned_to,
                            client_id = record.client_id,
                            start_date = record.start_date,
                            close_date = record.close_date,
                            practice_area = record.practice_area,
                            matter_status = record.matter_status,
                            matter_billing = record.matter_billing,
                            description = record.description,
                        };

                        if (dbhandler.UpdateMatterRecord(mymodel))
                        {
                            CaptureAuditTrail("Updated matters", "Updated matters: " + mymodel.matter_name);
                            ModelState.Clear();
                            response.error_code = "00";
                            response.error_desc = "Updated matters successfully ";
                        }
                        else
                        {
                            response.error_code = "01";
                            response.error_desc = "Could not Updated matters, kindly contact system admin ";
                        }
                    }
                    else
                    {
                        MattersModel mymodel = new MattersModel
                        {
                            matter_name = record.matter_name,
                            matter_number = record.matter_number,
                            assigned_to = record.assigned_to,
                            client_id = record.client_id,
                            start_date = record.start_date,
                            close_date = record.close_date,
                            practice_area = record.practice_area,
                            matter_status = record.matter_status,
                            matter_billing = record.matter_billing,
                            description = record.description,
                            created_by = Convert.ToInt16(HttpContext.Session.GetString("userid"))
                        };

                        if (dbhandler.AddMattersRecord(mymodel))
                        {
                            CaptureAuditTrail("Created matters", "Created matters: " + mymodel.matter_name);
                            ModelState.Clear();
                            response.error_code = "00";
                            response.error_desc = "Matter successfully created";
                        }
                        else
                        {
                            response.error_code = "01";
                            response.error_desc = "Could not Create matters, kindly contact system admin";
                        }
                            
                    }
                }
                catch
                {
                    response.error_code = "01";
                    response.error_desc = "Could not Create matters, kindly contact system admin";
                }
            }
            return Content(JsonConvert.SerializeObject(response, Formatting.Indented), "application/json");
        }

        [HttpPost]
        public ActionResult UpdateMatters(Int64 id, string module)
        {
            if (HttpContext.Session.GetString("name") == null )
                return RedirectToAction("AdminLogin", "AppAuth");
            else
            {
                switch (module)
                {
                    case "open_matter_status":
                        if (dbhandler.Update_Open_Matter_Status(id, module))
                        {
                            return Content("Success");

                        }
                        break;
                    case "close_matter_status":
                        if (dbhandler.Update_Open_Matter_Status(id, module))
                        {
                            return Content("Success");

                        }
                        break;
                    default:
                        break;
                }

                return Content("Fail");
            }
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
                    case "open_matters":
                        MattersModel mattermodel = dbhandler.GetMattersRecord().Find(mymodel => mymodel.id == id)!;
                        if (mattermodel != null)
                        {
                            dbhandler.DeleteRecord(id, Convert.ToInt16(HttpContext.Session.GetString("userid")), module);
                            CaptureAuditTrail("Deleted partner", "Deleted partner: " + mattermodel.matter_number);
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
