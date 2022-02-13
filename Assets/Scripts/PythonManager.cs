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
    public string MusicName;
    public float Tempo;
    public bool TempoIsAnalyzed = false;

    private string connectionIP = "127.0.0.1";
    private int connectionPort = 9999;
    private Process pythonServer;
    private string musicDirectory;


    public async void RunPythonServer()
    {
        Task chordAnalyzeTask;
        string CmdText;
        string pythonEnv;
        string pythonDirectory = Application.dataPath + "/Python/";
        string pythonScriptPath = pythonDirectory + "main.py";
        string chordAnalyzePath = pythonDirectory + "chord_analyzer.py";
        string targetChordMidi = GM.Track.clip.name.Replace(".mp3", string.Empty).Replace(".wav", string.Empty) + "-chord.midi";

        musicDirectory = $"{Application.dataPath}/MusicSample/";
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

        CmdText = $"/C {pythonEnv} \"{pythonScriptPath}\"";
        pythonServer = Process.Start("CMD.exe", CmdText);
        GetData();

        // chord midi 파일 생성.
        if (!File.Exists($"{musicDirectory}{targetChordMidi}"))
        {
            Process.Start("CMD.exe", $"/C cd {pythonDirectory}&{pythonEnv} \"{chordAnalyzePath}\"");
        }

        await chordAnalyzeTask;

        //여기에 MIDI CHORD 프로세싱 함수 넣기
        UnityEngine.Debug.Log($"This line should not show before midi file processing is done.");
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
                    byte[] data_path = Encoding.UTF8.GetBytes(musicDirectory + MusicName);
                    client.Send(data_path);

                    int size = 10;
                    byte[] data = new byte[size];
                    int bytes_read = client.Receive(data, data.Length, SocketFlags.None);

                    string data_received = Encoding.UTF8.GetString(data, 0, bytes_read);
                    int packetLength = int.Parse(data_received);
                    UnityEngine.Debug.Log("packet length: " + data_received);
                    

                    data  = new byte[packetLength];
                    bytes_read = client.Receive(data, data.Length, SocketFlags.None);
                    data_received = Encoding.UTF8.GetString(data, 0, bytes_read);
                    UnityEngine.Debug.Log("packet data: " + data_received);

                    ParsePacket(data_received);

                    //Tempo = float.Parse(data_received);
                    TempoIsAnalyzed = true;

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
    void ParsePacket(string packet)
    {
        string[] packetData = packet.Split(' ');

        GM.Tempo = float.Parse(packetData[0]);

        int beatArrayLength = packetData.Length - 1;
        GM.ChordArray = new GameManager.Chord[beatArrayLength];

        for (int i = 0; i < beatArrayLength; i++)
        {
            //GM.BeatArray[i] = float.Parse(packetData[i + 1]);

            string[] data = packetData[i + 1].Split(',');
            GameManager.Chord chord;
            chord.offset = float.Parse(data[0]);
            chord.pitch = int.Parse(data[1]) - 12;

            GM.ChordArray[i] = chord;
        }
    }

    private void OnDestroy()
    {
        //pythonServer.Kill();
    }
}
