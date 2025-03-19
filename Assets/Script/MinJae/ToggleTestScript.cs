using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleTestScript : MonoBehaviour
{
    public Toggle toggle; // Inspector에서 연결할 Toggle UI

    void Start()
    {
        // toggle이 null인지 확인 (예외 방지)
        if (toggle == null)
        {
            Debug.LogError("Toggle이 연결되지 않았습니다! Inspector에서 할당하세요.");
            return;
        }

        // Toggle 값이 변경될 때 실행될 함수 등록
        toggle.onValueChanged.AddListener(OnToggleChanged);

        // 초기 상태 로그 출력
        Debug.Log($"초기 Toggle 상태: {toggle.isOn}");
    }

    void OnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            Debug.Log("Toggle이 'True' 상태가 되었습니다.");
        }
        else
        {
            Debug.Log("Toggle이 'False' 상태가 되었습니다.");
        }
    }
}

