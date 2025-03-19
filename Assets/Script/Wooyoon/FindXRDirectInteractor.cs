using UnityEngine;


public class FindXRInteractors : MonoBehaviour
{
    void Start()
    {
        UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor[] interactors = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>();

        if (interactors.Length == 0)
        {
            Debug.LogWarning("씬 내에서 동작하는 XR 인터랙터가 없습니다!");
        }
        else
        {
            foreach (var interactor in interactors)
            {
                Debug.Log("XR 인터랙션 컴포넌트: " + interactor.gameObject.name + " - " + interactor.GetType().Name);
            }
        }
    }
}
