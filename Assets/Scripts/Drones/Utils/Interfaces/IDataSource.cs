using Drones.UI.Utils;

namespace Drones.Utils.Interfaces
{
    public interface IDataSource
    {
        uint UID { get; }

        bool IsDataStatic { get; }

        AbstractInfoWindow InfoWindow { get; set; }

        void GetData(ISingleDataSourceReceiver receiver);

        void OpenInfoWindow();
    }
}