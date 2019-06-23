using System;
using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Drones.Utils;
using NUnit.Framework.Constraints;

public struct Incrementer : IJobParallelFor
{
    public NativeConcurrentIntArray.Concurrent output;
    public void Execute(int index)
    {
        output.Increment(0);
    }
}

public class TestScript : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(Test());


    }

    IEnumerator Test()
    {
        var a = new NativeConcurrentIntArray(1,Allocator.TempJob);
        a.SetValue(0, 16);
        JobHandle job;
        var incrementer = new Incrementer
        {
            output = a.ToConcurrent(),
        };
        job = incrementer.Schedule(1230084, 64);

        yield return null;
        job.Complete();
        Debug.Log(a.GetValue(0));
        a.Dispose();
    }
}
