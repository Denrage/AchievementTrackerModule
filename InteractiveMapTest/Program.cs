// See https://aka.ms/new-console-template for more information
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System.Drawing;
using System.Security.Cryptography;
using System.Text.Json;

Console.WriteLine("Hello, World!");
// 'https://wiki.guildwars2.com/images/9/9b/World_map_tile_C1_F1_Z8_X237_Y175.jpg'
// https://wiki.guildwars2.com/images/9/9d/World_map_tile_C1_F1_Z8_X60820_Y45037.jpg
var iconUrl = string.Empty; // Url to the icon which to display in the middle of the map
var localTiles = string.Empty; // ?
var inputCoords = "[57760.8,21854.5]"; // Coordinates of the icon on the map // string pairs of [x,y] separated by ,  - e.g. [x1,y1],[x2,y2],[x3,y3]
var inputPath = string.Empty; // ? // string pairs of [x,y] separated by ,  - e.g. [x1,y1],[x2,y2],[x3,y3]
var inputBounds = string.Empty; // Extra lines to be drawn for bounds

var coords = ConvertStringToNestedArray(inputCoords);
var path = ConvertStringToNestedArray(inputPath);
var bounds = ConvertStringToNestedArray(inputBounds);

var mastercoords = bounds;

if (coords.Count > 0)
{
    mastercoords = coords;
}
else if (path.Count > 0)
{
    mastercoords = coords;
}

var floorId = 1.0;
if (mastercoords[0].Count > 2)
{
    floorId = mastercoords[0][2];
}
var continentId = 1.0;
if (mastercoords[0].Count > 3)
{
    continentId = mastercoords[0][3];
}

var centroid = GetCentroid(mastercoords.Select(x => (x[0], x[1])).ToList());
var size = GetSize(mastercoords.Select(x => (x[0], x[1])).ToList());

var minzoom = 2;
var maxzoom = 8;
((double X, double Y) StartCoordinate, (double X, double Y) EndCoordinate) continent_dims = ((0, 0), (131072, 131072));
if (continentId == 2)
{
    minzoom = 0;
    maxzoom = 6;
    continent_dims = ((0, 0), (16384, 16384));
}

(double X, double Y) mapBounds = ConvertCoordinates(continent_dims.EndCoordinate, maxzoom);

var zoom = maxzoom;

var (waypoints, tileWhiteList) = InitializeWaypointsAndTiles();
var mapCoords = ConvertCoordinates(centroid, maxzoom);
(int X, int Y) intMapCoords = ((int)Math.Floor(mapCoords.X), (int)Math.Floor(mapCoords.Y));

var startCoordinateX = 0;
var startCoordinateY = 0;

var tileUrls = new string[3, 3];
if (bounds.Count > 0)
{
    var boundCoords = bounds.Select(x => ConvertCoordinates((x[0], x[1]), maxzoom));
    var intBoundCoords = boundCoords.Select(x => ((int)Math.Floor(x.X), (int)Math.Floor(x.Y)));

    var lowerBoundX = intBoundCoords.Min(x => x.Item1) - 1;
    var upperBoundX = intBoundCoords.Max(x => x.Item1) + 1;
    var lowerBoundY = intBoundCoords.Min(x => x.Item2) - 1;
    var upperBoundY = intBoundCoords.Max(x => x.Item2) + 1;

    startCoordinateX = lowerBoundX;
    startCoordinateY = lowerBoundY;

    tileUrls = new string[upperBoundX - lowerBoundX, upperBoundY - lowerBoundY];

    for (int i = 0; i < upperBoundX - lowerBoundX; i++)
    {
        for (int j = 0; j < upperBoundY - lowerBoundY; j++)
        {
            tileUrls[i, j] = GetTileUrl((lowerBoundX + i, lowerBoundY + j, zoom), floorId, continentId, tileWhiteList);
        }
    }
}
else if (path.Count > 0)
{
    var pathCoords = path.Select(x => ConvertCoordinates((x[0], x[1]), maxzoom));
    var intPathCoords = pathCoords.Select(x => ((int)Math.Floor(x.X), (int)Math.Floor(x.Y)));

    var lowerBoundX = intPathCoords.Min(x => x.Item1) - 1;
    var upperBoundX = intPathCoords.Max(x => x.Item1) + 1;
    var lowerBoundY = intPathCoords.Min(x => x.Item2) - 1;
    var upperBoundY = intPathCoords.Max(x => x.Item2) + 1;

    startCoordinateX = lowerBoundX;
    startCoordinateY = lowerBoundY;

    tileUrls = new string[upperBoundX - lowerBoundX, upperBoundY - lowerBoundY];

    for (int i = 0; i < upperBoundX - lowerBoundX; i++)
    {
        for (int j = 0; j < upperBoundY - lowerBoundY; j++)
        {
            tileUrls[i, j] = GetTileUrl((lowerBoundX + i, lowerBoundY + j, zoom), floorId, continentId, tileWhiteList);
        }
    }
}
else
{
    var lowerBoundX = intMapCoords.X - 2;
    var upperBoundX = intMapCoords.X + 2;
    var lowerBoundY = intMapCoords.Y - 2;
    var upperBoundY = intMapCoords.Y + 2;

    startCoordinateX = lowerBoundX;
    startCoordinateY = lowerBoundY;
    tileUrls = new string[upperBoundX - lowerBoundX, upperBoundY - lowerBoundY];
    for (int i = 0; i < upperBoundX - lowerBoundX; i++)
    {
        for (int j = 0; j < upperBoundY - lowerBoundY; j++)
        {
            tileUrls[i, j] = GetTileUrl((lowerBoundX + i, lowerBoundY + j, zoom), floorId, continentId, tileWhiteList);
        }
    }
}

var client = new System.Net.WebClient();
var tiles = new SixLabors.ImageSharp.Image[tileUrls.GetLength(0), tileUrls.GetLength(1)];

for (int i = 0; i < tileUrls.GetLength(0); i++)
{
    for (int j = 0; j < tileUrls.GetLength(1); j++)
    {
        client.Headers.Add("user-agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
        try
        {
            var bytes = client.DownloadData(tileUrls[i, j]);
            tiles[i, j] = SixLabors.ImageSharp.Image.Load(bytes);
        }
        catch (Exception)
        {
        }
    }
}

using (SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image = new(tiles.GetLength(0) * 256, tiles.GetLength(1) * 256))
{


    for (int i = 0; i < tiles.GetLength(0); i++)
    {
        for (int j = 0; j < tiles.GetLength(1); j++)
        {
            if (tiles[i, j] is null)
            {
                continue;
            }

            var imagePart = tiles[i, j].CloneAs<SixLabors.ImageSharp.PixelFormats.Rgba32>();
            image.Mutate(x => x.DrawImage(imagePart, new SixLabors.ImageSharp.Point(i * imagePart.Width, j * imagePart.Height), 1));
        }
    }

    var iconX = (int)(((intMapCoords.X - startCoordinateX) * 256) + (256 * (mapCoords.X % 1)));
    var iconY = (int)(((intMapCoords.Y - startCoordinateY) * 256) + (256 * (mapCoords.Y % 1)));

    if (path.Count > 0)
    {
        client.Headers.Add("user-agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
        var startImageBytes = client.DownloadData("https://wiki.guildwars2.com/images/f/f0/Event_flag_green.png");
        var startImage = SixLabors.ImageSharp.Image.Load(startImageBytes);
        var startImageCoordinates = ConvertCoordinates((path[0][0], path[0][1]), maxzoom);
        var startImageX = (int)((((int)Math.Floor(startImageCoordinates.X) - startCoordinateX) * 256) + (256 * (startImageCoordinates.X % 1)));
        var startImageY = (int)((((int)Math.Floor(startImageCoordinates.Y) - startCoordinateY) * 256) + (256 * (startImageCoordinates.Y % 1)));

        client.Headers.Add("user-agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
        var endImageBytes = client.DownloadData("https://wiki.guildwars2.com/images/8/8d/Event_flag_red.png");
        var endImage = SixLabors.ImageSharp.Image.Load(endImageBytes);
        var endImageCoordinates = ConvertCoordinates((path[^1][0], path[^1][1]), maxzoom);
        var endImageX = (int)((((int)Math.Floor(endImageCoordinates.X) - startCoordinateX) * 256) + (256 * (endImageCoordinates.X % 1)));
        var endImageY = (int)((((int)Math.Floor(endImageCoordinates.Y) - startCoordinateY) * 256) + (256 * (endImageCoordinates.Y % 1)));

        var points = path.Select(x =>
        {
            var translatedCoord = ConvertCoordinates((x[0], x[1]), maxzoom);
            var intTranslatedCoordX = (int)Math.Floor(translatedCoord.X);
            var intTranslatedCoordY = (int)Math.Floor(translatedCoord.Y);
            translatedCoord.X = (int)(((intTranslatedCoordX - startCoordinateX) * 256) + (256 * (translatedCoord.X % 1)));
            translatedCoord.Y = (int)(((intTranslatedCoordY - startCoordinateY) * 256) + (256 * (translatedCoord.Y % 1)));
            return new SixLabors.ImageSharp.PointF((float)translatedCoord.X, (float)translatedCoord.Y);
        }).ToArray();

        image.Mutate(x => x.DrawLines(new Pen(SixLabors.ImageSharp.Color.Yellow, 10), points));
        image.Mutate(x => x.DrawImage(startImage, new SixLabors.ImageSharp.Point(startImageX - (startImage.Width / 2), startImageY - (startImage.Height / 2)), 1));
        image.Mutate(x => x.DrawImage(endImage, new SixLabors.ImageSharp.Point(endImageX - (endImage.Width / 2), endImageY - (endImage.Height / 2)), 1));
    }

    if (!string.IsNullOrEmpty(iconUrl))
    {
        client.Headers.Add("user-agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
        var iconBytes = client.DownloadData(iconUrl);
        var iconImage = SixLabors.ImageSharp.Image.Load(iconBytes);

        var pointX = iconX - (iconImage.Width / 2);
        var pointY = iconY - (iconImage.Height / 2);

        image.Mutate(x => x.DrawImage(iconImage, new SixLabors.ImageSharp.Point(pointX, pointY), 1));
    }
    else
    {
        client.Headers.Add("user-agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
        var dotBytes = client.DownloadData("https://wiki.guildwars2.com/images/2/23/Widget_map_dot.png");
        var dotImage = SixLabors.ImageSharp.Image.Load(dotBytes);
        var dotX = iconX - (dotImage.Width / 2);
        var dotY = iconY - (dotImage.Height / 2);

        image.Mutate(x => x.DrawImage(dotImage, new SixLabors.ImageSharp.Point(dotX, dotY), 1));
    }

    if (bounds.Count > 0)
    {
        var points = bounds.Select(x =>
        {
            var translatedCoord = ConvertCoordinates((x[0], x[1]), maxzoom);
            var intTranslatedCoordX = (int)Math.Floor(translatedCoord.X);
            var intTranslatedCoordY = (int)Math.Floor(translatedCoord.Y);
            translatedCoord.X = (int)(((intTranslatedCoordX - startCoordinateX) * 256) + (256 * (translatedCoord.X % 1)));
            translatedCoord.Y = (int)(((intTranslatedCoordY - startCoordinateY) * 256) + (256 * (translatedCoord.Y % 1)));
            return new SixLabors.ImageSharp.PointF((float)translatedCoord.X, (float)translatedCoord.Y);
        }).ToArray();
        image.Mutate(x => x.DrawPolygon(new Pen(SixLabors.ImageSharp.Color.Yellow, 4), points));
    }
    else
    {
        client.Headers.Add("user-agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
        var swirlBytes = client.DownloadData("https://wiki.guildwars2.com/images/8/8d/Widget_map_yellow_swirl.png");
        var swirlImage = SixLabors.ImageSharp.Image.Load(swirlBytes);
        var swirlX = iconX - (swirlImage.Width / 2);
        var swirlY = iconY - (swirlImage.Height / 2);

        image.Mutate(x => x.DrawImage(swirlImage, new SixLabors.ImageSharp.Point(swirlX, swirlY), 1));
    }

    var firstValidPixelX = -1;
    var firstValidPixelY = -1;
    var lastValidPixelX = -1;
    var lastValidPixelY = -1;

    for (int i = 0; i < image.Width; i++)
    {
        for (int j = 0; j < image.Height; j++)
        {
            if (image[i, j] != default && firstValidPixelX == -1)
            {
                firstValidPixelX = i;
                firstValidPixelY = j;
            }

            if (i < image.Width - 1 && image[i + 1, j] == default && lastValidPixelX == -1)
            {
                lastValidPixelX = i;
            }

            if (j < image.Height - 1 && image[i, j + 1] == default && lastValidPixelY == -1)
            {
                lastValidPixelY = j;
            }
        }
    }

    if (lastValidPixelX == -1)
    {
        lastValidPixelX = image.Width;
    }

    if(lastValidPixelY == -1)
    {
        lastValidPixelY = image.Height;
    }

    image.Mutate(x => x.Crop(new SixLabors.ImageSharp.Rectangle(firstValidPixelX, firstValidPixelY, lastValidPixelX - firstValidPixelX, lastValidPixelY - firstValidPixelY)));

    //image.Mutate(x => x.Resize(256, 256));

    using (var fileStream = new FileStream("test.jpg", FileMode.Create))
    {
        image.Save(fileStream, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder());
    }
}


Console.WriteLine("Done");



List<List<double>> ConvertStringToNestedArray(string input)
{
    if (string.IsNullOrEmpty(input))
    {
        return Array.Empty<List<double>>().ToList();
    }

    var result = new List<List<double>>();

    input = new string(input.Skip(1).Take(input.Length - 2).ToArray());

    if (!input.Contains('['))
    {
        var splittedParts = input.Split(",", StringSplitOptions.RemoveEmptyEntries);
        var innerList = new List<double>();
        foreach (var item in splittedParts)
        {
            innerList.Add(double.Parse(item, System.Globalization.CultureInfo.InvariantCulture));
        }

        result.Add(innerList);
    }
    else
    {
        var splittedFirstDimension = input.Replace("[", string.Empty).Split("]", StringSplitOptions.RemoveEmptyEntries);
        foreach (var item in splittedFirstDimension)
        {
            var innerList = new List<double>();
            var splittedSecondDimension = item.Split(",", StringSplitOptions.RemoveEmptyEntries);
            foreach (var innerItem in splittedSecondDimension)
            {
                innerList.Add(double.Parse(innerItem, System.Globalization.CultureInfo.InvariantCulture));
            }

            result.Add(innerList);
        }
    }

    return result;
}

string GenerateUrl(string fileName)
{
    var parts = GenerateUrlComponents(fileName);
    return $"https://wiki.guildwars2.com/images/{parts.FirstPart}/{parts.SecondPart}/{fileName}";
}

(string FirstPart, string SecondPart) GenerateUrlComponents(string fileName)
{
    var md5 = MD5.Create();
    var hash = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(fileName));
    var hex = BitConverter.ToString(hash[0..1]);
    return (hex[0].ToString().ToLower(), hex.ToLower());
}

static (double X, double Y) GetCentroid(List<(double X, double Y)> poly)
{
    return poly.Aggregate((x, y) => (x.X + (y.X / poly.Count), x.Y + (y.Y / poly.Count)));
}

static (double X, double Y) GetSize(List<(double X, double Y)> polygon)
{
    var x = polygon.Select(x => x.X).ToList();
    var y = polygon.Select(y => y.Y).ToList();
    var minimumArray = new double[] { x.Min(), y.Min() };
    var maximumArray = new double[] { x.Max(), y.Max() };

    return (maximumArray[0] - minimumArray[0], maximumArray[1] - minimumArray[1]);
}

string GetTileUrl((int X, int Y, int Z) coordinates, double floorId, double continentId, TileWhitelist tileWhitelist)
{
    if (floorId == 1)
    {
        // Wiki tiles available for floor 1
        var continent = tileWhitelist.Tyria;

        if (continentId == 2)
        {
            continent = tileWhitelist.Mists;
        }

        var floor = continent.Floors.First(x => x.Id == coordinates.Z);

        if (!floor.Coordinates.Contains("X" + coordinates.X + "_Y" + coordinates.Y))
        {
            return "https://wiki.guildwars2.com/images/c/cb/World_map_tile_under_construction.png";
        }

        var file = $"World_map_tile_C{continentId}_F{floorId}_Z{coordinates.Z}_X{coordinates.X}_Y{coordinates.Y}.jpg";
        return GenerateUrl(file);
    }
    else
    {
        // Otherwise use native API with a bodge applied

        // Bodge: X 32_768 and Y 16_384 (i.e. 128*256 plus 64*256)
        // i.e. 128,64 for z=8; 64,32 for z=7; 32,16 for z=6; 16,8 for z=5; 8,4 for z=4; 4,2 for z=3; 2,1 for z=2; ... impossible fractions for below 2.
        // correct values for z

        var xBodge = -1;
        var yBodge = -1;
        var zBodge = -1;

        if (continentId == 1)
        {
            xBodge = coordinates.X - (int)(128 / Math.Pow(2, 8 - coordinates.Z));
            yBodge = coordinates.Y - (int)(64 / Math.Pow(2, 8 - coordinates.Z));
            zBodge = coordinates.Z - 1;
        }
        else
        {
            xBodge = coordinates.X;
            yBodge = coordinates.Y;
            zBodge = coordinates.Z;
        }

        // Final negative and integer check
        if (xBodge < 0 || yBodge < 0)
        {
            return "https://wiki.guildwars2.com/images/c/cb/World_map_tile_under_construction.png";
        }
        else
        {
            return $"https://tiles.guildwars2.com/{continentId}/{floorId}/{zBodge}/{xBodge}/{yBodge}.jpg";
        }
    }
}


(double X, double Y) ConvertCoordinates((double X, double Y) gw2Coordinates, int zoom)
{
    var scale = Math.Pow(2, zoom);

    var x = gw2Coordinates.X / scale;
    var y = gw2Coordinates.Y / scale;

    return (x, y);
}

(Waypoints Waypoints, TileWhitelist TileWhitelist) InitializeWaypointsAndTiles()
{

    var client = new System.Net.WebClient();
    client.Headers.Add("user-agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
    var rawString = client.DownloadString("https://wiki.guildwars2.com/index.php?title=Widget:Interactive_map_data_builder/infobox-map-output.js&action=raw");

    var parts = rawString.Split(";", StringSplitOptions.RemoveEmptyEntries);

    var waypointJson = parts[0].Replace("var wiki_waypoints = ", string.Empty);
    var waypoints = System.Text.Json.JsonSerializer.Deserialize<Waypoints>(waypointJson);

    var tileJson = parts[1].Replace("var wiki_tile_whitelist = ", string.Empty);
    var tiles = System.Text.Json.JsonSerializer.Deserialize<TileWhitelist>(tileJson, new JsonSerializerOptions() { Converters = { new TileWhitelist.ContinentConverter() } });

    return (waypoints, tiles);
}


public class Waypoints
{
    [System.Text.Json.Serialization.JsonPropertyName("1")]
    public Continent[] Tyria { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("2")]
    public Continent[] Mists { get; set; }

    public class Continent
    {
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string Name { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("coord")]
        public double[] Coordinates { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public int Id { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("chat_link")]
        public string ChatLink { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("floors")]
        public int[] Floors { get; set; }
    }
}



public class TileWhitelist
{
    [System.Text.Json.Serialization.JsonPropertyName("1")]
    public Continent Tyria { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("2")]
    public Continent Mists { get; set; }

    public class Continent
    {
        public List<Floor> Floors { get; set; } = new List<Floor>();
    }

    public class Floor
    {
        public int Id { get; set; }

        public List<string> Coordinates { get; set; } = new List<string>();
    }

    public class ContinentConverter : System.Text.Json.Serialization.JsonConverter<Continent>
    {
        public override Continent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            var continent = new Continent();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return continent;
                }

                var floor = new Floor();
                continent.Floors.Add(floor);

                // Get the key.
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException();
                }

                floor.Id = int.Parse(reader.GetString());
                reader.Read();

                if (reader.TokenType != JsonTokenType.StartArray)
                {
                    throw new JsonException();
                }

                while (true)
                {
                    reader.Read();

                    if (reader.TokenType == JsonTokenType.EndArray)
                    {
                        break;
                    }

                    floor.Coordinates.Add(reader.GetString());
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, Continent value, JsonSerializerOptions options) => throw new NotImplementedException();
    }
}
