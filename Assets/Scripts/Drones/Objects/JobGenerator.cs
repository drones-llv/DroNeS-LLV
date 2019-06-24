using System.Collections;
using System.Diagnostics;
using Drones.Utils;
using UnityEngine;

namespace Drones.Objects
{
    public class JobGenerator
    {
        private readonly Hub _owner;
        private Vector3 Position => _owner.Position;
        private float _lambda;
        private readonly WaitForFixedUpdate _fixed = new WaitForFixedUpdate();
        private readonly WaitUntil _capper;
        public JobGenerator(Hub hub, float lambda)
        {
            _owner = hub;
            _lambda = lambda;
            _capper  = new WaitUntil(() => _owner.Scheduler.JobQueueLength < 1.5f * _owner.Drones.Count);
        }

        public void SetLambda(float l) => _lambda = l;

        public IEnumerator GenerateDeliveries()
        {
            var time = TimeKeeper.Chronos.Get();
            var watch = Stopwatch.StartNew();
            while (true)
            {
                time.Now();
                var f = Random.value;
                while (f >= 1) f = Random.value;
                var dt = -Mathf.Log(1 - f) / _lambda;

                while (time.Timer() < dt) yield return _fixed;
                watch.Restart();
                var v = Position;
                v.y = 200;
                var d = Random.insideUnitSphere * 7000;
                d.y = 200;
                while (!Physics.Raycast(new Ray(d, Vector3.down), 200, 1 << 13) || Vector3.Distance(v, d) < 100)
                {
                    d = Random.insideUnitSphere * 7000;
                    d.y = 200;
                    if (watch.ElapsedMilliseconds / 1000 < Time.fixedUnscaledDeltaTime) continue;
                    yield return _fixed;
                    watch.Restart();
                }
                d.y = 0;
                var job = new DeliveryJob(_owner, d, Random.Range(0.1f, 2.5f), 5);

                _owner.OnJobCreate(job);
                yield return _capper;
                watch.Restart();
            }

        }
    }
}
