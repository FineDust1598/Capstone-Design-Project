using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class VRRaycast : MonoBehaviour
{
    public LayerMask interactableLayer;
    private GameObject lastHitObject;
    public float maxRayDistance = 10f; // 레이 길이 제한

    public delegate void RaycastHitEvent(GameObject hitObject);
    public event RaycastHitEvent OnRaycastHit;
    public event RaycastHitEvent OnRaycastExit;
    public event RaycastHitEvent OnRaycastClick;

    private RaycastHit hitInfo;
    private bool isHitting = false;
    private LineRenderer lineRenderer;
    private EventSystem eventSystem;
    private PointerEventData pointerEventData;

    void Start()
    {
        // LineRenderer 설정
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.positionCount = 2;

        // EventSystem 설정
        eventSystem = EventSystem.current;
        pointerEventData = new PointerEventData(eventSystem);
    }

    void Update()
    {
        PerformRaycast();

        // 클릭 이벤트 감지 후 실행
        if (Input.GetMouseButtonDown(0))
        {
            if (lastHitObject != null)
            {
                OnRaycastClick?.Invoke(lastHitObject);
            }
            else
            {
                TryClickUI(); // UI 클릭 감지
            }
        }
    }

    void PerformRaycast()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxRayDistance, interactableLayer))
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

            hitInfo = hit;
            isHitting = true;

            // LineRenderer로 Ray 시각화
            lineRenderer.SetPosition(0, ray.origin);
            lineRenderer.SetPosition(1, hit.point);
            lineRenderer.material.color = Color.green;
        }
        else
        {
            if (lastHitObject != null)
            {
                OnRaycastExit?.Invoke(lastHitObject);
                lastHitObject = null;
            }

            isHitting = false;

            // 충돌한 오브젝트가 없을 때 10m 길이의 레이
            lineRenderer.SetPosition(0, ray.origin);
            lineRenderer.SetPosition(1, ray.origin + ray.direction * maxRayDistance);
            lineRenderer.material.color = Color.red;
        }
    }

    void TryClickUI()
    {
        pointerEventData.position = new Vector2(Screen.width / 2, Screen.height / 2);
        List<RaycastResult> results = new List<RaycastResult>();
        eventSystem.RaycastAll(pointerEventData, results);

        if (results.Count > 0)
        {
            OnRaycastClick?.Invoke(results[0].gameObject);
            ExecuteEvents.Execute(results[0].gameObject, new PointerEventData(eventSystem), ExecuteEvents.pointerClickHandler);
        }
    }
}
