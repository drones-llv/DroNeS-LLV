using System.Collections.Generic;
using Mapbox.Map;
using Mapbox.Unity.Map.Interfaces;
using Mapbox.Unity.Map.TileProviders;
using Mapbox.Utils;

namespace Drones.Mapbox.Tile_Providers
{
    public class ManhattanTiles : AbstractTileProvider {
        private bool _initialized = false;
        public static HashSet<UnwrappedTileId> tiles;
        public static readonly float[,] coordinates = {{40.699f,-74.025f},
            {40.699f,-74.003f},
            {40.702f,-74.025f},
            {40.702f,-74.003f},
            {40.705f,-74.023f},
            {40.705f,-74.003f},
            {40.707f,-74.025f},
            {40.707f,-73.992f},
            {40.71f,-74.02f},
            {40.71f,-73.971f},
            {40.734f,-74.018f},
            {40.734f,-73.972f},
            {40.745f,-74.018f},
            {40.745f,-73.967f},
            {40.753f,-74.01f},
            {40.753f,-73.964f},
            {40.757f,-74.01f},
            {40.757f,-73.97f},
            {40.76f,-74.01f},
            {40.76f,-73.963f},
            {40.762f,-74.01f},
            {40.762f,-73.958f},
            {40.767f,-74.00f},
            {40.767f,-73.953f},
            {40.775f,-73.995f},
            {40.775f,-73.940f},
            {40.783f,-73.990f},
            {40.783f,-73.940f},
            {40.788f,-73.986f},
            {40.788f,-73.945f},
            {40.792f,-73.986f},
            {40.792f,-73.936f},
            {40.796f,-73.981f},
            {40.796f,-73.928f},
            {40.804f,-73.974f},
            {40.804f,-73.937f},
            {40.807f,-73.974f},
            {40.807f,-73.937f},
            {40.814f,-73.963f},
            {40.814f,-73.934f},
            {40.83f,-73.958f},
            {40.83f,-73.934f}};
        public static readonly List<UnwrappedTileId> leftTiles = new List<UnwrappedTileId>();
        public static readonly List<UnwrappedTileId> rightTiles = new List<UnwrappedTileId>();

        public static void SetUpTiles(IMap map)
        {
            tiles = new HashSet<UnwrappedTileId>();
            var end = coordinates.GetUpperBound(0);
      
            for (var k = 0; k <= end; k += 2)
            {
                leftTiles.Add(TileCover.CoordinateToTileId(new Vector2d(coordinates[k, 0], coordinates[k, 1]), map.AbsoluteZoom));
            }
            for (var k = 1; k <= end; k += 2)
            {
                rightTiles.Add(TileCover.CoordinateToTileId(new Vector2d(coordinates[k, 0], coordinates[k, 1]), map.AbsoluteZoom));
            }

            var j = 0;
            for (var i = 0; i < leftTiles.Count; i++)
            {
                if (i != leftTiles.Count - 1) j = i + 1;
                else j = i;
                for (var y = leftTiles[j].Y; y <= leftTiles[i].Y; y++)
                {
                    for (var x = leftTiles[i].X; x <= rightTiles[i].X; x++)
                    {
                        tiles.Add(new UnwrappedTileId(map.AbsoluteZoom, x, y));
                    }
                }
            }
        }

        public override void OnInitialized() {

            _initialized = true;
            _currentExtent.activeTiles = new HashSet<UnwrappedTileId>();
        }

        public override void UpdateTileExtent() {
            if (!_initialized) { return; }

            _currentExtent.activeTiles.Clear();

            if (tiles == null)
            {
                SetUpTiles(_map);
            }

            _currentExtent.activeTiles = tiles;

            OnExtentChanged();
        }

        public override bool Cleanup(UnwrappedTileId tile) {
            return (!_currentExtent.activeTiles.Contains(tile));
        }

    }
}
