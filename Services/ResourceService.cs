using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Schedify.Data;
using Schedify.Models;
using Schedify.ViewModels;

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

    public async Task<ResourceViewModel?> GetResourceByIdAsync(Guid Id)
    {
        var resource = await _context.Resources
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == Id);

        if (resource == null) return null;

        return new ResourceViewModel
        {
            Id = resource.Id,
            ProviderName = resource.ProviderName,
            ProviderPhoneNumber = resource.ProviderPhoneNumber,
            ProviderEmail = resource.ProviderEmail,
            Name = resource.Name,
            Description = resource.Description,
            Type = resource.Type,
            Cost = resource.Cost.ToString("N2"),
            CostType = resource.CostType,
            Quantity = resource.Quantity,
            Capacity = resource.Type == ResourceType.Venue ? resource.Quantity : 0,
            Size = resource.Type == ResourceType.Venue ? resource.Size : null,
            AddressLine1 = resource.Type == ResourceType.Venue ? resource.AddressLine1 : null,
            AddressLine2 = resource.Type == ResourceType.Venue ? resource.AddressLine2 : null,
            CityMunicipality = resource.Type == ResourceType.Venue ? "Davao City" : null,
            Province = resource.Type == ResourceType.Venue ? "Davao del Sur" : null,
            Brand = resource.Type == ResourceType.Venue ? resource.Brand : null,
            Specifications = resource.Type == ResourceType.Venue ? JsonSerializer.Deserialize<Dictionary<string, string>>(resource.Specifications!)! : [],
            Material = resource.Type == ResourceType.Venue ? resource.Material : null,
            Dimensions = resource.Type == ResourceType.Venue ? resource.Dimensions : null,
            MenuItems = resource.Type == ResourceType.Venue ? resource.MenuItems : null,
            PriceItems = resource.Type == ResourceType.Venue ? resource.PriceItems : null,
            Position = resource.Type == ResourceType.Venue ? resource.Position : null,
            Shift = resource.Type == ResourceType.Venue ? resource.Shift : null,
            Experience = resource.Type == ResourceType.Venue ? resource.Experience : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public Dictionary<Guid, string?> GetResourceImages()
    {
        return _context.Images
            .Where(img => img.ResourceId != null)
            .ToDictionary(img => img.ResourceId!.Value, img => img.ImageFileName);
    }
}