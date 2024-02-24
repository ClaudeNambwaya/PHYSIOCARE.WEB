using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SHERIA.Helpers;
using SHERIA.Models;
using static SHERIA.Controllers.ClientManagementController;
using static SHERIA.Helpers.CryptoHelper;
using System.Collections;
using System.Data;

namespace PHYSIOCARE.Controllers
{
    public class PatientHistoryController : Controller
    {
        private IWebHostEnvironment ihostingenvironment;
        private ILoggerManager iloggermanager;
        private DBHandler dbhandler;

        public PatientHistoryController(ILoggerManager logger, IWebHostEnvironment environment, DBHandler mydbhandler)
        {
            iloggermanager = logger;
            ihostingenvironment = environment;
            dbhandler = mydbhandler;
        }

        public class patient_history_record
        {
            public Int64 id { set; get; }
            public Int64 client_id { get; set; }
            public string? weight { get; set; }
            public string? height { get; set; }
            public string? body_mass_index { get; set; }
            public string? blood_pressure { get; set; }
            public string? pulse_rate { get; set; }
            public string? standard_operating_procedure { get; set; }
            public DateTime visit_date { get; set; }
            public DateTime next_visit_date { get; set; }
            public string? prescription { get; set; }
            public string? remarks { get; set; }
        }

        public IActionResult PatientHistory()
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
        public ActionResult CreatePatientHistory(patient_history_record record)
        {
            processingresponse response = new processingresponse
            {
                system_ref = DateTime.Now.ToString("yyyyMMddHHmmssfff")
            };

            if (HttpContext.Session.GetString("name") == null)
                return RedirectToAction("AdminLogin", "AppAuth");
            else
            {
                if(record.client_id == 0)
                    return Content("Invalid client");

                try
                {
                    PatientHistoryModel existingrecord = dbhandler.GetPatientHistory().Find(mymodel => mymodel.id == record.id)!;
                    if (existingrecord != null)
                    {
                        PatientHistoryModel mymodel = new PatientHistoryModel
                        {
                            id = existingrecord.id,
                            client_id = record.client_id,
                            weight = record.weight,
                            height = record.height,
                            body_mass_index = record.body_mass_index,
                            blood_pressure = record.blood_pressure,
                            pulse_rate = record.pulse_rate,
                            standard_operating_procedure = record.standard_operating_procedure,
                            visit_date = record.visit_date,
                            next_visit_date = record.next_visit_date,
                            prescription = record.prescription,
                            remarks = record.remarks,
                        };

                        if (dbhandler.UpdatePatientHistory(mymodel))
                        {
                            CaptureAuditTrail("Updated patient history", "Updated patient history: " + mymodel.client_id);
                            ModelState.Clear();
                            response.error_code = "00";
                            response.error_desc = "Updated patient history successfully ";
                        }
                        else
                        {
                            response.error_code = "01";
                            response.error_desc = "Could not Updated patient history, kindly contact system admin ";
                        }
                    }
                    else
                    {
                        PatientHistoryModel mymodel = new PatientHistoryModel
                        {
                            client_id = record.client_id,
                            weight = record.weight,
                            height = record.height,
                            body_mass_index = record.body_mass_index,
                            blood_pressure = record.blood_pressure,
                            pulse_rate = record.pulse_rate,
                            standard_operating_procedure = record.standard_operating_procedure,
                            visit_date = record.visit_date,
                            next_visit_date = record.next_visit_date,
                            prescription = record.prescription,
                            remarks = record.remarks,
                            created_by = Convert.ToInt16(HttpContext.Session.GetString("userid"))
                        };

                        if (dbhandler.AddPatientHistory(mymodel))
                        {
                            CaptureAuditTrail("Created patient history", "Created patient history: " + mymodel.client_id);
                            ModelState.Clear();
                            response.error_code = "00";
                            response.error_desc = "task successfully created";
                        }
                        else
                        {
                            response.error_code = "01";
                            response.error_desc = "Could not Create patient history, kindly contact system admin";
                        }

                    }
                }
                catch
                {
                    response.error_code = "01";
                    response.error_desc = "Could not Create patient history, kindly contact system admin";
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
                    case "task_record":
                        TasksModel taskmodel = dbhandler.GetTasksRecord().Find(mymodel => mymodel.id == id)!;
                        if (taskmodel != null)
                        {
                            dbhandler.DeleteRecord(id, Convert.ToInt16(HttpContext.Session.GetString("userid")), module);
                            CaptureAuditTrail("Deleted task recod", "Deleted task record: " + taskmodel.task_name);
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
