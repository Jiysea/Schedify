
using Schedify.Models;

namespace Schedify.Data;

public class PublishedEventDto
{
    public Event Event { get; set; } = null!;
    public Resource? Resource { get; set; }
    public Image? Image { get; set; }
    public int AttendeeCount { get; set; }
}