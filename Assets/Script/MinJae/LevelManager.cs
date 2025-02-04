using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    // Start is called before the first frame update
    //#. ���� �޴� ���� �ٸ� ������ �Ѿ�� ���� Level �迭 ����
    public GameObject[] Levels;
    public Animator animator;


    private int currentIndex = 0;
    private void Start()
    {
        foreach (GameObject level in Levels)
        {
            level.SetActive(false);
        }
        Levels[currentIndex].SetActive(true);
    }

    public void ChangeLevel(int index)
    {
        StartCoroutine(ChangeLevelCoroutione(index));


        Debug.Log("AcceptChange");
    }

    public void CirculateLevel()
    {
        int temp = (currentIndex+1)%Levels.Length;
        ChangeLevel(temp);
        Debug.Log("Accept");
    }

    private void FadeOut()
    {
        animator.SetBool("FainIn", false);
    }

    private void FadeIn()
    {
        animator.SetBool("FadeIn", true);
    }

    IEnumerator ChangeLevelCoroutione(int index)
    {
        yield return new WaitForSeconds(1.0f);

        Levels[currentIndex].SetActive(false);
        currentIndex = index;
        Levels[currentIndex].SetActive(true);
    }
}
