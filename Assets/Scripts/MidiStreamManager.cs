using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;


public class MidiStreamManager : MonoBehaviour
{
    public MidiStreamPlayer midiStreamPlayer;

    private MPTKEvent NotePlaying;


    private void Start()
    {
        midiStreamPlayer = FindObjectOfType<MidiStreamPlayer>();

        
        // midiStreamPlayer.MPTK_PlayEvent(new MPTKEvent() { Command = MPTKCommand.ControlChange, Controller = MPTKController.BankSelectMsb, Value = index, Channel = StreamChannel, });

        NotePlaying = new MPTKEvent()
        {
             Command = MPTKCommand.NoteOn, // midi command
             Value = 48, // from 0 to 127, 48 for C4, 60 for C5, ...
             Channel = 10, // from 0 to 15, 9 reserved for drum
             Duration = 1000, // note duration in millisecond, -1 to play undefinitely, MPTK_StopChord to stop
             Velocity = 100, // from 0 to 127, sound can vary depending on the velocity
             Delay = 0, // delay in millisecond before playing the note
        };

        if (midiStreamPlayer == null)
            Debug.LogWarning("Can't find a MidiStreamPlayer Prefab in the current Scene Hierarchy. Add it with the MPTK menu.");

    }

    public void PlayNote(int value, int duration)
    {
        midiStreamPlayer.MPTK_StopEvent(NotePlaying);

        bool ret = midiStreamPlayer.MPTK_ChannelPresetChange(10, 30, 8);

        NotePlaying.Value = value;
        NotePlaying.Duration = duration;

        midiStreamPlayer.MPTK_PlayEvent(NotePlaying);
    }

    public void StopNote()
    {
        midiStreamPlayer.MPTK_StopEvent(NotePlaying);
    }

    public void ChangeChannel(int channel)
    {
        NotePlaying.Value = channel;
    }


}
