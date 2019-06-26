using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;
using Utils;

namespace Drones.JobSystem
{
    public struct DroneMovementInfo
    {
        public DroneMovement MoveType;
        public int IsWaiting;
        public float3 Waypoint;
        public float3 PrevPos;
    }
    [BurstCompile]
    public struct DroneMovementJob : IJobParallelForTransform
    {
        private const float g = 9.81f;
        public const float VerticalSpeed = 4.0f;
        public const float HorizontalSpeed = 12f;
        public float DeltaTime;
        public NativeArray<DroneMovementInfo> NextMove;

        public void Execute(int k, TransformAccess transform)
        {
            var next = NextMove[k];
            
            if (next.IsWaiting != 0) return;
            
            if (next.MoveType == DroneMovement.Ascend || next.MoveType == DroneMovement.Descend)
            {
                var target = transform.position;
                target.y = next.Waypoint.y;
                var step = DeltaTime * VerticalSpeed;
                next.PrevPos = transform.position;
                transform.position = Vector3.MoveTowards(transform.position, target, step);
            }
            else if (next.MoveType == DroneMovement.Horizontal)
            {
                var step = DeltaTime * HorizontalSpeed;
                next.PrevPos = transform.position;
                transform.position = Vector3.MoveTowards(transform.position, next.Waypoint, step);
            }
            else if (next.MoveType == DroneMovement.Drop)
            {
                var rt = (float3)transform.position;
                transform.position = 2 * rt - next.PrevPos + new float3(0,-g,0) * DeltaTime * DeltaTime;
                next.PrevPos = rt;
            }
            NextMove[k] = next;
        }
    }
}

