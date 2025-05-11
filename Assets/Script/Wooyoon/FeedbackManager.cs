using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

public class FeedbackManager : MonoBehaviour
{
    [Header("Upstage API Key")]
    public string upstageApiKey = "up_Dey90uXVx9bsfQ8TCZe5cyaYEbHsN";

    [Header("폴더 및 저장 설정")]
    public string feedbackDirectory = "ResultText/Feedback";
    private string feedbackFilePath;

    [Header("UI 카드 배열 (Text 컴포넌트)")]
    public Text[] feedbackCards;

    [Header("피드백 전체 패널 (비활성 → 완료 시 활성화)")]
    public GameObject feedbackPanel;

    private int feedbackIndex = 0;

    // 초기화: 파일 경로 설정 및 패널 비활성화
    void Start()
    {
        InitializeFile();

        if (feedbackPanel != null)
            feedbackPanel.SetActive(false);
    }

    // 피드백 파일 초기화 및 고유한 파일명 설정
    private void InitializeFile()
    {
        string directory = Path.Combine(Application.dataPath, feedbackDirectory);
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        string baseFileName = "InterviewFeedback.txt";
        feedbackFilePath = GetUniqueFilePath(Path.Combine(directory, baseFileName));

        Debug.Log($"피드백 저장 경로: {feedbackFilePath}");
    }

    // 동일한 이름의 파일이 있을 경우 (1), (2)... 식으로 이름을 변경하여 고유한 파일 경로 생성
    private string GetUniqueFilePath(string basePath)
    {
        int count = 1;
        string directory = Path.GetDirectoryName(basePath);
        string filename = Path.GetFileNameWithoutExtension(basePath);
        string extension = Path.GetExtension(basePath);
        string newPath = basePath;

        while (File.Exists(newPath))
        {
            newPath = Path.Combine(directory, $"{filename}({count}){extension}");
            count++;
        }

        return newPath;
    }

    // 질문과 답변을 Upstage API로 전송하고, 응답을 받아 피드백 저장 및 UI에 출력
    public IEnumerator SendToUpstage(string question, string answer)
    {
        string prompt = $"{question}\n- 현재 답변: \"{answer}\"\n면접관처럼 피드백을 주세요.";

        string json = $@"
        {{
            ""model"": ""solar-pro"",
            ""messages"": [
                {{""role"": ""system"", ""content"": ""당신은 지원자의 답변을 평가하는 면접관입니다."" }},
                {{""role"": ""user"", ""content"": ""{EscapeJson(prompt)}"" }}
            ],
            ""stream"": false
        }}";

        using (UnityWebRequest request = new UnityWebRequest("https://api.upstage.ai/v1/chat/completions", "POST"))
        {
            // API 요청 설정
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {upstageApiKey}");

            yield return request.SendWebRequest();

            // 응답 성공 시 피드백 파일 저장 및 UI 카드 업데이트
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                string feedback = ExtractFeedbackFromJson(response);

                string formatted = $"{question}\n- 현재 답변: \"{answer}\"\n{feedback}\n\n";

                File.AppendAllText(feedbackFilePath, formatted);

                if (feedbackIndex < feedbackCards.Length)
                {
                    feedbackCards[feedbackIndex].text = formatted;
                    feedbackIndex++;

                    // 모든 피드백 카드가 채워지면 패널 표시
                    if (feedbackIndex >= feedbackCards.Length && feedbackPanel != null)
                        feedbackPanel.SetActive(true);
                }

                Debug.Log("Upstage 피드백 저장 완료!");
            }
            else
            {
                Debug.LogError("Upstage 피드백 요청 실패: " + request.error);
                Debug.LogError("응답 본문: " + request.downloadHandler.text);
            }
        }
    }

    // JSON 응답에서 "content" 값 추출
    private string ExtractFeedbackFromJson(string json)
    {
        Match match = Regex.Match(json, "\"content\"\\s*:\\s*\"(.*?)\"", RegexOptions.Singleline);
        if (match.Success)
        {
            string content = match.Groups[1].Value;
            return content.Replace("\\n", "\n").Replace("\\\"", "\"");
        }
        return "피드백 추출 실패";
    }

    // 문자열을 JSON에서 안전하게 사용할 수 있도록 이스케이프 처리
    private string EscapeJson(string str)
    {
        return str.Replace("\\", "\\\\")
                  .Replace("\"", "\\\"")
                  .Replace("\n", "\\n")
                  .Replace("\r", "");
    }
}
