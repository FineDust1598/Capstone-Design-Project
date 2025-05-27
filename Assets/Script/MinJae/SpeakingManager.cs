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
            Debug.Log("오른손 컨트롤러 연결됨: " + rightController.name);
        }
    }

    void Update()
    {
        if (!rightController.isValid)
        {
            TryInitialize(); // 장치 재탐색
            return;
        }

        // 예시: A 버튼 (primaryButton), B 버튼 (secondaryButton), Trigger, Grip
        if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool primaryState) && primaryState)
        {
            //Debug.Log(primaryState);
            if (!previousPrimaryButtonState && primaryState)
            {
                //Debug.Log("A 버튼 Down");
            }
            // Button Up: 이전 상태는 true, 현재는 false
            else if (previousPrimaryButtonState && !primaryState)
            {
                //Debug.Log("A 버튼 Up");
            }

            // 현재 상태 저장
            previousPrimaryButtonState = primaryState;
        }

        if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out bool secondaryPressed) && secondaryPressed)
        {
            Debug.Log("B 버튼 눌림");
        }

        if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool triggerPressed) && triggerPressed)
        {
            Debug.Log("트리거 버튼 눌림");
        }

        if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out bool gripPressed) && gripPressed)
        {
            Debug.Log("그립 버튼 눌림");
        }
    }
}
