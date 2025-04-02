using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SlidingDoorController : MonoBehaviour
{

    public Transform door; // �� ������Ʈ
    public float minZ = 32.5f; // ���� ���� ��ġ
    public float maxZ = 48.5f; // ���� ���� ��ġ
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
