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
    print("End of instruments found on MIDI file")


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
    offset_array = []
    pitch_array = []


    for i in range(len(midi.parts)):
        if midi.parts.stream()[i].partName == 'Piano':
            top = midi.parts[i].flat.notes
            y, parent_element = extract_notes(top)

            unique_offset = -1
            for n in parent_element:
                if unique_offset != n.offset:
                    unique_offset = n.offset

                    offset_array.append(float(n.offset))
                    pitch_array.append(int(n.pitches[0].ps))

    return offset_array, pitch_array


def extract_features(midi):
    measurement = midi.parts.stream()[0].measure(1)

    time_signature = None
    bpm = None

    for el in measurement.iter().getElementsByClass(meter.TimeSignature):
        time_signature = el.ratioString

    for el in measurement.iter().getElementsByClass(tempo.MetronomeMark):
        bpm = el.number

    return bpm, time_signature