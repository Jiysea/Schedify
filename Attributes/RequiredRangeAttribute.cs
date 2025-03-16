using System.ComponentModel.DataAnnotations;

namespace Schedify.Attributes;

public class RequiredRangeAttribute : ValidationAttribute
{
    private readonly ResourceType[] _requiredForTypes;

    public RequiredRangeAttribute(params ResourceType[] requiredForTypes)
    {
        _requiredForTypes = requiredForTypes;
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        var resourceTypeProperty = validationContext.ObjectType.GetProperty("ResourceType");
        var quantityProperty = validationContext.ObjectType.GetProperty(validationContext.MemberName!);

        if (resourceTypeProperty == null || quantityProperty == null)
        {
            return new ValidationResult("Validation failed: Missing required properties.");
        }

        var resourceTypeValue = resourceTypeProperty.GetValue(validationContext.ObjectInstance);

        if (resourceTypeValue is ResourceType resourceType && _requiredForTypes.Contains(resourceType) && value == null)
        {
            return new ValidationResult($"Value must be at least 1.");
        }

        return ValidationResult.Success!;
    }
}