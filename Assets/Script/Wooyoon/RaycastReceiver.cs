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
}
