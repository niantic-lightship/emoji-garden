// Copyright 2022-2024 Niantic.
using Niantic.Lightship.AR.VpsCoverage;
using UnityEngine;

public class SharedData : MonoBehaviour
{
    public static SharedData Instance { get; private set; }
    public LocalizationTarget target;
    public Texture2D HintImage;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(Instance);
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
