using System;
using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;
using Drones.Managers;
using Drones.Scheduler;
using Drones.UI.Utils;
using Utils;

namespace Drones.Utils
{
    public class TimeKeeper : MonoBehaviour
    {
        private static TimeKeeper Instance { get; set; }
        [SerializeField]
        private TimeSpeed timeSpeed = TimeSpeed.Pause;
        public static TimeSpeed TimeSpeed
        {
            get => Instance.timeSpeed;

            set
            {
                if (Instance.timeSpeed != value)
                {
                    if (SimManager.Status == SimulationStatus.EditMode && value != TimeSpeed.Pause) return;
                    Instance.timeSpeed = value;
                }
                ShowSimSpeed.OnSpeedChange();
            }
        }
        private const float DegPerDay = 360.0f / (24 * 3600);
        private static readonly Dictionary<TimeSpeed, float> _Scale = new Dictionary<TimeSpeed, float>
        {
            {TimeSpeed.Slow, 0.5f},
            {TimeSpeed.Normal, 1f},
            {TimeSpeed.Fast, 4f},
            {TimeSpeed.Ultra, 8f},
            {TimeSpeed.WTF, 16f},
            {TimeSpeed.Pause, 0f}
        };
        private static Stopwatch StopWatch { get; set; } = Stopwatch.StartNew();
        public static long DeltaFrame() => StopWatch.ElapsedMilliseconds;

        private void OnDestroy()
        {
            Instance = null;
        }

        private static float _degree;

        private static int _day;

        private static int Hour => (int)(_degree / 360 * 24);

        private static int Minute => (int)((_degree / 360 * 24 - Hour) * 60);

        private static float Seconds => ((_degree / 360 * 24 - Hour) * 60 - Minute) * 60;

        private void Awake()
        {
            Instance = this;
            var sun = transform;
            sun.position = Vector3.up * 200;
            sun.eulerAngles = new Vector3(90, -90, -90);
            sun.RotateAround(Vector3.zero, new Vector3(0, 0, 1), 180);
            sun.RotateAround(Vector3.zero, new Vector3(0, 0, 1), 135); // 9am
            _degree = 135;
            _day = 0;
        }

        private void FixedUpdate()
        {
            var speed = _Scale[TimeSpeed] * DegPerDay;

            var dTheta = Time.fixedDeltaTime * speed;

            transform.RotateAround(Vector3.zero, new Vector3(0, 0, 1), dTheta);

            _degree += dTheta;

            if (!(_degree > 360)) return;
            _day++;
            _degree %= 360;

        }

        private void Update() => StopWatch.Restart();

        public struct Chronos
        {
            private const float Epsilon = 0.001f;
            private int _cDay;
            private int _hr;
            private int _min;
            private float _sec;

            public Chronos(int d, int h, int m, float s)
            {
                _sec = s % 60;
                if (_sec < 0) _sec += 60;
                var acc = Mathf.FloorToInt(s / 60);
                _min = (m + acc) % 60;
                if (_min < 0) _min += 60;
                acc = Mathf.FloorToInt((m + acc) / 60f);
                _hr = (h + acc) % 24;
                if (_hr < 0) _hr += 24;
                acc = Mathf.FloorToInt((h + acc) / 24f);
                _cDay = d + acc;
            }

            public override string ToString()
            {
                return IsNull() ? "" : $"Day {_cDay}, {_hr:00}:{_min:00}";
            }

            public string ToStringLong()
            {
                return IsNull() ? "" : $"{ToString()}:{_sec:00.000}";
            }

            public static Chronos Get()
            {
                return new Chronos(TimeKeeper._day, Hour, Minute, Seconds);
            }

            public bool IsNull()
            {
                return _cDay == 0 && _hr == 0 && _min == 0 && Mathf.Abs(_sec - 0) < 0.001f;
            }

            public Chronos Now()
            {
                _cDay = _day;
                _hr = Hour;
                _min = Minute;
                _sec = Seconds;
                return this;
            }

            public float Timer()
            {
                return (_day - _cDay) * 24 * 3600 + (Hour - _hr) * 3600 + (Minute - _min) * 60 + (Seconds - _sec);
            }

            public string ToCsvFormat()
            {
                return IsNull() ? "" : (_cDay * 24 * 3600 + (_hr - 9) * 3600 + _min * 60 + _sec).ToString("0.0");
            }

            public override bool Equals(object obj)
            {
                if (obj is null) return false;
                return obj is Chronos chronos && this == chronos;
            }

            public override int GetHashCode()
            {
                return _cDay.GetHashCode() ^ _hr.GetHashCode() << 2 ^ _min.GetHashCode() >> 2 ^ Mathf.FloorToInt(_sec).GetHashCode() >> 1;
            }

            #region Operators
            public static bool operator ==(Chronos t1, Chronos t2)
            {
                if (t1.GetHashCode() != t2.GetHashCode()) return false;
                return Mathf.Abs(t1._sec - t2._sec) < Epsilon;
            }
            
            public static bool operator != (Chronos t1, Chronos t2)
            {
                return !(t1 == t2);
            }
            
            public static bool operator >(Chronos t1, Chronos t2)
            {
                if (t1._cDay > t2._cDay) return true;

                if (t1._cDay != t2._cDay) return false;
                
                if (t1._hr > t2._hr) return true;

                if (t1._hr != t2._hr) return false;

                if (t1._min > t2._min) return true;

                if (t1._min != t2._min) return false;

                return t1._sec - t2._sec > Epsilon;
            }

            public static bool operator <(Chronos t1, Chronos t2)
            {
                return !(t1 > t2) && t1 != t2;
            }
            
            public static bool operator >=(Chronos t1, Chronos t2)
            {
                return t1 > t2 || t1 == t2;
            }

            public static bool operator <=(Chronos t1, Chronos t2)
            {
                return t1 < t2 || t1 == t2;
            }

            public static Chronos operator + (Chronos t1, float s)
            {
                return new Chronos(t1._cDay, t1._hr, t1._min, t1._sec + s);
            }

            public static Chronos operator - (Chronos t1, float s)
            {
                return new Chronos(t1._cDay, t1._hr, t1._min, t1._sec - s);
            }

            public static float operator - (Chronos t1, Chronos t2)
            {
                return (t1._cDay - t2._cDay) * 24f * 3600f + (t1._hr - t2._hr) * 3600 + (t1._min - t2._min) * 60 + (t1._sec - t1._sec);
            }
            #endregion
        }

    }


}
