using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class SettingsController : MonoBehaviour
{

    public Toggle postProcessing;
    public Slider volume;

    void Awake() {
        if (PlayerPrefs.GetInt("initialized") == 0) {
            //Settings are not initialized. Initialize them.
            PlayerPrefs.SetFloat("volume", 1);
            PlayerPrefs.SetInt("postprocessing", 1);
        }
    }

    public void UpdateSettings() {
        bool postProcessingValue = (PlayerPrefs.GetInt("postprocessing") == 1) ? true : false;
        float volumeValue = PlayerPrefs.GetFloat("volume");

        postProcessing.isOn = postProcessingValue;
        volume.value = volumeValue;

        foreach (AudioSource aso in FindObjectsOfType<AudioSource>()) {
            aso.volume = volumeValue;
        }

        Resources.FindObjectsOfTypeAll<PostProcessVolume>()[0].gameObject.SetActive(postProcessingValue);//This might not work try it
    }

    public void PostProcessing() {
        PlayerPrefs.SetInt("postprocessing", postProcessing.isOn ? 1 : 0);
        UpdateSettings();
    }

    public void Volume() {
        PlayerPrefs.SetFloat("volume", volume.value);
        UpdateSettings();
    }
}
