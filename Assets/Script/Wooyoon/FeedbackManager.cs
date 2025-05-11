using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class FeedbackManager : MonoBehaviour
{
    public string upstageApiKey = "up_Dey90uXVx9bsfQ8TCZe5cyaYEbHsN";
    public string feedbackDirectory = "ResultText/Feedback";
    private string feedbackFilePath;

    void Start()
    {
        string directory = Path.Combine(Application.dataPath, feedbackDirectory);
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        feedbackFilePath = Path.Combine(directory, "InterviewFeedback.txt");
    }

    public IEnumerator SendToUpstage(string question, string answer)
    {
        string prompt = $"{question}\n- 현재 답변: \"{answer}\"\n면접관처럼 피드백을 주고, 개선 예시도 제시해 주세요.";

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
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {upstageApiKey}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                string feedback = ExtractFeedbackFromJson(response);

                string formatted = $"{question}\n- 현재 답변: \"{answer}\"\n{feedback}\n\n";
                File.AppendAllText(feedbackFilePath, formatted);
                Debug.Log("Upstage 피드백 저장 완료!");
            }
            else
            {
                Debug.LogError("Upstage 피드백 요청 실패: " + request.error);
                Debug.LogError("응답 본문: " + request.downloadHandler.text);
            }
        }
    }

    // JSON 응답에서 content 추출 (stream=false 응답 구조)
    private string ExtractFeedbackFromJson(string json)
    {
        const string marker = "\"content\":\"";
        int start = json.IndexOf(marker);
        if (start == -1) return "피드백 추출 실패";

        start += marker.Length;
        int end = json.IndexOf("\"", start);
        if (end == -1) return "피드백 추출 실패";

        string content = json.Substring(start, end - start);
        return content.Replace("\\n", "\n").Replace("\\\"", "\"");
    }

    private string EscapeJson(string str)
    {
        return str.Replace("\\", "\\\\")
                  .Replace("\"", "\\\"")
                  .Replace("\n", "\\n")
                  .Replace("\r", "");
    }
}
