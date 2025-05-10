using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using System.IO;
using System;

public class TTSManager : MonoBehaviour
{
    public AudioSource audioSource;        // 음성을 재생할 AudioSource
    public Text subtitleText;              // 화면에 표시될 자막 텍스트
    public Button continueButton;          // [PAUSE] 이후 계속 진행할 버튼

    private string apiKey;                 // Google Cloud TTS API 키
    private string tempPath;              // 임시 오디오 파일 경로
    private bool isContinuePressed = false; // 사용자가 continue 버튼을 눌렀는지 여부

    void Start()
    {
        // API 키 파일 로드
        apiKey = "AIzaSyAZ65K3j8HyYnAZ-M6QcoLmZp2ll75CGTU";

        // 임시 파일 경로 설정 및 기존 파일 삭제
        tempPath = Path.Combine(Application.persistentDataPath, "tts_output.mp3");
        DeleteTempFile();

        // 텍스트 파일 읽고 한 줄씩 음성 변환 재생 시작
        string textFilePath = Path.Combine(Application.dataPath, "ResultText/Question/input.txt");
        if (File.Exists(textFilePath))
        {
            StartCoroutine(PlayTextFileLineByLine(textFilePath));
        }
        else
        {
            Debug.LogError("TTS 텍스트 파일이 없습니다: " + textFilePath);
        }

        // 버튼 이벤트 연결 및 초기 비활성화
        continueButton.onClick.AddListener(OnContinuePressed);
        continueButton.gameObject.SetActive(false);
    }

    // 텍스트 파일을 한 줄씩 읽고 음성으로 변환하여 재생
    private IEnumerator PlayTextFileLineByLine(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        foreach (string line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                // [PAUSE] 명령이 포함된 경우, 사용자 입력 대기
                if (line.Contains("[PAUSE]"))
                {
                    subtitleText.text = "[일시 정지 중 - 버튼 클릭 시 재생]";
                    isContinuePressed = false;
                    continueButton.gameObject.SetActive(true);

                    while (!isContinuePressed)
                    {
                        yield return null;
                    }

                    continueButton.gameObject.SetActive(false);
                    subtitleText.text = "";
                    continue;
                }

                // 자막 표시 및 음성 합성 실행
                subtitleText.text = line;
                yield return StartCoroutine(SynthesizeSpeech(line));

                // 음성 재생이 끝날 때까지 대기
                while (audioSource.isPlaying)
                {
                    yield return null;
                }

                // 다음 문장 전 잠깐 대기
                yield return new WaitForSeconds(0.5f);
            }
        }

        // 자막 초기화
        subtitleText.text = "";
    }

    // 사용자 continue 버튼 클릭 처리
    private void OnContinuePressed()
    {
        isContinuePressed = true;
    }

    // 외부에서 텍스트를 직접 재생 요청하는 경우 호출
    public void SynthesizeAndPlay(string text)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("TTS API 키가 설정되지 않았습니다.");
            return;
        }

        subtitleText.text = text;
        StartCoroutine(SynthesizeSpeech(text));
    }

    // 텍스트를 음성으로 변환하고 임시 파일에 저장 후 재생
    private IEnumerator SynthesizeSpeech(string text)
    {
        string url = "https://texttospeech.googleapis.com/v1/text:synthesize?key=" + apiKey;

        // 요청 바디 구성
        TTSRequest requestBody = new TTSRequest
        {
            input = new Input { text = text },
            voice = new Voice { languageCode = "ko-KR", name = "ko-KR-Standard-A" },
            audioConfig = new AudioConfig { audioEncoding = "MP3" }
        };

        string jsonData = JsonUtility.ToJson(requestBody);
        Debug.Log("보내는 JSON: " + jsonData);

        // Google TTS API 요청
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                // 응답 받은 음성 데이터를 파일로 저장
                string jsonResponse = www.downloadHandler.text;
                var response = JsonUtility.FromJson<TTSResponse>(jsonResponse);

                byte[] audioData = Convert.FromBase64String(response.audioContent);
                File.WriteAllBytes(tempPath, audioData);

                // 저장된 오디오 파일 재생
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
                Debug.LogError("응답 내용: " + www.downloadHandler.text);
            }
        }
    }

    // 기존 임시 오디오 파일 삭제
    private void DeleteTempFile()
    {
        if (File.Exists(tempPath))
        {
            File.Delete(tempPath);
            Debug.Log("기존 TTS 오디오 파일 삭제 완료.");
        }
    }

    // JSON 직렬화를 위한 내부 클래스 정의
    [Serializable] private class TTSResponse { public string audioContent; }
    [Serializable] private class TTSRequest { public Input input; public Voice voice; public AudioConfig audioConfig; }
    [Serializable] private class Input { public string text; }
    [Serializable] private class Voice { public string languageCode; public string name; }
    [Serializable] private class AudioConfig { public string audioEncoding; }
}
