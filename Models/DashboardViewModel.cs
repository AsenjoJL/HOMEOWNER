using HOMEOWNER.Models;

namespace HOMEOWNER.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalHomeowners { get; set; }
        public int TotalReservations { get; set; }
        public int TotalPayments { get; set; }
        public int PendingRequests { get; set; }

        public int ApprovedReservations { get; set; }
        public int PendingReservations { get; set; }
        public int CompletedReservations { get; set; }
        public int CancelledReservations { get; set; }

        public List<Reservation> RecentReservations { get; set; }
        public List<Activity> RecentActivities { get; set; }
    }

    public class Activity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string TimeAgo { get; set; }
    }
}