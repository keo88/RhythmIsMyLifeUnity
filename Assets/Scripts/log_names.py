import UnityEngine as ue
import socket
import librosa

ue.Debug.Log("log_names start!")

host, port = "127.0.0.1", 9999
socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

socket.connect((host, port))

receivedMusicPath = socket.recv(1024).decode("UTF-8")
print(receivedMusicPath)

music_file = receivedMusicPath
file_split = False

y, sr = librosa.load(music_file)
tempo, beats = librosa.beat.beat_track(y, sr)
tempo = str(int(tempo))

socket.sendall(tempo.encode("UTF-8"))
receivedData = socket.recv(1024).decode("UTF-8")
print(receivedData)

# objects = ue.Object.FindObjectsOfType(ue.GameObject)
# for go in objects:
#     # ue.Debug.Log(go.name)
#     if go.name == "PythonManager":
#         ue.GameObject pythonManager
#         python_manager pyScript = pythonManager.GetComponent<python_manager>()
#         pyScript.tempo = 20
#         ue.Debug.Log("this is python manager!")