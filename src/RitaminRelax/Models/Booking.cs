using System.Text.Json.Serialization;

namespace RitaminRelax.Models;

public enum BookingStatus { Pending, Confirmed, Cancelled }
public enum BookingPeriod { T30, T60 }
public enum BookingType { Massage1, Massage2, Massage3 }

/// <summary>
/// Describes a massage booking
/// </summary>
public class Booking
{
    /// <summary>
    /// Gets or sets the email of the customer
    /// </summary>
    public string? User { get; set; }
    /// <summary>
    /// Gets or sets the start of booking
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// Gets or sets the booking period for the reservation.
    /// </summary>
    /// <remarks>The <see cref="BookingPeriod"/> value determines the time frame associated with the booking.
    /// This property is serialized as a string when using JSON.</remarks>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BookingPeriod? Period { get; set; }

    /// <summary>
    /// Gets or sets the type of the booking.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BookingType? Type { get; set; }

    /// <summary>
    /// Gets or sets the current status of the booking e.g. Pending, Confirmed, Cancelled.
    /// </summary>
    /// <remarks>The status is serialized and deserialized as a string when using JSON, due to the applied
    /// <see cref="JsonStringEnumConverter"/>.</remarks>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public BookingStatus? Status { get; set; }
}
