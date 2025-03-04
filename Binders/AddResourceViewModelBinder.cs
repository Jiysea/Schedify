using Microsoft.AspNetCore.Mvc.ModelBinding;
using Schedify.ViewModels;
using System.Text.Json;
namespace Schedify.Binders;

public class AddResourceViewModelBinder : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        Console.WriteLine("Model Binder Called"); // This should log to the console

        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }
        Console.WriteLine("Model Binder Called 2"); // This should log to the console
        var request = bindingContext.HttpContext.Request;
        request.EnableBuffering(); // Allows multiple reads

        using (var reader = new StreamReader(request.Body, System.Text.Encoding.UTF8, true, 1024, true))
        {
            var body = await reader.ReadToEndAsync();
            Console.WriteLine($"Raw Request Body: {body}");
        }

        Console.WriteLine($"Request Content-Type: {bindingContext.HttpContext.Request.ContentType}");



        
        var typeValue = bindingContext.ValueProvider.GetValue("type").FirstValue;
        if (string.IsNullOrEmpty(typeValue) || !Enum.TryParse(typeValue, out ResourceType resourceType))
        {
            bindingContext.Result = ModelBindingResult.Failed();
            Console.WriteLine("Model Binder Failed"); // This should log to the console
            return;
        }

        AddResourceViewModel model = resourceType switch
        {
            ResourceType.Venue => new VenueViewModel(),
            ResourceType.Equipment => new EquipmentViewModel(),
            ResourceType.Furniture => new FurnitureViewModel(),
            ResourceType.Personnel => new PersonnelViewModel(),
            ResourceType.Catering => new CateringViewModel(),
            _ => new AddResourceViewModel()
        };

        // Use FormCollection or JSON Deserialization to populate the model
        await bindingContext.HttpContext.Request.ReadFormAsync();
        bindingContext.Model = model;
        bindingContext.Result = ModelBindingResult.Success(model);

        Console.WriteLine($"Model Binder Successfully Created Model: {model.GetType().Name}");
    }
}
