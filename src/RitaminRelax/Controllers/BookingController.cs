using Microsoft.AspNetCore.Mvc;
using RitaminRelax.Models;
using SenseNet.Configuration;
using SenseNet.ContentRepository.Storage.Security;
using SNCR = SenseNet.ContentRepository;

namespace RitaminRelax.Controllers;

/// <summary>
/// MVC Controller for managing booking operations
/// </summary>
public class BookingController : Controller
{
    private readonly Random _random = new();

    /// <summary>
    /// Index page showing calendar view for booking
    /// </summary>
    /// <param name="year">Year for the calendar (optional)</param>
    /// <param name="month">Month for the calendar (optional)</param>
    /// <returns>Calendar view for the specified month</returns>
    public IActionResult Index(int? year, int? month)
    {
        var currentDate = DateTime.Today;
        
        // If year and month are provided, use them; otherwise use current date
        if (year.HasValue && month.HasValue)
        {
            try
            {
                currentDate = new DateTime(year.Value, month.Value, 1);
            }
            catch (ArgumentOutOfRangeException)
            {
                // If invalid date parameters, fall back to current date
                currentDate = DateTime.Today;
            }
        }
        
        return View(currentDate);
    }

    /// <summary>
    /// Bookings list page
    /// </summary>
    /// <returns>View with list of bookings</returns>
    public IActionResult List()
    {
        var bookings = GetBookings();
        return View(bookings);
    }

    /// <summary>
    /// Details page for a specific booking
    /// </summary>
    /// <param name="id">Booking index</param>
    /// <returns>Details view for the booking</returns>
    public IActionResult Details(int id)
    {
        var bookings = GetBookings().ToList();
        if (id < 0 || id >= bookings.Count)
        {
            return NotFound();
        }
        
        return View(bookings[id]);
    }

    /// <summary>
    /// Create new booking form
    /// </summary>
    /// <returns>Create view with empty booking model</returns>
    public IActionResult Create()
    {
        var booking = new Booking 
        { 
            Time = DateTime.Today.AddDays(1).AddHours(16),
            Status = BookingStatus.Pending
        };
        return View(booking);
    }

    /// <summary>
    /// Create new booking - POST action
    /// </summary>
    /// <param name="booking">Booking to create</param>
    /// <returns>Redirect to index or return to create view with errors</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Booking booking)
    {
        if (ModelState.IsValid)
        {
            // In a real application, save to database here
            TempData["SuccessMessage"] = "Booking created successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(booking);
    }

    /// <summary>
    /// Random bookings page for demo purposes
    /// </summary>
    /// <returns>View with randomly generated bookings</returns>
    public IActionResult Random()
    {
        var bookings = GetRandomBookings();
        return View(bookings);
    }

    /// <summary>
    /// API endpoint to get bookings (legacy support)
    /// </summary>
    /// <returns>JSON array of bookings</returns>
    [HttpGet]
    [Route("api/[controller]")]
    public IActionResult GetBookingsApi()
    {
        var bookings = GetBookings();
        return Json(bookings);
    }

    /// <summary>
    /// API endpoint to get random bookings (legacy support)
    /// </summary>
    /// <returns>JSON array of random bookings</returns>
    [HttpGet]
    [Route("api/[controller]/random")]
    public IActionResult GetRandomBookingsApi()
    {
        var bookings = GetRandomBookings();
        return Json(bookings);
    }

    private IEnumerable<Booking> GetBookings()
    {
        var user = SNCR.User.Current;
        if(user == null || user.Id == Identifiers.VisitorUserId)
        {
            return
            [
                new Booking { Time = DateTime.Today.AddDays(1).AddHours(16), },
                new Booking { Time = DateTime.Today.AddDays(2).AddHours(16) },
                new Booking { Time = DateTime.Today.AddDays(2).AddHours(17), }
            ];
        }
        
        using var _ = new SystemAccount();

        if (user.IsInGroup(Identifiers.AdministratorsGroupId) || user.IsInGroup(RRTools.RRManagers))
        {
            return
            [
                new Booking
                {
                    User = "TestUser1",
                    Time = DateTime.Today.AddDays(1).AddHours(16),
                    Period = BookingPeriod.T60,
                    Type = BookingType.Massage1,
                    Status = BookingStatus.Confirmed
                },
                new Booking
                {
                    User = "TestUser2",
                    Time = DateTime.Today.AddDays(2).AddHours(16),
                    Period = BookingPeriod.T30,
                    Type = BookingType.Massage2,
                    Status = BookingStatus.Pending
                },
                new Booking
                {
                    User = "TestUser3",
                    Time = DateTime.Today.AddDays(2).AddHours(17),
                    Period = BookingPeriod.T60,
                    Type = BookingType.Massage3,
                    Status = BookingStatus.Confirmed
                }
            ];
        }
        return
        [
            new Booking { Time = DateTime.Today.AddDays(1).AddHours(16), Status = BookingStatus.Pending },
            new Booking
            {
                User = "TestUser2",
                Time = DateTime.Today.AddDays(2).AddHours(16),
                Period = BookingPeriod.T30,
                Type = BookingType.Massage2,
                Status = BookingStatus.Confirmed
            },
            new Booking { Time = DateTime.Today.AddDays(2).AddHours(17), Status = BookingStatus.Pending }
        ];
    }

    private IEnumerable<Booking> GetRandomBookings()
    {
        var periods = Enum.GetValues<BookingPeriod>();
        var types = Enum.GetValues<BookingType>();
        var statuses = Enum.GetValues<BookingStatus>();
        
        return Enumerable.Range(0, 2).Select(i => new Booking
        {
            User = $"DemoUser{_random.Next(1, 100)}",
            Time = DateTime.Today.AddDays(_random.Next(1, 30)).AddHours(_random.Next(8, 20)),
            Period = periods[_random.Next(periods.Length)],
            Type = types[_random.Next(types.Length)],
            Status = statuses[_random.Next(statuses.Length)]
        });
    }
}
