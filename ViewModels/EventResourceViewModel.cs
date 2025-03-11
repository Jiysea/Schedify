using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Schedify.Models;

namespace Schedify.ViewModels;

public class EventResourceViewModel
{
    public Guid ResourceId { get; set; }
    public Guid EventId { get; set; }

    // Get the cost type
    public string? CostType { get; set; }
    public ResourceType Type { get; set; }

    // From Resource
    public Resource? Resource { get; set; }
    public int QuantityFromResource { get; set; }
    public decimal CostFromResource { get; set; }


    public string SelectedEvent { get; set; } = "No Draft Events";
    public List<Event> DraftEvents { get; set; } = new List<Event>();

    // For Venues
    public DateTime EventStartAt { get; set; }
    public DateTime EventEndAt { get; set; }

    // For Personnels
    public string? Shift { get; set; }
    public int TotalHours
    {
        get
        {
            if (Shift != null)
            {
                string[] times = Shift.Split(" to ");
                var StartTime = DateTime.ParseExact(times[0].Trim(), "hh:mm tt", CultureInfo.InvariantCulture);
                var EndTime = DateTime.ParseExact(times[1].Trim(), "hh:mm tt", CultureInfo.InvariantCulture);
                return (int)(EndTime - StartTime).TotalHours;
            }

            return (int)(EventEndAt - EventStartAt).TotalHours;
        }
    }

    public int TotalDays
    {
        get
        {
            return (int)(EventEndAt - EventStartAt).TotalDays;
        }
    }

    // From Form
    [Range(1, int.MaxValue, ErrorMessage = "Value must be at least 1.")]
    public int QuantityFromForm { get; set; } = 1;

    [DataType(DataType.Currency)]
    [Range(typeof(decimal), "1.00", "9999999999")]
    public decimal TotalCost
    {
        get
        {
            if (MaxQuantityReached)
            {
                Console.WriteLine("MaxQuantityReached? " + true);
                return 0.00m;
            }

            switch (Type)
            {
                case ResourceType.Venue:
                    if (CostType == "Per Hour")
                    {
                        return CostFromResource * TotalHours;
                    }
                    if (CostType == "Per Day")
                    {
                        return CostFromResource * TotalDays;
                    }
                    if (CostType == "Fixed Rate")
                    {
                        return CostFromResource;
                    }
                    break;
                case ResourceType.Equipment:
                case ResourceType.Furniture:
                    if (CostType == "Per Unit")
                    {
                        return QuantityFromForm * CostFromResource;
                    }
                    if (CostType == "In Bulk")
                    {
                        return CostFromResource;
                    }
                    break;
                case ResourceType.Catering:
                    if (CostType == "Per Serving")
                    {
                        return QuantityFromForm * CostFromResource;
                    }
                    if (CostType == "In Bulk")
                    {
                        return CostFromResource;
                    }
                    break;
                case ResourceType.Personnel:
                    if (CostType == "Per Hour")
                    {
                        return QuantityFromForm * (CostFromResource * TotalHours);
                    }
                    if (CostType == "Per Day")
                    {
                        return QuantityFromForm * (CostFromResource * TotalDays);
                    }
                    if (CostType == "Fixed Rate")
                    {
                        return QuantityFromForm * CostFromResource;
                    }
                    break;
            }

            Console.WriteLine("Yawa");
            return 0.00m;
        }
    }

    public bool MaxQuantityReached
    {
        get
        {
            if (QuantityFromResource < QuantityFromForm)
            {
                return true;
            }

            return false;
        }
    }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}