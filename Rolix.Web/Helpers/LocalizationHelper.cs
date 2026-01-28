using Microsoft.AspNetCore.Http;
using Rolix.Web.Models;
using System.Globalization;
using System.Resources;

namespace Rolix.Web.Helpers;

public static class LocalizationHelper
{
    private static readonly ResourceManager ResourceManager = new ResourceManager(typeof(Rolix.Web.Resources.SharedResources));

    public static string Get(string key, HttpContext context)
    {
        var language = context.Session.GetString(SessionKeys.Language) ?? "fr";
        var culture = new CultureInfo(language);
        
        try
        {
            var value = ResourceManager.GetString(key, culture);
            return !string.IsNullOrEmpty(value) ? value : key;
        }
        catch
        {
            return key;
        }
    }
}
