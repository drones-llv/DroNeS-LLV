using System.Collections.Generic;
using Drones.UI.Utils;
using Utils;

namespace Drones.Utils.Interfaces
{
    public interface IMultiDataSourceReceiver : IDataReceiver
    {
        SecureSortedSet<uint, IDataSource> Sources { get; set; }
        Dictionary<IDataSource, ObjectTuple> DataReceivers { get; }
        void OnNewSource(IDataSource source);
        void OnLooseSource(IDataSource source);
        bool IsClearing { get; }
        void ClearDataReceivers();
    }
}