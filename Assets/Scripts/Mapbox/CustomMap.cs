namespace Drones.Utils
{
    using Drones.Managers;
    using Mapbox.Unity.Map;

    public class CustomMap : AbstractMap
    {

        public static float FilterHeight { get; set; } = 0;

        protected override void Awake()
        {
            base.Awake();
            if (FilterHeight > 1)
            {
                VectorData.GetFeatureSubLayerAtIndex(0)?.filterOptions.AddNumericFilterGreaterThan("height", FilterHeight);
            }
        }
    }
}
