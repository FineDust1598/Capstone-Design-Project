using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

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

        [SerializeField] private TextMeshProUGUI m_StepButtonTextField;
        [SerializeField] private List<Step> m_StepList = new List<Step>();

        public string customDirectoryPath = "ResultText/ModalText"; // public으로 경로 공개

        private int m_CurrentStepIndex = 0;
        private string filePath;

        private void Start()
        {
            InitializeFile();
        }

        private void InitializeFile()
        {
            // 지정한 경로에 저장
            string directoryPath = Path.Combine(Application.dataPath, customDirectoryPath);
            filePath = Path.Combine(directoryPath, "ModalText.txt");

            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    Debug.Log($"폴더 생성: {directoryPath}");
                }

                // 파일 이름 중복 시 새로운 파일 생성
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

            // 현재 카드 비활성화
            if (m_StepList[m_CurrentStepIndex].stepObject != null)
            {
                Debug.Log($"현재 카드 비활성화: {m_StepList[m_CurrentStepIndex].stepObject.name}");
                m_StepList[m_CurrentStepIndex].stepObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("현재 카드 stepObject가 null입니다.");
            }

            // 다음 카드 활성화
            m_CurrentStepIndex = (m_CurrentStepIndex + 1) % m_StepList.Count;
            if (m_StepList[m_CurrentStepIndex].stepObject != null)
            {
                Debug.Log($"다음 카드 활성화: {m_StepList[m_CurrentStepIndex].stepObject.name}");
                m_StepList[m_CurrentStepIndex].stepObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("다음 카드 stepObject가 null입니다.");
            }

            // 버튼 텍스트 업데이트
            m_StepButtonTextField.text = m_StepList[m_CurrentStepIndex].buttonText;
            Debug.Log($"버튼 텍스트 변경: {m_StepButtonTextField.text}");
        }

        private void SaveModalTextToFile()
        {
            GameObject currentCard = m_StepList[m_CurrentStepIndex].stepObject;
            if (currentCard == null)
            {
                Debug.LogWarning("현재 활성화된 카드가 없습니다.");
                return;
            }

            TextMeshProUGUI modalText = currentCard.GetComponentInChildren<TextMeshProUGUI>();
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