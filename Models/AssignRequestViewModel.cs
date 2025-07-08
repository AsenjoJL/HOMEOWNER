using HOMEOWNER.Models;

public class AssignRequestViewModel
{
    public int RequestID { get; set; }
    public List<Staff> AvailableStaff { get; set; } = new List<Staff>();
}
