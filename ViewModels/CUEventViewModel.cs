using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Schedify.ViewModels;

public class CUEventViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(20)]
    public string ShortName { get; set; } = null!;

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(500)]
    public string Description { get; set; } = null!;

    public DateTime StartAt
    {
        get
        {
            return ParseDateTimeOrDefault(StartAtString);
        }
    }

    public DateTime EndAt
    {
        get
        {
            return ParseDateTimeOrDefault(EndAtString);
        }
    }

    public TimeSpan TimeStart
    {
        get
        {
            return DateTime.TryParseExact(TimeStartString, "h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime shiftDateTime)
                ? shiftDateTime.TimeOfDay
                : TimeSpan.Zero; // Default to 00:00 if parsing fails
        }
    }

    public TimeSpan TimeEnd
    {
        get
        {
            return DateTime.TryParseExact(TimeEndString, "h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime shiftDateTime)
                ? shiftDateTime.TimeOfDay
                : TimeSpan.Zero; // Default to 00:00 if parsing fails
        }
    }

    [Required(ErrorMessage = "This field is required.")]
    public string? StartAtString { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public string? EndAtString { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public string? TimeStartString { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public string? TimeEndString { get; set; }

    public EventStatus Status { get; set; } = EventStatus.Draft;

    [MaxLength(7, ErrorMessage = "Value shouldn\'t exceed 6 digits.")]
    [Required(ErrorMessage = "This field is required.")]
    public string EntryFeeString { get; set; } = null!;

    [Range(typeof(decimal), "0.01", "999999")]
    public decimal EntryFee
    {
        get
        {
            if (decimal.TryParse(EntryFeeString, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture, out decimal parsedValue))
            {
                return parsedValue;
            }
            return 0; // Or throw an error if necessary
        }
    }

    private DateTime ParseDateTimeOrDefault(string? dateString)
    {
        if (!string.IsNullOrEmpty(dateString) &&
            DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
        {
            return parsedDate;
        }

        return DateTime.UtcNow; // Default value if parsing fails
    }

    public bool IsEdit { get; set; } = false;
}