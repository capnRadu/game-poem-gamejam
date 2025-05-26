using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    private int collectedPages = 0;
    private int stage1Start = 2;
    private int stage2Start = 6;
    private int stage3Start = 10;
    private int stage4Start = 15;

    [Header("Volume Settings")]
    [SerializeField] private List<VolumeProfile> volumeProfiles;
    [SerializeField] private float transitionDuration = 2f;

    private List<Volume> stagedVolumes = new List<Volume>();
    private int targetFromIndex = 0;
    private int targetToIndex = 1;
    private float targetBlend = 0f;
    private Coroutine blendCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        SetupVolumes();
    }

    private void SetupVolumes()
    {
        foreach (var profile in volumeProfiles)
        {
            GameObject volumeObj = new GameObject("StagedVolume_" + profile.name);
            volumeObj.transform.SetParent(transform);
            Volume vol = volumeObj.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.profile = profile;
            vol.priority = 1;
            vol.weight = 0f;
            stagedVolumes.Add(vol);
        }

        stagedVolumes[0].weight = 1f;
    }

    public void CollectPage()
    {
        collectedPages++;
        Debug.Log("Collected Pages: " + collectedPages);
        collectedPages = Mathf.Clamp(collectedPages, 0, stage4Start);
        UpdateVolumeBlendGradually();
        
        if (collectedPages == stage3Start)
        {
            FindFirstObjectByType<PlayerController>().UpdateSpeed("State4");
        }

        if (collectedPages == stage2Start)
        {
            FindFirstObjectByType<PlayerController>().UpdateSpeed("State3");
        }

        if (collectedPages == stage1Start)
        {
            FindFirstObjectByType<PlayerController>().UpdateSpeed("State2");
        }

        if (collectedPages == stage4Start)
        {
            ShowFinalMessage();
        }
    }

    private void UpdateVolumeBlendGradually()
    {
        if (collectedPages <= stage1Start)
        {
            targetFromIndex = 0;
            targetToIndex = 1;
            targetBlend = Mathf.InverseLerp(0, stage1Start, collectedPages);
        }
        else if (collectedPages <= stage2Start)
        {
            targetFromIndex = 1;
            targetToIndex = 2;
            targetBlend = Mathf.InverseLerp(stage1Start, stage2Start, collectedPages);
        }
        else if (collectedPages <= stage3Start)
        {
            targetFromIndex = 2;
            targetToIndex = 3;
            targetBlend = Mathf.InverseLerp(stage2Start, stage3Start, collectedPages);
        }
        else
        {
            targetFromIndex = 3;
            targetToIndex = 4;
            targetBlend = Mathf.InverseLerp(stage3Start, stage4Start, collectedPages);
        }

        if (blendCoroutine != null)
            StopCoroutine(blendCoroutine);

        blendCoroutine = StartCoroutine(BlendVolumesOverTime(transitionDuration));
    }

    private IEnumerator BlendVolumesOverTime(float duration)
    {
        float elapsed = 0f;
        float startWeightFrom = stagedVolumes[targetFromIndex].weight;
        float startWeightTo = stagedVolumes[targetToIndex].weight;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float currentBlend = Mathf.Lerp(startWeightTo, targetBlend, t);

            for (int i = 0; i < stagedVolumes.Count; i++)
            {
                if (i == targetFromIndex)
                {
                    stagedVolumes[i].weight = 1f - currentBlend;
                }
                else if (i == targetToIndex)
                {
                    stagedVolumes[i].weight = currentBlend;
                }
                else
                {
                    stagedVolumes[i].weight = 0f;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        stagedVolumes[targetFromIndex].weight = 1f - targetBlend;
        stagedVolumes[targetToIndex].weight = targetBlend;
    }

    private void ShowFinalMessage()
    {
        Debug.Log("You made it. Perseverance wins.");
    }
}