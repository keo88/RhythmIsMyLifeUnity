import socket
import threading
import librosa
import time


def binder(client_socket, addr):
    try:
        received_music_path = client_socket.recv(1024).decode("UTF-8")
        print(received_music_path)
        music_file = received_music_path

        y, sr = librosa.load(music_file)
        tempo, beats = librosa.beat.beat_track(y, sr)
        tempo = str(tempo)

        client_socket.sendall(tempo.encode("UTF-8"))

        received_data = client_socket.recv(1024).decode("UTF-8")
        print('LibrosaAnalyzer : ', received_data)

    except Exception as e:
        print('Client Error : ' + str(e))
    finally:
        client_socket.close()

server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
server_socket.bind(('127.0.0.1', 9999))
server_socket.listen(30)

print('server established on :')

while True:
    try:
        print('listening')
        client_socket, addr = server_socket.accept()
        print('accepted')
        
        th = threading.Thread(target=binder, args=(client_socket, addr))
        th.start()

    except Exception as e:
        print('Acceptance Error : ' + str(e))
        time.sleep(2)
    finally:
        server_socket.close()