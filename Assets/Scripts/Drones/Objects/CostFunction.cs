﻿using System;
using Drones.Scheduler;
using Drones.Utils;
using UnityEngine;

namespace Drones.Objects
{
    public class CostFunction
    {
        public CostFunction(TimeKeeper.Chronos startTime, float revenue, float penalty = 5)
        {
            Start = startTime;
            Reward = revenue;
            Penalty = -Mathf.Abs(penalty);
        }

        private TimeKeeper.Chronos Start { get; set; }
        public float Reward { get; private set; }
        public float Penalty { get; private set; }
        public const float Guarantee = 1800; // half hour

        public float GetPaid(TimeKeeper.Chronos complete)
        {
            var dt = Normalize(complete - Start);
            var reduction = (dt > int.MaxValue) ? float.MinValue : 1 - Discretize(dt);
            return (reduction > 0) ? Reward * reduction : Penalty;
        }

        public static float Evaluate(StrippedJob job, TimeKeeper.Chronos complete)
        {
            var dt = Normalize(complete - job.start);

            var reduction = (dt > int.MaxValue) ? float.MinValue : 1 - Discretize(dt);
            return (reduction > 0) ? job.reward * reduction : job.penalty;
        }

        public static TimeKeeper.Chronos Inverse(StrippedJob job, float value)
        {
            if (Mathf.Abs(value - job.penalty) < 0.01f) return job.start + Guarantee;

            return job.start + (1 - Discretize(value / job.reward)) * Guarantee;
        }

        private static float Normalize(float dt) => dt / Guarantee;

        private static float Discretize(float ndt, int division = 10)
        {
            if (division < 1) division = 1;

            return ((int)(ndt * division)) / (float)division;
        }

        public float Step(float x, float n, float yU = 1, float yD = -1) => (x <= n) ? yU : yD;

        public float Tanh(float x, float a = 1, float b = 1, float c = 0) => (float)(a * Math.Tanh(b * x + c));

        public float Poly(float x, float a = 1, int n = 1) => (float)(a * Math.Pow(x, n));

        public float Exp(float x, float a = 1, float b = 1) => (float)(a * Math.Exp(b * x));
    }
}
