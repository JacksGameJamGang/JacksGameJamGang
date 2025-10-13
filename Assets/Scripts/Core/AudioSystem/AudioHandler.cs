using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static AudioManager;

[RequireComponent(typeof(AudioSource))]
public class AudioHandler : MonoBehaviour
{
	[Header("Audio Settings")]
	[SerializeField] private AudioTypes audioType;
	public bool playsBackgroundMusic;

	private AudioSource audioSource;

	public List<AudioClip> audioClips;

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
		audioSource.outputAudioMixerGroup = AudioManager.GetAudioGroupFromAudioType(audioType);
		audioSource.loop = false;
		audioSource.playOnAwake = false;
	}

	public void PlayAudio(AudioClip clip, bool forcePlay)
	{
		if (forcePlay)
		{
			audioSource.Stop();
			audioSource.clip = clip;
			audioSource.Play();
		}
		else
		{
			if (audioSource.isPlaying) return;
			audioSource.clip = clip;
			audioSource.Play();
		}
	}
	public void PlayAudio(AudioClip clip, AudioTypes audioType, bool forcePlay)
	{
		this.audioType = audioType;
		audioSource.outputAudioMixerGroup = AudioManager.GetAudioGroupFromAudioType(audioType);

		if (forcePlay)
		{
			audioSource.Stop();
			audioSource.clip = clip;
			audioSource.Play();
		}
		else
		{
			if (audioSource.isPlaying) return;
			audioSource.clip = clip;
			audioSource.Play();
		}
	}
}
