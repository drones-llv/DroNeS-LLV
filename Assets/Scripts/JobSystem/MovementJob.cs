using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace Drones.Utils.Jobs
{
    public struct MovementInfo
    {
        public DroneMovement moveType;
        public float height;
        public Vector3 waypoint;
        public int isWaiting;
    }

    public struct MovementJob : IJobParallelForTransform
    {
        public const float g = 9.81f;
        public const float vSpeed = 4.0f;
        public const float hSpeed = 9.0f;
        public float deltaTime;
        [ReadOnly]
        public NativeArray<MovementInfo> nextMove;
        public NativeArray<Vector3> rt_dt;

        public void Execute(int k, TransformAccess transform)
        {
            if (nextMove[k].isWaiting != 0) return;


            if (nextMove[k].moveType == DroneMovement.Ascend || nextMove[k].moveType == DroneMovement.Descend)
            {
                float step = deltaTime * vSpeed;
                Vector3 target = transform.position;
                target.y = nextMove[k].height;
                rt_dt[k] = transform.position;
                transform.position = Vector3.MoveTowards(transform.position, target, step);
            }
            else if (nextMove[k].moveType == DroneMovement.Horizontal)
            {
                float step = deltaTime * hSpeed;
                rt_dt[k] = transform.position;
                transform.position = Vector3.MoveTowards(transform.position, nextMove[k].waypoint, step);
            }
            else if (nextMove[k].moveType == DroneMovement.Drop)
            {
                var rt = transform.position;
                transform.position = 2 * rt - rt_dt[k] + g * Vector3.down * deltaTime * deltaTime;
                rt_dt[k] = rt;
            }
        }
    }
}

