using System;
using Utils.Interfaces;

namespace Utils
{
    [Serializable]
    public class CameraController
    {
        public ICameraMovement Move
        {
            get
            {
                return _move;
            }
            set
            {
                if (_move == null)
                {
                    _move = value;
                }
            }
        }

        private ICameraMovement _move;
        private float _upperPitch = -35;
        private float _lowerPitch = 90;
        private float _moveSpeed = 1;
        private float _zoomSpeed = 3;
        private float _rotationSpeed = 3;
        private float _mouseSensitivity = 30;
        private float _floor;
        private float _ceiling = 800;
        private float _speedToHeightGradient = 2;

        public CameraController(ICameraMovement movementController)
        {
            Move = movementController;
        }

        #region Properties
        public float MoveSpeed
        {
            get { return _moveSpeed; }

            set
            {
                _moveSpeed = Abs(value);
            }
        }

        public float ZoomSpeed
        {
            get { return _zoomSpeed; }

            set
            {
                _zoomSpeed = Abs(value);
            }
        }

        public float RotationSpeed
        {
            get { return _rotationSpeed; }

            set
            {
                _rotationSpeed = Abs(value);
            }
        }

        public float MouseSensitivity
        {
            get { return _mouseSensitivity; }

            set
            {
                _mouseSensitivity = Abs(value);
            }
        }

        public float Floor
        {
            get { return _floor; }

            set
            {
                _floor = Abs(value);
                if (_floor > _ceiling) { _floor = _ceiling; }
            }
        }

        public float Ceiling
        {
            get { return _ceiling; }

            set
            {
                _ceiling = Abs(value);
                if (_floor > _ceiling) { _ceiling = _floor; }
            }
        }

        public float SpeedToHeightGradient
        {
            get { return _speedToHeightGradient; }

            set
            {
                _speedToHeightGradient = Abs(value);
            }
        }

        public bool InvertYAxis { get; set; } = false;

        public float UpperPitch
        {
            get { return _upperPitch; }

            set
            {
                _upperPitch = KeepAcute(value);
                if (_upperPitch < 0 && _lowerPitch < -_upperPitch) { _upperPitch = -_lowerPitch; }
            }
        }

        public float LowerPitch
        {
            get { return _lowerPitch; }

            set
            {
                _lowerPitch = KeepAcute(value);
                if (_lowerPitch < 0 && -_lowerPitch < _upperPitch) {  _lowerPitch = -_upperPitch; }
            }
        }




        #endregion

        #region Methods
        private float KeepAcute(float angle)
        {
            if (angle < -90) { return -90; }
            if (angle > 90) { return 90; }
            return angle;
        }

        private float Abs(float number)
        {
            return (number < 0) ? 0 : number;
        }

        public void MoveLongitudinal(float input)
        {
            _move.MoveLongitudinal(input * MoveSpeed);
        }

        public void MoveLateral(float input)
        {
            _move.MoveLateral(input * MoveSpeed);
        }

        public void Zoom(float input)
        {
            _move.Zoom(input * ZoomSpeed);
        }

        public void Pitch(float input)
        {
            _move.Pitch((InvertYAxis ? input : -input) * MouseSensitivity);
        }

        public void Roll(float input)
        {
            _move.Roll(input * MouseSensitivity);
        }

        public void Yaw(float input)
        {
            _move.Yaw(input * MouseSensitivity);
        }

        public void Rotate(float input)
        {
            _move.Rotate(input * RotationSpeed);
        }

        public void ClampVertical()
        {
            _move.ClampVertical(Floor, Ceiling);
        }

        public void ClampPitch()
        {
            _move.ClampPitch(LowerPitch, UpperPitch);
        }


        #endregion
    }
}
