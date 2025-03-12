using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SlidingDoorController : MonoBehaviour
{

    public Transform door; // �� ������Ʈ
    public float minX = 0f; // ���� ���� ��ġ
    public float maxX = 3f; // ���� ���� ��ġ
    public float speed = 2f; // ���� �����̴� �ӵ�

    private bool isGrabbed = false;
    private XRGrabInteractable grabInteractable;
    private Vector3 initialPosition;

    private Quaternion initialRotation; // �ʱ� ȸ�� �� ����


    // Start is called before the first frame update
    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);

        initialPosition = door.position;
        initialRotation = door.rotation; // �ʱ� ȸ�� �� ����

    }

    // Update is called once per frame
    void Update()
    {
        if (isGrabbed)
        {
            // ���� X�� ���� �������� �����̵��� ����
            float clampedX = Mathf.Clamp(door.position.x, minX, maxX);
            door.position = new Vector3(clampedX, initialPosition.y, initialPosition.z);

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
