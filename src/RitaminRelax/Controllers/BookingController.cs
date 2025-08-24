using Microsoft.AspNetCore.Mvc;
using RitaminRelax.ContentHandlers;
using SenseNet.Configuration;
using SenseNet.ContentRepository.Storage.Security;
using SNCR = SenseNet.ContentRepository;

namespace RitaminRelax.Controllers;

public class BookingRequest
{
    public DateTime time { get; set; }
    public int period { get; set; }
    public string type { get; set; }
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
    public IEnumerable<Booking> Get()
    {
        var user = SNCR.User.Current;

        if(user == null || user.Id == Identifiers.VisitorUserId)
        {
            return Array.Empty<Booking>();
        }

        using var _ = new SystemAccount();

        if (user.IsInGroup(Identifiers.AdministratorsGroupId) || user.IsInGroup(RRTools.RRManagers))
        {
            return Array.Empty<Booking>();
        }
        
        return Array.Empty<Booking>();
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
        if(user.Id == Identifiers.VisitorUserId)
        {
            return Forbid("Authentication required");
        }

        try
        {
            using var _ = new SystemAccount();

            var booking = new Booking(RRTools.BookingContainer);
            booking.Name = $"{request.time:yyyy-MM-dd_HH-mm}_{AccessProvider.Current.GetOriginalUser()}";
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
