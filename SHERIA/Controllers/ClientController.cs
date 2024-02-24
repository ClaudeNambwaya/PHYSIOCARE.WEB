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
    public class ClientController : Controller
    {
        private IWebHostEnvironment ihostingenvironment;
        private ILoggerManager iloggermanager;
        private DBHandler dbhandler;

        public ClientController(ILoggerManager logger, IWebHostEnvironment environment, DBHandler mydbhandler)
        {
            iloggermanager = logger;
            ihostingenvironment = environment;
            dbhandler = mydbhandler;
        }

        public class client_record
        {
            public Int64 id { set; get; }
            public string? first_name { get; set; }
            public string? last_name { get; set; }
            public string? phone_number { get; set; }
            public string? email { get; set; }
            public string? id_number { get; set; }
            public string? sex { get; set; }
            public string? occupation { get; set; }
            public string? nationality { get; set; }
            public string? physical_address { get; set; }
            public string? next_of_kin_name { get; set; }
            public string? next_of_kin_phone_number { get; set; }
            public DateTime date_of_birth { get; set; }
            public string? remarks { get; set; }
        }

        public IActionResult Client()
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
        public ActionResult CreateClient(client_record record)
        {
            processingresponse response = new processingresponse
            {
                system_ref = DateTime.Now.ToString("yyyyMMddHHmmssfff")
            };

            if (HttpContext.Session.GetString("name") == null)
                return RedirectToAction("AdminLogin", "AppAuth");
            else
            {
                if (record.first_name == null)
                    return Content("Invalid first name");

                if (record.last_name == null)
                    return Content("Invalid last name");

                if (record.phone_number == null)
                    return Content("Invalid phone number");

                try
                {
                    ClientRecordModel existingrecord = dbhandler.GetClientRecord().Find(mymodel => mymodel.id == record.id)!;
                    if (existingrecord != null)
                    {
                        ClientRecordModel mymodel = new ClientRecordModel
                        {
                            id = existingrecord.id,
                            first_name = record.first_name,
                            last_name = record.last_name,
                            phone_number = record.phone_number,
                            email = record.email,
                            id_number = record.id_number,
                            sex = record.sex,
                            occupation = record.occupation,
                            nationality = record.nationality,
                            physical_address = record.physical_address,
                            next_of_kin_name = record.next_of_kin_name,
                            next_of_kin_phone_number = record.next_of_kin_phone_number,
                            date_of_birth = record.date_of_birth,
                            remarks = record.remarks,
                        };

                        if (dbhandler.UpdateClient(mymodel))
                        {
                            CaptureAuditTrail("Updated client", "Updated client: " + mymodel.first_name);
                            ModelState.Clear();
                            response.error_code = "00";
                            response.error_desc = "Updated client successfully ";
                        }
                        else
                        {
                            response.error_code = "01";
                            response.error_desc = "Could not Updated client, kindly contact system admin ";
                        }
                    }
                    else
                    {
                        ClientRecordModel mymodel = new ClientRecordModel
                        {
                            first_name = record.first_name,
                            last_name = record.last_name,
                            phone_number = record.phone_number,
                            email = record.email,
                            id_number = record.id_number,
                            sex = record.sex,
                            occupation = record.occupation,
                            nationality = record.nationality,
                            physical_address = record.physical_address,
                            next_of_kin_name = record.next_of_kin_name,
                            next_of_kin_phone_number = record.next_of_kin_phone_number,
                            date_of_birth = record.date_of_birth,
                            remarks = record.remarks,
                            created_by = Convert.ToInt16(HttpContext.Session.GetString("userid"))
                        };

                        if (dbhandler.AddClient(mymodel))
                        {
                            CaptureAuditTrail("Updated client", "Created client: " + mymodel.first_name);
                            ModelState.Clear();
                            response.error_code = "00";
                            response.error_desc = "task successfully created";
                        }
                        else
                        {
                            response.error_code = "01";
                            response.error_desc = "Could not Create client, kindly contact system admin";
                        }

                    }
                }
                catch
                {
                    response.error_code = "01";
                    response.error_desc = "Could not Create task, kindly contact system admin";
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
                    case "client_record":
                        ClientRecordModel clientrecordmodel = dbhandler.GetClientRecord().Find(mymodel => mymodel.id == id)!;
                        if (clientrecordmodel != null)
                        {
                            dbhandler.DeleteRecord(id, Convert.ToInt16(HttpContext.Session.GetString("userid")), module);
                            CaptureAuditTrail("Deleted client recod", "Deleted client record: " + clientrecordmodel.first_name);
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



