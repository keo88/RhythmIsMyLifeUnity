using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Scripting.Python;
using System.Threading;
using System.Net;
using System.Text;
using System.Net.Sockets;

public class PythonManager : MonoBehaviour
{
    public string connectionIP = "127.0.0.1";
    public int connectionPort = 9999;
    public string MusicFilePath = "Assets/MusicSample/Canon.wav";
    public int Tempo = 0;
    public bool TempoIsAnalyzed = false;

    Thread mThread;
    IPAddress localAdd;
    TcpListener listener;
    TcpClient client;

    bool has_sent_info = false;

    private void Start()
    {
        ThreadStart ts = new ThreadStart(GetInfo);
        mThread = new Thread(ts);
        mThread.Start();

        string path = Application.dataPath + "/Scripts/log_names.py";
        PythonRunner.RunFile(path);
    }

    void GetInfo()
    {
        localAdd = IPAddress.Parse(connectionIP);
        listener = new TcpListener(IPAddress.Any, connectionPort);
        listener.Start();

        client = listener.AcceptTcpClient();

        while (true)
        {
            SendAndReceiveData();
        }
    }

    void SendAndReceiveData()
    {
        NetworkStream nwStream = client.GetStream();

        if (!has_sent_info && client.Connected && nwStream.CanWrite)
        {
            has_sent_info = true;
            byte[] StrByte = Encoding.UTF8.GetBytes(MusicFilePath);
            nwStream.Write(StrByte, 0, StrByte.Length);
        }

        
        byte[] buffer = new byte[client.ReceiveBufferSize];

        // reveiving Data from the Host
        int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
        string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        if (dataReceived != null && dataReceived != "")
        {
            Debug.Log(dataReceived);
            Tempo = int.Parse(dataReceived);
            TempoIsAnalyzed = true;
            byte[] myWirteBuffer = Encoding.ASCII.GetBytes("Got message!");
            nwStream.Write(myWirteBuffer, 0, myWirteBuffer.Length);
        }

    }
}
