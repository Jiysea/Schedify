
using Schedify.Models;

namespace Schedify.Data;

public class EventsDropdownDto
{
    public List<Event> Events { get; set; } = [];
    public string? SelectedName { get; set; } = null!;
}