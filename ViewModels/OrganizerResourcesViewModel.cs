using Schedify.Models;

namespace Schedify.ViewModels;

public class OrganizerResourcesViewModel
{
    public string? searchQuery { get; set; }
    public List<Resource> Resources { get; set; } = new List<Resource>();
    public Dictionary<Guid, string> ResourceImages { get; set; } = new(); // Maps ResourceId → ImageFileName
    public ViewResourceViewModel? ViewResourceViewModel { get; set; } = new ViewResourceViewModel();
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}