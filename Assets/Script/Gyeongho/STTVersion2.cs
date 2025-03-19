using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;
using System;
using TMPro;
using System.Xml.Serialization;

public class STTVersion2 : MonoBehaviour
{
    public TextMeshProUGUI resultText; //#. Unity UI의 TextMeshPro 오브젝트 연결
    private string apiKey = "AIzaSyAv6ryH4XWPVNwItU_CRiSs-EruDNeA7tY"; // Google API 키 입력
    private AudioClip recordedClip; //녹음된 오디오 저장
    private bool isRecording = false; // 녹음 상태 확인
    public Toggle toggle; // Inspector에서 연결할 Toggle UI

    void Start()
    {
        resultText.text = "Change Toggle to speak~";
        // toggle이 null인지 확인 (예외 방지)
        if (toggle == null)
        {
            Debug.LogError("Toggle이 연결되지 않았습니다! Inspector에서 할당하세요.");
            return;
        }

        // Toggle 값이 변경될 때 실행될 함수 등록
        toggle.onValueChanged.AddListener(OnToggleChanged);

        // 초기 상태 로그 출력
        Debug.Log($"초기 Toggle 상태: {toggle.isOn}");
    }
    //void Update()
    //{
    //    // 스페이스바를 누르면 녹음 시작 & 떼면 녹음 종료 후 STT 변환 요청
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        StartRecording();
    //    }
    //    if (Input.GetKeyUp(KeyCode.Space))
    //    {
    //        StopRecording();
    //    }
    //}

    //마이크 녹음 시작
    void StartRecording()
    {
        if (!isRecording)
        {
            isRecording = true;
            recordedClip = Microphone.Start(null, false, 5, 16000); // 5초 동안 16kHz로 녹음
            resultText.text = "녹음 중...";
            Debug.Log("녹음 시작!");
        }
    }

    // 🎤 마이크 녹음 종료 후 Google STT 요청
    void StopRecording()
    {
        if (isRecording)
        {
            isRecording = false;
            Microphone.End(null);
            resultText.text = "녹음 종료! 변환 중...";
            Debug.Log("녹음 종료! 변환 중...");

            // 오디오 데이터를 WAV 형식으로 변환 후 Google STT 요청
            byte[] audioData = ConvertAudioClipToWAV(recordedClip);
            StartCoroutine(SendAudioToGoogle(audioData));
        }
    }

    // WAV 파일 변환 함수
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

    //  Google STT API 요청
    private IEnumerator SendAudioToGoogle(byte[] audioData)
    {
        string url = $"https://speech.googleapis.com/v1/speech:recognize?key={apiKey}";
        string base64Audio = System.Convert.ToBase64String(audioData);
        string json = "{\"config\":{\"encoding\":\"LINEAR16\",\"sampleRateHertz\":16000,\"languageCode\":\"ko-KR\"},\"audio\":{\"content\":\"" + base64Audio + "\"}}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            resultText.text = " 변환 중...";
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log(" STT 성공! 응답: " + responseText);
                resultText.text = " 변환 결과: " + ParseGoogleSTTResponse(responseText);
            }
            else
            {
                Debug.LogError("STT 실패: " + request.error);
                resultText.text = "변환 실패!";
            }
        }
    }

    //  Google 응답에서 변환된 텍스트 추출
    private string ParseGoogleSTTResponse(string jsonResponse)
    {
        var json = JsonUtility.FromJson<GoogleSTTResponse>(jsonResponse);
        if (json != null && json.results.Length > 0)
        {
            return json.results[0].alternatives[0].transcript;
        }
        return " 음성 인식 실패!";
    }

    void OnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            StartRecording();
            Debug.Log("Toggle이 'True' 상태가 되었습니다.");
        }
        else
        {
            StopRecording();
            Debug.Log("Toggle이 'False' 상태가 되었습니다.");
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
