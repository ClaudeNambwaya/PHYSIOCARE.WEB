namespace SHERIA.Models
{
    public class ClientRecordModel
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
        public Int64 created_by { get; set; }
        public DateTime created_on { get; set; }
        public string? approved { get; set; }
        public DateTime approved_on { get; set; }
        public bool is_deleted { get; set; }
        public DateTime deleted_on { get; set; }
        public Int64 deleted_by { get; set; }
    }
}
