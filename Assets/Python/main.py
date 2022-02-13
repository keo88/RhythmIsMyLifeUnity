import socket
import threading
import time
import librosa
import numpy as np
import midi_analyzer as ma

test_mode = False


def tuple_to_string(data):
    return str(data[0]) + ',' + str(data[1])


def beat_packetizing(music_file):
    x, sr = librosa.load(music_file)
    tempo, beats = librosa.beat.beat_track(y=x, sr=sr, start_bpm=60, units='time')
    beat_times = np.round(beats, 2)
    print(f'estimated Tempo : {tempo}')

    return ' '.join(list(map(lambda item: str(item), beat_times)))


def midi_packetizing(base_midi):
    bpm, time_signature = ma.extract_features(base_midi)

    offset_array, pitch_array = ma.extract_beats_array(base_midi)
    offset_list = list(offset_array)
    pitch_list = list(pitch_array)
    ddd = list(zip(offset_list, pitch_list))
    ddd = list(map(tuple_to_string, ddd))

    data_packet = ' '.join(ddd)

    data_packet = str(bpm) + ' ' + data_packet

    return data_packet


def binder(client_socket, address, thread_number):
    try:
        received_music_path = client_socket.recv(1024).decode("UTF-8")
        music_file = received_music_path
        print(f'Thread {thread_number}: Current Music Path: ', received_music_path)

        # Midi processing
        chord_midi_path = received_music_path.replace('.mp3', '').replace('.wav', '') + '-chord.midi'

        base_midi = ma.open_midi(chord_midi_path)
        midi_data_packet = midi_packetizing(base_midi)

        packet_length = str(len(midi_data_packet))
        client_socket.sendall(packet_length.encode("UTF-8"))

        client_socket.sendall(midi_data_packet.encode("UTF-8"))

        # Beat processing
        beat_data_packet = beat_packetizing(music_file)
        packet_length = str(len(beat_data_packet))
        client_socket.sendall(packet_length.encode("UTF-8"))

        client_socket.sendall(beat_data_packet.encode("UTF-8"))

        # Checking process done
        received_data = client_socket.recv(1024).decode("UTF-8")
        print(f'Thread {thread_number}: From client - ', received_data)

    #except Exception as e:
    #    print(f'Thread {thread_number}: Client Error - ' + str(e))

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
    music_file = 'C:/Projects/2022/RhythmIsMyLifeUnity/Assets/MusicSample/Green-Day-Holiday-Lyrics-chord.midi'

    base_midi = ma.open_midi(music_file)

    ma.list_instruments(base_midi)

    offset_array, pitch_array = ma.extract_beats_array(base_midi)

    packet = midi_packetizing(base_midi)
    print(len(packet), packet)

else:
    try:
        server_socket = initialize_socket()
        thread_cnt = 0
        print('Server Established On!')

        while True:
            try:
                print('listening')
                client_socket, address = server_socket.accept()
                print('accepted')

                th = threading.Thread(target=binder, args=(client_socket, address, thread_cnt))
                th.start()
                thread_cnt += 1

            except Exception as e:
                print('Acceptance Error: ' + str(e))
                time.sleep(2)
                break

        server_socket.close()
    except Exception as e:
        f = open('log.txt', 'w')
        f.write(str(e))