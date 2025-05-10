using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class STTText : MonoBehaviour
{
    public TextMeshProUGUI resultText; // Unity UI의 TextMeshPro 오브젝트 연결
    public Toggle toggle; // Toggle UI 연결
    public string customDirectoryPath = "ResultText/STT"; // 저장 경로 (Public으로 변경)

    private string apiKey = "AIzaSyAv6ryH4XWPVNwItU_CRiSs-EruDNeA7tY"; // Google API 키
    private AudioClip recordedClip; // 녹음된 오디오 저장
    private bool isRecording = false; // 녹음 상태 확인
    private string filePath; // 저장 경로

    void Start()
    {
        resultText.text = "Change Toggle to speak~";

        if (toggle == null)
        {
            Debug.LogError("Toggle이 연결되지 않았습니다! Inspector에서 할당하세요.");
            return;
        }
        toggle.onValueChanged.AddListener(OnToggleChanged);
        InitializeFile();
    }

    // STT 텍스트 저장을 위한 초기화
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

            // 중복 방지: 새로운 파일명 생성
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

    // 녹음 시작
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

    // 녹음 종료 및 STT 변환
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

    // AudioClip을 WAV로 변환
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

    // Google STT API로 전송
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

                // STT 결과 파일 저장
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

        string formattedText = $"면접자 : {sttText}\n\n";
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