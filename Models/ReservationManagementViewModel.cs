namespace HOMEOWNER.Models.ViewModels
{
    public class ReservationManagementViewModel
    {
        public IEnumerable<Facility> Facilities { get; set; } = new List<Facility>();
        public IEnumerable<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
