using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;


public class DropdownTestScript : MonoBehaviour
{
    public TextMeshProUGUI tmpText; // 또는 TextMeshPro (3D Text)
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
                        Debug.Log(tmpText.text + " 기능 활성화");
                        break;
                    case "Option B":
                        Debug.Log(tmpText.text + " 기능 활성화");
                        break;
                    case "Option C":
                        Debug.Log(tmpText.text + " 기능 활성화");
                        break;

                }
                Debug.Log("Text 변경됨 : " + tmpText.text);
                previousText = tmpText.text;
            }
            yield return null; // 다음 프레임까지 대기
        }
    }
}

