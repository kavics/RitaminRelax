using RitaminRelax.Models;

namespace RitaminRelax.Controllers;

/// <summary>
/// View model for calendar with bookings
/// </summary>
public class CalendarViewModel
{
    public DateTime CurrentMonth { get; set; }
    public List<Booking> Bookings { get; set; } = new();
}