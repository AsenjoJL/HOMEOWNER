namespace HOMEOWNER.Models
{
    public class Reservation
    {
        public int ReservationID { get; set; }
        public int HomeownerID { get; set; }
        public virtual Homeowner? Homeowner { get; set; }  // ✅ Add this navigation property
        public int FacilityID { get; set; }
        public virtual Facility? Facility { get; set; }
        public DateTime ReservationDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Status { get; set; }
        public string? Purpose { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? Rating { get; set; }  // Nullable, as the user may not have rated yet
    }
}
