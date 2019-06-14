using System;
using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;

namespace Drones.Utils
{
    using Serializable;
    using Managers;
    using UI;
    using Drones.Utils.Scheduler;

    public class TimeKeeper : MonoBehaviour
    {

        public static TimeKeeper Instance { get; private set; }
        [SerializeField]
        private TimeSpeed _TimeSpeed = TimeSpeed.Pause;
        public static TimeSpeed TimeSpeed
        {
            get => Instance._TimeSpeed;

            set
            {
                if (Instance._TimeSpeed != value)
                {
                    if (SimManager.Status == SimulationStatus.EditMode && value != TimeSpeed.Pause) return;
                    Instance._TimeSpeed = value;
                }
                ShowSimSpeed.OnSpeedChange();
            }
        }
        private const float DEG_PER_DAY = 360.0f / (24 * 3600);
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

        public static void SetTime(STime time)
        {
            _Degree = time.hr / 24f * 360f + time.min / 24f / 60f * 360f + time.sec * DEG_PER_DAY;
        }

        private static float _Degree;

        private static int _Day;

        private static int Hour => (int)(_Degree / 360 * 24);

        private static int Minute => (int)((_Degree / 360 * 24 - Hour) * 60);

        private static float Seconds => ((_Degree / 360 * 24 - Hour) * 60 - Minute) * 60;

        void Awake()
        {
            Instance = this;
            transform.position = Vector3.up * 200;
            transform.eulerAngles = new Vector3(90, -90, -90);
            transform.RotateAround(Vector3.zero, new Vector3(0, 0, 1), 180);
            transform.RotateAround(Vector3.zero, new Vector3(0, 0, 1), 135); // 9am
            _Degree = 135;
        }

        private void FixedUpdate()
        {
            float speed = _Scale[TimeSpeed] * DEG_PER_DAY;

            float dTheta = Time.fixedDeltaTime * speed;

            transform.RotateAround(Vector3.zero, new Vector3(0, 0, 1), dTheta);

            _Degree += dTheta;

            if (_Degree > 360)
            {
                _Day++;
                _Degree %= 360;
            }

        }

        private void Update() => StopWatch.Restart();

        public class Chronos
        {
            int day;
            int hr;
            int min;
            float sec;
            bool readOnly;

            public Chronos(int d, int h, int m, float s)
            {
                int acc;
                sec = s % 60;
                if (sec < 0) sec += 60;
                acc = Mathf.FloorToInt(s / 60);
                min = (m + acc) % 60;
                if (min < 0) min += 60;
                acc = Mathf.FloorToInt((m + acc) / 60f);
                hr = (h + acc) % 24;
                if (hr < 0) hr += 24;
                acc = Mathf.FloorToInt((h + acc) / 24f);
                day = d + acc;
                readOnly = false;
            }

            public Chronos(STime time)
            {
                sec = time.sec;
                min = time.min;
                hr = time.hr;
                day = time.day;
                readOnly = time.isReadOnly;
            }

            public Chronos SetReadOnly()
            {
                readOnly = true;
                return this;
            }

            public override string ToString()
            {
                return string.Format("Day {0}, {1}:{2}", day, hr.ToString("00"), min.ToString("00"));
            }

            public string ToStringLong()
            {
                return ToString() + ":" + sec.ToString("00.000");
            }

            public static Chronos Get()
            {
                return new Chronos(_Day, Hour, Minute, Seconds);
            }

            public Chronos Now()
            {
                if (!readOnly)
                {
                    day = _Day;
                    hr = Hour;
                    min = Minute;
                    sec = Seconds;
                }
                return this;
            }

            public float Timer()
            {
                return (_Day - day) * 24 * 3600 + (Hour - hr) * 3600 + (Minute - min) * 60 + (Seconds - sec);
            }

            public string ToCSVFormat()
            {
                return (day * 24 * 3600 + (hr - 9) * 3600 + min * 60 + sec).ToString("0.0");
            }

            public STime Serialize()
            {
                return new STime
                {
                    sec = this.sec,
                    min = this.min,
                    hr = this.hr,
                    day = this.day,
                    isReadOnly = readOnly
                };
            }

            public override bool Equals(object obj) => obj is Chronos && this == ((Chronos)obj);

            public override int GetHashCode() => base.GetHashCode();

            static public explicit operator ChronoWrapper(Chronos time)
            {
                return new ChronoWrapper(time.day, time.hr, time.min, time.sec);
            }

            #region Operators
            public static bool operator <(Chronos t1, Chronos t2)
            {
                if (t1.day < t2.day) return true;

                if (t1.day == t2.day)
                {
                    if (t1.hr < t2.hr) return true;
                    if (t1.hr == t2.hr)
                    {
                        if (t1.min < t2.min) return true;
                    }
                }
                return false;
            }

            public static bool operator >(Chronos t1, Chronos t2)
            {
                if (t1.day > t2.day) return true;

                if (t1.day == t2.day)
                {
                    if (t1.hr > t2.hr) return true;

                    if (t1.hr == t2.hr)
                    {
                        if (t1.min > t2.min) return true;
                    }
                }
                return false;
            }

            public static bool operator ==(Chronos t1, Chronos t2)
            {

                return !(t1 is null) && !(t2 is null) && t1.day == t2.day && t1.hr == t2.hr && t1.min == t2.min
                    || (t1 is null && t2 is null);
            }

            public static bool operator >=(Chronos t1, Chronos t2)
            {
                return t1 > t2 || t1 == t2;
            }

            public static bool operator <=(Chronos t1, Chronos t2)
            {
                return t1 < t2 || t1 == t2;
            }

            public static bool operator != (Chronos t1, Chronos t2)
            {
                return !(t1 == t2);
            }

            public static Chronos operator + (Chronos t1, float s)
            {
                return new Chronos(t1.day, t1.hr, t1.min, t1.sec + s);
            }

            public static Chronos operator - (Chronos t1, float s)
            {
                return new Chronos(t1.day, t1.hr, t1.min, t1.sec - s);
            }

            public static float operator - (Chronos t1, Chronos t2)
            {
                return (t1.day - t2.day) * 24f * 3600f + (t1.hr - t2.hr) * 3600 + (t1.min - t2.min) * 60 + (t1.sec - t1.sec);
            }
            #endregion
        }

    }


}
