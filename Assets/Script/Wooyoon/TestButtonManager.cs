using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;  // SceneManager 추가

namespace Unity.VRTemplate
{
    /// <summary>
    /// Controls the steps in the coaching card.
    /// </summary>
    public class TestButtonManager : MonoBehaviour
    {
        [SerializeField]
        public TextMeshProUGUI m_StepButtonTextField; // 버튼 텍스트 표시

        // 버튼 클릭 시 변경되는 단계에 맞는 텍스트들
        [SerializeField]
        private string[] m_StepTexts = new string[] { "Step 1", "Step 2", "Step 3" };

        private int m_CurrentStepIndex = 0;

        public void Next()
        {
            // 버튼 텍스트를 다음 단계로 변경
            m_CurrentStepIndex = (m_CurrentStepIndex + 1) % m_StepTexts.Length;
            m_StepButtonTextField.text = m_StepTexts[m_CurrentStepIndex];

            // 마지막 단계에서 특정 씬으로 이동하고 싶다면
            if (m_CurrentStepIndex == m_StepTexts.Length - 1)
            {
                LoadNextScene();
            }
        }

        // 씬을 전환하는 함수
        private void LoadNextScene()
        {
            // 원하는 씬 이름으로 교체
            SceneManager.LoadScene("NextSceneName");
        }
    }
}
