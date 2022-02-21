using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class SavedAudioInfo
{
    public string MusicName;
    public float Tempo;
    public float BeatTrackTempo;
    public List<Chord> ChordArray;
    public List<float> BeatArray;

    public SavedAudioInfo()
    {
        MusicName = string.Empty;
        ChordArray = null;
        BeatArray = null;
    }

    public SavedAudioInfo(string musicName, float tempo, float beatTrackTempo, List<Chord> chordArray, List<float> beatArray)
    {
        MusicName = musicName;
        BeatTrackTempo = beatTrackTempo;
        Tempo = tempo;
        ChordArray = chordArray;
        BeatArray = beatArray;
    }

    public static void Save(string musicName, float tempo, float beatTrackTempo, List<Chord> chordArray, List<float> beatArray)
    {
        SavedAudioInfo saveInfo = new SavedAudioInfo(musicName, tempo, beatTrackTempo, chordArray, beatArray);

        string saveText = JsonUtility.ToJson(saveInfo);
        string jsonFilePath = Path.ChangeExtension(PythonManager.musicDataDirectory + musicName, "json");
        File.WriteAllText(jsonFilePath, saveText);
    }

    public static bool Load(string musicName, ref SavedAudioInfo retInfo)
    {
        string jsonFilePath = Path.ChangeExtension(PythonManager.musicDataDirectory + musicName, "json");

        if (File.Exists(jsonFilePath))
        {
            try
            {
                string saveText = File.ReadAllText(jsonFilePath);
                retInfo = JsonUtility.FromJson<SavedAudioInfo>(saveText);

                return true;
            }
            catch (Exception e)
            {
                Debug.Log($"Json Load Error : {e.Message}");

                return false;
            }
        }
        else
        {
            return false;
        }
    }
}
