using SoundManager;
using System;
using UnityEngine;
using UnityEngine.Audio;
using CustomUtils;

namespace SoundManager
{
    public class SoundsManager : SingletonMono<SoundsManager>
    {
        public SoundSO SO;
        public AudioSource musicSource;
        public AudioSource sfxSource;

        private float musicVolume;
        private float sfxVolume;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this);

            SetMusicVolume(PlayerPrefs.GetFloat("Music Volume", 1f));
            SetSFXVolume(PlayerPrefs.GetFloat("VFX Volume", 1f));
        }

        public float GetMusicVolume()
        { 
            return musicVolume; 
        }
        public float GetSFXVolume() 
        {  
            return sfxVolume; 
        }

        public void SetMusicVolume(float value)
        {
            musicVolume = value;
            musicSource.volume = musicVolume;
            PlayerPrefs.SetFloat("Music Volume", musicVolume);
        }

        public void SetSFXVolume(float value)
        {
            sfxVolume = value;
            sfxSource.volume = sfxVolume;
            PlayerPrefs.SetFloat("VFX Volume", sfxVolume);
        }

        public void PlaySFX(SoundType sound, AudioSource source = null)
        {
            SoundList soundList = SO.sounds[(int)sound];
            AudioClip[] clips = soundList.sounds;
            AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

            if (source)
            {
                source.outputAudioMixerGroup = soundList.mixer;
                source.clip = randomClip;
                source.volume = sfxVolume * soundList.volume;
                source.Play();
            }
            else
            {
                sfxSource.outputAudioMixerGroup = soundList.mixer;
                sfxSource.PlayOneShot(randomClip, sfxVolume * soundList.volume);
            }
        }

        public void PlayMusic(SoundType sound, AudioSource source = null)
        {
            SoundList soundList = SO.sounds[(int)sound];
            AudioClip[] clips = soundList.sounds;
            AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

            if (source)
            {
                source.outputAudioMixerGroup = soundList.mixer;
                source.clip = randomClip;
                source.volume = musicVolume * soundList.volume;
                source.Play();
            }
            else
            {
                musicSource.outputAudioMixerGroup = soundList.mixer;
                musicSource.clip = randomClip;
                musicSource.volume = musicVolume * soundList.volume;
                musicSource.Play();
            }
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        public void StopSFX()
        {
            sfxSource.Stop();
        }
    }

    [Serializable]
    public struct SoundList
    {
        //[HideInInspector] 
        public string name;
        [Range(0, 1)] public float volume;
        public AudioMixerGroup mixer;
        public AudioClip[] sounds;
    }
}