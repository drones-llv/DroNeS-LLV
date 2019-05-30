using System.Collections;
using System.Collections.Generic;
using Drones.Utils.Jobs;
using UnityEngine;
namespace Drones.Utils.Scheduler
{
    public class LLV : AbstractScheduler
    {
        public override List<StrippedJob> Sort(List<StrippedJob> jobs, ChronoWrapper time)
        {
            jobs.Sort(delegate (StrippedJob a, StrippedJob b) {
                return (NetLostValue(a, jobs, time) > NetLostValue(b, jobs, time)) ? 1 : -1;
            });
            return jobs;
        }

        private static float NetLostValue(StrippedJob job, List<StrippedJob> allJobs, ChronoWrapper time)
        {
            var lostValue = PotentialLostValue(job, allJobs, time);
            var wonValue = PotentialGainedValue(job, allJobs, time);

            return lostValue - wonValue;
        }

        private static float PotentialLostValue(StrippedJob job, List<StrippedJob> allJobs, ChronoWrapper time)
        {
            float lostValue = 0;
            for (int i = 0; i < allJobs.Count; i++)
            {
                if (allJobs[i].UID != job.UID)
                {
                    lostValue += ExpectedValue(allJobs[i], time) - ExpectedValue(allJobs[i], time + ExpectedDuration(allJobs[i]));
                }
            }

            return lostValue;
        }

        private static float PotentialGainedValue(StrippedJob job, List<StrippedJob> allJobs, ChronoWrapper time)
        {
            float cumulativeDuration = 0;
            for (int i = 0; i < allJobs.Count; i++)
            {
                if (allJobs[i].UID != job.UID)
                {
                    cumulativeDuration += ExpectedDuration(allJobs[i]);
                }
            }
            var mean = cumulativeDuration / (allJobs.Count - 1);

            return ExpectedValue(job, time) - ExpectedValue(job, time + mean);
        }

        private static float ExpectedDuration(StrippedJob job)
        {
            var manhattan = ManhattanDist(job);
            var euclidean = EuclideanDist(job);

            var mean = (manhattan + euclidean) / 2;
            // float sigmaDist = meanDist - euclidean;
            // NOTE: Variance not necessary since normal dist symmetric around mean

            return mean / MovementJob.HSPEED;
        }


    }
}
