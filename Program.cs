using System.Text;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Newtonsoft.Json;

namespace SplitBuildingLimits;

public class Program
{
    // Update these to the relevant paths
    private static readonly string BuildingLimitsFilePath = "../../../samples/SampleBuildingLimits.json";
    private static readonly string SampleHeightPlateausFilePath = "../../../samples/SampleHeightPlateaus.json";
    private static readonly string SplitLimitsFilePath = "../../../samples/SplitLimits.json";
    private static object writeLock = new object();

    private static string ReadFile(string filePath)
    {
        string text;

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);

        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
        {
            text = streamReader.ReadToEnd();
        }
        if (string.IsNullOrEmpty(text))
        {
            throw new Exception("Contents of file are empty");
        }
        return text;
    }

    private static FeatureCollection DeserializeGeojson(string geojson)
    {
        var serializer = GeoJsonSerializer.Create();
        using (var stringReader = new StringReader(geojson))
        using (var jsonReader = new JsonTextReader(stringReader))
        {
            var geometry = serializer.Deserialize<FeatureCollection>(jsonReader) ?? throw new Exception("Geometry is null");
            return geometry;
        }
        throw new Exception("Unable to deserialize json");
    }

    private static void SerializeGeoJson(FeatureCollection features)
    {
        using (var fileStream = new FileStream(SplitLimitsFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            var serializer = GeoJsonSerializer.Create();
            var streamWriter = new StreamWriter(fileStream);
            var jsonWriter = new JsonTextWriter(streamWriter);

            serializer.Serialize(jsonWriter, features);
            jsonWriter.Flush();
            fileStream.Flush();
            fileStream.Close();
        }
    }

    private static List<IFeature> FilterPolygonsFromFeatureCollection(FeatureCollection featureCollection)
    {
        return featureCollection.Where((feature) => feature.Geometry.GeometryType == "Polygon").ToList();
    }

    private static bool IsPolygonsValid(List<IFeature> polygons)
    {
        bool isValid = false;
        if (polygons != null && polygons.Count > 0 && polygons.All(p => p is not null))
        {
            isValid = true;
            foreach (var polygon in polygons)
            {
                if (!polygon.Geometry.IsValid)
                {
                    isValid = false;
                    break;
                }
            }
        }

        return isValid;
    }


    public static void Main()
    {
        var sampleBuildingLimits = ReadFile(BuildingLimitsFilePath);
        var sampleHeightPlateaus = ReadFile(SampleHeightPlateausFilePath);

        FeatureCollection buildingLimitsFeatureCollection;
        FeatureCollection heightPlateausFeatureCollection;

        // Deserialize input
        try
        {
            buildingLimitsFeatureCollection = DeserializeGeojson(sampleBuildingLimits);
            heightPlateausFeatureCollection = DeserializeGeojson(sampleHeightPlateaus);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Deserialization exception: {ex.Message}. Exiting program.");
            return;
        }

        // Filter out Polygons
        List<IFeature> buildingLimitPolygons = FilterPolygonsFromFeatureCollection(buildingLimitsFeatureCollection);
        if (!IsPolygonsValid(buildingLimitPolygons))
        {
            Console.WriteLine($"Invalid input found in {BuildingLimitsFilePath}, exiting program.");
            return;
        }

        List<IFeature> heightPlateausPolygons = FilterPolygonsFromFeatureCollection(heightPlateausFeatureCollection);
        if (!IsPolygonsValid(buildingLimitPolygons))
        {
            Console.WriteLine($"Invalid input found in {SampleHeightPlateausFilePath}, exiting program.");
            return;
        }

        // Split into Building Limits
        List<IFeature> splitLimits = SplitBuildingLimitsClass<IFeature>.SplitBuildingLimits(buildingLimitPolygons, heightPlateausPolygons);

        if (!IsPolygonsValid(splitLimits))
        {
            Console.WriteLine($"Invalid polygons found in result, output will not be serialized to {SplitLimitsFilePath}") ;
        }

        // Convert to FeatureCollection
        FeatureCollection fc = new FeatureCollection();
        foreach (IFeature feature in splitLimits)
        {
            fc.Add(feature);
        }

        // Serialize the output
        try
        {
            SerializeGeoJson(fc);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Serialization failed: ex: {ex.Message}, exiting program.");
            return;
        }
    }
}