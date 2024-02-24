namespace PHYSIOCARE.Models
{
    public class TreatmentPlanModel
    { 
        public Int64 id { get; set; }
        public Int64 client_id { get; set; }
        public DateTime treatment_plan_date { get; set; }
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
