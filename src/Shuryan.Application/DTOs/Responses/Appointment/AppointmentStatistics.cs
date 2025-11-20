namespace Shuryan.Application.DTOs.Responses.Appointment
{
    public class AppointmentStatistics
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Confirmed { get; set; }
        public int CheckedIn { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
        public int NoShow { get; set; }
        public int Cancelled { get; set; }
    }
}
