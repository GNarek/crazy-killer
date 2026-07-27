using UnityEngine;
using UnityEngine.UI;

public class MuteToggle : MonoBehaviour
{
    private const string PrefKey = "AudioMuted";

    private void Awake()
    {
        bool muted = PlayerPrefs.GetInt(PrefKey, 0) == 1;
        ApplyMute(muted);

        if (TryGetComponent(out Toggle toggle))
        {
            toggle.isOn = muted;
            toggle.onValueChanged.AddListener(SetMuted);
        }
    }

    public void SetMuted(bool muted)
    {
        ApplyMute(muted);
        PlayerPrefs.SetInt(PrefKey, muted ? 1 : 0);
    }

    private static void ApplyMute(bool muted)
    {
        AudioListener.volume = muted ? 0f : 1f;
    }
}
