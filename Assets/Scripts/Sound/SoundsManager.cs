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

        private bool isMusicMuted;
        private bool isAllMuted;

        public bool IsMusicMuted => isMusicMuted;
        public bool IsAllMuted => isAllMuted;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this);

            if (musicSource == null)
            {
                musicSource = transform.Find("MusicSource")?.GetComponent<AudioSource>();
                if (musicSource == null) musicSource = GetComponent<AudioSource>();
                if (musicSource == null)
                {
                    GameObject go = new GameObject("MusicSource");
                    go.transform.SetParent(transform);
                    musicSource = go.AddComponent<AudioSource>();
                }
            }

            if (sfxSource == null)
            {
                sfxSource = transform.Find("SFXSource")?.GetComponent<AudioSource>();
                if (sfxSource == null)
                {
                    AudioSource[] sources = GetComponents<AudioSource>();
                    if (sources.Length > 1) sfxSource = sources[1];
                    else if (sources.Length > 0 && sources[0] != musicSource) sfxSource = sources[0];
                }
                if (sfxSource == null)
                {
                    GameObject go = new GameObject("SFXSource");
                    go.transform.SetParent(transform);
                    sfxSource = go.AddComponent<AudioSource>();
                }
            }

            isMusicMuted = PlayerPrefs.GetInt("Music Muted", 0) == 1;
            isAllMuted = PlayerPrefs.GetInt("All Muted", 0) == 1;

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
            PlayerPrefs.SetFloat("Music Volume", musicVolume);
            UpdateVolumeLevels();
        }

        public void SetSFXVolume(float value)
        {
            sfxVolume = value;
            PlayerPrefs.SetFloat("VFX Volume", sfxVolume);
            UpdateVolumeLevels();
        }

        public void ToggleMusicMute()
        {
            isMusicMuted = !isMusicMuted;
            PlayerPrefs.SetInt("Music Muted", isMusicMuted ? 1 : 0);
            UpdateVolumeLevels();
        }

        public void ToggleAllMute()
        {
            isAllMuted = !isAllMuted;
            PlayerPrefs.SetInt("All Muted", isAllMuted ? 1 : 0);
            UpdateVolumeLevels();
        }

        private void UpdateVolumeLevels()
        {
            if (musicSource != null)
                musicSource.volume = (isMusicMuted || isAllMuted) ? 0f : musicVolume;
            if (sfxSource != null)
                sfxSource.volume = isAllMuted ? 0f : sfxVolume;
        }

        public void PlaySFX(SoundType sound, AudioSource source = null)
        {
            if (SO == null || SO.sounds == null || (int)sound >= SO.sounds.Length) return;
            SoundList soundList = SO.sounds[(int)sound];
            AudioClip[] clips = soundList.sounds;
            if (clips == null || clips.Length == 0) return;
            AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

            if (source)
            {
                source.outputAudioMixerGroup = soundList.mixer;
                source.clip = randomClip;
                source.volume = isAllMuted ? 0f : (sfxVolume * soundList.volume);
                source.Play();
            }
            else
            {
                if (sfxSource == null) return;
                sfxSource.outputAudioMixerGroup = soundList.mixer;
                if (sound == SoundType.Countdown_Tick)
                {
                    sfxSource.clip = randomClip;
                    sfxSource.volume = isAllMuted ? 0f : (sfxVolume * soundList.volume);
                    sfxSource.Play();
                }
                else
                {
                    sfxSource.PlayOneShot(randomClip, isAllMuted ? 0f : (sfxVolume * soundList.volume));
                }
            }
        }

        public void PlayMusic(SoundType sound, AudioSource source = null)
        {
            if (SO == null || SO.sounds == null || (int)sound >= SO.sounds.Length) return;
            SoundList soundList = SO.sounds[(int)sound];
            AudioClip[] clips = soundList.sounds;
            if (clips == null || clips.Length == 0) return;
            AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

            if (source)
            {
                source.outputAudioMixerGroup = soundList.mixer;
                source.clip = randomClip;
                source.volume = (isMusicMuted || isAllMuted) ? 0f : (musicVolume * soundList.volume);
                source.Play();
            }
            else
            {
                if (musicSource == null) return;
                musicSource.outputAudioMixerGroup = soundList.mixer;
                musicSource.clip = randomClip;
                musicSource.volume = (isMusicMuted || isAllMuted) ? 0f : (musicVolume * soundList.volume);
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