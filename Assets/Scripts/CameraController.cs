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

    private void ActivateMonitors()
    {
        for (int i = 0; i < Map.Instance.MonitorsList.Count; i++)
        {
            var monitor = Map.Instance.MonitorsList[i];
            if (monitor != null)
            {
                monitor.SetActive((transform.position- monitor.transform.position).sqrMagnitude < 600f);
            }
        }
    }
}
