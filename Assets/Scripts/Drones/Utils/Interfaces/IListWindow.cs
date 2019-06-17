
using Drones.UI.Utils;
using Utils;

namespace Drones.Utils.Interfaces
{
    public delegate void ListChangeHandler();

    public interface IListWindow
    {
        ListTupleContainer TupleContainer { get; }

        ListElement TupleType { get; }

        event ListChangeHandler ListChanged;
    }

}