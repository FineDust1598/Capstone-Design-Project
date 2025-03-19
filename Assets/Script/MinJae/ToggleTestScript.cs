using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleTestScript : MonoBehaviour
{
    public Toggle toggle; // Inspector���� ������ Toggle UI

    void Start()
    {
        // toggle�� null���� Ȯ�� (���� ����)
        if (toggle == null)
        {
            Debug.LogError("Toggle�� ������� �ʾҽ��ϴ�! Inspector���� �Ҵ��ϼ���.");
            return;
        }

        // Toggle ���� ����� �� ����� �Լ� ���
        toggle.onValueChanged.AddListener(OnToggleChanged);

        // �ʱ� ���� �α� ���
        Debug.Log($"�ʱ� Toggle ����: {toggle.isOn}");
    }

    void OnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            Debug.Log("Toggle�� 'True' ���°� �Ǿ����ϴ�.");
        }
        else
        {
            Debug.Log("Toggle�� 'False' ���°� �Ǿ����ϴ�.");
        }
    }
}

