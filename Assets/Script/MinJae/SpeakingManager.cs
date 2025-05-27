using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class SpeakingManager : MonoBehaviour
{

    private UnityEngine.XR.InputDevice rightController;
    private bool previousPrimaryButtonState = false;

    void Start()
    {
        TryInitialize();
    }

    void TryInitialize()
    {
        var devices = new List<UnityEngine.XR.InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, devices);

        if (devices.Count > 0)
        {
            rightController = devices[0];
            Debug.Log("������ ��Ʈ�ѷ� �����: " + rightController.name);
        }
    }

    void Update()
    {
        if (!rightController.isValid)
        {
            TryInitialize(); // ��ġ ��Ž��
            return;
        }

        // ����: A ��ư (primaryButton), B ��ư (secondaryButton), Trigger, Grip
        if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool primaryState) && primaryState)
        {
            //Debug.Log(primaryState);
            if (!previousPrimaryButtonState && primaryState)
            {
                //Debug.Log("A ��ư Down");
            }
            // Button Up: ���� ���´� true, ����� false
            else if (previousPrimaryButtonState && !primaryState)
            {
                //Debug.Log("A ��ư Up");
            }

            // ���� ���� ����
            previousPrimaryButtonState = primaryState;
        }

        if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out bool secondaryPressed) && secondaryPressed)
        {
            Debug.Log("B ��ư ����");
        }

        if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool triggerPressed) && triggerPressed)
        {
            Debug.Log("Ʈ���� ��ư ����");
        }

        if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out bool gripPressed) && gripPressed)
        {
            Debug.Log("�׸� ��ư ����");
        }
    }
}
