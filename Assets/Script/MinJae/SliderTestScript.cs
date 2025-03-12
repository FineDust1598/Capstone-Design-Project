using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SliderTestScript : MonoBehaviour
{

    public Slider slider; // UI�� Slider ����
    public AudioMixer audioMixer; // Audio Mixer ����


    // Start is called before the first frame update
    void Start()
    {
        if (slider != null)
        {
            // ���� ����� ������ LogSliderValue ����
            slider.onValueChanged.AddListener(SetVolume);
        }
        else
        {
            Debug.LogError("Volume Slider�� ������� �ʾҽ��ϴ�! Inspector���� �Ҵ��ϼ���.");
            return;
        }

        // ������ ����� ���� �� �ҷ�����
        if (PlayerPrefs.HasKey("Volume"))
        {
            float savedVolume = PlayerPrefs.GetFloat("Volume");
            slider.value = savedVolume;
            SetVolume(savedVolume);
        }
        else
        {
            slider.value = 1f; // �⺻�� ����
            SetVolume(1f);
            Debug.Log("Failed in HasKey~");
        }
    }


    void SetVolume(float volume)
    {
        float volumeDB = Mathf.Log10(volume) * 20; // �α� ��ȯ
        if(audioMixer.SetFloat("Master", volumeDB))
        {
            Debug.Log("Success~");
        }
        else
        {
            Debug.Log("Failed~");
        }
        Debug.Log("���� ���� (dB): " + volumeDB);

        // ���� ����
        PlayerPrefs.SetFloat("Volume", volume);
    }
}
