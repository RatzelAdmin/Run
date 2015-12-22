using UnityEngine;
using System.Collections;

public class RecordComponent : MonoBehaviour {
	
	public PhaseComponent	phase;
	public AudioSource		MicrophoneAudio;
	public int				recLength = 1;
	
	void OnDisable() {
		if(Microphone.IsRecording(null)) {
			Microphone.End (null);
		}
		
		if(MicrophoneAudio.isPlaying)
			MicrophoneAudio.Stop ();
	}
	
	public void StartRecord() {
		phase.readHead = 0;
		
		//MicrophoneAudio.clip   = Microphone.Start (null, true, recLength, 11025); 	
		//MicrophoneAudio.clip   = Microphone.Start (null, true, recLength, 5512); 		
		MicrophoneAudio.clip   = Microphone.Start (null, true, recLength, 11025); 	
		MicrophoneAudio.pitch  = 1;
		MicrophoneAudio.mute   = true;
		MicrophoneAudio.volume = 1.0f;
	}
	
	public void EndRecord() {
		Microphone.End (null);

	}
}
