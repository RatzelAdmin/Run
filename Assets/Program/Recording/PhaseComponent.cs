using UnityEngine;
using System.Collections;

public class PhaseComponent : MonoBehaviour {
	// phase
	[HideInInspector]
	public int 		readHead = 0;
	[HideInInspector]
	public float	maxL     = 0;
	
	float[] 		buffer;
	int     		toGet;
	
	void FixedUpdate() {
		if(Microphone.IsRecording(null)) {
			int writeHead = Microphone.GetPosition(null);
			toGet = (audio.clip.samples + writeHead - readHead ) % audio.clip.samples;
			
			if(toGet > 0) {
				float[] buffer = new float [toGet];
				audio.clip.GetData(buffer, readHead);
				
				maxL = float.MinValue;
				for(int i = 0; i < toGet; i++) {
					float v = Mathf.Abs(buffer[i]);
					if(v > maxL) {
						maxL = v;
					}
				}
				//Debug.Log (writeHead + " / " + readHead + " / " + buffer[0]);
				
				readHead = (readHead + toGet) % audio.clip.samples;
			}
			else
			{
				maxL = 0;
			}
		}
	}
}
