using System;
using Drones.Serializable;
using Drones.Utils.Scheduler;
using UnityEngine;

namespace Drones.Utils
{
    public class CostFunction
    {
        public CostFunction(TimeKeeper.Chronos startTime, float revenue, float penalty = 5)
        {
            Start = startTime.SetReadOnly();
            Reward = revenue;
            Penalty = -Mathf.Abs(penalty);
        }

        public CostFunction(SCostFunction sCost)
        {
            Start = new TimeKeeper.Chronos(sCost.start);
            Reward = sCost.reward;
            Penalty = sCost.penalty;
        }

        public TimeKeeper.Chronos Start { get; private set; }
        public float Reward { get; private set; }
        public float Penalty { get; private set; }
        public const float GUARANTEE = 1800; // half hour

        public float GetPaid(TimeKeeper.Chronos complete)
        {
            float dt = Normalize(complete - Start);
            float reduction = 1 - Discretize(dt);

            return (reduction > 0) ? Reward * reduction : Penalty;
        }

        public static float Evaluate(StrippedJob job, ChronoWrapper complete)
        {
            float dt = Normalize(complete - job.start);
            float reduction = 1 - Discretize(dt);
            job.penalty = -Mathf.Abs(job.penalty);
            return (reduction > 0) ? job.reward * reduction : job.penalty;
        }

        public static ChronoWrapper Inverse(StrippedJob job, float value)
        {
            if (Mathf.Abs(value - job.penalty) < 0.01f) return job.start + GUARANTEE;

            return job.start + (1 - Discretize(value / job.reward)) * GUARANTEE;
        }

        public static float Normalize(float dt) => dt / GUARANTEE;

        public static float Discretize(float ndt, int division = 10)
        {
            if (division < 1) division = 1;

            return ((int)(ndt * division)) / (float)division;
        }

        public SCostFunction Serialize()
        {
            return new SCostFunction
            {
                start = Start.Serialize(),
                reward = Reward,
                penalty = Penalty
            };
        }

        public float Step(float x, float n, float yU = 1, float yD = -1) => (x <= n) ? yU : yD;

        public float Tanh(float x, float a = 1, float b = 1, float c = 0) => (float)(a * Math.Tanh(b * x + c));

        public float Poly(float x, float a = 1, int n = 1) => (float)(a * Math.Pow(x, n));

        public float Exp(float x, float a = 1, float b = 1) => (float)(a * Math.Exp(b * x));
    }
}
