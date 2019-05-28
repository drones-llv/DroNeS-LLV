using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Drones.Utils
{
    public class JobGenerator
    {
        readonly Hub _Owner;
        Vector3 Position => _Owner.Position;
        float _lambda;
        public JobGenerator(Hub hub, float lambda)
        {
            _Owner = hub;
            _lambda = lambda;
        }

        public void SetLambda(float l) => _lambda = l;

        public IEnumerator Generate()
        {
            var wait = new WaitForFixedUpdate();
            var time = TimeKeeper.Chronos.Get();
            var watch = Stopwatch.StartNew();
            while (true)
            {
                time.Now();
                var F = Random.value;
                while (F >= 1) F = Random.value;
                var dt = -Mathf.Log(1 - F) / _lambda;

                while (time.Timer() < dt) yield return wait;
                watch.Restart();
                var v = Position;
                v.y = 200;
                var d = Random.insideUnitSphere * 7000;
                d.y = 200;
                while (!Physics.Raycast(new Ray(d, Vector3.down), 200, 1 << 13) || Vector3.Distance(v, d) < 100)
                {
                    d = Random.insideUnitSphere * 7000;
                    d.y = 200;
                    if (watch.ElapsedMilliseconds / 1000 > Time.fixedUnscaledDeltaTime)
                    {
                        yield return wait;
                        watch.Restart();
                    }
                }

                d.y = 0;

                Job job = new Job(_Owner, d, Random.Range(0.1f, 2.5f), 5);
                _Owner.OnJobCreate(job);
            }

        }
    }
}
