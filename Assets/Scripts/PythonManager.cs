using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

public class PythonManager : MonoBehaviour
{
    public static string musicDirectory;
    public static string musicDataDirectory;

    public GameManager GM;
    public float Tempo;
    public bool TempoIsAnalyzed = false;
    public int PitchModifier;
    
    private static string pythonDirectory;
    private static string pythonScriptPath;
    private static string chordAnalyzePath;

    private string connectionIP = "127.0.0.1";
    private int connectionPort = 9999;
    private Process pythonServer;

    private void Awake()
    {
        musicDirectory = $"{Application.dataPath}/Resources/Music/";
        musicDataDirectory = $"{musicDirectory}Data/";
        pythonDirectory = $"{Application.dataPath}/Python/";
        pythonScriptPath = $"{pythonDirectory}main.py";
        chordAnalyzePath = $"{pythonDirectory}chord_analyzer.py";

        if (!Directory.Exists(musicDataDirectory))
        {
            Directory.CreateDirectory(musicDataDirectory);
        }
    }

    public async Task<bool> RunPythonServer()
    {
        Task chordAnalyzeTask;
        string CmdText;
        string pythonEnv;

        string musicFullPath = musicDirectory + GM.MusicName;
        string targetChordMidi = GM.Track.clip.name.Replace(".mp3", string.Empty).Replace(".wav", string.Empty) + "-chord.midi";

        Tempo = 0;

        // 김기오 사용환경일때
        if (Directory.Exists(@"E:\Programs\Miniconda\envs\rhythm_env"))
        {
            pythonEnv = "conda activate rhythm_env&python";
        }
        else
        {
            pythonEnv = $"{pythonDirectory}venv/Scripts/python.exe";
        }

        chordAnalyzeTask = WhenFileCreated($"{musicDirectory}{targetChordMidi}");

        // chord midi 파일 생성.
        if (!File.Exists($"{musicDirectory}{targetChordMidi}"))
        {
            Process.Start("CMD.exe", $"/C cd {pythonDirectory}&{pythonEnv} \"{chordAnalyzePath}\"");
        }

        await chordAnalyzeTask;

        string jsonFilePath = Path.ChangeExtension($"{musicDataDirectory}{GM.MusicName}", "json");

        if (File.Exists(jsonFilePath))
        {
            SavedAudioInfo preprocessedInfo = new SavedAudioInfo();
            bool ret = SavedAudioInfo.Load(GM.MusicName, ref preprocessedInfo);

            if (ret)
            {
                GM.Tempo = preprocessedInfo.Tempo;
                GM.BeatTrackTempo = preprocessedInfo.BeatTrackTempo;
                GM.BeatArray = preprocessedInfo.BeatArray;
                GM.ChordArray = preprocessedInfo.ChordArray;

                return true;
            }
        }

        CmdText = $"/C {pythonEnv} \"{pythonScriptPath}\"";
        pythonServer = Process.Start("CMD.exe", CmdText);
        GetData(musicFullPath);
        pythonServer.CloseMainWindow();

        SavedAudioInfo.Save(GM.MusicName, GM.Tempo, GM.BeatTrackTempo, GM.ChordArray, GM.BeatArray);

        return true;
    }


    public void GetData(string musicFullPath)
    {

        using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
            while (true)
            {
                try
                {
                    client.Connect(new IPEndPoint(IPAddress.Parse(connectionIP), connectionPort));
                    byte[] data_path = Encoding.UTF8.GetBytes(musicFullPath);
                    client.Send(data_path);

                    int size = 10;
                    byte[] data = new byte[size];

                    int bytes_read = client.Receive(data, data.Length, SocketFlags.None);
                    string data_received = Encoding.UTF8.GetString(data, 0, bytes_read);
                    int packetLength = int.Parse(data_received);
                    UnityEngine.Debug.Log("Midi packet length: " + data_received);

                    data  = new byte[packetLength];
                    bytes_read = client.Receive(data, data.Length, SocketFlags.None);
                    data_received = Encoding.UTF8.GetString(data, 0, bytes_read);
                    UnityEngine.Debug.Log("Midi packet data: " + data_received);

                    ParseMidiPacket(data_received);
                    TempoIsAnalyzed = true;

                    bytes_read = client.Receive(data, data.Length, SocketFlags.None);
                    data_received = Encoding.UTF8.GetString(data, 0, bytes_read);
                    packetLength = int.Parse(data_received);
                    UnityEngine.Debug.Log("Beat packet length: " + data_received);

                    data = new byte[packetLength];
                    bytes_read = client.Receive(data, data.Length, SocketFlags.None);
                    data_received = Encoding.UTF8.GetString(data, 0, bytes_read);
                    UnityEngine.Debug.Log("Beat packet data: " + data_received);

                    ParseBeatPacket(data_received);

                    byte[] msg_data = Encoding.ASCII.GetBytes("Got message!");
                    client.Send(msg_data);

                    break;
                }
                catch (SocketException se)
                {
                    UnityEngine.Debug.Log(se.Message);
                }
            }
        }

        UnityEngine.Debug.Log("Got data from python server.");
    }

    private static Task WhenFileCreated(string path)
    {
        UnityEngine.Debug.Log($"When File Created path :{path}");
        if (File.Exists(path))
            return Task.FromResult(true);

        var tcs = new TaskCompletionSource<bool>();
        FileSystemWatcher watcher = new FileSystemWatcher(Path.GetDirectoryName(path));

        FileSystemEventHandler createdHandler = null;
        RenamedEventHandler renamedHandler = null;
        createdHandler = (s, e) =>
        {
            if (Path.GetFileName(e.Name) == Path.GetFileName(path))
            {
                UnityEngine.Debug.Log($"CreateHandler called");
                tcs.TrySetResult(true);
                watcher.Created -= createdHandler;
                watcher.Dispose();
            }
        };

        renamedHandler = (s, e) =>
        {
            if (Path.GetFileName(e.Name) == Path.GetFileName(path))
            {
                UnityEngine.Debug.Log($"RenameHandler called");
                tcs.TrySetResult(true);
                watcher.Renamed -= renamedHandler;
                watcher.Dispose();
            }
        };

        watcher.Created += createdHandler;
        watcher.Renamed += renamedHandler;

        watcher.EnableRaisingEvents = true;

        return tcs.Task;
    }

    void ParseBeatPacket(string packet)
    {
        string[] packetData = packet.Split(' ');
        float beatInSec;

        GM.BeatArray = new List<float>();
        GM.BeatTrackTempo = float.Parse(packetData[0]);

        int packetDataLength = packetData.Length;

        for (int i = 1; i < packetDataLength; i++)
        {
            beatInSec = float.Parse(packetData[i]);
            GM.BeatArray.Add(beatInSec * GM.Tempo / 60.0f);
        }

        UnityEngine.Debug.Log(string.Join(" ", GM.BeatArray));
    }

    void ParseMidiPacket(string packet)
    {
        string[] packetData = packet.Split(' ');

        GM.ChordArray = new List<Chord>();
        GM.Tempo = float.Parse(packetData[0]);

        int beatArrayLength = packetData.Length - 1;

        for (int i = 0; i < beatArrayLength; i++)
        {
            string[] data = packetData[i + 1].Split(',');
            Chord chord = new Chord();
            chord.offset = float.Parse(data[0]);
            chord.pitch = int.Parse(data[1]) + PitchModifier;

            GM.ChordArray.Add(chord);
        }

        UnityEngine.Debug.Log("Last Chord Length: " + GM.ChordArray[GM.ChordArray.Count - 1].offset);
    }

    private void OnDestroy()
    {
        //pythonServer.Kill();
    }
}
