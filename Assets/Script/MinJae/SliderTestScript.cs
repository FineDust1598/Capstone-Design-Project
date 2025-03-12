using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SliderTestScript : MonoBehaviour
{

    public Slider slider; // UI의 Slider 연결
    public AudioMixer audioMixer; // Audio Mixer 연결


    // Start is called before the first frame update
    void Start()
    {
        if (slider != null)
        {
            // 값이 변경될 때마다 LogSliderValue 실행
            slider.onValueChanged.AddListener(SetVolume);
        }
        else
        {
            Debug.LogError("Volume Slider가 연결되지 않았습니다! Inspector에서 할당하세요.");
            return;
        }

        // 이전에 저장된 볼륨 값 불러오기
        if (PlayerPrefs.HasKey("Volume"))
        {
            float savedVolume = PlayerPrefs.GetFloat("Volume");
            slider.value = savedVolume;
            SetVolume(savedVolume);
        }
        else
        {
            slider.value = 1f; // 기본값 설정
            SetVolume(1f);
            Debug.Log("Failed in HasKey~");
        }
    }


    void SetVolume(float volume)
    {
        float volumeDB = Mathf.Log10(volume) * 20; // 로그 변환
        if(audioMixer.SetFloat("Master", volumeDB))
        {
            Debug.Log("Success~");
        }
        else
        {
            Debug.Log("Failed~");
        }
        Debug.Log("현재 볼륨 (dB): " + volumeDB);

        // 설정 저장
        PlayerPrefs.SetFloat("Volume", volume);
    }
}
