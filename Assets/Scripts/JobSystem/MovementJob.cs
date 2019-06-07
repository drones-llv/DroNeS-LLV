using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Burst;
using Unity.Mathematics;

namespace Drones.Utils.Jobs
{
    public struct MovementInfo
    {
        public DroneMovement moveType;
        public float height;
        public int isWaiting;
        public float3 waypoint;
        public float3 prev_pos;
    }
    [BurstCompile]
    public struct MovementJob : IJobParallelForTransform
    {
        public const float g = 9.81f;
        public const float VSPEED = 4.0f;
        public const float HSPEED = 12f;
        public float deltaTime;
        public NativeArray<MovementInfo> nextMove;

        public void Execute(int k, TransformAccess transform)
        {
            if (nextMove[k].isWaiting != 0) return;

            var info = nextMove[k];
            if (info.moveType == DroneMovement.Ascend || nextMove[k].moveType == DroneMovement.Descend)
            {
                float step = deltaTime * VSPEED;
                Vector3 target = transform.position;
                target.y = nextMove[k].height;
                info.prev_pos = transform.position;
                transform.position = Vector3.MoveTowards(transform.position, target, step);
            }
            else if (nextMove[k].moveType == DroneMovement.Horizontal)
            {
                float step = deltaTime * HSPEED;
                info.prev_pos = transform.position;
                transform.position = Vector3.MoveTowards(transform.position, nextMove[k].waypoint, step);
            }
            else if (nextMove[k].moveType == DroneMovement.Drop)
            {
                var rt = (float3)transform.position;
                transform.position = 2 * rt - info.prev_pos + new float3(0,-g,0) * deltaTime * deltaTime;
                info.prev_pos = rt;
            }
            nextMove[k] = info;
        }
    }
}

