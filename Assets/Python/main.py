import socket
import threading
import time
from music21 import *
import numpy as np
import midi_analyzer as ma

test_mode = False


def packetizing(base_midi):
    bpm, time_signature = ma.extract_features(base_midi)

    beats_array = ma.extract_beats_array(base_midi)
    beats_list = list(map(str, beats_array))

    beats_line = ' '.join(beats_list)

    data_packet = str(bpm) + ' ' + beats_line

    return data_packet


def binder(client_socket, address):
    try:
        received_music_path = client_socket.recv(1024).decode("UTF-8")
        print('Current Music Path: ', received_music_path)
        music_file = received_music_path

        base_midi = ma.open_midi(music_file)

        data_packet = packetizing(base_midi)
        # print(len(data_packet), data_packet)

        packet_length = str(len(data_packet))
        client_socket.sendall(packet_length.encode("UTF-8"))
        data_packet = data_packet
        client_socket.sendall(data_packet.encode("UTF-8"))

        received_data = client_socket.recv(1024).decode("UTF-8")
        print('Python Server: ', received_data)

    except Exception as e:
        print('Client Error: ' + str(e))
        print()
    finally:
        client_socket.close()


def initialize_socket():
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    server_socket.bind(('127.0.0.1', 9999))
    server_socket.listen(30)

    return server_socket


if test_mode:
    print('Start Test Mode ... ')
    music_file = 'C:/Projects/2022/RhythmIsMyLifeUnity/Assets/MusicSample/Holiday.mid'

    base_midi = ma.open_midi(music_file)

    ma.list_instruments(base_midi)

    beat_array = ma.extract_beats_array(base_midi)

    print(len(beat_array))

    packet = packetizing(base_midi)
    print(len(packet), packet)

else:
    server_socket = initialize_socket()
    print('Server Established On!')

    while True:
        try:
            print('listening')
            client_socket, address = server_socket.accept()
            print('accepted')

            th = threading.Thread(target=binder, args=(client_socket, address))
            th.start()

        except Exception as e:
            print('Acceptance Error: ' + str(e))
            time.sleep(2)
        finally:
            server_socket.close()
