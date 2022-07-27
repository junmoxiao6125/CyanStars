using System;
using System.Collections.Generic;

using CyanStars.Framework;
using CyanStars.Gameplay.MusicGame;

using UnityEngine;


public class DebugHelper : MonoBehaviour
{
    private MusicGameModule dataModule = GameRoot.GetDataModule<MusicGameModule>();

    public float CurrentTime;
    private float LastTime;
    [Range(0, 1)] public float CurrentPlayRate;
    private float LastPlayRate;

    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GameObject.Find("SceneRoot").GetComponent<AudioSource>(); 
    }

    void Update()
    {
        if (dataModule.RunningTimeline != null)
        {
            if (LastTime != CurrentTime)
            {
                dataModule.RunningTimeline.SetCurTime(CurrentTime);
                audioSource.time = CurrentTime;
                Debug.Log("Debug:通过输入time的方式改变当前时间");
            }
            else if(LastPlayRate != CurrentPlayRate)
            {
                CurrentTime = CurrentPlayRate * dataModule.RunningTimeline.Length;
                dataModule.RunningTimeline.SetCurTime(CurrentTime);
                audioSource.time = CurrentTime;
                Debug.Log("Debug:通过拖动的方式改变当前时间");
            }
            CurrentTime = dataModule.RunningTimeline.CurrentTime;
            LastTime = CurrentTime;
            CurrentPlayRate = CurrentTime / dataModule.RunningTimeline.Length;
            LastPlayRate = CurrentPlayRate;
        }
    }
}
