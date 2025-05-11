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

        // 토글 리스너 설정
        if (toggle == null)
        {
            Debug.LogError("Toggle이 연결되지 않았습니다! Inspector에서 할당하세요.");
            return;
        }
        toggle.onValueChanged.AddListener(OnToggleChanged);

        // FeedbackManager 연결
        feedbackManager = FindObjectOfType<FeedbackManager>();
        if (feedbackManager == null)
        {
            Debug.LogError("FeedbackManager를 찾을 수 없습니다.");
        }

        InitializeFile();    // 파일 저장 경로 초기화
        LoadQuestions();     // 질문 파일 로드
    }

    // STT 텍스트 저장할 폴더 및 고유 파일 경로 설정
    private void InitializeFile()
    {
        string directoryPath = Path.Combine(Application.dataPath, customDirectoryPath);
        filePath = Path.Combine(directoryPath, "STT Text.txt");

        try
        {
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            filePath = GetUniqueFilePath(filePath);
        }
        catch (Exception e)
        {
            Debug.LogError($"파일 초기화 실패: {e.Message}");
        }
    }

    // 동일한 이름의 파일이 존재하면 번호 붙여 고유 경로 생성
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

    // 질문 텍스트 파일을 읽어 리스트에 저장
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

        // 첫/마지막 줄 제거, 공백 및 [PAUSE] 제외
        List<string> trimmedLines = new List<string>(lines);
        trimmedLines.RemoveAt(0);
        trimmedLines.RemoveAt(trimmedLines.Count - 1);

        foreach (var line in trimmedLines)
        {
            if (!string.IsNullOrWhiteSpace(line) && !line.Contains("[PAUSE]"))
                questions.Add("Q. " + line.Trim());
        }
    }

    // 음성 녹음 시작
    void StartRecording()
    {
        if (!isRecording)
        {
            isRecording = true;
            recordedClip = Microphone.Start(null, false, 5, 16000);
            resultText.text = "녹음 중...";
        }
    }

    // 음성 녹음 종료 및 구글 STT 전송
    void StopRecording()
    {
        if (isRecording)
        {
            isRecording = false;
            Microphone.End(null);
            resultText.text = "녹음 종료! 변환 중...";

            byte[] audioData = ConvertAudioClipToWAV(recordedClip);
            StartCoroutine(SendAudioToGoogle(audioData));
        }
    }

    // AudioClip을 WAV 포맷의 byte 배열로 변환
    byte[] ConvertAudioClipToWAV(AudioClip clip)
    {
        MemoryStream stream = new MemoryStream();
        int length = clip.samples * clip.channels;
        float[] data = new float[length];
        clip.GetData(data, 0);
        short[] intData = new short[length];

        for (int i = 0; i < length; i++)
            intData[i] = (short)(data[i] * 32767);

        byte[] bytes = new byte[length * 2];
        Buffer.BlockCopy(intData, 0, bytes, 0, bytes.Length);
        return bytes;
    }

    // 구글 STT API에 오디오 전송 및 결과 수신
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

                SaveSTTTextToFile(transcript); // 저장 및 피드백 요청
            }
            else
            {
                Debug.LogError("STT 실패: " + request.error);
                resultText.text = "변환 실패!";
            }
        }
    }

    // JSON 응답에서 transcript 추출
    private string ParseGoogleSTTResponse(string jsonResponse)
    {
        var json = JsonUtility.FromJson<GoogleSTTResponse>(jsonResponse);
        if (json != null && json.results.Length > 0)
            return json.results[0].alternatives[0].transcript;

        return "음성 인식 실패!";
    }

    // STT 결과를 파일로 저장하고 FeedbackManager에 전달
    private void SaveSTTTextToFile(string sttText)
    {
        if (string.IsNullOrWhiteSpace(sttText)) return;

        string questionLine = (questionIndex < questions.Count) ? questions[questionIndex] : "Q. (질문 없음)";
        questionIndex++;

        string formattedText = $"{questionLine}\n면접자 : {sttText}\n\n";

        try
        {
            File.AppendAllText(filePath, formattedText);
        }
        catch (Exception e)
        {
            Debug.LogError($"STT 텍스트 저장 실패: {e.Message}");
        }

        // 피드백 요청
        if (feedbackManager != null)
        {
            StartCoroutine(feedbackManager.SendToUpstage(questionLine, sttText));
        }
    }

    // 토글 온/오프 시 녹음 시작/종료
    void OnToggleChanged(bool isOn)
    {
        if (isOn)
            StartRecording();
        else
            StopRecording();
    }

    // 구글 STT 응답 파싱용 클래스
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
