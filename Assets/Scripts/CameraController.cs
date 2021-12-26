using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float MoveSpeed = 10f;
    private float RotateSpeed = 20f;

    void Update()
    {
        float rotation = Input.GetAxis("Horizontal") * RotateSpeed;
        float movement = Input.GetAxis("Vertical") * MoveSpeed;

        transform.position += transform.forward * Time.deltaTime * movement;
        transform.RotateAround(transform.position, Vector3.up, Time.deltaTime * rotation);
    }
}
