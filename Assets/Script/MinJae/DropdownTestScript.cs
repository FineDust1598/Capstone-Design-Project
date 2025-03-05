using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;


public class DropdownTestScript : MonoBehaviour
{
    public TextMeshProUGUI tmpText; // �Ǵ� TextMeshPro (3D Text)
    private string previousText = "";

    void Start()
    {
        if (tmpText != null)
        {
            Debug.Log("hello");
            previousText = tmpText.text;
            StartCoroutine(WatchTextChange());
        }
    }

    IEnumerator WatchTextChange()
    {
        
        while (true)
        {
            if (tmpText.text != previousText)
            {
                switch (tmpText.text)
                {
                    case "Option A":
                        Debug.Log(tmpText.text + " ��� Ȱ��ȭ");
                        break;
                    case "Option B":
                        Debug.Log(tmpText.text + " ��� Ȱ��ȭ");
                        break;
                    case "Option C":
                        Debug.Log(tmpText.text + " ��� Ȱ��ȭ");
                        break;

                }
                Debug.Log("Text ����� : " + tmpText.text);
                previousText = tmpText.text;
            }
            yield return null; // ���� �����ӱ��� ���
        }
    }
}

