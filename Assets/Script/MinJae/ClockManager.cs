using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockManager : MonoBehaviour
{
    public Transform hourHand;
    public Transform minuteHand;
    public Transform secondHand;

    // Start is called before the first frame update
    void Start()
    {

    }

    void Update()
    {
        DateTime time = DateTime.Now;

        float hour = time.Hour % 12 + time.Minute / 60f;
        float minute = time.Minute + time.Second / 60f;
        float second = time.Second;

        hourHand.localRotation = Quaternion.Euler(0f, 0f, hour * 30f);
        minuteHand.localRotation = Quaternion.Euler(0f, 0f, minute * 6f);
        secondHand.localRotation = Quaternion.Euler(0f, 0f, second * 6f);
    }
}
