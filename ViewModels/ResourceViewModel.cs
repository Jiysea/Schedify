using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Globalization;
using Schedify.Models;
using Schedify.Attributes;
using Schedify.Data;
namespace Schedify.ViewModels;

public class ResourceViewModel
{
    // Add a list of resources
    public List<Resource> Resources { get; set; } = new List<Resource>();

    // Add the image
    public Dictionary<Guid, string?> ResourceImages { get; set; } = []; // Maps ResourceId → ImageFileName

    public CreateResourceViewModel CreateResourceViewModel { get; set; } = new CreateResourceViewModel();
    public ViewResourceViewModel ViewResourceViewModel { get; set; } = new ViewResourceViewModel();
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public string? searchQuery { get; set; }
}