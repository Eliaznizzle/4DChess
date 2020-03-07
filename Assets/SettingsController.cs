using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{

    public Toggle postProcessing;
    public Slider volume;

    public void PostProcessing() {
        PlayerPrefs.SetInt("postprocessing", postProcessing.isOn ? 1 : 0);
    }

    public void Volume() {
        PlayerPrefs.SetFloat("volume", volume.value);
    }
}
