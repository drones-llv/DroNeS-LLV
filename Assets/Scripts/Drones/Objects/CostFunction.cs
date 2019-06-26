using Drones.Managers;
using Drones.Utils;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Drones.Objects
{
    public struct CostFunction
    {
        public CostFunction(float reward)
        {
            Start = TimeKeeper.Chronos.Get();
            _mode = SimManager.Mode;
            Reward = reward;
            Penalty = _mode == SimulationMode.Delivery ? -5 : 0;
            if (SimManager.Mode == SimulationMode.Delivery) Guarantee = 1800f;
            else Guarantee = Random.value < 0.5 ? 7 * 60 : 18 * 60;
        }
        
        public TimeKeeper.Chronos Start;
        public readonly float Reward;
        public readonly float Penalty;
        public readonly float Guarantee;
        private readonly SimulationMode _mode;  
        

        public static float Evaluate(in CostFunction job, in TimeKeeper.Chronos complete)
        {
            var dt = (complete - job.Start) / job.Guarantee;

            var reduction = (dt > int.MaxValue) ? float.MinValue : 1 - Discretize(dt);
            return (reduction > 0) ? job.Reward * reduction : job.Penalty;
        }

        public static TimeKeeper.Chronos Inverse(in CostFunction job, float value)
        {
            if (Mathf.Abs(value - job.Penalty) < 0.01f) return job.Start + job.Guarantee;

            return job.Start + (1 - Discretize(value / job.Reward)) * job.Guarantee;
        }

        private static float Discretize(float ndt, int division = 10)
        {
            if (division < 1) division = 1;

            return ((int)(ndt * division)) / (float)division;
        }
        
    }
}
