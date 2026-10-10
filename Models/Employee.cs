namespace CampusCafe.API.Models
{
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public string WorkSchedule { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
    }
}