using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SHERIA.Helpers;
using SHERIA.Models;
using System.Collections;

namespace SHERIA.Controllers
{
    public class ClientManagementController : Controller
    {
        private IWebHostEnvironment ihostingenvironment;
        private ILoggerManager iloggermanager;
        private DBHandler dbhandler;

        public ClientManagementController(ILoggerManager logger, IWebHostEnvironment environment, DBHandler mydbhandler)
        {
            iloggermanager = logger;
            ihostingenvironment = environment;
            dbhandler = mydbhandler;
        }

        public class onboardrecord
        {
            public onboardclientrecord[]? applicant_details { get; set; }
            public string? client_files { get; set; }
        }

        public class onboardclientrecord
        {
            public Int64 id { get; set; }
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
        public class processingresponse
        {
            public string? error_code { get; set; }
            public string? error_desc { get; set; }
            public string? system_ref { get; set; }
            public string? account_number { get; set; }
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Onboard()
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
        public ActionResult OnboardClient(onboardrecord record)
        {
            ArrayList details = new ArrayList();
            processingresponse response = new processingresponse
            {
                system_ref = DateTime.Now.ToString("yyyyMMddHHmmssfff")
            };

            try
            {
                ClientRecordModel patientexistingrecord = dbhandler.GetClientRecord()!.Find(model => model.phone_number!.Equals(record.applicant_details![0].phone_number))!;
                if (patientexistingrecord != null)
                {
                    record = null;
                    response.error_code = "01";
                    response.error_desc = "Record already submitted, kindly contact system admin";
                }
                else
                {
                    var user = HttpContext.Session.GetString("userid");
                    //1. create primary registration
                    ClientRecordModel clientModel = new ClientRecordModel
                    {
                        first_name = record.applicant_details![0].first_name,
                        last_name = record.applicant_details[0].last_name,
                        phone_number = record.applicant_details[0].phone_number,
                        email = record.applicant_details[0].email,
                        id_number = record.applicant_details[0].id_number,
                        sex = record.applicant_details[0].sex,
                        occupation = record.applicant_details[0].occupation,
                        nationality = record.applicant_details[0].nationality,
                        physical_address = record.applicant_details[0].physical_address,
                        remarks = record.applicant_details[0].remarks,
                        next_of_kin_name = record.applicant_details[0].next_of_kin_name,
                        next_of_kin_phone_number = record.applicant_details[0].next_of_kin_phone_number,
                        date_of_birth = record.applicant_details[0].date_of_birth,
                        created_by = Convert.ToInt64(HttpContext.Session.GetString("userid"))
                    };

                    Int64 client_id = dbhandler.AddClientRecord(clientModel);


                    if (client_id > 0)
                    {
                        JArray jarray = JArray.FromObject(record.applicant_details);

                        //2. Create customer files records

                        // PatientRecordModel personal_rec = dbhandler.GetPatientRecord()!.Find(model => model.id!.Equals(patient_id))!;


                        //var personal_rec = dbhandler.GetRecordsById("account_no", patient_id);

                        string[] client_files = record.client_files!.Split('|');
                        //remove last item
                        //client_files = client_files.Take(client_files.Count() - 1).ToArray();

                        for (int i = 0; i < client_files.Length; i++)
                        {
                            ClientFilesModel filesmodel = new ClientFilesModel
                            {
                                client_id = client_id,
                                file_name = client_files[i],
                            };

                            if (dbhandler.AddClientFiles(filesmodel))
                            {
                                ModelState.Clear();
                                response.error_code = "00";
                                response.error_desc = "Registration was success, you can proceed";
                            }
                            else
                            {
                                dbhandler.DeleteRecord(client_id, Convert.ToInt16(HttpContext.Session.GetString("userid")), "patient_register_fail_delete");
                                ModelState.Clear();
                                response.error_code = "01";
                                response.error_desc = "File Upload Failed , kindly contact system admin";
                            }
                        }


                    }
                }

            }

            catch (Exception ex)
            {

                iloggermanager.LogError(ex.Message);
                response.error_code = "01";
                response.error_desc = "Could not create client, kindly contact system admin";
            }

            return Content(JsonConvert.SerializeObject(response, Formatting.Indented), "application/json");

        }

        [HttpPost]
        public IActionResult Upload(List<IFormFile> postedFiles)
        {
            JArray jarray = new JArray();
            string wwwPath = ihostingenvironment.WebRootPath;
            string contentPath = ihostingenvironment.ContentRootPath;

            string path = Path.Combine(ihostingenvironment.WebRootPath, "Uploads");
            //string path = dbhandler.GetRecords("parameters", "UPLOAD_FILE_PATH").Rows[0]["item_value"].ToString()!;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            foreach (IFormFile postedFile in postedFiles)
            {
                string fileName = DateTime.Now.ToFileTimeUtc().ToString() + Path.GetExtension(postedFile.FileName);
                using FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create);
                postedFile.CopyTo(stream);

                jarray.Add(new JObject {
                    { "original_file_name",  postedFile.FileName },
                    { "new_file_name",  fileName },
                    { "message",  "success" }
                });
            }

            //return Content("Success");
            return Content(JsonConvert.SerializeObject(jarray, Formatting.Indented), "application/json");
        }

       
    }

    
}
