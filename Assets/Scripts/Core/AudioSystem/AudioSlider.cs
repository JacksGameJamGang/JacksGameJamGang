using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using static AudioManager;

public class AudioSlider : MonoBehaviour
{
	private Slider slider;
	[SerializeField] private AudioMixer _AudioMixer;
	[SerializeField] private TMP_Text _AudioText;

	[SerializeField] private AudioTypes _SliderAudioType;

	private void Awake()
	{
		slider = GetComponent<Slider>();

		switch (_SliderAudioType)
		{
			case AudioTypes.master:
			gameObject.name = "MasterAudioSlider"; break;
			case AudioTypes.music:
			gameObject.name = "MusicAudioSlider"; break;
			case AudioTypes.sfx:
			gameObject.name = "SfxAudioSlider"; break;
		}
	}

	private void Start()
	{
		float debugStartingVolume = 0.5f;
		slider.value = debugStartingVolume;
	}

	public void OnSliderValueChange(float volume)
	{
		switch (_SliderAudioType)
		{
			case AudioTypes.master:
				UpdateMasterVolume(volume); break;
			case AudioTypes.music:
				UpdateMusicVolume(volume); break;
			case AudioTypes.sfx:
				UpdateSfxVolume(volume); break;
		}

		UpdateTextUi(volume);
	}

	void UpdateMasterVolume(float volume)
	{
		if (volume == 0)
			_AudioMixer.SetFloat("Master", -100);
		else
			_AudioMixer.SetFloat("Master", Mathf.Log10(volume) * 20);
	}
	void UpdateMusicVolume(float volume)
	{
		if (volume == 0)
			_AudioMixer.SetFloat("Music", -100);
		else
			_AudioMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
	}
	void UpdateSfxVolume(float volume)
	{
		if (volume == 0)
			_AudioMixer.SetFloat("Sfx", -100);
		else
			_AudioMixer.SetFloat("Sfx", Mathf.Log10(volume) * 20);
	}

	void UpdateTextUi(float volume)
	{
		string volumeType = "";

		switch (_SliderAudioType)
		{
			case AudioTypes.master:
				volumeType = "Master Volume: "; break;
			case AudioTypes.music:
				volumeType = "Music Volume: "; break;
			case AudioTypes.sfx:
				volumeType = "Sfx Volume: "; break;
		}

		_AudioText.text = $"{volumeType} {(int)(volume * 100)}";
	}
}
