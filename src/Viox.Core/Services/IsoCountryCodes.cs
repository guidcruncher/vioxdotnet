namespace Viox.Core.Services;

using System.Globalization;

public static class IsoCountryCodes
{

    public static Dictionary<string, string> GetIso3166Codes()
    {
        return CultureInfo.GetCultures(CultureTypes.SpecificCultures)
            .Select(culture => new RegionInfo(culture.Name))
            .GroupBy(region => region.TwoLetterISORegionName)
            .ToDictionary(
                group => group.Key,
                group => group.First().EnglishName,
                StringComparer.OrdinalIgnoreCase
            );
    }

}
