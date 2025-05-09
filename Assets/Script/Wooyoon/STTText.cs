using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.IO;
using System;

public class STTText : MonoBehaviour
{
    public AudioSource audioSource;

    private string apiKey;  // AIzaSyAZ65K3j8HyYnAZ-M6QcoLmZp2ll75CGTU
    private string tempPath;

    void Start()
    {
        // 환경 변수에서 API 키 로드
        apiKey = Environment.GetEnvironmentVariable("GOOGLE_TTS_API_KEY");

        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("Google TTS API 키가 환경 변수에서 설정되지 않았습니다.");
            return;
        }

        // 임시 파일 경로 지정
        tempPath = Path.Combine(Application.persistentDataPath, "tts_output.mp3");

        // 게임 시작 시 임시 파일 삭제
        DeleteTempFile();
    }

    public void SynthesizeAndPlay(string text)
    {
        StartCoroutine(SynthesizeSpeech(text));
    }

    private IEnumerator SynthesizeSpeech(string text)
    {
        string url = "https://texttospeech.googleapis.com/v1/text:synthesize?key=" + apiKey;

        var requestBody = new
        {
            input = new { text = text },
            voice = new { languageCode = "ko-KR", name = "ko-KR-Standard-A" },
            audioConfig = new { audioEncoding = "MP3" }
        };

        string jsonData = JsonUtility.ToJson(requestBody);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = www.downloadHandler.text;
                var response = JsonUtility.FromJson<TTSResponse>(jsonResponse);

                byte[] audioData = Convert.FromBase64String(response.audioContent);
                File.WriteAllBytes(tempPath, audioData);

                using (UnityWebRequest audioRequest = UnityWebRequestMultimedia.GetAudioClip("file://" + tempPath, AudioType.MPEG))
                {
                    yield return audioRequest.SendWebRequest();

                    if (audioRequest.result == UnityWebRequest.Result.Success)
                    {
                        AudioClip clip = DownloadHandlerAudioClip.GetContent(audioRequest);
                        audioSource.clip = clip;
                        audioSource.Play();
                    }
                    else
                    {
                        Debug.LogError("오디오 로딩 실패: " + audioRequest.error);
                    }
                }
            }
            else
            {
                Debug.LogError("TTS 요청 실패: " + www.error);
            }
        }
    }

    private void DeleteTempFile()
    {
        if (File.Exists(tempPath))
        {
            File.Delete(tempPath);
            Debug.Log("기존 TTS 오디오 파일 삭제 완료.");
        }
    }

    [Serializable]
    private class TTSResponse
    {
        public string audioContent;
    }
}
