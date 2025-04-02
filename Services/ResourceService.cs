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
            .Include(r => r.Image)
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
            .Include(r => r.ResourceVenue)
            .Include(r => r.ResourceEquipment)
            .Include(r => r.ResourceFurniture)
            .Include(r => r.ResourceCatering)
            .Include(r => r.ResourcePersonnel)
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

    public async Task<CUResourceViewModel> GetCUResourceViewModel(Guid ResourceId)
    {

        var resource = await _context.Resources
            .Include(r => r.Image)
            .Include(r => r.ResourceVenue)
            .Include(r => r.ResourceEquipment)
            .Include(r => r.ResourceFurniture)
            .Include(r => r.ResourceCatering)
            .Include(r => r.ResourcePersonnel)
            .FirstOrDefaultAsync(r => r.Id == ResourceId);

        if (resource == null) return new CUResourceViewModel();

        var viewModel = new CUResourceViewModel()
        {
            Id = resource.Id,
            EventId = resource.EventId,
            ImageFileName = resource.Image!.ImageFileName,
            ProviderName = resource.ProviderName,
            ProviderEmail = resource.ProviderEmail,
            ProviderPhoneNumber = resource.ProviderPhoneNumber,
            Name = resource.Name,
            Description = resource.Description,
            ResourceType = resource.ResourceType,
            CostAsString = resource.Cost.ToString("N2"),
            CostType = resource.CostType,
            CreatedAt = resource.CreatedAt,
            UpdatedAt = resource.UpdatedAt,
        };

        if (resource.ResourceType == ResourceType.Venue)
        {
            viewModel.Capacity = resource.ResourceVenue.Capacity;
            viewModel.Size = resource.ResourceVenue.Size;
            viewModel.AddressLine1 = resource.ResourceVenue.AddressLine1;
            viewModel.AddressLine2 = resource.ResourceVenue.AddressLine2;
            viewModel.CityMunicipality = resource.ResourceVenue.CityMunicipality;
            viewModel.Province = resource.ResourceVenue.Province;
        }
        else if (resource.ResourceType == ResourceType.Equipment)
        {
            var duration = ConvertDateTimeToDuration(resource.ResourceEquipment.Warranty);

            viewModel.Quantity = resource.ResourceEquipment.Quantity;
            viewModel.Brand = resource.ResourceEquipment.Brand;
            viewModel.Warranty = duration.First().Key == "0" ? null : duration.First().Key;
            viewModel.WarrantyDuration = duration.First().Value;
            viewModel.Specifications = JsonSerializer.Deserialize<Dictionary<string, string>>(resource.ResourceEquipment.Specifications!)!;
        }
        else if (resource.ResourceType == ResourceType.Furniture)
        {
            var duration = ConvertDateTimeToDuration(resource.ResourceFurniture.Warranty);

            viewModel.Quantity = resource.ResourceFurniture.Quantity;
            viewModel.Material = resource.ResourceFurniture.Material;
            viewModel.OtherMaterial = resource.ResourceFurniture.OtherMaterial;
            viewModel.Dimensions = resource.ResourceFurniture.Dimensions;
            viewModel.Warranty = duration.First().Key == "0" ? null : duration.First().Key;
            viewModel.WarrantyDuration = duration.First().Value;
        }
        else if (resource.ResourceType == ResourceType.Catering)
        {
            viewModel.GuestCapacity = resource.ResourceCatering.GuestCapacity;
            viewModel.MenuItems = resource.ResourceCatering.MenuItems.Split(",").ToList();
        }
        else if (resource.ResourceType == ResourceType.Personnel)
        {
            var duration = GetExperienceDictionary(resource.ResourcePersonnel!.Experience!);

            viewModel.Position = resource.ResourcePersonnel.Position;
            viewModel.ShiftStartString = resource.ResourcePersonnel.ShiftStart.ToString(@"hh\:mm");
            viewModel.ShiftEndString = resource.ResourcePersonnel.ShiftEnd.ToString(@"hh\:mm");
            viewModel.Experience = duration.First().Key == "0" ? null : duration.First().Key;
            viewModel.ExperienceType = duration.First().Value;
        }

        return viewModel;
    }

    public async Task<ViewResourceViewModel?> GetViewResourceViewModel(Guid ResourceId)
    {
        var resource = await GetResourceByIdAsync(ResourceId);
        if (resource == null) return null;
        
        var viewModel = new ViewResourceViewModel
        {
            Id = resource.Id,
            EventId = resource.EventId,
            ImageFileName = resource.Image!.ImageFileName,
            ProviderName = resource.ProviderName,
            ProviderEmail = resource.ProviderEmail,
            ProviderPhoneNumber = resource.ProviderPhoneNumber,
            Name = resource.Name,
            Description = resource.Description,
            ResourceType = resource.ResourceType,
            Cost = resource.Cost,
            CostType = resource.CostType,
            CreatedAt = resource.CreatedAt,
            UpdatedAt = resource.UpdatedAt,
        };

        if (resource.ResourceType == ResourceType.Venue)
        {
            viewModel.Capacity = resource.ResourceVenue.Capacity;
            viewModel.Size = resource.ResourceVenue.Size;
            viewModel.AddressLine1 = resource.ResourceVenue.AddressLine1;
            viewModel.AddressLine2 = resource.ResourceVenue.AddressLine2;
            viewModel.CityMunicipality = resource.ResourceVenue.CityMunicipality;
            viewModel.Province = resource.ResourceVenue.Province;
        }
        else if (resource.ResourceType == ResourceType.Equipment)
        {
            viewModel.Quantity = resource.ResourceEquipment.Quantity;
            viewModel.Brand = resource.ResourceEquipment.Brand;
            viewModel.Warranty = resource.ResourceEquipment.Warranty;
            viewModel.Specifications = JsonSerializer.Deserialize<Dictionary<string, string>>(resource.ResourceEquipment.Specifications!)!;
        }
        else if (resource.ResourceType == ResourceType.Furniture)
        {
            viewModel.Quantity = resource.ResourceFurniture.Quantity;
            viewModel.Material = resource.ResourceFurniture.Material;
            viewModel.OtherMaterial = resource.ResourceFurniture.OtherMaterial;
            viewModel.Dimensions = resource.ResourceFurniture.Dimensions;
            viewModel.Warranty = resource.ResourceFurniture.Warranty;
        }
        else if (resource.ResourceType == ResourceType.Catering)
        {
            viewModel.GuestCapacity = resource.ResourceCatering.GuestCapacity;
            viewModel.MenuItems = resource.ResourceCatering.MenuItems.Split(",").ToList();
        }
        else if (resource.ResourceType == ResourceType.Personnel)
        {
            viewModel.Position = resource.ResourcePersonnel.Position;
            viewModel.ShiftStart = resource.ResourcePersonnel.ShiftStart;
            viewModel.ShiftEnd = resource.ResourcePersonnel.ShiftEnd;
            viewModel.Experience = resource.ResourcePersonnel.Experience;
        }

        return viewModel;
    }

    public async Task<(bool IsSuccess, Dictionary<string, string>? Error, Resource? Resource)> CreateResourceAsync(CUResourceViewModel model, Guid EventId)
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

        var evt = _context.Events
            .Where(e => e.UserId == Guid.Parse(_userService.GetUserId()!) && e.Id == EventId)
            .FirstOrDefault();

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

            DateTime WarrantyResult;

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

                    if (!string.IsNullOrEmpty(model.Warranty) &&
                        int.TryParse(model.Warranty, out int EWarranty) &&
                        int.TryParse(model.WarrantyDuration, out int EWarrantyDuration))
                    {
                        WarrantyResult = GetWarranty(EWarranty, EWarrantyDuration);
                    }
                    else
                        WarrantyResult = DateTime.UtcNow;

                    typeResource = new ResourceEquipment
                    {
                        ResourceId = resource.Id,
                        Quantity = model.Quantity,
                        Brand = model.Brand,
                        Specifications = JsonSerializer.Serialize(model.Specifications),
                        Warranty = WarrantyResult,
                    };
                    break;

                case ResourceType.Furniture:

                    if (!string.IsNullOrEmpty(model.Warranty) &&
                        int.TryParse(model.Warranty, out int FWarranty) &&
                        int.TryParse(model.WarrantyDuration, out int FWarrantyDuration))
                    {
                        WarrantyResult = GetWarranty(FWarranty, FWarrantyDuration);
                    }
                    else
                        WarrantyResult = DateTime.UtcNow;

                    typeResource = new ResourceFurniture
                    {
                        ResourceId = resource.Id,
                        Quantity = model.Quantity,
                        Material = model.Material,
                        OtherMaterial = model.OtherMaterial,
                        Dimensions = model.Dimensions,
                        Warranty = WarrantyResult,
                    };
                    break;

                case ResourceType.Catering:
                    typeResource = new ResourceCatering
                    {
                        ResourceId = resource.Id,
                        GuestCapacity = model.GuestCapacity,
                        MenuItems = string.Join(",", model.MenuItems!),
                    };
                    break;

                case ResourceType.Personnel:
                    string experience = string.Empty;

                    if (!string.IsNullOrEmpty(model.Experience) && int.TryParse(model.Experience, out int Experience) && int.TryParse(model.ExperienceType, out int ExperienceType))
                    {
                        experience = GetExperience(Experience, ExperienceType);
                    }
                    else
                    {
                        experience = "None";
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

    public async Task<(bool IsSuccess, Dictionary<string, string>? Error, Resource? Resource)> UpdateResourceAsync(CUResourceViewModel model)
    {
        var validationErrors = ValidateResourceModel(model);
        if (validationErrors.Any())
            return (false, validationErrors, null);

        var user = await _userService.GetUserAsync();
        if (user == null)
            return (false, new Dictionary<string, string> { { "Authentication", "User must be authenticated." } }, null);

        var resource = await GetResourceByIdAsync(model.Id);

        if (resource == null)
            return (false, new Dictionary<string, string> { { "NullResource", "No resource found." } }, null);

        var evt = _context.Events
            .Where(e => e.UserId == Guid.Parse(_userService.GetUserId()!) && e.Id == model.EventId)
            .FirstOrDefault();

        if (evt == null || evt.UserId != user.Id)
            return (false, new Dictionary<string, string> { { "Authorization", "Invalid user or event not found." } }, null);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Update existing resource properties
            resource.ProviderName = model.ProviderName;
            resource.ProviderPhoneNumber = model.ProviderPhoneNumber;
            resource.ProviderEmail = model.ProviderEmail;
            resource.Name = model.Name;
            resource.Description = model.Description;
            resource.ResourceType = model.ResourceType;
            resource.Cost = model.Cost;
            resource.CostType = model.CostType;
            resource.UpdatedAt = DateTime.UtcNow;

            _context.Resources.Update(resource);
            await _context.SaveChangesAsync();

            DateTime WarrantyResult;
            string experience = string.Empty;

            switch (model.ResourceType)
            {
                case ResourceType.Venue:

                    var venue = resource.ResourceVenue;
                    venue.Capacity = model.Capacity;
                    venue.Size = model.Size;
                    venue.AddressLine1 = model.AddressLine1!;
                    venue.AddressLine2 = model.AddressLine2;
                    venue.CityMunicipality = "Davao City";
                    venue.Province = "Davao del Sur";
                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                    break;

                case ResourceType.Equipment:

                    if (!string.IsNullOrEmpty(model.Warranty) &&
                        int.TryParse(model.Warranty, out int EWarranty) &&
                        int.TryParse(model.WarrantyDuration, out int EWarrantyDuration))
                    {
                        WarrantyResult = GetWarranty(EWarranty, EWarrantyDuration);
                    }
                    else
                        WarrantyResult = DateTime.MinValue;

                    var equipment = resource.ResourceEquipment;
                    equipment.Quantity = model.Quantity;
                    equipment.Brand = model.Brand;
                    equipment.Specifications = JsonSerializer.Serialize(model.Specifications);
                    equipment.Warranty = WarrantyResult;
                    _context.Update(equipment);
                    await _context.SaveChangesAsync();
                    break;

                case ResourceType.Furniture:
                    if (!string.IsNullOrEmpty(model.Warranty) &&
                        int.TryParse(model.Warranty, out int FWarranty) &&
                        int.TryParse(model.WarrantyDuration, out int FWarrantyDuration))
                    {
                        WarrantyResult = GetWarranty(FWarranty, FWarrantyDuration);
                    }
                    else
                        WarrantyResult = DateTime.MinValue;

                    var furniture = resource.ResourceFurniture;
                    furniture.Quantity = model.Quantity;
                    furniture.Material = model.Material;
                    furniture.OtherMaterial = model.OtherMaterial;
                    furniture.Dimensions = model.Dimensions;
                    furniture.Warranty = WarrantyResult;
                    _context.Update(furniture);
                    await _context.SaveChangesAsync();
                    break;

                case ResourceType.Catering:
                    var catering = resource.ResourceCatering;
                    catering.GuestCapacity = model.GuestCapacity;
                    catering.MenuItems = string.Join(",", model.MenuItems!);
                    _context.Update(catering);
                    await _context.SaveChangesAsync();
                    break;

                case ResourceType.Personnel:

                    if (!string.IsNullOrEmpty(model.Experience) && int.TryParse(model.Experience, out int Experience) && int.TryParse(model.ExperienceType, out int ExperienceType))
                    {
                        experience = GetExperience(Experience, ExperienceType);
                    }
                    else
                    {
                        experience = "None";
                    }

                    var personnel = resource.ResourcePersonnel;
                    personnel.Position = model.Position!;
                    personnel.ShiftStart = model.ShiftStart;
                    personnel.ShiftEnd = model.ShiftEnd;
                    personnel.Experience = experience;
                    _context.Update(personnel);
                    await _context.SaveChangesAsync();
                    break;
            }

            if (model.ImageFile != null)
            {
                var imageResult = await SaveResourceImageAsync(model.ImageFile, resource.Id, model.ImageFileName);

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

    public async Task<bool> DeleteResourceAsync(Guid ResourceId)
    {
        var resource = await _context.Resources
        .Include(r => r.Image)
        .FirstOrDefaultAsync(r => r.Id == ResourceId);

        if (resource == null) return false;

        // Delete the associated resource type model
        switch (resource.ResourceType)
        {
            case ResourceType.Venue:
                var venue = await _context.ResourceVenues.FirstOrDefaultAsync(r => r.ResourceId == ResourceId);
                if (venue != null) _context.ResourceVenues.Remove(venue);
                break;

            case ResourceType.Equipment:
                var equipment = await _context.ResourceEquipments.FirstOrDefaultAsync(r => r.ResourceId == ResourceId);
                if (equipment != null) _context.ResourceEquipments.Remove(equipment);
                break;

            case ResourceType.Furniture:
                var furniture = await _context.ResourceFurnitures.FirstOrDefaultAsync(r => r.ResourceId == ResourceId);
                if (furniture != null) _context.ResourceFurnitures.Remove(furniture);
                break;

            case ResourceType.Catering:
                var catering = await _context.ResourceCaterings.FirstOrDefaultAsync(r => r.ResourceId == ResourceId);
                if (catering != null) _context.ResourceCaterings.Remove(catering);
                break;

            case ResourceType.Personnel:
                var personnel = await _context.ResourcePersonnels.FirstOrDefaultAsync(r => r.ResourceId == ResourceId);
                if (personnel != null) _context.ResourcePersonnels.Remove(personnel);
                break;
        }

        // Delete associated images first
        if (resource.Image?.ImageFileName != null)
        {
            string imagePath = Path.Combine(_environment.WebRootPath, "resources", resource.Image.ImageFileName);
            if (File.Exists(imagePath)) File.Delete(imagePath);
            _context.Images.Remove(resource.Image);
        }

        _context.Resources.Remove(resource);
        await _context.SaveChangesAsync();
        return true;
    }

    private Dictionary<string, string> ValidateResourceModel(CUResourceViewModel model)
    {
        var errors = new Dictionary<string, string>();

        // Require an image only if it's a new resource (no existing ImageFileName)
        if (string.IsNullOrEmpty(model.ImageFileName) && model.ImageFile == null)
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

    private async Task<(bool IsSuccess, string? Error)> SaveResourceImageAsync(IFormFile? imageFile, Guid ResourceId, string? oldFileName = null)
    {
        try
        {
            if (imageFile == null)
            {
                // No new image uploaded, return success without changes
                return (true, null);
            }

            string newFileName = DateTime.UtcNow.ToString("yyyyMMddHHmmss") + Path.GetExtension(imageFile.FileName);
            string imageFullPath = Path.Combine(_environment.WebRootPath, "resources", newFileName);

            // Ensure directory exists
            string directoryPath = Path.GetDirectoryName(imageFullPath)!;
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Save the new image
            using (var stream = File.Create(imageFullPath))
            {
                await imageFile.CopyToAsync(stream);
            }

            // Delete old image if it exists
            if (!string.IsNullOrEmpty(oldFileName))
            {
                string oldImagePath = Path.Combine(_environment.WebRootPath, "resources", oldFileName);
                if (File.Exists(oldImagePath))
                {
                    File.Delete(oldImagePath);
                }
            }

            // Update or create image record
            var existingImage = await _context.Images.FirstOrDefaultAsync(i => i.ResourceId == ResourceId);
            if (existingImage != null)
            {
                existingImage.ImageFileName = newFileName;
            }
            else
            {
                var image = new Image
                {
                    ResourceId = ResourceId,
                    ImageFileName = newFileName
                };
                _context.Images.Add(image);
            }

            await _context.SaveChangesAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Image saving failed: {ex.Message}");
        }
    }


    // Methods
    private DateTime GetWarranty(int Warranty, int WarrantyDuration)
    {
        string FinalWarranty;

        // If By Month
        if (WarrantyDuration == 2)
        {
            if (Warranty < 2)
                FinalWarranty = Warranty.ToString() + " month";
            else
                FinalWarranty = Warranty.ToString() + " months";
        }
        else
        {
            if (Warranty < 2)
                FinalWarranty = Warranty.ToString() + " year";
            else
                FinalWarranty = Warranty.ToString() + " years";
        }

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

    private string GetExperience(int Experience, int ExperienceType)
    {
        string experience;

        if (ExperienceType == 1)
        {
            if (Experience > 1)
            {
                experience = Experience + " years";
            }
            else
            {
                experience = Experience + " year";
            }

        }
        else if (ExperienceType == 2)
        {
            if (Experience > 1)
            {
                experience = Experience + " months";
            }
            else
            {
                experience = Experience + " month";
            }

        }
        else
        {
            experience = "None";
        }

        return experience;
    }

    private static Dictionary<string, string> ConvertDateTimeToDuration(DateTime date)
    {
        var result = new Dictionary<string, string>();

        if(date == DateTime.MinValue)
        {
            result.Add("0", "1");
            return result;
        }
        DateTime today = DateTime.Today;
        // Calculate the total number of months between date and today
        int totalMonths = (today.Year - date.Year) * 12 + (today.Month - date.Month);

        if (totalMonths <= 0)
        {
            result.Add("0", "1");
            return result;
        }

        // If totalMonths is evenly divisible by 12, return the duration in years.
        if (totalMonths % 12 == 0)
        {
            int years = totalMonths / 12;
            result.Add(years.ToString(), "1");
        }
        else
        {
            // Otherwise, return the total months.
            result.Add(totalMonths.ToString(), "2");
        }

        return result;
    }

    private static Dictionary<string, string> GetExperienceDictionary(string experienceString)
    {
        var result = new Dictionary<string, string>();

        // Handle the "None" case.
        if (string.Equals(experienceString.Trim(), "None", StringComparison.OrdinalIgnoreCase))
        {
            result.Add("0", "1");
            return result;
        }

        // Split the input string by spaces.
        var parts = experienceString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // If the input doesn't have exactly 2 parts, return a default "None" value.
        if (parts.Length != 2)
        {
            result.Add("0", "1");
            return result;
        }

        // Extract the numeric portion.
        string numberPart = parts[0];
        // Extract the unit part and determine the corresponding value ("1" for years, "2" for months)
        string unitPart = parts[1].ToLower();

        string unitValue;
        if (unitPart == "year" || unitPart == "years")
        {
            unitValue = "1";
        }
        else if (unitPart == "month" || unitPart == "months")
        {
            unitValue = "2";
        }
        else
        {
            result.Add("0", "1");
            return result;
        }

        result.Add(numberPart, unitValue);
        return result;
    }
}