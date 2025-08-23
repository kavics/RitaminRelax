using Microsoft.AspNetCore.Mvc;
using RitaminRelax.Models;
using SenseNet.Configuration;
using SenseNet.ContentRepository.Storage.Security;
using SNCR = SenseNet.ContentRepository;

namespace RitaminRelax.Controllers;

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
    [HttpGet(Name = "Test")]
    public IEnumerable<Booking> Get()
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
                    Type = BookingType.Massage1
                },
                new Booking
                {
                    User = "TestUser2",
                    Time = DateTime.Today.AddDays(2).AddHours(16),
                    Period = BookingPeriod.T30,
                    Type = BookingType.Massage2
                },
                new Booking
                {
                    User = "TestUser3",
                    Time = DateTime.Today.AddDays(2).AddHours(17),
                    Period = BookingPeriod.T60,
                    Type = BookingType.Massage3
                }
            ];
        }
        return
        [
            new Booking { Time = DateTime.Today.AddDays(1).AddHours(16), },
            new Booking
            {
                User = "TestUser2",
                Time = DateTime.Today.AddDays(2).AddHours(16),
                Period = BookingPeriod.T30,
                Type = BookingType.Massage2
            },
            new Booking { Time = DateTime.Today.AddDays(2).AddHours(17), }
        ];
    }

    /// <summary>
    /// Gets 2 random bookings for demo purposes
    /// </summary>
    /// <returns>A collection of 2 randomly generated booking objects</returns>
    [HttpGet("random")]
    public IEnumerable<Booking> GetRandomBookings()
    {
        var periods = Enum.GetValues<BookingPeriod>();
        var types = Enum.GetValues<BookingType>();
        var statuses = Enum.GetValues<BookingStatus>();
        
        return Enumerable.Range(0, 2).Select(_ => new Booking
        {
            User = $"DemoUser{_random.Next(1, 100)}",
            Time = DateTime.Today.AddDays(_random.Next(1, 30)).AddHours(_random.Next(8, 20)),
            Period = periods[_random.Next(periods.Length)],
            Type = types[_random.Next(types.Length)],
            Status = statuses[_random.Next(statuses.Length)]
        });
    }
}
