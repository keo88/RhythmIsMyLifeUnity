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
    public GameManager GM;
    public float Tempo;
    public bool TempoIsAnalyzed = false;
    public int PitchModifier;

    private string connectionIP = "127.0.0.1";
    private int connectionPort = 9999;
    private Process pythonServer;
    private string musicDirectory;


    public async Task<bool> RunPythonServer()
    {
        Task chordAnalyzeTask;
        string CmdText;
        string pythonEnv;
        string pythonDirectory = Application.dataPath + "/Python/";
        string pythonScriptPath = pythonDirectory + "main.py";
        string chordAnalyzePath = pythonDirectory + "chord_analyzer.py";

        string targetChordMidi = GM.Track.clip.name.Replace(".mp3", string.Empty).Replace(".wav", string.Empty) + "-chord.midi";

        musicDirectory = $"{Application.dataPath}/Resources/Music/";
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

        CmdText = $"/C {pythonEnv} \"{pythonScriptPath}\"";
        pythonServer = Process.Start("CMD.exe", CmdText);
        GetData();

        return true;
    }


    public void GetData()
    {
        using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
            while (true)
            {
                try
                {
                    client.Connect(new IPEndPoint(IPAddress.Parse(connectionIP), connectionPort));
                    byte[] data_path = Encoding.UTF8.GetBytes(musicDirectory + GM.MusicName);
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

        UnityEngine.Debug.Log("get data");
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

        foreach (string beat in packetData)
        {
            beatInSec = float.Parse(beat);
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
            //GM.BeatArray[i] = float.Parse(packetData[i + 1]);

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
