using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastReceiver : MonoBehaviour
{
    void Start()
    {
        VRRaycast vrRaycast = Camera.main.GetComponent<VRRaycast>();
        if (vrRaycast != null)
        {
            vrRaycast.OnRaycastHit += HandleRaycastHit;
            vrRaycast.OnRaycastExit += HandleRaycastExit;
            vrRaycast.OnRaycastClick += HandleRaycastClick; // 클릭 이벤트 구독
        }
    }

    void HandleRaycastHit(GameObject obj)
    {
        Debug.Log("Raycast Hit: " + obj.name);
    }

    void HandleRaycastExit(GameObject obj)
    {
        Debug.Log("Raycast Exit: " + obj.name);
    }

    void HandleRaycastClick(GameObject obj)
    {
        Debug.Log("Clicked on: " + obj.name); // 클릭된 오브젝트 이름 출력
    }
}
