using System;
using System.Collections.Generic;
using UnityEngine;


public static class Consts
{
    public const int DRONE_SPEED = 10;
}


// Dumb placeholder job class
public class Job
{
    public Vector3 Pickup;
    public Vector3 DropOff;
    public uint UID;

    public int CostFunction(long time)
    {
        return 42;
    }
}


public class LLV
{
    public List<Job> Sort(List<Job> jobs, long time)
    {
        jobs.Sort(delegate (Job a, Job b) {
            if (NetLostValue(a, jobs, time) > NetLostValue(b, jobs, time))
                return 1;
            return -1;
        });

        return jobs;
    }


    private double NetLostValue(Job job, List<Job> allJobs, long time)
    {
        double lostValue = PotentialLostValue(job, allJobs, time);
        double wonValue = PotentialGainedValue(job, allJobs, time);

        return lostValue - wonValue;
    }


    private double PotentialLostValue(Job job, List<Job> allJobs, long time)
    {
        double lostValue = 0;
        foreach (Job j in allJobs)
        {
            if (!(j.UID == job.UID))
            {
                double loss = ExpectedValue(j, time) - ExpectedValue(j, time + (long)ExpectedDuration(j));
                lostValue += loss;
            }
        }

        return lostValue;
    }


    private double PotentialGainedValue(Job job, List<Job> allJobs, long time)
    {
        double cumulativeDuration = 0;
        foreach (Job j in allJobs)
        {
            if (!(j.UID == job.UID))
            {
                cumulativeDuration += ExpectedDuration(j);
            }
        }
        double expectedExpectedDuration = cumulativeDuration / (allJobs.Count - 1);

        return ExpectedValue(job, time) - ExpectedValue(job, time + (long)expectedExpectedDuration);
    }


    private static float EuclideanDist(Job job)
    {
        return (job.Pickup - job.DropOff).magnitude;
    }


    private static float ManhattanDist(Job job)
    {
        return Math.Abs(job.Pickup.x - job.DropOff.x) + Math.Abs(job.Pickup.z - job.DropOff.z);
    }


    private static double ExpectedDuration(Job job)
    {
        double manhattan = ManhattanDist(j);
        double euclidean = EuclideanDist(j);

        double meanDist = (manhattan + euclidean) / 2;
        // double sigmaDist = meanDist - euclidean;
        // NOTE: Variance not necessary since normal dist symmetric around mean

        return meanDist / Consts.DRONE_SPEED;
    }


    private static double ExpectedValue(Job j, long time)
    {
        double manhattan = ManhattanDist(j);
        double euclidean = EuclideanDist(j);

        double meanDist = (manhattan + euclidean) / 2;
        double sigmaDist = meanDist - euclidean;
        double varDist = Math.Pow(sigmaDist, 2);

        double meanDuration = meanDist / Consts.DRONE_SPEED;
        double varDuration = varDist / (Consts.DRONE_SPEED * Consts.DRONE_SPEED);

        // Approximate integral as a sum over 2 hours, assume probability trails off after 2 hours
        double expectedVal = 0;
        for (long t = time; t < 7200; t++)
        {
            // Calculating a lower estimate on distribution
            double coeff = 1 / Math.Sqrt(2 * Math.PI * varDuration);
            double exp = -1 * Math.Pow((t - time - meanDuration), 2) / (2 * varDuration);
            double lowerEstimate = j.CostFunction(t) * coeff * Math.Pow(Math.E, exp);

            // Calculating an upper estimate on distribution
            exp = -1 * Math.Pow((t + 1 - time - meanDuration), 2) / (2 * varDuration);
            double upperEstimate = j.CostFunction(t + 1) * coeff * Math.Pow(Math.E, exp);

            // Average two estimates
            double estimate = (lowerEstimate + upperEstimate) / 2;
            expectedVal += estimate;
        }

        return expectedVal;
    }
}
