using UnityEngine;
using TMPro;
using System.Collections;

public class InterviewManager : MonoBehaviour
{
    public STTText sttSystem;            // STT 처리 객체 (수정 없이 그대로 유지)
    public AIResponse aiResponse;        // AI 응답 및 자막/제스처 처리 객체
    public FeedbackUI feedbackUI;        // 피드백 UI
    public string[] questions;           // 질문 리스트
    public TextMeshProUGUI questionText; // 질문을 UI로 표시할 텍스트 컴포넌트

    private int currentQuestionIndex = 0;
    private bool isInterviewActive = false;

    void Start()
    {
        // 첫 질문 출력 및 면접 시작
        ShowNextQuestion();
        isInterviewActive = true;
    }

    // 외부에서 호출: 사용자가 말 끝낸 후 버튼 누르면 실행
    public void OnUserFinishedSpeaking(string userText)
    {
        if (!isInterviewActive) return;

        if (string.IsNullOrEmpty(userText))
        {
            Debug.LogWarning("사용자 입력 없음. 다음 질문으로 이동");
            AdvanceToNextQuestion();
            return;
        }

        StartCoroutine(HandleAIResponse(userText));
    }

    // AI 응답 처리 후 다음 질문 이동
    private IEnumerator HandleAIResponse(string userText)
    {
        // RespondToUserCoroutine 코루틴을 기다리고 처리
        yield return StartCoroutine(aiResponse.RespondToUserCoroutine(userText));

        AdvanceToNextQuestion();
    }

    // 질문 표시
    private void ShowNextQuestion()
    {
        if (currentQuestionIndex < questions.Length)
        {
            questionText.text = questions[currentQuestionIndex];
            Debug.Log("[질문] " + questions[currentQuestionIndex]);
        }
        else
        {
            EndInterview();
        }
    }

    // 다음 질문으로 진행
    private void AdvanceToNextQuestion()
    {
        currentQuestionIndex++;
        ShowNextQuestion();
    }

    // 면접 종료 시 피드백 표시
    private void EndInterview()
    {
        isInterviewActive = false;
        feedbackUI.Show("면접이 종료되었습니다. 수고하셨습니다!");
    }
}
