using System.Collections;
using System.Diagnostics;
using Drones.Managers;
using Drones.Utils;
using UnityEngine;
using Utils;

namespace Drones.Objects
{
    public class JobGenerator
    {
        private readonly Hub _owner;
        private Vector3 Position => _owner.Position;
        private float _lambda;

        private readonly WaitUntil _capper;
        public JobGenerator(Hub hub, float lambda)
        {
            _owner = hub;
            _lambda = lambda;
            _capper = new WaitUntil(() => _owner.Scheduler.JobQueueLength < 1.5f * _owner.Drones.Count);
        }

        public void SetLambda(float l) => _lambda = l;
        
        public IEnumerator Generate()
        {
            var time = TimeKeeper.Chronos.Get();
            while (true)
            {
                time.Now();
                var f = Random.value;
                while (f >= 1) f = Random.value;
                var dt = -Mathf.Log(1 - f) / _lambda;
                
                while (time.Timer() < dt) yield return null;
                
                var v = Position;
                v.y = 200;
                Vector3 d = Random.insideUnitCircle * (SimManager.Mode == SimulationMode.Delivery ? 7000 : 3500);
                d.z = d.y;
                d += Position;
                d.y = 200;
                while (!Physics.Raycast(new Ray(d, Vector3.down), 200, 1 << 13) || Vector3.Distance(v, d) < 100)
                {
                    d = Random.insideUnitCircle * (SimManager.Mode == SimulationMode.Delivery ? 7000 : 2750);
                    d.z = d.y;
                    d += Position;
                    d.y = 200;
                    if (TimeKeeper.DeltaFrame() < 16) continue;
                    yield return null;
                }
                d.y = 0;
                var job = new Job(_owner, d);

                _owner.OnJobCreate(job);
                yield return _capper;
            }

        }
    }
}
