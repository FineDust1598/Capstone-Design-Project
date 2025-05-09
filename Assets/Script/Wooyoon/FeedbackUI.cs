using UnityEngine;
using UnityEngine.UI;

public class FeedbackUI : MonoBehaviour
{
    public GameObject feedbackPanel;   // 피드백 패널 전체 오브젝트
    public Text feedbackText;          // 피드백 내용을 출력할 UI Text

    // 피드백 텍스트 표시 함수
    public void Show(string feedback)
    {
        feedbackPanel.SetActive(true);
        feedbackText.text = feedback;
    }

    // 다시 시도 버튼에 연결
    public void OnRetry()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0); // 첫 씬 재시작
    }

    // 종료 버튼에 연결
    public void OnExit()
    {
        Application.Quit(); // 애플리케이션 종료
    }
}
