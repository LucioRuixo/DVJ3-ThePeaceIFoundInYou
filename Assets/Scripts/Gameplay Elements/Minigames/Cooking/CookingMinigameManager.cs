﻿using System;
using UnityEngine;
using UnityEngine.UI;

public class CookingMinigameManager : MonoBehaviour
{
    bool playing = false;

    public int cuttingTarget;

    public float timer = 60f;
    float currentlyCut = 0f;


    public FoodGenerator foodGenerator;
    public Slider progressBar;
    public Clock clock;

    static public event Action<bool> OnGameEnd;

    void OnEnable()
    {
        playing = true;

        Food.OnCut += IncreaseCurrentlyCut;
        Food.OnFallenUnCut += DecreaseCurrentlyCut;
        Clock.OnTimeUp += EndGame;
    }

    void OnDisable()
    {
        Food.OnCut -= IncreaseCurrentlyCut;
        Food.OnFallenUnCut -= DecreaseCurrentlyCut;
        Clock.OnTimeUp -= EndGame;
    }

    void Start()
    {
        clock.StartTimer(timer);
    }

    void IncreaseCurrentlyCut()
    {
        if (!playing) return;

        currentlyCut++;
        progressBar.value = currentlyCut / cuttingTarget;

        if (currentlyCut >= cuttingTarget) EndGame(true);
    }

    void DecreaseCurrentlyCut()
    {
        if (!playing) return;

        if (currentlyCut > 0)
        {
            currentlyCut--;
            progressBar.value = currentlyCut / cuttingTarget;
        }
    }

    void EndGame(bool win)
    {
        playing = false;

        foodGenerator.generationActive = false;
        clock.timerActive = false;

        OnGameEnd?.Invoke(win);
    }
}