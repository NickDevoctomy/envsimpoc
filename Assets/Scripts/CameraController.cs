using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float _moveSpeed = 10f;
    private float _rotateSpeed = 20f;
    private Vector3? _lastPosition = null;

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
        for (int i = 0; i < Map.Instance.MonitorGroups.Count; i++)
        {
            var monitorGroup = Map.Instance.MonitorGroups[i];
            if (monitorGroup != null)
            {
                monitorGroup.SetActive((transform.position - monitorGroup.transform.position).sqrMagnitude < 1000f);
            }
        }
    }
}
