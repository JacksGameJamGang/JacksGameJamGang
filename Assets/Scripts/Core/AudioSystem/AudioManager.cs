using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : Singleton<AudioManager>
{
	public enum AudioTypes { master, music, sfx };

	[SerializeField] private AudioMixer _AudioMixer;
	public AudioMixerGroup MasterAudioGroup { get; private set; }
	public AudioMixerGroup MusicAudioGroup { get; private set; }
	public AudioMixerGroup SfxAudioGroup { get; private set; }

	public static event Action<float, AudioTypes> OnVolumeLoadUpdateSliders;

	//include ways of saving/loading player audio volumes at some point

	private float masterAudioVolume;
	private float musicAudioVolume;
	private float sfxAudioVolume;

	private void Start()
	{
		foreach (var group in _AudioMixer.FindMatchingGroups(string.Empty))
		{
			switch (group.name)
			{
				case "Master":
				MasterAudioGroup = group; continue;
				case "Music":
				MusicAudioGroup = group; continue;
				case "Sfx":
				SfxAudioGroup = group; continue;
				default:
				Debug.LogError("Failed to match audio group with name"); continue;
			}
		}

		LoadAudioVolumes();
	}

	private void OnEnable()
	{
		UnityEngine.SceneManagement.SceneManager.sceneLoaded += RestoreAudioVolumesOnMainMenuLoaded;
	}
	private void OnDisable()
	{
		UnityEngine.SceneManagement.SceneManager.sceneLoaded -= RestoreAudioVolumesOnMainMenuLoaded;
	}

	void RestoreAudioVolumesOnMainMenuLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene.name != "MainMenuScene") return;
		LoadAudioVolumes();
	}

	public void SaveAudioVolumes()
	{
		//save audio volumes to player settings file
	}
	public void LoadAudioVolumes()
	{
		//load audio volumes from player settings file
		masterAudioVolume = 0.5f;
		musicAudioVolume = 0.5f;
		sfxAudioVolume = 0.5f;

		OnVolumeLoadUpdateSliders?.Invoke(masterAudioVolume, AudioTypes.master);
		OnVolumeLoadUpdateSliders?.Invoke(musicAudioVolume, AudioTypes.music);
		OnVolumeLoadUpdateSliders?.Invoke(sfxAudioVolume, AudioTypes.sfx);
	}

	//update volumes from sliders in ui
	public static void UpdateMasterVolume(float volume)
	{
		if (volume == 0)
			Instance._AudioMixer.SetFloat("Master", -100);
		else
			Instance._AudioMixer.SetFloat("Master", Mathf.Log10(volume) * 20);

		Instance.masterAudioVolume = volume;
	}
	public static void UpdateMusicVolume(float volume)
	{
		if (volume == 0)
			Instance._AudioMixer.SetFloat("Music", -100);
		else
			Instance._AudioMixer.SetFloat("Music", Mathf.Log10(volume) * 20);

		Instance.musicAudioVolume = volume;
	}
	public static void UpdateSfxVolume(float volume)
	{
		if (volume == 0)
			Instance._AudioMixer.SetFloat("Sfx", -100);
		else
			Instance._AudioMixer.SetFloat("Sfx", Mathf.Log10(volume) * 20);

		Instance.sfxAudioVolume = volume;
	}

	public static AudioMixerGroup GetAudioGroupFromAudioType(AudioTypes audioType)
	{
		switch (audioType)
		{
			case AudioTypes.master:
				return Instance.MasterAudioGroup;
			case AudioTypes.music:
				return Instance.MusicAudioGroup;
			case AudioTypes.sfx:
				return Instance.SfxAudioGroup;
			default:
				Debug.LogError("Failed to match audio Type to audio group");
				return Instance.MasterAudioGroup;
		}
	}
}
