
using System.ComponentModel.DataAnnotations;

namespace Schedify.Attributes;

public class MaxFileSizeAttribute : ValidationAttribute
{
    private readonly int _maxSize;

    public MaxFileSizeAttribute(int maxSizeInMB)
    {
        _maxSize = maxSizeInMB * 1024 * 1024; // Convert MB to bytes
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is IFormFile file)
        {
            if (file.Length > _maxSize)
            {
                return new ValidationResult($"File size must be less than {_maxSize / (1024 * 1024)}MB.");
            }
        }
        return ValidationResult.Success!;
    }
}
