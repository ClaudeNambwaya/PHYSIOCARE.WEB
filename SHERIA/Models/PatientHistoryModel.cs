namespace SHERIA.Models
{
    public class PatientHistoryModel
    { 
        public Int64 id { get; set; }
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
        public Int64 created_by { get; set; }
        public DateTime created_on { get; set; }
        public string? approved { get; set; }
        public DateTime approved_on { get; set; }
        public bool is_deleted { get; set; }
        public DateTime deleted_on { get; set; }
        public Int64 deleted_by { get; set; }
    }
}
