using Microsoft.AspNetCore.Mvc;
using RitaminRelax.ContentHandlers;
using SenseNet.Configuration;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage.Security;
using SNCR = SenseNet.ContentRepository;

namespace RitaminRelax.Controllers;

public class BookingRequest
{
    public DateTime time { get; set; }
    public int period { get; set; }
    public string type { get; set; }
}
public class BookingResponse
{
    public string? Customer { get;set; }
    public DateTime Time { get; set; }
    public int? Period { get; set; }
    public string? Type { get; set; }
    public bool Accepted { get; set; }
}

/// <summary>
/// Controller for managing booking operations
/// </summary>
[ApiController]
[Route("[controller]")]
public class BookingController : ControllerBase
{
    private readonly Random _random = new();

    /// <summary>
    /// Gets a list of test bookings
    /// </summary>
    /// <returns>A collection of test booking objects</returns>
    [HttpGet]
    public IEnumerable<BookingResponse> Get()
    {
        var user = SNCR.User.Current;

        using var _ = new SystemAccount();

        if(user == null || user.Id == Identifiers.VisitorUserId)
            return GetBookings(null, false);
        if (user.IsInGroup(Identifiers.AdministratorsGroupId) || user.IsInGroup(RRTools.RRManagers))
            return GetBookings(null, true);
        return GetBookings(user, false);
    }

    private IEnumerable<BookingResponse> GetBookings(IUser? user, bool isAdmin)
    {
        var bookings = SNCR.Content.All
            .Where(c => c.TypeIs(nameof(Booking)))
            .ToArray();

        var result = bookings
            .Select(c => (Booking)c.ContentHandler)
            .Select(b =>
            {
                var r = new BookingResponse { Time = b.BookingTime, Accepted = b.Accepted };
                if (isAdmin || (user != null && b.Customer.Id == user.Id))
                {
                    r.Customer = b.Customer.Email;
                    r.Period = b.BookingPeriod;
                    r.Type = b.BookingType;
                }
                return r;
            })
            .OrderBy(b => b.Time)
            .ToArray();

        return result;
    }

    /// <summary>
    /// Creates a new booking
    /// </summary>
    /// <param name="request">Booking request data</param>
    /// <returns>Action result indicating success or failure</returns>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] BookingRequest request)
    {
        var user = (SNCR.User)SNCR.User.Current;
        if (user == null)
            return BadRequest("Unknown user");
        if (user.Id == Identifiers.VisitorUserId)
            return Forbid("Authentication required");

        try
        {
            using var _ = new SystemAccount();

            var booking = new Booking(RRTools.BookingContainer);
            booking.Name = $"{request.time:yyyy-MM-dd_HH-mm}_{user.LoginName}";
            booking.Customer = user;
            booking.BookingTime = request.time;
            booking.BookingPeriod = request.period;
            booking.BookingType = request.type;
            booking.Accepted = false;

            await booking.SaveAsync(HttpContext.RequestAborted);
            
            return Ok("Booking created successfully");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error creating booking: {ex.Message}");
        }
    }
}
