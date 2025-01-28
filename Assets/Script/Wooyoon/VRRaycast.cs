using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRRaycast : MonoBehaviour
{
    public LayerMask interactableLayer;
    private GameObject lastHitObject;

    public delegate void RaycastHitEvent(GameObject hitObject);
    public event RaycastHitEvent OnRaycastHit;
    public event RaycastHitEvent OnRaycastExit;

    private RaycastHit hitInfo; // 레이 히트 정보를 저장
    private bool isHitting = false; // 현재 레이캐스트가 히트했는지 여부

    void Update()
    {
        PerformRaycast();
    }

    void PerformRaycast()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, interactableLayer))
        {
            if (lastHitObject != hit.transform.gameObject)
            {
                if (lastHitObject != null)
                {
                    OnRaycastExit?.Invoke(lastHitObject);
                }

                lastHitObject = hit.transform.gameObject;
                OnRaycastHit?.Invoke(lastHitObject);
            }

            hitInfo = hit; // 히트 정보 저장
            isHitting = true;
        }
        else
        {
            if (lastHitObject != null)
            {
                OnRaycastExit?.Invoke(lastHitObject);
                lastHitObject = null;
            }

            isHitting = false;
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return; // 플레이 중일 때만 실행

        Gizmos.color = isHitting ? Color.green : Color.red; // 히트하면 녹색, 안 하면 빨간색

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (isHitting)
        {
            Gizmos.DrawLine(ray.origin, hitInfo.point); // 충돌 지점까지 선을 그림
            Gizmos.DrawSphere(hitInfo.point, 0.05f); // 충돌 지점에 작은 구를 그림
        }
        else
        {
            Gizmos.DrawRay(ray.origin, ray.direction * 10); // 10m 길이의 빨간색 레이 표시
        }
    }
}
