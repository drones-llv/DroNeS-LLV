using Mapbox.Unity.Map;
using Mapbox.Unity.Map.TileProviders;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.MeshGeneration.Modifiers;
using UnityEngine;
using Utils;

namespace Drones.Utils
{
    public static class MapController
    {
        public static void BuildCity(AbstractMap map)
        {
            map.Initialize(new global::Mapbox.Utils.Vector2d(Constants.OriginCoordinates[0], Constants.OriginCoordinates[1]), Constants.mapboxZoom);
        }

        public static AbstractMap CreateAbstractMaps(string cityName)
        {
            GameObject city = new GameObject
            {
                name = cityName
            };
            return city.AddComponent<AbstractMap>();
        }

        public static void SetUpAbstractMap(AbstractMap map, AbstractTileProvider tiles)
        {
            map.Options.extentOptions.extentType = MapExtentType.Custom;
            map.TileProvider = tiles;
            map.InitializeOnStart = false;
            map.Options.scalingOptions.scalingType = MapScalingType.WorldScale;
            map.Options.placementOptions.placementType = MapPlacementType.AtTileCenter;
            map.Options.placementOptions.snapMapToZero = true;
            map.Options.tileMaterial = Resources.Load("Materials/TileMaterial") as Material;
            map.ImageLayer.SetLayerSource(ImagerySourceType.Custom);
            map.ImageLayer.UseCompression(true);
            map.ImageLayer.UseMipMap(true);
            map.ImageLayer.SetLayerSource(Constants.mapStyle);
            map.VectorData.SetLayerSource(VectorSourceType.MapboxStreetsWithBuildingIds);
        }

        public static void SetElevation(Elevation type, AbstractMap map)
        {
            ElevationLayerProperties elevation = new ElevationLayerProperties();
            map.Terrain.EnableCollider(true);
            if (type == Elevation.Real)
            {
                elevation.modificationOptions.earthRadius = Constants.R * 1000;
                elevation.modificationOptions.sampleCount = 10;
                map.Terrain.Update(elevation);
                map.Terrain.SetElevationType(ElevationLayerType.TerrainWithElevation);
                return;
            }
            elevation.modificationOptions.sampleCount = 2;
            map.Terrain.Update(elevation);
            map.Terrain.SetElevationType(ElevationLayerType.FlatTerrain);

        }
        // height in meters
        public static void AssignBuildings(Building type, float min, float max, AbstractMap map, bool combineMesh = false)
        {
            if (type == Building.Road) { return; }

            VectorSubLayerProperties vectorSubLayerProperties = new VectorSubLayerProperties();
            vectorSubLayerProperties.colliderOptions.colliderType = ColliderType.MeshCollider;
            vectorSubLayerProperties.coreOptions.combineMeshes = false;
            vectorSubLayerProperties.coreOptions.geometryType = VectorPrimitiveType.Polygon;
            vectorSubLayerProperties.coreOptions.layerName = "building";
            vectorSubLayerProperties.coreOptions.snapToTerrain = true;
            vectorSubLayerProperties.coreOptions.combineMeshes = combineMesh;
            vectorSubLayerProperties.extrusionOptions.extrusionType = ExtrusionType.PropertyHeight;
            vectorSubLayerProperties.extrusionOptions.extrusionScaleFactor = 1.3203f;
            vectorSubLayerProperties.extrusionOptions.propertyName = "height";
            vectorSubLayerProperties.extrusionOptions.extrusionGeometryType = ExtrusionGeometryType.RoofAndSide;
            vectorSubLayerProperties.moveFeaturePositionTo = PositionTargetType.CenterOfVertices;
            vectorSubLayerProperties.coreOptions.sublayerName = type.ToString();

            string atlasPath = "Atlases/" + type.ToString() + "BuildingAtlas";
            AtlasInfo atlasInfo = Resources.Load(atlasPath) as AtlasInfo;
            Material material = Resources.Load(Constants.buildingMaterialPath) as Material;
            GeometryMaterialOptions materialOptions = new GeometryMaterialOptions();
            materialOptions.SetDefaultMaterialOptions();
            materialOptions.customStyleOptions.texturingType = UvMapType.Atlas;
            materialOptions.customStyleOptions.materials[0].Materials[0] = material;
            materialOptions.customStyleOptions.materials[1].Materials[0] = material;
            materialOptions.customStyleOptions.atlasInfo = atlasInfo;
            materialOptions.SetStyleType(StyleTypes.Custom);
            vectorSubLayerProperties.materialOptions = materialOptions;
            vectorSubLayerProperties.buildingsWithUniqueIds = true;
            if (min < 100)
            {
                vectorSubLayerProperties.filterOptions.AddNumericFilterInRange("height", min, max);
                vectorSubLayerProperties.filterOptions.AddNumericFilterEquals("height", max);
            }
            else
            {
                vectorSubLayerProperties.filterOptions.AddNumericFilterGreaterThan("height", min);
            }

            map.VectorData.AddFeatureSubLayer(vectorSubLayerProperties);
        }

        public static void AddRoad(AbstractMap map)
        {
            VectorSubLayerProperties vectorSubLayerProperties = new VectorSubLayerProperties();
            vectorSubLayerProperties.colliderOptions.colliderType = ColliderType.MeshCollider;
            vectorSubLayerProperties.coreOptions.combineMeshes = false;
            vectorSubLayerProperties.coreOptions.geometryType = VectorPrimitiveType.Line;
            vectorSubLayerProperties.coreOptions.layerName = "Road";
            vectorSubLayerProperties.coreOptions.snapToTerrain = true;
            vectorSubLayerProperties.coreOptions.combineMeshes = true;
            vectorSubLayerProperties.extrusionOptions.extrusionGeometryType = ExtrusionGeometryType.RoofAndSide;
            vectorSubLayerProperties.extrusionOptions.extrusionType = ExtrusionType.AbsoluteHeight;
            vectorSubLayerProperties.extrusionOptions.extrusionScaleFactor = 1;
            // This value doesn't really matter but should be big
            vectorSubLayerProperties.extrusionOptions.SetAbsoluteHeight(24);
            vectorSubLayerProperties.moveFeaturePositionTo = PositionTargetType.CenterOfVertices;
            vectorSubLayerProperties.coreOptions.sublayerName = Building.Road.ToString();
        }

    }
}



