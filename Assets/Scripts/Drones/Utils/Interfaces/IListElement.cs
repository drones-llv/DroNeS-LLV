using Drones.UI.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Drones.Utils.Interfaces
{
    public interface IListElement
    {
        AbstractWindow Window { get; }
        Color Odd { get; }
        Color Even { get; }
        Image ItemImage { get; }
        void OnListChange();
    }
}
