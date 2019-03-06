using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public enum AudioChannel
{
    SFX,
    Music
}

[System.Serializable]
public struct Audio
{
    public int ID;
    public AudioChannel audioChannel;
    public string name;
    public AudioClip clip;
}

[System.Serializable]
public struct RandomAudioGroups
{
    public int ID;
    public AudioChannel audioChannel;
    public string name;
    public List<AudioClip> audioGroup;
}

[CreateAssetMenu(menuName ="AudioDB")]
public class AudioDB : ScriptableObject
{
    [Title("Audios")]
    public List<Audio> audios = new List<Audio>();

    [Title("Random Group Audios")]
    public List<RandomAudioGroups> randomGroupAudios = new List<RandomAudioGroups>();
}
