using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI; // UnityEngine.UI.Text 사용
using TMPro; // TextMeshPro 사용

namespace Unity.VRTemplate
{
    public class TestButtonManager : MonoBehaviour
    {
        [Serializable]
        class Step
        {
            [SerializeField] public GameObject stepObject;
            [SerializeField] public string buttonText;
        }

        [SerializeField] private TextMeshProUGUI m_StepButtonTextField; // TMP 사용
        [SerializeField] private List<Step> m_StepList = new List<Step>();

        public string customDirectoryPath = "ResultText/ModalText";

        private int m_CurrentStepIndex = 0;
        private string filePath;

        private void Start()
        {
            InitializeFile();
        }

        private void InitializeFile()
        {
            string directoryPath = Path.Combine(Application.dataPath, customDirectoryPath);
            filePath = Path.Combine(directoryPath, "ModalText.txt");

            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    Debug.Log($"폴더 생성: {directoryPath}");
                }

                filePath = GetUniqueFilePath(filePath);
                File.WriteAllText(filePath, "--- Modal Text 기록 시작 ---\n\n");
                Debug.Log($"새 파일 생성 및 초기화 완료: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"파일 초기화 실패: {e.Message}");
            }
        }

        public void Next()
        {
            Debug.Log("Next() 버튼이 눌렸습니다.");

            if (m_StepList.Count == 0)
            {
                Debug.LogWarning("m_StepList가 비어있습니다. 카드가 없습니다.");
                return;
            }

            SaveModalTextToFile();

            if (m_StepList[m_CurrentStepIndex].stepObject != null)
            {
                Debug.Log($"현재 카드 비활성화: {m_StepList[m_CurrentStepIndex].stepObject.name}");
                m_StepList[m_CurrentStepIndex].stepObject.SetActive(false);
            }

            m_CurrentStepIndex = (m_CurrentStepIndex + 1) % m_StepList.Count;

            if (m_StepList[m_CurrentStepIndex].stepObject != null)
            {
                Debug.Log($"다음 카드 활성화: {m_StepList[m_CurrentStepIndex].stepObject.name}");
                m_StepList[m_CurrentStepIndex].stepObject.SetActive(true);
            }

            if (m_StepButtonTextField != null)
            {
                m_StepButtonTextField.text = m_StepList[m_CurrentStepIndex].buttonText;
                Debug.Log($"버튼 텍스트 변경: {m_StepButtonTextField.text}");
            }
            else
            {
                Debug.LogWarning("버튼 텍스트 필드가 할당되지 않았습니다.");
            }
        }

        private void SaveModalTextToFile()
        {
            GameObject currentCard = m_StepList[m_CurrentStepIndex].stepObject;
            if (currentCard == null)
            {
                Debug.LogWarning("현재 활성화된 카드가 없습니다.");
                return;
            }

            Text modalText = currentCard.GetComponentInChildren<Text>(); // UnityEngine.UI.Text 사용
            if (modalText == null)
            {
                Debug.LogWarning($"Modal Text가 {currentCard.name} 안에서 발견되지 않았습니다.");
                return;
            }

            string textContent = modalText.text;
            Debug.Log($"Modal Text 내용: {textContent}");

            try
            {
                File.AppendAllText(filePath, textContent + "\n\n");
                Debug.Log($"Modal Text 저장 완료: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Modal Text 저장 실패: {e.Message}");
            }
        }

        private string GetUniqueFilePath(string filePath)
        {
            int count = 1;
            string directory = Path.GetDirectoryName(filePath);
            string filename = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);

            while (File.Exists(filePath))
            {
                filePath = Path.Combine(directory, $"{filename}({count}){extension}");
                count++;
            }
            return filePath;
        }
    }
}
