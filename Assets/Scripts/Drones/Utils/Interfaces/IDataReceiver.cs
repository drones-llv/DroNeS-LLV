using System.Collections;

namespace Drones.Utils.Interfaces
{
    public interface IDataReceiver
    {
        int UID { get; }

        IEnumerator WaitForAssignment();
    }
}
