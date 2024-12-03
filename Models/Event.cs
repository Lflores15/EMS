namespace EMS.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public string Organizer { get; set; }

        // New property to track event confirmation status
        public bool IsConfirmed { get; set; }  // Default is false (unconfirmed)
    }
}
