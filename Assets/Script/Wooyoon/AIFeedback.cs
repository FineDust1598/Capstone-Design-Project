using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Text;

public class AIFeedback : MonoBehaviour
{
    public string modelAnswerFilePath = "Assets/ResultText/AnswerSheet/ModelAnswer.txt"; // 모범 답안 파일 경로
    public string resultDirectoryPath = "Assets/ResultText/ResultFeedback"; // 결과 저장 폴더
    private string apiKeyPath = "Assets/Script/wooyoon/API/gpt_api.txt"; // API 키 파일 경로
    private string apiKey;

    void Start()
    {
        Debug.Log("AIFeedback 스크립트 시작됨");

        apiKey = LoadApiKey();
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("API 키를 불러올 수 없습니다.");
        }

        string modelAnswer = LoadModelAnswer();
        if (string.IsNullOrEmpty(modelAnswer))
        {
            Debug.LogError("모범 답안을 불러올 수 없습니다.");
        }
    }

    public void AnalyzeAnswer(string userAnswer)
    {
        Debug.Log("AnalyzeAnswer 호출됨, 사용자 답변: " + userAnswer);
        string modelAnswer = LoadModelAnswer();
        if (string.IsNullOrEmpty(modelAnswer))
        {
            Debug.LogError("모범 답안을 불러올 수 없습니다.");
            return;
        }
        StartCoroutine(SendToChatGPT(userAnswer, modelAnswer));
    }

    private string LoadModelAnswer()
    {
        string path = Path.Combine(Application.dataPath, modelAnswerFilePath.Substring("Assets/".Length));
        Debug.Log("모범 답안 파일 예상 경로: " + path);

        if (File.Exists(path))
        {
            string content = File.ReadAllText(path, Encoding.UTF8);
            Debug.Log("모범 답안 로드 완료: " + content);
            return RemoveSpeakerPrefix(content);
        }

        Debug.LogError("모범 답안 파일이 존재하지 않습니다: " + path);
        return null;
    }

    private string LoadApiKey()
    {
        string path = Path.Combine(Application.dataPath, apiKeyPath.Substring("Assets/".Length));
        Debug.Log("API 키 파일 예상 경로: " + path);

        if (File.Exists(path))
        {
            return File.ReadAllText(path).Trim();
        }

        Debug.LogError("API 키 파일이 존재하지 않습니다: " + path);
        return null;
    }

    private string RemoveSpeakerPrefix(string text)
    {
        string[] lines = text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].Replace("면접자 : ", "").Trim();
        }
        return string.Join("\n", lines);
    }

    private IEnumerator SendToChatGPT(string userAnswer, string modelAnswer)
    {
        Debug.Log("SendToChatGPT 호출됨");

        string prompt = $"사용자 답변: {userAnswer}\n모범 답안: {modelAnswer}\n\n이 두 개를 비교하여 유사도를 %로 계산하고, 개선점을 포함한 피드백을 제공해줘.";
        string json = "{\"model\": \"gpt-4\", \"messages\": [{\"role\": \"system\", \"content\": \"You are an interview evaluator.\"}, {\"role\": \"user\", \"content\": \"" + prompt + "\"}]}";

        using (UnityWebRequest request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("ChatGPT 응답 수신 완료: " + responseText);

                string feedback = ParseChatGPTResponse(responseText);
                Debug.Log("ChatGPT 피드백: " + feedback);

                SaveFeedbackToFile(feedback); // 파일 저장
            }
            else
            {
                Debug.LogError("ChatGPT 요청 실패: " + request.error);
            }
        }
    }

    private string ParseChatGPTResponse(string jsonResponse)
    {
        var response = JsonUtility.FromJson<ChatGPTResponse>(jsonResponse);
        if (response != null && response.choices.Length > 0)
        {
            return response.choices[0].message.content;
        }
        return "피드백을 가져올 수 없습니다.";
    }

    private void SaveFeedbackToFile(string feedback)
    {
        Debug.Log("SaveFeedbackToFile 호출됨");

        string directoryPath = Path.Combine(Application.dataPath, resultDirectoryPath.Substring("Assets/".Length));
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            Debug.Log("ResultFeedback 폴더 생성됨: " + directoryPath);
        }

        string filePath = Path.Combine(directoryPath, "ResultFeedback.txt");
        filePath = GetUniqueFilePath(filePath);

        try
        {
            File.WriteAllText(filePath, feedback);
            Debug.Log("피드백 저장 완료: " + filePath);
            Debug.Log("저장된 피드백 내용: " + feedback);
        }
        catch (Exception e)
        {
            Debug.LogError("피드백 저장 실패: " + e.Message);
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

        Debug.Log("생성된 파일 경로: " + filePath);
        return filePath;
    }

    [Serializable]
    private class ChatGPTResponse
    {
        public Choice[] choices;
    }

    [Serializable]
    private class Choice
    {
        public Message message;
    }

    [Serializable]
    private class Message
    {
        public string content;
    }
}
