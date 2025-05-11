using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class STTManager : MonoBehaviour
{
    public Text resultText;
    public Toggle toggle;
    public string customDirectoryPath = "ResultText/STT";
    public string questionPath = "ResultText/Question/input.txt";

    private string apiKey = "AIzaSyAv6ryH4XWPVNwItU_CRiSs-EruDNeA7tY"; // 구글 STT API 키
    private AudioClip recordedClip;
    private bool isRecording = false;
    private string filePath;

    private List<string> questions = new List<string>();
    private int questionIndex = 0;

    private FeedbackManager feedbackManager;

    void Start()
    {
        resultText.text = "Change Toggle to speak~";

        if (toggle == null)
        {
            Debug.LogError("Toggle이 연결되지 않았습니다! Inspector에서 할당하세요.");
            return;
        }
        toggle.onValueChanged.AddListener(OnToggleChanged);

        feedbackManager = FindObjectOfType<FeedbackManager>();
        if (feedbackManager == null)
        {
            Debug.LogError("FeedbackManager를 찾을 수 없습니다. 씬에 추가되어 있는지 확인하세요.");
        }

        InitializeFile();
        LoadQuestions();
    }

    private void InitializeFile()
    {
        string directoryPath = Path.Combine(Application.dataPath, customDirectoryPath);
        filePath = Path.Combine(directoryPath, "STT Text.txt");

        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Debug.Log($"폴더 생성: {directoryPath}");
            }

            filePath = GetUniqueFilePath(filePath);
            Debug.Log($"STT 파일 경로: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"파일 초기화 실패: {e.Message}");
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

    private void LoadQuestions()
    {
        string fullPath = Path.Combine(Application.dataPath, questionPath);

        if (!File.Exists(fullPath))
        {
            Debug.LogError("질문 파일을 찾을 수 없습니다: " + fullPath);
            return;
        }

        string[] lines = File.ReadAllLines(fullPath);
        if (lines.Length <= 2)
        {
            Debug.LogError("질문 파일에 충분한 줄이 없습니다.");
            return;
        }

        List<string> trimmedLines = new List<string>(lines);
        trimmedLines.RemoveAt(0); // 첫 줄
        trimmedLines.RemoveAt(trimmedLines.Count - 1); // 마지막 줄

        foreach (var line in trimmedLines)
        {
            if (!string.IsNullOrWhiteSpace(line) && !line.Contains("[PAUSE]"))
            {
                questions.Add("Q. " + line.Trim());
            }
        }

        Debug.Log("질문 로드 완료: " + questions.Count + "개");
    }

    void StartRecording()
    {
        if (!isRecording)
        {
            isRecording = true;
            recordedClip = Microphone.Start(null, false, 5, 16000);
            resultText.text = "녹음 중...";
            Debug.Log("녹음 시작!");
        }
    }

    void StopRecording()
    {
        if (isRecording)
        {
            isRecording = false;
            Microphone.End(null);
            resultText.text = "녹음 종료! 변환 중...";
            Debug.Log("녹음 종료! 변환 중...");

            byte[] audioData = ConvertAudioClipToWAV(recordedClip);
            StartCoroutine(SendAudioToGoogle(audioData));
        }
    }

    byte[] ConvertAudioClipToWAV(AudioClip clip)
    {
        MemoryStream stream = new MemoryStream();
        int length = clip.samples * clip.channels;
        float[] data = new float[length];
        clip.GetData(data, 0);
        short[] intData = new short[length];

        for (int i = 0; i < length; i++)
        {
            intData[i] = (short)(data[i] * 32767);
        }

        byte[] bytes = new byte[length * 2];
        Buffer.BlockCopy(intData, 0, bytes, 0, bytes.Length);
        return bytes;
    }

    private IEnumerator SendAudioToGoogle(byte[] audioData)
    {
        string url = $"https://speech.googleapis.com/v1/speech:recognize?key={apiKey}";
        string base64Audio = Convert.ToBase64String(audioData);
        string json = "{\"config\":{\"encoding\":\"LINEAR16\",\"sampleRateHertz\":16000,\"languageCode\":\"ko-KR\"},\"audio\":{\"content\":\"" + base64Audio + "\"}}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("STT 성공! 응답: " + responseText);

                string transcript = ParseGoogleSTTResponse(responseText);
                resultText.text = "변환 결과: " + transcript;

                SaveSTTTextToFile(transcript);
            }
            else
            {
                Debug.LogError("STT 실패: " + request.error);
                resultText.text = "변환 실패!";
            }
        }
    }

    private string ParseGoogleSTTResponse(string jsonResponse)
    {
        var json = JsonUtility.FromJson<GoogleSTTResponse>(jsonResponse);
        if (json != null && json.results.Length > 0)
        {
            return json.results[0].alternatives[0].transcript;
        }
        return "음성 인식 실패!";
    }

    private void SaveSTTTextToFile(string sttText)
    {
        if (string.IsNullOrWhiteSpace(sttText))
        {
            Debug.LogWarning("저장할 STT 텍스트가 비어 있습니다.");
            return;
        }

        string questionLine = (questionIndex < questions.Count) ? questions[questionIndex] : "Q. (질문 없음)";
        questionIndex++;

        string formattedText = $"{questionLine}\n면접자 : {sttText}\n\n";
        Debug.Log($"저장할 STT 텍스트: {formattedText}");

        try
        {
            File.AppendAllText(filePath, formattedText);
            Debug.Log($"STT 텍스트 저장 완료: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"STT 텍스트 저장 실패: {e.Message}");
        }

        if (feedbackManager != null)
        {
            Debug.Log($"[피드백 요청] 질문: {questionLine}\n답변: {sttText}");
            StartCoroutine(feedbackManager.SendToUpstage(questionLine, sttText));
        }
    }

    void OnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            StartRecording();
            Debug.Log("녹음 시작 (Toggle: On)");
        }
        else
        {
            StopRecording();
            Debug.Log("녹음 종료 (Toggle: Off)");
        }
    }

    [Serializable]
    private class GoogleSTTResponse
    {
        public Result[] results;
    }

    [Serializable]
    private class Result
    {
        public Alternative[] alternatives;
    }

    [Serializable]
    private class Alternative
    {
        public string transcript;
    }
}
