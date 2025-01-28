using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestController : MonoBehaviour
{
    public float moveSpeed = 3.0f;  // 이동 속도
    public float lookSpeed = 2.0f;  // 마우스 감도

    private float rotationX = 0f;   // X축 회전값

    void Update()
    {
        Move();
        LookAround();
    }

    void Move()
    {
        float moveX = Input.GetAxis("Horizontal"); // A, D → 좌우 이동
        float moveZ = Input.GetAxis("Vertical");   // W, S → 앞뒤 이동

        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.Self);
    }

    void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f); // 위아래 회전 제한

        transform.Rotate(Vector3.up * mouseX);
        Camera.main.transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
    }
}