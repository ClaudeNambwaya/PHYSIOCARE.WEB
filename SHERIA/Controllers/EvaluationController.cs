using Microsoft.AspNetCore.Mvc;
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
    public class EvaluationController : Controller
    {

        private IWebHostEnvironment ihostingenvironment;
        private ILoggerManager iloggermanager;
        private DBHandler dbhandler;

        public EvaluationController(ILoggerManager logger, IWebHostEnvironment environment, DBHandler mydbhandler)
        {
            iloggermanager = logger;
            ihostingenvironment = environment;
            dbhandler = mydbhandler;
        }

        public class evaluation_record
        {
            public Int64 id { set; get; }
            public Int64 client_id { get; set; }
            public DateTime evaluation_date { get; set; }
            public DateTime next_visit_date { get; set; }
            public string? remarks { get; set; }
        }

        public IActionResult PatientEvaluation()
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
        public ActionResult CreateEvaluation(evaluation_record record)
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
                    EvaluationModel existingrecord = dbhandler.GetEvaluation().Find(mymodel => mymodel.id == record.id)!;
                    if (existingrecord != null)
                    {
                        EvaluationModel mymodel = new EvaluationModel
                        {
                            id = existingrecord.id,
                            client_id = record.client_id,
                            evaluation_date = record.evaluation_date,
                            next_visit_date = record.next_visit_date,
                            remarks = record.remarks,
                        };

                        if (dbhandler.UpdateEvaluation(mymodel))
                        {
                            CaptureAuditTrail("Updated evaluation", "Updated evaluation: " + mymodel.client_id);
                            ModelState.Clear();
                            response.error_code = "00";
                            response.error_desc = "Updated eveluation record";
                        }
                        else
                        {
                            response.error_code = "01";
                            response.error_desc = "Could not Update evaluation, kindly contact system admin ";
                        }
                    }
                    else
                    {
                        EvaluationModel mymodel = new EvaluationModel
                        {
                            client_id = record.client_id,
                            evaluation_date = record.evaluation_date,
                            next_visit_date = record.evaluation_date,
                            remarks = record.remarks,
                            created_by = Convert.ToInt16(HttpContext.Session.GetString("userid"))
                        };

                        if (dbhandler.AddEvaluation(mymodel))
                        {
                            CaptureAuditTrail("Created evaluation", "Created evaluation: " + mymodel.client_id);
                            ModelState.Clear();
                            response.error_code = "00";
                            response.error_desc = "evaluation successfully created";
                        }
                        else
                        {
                            response.error_code = "01";
                            response.error_desc = "Could not Create evaluation, kindly contact system admin";
                        }

                    }
                }
                catch
                {
                    response.error_code = "01";
                    response.error_desc = "Could not Create evaluation, kindly contact system admin";
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
                    case "evaluation_record":
                        EvaluationModel evaluation_model = dbhandler.GetEvaluation().Find(mymodel => mymodel.id == id)!;
                        if (evaluation_model != null)
                        {
                            dbhandler.DeleteRecord(id, Convert.ToInt16(HttpContext.Session.GetString("userid")), module);
                            CaptureAuditTrail("Deleted evaluation record", "Deleted evaluation record: " + evaluation_model.client_id);
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
