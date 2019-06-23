using System.Collections.Generic;
using Mapbox.Map;
using Mapbox.Unity.Map.Interfaces;
using Mapbox.Unity.Map.TileProviders;
using Mapbox.Utils;

namespace Drones.Mapbox.Tile_Providers
{
    public class MaseruTiles : AbstractTileProvider {
        private bool _initialized = false;
        private static HashSet<UnwrappedTileId> _tileSet;

        private static readonly float[,] _Coordinates = {{-29.2652149f, 27.4233034f},
            {-29.2652149f, 27.6097277f},
            {-29.4012808f, 27.4233034f},                                           
            {-29.4012808f, 27.6097277f}};

        private static readonly List<UnwrappedTileId> _Tiles = new List<UnwrappedTileId>();

        private static void SetUpTiles(IMap map)
        {
            _tileSet = new HashSet<UnwrappedTileId>();
            var end = _Coordinates.GetUpperBound(0);
      
            for (var k = 0; k <= end; k++)
            {
                _Tiles.Add(TileCover.CoordinateToTileId(new Vector2d(_Coordinates[k, 0], _Coordinates[k, 1]), map.AbsoluteZoom));
            }
            
            for (var y = _Tiles[0].Y; y <= _Tiles[3].Y; y++)
            {
                for (var x = _Tiles[0].X; x <= _Tiles[1].X; x++)
                {
                    _tileSet.Add(new UnwrappedTileId(map.AbsoluteZoom, x, y));
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

            if (_tileSet == null)
            {
                SetUpTiles(_map);
            }

            _currentExtent.activeTiles = _tileSet;

            OnExtentChanged();
        }

        public override bool Cleanup(UnwrappedTileId tile) {
            return (!_currentExtent.activeTiles.Contains(tile));
        }

    }
}