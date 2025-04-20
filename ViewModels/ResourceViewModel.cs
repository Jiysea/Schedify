using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Globalization;
using Schedify.Models;
using Schedify.Attributes;
using Schedify.Data;
namespace Schedify.ViewModels;

public class ResourceViewModel
{
    // List of Resources
    public List<Resource> Resources { get; set; } = new List<Resource>();

    // List of Events (for dropdown)
    public List<Event> Events { get; set; } = new List<Event>();
    public Guid EventId { get; set; }
    public string? SelectedName { get; set; } = null!;
    public bool IsEventOnDraft { get; set; } = false;

    // Add the image
    public Dictionary<Guid, string?> ResourceImages { get; set; } = []; // Maps ResourceId → ImageFileName
    public CUResourceViewModel CUResourceViewModel { get; set; } = new CUResourceViewModel();
    public ViewResourceViewModel? ViewResourceViewModel { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public string? searchQuery { get; set; }
}