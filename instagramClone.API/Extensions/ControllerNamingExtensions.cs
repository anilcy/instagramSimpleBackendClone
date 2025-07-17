using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace instagramClone.API.Extensions;

public class AutomaticControllerNamingConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        // Remove "Controller" suffix from controller name
        var controllerName = controller.ControllerName;
        
        // Convert PascalCase to kebab-case for better REST API conventions
        var routeName = ConvertToKebabCase(controllerName);
        
        // Apply automatic pluralization for common patterns
        routeName = ApplyPluralization(routeName);
        
        // Set the route template with automatic versioning
        var routeTemplate = $"api/v1/{routeName}";
        
        // Apply the route to the controller
        controller.Selectors.Clear();
        controller.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel
            {
                Template = routeTemplate
            }
        });
        
        // Add additional routes for backward compatibility if needed
        if (routeName != controllerName.ToLower())
        {
            controller.Selectors.Add(new SelectorModel
            {
                AttributeRouteModel = new AttributeRouteModel
                {
                    Template = $"api/{controllerName.ToLower()}"
                }
            });
        }
    }
    
    private static string ConvertToKebabCase(string pascalCase)
    {
        if (string.IsNullOrEmpty(pascalCase))
            return pascalCase;
            
        // Insert hyphens before uppercase letters (except the first one)
        var kebabCase = Regex.Replace(pascalCase, "(?<!^)([A-Z])", "-$1");
        return kebabCase.ToLower();
    }
    
    private static string ApplyPluralization(string name)
    {
        // Simple pluralization rules for common cases
        var pluralizations = new Dictionary<string, string>
        {
            { "auth", "auth" },              // Auth doesn't need pluralization
            { "user", "users" },
            { "post", "posts" },
            { "comment", "comments" },
            { "like", "likes" },
            { "follow", "follows" },
            { "story", "stories" },
            { "message", "messages" },
            { "notification", "notifications" }
        };
        
        if (pluralizations.ContainsKey(name))
        {
            return pluralizations[name];
        }
        
        // Default pluralization rules
        if (name.EndsWith("y") && !name.EndsWith("ay") && !name.EndsWith("ey") && !name.EndsWith("iy") && !name.EndsWith("oy") && !name.EndsWith("uy"))
        {
            return name.Substring(0, name.Length - 1) + "ies";
        }
        
        if (name.EndsWith("s") || name.EndsWith("sh") || name.EndsWith("ch") || name.EndsWith("x") || name.EndsWith("z"))
        {
            return name + "es";
        }
        
        return name + "s";
    }
}

public static class ControllerNamingExtensions
{
    public static IServiceCollection AddAutomaticControllerNaming(this IServiceCollection services)
    {
        services.Configure<MvcOptions>(options =>
        {
            options.Conventions.Add(new AutomaticControllerNamingConvention());
        });
        
        return services;
    }
}

// Attribute for custom controller naming
[AttributeUsage(AttributeTargets.Class)]
public class AutoRouteAttribute : Attribute
{
    public string? CustomName { get; set; }
    public string? Version { get; set; }
    public bool SkipPluralization { get; set; }
    
    public AutoRouteAttribute(string? customName = null)
    {
        CustomName = customName;
        Version = "v1";
        SkipPluralization = false;
    }
}

// Enhanced convention that respects the AutoRoute attribute
public class EnhancedControllerNamingConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        var autoRouteAttribute = controller.Attributes.OfType<AutoRouteAttribute>().FirstOrDefault();
        
        string routeName;
        string version = "v1";
        bool skipPluralization = false;
        
        if (autoRouteAttribute != null)
        {
            routeName = autoRouteAttribute.CustomName ?? controller.ControllerName;
            version = autoRouteAttribute.Version ?? "v1";
            skipPluralization = autoRouteAttribute.SkipPluralization;
        }
        else
        {
            routeName = controller.ControllerName;
        }
        
        // Convert to kebab-case
        routeName = ConvertToKebabCase(routeName);
        
        // Apply pluralization if not skipped
        if (!skipPluralization)
        {
            routeName = ApplyPluralization(routeName);
        }
        
        // Create the route template
        var routeTemplate = $"api/{version}/{routeName}";
        
        // Clear existing selectors and add the new one
        controller.Selectors.Clear();
        controller.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel
            {
                Template = routeTemplate
            }
        });
        
        Console.WriteLine($"🔄 Auto-named controller: {controller.ControllerName} → {routeTemplate}");
    }
    
    private static string ConvertToKebabCase(string pascalCase)
    {
        if (string.IsNullOrEmpty(pascalCase))
            return pascalCase;
            
        var kebabCase = Regex.Replace(pascalCase, "(?<!^)([A-Z])", "-$1");
        return kebabCase.ToLower();
    }
    
    private static string ApplyPluralization(string name)
    {
        var pluralizations = new Dictionary<string, string>
        {
            { "auth", "auth" },
            { "user", "users" },
            { "post", "posts" },
            { "comment", "comments" },
            { "like", "likes" },
            { "follow", "follows" },
            { "story", "stories" },
            { "message", "messages" },
            { "notification", "notifications" },
            { "profile", "profiles" },
            { "media", "media" },
            { "tag", "tags" }
        };
        
        if (pluralizations.ContainsKey(name))
        {
            return pluralizations[name];
        }
        
        if (name.EndsWith("y") && !name.EndsWith("ay") && !name.EndsWith("ey") && !name.EndsWith("iy") && !name.EndsWith("oy") && !name.EndsWith("uy"))
        {
            return name.Substring(0, name.Length - 1) + "ies";
        }
        
        if (name.EndsWith("s") || name.EndsWith("sh") || name.EndsWith("ch") || name.EndsWith("x") || name.EndsWith("z"))
        {
            return name + "es";
        }
        
        return name + "s";
    }
}