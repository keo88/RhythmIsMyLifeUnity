from music21 import *
import numpy as np

def open_midi(midi_path):
    mf = midi.MidiFile()
    mf.open(midi_path)
    mf.read()
    mf.close()

    return midi.translate.midiFileToStream(mf)


def list_instruments(midi):
    part_stream = midi.parts.stream()
    print("List of instruments found on MIDI file:")
    for p in part_stream:
        print(p.partName)


def extract_notes(midi_part):
    parent_element = []
    ret = []
    for nt in midi_part.flat.notes:
        if isinstance(nt, note.Note):
            ret.append(max(0.0, nt.pitch.ps))
            parent_element.append(nt)
        elif isinstance(nt, chord.Chord):
            for pitch in nt.pitches:
                ret.append(max(0.0, pitch.ps))
                parent_element.append(nt)

    return ret, parent_element


def extract_beats_array(midi):
    beats_array = None

    for i in range(len(midi.parts)):
        if midi.parts.stream()[i].partName == 'Billie Joe Armstrong (Lead Guitar)':
            top = midi.parts[i].flat.notes
            y, parent_element = extract_notes(top)
            beats_array = [n.offset for n in parent_element]

    beats_array = np.array(beats_array)
    beats_array = np.unique(beats_array)
    return beats_array


def extract_features(midi):
    measurement = midi.parts.stream()[0].measure(1)

    time_signature = None
    bpm = None

    for el in measurement.iter().getElementsByClass(meter.TimeSignature):
        time_signature = el.ratioString

    for el in measurement.iter().getElementsByClass(tempo.MetronomeMark):
        bpm = el.number

    return bpm, time_signature