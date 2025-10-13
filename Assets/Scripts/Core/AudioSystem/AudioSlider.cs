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

	private void OnEnable()
	{
		AudioManager.OnVolumeLoadUpdateSliders += UpdateSlidersOnVolumeLoad;
	}
	private void OnDisable()
	{
		AudioManager.OnVolumeLoadUpdateSliders -= UpdateSlidersOnVolumeLoad;
	}

	void UpdateSlidersOnVolumeLoad(float volume, AudioTypes audioType)
	{
		if (_SliderAudioType != audioType) return;
		
		slider.value = volume;
		UpdateTextUi(volume);
	}

	public void OnSliderValueChange(float volume)
	{
		switch (_SliderAudioType)
		{
			case AudioTypes.master:
				AudioManager.UpdateMasterVolume(volume); break;
			case AudioTypes.music:
				AudioManager.UpdateMusicVolume(volume); break;
			case AudioTypes.sfx:
				AudioManager.UpdateSfxVolume(volume); break;
		}

		UpdateTextUi(volume);
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
