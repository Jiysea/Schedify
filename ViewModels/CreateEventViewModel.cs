using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Schedify.ViewModels;

public class CreateEventViewModel
{
    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(500)]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "This field is required.")]
    public DateTime StartAt { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public DateTime EndAt { get; set; }

    public EventStatus Status { get; set; } = EventStatus.Draft;

    [Required(ErrorMessage = "This field is required.")]
    public string EntryFee { get; set; } = null!;

    [DataType(DataType.Currency)]
    [Range(typeof(decimal), "0.01", "999999")]
    public decimal EntryFeeDecimal
    {
        get
        {
            if (decimal.TryParse(EntryFee, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands,
                CultureInfo.InvariantCulture, out decimal parsedValue))
            {
                return parsedValue;
            }
            return 0; // Or throw an error if necessary
        }
    }
}