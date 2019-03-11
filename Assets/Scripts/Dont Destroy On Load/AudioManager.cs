﻿using UnityEngine;
using Sirenix.OdinInspector;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Title("DataBase Sounds")]
    public AudioDB audioDB;

    [Title("Audio Sources")]
    public AudioSource SFX;
    public AudioSource Music;

    [Title("Volumes")]
    public bool mute;
    [Range(0f,1f)]
    public float volumeSFX;
    [Range(0f, 1f)]
    public float volumeMusic;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public void PlayByName(string soundName)
    {

        for (int i = 0; i < audioDB.audios.Count; i++)
        {
            if(audioDB.audios[i].name == soundName)
            {
                if(audioDB.audios[i].audioChannel == AudioChannel.SFX)
                {
                    SFX.PlayOneShot(audioDB.audios[i].clip, volumeSFX);

                }else if(audioDB.audios[i].audioChannel == AudioChannel.Music)
                {
                    Music.Stop();
                    Music.volume = volumeMusic;
                    Music.clip = audioDB.audios[i].clip;
                }
            }
        }

    }

    [Button(Name = "Play Sound ID")]
    public void PlayByID(int id)
    {

        for (int i = 0; i < audioDB.audios.Count; i++)
        {
            if (audioDB.audios[i].ID == id)
            {
                if (audioDB.audios[i].audioChannel == AudioChannel.SFX)
                {
                    SFX.PlayOneShot(audioDB.audios[i].clip, volumeSFX);

                }
                else if (audioDB.audios[i].audioChannel == AudioChannel.Music)
                {
                    Music.Stop();
                    Music.volume = volumeMusic;
                    Music.clip = audioDB.audios[i].clip;
                    Music.Play();
                }
            }
        }

    }

    [Button(Name = "Play Group ID")]
    public void PlayRandomByID(int groupID)
    {
        //TODO implement play by random group
    }

    [Button(Name ="Mute")]
    public void Mute()
    {
        mute = !mute;
        Music.mute = mute;
        SFX.mute = mute;
    }

    [Button(Name = "Pause")]
    public void PauseMusic()
    {
        Music.Stop();
    }

}