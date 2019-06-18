using System.Collections;
using Drones.Event_System;
using UnityEngine;
using Utils;

namespace Drones.UI.Console
{
    public class LogEventTest : MonoBehaviour
    {
        IEnumerator Start()
        {
            var wait = new WaitForSeconds(.1f);
            yield return new WaitForSeconds(1);
            for (int i = 0; i < 24; i++)
            {
                ConsoleLog.WriteToConsole(new POIMarked(i.ToString(), (Random.insideUnitSphere * 150).ToArray()));
                yield return wait;
            }
            yield break;
        }

    }
}
