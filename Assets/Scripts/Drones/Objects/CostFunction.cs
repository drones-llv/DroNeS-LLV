using System;
using Drones.Managers;
using Drones.Scheduler;
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


        public static float Evaluate(in CostFunction cost, in TimeKeeper.Chronos complete)
        {
            var dt = (complete - cost.Start) / cost.Guarantee;;
            if (cost._mode == SimulationMode.Emergency)
            {
                var x = 1 - dt;
                return x > 0 ? x : 0;
            }

            var reduction = (dt > int.MaxValue) ? float.MinValue : 1 - Discretize(dt);
            return (reduction > 0) ? cost.Reward * reduction : cost.Penalty;
        }

        public static TimeKeeper.Chronos Inverse(in CostFunction cost, float value)
        {
            if (cost._mode == SimulationMode.Emergency)
            {
                return cost.Start + (1 - value) * cost.Guarantee;
            }
            
            if (Mathf.Abs(value - cost.Penalty) < 0.01f) return cost.Start + cost.Guarantee;

            return cost.Start + (1 - Discretize(value / cost.Reward)) * cost.Guarantee;
        }

        private static float Discretize(float dt) => ((int)(dt * 10)) / (float)10;
    }
}
