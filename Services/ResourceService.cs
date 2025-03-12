using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Schedify.Data;
using Schedify.Models;
using Schedify.ViewModels;

namespace Schedify.Services;

public class ResourceService
{
    private readonly ApplicationDbContext _context;
    private readonly UserService _userService;
    private readonly IWebHostEnvironment _environment;

    public ResourceService(ApplicationDbContext context, UserService userService, IWebHostEnvironment environment)
    {
        _context = context;
        _userService = userService;
        _environment = environment;
    }

    public List<Resource> GetResources()
    {
        return _context.Resources.Include(r => r.EventResources).OrderByDescending(r => r.CreatedAt).ToList();
    }

    public Dictionary<Guid, string?> GetResourceImages()
    {
        return _context.Images
            .Where(img => img.ResourceId != null)
            .ToDictionary(img => img.ResourceId!.Value, img => img.ImageFileName);
    }

    public List<Resource> GetResourcesByType(ResourceType type, int newPage, int pageSize)
    {
        return _context.Resources
            .Where(r => r.Type == type && r.Quantity > 0)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((newPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public Dictionary<Guid, string> GetResourceImageFromList(List<Resource> resources)
    {
        var resourceIds = resources.Select(e => e.Id).ToList();

        return _context.Images
            .Where(img => img.ResourceId.HasValue && resourceIds.Contains(img.ResourceId.Value))
            .ToDictionary(img => img.ResourceId!.Value, img => img.ImageFileName!);
    }

    public async Task<EventResourceViewModel?> GetResourceAndEvents(Guid ResourceId, List<Event> Events)
    {
        var resource = await _context.Resources
            .Include(r => r.Image)
            .FirstOrDefaultAsync(r => r.Id == ResourceId);

        if (resource == null) return null;

        return new EventResourceViewModel
        {
            ResourceId = ResourceId,
            EventId = Events.First().Id,
            Resource = resource,
            EventStartAt = Events.First().StartAt,
            EventEndAt = Events.First().EndAt,
            DraftEvents = Events,
            SelectedEvent = Events.First().Name,
            CostType = resource.CostType,
            Type = resource.Type,
            QuantityFromResource = resource.Quantity,
            CostFromResource = resource.Cost,
            Shift = resource.Type == ResourceType.Personnel ? resource.Shift : null,
        };
    }

    public async Task<ResourceViewModel?> GetResourceByIdAsync(Guid Id)
    {
        var resource = await _context.Resources
            .Include(r => r.Image)
            .Include(r => r.EventResources)
            .FirstOrDefaultAsync(r => r.Id == Id);

        var imageFileName = await _context.Images
            .Where(img => img.ResourceId == Id)
            .Select(img => img.ImageFileName)
            .FirstOrDefaultAsync();

        if (resource == null) return null;

        bool isUsed = false;

        if (resource.EventResources.Any() == true) isUsed = true;

        return new ResourceViewModel
        {
            IsUsed = isUsed,
            ImageFileName = resource.Image?.ImageFileName,
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
            Capacity = resource.Type == ResourceType.Venue ? resource.Capacity : 0,
            Size = resource.Type == ResourceType.Venue ? (int.TryParse(resource.Size, out var num) ? num : 0).ToString("N0") : null,
            AddressLine1 = resource.Type == ResourceType.Venue ? resource.AddressLine1 : null,
            AddressLine2 = resource.Type == ResourceType.Venue ? resource.AddressLine2 : null,
            CityMunicipality = resource.Type == ResourceType.Venue ? "Davao City" : null,
            Province = resource.Type == ResourceType.Venue ? "Davao del Sur" : null,
            Brand = resource.Type == ResourceType.Equipment ? resource.Brand : null,
            Specifications = resource.Type == ResourceType.Equipment ? JsonSerializer.Deserialize<Dictionary<string, string>>(resource.Specifications!)! : [],
            Material = resource.Type == ResourceType.Furniture ? resource.Material : null,
            Dimensions = resource.Type == ResourceType.Furniture ? resource.Dimensions : null,
            MenuItems = resource.Type == ResourceType.Catering ? resource.MenuItems : null,
            Position = resource.Type == ResourceType.Personnel ? resource.Position : null,
            Shift = resource.Type == ResourceType.Personnel ? resource.Shift : null,
            Experience = resource.Type == ResourceType.Personnel ? resource.Experience : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public async Task<ViewResourceViewModel?> GetResourceByIdForOrganizersAsync(Guid Id)
    {
        var resource = await _context.Resources
            .Include(r => r.Image)
            .FirstOrDefaultAsync(r => r.Id == Id);

        if (resource == null || resource.Image!.ImageFileName == null) return null;

        // Check if the image exists in local storage
        if (!File.Exists(Path.Combine(_environment.WebRootPath, "resources", resource.Image!.ImageFileName))) return null;

        return new ViewResourceViewModel
        {
            ImageFileName = resource.Image!.ImageFileName,
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
            Capacity = resource.Type == ResourceType.Venue ? resource.Capacity : 0,
            Size = resource.Type == ResourceType.Venue ? (int.TryParse(resource.Size, out var num) ? num : 0).ToString("N0") : null,
            AddressLine1 = resource.Type == ResourceType.Venue ? resource.AddressLine1 : null,
            AddressLine2 = resource.Type == ResourceType.Venue ? resource.AddressLine2 : null,
            CityMunicipality = resource.Type == ResourceType.Venue ? resource.CityMunicipality : null,
            Province = resource.Type == ResourceType.Venue ? resource.Province : null,
            Brand = resource.Type == ResourceType.Equipment ? resource.Brand : null,
            Specifications = resource.Type == ResourceType.Equipment ? JsonSerializer.Deserialize<Dictionary<string, string>>(resource.Specifications!)! : [],
            Material = resource.Type == ResourceType.Furniture ? resource.Material : null,
            Dimensions = resource.Type == ResourceType.Furniture ? resource.Dimensions : null,
            MenuItems = resource.Type == ResourceType.Catering ? resource.MenuItems : null,
            Position = resource.Type == ResourceType.Personnel ? resource.Position : null,
            Shift = resource.Type == ResourceType.Personnel ? resource.Shift : null,
            Experience = resource.Type == ResourceType.Personnel ? resource.Experience : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }


    public async Task<bool> DeleteResourceAsync(Guid Id)
    {
        var resource = await _context.Resources
            .Include(r => r.Image)
            .FirstOrDefaultAsync(r => r.Id == Id);

        if (resource == null) return false;

        // Delete associated images first
        string imagePath = Path.Combine(_environment.WebRootPath, "resources", resource.Image!.ImageFileName!);

        // Delete the physical file
        if (System.IO.File.Exists(imagePath))
        {
            System.IO.File.Delete(imagePath);
        }

        _context.Images.Remove(resource.Image!);


        _context.Resources.Remove(resource);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(bool IsSuccess, Dictionary<string, string>? Error, Resource? Resource)> CreateResourceAsync(ResourceViewModel model)
    {
        var validationErrors = ValidateResourceModel(model);
        if (validationErrors.Any())
        {
            return (false, validationErrors, null);
        }

        var user = await _userService.GetUserAsync();
        if (user == null)
        {
            return (false, new Dictionary<string, string> { { "Authentication", "User must be authenticated." } }, null);
        }

        var resource = MapViewModelToResource(model, user.Id);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Resources.Add(resource);
            await _context.SaveChangesAsync();

            if (model.ImageFile != null)
            {
                var imageResult = await SaveResourceImageAsync(model.ImageFile, resource.Id);
                if (!imageResult.IsSuccess)
                {
                    await transaction.RollbackAsync();
                    return (false, new Dictionary<string, string> { { "ImageFile", imageResult.Error! } }, null);
                }
            }

            await transaction.CommitAsync();
            return (true, null, resource);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, new Dictionary<string, string> { { "Exception", $"An error occurred: {ex.Message}" } }, null);
        }
    }

    private Dictionary<string, string> ValidateResourceModel(ResourceViewModel model)
    {
        var errors = new Dictionary<string, string>();

        if (model.ImageFile == null)
        {
            errors["ImageFile"] = "An image is required.";
        }

        switch (model.Type)
        {
            case ResourceType.Venue:
                if (string.IsNullOrWhiteSpace(model.AddressLine1)) errors["AddressLine1"] = "Address is required.";
                if (string.IsNullOrWhiteSpace(model.Size)) errors["Size"] = "Size is required.";
                if (model.Capacity <= 0) errors["Capacity"] = "Capacity must be at least 1.";
                break;

            case ResourceType.Equipment:
                if (string.IsNullOrWhiteSpace(model.Brand)) errors["Brand"] = "Brand is required.";
                if (model.Specifications == null || model.Specifications.Count == 0) errors["Specifications"] = "Specifications cannot be empty.";
                break;

            case ResourceType.Furniture:
                if (string.IsNullOrWhiteSpace(model.Material)) errors["Material"] = "Material is required.";
                if (string.IsNullOrWhiteSpace(model.Dimensions)) errors["Dimensions"] = "Dimensions are required.";
                break;

            case ResourceType.Catering:
                if (string.IsNullOrWhiteSpace(model.MenuItems)) errors["MenuItems"] = "Menu Items are required.";
                break;

            case ResourceType.Personnel:
                if (string.IsNullOrWhiteSpace(model.Position)) errors["Position"] = "Position is required.";
                if (string.IsNullOrWhiteSpace(model.ShiftAsString)) errors["ShiftAsString"] = "Shift is required.";
                if (string.IsNullOrWhiteSpace(model.Experience)) errors["Experience"] = "Experience is required.";
                break;
        }

        return errors;
    }

    private Resource MapViewModelToResource(ResourceViewModel model, Guid userId)
    {
        var resource = new Resource
        {
            UserId = userId,
            ProviderName = model.ProviderName,
            ProviderPhoneNumber = model.ProviderPhoneNumber,
            ProviderEmail = model.ProviderEmail,
            Name = model.Name,
            Description = model.Description,
            Type = model.Type,
            Cost = model.CostAsDecimal,
            CostType = model.CostType,
            Quantity = model.Quantity,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        switch (model.Type)
        {
            case ResourceType.Venue:
                resource.Capacity = model.Capacity;
                resource.Size = model.Size;
                resource.AddressLine1 = model.AddressLine1;
                resource.AddressLine2 = model.AddressLine2;
                resource.CityMunicipality = "Davao City";
                resource.Province = "Davao del Sur";
                break;

            case ResourceType.Equipment:
                resource.Brand = model.Brand;
                resource.Specifications = JsonSerializer.Serialize(model.Specifications);
                break;

            case ResourceType.Furniture:
                resource.Material = model.Material;
                resource.Dimensions = model.Dimensions;
                break;

            case ResourceType.Catering:
                resource.MenuItems = model.MenuItems;
                break;

            case ResourceType.Personnel:
                resource.Position = model.Position;
                resource.Shift = model.ShiftAsString;
                resource.Experience = model.Experience;
                break;
        }

        return resource;
    }

    private async Task<(bool IsSuccess, string? Error)> SaveResourceImageAsync(IFormFile imageFile, Guid resourceId)
    {
        try
        {
            string newFileName = DateTime.UtcNow.ToString("yyyyMMddHHmmss") + Path.GetExtension(imageFile.FileName);
            string imageFullPath = Path.Combine(_environment.WebRootPath, "resources", newFileName);

            // Ensure directory exists
            string directoryPath = Path.GetDirectoryName(imageFullPath)!;
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using (var stream = System.IO.File.Create(imageFullPath))
            {
                await imageFile.CopyToAsync(stream);
            }

            var image = new Image
            {
                ResourceId = resourceId,
                ImageFileName = newFileName
            };

            _context.Images.Add(image);
            await _context.SaveChangesAsync();

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Image saving failed: {ex.Message}");
        }
    }
}