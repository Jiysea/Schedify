using Schedify.Data;
using Schedify.Models;

namespace Schedify.Services;

public class ResourceService
{
    private readonly ApplicationDbContext _context;

    public ResourceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Resource> GetResources()
    {
        return _context.Resources.OrderByDescending(r => r.CreatedAt).ToList();
    }

    public Dictionary<Guid, string?> GetResourceImages()
    {
        return _context.Images
            .Where(img => img.ResourceId != null)
            .ToDictionary(img => img.ResourceId!.Value, img => img.ImageFileName);
    }
}