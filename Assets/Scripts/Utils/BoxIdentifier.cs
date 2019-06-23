using System;
using Utils.Interfaces;

namespace Utils
{

    public class BoxIdentifier
    {
        readonly IClosestPoint unity;
        private readonly XVector3 _Centre;
        private XVector3[] _verts;
        public XVector3 Corner { get; private set; }
        public XVector3 End { get; private set; }
        public XVector3 Start { get; private set; }
        public float Width { get; private set; }
        public float Length { get; private set; }
        public bool TooSmall;
        private readonly float _Epsilon = (float)Math.Cos(Math.PI / 2 - Math.PI / 45);

        private float Abs(float x)
        {
            return (x < 0) ? -x : x;
        }

        private float Exp(float x) => x < 0 ? 0 : (float) Math.Pow(1.6, x);

        public BoxIdentifier(IClosestPoint point)
        {
            unity = point;
            _Centre = new XVector3(point.GetCentre());
            try 
            {
                GetVertices();
                if (!CheckVolume()) { GetDimensions(); }
            } 
            catch
            {
                GetAltVertices();
                if (!CheckVolume()) { GetDimensions(); }
            }

        }

        private XVector3 GetPoint(Directions dir) => unity.GetClosestPoint(_Centre, dir);

        private bool CheckVolume()
        {
            float volume = XVector3.Distance(_verts[0], _verts[1]);
            volume *= XVector3.Distance(_verts[1], _verts[2]);

            XVector3 high = GetPoint(Directions.Up);
            XVector3 low = GetPoint(Directions.Down);
            volume *= high.y - low.y;

            TooSmall = volume < 70;
            return TooSmall;
        }

        private XVector3[] SetYZero(XVector3[] points)
        {
            for (int i = 0; i < points.Length; i++)
                points[i].y = 0;
            return points;
        }

        private XVector3[] GetVertices()
        {
            _verts = new XVector3[4];
            _verts[0] = GetPoint(Directions.East);
            _verts[1] = GetPoint(Directions.South);
            _verts[2] = GetPoint(Directions.West);
            _verts[3] = GetPoint(Directions.North);
            _verts = SetYZero(_verts);
            GetReferencePoint();
            return _verts;
        }

        private XVector3[] GetAltVertices()
        {
            _verts = new XVector3[4];

            _verts[0] = GetPoint(Directions.Northeast);
            _verts[1] = GetPoint(Directions.Southeast);
            _verts[2] = GetPoint(Directions.Southwest);
            _verts[3] = GetPoint(Directions.Northwest);
            _verts = SetYZero(_verts);
            GetReferencePoint();

            return _verts;
        }

        private void GetReferencePoint()
        {
            bool assigned = false;
            float min = float.MaxValue;

            var ji = XVector3.Normalize(_verts[0] - _verts[1]);
            var jk = XVector3.Normalize(_verts[2] - _verts[1]);
            var kl = XVector3.Normalize(_verts[3] - _verts[2]);
            var il = XVector3.Normalize(_verts[3] - _verts[0]);
            var ij = -ji;
            var kj = -jk;
            var lk = -kl;
            var li = -il;

            float[] dots = { XVector3.Dot(ij, il), XVector3.Dot(ji, jk),
                                XVector3.Dot(kj, kl), XVector3.Dot(lk, li) };

            for (int n = 0; n < dots.Length; n++)
            {
                if (dots[n] > -_Epsilon  && min > dots[n] && (dots[(n + 3) % dots.Length] > -_Epsilon ||
                    dots[(n + 1) % dots.Length] > - _Epsilon))
                {
                    assigned = true;
                    Start = _verts[n];
                    Corner = _verts[(n + 1) % dots.Length];
                    min = dots[n];
                }
            }
            if (!assigned)
            {
                Start = _verts[0];
                Corner = _verts[1];
            }
        }

        private XVector3 GetFurthestVertFrom(XVector3 o)
        {
            MaxHeap<XVector3> sorter = new MaxHeap<XVector3>((XVector3 a, XVector3 b) =>
            {
                return (XVector3.Distance(o, a) <= XVector3.Distance(o, b)) ? -1 : 1;
            });

            foreach (XVector3 vert in _verts)
            {
                sorter.Add(vert);
            }

            return sorter.Remove();
        }

        private float MinDistanceBetweenAdjacentVerts()
        {
            float min = float.MaxValue;
            for (int i = 0; i < _verts.Length - 1; i++)
            {
                for (int j = i + 1; j < _verts.Length; j++)
                {
                    float a = XVector3.Distance(_verts[i], _verts[j]);
                    if (min > a) { min = a; }
                }
            }
            return min;
        }

        private void GetDimensions()
        {
            int step = ExponentialSearch(Start, Corner);

            if (step > 0)
            {
                Corner = BisectionSearch(Start, Corner, Exp(step - 1), Exp(step));
            }
            Length = XVector3.Distance(Start, Corner);
            //unity.Message(Start +" " + Corner);
            End = Start - Corner;
            End = new XVector3(-End.z, End.y, End.x);
            End = Corner + End.normalized;// * MinDistanceBetweenAdjacentVerts();

            step = ExponentialSearch(Corner, End);

            if (step > 0)
            {
                End = BisectionSearch(Corner, End, Exp(step - 1), Exp(step));
            }
            Width = XVector3.Distance(Corner, End);
            /* refPoint -> lineEnd -> lineEnd2 ALWAYS clockwise; vertices[i++] ALWAYS clockwise */
        }

        private int ExponentialSearch(XVector3 origin, XVector3 end, int step = -1)
        {
            var newEnd = end + Exp(step) * XVector3.Normalize(end - origin);
            for (int i = 0; i < _verts.Length; i++)
            {
                float dot = XVector3.Dot(XVector3.Normalize(origin - newEnd), XVector3.Normalize(_verts[i] - newEnd));
                if (dot < 0) 
                {
                    return ExponentialSearch(origin, end, step + 1); 
                }
            }
            return step;
        }

        private XVector3 BisectionSearch(XVector3 origin, XVector3 end, float min, float max)
        {
            float mid = (min + max) / 2;
            XVector3 newEnd = end + mid * XVector3.Normalize(end - origin);
            float minDot = float.MaxValue;

            for (int i = 0; i < _verts.Length; i++)
            {
                float dot = XVector3.Dot(XVector3.Normalize(origin - newEnd), XVector3.Normalize(_verts[i] - newEnd));
                if (dot < -_Epsilon)
                {
                    return BisectionSearch(origin, end, mid, max);
                }
                if (minDot > dot) { minDot = dot; }
            }

            if (minDot > _Epsilon)
            {
                return BisectionSearch(origin, end, min, mid);
            }
            return newEnd;
        }


    }
}
