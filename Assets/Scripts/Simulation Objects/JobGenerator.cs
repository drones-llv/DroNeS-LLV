using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones.Utils
{
    public class JobGenerator
    {
        readonly Hub _Owner;
        float _lambda;
        public JobGenerator(Hub hub, float lambda)
        {
            _Owner = hub;
            _lambda = lambda;
        }

        public void SetLambda(float l) => _lambda = l;

        public IEnumerator Generate()
        {
            var time = TimeKeeper.Chronos.Get();
            while (true)
            {
                time.Now();
                var F = Random.value;
                while (F >= 1) F = Random.value;
                var dt = -Mathf.Log(1 - F) / _lambda;

                yield return new WaitUntil(() => time.Timer() > dt);

                Job[] jobs = new Job[(int)(time.Timer() / dt)];
                for (int i = 0;  i < jobs.Length; i++)
                {
                    var d = Random.insideUnitSphere * 7000;
                    d.y = 200;
                    while (!Physics.Raycast(new Ray(d,Vector3.down), 200, 1 << 13))
                    {
                        d = Random.insideUnitSphere * 7000;
                        d.y = 200;
                    }

                    d.y = 0;

                    jobs[i] = new Job(_Owner, d, Random.Range(0.1f, 2.5f), 5);
                }
                _Owner.OnJobCreate(jobs);
            }


        }
    }
}
