using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Drones.Utils.Scheduler
{
    public class EP : AbstractScheduler
    {
        public override List<StrippedJob> Sort(List<StrippedJob> jobs, ChronoWrapper time)
        {
            jobs.Sort(delegate (StrippedJob j1, StrippedJob j2) {
                var a = ExpectedValue(j2, time) + ExpectedValue(j1, time + LeftTime(j2, time));
                var b = ExpectedValue(j1, time) + ExpectedValue(j2, time + LeftTime(j1, time));
                return (a > b) ? 1 : -1;
            });

            return jobs;
        }

        public float LeftTime(StrippedJob job, ChronoWrapper time)
        {
            var finishValue = ExpectedValue(job, time);
            ChronoWrapper finishTime = CostFunction.Inverse(job, finishValue);
            float timeLeft = finishTime - time;

            // fix when timeLeft is smaller than 0
            return timeLeft < 0 ? -timeLeft : timeLeft;
        }
    }
}

