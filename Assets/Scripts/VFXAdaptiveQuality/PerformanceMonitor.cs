using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceMonitor : MonoBehaviour
{
    public static PerformanceMonitor Instance { get; private set; }

    [Header("FPS")]
    [SerializeField] private float updateInterval = 0.5f;

    public float AverageFPS { get; private set; }

    private float timer;
    private int frameCount;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        frameCount++;
        timer += Time.unscaledDeltaTime;

        if (timer >= updateInterval)
        {
            AverageFPS = frameCount / timer;

            frameCount = 0;
            timer = 0f;
        }
    }
}
