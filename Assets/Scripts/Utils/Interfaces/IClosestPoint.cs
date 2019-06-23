namespace Utils.Interfaces
{
    public interface IClosestPoint
    {
        float[] GetClosestPoint(float[] point, Directions dir);
        float[] GetCentre();
        void Message(object msg);
    }

}