namespace Utils.Interfaces
{
    public interface ICameraMovement
    {
        void MoveLongitudinal(float input);

        void MoveLateral(float input);

        void MoveVertical(float input);

        void Zoom(float input);

        void Pitch(float input);

        void Yaw(float input);

        void Roll(float input);

        void Rotate(float input);

        void ClampLongitudinal(float lowerBound, float upperBound);

        void ClampLateral(float lowerBound, float upperBound);

        void ClampVertical(float lowerBound, float upperBound);

        void ClampZoom(float lowerBound, float upperBound);

        void ClampPitch(float lowerBound, float upperBound);

        void ClampYaw(float lowerBound, float upperBound);

        void ClampRoll(float lowerBound, float upperBound);

        void ClampRotate(float lowerBound, float upperBound);
    }
}
