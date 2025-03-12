using Schedify.Data;
using Schedify.Models;

namespace Schedify.ViewModels;

public class AttendeeEventsViewModel
{
     public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<PublishedEventDto> PublishedEvents { get; set; } = new List<PublishedEventDto>();
    public int EventsCount {get;set;}
}