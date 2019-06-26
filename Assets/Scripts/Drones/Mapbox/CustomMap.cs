using Mapbox.Unity.Map;

namespace Drones.Mapbox
{
    public class CustomMap : AbstractMap
    {

        public static float FilterHeight { get; set; } = 0;

        public static bool PotatoMode = false;

        protected override void Awake()
        {
            if (PotatoMode)
            {
                Options.extentOptions.extentType = MapExtentType.RangeAroundCenter;
            }
            base.Awake();
            if (FilterHeight > 1)
            {
                VectorData.GetFeatureSubLayerAtIndex(0)?.filterOptions.AddNumericFilterGreaterThan("height", FilterHeight);
            }
        }
    }
}
