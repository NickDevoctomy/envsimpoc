using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float _moveSpeed = 10f;
    private float _rotateSpeed = 20f;
    private Vector3? _lastPosition = null;
    //private List<GameObject> _lastMonitors = new List<GameObject>();

    void Update()
    {
        float rotation = Input.GetAxis("Horizontal") * _rotateSpeed;
        float movement = Input.GetAxis("Vertical") * _moveSpeed;

        transform.position += transform.forward * Time.deltaTime * movement;
        transform.RotateAround(transform.position, Vector3.up, Time.deltaTime * rotation);
        if(_lastPosition != transform.position)
        {
            ActivateMonitors();
            _lastPosition = transform.position;
        }
    }

    private void SetMonitorActive(GameObject gameObject, bool active)
    {
        if(gameObject.GetComponent<Monitor>() != null)
        {
            gameObject.SetActive(active);
        }
    }

    private void ActivateMonitors()
    {
        var monitors = Map.Instance.Monitors;
        for (int x = 0; x < Map.Instance.Width; x++)
        {
            for (int y = 0; y < Map.Instance.Height; y++)
            {
                var monitor = monitors[x, y];
                if (monitor != null)
                {
                    var distSqr = (transform.position - monitor.transform.position).sqrMagnitude;
                    monitor.SetActive(distSqr < 100f);
                }
            }
        }
    }
}
