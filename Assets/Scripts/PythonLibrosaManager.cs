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

public class PythonLibrosaManager : MonoBehaviour
{
    public string connectionIP = "127.0.0.1";
    public int connectionPort = 9999;
    public string MusicFilePath;
    public float Tempo;
    public bool TempoIsAnalyzed = false;

    Thread mThread;
    string full_path;
    string pythonEnv;

    // Start is called before the first frame update
    void Start()
    {
        Tempo = 0;
        full_path = Application.dataPath + "/Scripts/librosaAnalyzer.py";

        // 김기오 : 전용 파이썬 환경 경로
        pythonEnv = @"E:\Programs\Python\Anaconda\envs\librosa\python.exe";
        if (!File.Exists(pythonEnv))
        {
            pythonEnv = "python";
        }
        StartPythonLibrosaServer(full_path);

        GetLibrosa();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void StartPythonLibrosaServer(string file_path)
    {
        Process python_server = new Process();

        file_path = "\"" + file_path + "\"";
        UnityEngine.Debug.Log(pythonEnv + " " + file_path);
        python_server.StartInfo = new ProcessStartInfo(pythonEnv, file_path)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        python_server.Start();
    }

    void GetLibrosa()
    {
        using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
            while (true)
            {
                try
                {
                client.Connect(new IPEndPoint(IPAddress.Parse(connectionIP), connectionPort));
                byte[] data_path = Encoding.UTF8.GetBytes(Application.dataPath + MusicFilePath);
                client.Send(data_path);
                byte[] data = new byte[8];
                int bytes_read = client.Receive(data, data.Length, SocketFlags.None);
                string data_received = Encoding.UTF8.GetString(data, 0, bytes_read);
                    UnityEngine.Debug.Log("data_recieved " + data_received);
                Tempo = float.Parse(data_received);
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

            client.Close();
        }
    }
}
