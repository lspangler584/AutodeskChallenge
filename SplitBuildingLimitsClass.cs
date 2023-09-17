using NetTopologySuite.Features;

namespace SplitBuildingLimits;

public static class SplitBuildingLimitsClass<TPolygon> where TPolygon : IFeature
{
    private static readonly string attributeElevation = "elevation";
    /**
    * Example usage: 
    * GetPolygonMember<double>(polygon, "elevation"); // Returns the elevation property corresponding to the polygon if it exists, else null
    */
    private static T GetPolygonMember<T>(TPolygon polygon, string key)
    {
        return (T) polygon.Attributes.GetOptionalValue(key);
    }

    /**
     * Consumes a list of building limits (polygons) and a list of height plateaus (polygons). Splits up the building
     * limits according to the height plateaus and persists:
     * 1. The original building limits
     * 2. The original height plateaus
     * 3. The split building limits
     * 
     * <param name="buildingLimits"> A list of buildings limits. A building limit is a polygon indicating where building can happen. </param>
     * <param name="heightPlateaus"> A list of height plateaus. A height plateau is a discrete polygon with a constant elevation. </param>
     */
    public static List<IFeature> SplitBuildingLimits(List<TPolygon> buildingLimits, List<TPolygon> heightPlateaus)
    {
        Console.WriteLine("Splitting building limits according to height plateaus");

        List<IFeature> splitLimits = new List<IFeature>();

        Parallel.ForEach(buildingLimits, buildingLimit =>
        {
            foreach (var plateau in heightPlateaus)
            {
                var intersection = buildingLimit.Geometry.Intersection(plateau.Geometry);

                if (intersection is not null)
                {
                    var feature = new Feature() { Geometry = intersection, Attributes = new AttributesTable() };

                    // using private method here, but if elevation in plateau doesn't exist, this value will be 0
                    // may be better to call GetOptionalValue here instead and not add the property if it's not present 
                    // in the source (plateau)
                    var elevation = GetPolygonMember<double>(plateau, attributeElevation);
                    feature.Attributes.Add(attributeElevation, elevation);

                    lock (splitLimits)
                    {
                        splitLimits.Add(feature);
                    }
                }
            }
        });

        return splitLimits;
    }
}