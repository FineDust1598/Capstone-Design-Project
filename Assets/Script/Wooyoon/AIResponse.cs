using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class AIResponse : MonoBehaviour
{
    public TextMeshProUGUI aiSubtitleText;
    public AudioSource ttsAudioSource;
    // public Animator npcAnimator;

    public IEnumerator RespondToUserCoroutine(string userInput)
    {
        // Step 1: GPT API 등으로 AI 응답 생성 (여기선 예시 텍스트)
        string aiResponse = $"좋은 질문입니다. 이에 대한 제 생각은...";

        // Step 2: 자막 출력
        aiSubtitleText.text = aiResponse;

        // Step 3: TTS 출력 (구글 TTS 또는 로컬 클립)
        yield return StartCoroutine(PlayTTS(aiResponse));

        // Step 4: 제스처 트리거
        // npcAnimator.SetTrigger("Talk");
    }

    private IEnumerator PlayTTS(string text)
    {
        string ttsUrl = $"https://api.fake-tts.com/speak?text={UnityWebRequest.EscapeURL(text)}"; // 예시
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(ttsUrl, AudioType.MPEG);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            ttsAudioSource.clip = clip;
            ttsAudioSource.Play();
        }
        else
        {
            Debug.LogError("TTS 실패: " + www.error);
        }
    }
}
