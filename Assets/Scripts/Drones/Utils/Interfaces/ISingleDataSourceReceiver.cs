using System;
using System.Collections;
using Drones.UI.Utils;

namespace Drones.Utils.Interfaces
{
    public interface ISingleDataSourceReceiver : IDataReceiver
    {
        Type ReceiverType { get; }

        IDataSource Source { get; }

        DataField[] Data { get; }

        void SetData(IData data);

        IEnumerator StreamData();
    }
}
