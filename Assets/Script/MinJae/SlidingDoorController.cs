using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SlidingDoorController : MonoBehaviour
{

    public Transform door; // 문 오브젝트
    public float minZ = 32.5f; // 닫힌 상태 위치
    public float maxZ = 48.5f; // 열린 상태 위치
    public float speed = 2f; // 문이 움직이는 속도

    private bool isGrabbed = false;
    private XRGrabInteractable grabInteractable;
    private Vector3 initialPosition;

    private Quaternion initialRotation; // 초기 회전 값 저장


    // Start is called before the first frame update
    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);

        initialPosition = door.position;
        initialRotation = door.rotation; // 초기 회전 값 저장

    }

    // Update is called once per frame
    void Update()
    {
        if (isGrabbed)
        {
            // 문이 X축 범위 내에서만 움직이도록 제한
            float clampedZ = Mathf.Clamp(door.position.z, minZ, maxZ);
            door.position = new Vector3(initialPosition.x, initialPosition.y, clampedZ);

            door.rotation = initialRotation;
        }
    }
    private void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
    }
}
