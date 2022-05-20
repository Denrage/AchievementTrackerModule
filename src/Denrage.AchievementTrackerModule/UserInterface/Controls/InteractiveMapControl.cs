using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Flurl.Http;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Denrage.AchievementTrackerModule.UserInterface.Controls
{
    // This control is not complete, it is missing panning and zooming to mimic the behaviour in the wiki fully. Most of the code is transferred from the js code of the wiki
    public class InteractiveMapControl : Control
    {
        private const string USER_AGENT = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";
        private const int TILE_SIZE = 256;

        private readonly string iconUrl;
        private readonly (double X, double Y) mapCoords;
        private readonly (int X, int Y) intMapCoords;
        private readonly List<List<double>> path;
        private readonly List<List<double>> bounds;
        private readonly int maxzoom;
        private readonly double floorId;
        private readonly double continentId;
        private readonly Waypoints waypoints;
        private readonly TileWhitelist tileWhiteList;

        // Not needed yet
        private readonly ((double X, double Y) StartCoordinate, (double X, double Y) EndCoordinate) continentDimensions;
        //private readonly int minzoom;
        private readonly string localTiles;
        private readonly (float X, float Y) mapBounds;

        private (int X, int Y) startCoordinate;
        private AsyncTexture2D icon;
        private AsyncTexture2D[,] tiles;

        private static AsyncTexture2D dot;
        private static AsyncTexture2D swirl;
        private static AsyncTexture2D flagStart;
        private static AsyncTexture2D flagEnd;

        public InteractiveMapControl(string iconUrl, string localTiles, string inputCoords, string path, string bounds)
        {
            this.ClipsBounds = true;
            this.iconUrl = iconUrl;
            this.localTiles = localTiles;
            var coords = this.ConvertStringToNestedArray(inputCoords);
            this.path = this.ConvertStringToNestedArray(path);
            this.bounds = this.ConvertStringToNestedArray(bounds);

            var mastercoords = this.bounds;

            if (coords.Count > 0)
            {
                mastercoords = coords;
            }
            else if (this.path.Count > 0)
            {
                mastercoords = this.path;
            }

            this.floorId = 1.0;
            if (mastercoords[0].Count > 2)
            {
                this.floorId = mastercoords[0][2];
            }

            this.continentId = 1.0;
            if (mastercoords[0].Count > 3)
            {
                this.continentId = mastercoords[0][3];
            }

            var centroid = GetCentroid(mastercoords.Select(x => (x[0], x[1])).ToList());
            var size = GetSize(mastercoords.Select(x => (x[0], x[1])).ToList());

            //this.minzoom = 2;
            this.maxzoom = 8;

            this.continentDimensions = ((0, 0), (131072, 131072));

            if (this.continentId == 2)
            {
                //this.minzoom = 0;
                this.maxzoom = 6;
                this.continentDimensions = ((0, 0), (16384, 16384));
            }

            this.mapBounds = this.ConvertCoordinates(this.continentDimensions.EndCoordinate, this.maxzoom);

            var (waypoints, tileWhiteList) = this.InitializeWaypointsAndTiles();
            this.waypoints = waypoints;
            this.tileWhiteList = tileWhiteList;

            this.mapCoords = this.ConvertCoordinates(centroid, this.maxzoom);
            this.intMapCoords = ((int)Math.Floor(this.mapCoords.X), (int)Math.Floor(this.mapCoords.Y));

            this.InitializeTextures();
        }

        static InteractiveMapControl()
        {
            InitializeStaticTextures();
        }

        protected override void DisposeControl()
        {
            this.icon?.Dispose();
            for (var i = 0; i < this.tiles.GetLength(0); i++)
            {
                for (var j = 0; j < this.tiles.GetLength(1); j++)
                {
                    this.tiles[i, j]?.Dispose();
                }
            }

            base.DisposeControl();
        }

        private static void InitializeStaticTextures()
        {
            dot = InitializeTexture("https://wiki.guildwars2.com/images/2/23/Widget_map_dot.png");
            swirl = InitializeTexture("https://wiki.guildwars2.com/images/8/8d/Widget_map_yellow_swirl.png");
            flagStart = InitializeTexture("https://wiki.guildwars2.com/images/f/f0/Event_flag_green.png");
            flagEnd = InitializeTexture("https://wiki.guildwars2.com/images/8/8d/Event_flag_red.png");
        }

        private static AsyncTexture2D InitializeTexture(string url)
        {
            var texture = new AsyncTexture2D(ContentService.Textures.TransparentPixel);
            _ = Task.Run(async () =>
              {
                  try
                  {
                      var imageStream = await url.WithHeader("user-agent", USER_AGENT).GetStreamAsync();

                      GameService.Graphics.QueueMainThreadRender(device =>
                      {
                          texture.SwapTexture(TextureUtil.FromStreamPremultiplied(device, imageStream));
                          imageStream.Close();
                      });
                  }
                  catch (Exception ex)
                  {
                      if (ex.InnerException is FlurlHttpException httpException)
                      {
                          if (!httpException.Message.Contains("404 (Not Found)")) // Ignore 404 Errors
                          {
                              throw;
                          }
                      }
                      else
                      {
                          throw;
                      }
                  }
              });

            return texture;
        }

        private void InitializeTextures()
        {
            if (!string.IsNullOrEmpty(this.iconUrl))
            {
                this.icon = InitializeTexture(this.iconUrl);
            }

            this.InitializeTiles();
        }

        private void InitializeTiles()
        {
            var tileUrls = this.GetTileUrls();
            this.tiles = new AsyncTexture2D[tileUrls.GetLength(0), tileUrls.GetLength(1)];

            for (var i = 0; i < tileUrls.GetLength(0); i++)
            {
                for (var j = 0; j < tileUrls.GetLength(1); j++)
                {
                    this.tiles[i, j] = InitializeTexture(tileUrls[i, j]);
                }
            }

            var firstValidTileIndexX = -1;
            var lastValidTileIndexX = -1;
            var firstValidTileIndexY = -1;
            var lastValidTileIndexY = -1;

            for (var i = 0; i < this.tiles.GetLength(0); i++)
            {
                for (var j = 0; j < this.tiles.GetLength(1); j++)
                {
                    if (tiles[i, j] != null && firstValidTileIndexX == -1 && firstValidTileIndexY == -1)
                    {
                        firstValidTileIndexX = i;
                        firstValidTileIndexY = j;
                    }

                    if (firstValidTileIndexX != -1
                        && firstValidTileIndexY != -1
                        && i > firstValidTileIndexX
                        && j > firstValidTileIndexY
                        && i < this.tiles.GetLength(0) - 1
                        && j < this.tiles.GetLength(1) - 1
                        && (this.tiles[i + 1, j] == null || this.tiles[i, j + 1] == null))
                    {
                        lastValidTileIndexX = i;
                        lastValidTileIndexY = j;
                    }
                }
            }

            if (lastValidTileIndexX == -1)
            {
                lastValidTileIndexX = this.tiles.GetLength(0);
            }

            if (lastValidTileIndexY == -1)
            {
                lastValidTileIndexY = this.tiles.GetLength(1);
            }

            var newTiles = new AsyncTexture2D[lastValidTileIndexX - firstValidTileIndexX, lastValidTileIndexY - firstValidTileIndexY];

            for (int i = firstValidTileIndexX, newI = 0; i < lastValidTileIndexX; i++, newI++)
            {
                for (int j = firstValidTileIndexY, newJ = 0; j < lastValidTileIndexY; j++, newJ++)
                {
                    newTiles[newI, newJ] = this.tiles[i, j];
                }
            }

            this.startCoordinate.X += firstValidTileIndexX;
            this.startCoordinate.Y += firstValidTileIndexY;

            this.tiles = newTiles;
        }

        private string[,] GetTileUrls()
        {
            var tileUrls = new string[3, 3];
            if (this.bounds.Count > 0)
            {
                var boundCoords = this.bounds.Select(x => this.ConvertCoordinates((x[0], x[1]), this.maxzoom));
                var intBoundCoords = boundCoords.Select(x => ((int)Math.Floor(x.X), (int)Math.Floor(x.Y)));

                var lowerBoundX = intBoundCoords.Min(x => x.Item1) - 1;
                var upperBoundX = intBoundCoords.Max(x => x.Item1) + 1;
                var lowerBoundY = intBoundCoords.Min(x => x.Item2) - 1;
                var upperBoundY = intBoundCoords.Max(x => x.Item2) + 1;

                this.startCoordinate.X = lowerBoundX;
                this.startCoordinate.Y = lowerBoundY;

                tileUrls = new string[upperBoundX - lowerBoundX, upperBoundY - lowerBoundY];

                for (var i = 0; i < upperBoundX - lowerBoundX; i++)
                {
                    for (var j = 0; j < upperBoundY - lowerBoundY; j++)
                    {
                        tileUrls[i, j] = this.GetTileUrl((lowerBoundX + i, lowerBoundY + j, this.maxzoom), this.floorId, this.continentId, this.tileWhiteList);
                    }
                }
            }
            else if (this.path.Count > 0)
            {
                var pathCoords = this.path.Select(x => this.ConvertCoordinates((x[0], x[1]), this.maxzoom));
                var intPathCoords = pathCoords.Select(x => ((int)Math.Floor(x.X), (int)Math.Floor(x.Y)));

                var lowerBoundX = intPathCoords.Min(x => x.Item1) - 1;
                var upperBoundX = intPathCoords.Max(x => x.Item1) + 1;
                var lowerBoundY = intPathCoords.Min(x => x.Item2) - 1;
                var upperBoundY = intPathCoords.Max(x => x.Item2) + 1;

                this.startCoordinate.X = lowerBoundX;
                this.startCoordinate.Y = lowerBoundY;

                tileUrls = new string[upperBoundX - lowerBoundX, upperBoundY - lowerBoundY];

                for (var i = 0; i < upperBoundX - lowerBoundX; i++)
                {
                    for (var j = 0; j < upperBoundY - lowerBoundY; j++)
                    {
                        tileUrls[i, j] = this.GetTileUrl((lowerBoundX + i, lowerBoundY + j, this.maxzoom), this.floorId, this.continentId, this.tileWhiteList);
                    }
                }
            }
            else
            {
                var lowerBoundX = this.intMapCoords.X - 2;
                var upperBoundX = this.intMapCoords.X + 2;
                var lowerBoundY = this.intMapCoords.Y - 2;
                var upperBoundY = this.intMapCoords.Y + 2;

                this.startCoordinate.X = lowerBoundX;
                this.startCoordinate.Y = lowerBoundY;
                tileUrls = new string[upperBoundX - lowerBoundX, upperBoundY - lowerBoundY];
                for (var i = 0; i < upperBoundX - lowerBoundX; i++)
                {
                    for (var j = 0; j < upperBoundY - lowerBoundY; j++)
                    {
                        tileUrls[i, j] = this.GetTileUrl((lowerBoundX + i, lowerBoundY + j, this.maxzoom), this.floorId, this.continentId, this.tileWhiteList);
                    }
                }
            }

            return tileUrls;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds)
        {
            var tileWidth = this.Width / this.tiles.GetLength(0);
            var tileHeight = this.Height / this.tiles.GetLength(1);
            var scaleX = tileWidth / (float)TILE_SIZE;
            var scaleY = tileHeight / (float)TILE_SIZE;

            for (var i = 0; i < this.tiles.GetLength(0); i++)
            {
                for (var j = 0; j < this.tiles.GetLength(1); j++)
                {
                    if (this.tiles[i, j] is null)
                    {
                        continue;
                    }

                    spriteBatch.DrawOnCtrl(this, this.tiles[i, j], new Rectangle(i * tileWidth, j * tileHeight, tileWidth, tileHeight), Color.White);
                }
            }

            var iconX = (float)(((this.intMapCoords.X - this.startCoordinate.X) * TILE_SIZE) + (TILE_SIZE * (this.mapCoords.X % 1)));
            var iconY = (float)(((this.intMapCoords.Y - this.startCoordinate.Y) * TILE_SIZE) + (TILE_SIZE * (this.mapCoords.Y % 1)));

            iconX = this.Scale(iconX, scaleX);
            iconY = this.Scale(iconY, scaleY);

            if (this.path.Count > 0)
            {
                var startImageCoordinates = this.ConvertCoordinates((this.path[0][0], this.path[0][1]), this.maxzoom);
                var startImageX = (int)((((int)Math.Floor(startImageCoordinates.X) - this.startCoordinate.X) * TILE_SIZE) + (TILE_SIZE * (startImageCoordinates.X % 1)));
                var startImageY = (int)((((int)Math.Floor(startImageCoordinates.Y) - this.startCoordinate.Y) * TILE_SIZE) + (TILE_SIZE * (startImageCoordinates.Y % 1)));

                var endImageCoordinates = this.ConvertCoordinates((this.path[this.path.Count - 1][0], this.path[this.path.Count - 1][1]), this.maxzoom);
                var endImageX = (int)((((int)Math.Floor(endImageCoordinates.X) - this.startCoordinate.X) * TILE_SIZE) + (TILE_SIZE * (endImageCoordinates.X % 1)));
                var endImageY = (int)((((int)Math.Floor(endImageCoordinates.Y) - this.startCoordinate.Y) * TILE_SIZE) + (TILE_SIZE * (endImageCoordinates.Y % 1)));

                var points = this.path.Select(x =>
                {
                    var translatedCoord = this.ConvertCoordinates((x[0], x[1]), this.maxzoom);
                    var intTranslatedCoordX = (int)Math.Floor(translatedCoord.X);
                    var intTranslatedCoordY = (int)Math.Floor(translatedCoord.Y);
                    translatedCoord.X = (int)(((intTranslatedCoordX - this.startCoordinate.X) * TILE_SIZE) + (TILE_SIZE * (translatedCoord.X % 1)));
                    translatedCoord.Y = (int)(((intTranslatedCoordY - this.startCoordinate.Y) * TILE_SIZE) + (TILE_SIZE * (translatedCoord.Y % 1)));
                    return new Vector2(translatedCoord.X, translatedCoord.Y);
                }).ToArray();

                for (var i = 0; i < points.Length - 1; i++)
                {
                    var startPosition = new Vector2(this.Scale(points[i].X, scaleX), this.Scale(points[i].Y, scaleY));
                    var nextPosition = new Vector2(this.Scale(points[i + 1].X, scaleX), this.Scale(points[i + 1].Y, scaleY));
                    spriteBatch.DrawLine(this.ToBounds(startPosition), this.ToBounds(nextPosition), Color.Yellow, 10);
                }

                var flagStartX = this.Scale(startImageX, scaleX) - (this.Scale(flagStart.Texture.Width, scaleX) / 2);
                var flagStartY = this.Scale(startImageY, scaleY) - (this.Scale(flagStart.Texture.Height, scaleY) / 2);

                var flagEndX = this.Scale(endImageX, scaleX) - (this.Scale(flagEnd.Texture.Width, scaleX) / 2);
                var flagEndY = this.Scale(endImageY, scaleY) - (this.Scale(flagEnd.Texture.Height, scaleY) / 2);

                spriteBatch.Draw(flagStart, this.ToBounds(new Vector2(flagStartX, flagStartY)), null, Color.White, 0f, default, new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                spriteBatch.Draw(flagEnd, this.ToBounds(new Vector2(flagEndX, flagEndY)), null, Color.White, 0f, default, new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
            }

            if (!string.IsNullOrEmpty(this.iconUrl))
            {
                var pointX = iconX - (this.Scale(this.icon.Texture.Width, scaleX) / 2);
                var pointY = iconY - (this.Scale(this.icon.Texture.Height, scaleY) / 2);

                spriteBatch.Draw(this.icon, this.ToBounds(new Vector2(pointX, pointY)), null, Color.White, 0f, default, new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
            }
            else
            {
                var dotX = iconX - (this.Scale(dot.Texture.Width, scaleX) / 2);
                var dotY = iconY - (this.Scale(dot.Texture.Height, scaleY) / 2);
                spriteBatch.Draw(dot, this.ToBounds(new Vector2(dotX, dotY)), null, Color.White, 0f, default, new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
            }

            if (this.bounds.Count > 0)
            {
                var points = this.bounds.Select(x =>
                {
                    var translatedCoord = this.ConvertCoordinates((x[0], x[1]), this.maxzoom);
                    var intTranslatedCoordX = (int)Math.Floor(translatedCoord.X);
                    var intTranslatedCoordY = (int)Math.Floor(translatedCoord.Y);
                    translatedCoord.X = ((intTranslatedCoordX - this.startCoordinate.X) * TILE_SIZE) + (TILE_SIZE * (translatedCoord.X % 1));
                    translatedCoord.Y = ((intTranslatedCoordY - this.startCoordinate.Y) * TILE_SIZE) + (TILE_SIZE * (translatedCoord.Y % 1));

                    return new Vector2(translatedCoord.X, translatedCoord.Y);
                }).ToArray();

                for (var i = 0; i < points.Length; i++)
                {
                    var nextIndex = i + 1;

                    if (i == points.Length - 1)
                    {
                        nextIndex = 0;
                    }

                    var startPosition = new Vector2(this.Scale(points[i].X, scaleX), this.Scale(points[i].Y, scaleY));
                    var nextPosition = new Vector2(this.Scale(points[nextIndex].X, scaleX), this.Scale(points[nextIndex].Y, scaleY));
                    spriteBatch.DrawLine(this.ToBounds(startPosition), this.ToBounds(nextPosition), Color.Yellow, 4);
                }

                var polygon = new MonoGame.Extended.Shapes.Polygon(points);

                spriteBatch.DrawPolygon(points.OrderBy(x => x.Y).First(), polygon, Color.Yellow, 4);

            }
            else
            {
                var swirlX = iconX - (this.Scale(swirl.Texture.Width, scaleX) / 2);
                var swirlY = iconY - (this.Scale(swirl.Texture.Height, scaleY) / 2);
                spriteBatch.Draw(swirl, this.ToBounds(new Vector2(swirlX, swirlY)), null, Color.White, 0f, default, new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);

            }
        }

        private Vector2 ToBounds(Vector2 vector)
            => new Vector2(vector.X + this.AbsoluteBounds.X, vector.Y + this.AbsoluteBounds.Y);

        private float Scale(float input, float scale)
            => input * scale;

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
                var splittedParts = input.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                var innerList = new List<double>();
                foreach (var item in splittedParts)
                {
                    innerList.Add(double.Parse(item, System.Globalization.CultureInfo.InvariantCulture));
                }

                result.Add(innerList);
            }
            else
            {
                var splittedFirstDimension = input.Replace("[", string.Empty).Split(new[] { "]" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in splittedFirstDimension)
                {
                    var innerList = new List<double>();
                    var splittedSecondDimension = item.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
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
            var parts = this.GenerateUrlComponents(fileName);
            return $"https://wiki.guildwars2.com/images/{parts.FirstPart}/{parts.SecondPart}/{fileName}";
        }

        (string FirstPart, string SecondPart) GenerateUrlComponents(string fileName)
        {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.ASCII.GetBytes(fileName));
            var hex = BitConverter.ToString(hash, 0, 1);
            return (hex[0].ToString().ToLower(), hex.ToLower());
        }

        static (double X, double Y) GetCentroid(List<(double X, double Y)> poly)
            => poly.Aggregate((x, y) => (x.X + (y.X / poly.Count), x.Y + (y.Y / poly.Count)));

        static (double X, double Y) GetSize(List<(double X, double Y)> polygon)
        {
            var xValues = polygon.Select(x => x.X).ToList();
            var yValues = polygon.Select(y => y.Y).ToList();
            var minimumArray = new double[] { xValues.Min(), yValues.Min() };
            var maximumArray = new double[] { xValues.Max(), yValues.Max() };

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
                return this.GenerateUrl(file);
            }
            else
            {
                // Otherwise use native API with a bodge applied

                // Bodge: X 32_768 and Y 16_384 (i.e. 128*TILE_SIZE plus 64*TILE_SIZE)
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
                return xBodge < 0 || yBodge < 0
                    ? "https://wiki.guildwars2.com/images/c/cb/World_map_tile_under_construction.png"
                    : $"https://tiles.guildwars2.com/{continentId}/{floorId}/{zBodge}/{xBodge}/{yBodge}.jpg";
            }
        }

        (float X, float Y) ConvertCoordinates((double X, double Y) gw2Coordinates, int zoom)
        {
            var scale = Math.Pow(2, zoom);

            var x = gw2Coordinates.X / scale;
            var y = gw2Coordinates.Y / scale;

            return ((float)x, (float)y);
        }

        (Waypoints Waypoints, TileWhitelist TileWhitelist) InitializeWaypointsAndTiles()
        {

            var client = new System.Net.WebClient();
            client.Headers.Add("user-agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36");
            var rawString = client.DownloadString("https://wiki.guildwars2.com/index.php?title=Widget:Interactive_map_data_builder/infobox-map-output.js&action=raw");

            var parts = rawString.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            var waypointJson = parts[0].Replace("var wiki_waypoints = ", string.Empty);
            var waypoints = System.Text.Json.JsonSerializer.Deserialize<Waypoints>(waypointJson);

            var tileJson = parts[1].Replace("var wiki_tile_whitelist = ", string.Empty);
            var tiles = System.Text.Json.JsonSerializer.Deserialize<TileWhitelist>(tileJson, new System.Text.Json.JsonSerializerOptions() { Converters = { new TileWhitelist.ContinentConverter() } });

            return (waypoints, tiles);
        }
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
            public override Continent Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
            {
                if (reader.TokenType != System.Text.Json.JsonTokenType.StartObject)
                {
                    throw new System.Text.Json.JsonException();
                }

                var continent = new Continent();

                while (reader.Read())
                {
                    if (reader.TokenType == System.Text.Json.JsonTokenType.EndObject)
                    {
                        return continent;
                    }

                    var floor = new Floor();
                    continent.Floors.Add(floor);

                    // Get the key.
                    if (reader.TokenType != System.Text.Json.JsonTokenType.PropertyName)
                    {
                        throw new System.Text.Json.JsonException();
                    }

                    floor.Id = int.Parse(reader.GetString());
                    _ = reader.Read();

                    if (reader.TokenType != System.Text.Json.JsonTokenType.StartArray)
                    {
                        throw new System.Text.Json.JsonException();
                    }

                    while (true)
                    {
                        _ = reader.Read();

                        if (reader.TokenType == System.Text.Json.JsonTokenType.EndArray)
                        {
                            break;
                        }

                        floor.Coordinates.Add(reader.GetString());
                    }
                }

                throw new System.Text.Json.JsonException();
            }

            public override void Write(System.Text.Json.Utf8JsonWriter writer, Continent value, System.Text.Json.JsonSerializerOptions options) => throw new NotImplementedException();
        }
    }
}
