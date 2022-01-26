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
    private Thread mThread;
    private string pythonScriptPath;
    private string pythonEnv;
    private Process pythonServer;


    public void RunPythonServer()
    {
        Tempo = 0;

        pythonScriptPath = Application.dataPath + "/Python/main.py";
        pythonEnv = Application.dataPath + "/Python/venv/Scripts/python.exe";

        pythonServer = new Process();
        UnityEngine.Debug.Log("Run Python Server: " + pythonEnv + " " + pythonScriptPath);

        pythonServer.StartInfo = new ProcessStartInfo(pythonEnv, pythonScriptPath)
        {
            UseShellExecute = true,
            CreateNoWindow = false,
        };

        pythonServer.Start();

        GetData();

        pythonServer.Kill();
    }


    void ParsePacket(string packet)
    {
        string[] packetData = packet.Split(' ');

        GM.Tempo = float.Parse(packetData[0]);

        int beatArrayLength = packetData.Length - 1;
        GM.BeatArray = new float[beatArrayLength];
        for (int i = 0; i < beatArrayLength; i++)
        {
            GM.BeatArray[i] = float.Parse(packetData[i+1]);
        }
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
                    byte[] data_path = Encoding.UTF8.GetBytes(Application.dataPath + "/MusicSample/" + MusicName);
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

            client.Close();
        }

        UnityEngine.Debug.Log("get data");
    }

    private void OnDestroy()
    {
        //pythonServer.Kill();
    }
}
