using System.Text.Json;
using System.Text.RegularExpressions;
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
    private readonly EventService _eventService;
    private readonly UserService _userService;
    private readonly IWebHostEnvironment _environment;

    public ResourceService(ApplicationDbContext context, EventService eventService, UserService userService, IWebHostEnvironment environment)
    {
        _context = context;
        _eventService = eventService;
        _userService = userService;
        _environment = environment;
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

    public List<Resource>? GetResourcesByEventId(Guid EventId)
    {
        return _context.Resources.Where(r => r.EventId == EventId)
            .Include(r => r.ResourceVenue)
            .Include(r => r.ResourceEquipment)
            .Include(r => r.ResourceFurniture)
            .Include(r => r.ResourceCatering)
            .Include(r => r.ResourcePersonnel)
            .ToList();
    }

    public async Task<Resource?> GetResourceByIdAsync(Guid ResourceId)
    {
        return await _context.Resources
            .Include(r => r.Image)
            .FirstOrDefaultAsync(r => r.Id == ResourceId);
    }

    public async Task<Resource?> GetResourceByTypeAsync(Guid ResourceId, ResourceType ResourceType)
    {
        var query = _context.Resources
                        .Include(r => r.Image)
                        .AsQueryable(); // Start with a base query

        // Dynamically include the correct related entity
        query = ResourceType switch
        {
            ResourceType.Venue => query.Include(r => r.ResourceVenue),
            ResourceType.Equipment => query.Include(r => r.ResourceEquipment),
            ResourceType.Furniture => query.Include(r => r.ResourceFurniture),
            ResourceType.Catering => query.Include(r => r.ResourceCatering),
            ResourceType.Personnel => query.Include(r => r.ResourcePersonnel),
            _ => query
        };

        return await query.FirstOrDefaultAsync(r => r.Id == ResourceId);
    }

    public Dictionary<Guid, string?> GetResourceImageFromList(List<Resource> resources)
    {
        var resourceIds = resources.Select(e => e.Id).ToList();

        return _context.Images
            .Where(img => img.ResourceId.HasValue && resourceIds.Contains(img.ResourceId.Value))
            .ToDictionary(img => img.ResourceId!.Value, img => img.ImageFileName);
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
            // Type = resource.Type,
            // QuantityFromResource = resource.Quantity,
            // CostFromResource = resource.Cost,
            // Shift = resource.Type == ResourceType.Personnel ? resource.Shift : null,
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

    public async Task<(bool IsSuccess, Dictionary<string, string>? Error, Resource? Resource)> CreateResourceAsync(CreateResourceViewModel model, Guid EventId)
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

        var evt = _eventService.GetEventById(EventId);

        if (evt!.UserId != user.Id)
        {
            return (false, new Dictionary<string, string> { { "Authorization", "Invalid user." } }, null);
        }

        var resource = new Resource
        {
            EventId = EventId,
            ProviderName = model.ProviderName,
            ProviderPhoneNumber = model.ProviderPhoneNumber,
            ProviderEmail = model.ProviderEmail,
            Name = model.Name,
            Description = model.Description,
            ResourceType = model.ResourceType,
            Cost = model.Cost,
            CostType = model.CostType,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        object? typeResource = null;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            _context.Resources.Add(resource);
            await _context.SaveChangesAsync();
            DateTime WarrantyResult = DateTime.UtcNow;
            string FinalWarranty = string.Empty;

            if (!string.IsNullOrEmpty(model.Warranty))
            {
                if (int.TryParse(model.WarrantyDuration, out int WarrantyDuration))
                {
                    if (int.TryParse(model.Warranty, out int Warranty))
                    {
                        // If By Month
                        if (WarrantyDuration == 2)
                        {
                            if (Warranty < 2)
                                FinalWarranty = Warranty.ToString() + " month";
                            else
                                FinalWarranty = Warranty.ToString() + " months";
                        }
                        if (Warranty < 2)
                            FinalWarranty = Warranty.ToString() + " year";
                        else
                            FinalWarranty = Warranty.ToString() + " years";
                    }
                    else
                        return (false, new Dictionary<string, string> { { "Warranty", "Invalid warranty." } }, null);
                }
                else
                    return (false, new Dictionary<string, string> { { "Warranty", "Invalid warranty." } }, null);
            }

            switch (model.ResourceType)
            {
                case ResourceType.Venue:

                    typeResource = new ResourceVenue
                    {
                        ResourceId = resource.Id,
                        Capacity = model.Capacity,
                        Size = model.Size,
                        AddressLine1 = model.AddressLine1!,
                        AddressLine2 = model.AddressLine2,
                        CityMunicipality = "Davao City",
                        Province = "Davao del Sur",
                    };
                    break;

                case ResourceType.Equipment:

                    WarrantyResult = GetWarranty(FinalWarranty);

                    typeResource = new ResourceEquipment
                    {
                        ResourceId = resource.Id,
                        Quantity = model.Quantity,
                        Brand = model.Brand,
                        Specifications = JsonSerializer.Serialize(model.Specifications),
                        Warranty = !string.IsNullOrEmpty(model.Warranty) ? WarrantyResult : DateTime.UtcNow,
                    };
                    break;

                case ResourceType.Furniture:

                    WarrantyResult = GetWarranty(FinalWarranty);

                    typeResource = new ResourceFurniture
                    {
                        ResourceId = resource.Id,
                        Quantity = model.Quantity,
                        Material = model.Material,
                        OtherMaterial = model.OtherMaterial,
                        Dimensions = model.Dimensions,
                        Warranty = !string.IsNullOrEmpty(model.Warranty) ? WarrantyResult : DateTime.UtcNow,
                    };
                    break;

                case ResourceType.Catering:
                    typeResource = new ResourceCatering
                    {
                        ResourceId = resource.Id,
                        GuestCapacity = model.GuestCapacity,
                        MenuItems = JsonSerializer.Serialize(model.MenuItems),
                    };
                    break;

                case ResourceType.Personnel:
                    string experience = string.Empty;
                    if (int.TryParse(model.Experience, out int result))
                    {
                        if (int.TryParse(model.ExperienceType, out int resultType))
                        {
                            if (resultType == 1)
                            {
                                if (result > 1)
                                {
                                    experience = model.Experience + " years";
                                }

                                experience = model.Experience + " year";
                            }
                            else if (resultType == 2)
                            {
                                if (result > 1)
                                {
                                    experience = model.Experience + " months";
                                }

                                experience = model.Experience + " month";
                            }
                            else
                            {
                                experience = "None";

                            }
                        }
                        else
                        {
                            return (false, new Dictionary<string, string> { { "Experience", "Invalid experience." } }, null);
                        }
                    }
                    else
                    {
                        return (false, new Dictionary<string, string> { { "Experience", "Invalid experience." } }, null);
                    }

                    typeResource = new ResourcePersonnel
                    {
                        ResourceId = resource.Id,
                        Position = model.Position!,
                        ShiftStart = model.ShiftStart,
                        ShiftEnd = model.ShiftEnd,
                        Experience = experience,
                    };
                    break;
            }

            if (typeResource != null)
            {
                _context.Add(typeResource);
                await _context.SaveChangesAsync();
            }

            if (model.ImageFile != null)
            {
                var imageResult = await SaveResourceImageAsync(model.ImageFile, resource.Id);
                if (!imageResult.IsSuccess)
                {
                    await transaction.RollbackAsync();
                    return (false, new Dictionary<string, string> { { "ImageFileError", imageResult.Error! } }, null);
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

    private Dictionary<string, string> ValidateResourceModel(CreateResourceViewModel model)
    {
        var errors = new Dictionary<string, string>();

        if (model.ImageFile == null)
        {
            errors["ImageFile"] = "An image is required.";
        }

        switch (model.ResourceType)
        {
            case ResourceType.Venue:
                if (string.IsNullOrWhiteSpace(model.AddressLine1)) errors["AddressLine1"] = "Address is required.";
                if (model.Size <= 0) errors["Size"] = "Size must be at least 1.";
                if (model.Capacity <= 0) errors["Capacity"] = "Capacity must be at least 1.";
                break;

            case ResourceType.Equipment:
                if (string.IsNullOrWhiteSpace(model.Brand)) errors["Brand"] = "Brand is required.";
                if (model.Specifications == null || model.Specifications.Count == 0) errors["Specifications"] = "Specifications cannot be empty.";
                break;

            case ResourceType.Furniture:
                if (model.Material == FurnitureMaterial.Other && string.IsNullOrWhiteSpace(model.OtherMaterial)) errors["Material"] = "Material is required.";
                break;

            case ResourceType.Catering:
                if (model.MenuItems == null || model.MenuItems.Count == 0) errors["MenuItems"] = "Menu Items are required.";
                break;

            case ResourceType.Personnel:
                if (string.IsNullOrWhiteSpace(model.Position)) errors["Position"] = "Position is required.";
                break;
        }

        return errors;
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

    // Methods
    private DateTime GetWarranty(string FinalWarranty)
    {
        var match = Regex.Match(FinalWarranty, @"(\d+)\s*(year|years|month|months)", RegexOptions.IgnoreCase);
        DateTime today = DateTime.Today;
        DateTime WarrantyResult = DateTime.UtcNow;

        if (match.Success)
        {
            int value = int.Parse(match.Groups[1].Value);
            string unit = match.Groups[2].Value.ToLower();

            switch (unit)
            {
                case "year":
                case "years":
                    WarrantyResult = today.AddYears(value);
                    break;
                case "month":
                case "months":
                    WarrantyResult = today.AddMonths(value);
                    break;
                default:
                    WarrantyResult = today;
                    break;
            }
        }

        return WarrantyResult;
    }
}