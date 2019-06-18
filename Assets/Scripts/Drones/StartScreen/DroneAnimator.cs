using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Drones.StartScreen
{
    public class DroneAnimator : MonoBehaviour
    {
        private readonly WaitForSecondsRealtime _spawnFreq = new WaitForSecondsRealtime(1);
        private GameObject _template;
        private readonly Stack<RectTransform> _idles = new Stack<RectTransform>();
        private readonly List<RectTransform> _goingLeft = new List<RectTransform>();
        private readonly List<RectTransform> _goingRight = new List<RectTransform>();
        private readonly List<float> _speedLeft = new List<float>();
        private readonly List<float> _speedRight = new List<float>();
        private const int Max = 20;
        private const float MaxHeight = 580;
        private const float Sides = 1000;

        private void Awake()
        {
            _template = Resources.Load("Prefabs/StartScreen/Drone", typeof(GameObject)) as GameObject;
            for (var i = 0; i < Max; i++)
            {
                _idles.Push(Instantiate(_template).transform.ToRect());
                _idles.Peek().SetParent(transform);
                _idles.Peek().gameObject.SetActive(false);
            }

        }

        private void OnEnable()
        {
            StartCoroutine(Spawner());
            StartCoroutine(Animate());
        }

        private void OnDisable()
        {
            StopCoroutine(Spawner());
            StopCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            while (true)
            {
                for (var i = _goingLeft.Count - 1; i >= 0; i--)
                {
                    var drone = _goingLeft[i];
                    drone.localPosition -= Vector3.right * _speedLeft[i];
                    if (!(drone.localPosition.x <= -Sides)) continue;
                    drone.gameObject.SetActive(false);
                    _speedLeft.RemoveAt(i);
                    _goingLeft.RemoveAt(i);
                    _idles.Push(drone);
                }
                for (var i = _goingRight.Count - 1; i >= 0; i--)
                {
                    var drone = _goingRight[i];
                    drone.localPosition += Vector3.right * _speedRight[i];
                    if (!(drone.localPosition.x >= Sides)) continue;
                    drone.gameObject.SetActive(false);
                    _speedRight.RemoveAt(i);
                    _goingRight.RemoveAt(i);
                    _idles.Push(drone);
                }
                yield return null;
            }

        }

        private IEnumerator Spawner()
        {
            var goLeft = Random.value < 0.5f;
            while (true)
            {
                if (_idles.Count > 0)
                {
                    var d = _idles.Pop();
                    d.gameObject.SetActive(true);
                    d.localPosition = new Vector3(goLeft ? Sides : -Sides, Random.value * MaxHeight, 0);

                    if (goLeft)
                    {
                        _goingLeft.Add(d);
                        _speedLeft.Add(2 + Random.value * 3);
                    }
                    else
                    {
                        _goingRight.Add(d);
                        _speedRight.Add(2 + Random.value * 3);
                    }

                    goLeft = Random.value < 0.5f;
                }
                yield return _spawnFreq;

            }

        }


    }

}
