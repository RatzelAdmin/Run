using UnityEngine;
using System.Collections;

public class MicComponent : MonoBehaviour {
	
	public PhaseComponent		phase;
	public RecordComponent		record;
	public bool isSounded = false;
	public bool isTimeOut = false;
	float						micSample = 0;
	
	public AudioSource sound_SoundInput;//oneway
	public AudioSource sound_SoundComplete;//oneway
	public GameObject obj_Record;
	public GameObject obj_Mic;
	public GameObject obj_MicWaiting;
	public GameObject obj_MicComplete;
	public GameObject[] obj_MicLevels;
	bool bComplete = false;
	bool bBlowStart = false;
	float startTime_Blow = 0.0f;
	float durationMic = 0;
	int soundCheckType = 0;
	//IEnumerator Start()
	void Start()
	{
//		record.recLength = 1;
//		record.StartRecord();
//        yield return new WaitForSeconds(0.5f);
//		record.EndRecord();
//        yield return new WaitForSeconds(0.5f);
		//obj_Record.SetActive(false);
	}
	
	void FixedUpdate() {
		
		if( !bComplete )
		{
			if(Microphone.IsRecording(null)) {
				micSample = phase.maxL;
				
				if( soundCheckType == 1 )
				{
					if( micSample > 0.1f &&  micSample <= 0.3f)
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(false);
						obj_MicLevels[2].SetActive(false);
						obj_MicLevels[3].SetActive(false);
					}
					else if( micSample > 0.3f &&  micSample <= 0.5f)
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(true);
						obj_MicLevels[2].SetActive(false);
						obj_MicLevels[3].SetActive(false);
					}
					else if( micSample > 0.5f &&  micSample <= 0.8f)
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(true);
						obj_MicLevels[2].SetActive(true);
						obj_MicLevels[3].SetActive(false);
					}
					else if( micSample > 0.8f )
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(true);
						obj_MicLevels[2].SetActive(true);
						obj_MicLevels[3].SetActive(true);
						bComplete = true;
						StartCoroutine("MicComplete");
						
						if (sound_SoundInput != null) 
							sound_SoundInput.Stop();
					}
				}
				else if( soundCheckType == 2 ) // blow
				{
					if( micSample > 0.05f &&  micSample <= 0.08f)
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(false);
						obj_MicLevels[2].SetActive(false);
						obj_MicLevels[3].SetActive(false);
						bBlowStart = false;
					}
					else if( micSample > 0.08f &&  micSample <= 0.1f)
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(true);
						obj_MicLevels[2].SetActive(false);
						obj_MicLevels[3].SetActive(false);
						bBlowStart = false;
					}
					else if( micSample > 0.1f &&  micSample <= 0.2f)
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(true);
						obj_MicLevels[2].SetActive(true);
						obj_MicLevels[3].SetActive(false);
						
						bBlowStart = false;
					}
					else if( micSample > 0.2f )
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(true);
						obj_MicLevels[2].SetActive(true);
						obj_MicLevels[3].SetActive(true);
						
						if( !bBlowStart )
						{
							bBlowStart = true;
							startTime_Blow = Time.time;
						}
						else
						{
							// blow continue time
							if( Time.time - startTime_Blow > 0.7f )
							{
								bComplete = true;
								StartCoroutine("MicComplete");
								
								if (sound_SoundInput != null) 
									sound_SoundInput.Stop();
							}
						}
						
					}
				}
				else if( soundCheckType == 3 ) // clap
				{
					
					if( micSample > 0.1f &&  micSample <= 0.3f)
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(false);
						obj_MicLevels[2].SetActive(false);
						obj_MicLevels[3].SetActive(false);
					}
					else if( micSample > 0.3f &&  micSample <= 0.5f)
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(true);
						obj_MicLevels[2].SetActive(false);
						obj_MicLevels[3].SetActive(false);
					}
					else if( micSample > 0.5f &&  micSample <= 0.7f)
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(true);
						obj_MicLevels[2].SetActive(true);
						obj_MicLevels[3].SetActive(false);
					}
					else if( micSample > 0.7f )
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(true);
						obj_MicLevels[2].SetActive(true);
						obj_MicLevels[3].SetActive(true);
						bComplete = true;
						StartCoroutine("MicComplete");
						
						if (sound_SoundInput != null) 
							sound_SoundInput.Stop();
					}
				}
				else if( soundCheckType == 4 ) // blow No Delay
				{
					//if( micSample > 0.05f &&  micSample <= 0.2f)
					if( micSample > 0.05f &&  micSample <= 0.08f)
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(false);
						obj_MicLevels[2].SetActive(false);
						obj_MicLevels[3].SetActive(false);
						bBlowStart = false;
					}
					//else if( micSample > 0.2f &&  micSample <= 0.3f)
					else if( micSample > 0.08f &&  micSample <= 0.1f)
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(true);
						obj_MicLevels[2].SetActive(false);
						obj_MicLevels[3].SetActive(false);
						bBlowStart = false;
					}
					//else if( micSample > 0.3f &&  micSample <= 0.4f)
					else if( micSample > 0.1f &&  micSample <= 0.2f)
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(true);
						obj_MicLevels[2].SetActive(true);
						obj_MicLevels[3].SetActive(false);
						
						bBlowStart = false;
					}
					else if( micSample > 0.2f )
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(true);
						obj_MicLevels[2].SetActive(true);
						obj_MicLevels[3].SetActive(true);
						
						if( !bBlowStart )
						{
							bBlowStart = true;
							startTime_Blow = Time.time;
						}
						else
						{
							// blow continue time
							if( Time.time - startTime_Blow > 0.7f )
							{
								bComplete = true;
								isSounded = true;
								StartCoroutine("MicComplete");
								
								if (sound_SoundInput != null) 
									sound_SoundInput.Stop();
							}
						}
						
					}
//					if( micSample > 0.05f &&  micSample <= 0.2f)
//					{
//						obj_MicLevels[0].SetActive(false);
//						obj_MicLevels[1].SetActive(false);
//						obj_MicLevels[2].SetActive(false);
//						obj_MicLevels[3].SetActive(false);
//						bBlowStart = false;
//					}
//					else if( micSample > 0.2f &&  micSample <= 0.3f)
//					{
//						obj_MicLevels[0].SetActive(false);
//						obj_MicLevels[1].SetActive(false);
//						obj_MicLevels[2].SetActive(false);
//						obj_MicLevels[3].SetActive(false);
//						bBlowStart = false;
//					}
//					else if( micSample > 0.3f &&  micSample <= 0.4f)
//					{
//						obj_MicLevels[0].SetActive(false);
//						obj_MicLevels[1].SetActive(false);
//						obj_MicLevels[2].SetActive(false);
//						obj_MicLevels[3].SetActive(false);
//						
//						bBlowStart = false;
//					}
//					else if( micSample > 0.4f )
//					{
//						obj_MicLevels[0].SetActive(false);
//						obj_MicLevels[1].SetActive(false);
//						obj_MicLevels[2].SetActive(false);
//						obj_MicLevels[3].SetActive(false);
//						
//						if( !bBlowStart )
//						{
//							bBlowStart = true;
//							startTime_Blow = Time.time;
//						}
//						else
//						{
//							// blow continue time
//							if( Time.time - startTime_Blow > 1.0f )
//							{
//								bComplete = true;
//								isSounded = true;
//								StartCoroutine("MicComplete");
//								
//								if (sound_SoundInput != null) 
//									sound_SoundInput.Stop();
//							}
//						}
//						
//					}
				}
				else if( soundCheckType == 5 ) // clap No Delay
				{
					
					if( micSample > 0.1f &&  micSample <= 0.3f)
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(false);
						obj_MicLevels[2].SetActive(false);
						obj_MicLevels[3].SetActive(false);
					}
					else if( micSample > 0.3f &&  micSample <= 0.5f)
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(true);
						obj_MicLevels[2].SetActive(false);
						obj_MicLevels[3].SetActive(false);
					}
					else if( micSample > 0.5f &&  micSample <= 0.7f)
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(true);
						obj_MicLevels[2].SetActive(true);
						obj_MicLevels[3].SetActive(false);
					}
					else if( micSample > 0.7f )
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(true);
						obj_MicLevels[2].SetActive(true);
						obj_MicLevels[3].SetActive(true);
						bComplete = true;
						isSounded = true;
						StartCoroutine("MicComplete");
						
						if (sound_SoundInput != null) 
							sound_SoundInput.Stop();
					}
				}
				else if(soundCheckType == 6) // record
				{
					if( micSample > 0.1f &&  micSample <= 0.3f)
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(false);
						obj_MicLevels[2].SetActive(false);
						obj_MicLevels[3].SetActive(false);
					}
					else if( micSample > 0.3f &&  micSample <= 0.5f)
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(true);
						obj_MicLevels[2].SetActive(false);
						obj_MicLevels[3].SetActive(false);
					}
					else if( micSample > 0.5f &&  micSample <= 0.8f)
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(true);
						obj_MicLevels[2].SetActive(true);
						obj_MicLevels[3].SetActive(false);
					}
					else if( micSample > 0.8f )
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(true);
						obj_MicLevels[2].SetActive(true);
						obj_MicLevels[3].SetActive(true);
						//bComplete = true;
						//StartCoroutine("MicComplete");
						
						if (sound_SoundInput != null) 
							sound_SoundInput.Stop();
					}
				}
				else
				{
					if( micSample > 0.1f &&  micSample <= 0.3f)
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(false);
						obj_MicLevels[2].SetActive(false);
						obj_MicLevels[3].SetActive(false);
					}
					else if( micSample > 0.3f &&  micSample <= 0.5f)
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(true);
						obj_MicLevels[2].SetActive(false);
						obj_MicLevels[3].SetActive(false);
					}
					else if( micSample > 0.5f &&  micSample <= 0.8f)
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(true);
						obj_MicLevels[2].SetActive(true);
						obj_MicLevels[3].SetActive(false);
					}
					else if( micSample > 0.8f )
					{
						obj_MicLevels[0].SetActive(true);
						obj_MicLevels[1].SetActive(true);
						obj_MicLevels[2].SetActive(true);
						obj_MicLevels[3].SetActive(true);
						bComplete = true;
						StartCoroutine("MicComplete");
						
						if (sound_SoundInput != null) 
							sound_SoundInput.Stop();
					}
				}
				
			}
		}
		
		
	}
	
	
    IEnumerator MicComplete() {
        yield return new WaitForSeconds(0.2f);
		EndMic();
        yield return new WaitForSeconds(1.0f);
		isTimeOut = false;
		isSounded = true;
		
		if (sound_SoundComplete != null) 
			sound_SoundComplete.Play();
		obj_MicWaiting.SetActive(false);
		obj_Mic.SetActive(false);
//		obj_MicComplete.SetActive(true);
//		obj_MicComplete.SendMessage("StartEffect",SendMessageOptions.DontRequireReceiver);
		
    }
    IEnumerator MicCompleteTimeOut() {
        yield return new WaitForSeconds(0.2f);
		EndMic();
        yield return new WaitForSeconds(1.0f);
		isTimeOut = true;
		isSounded = true;
		
		if (sound_SoundComplete != null) 
			sound_SoundComplete.Play();
		obj_MicWaiting.SetActive(false);
		obj_Mic.SetActive(false);
//		obj_MicComplete.SetActive(true);
//		obj_MicComplete.SendMessage("StartEffect",SendMessageOptions.DontRequireReceiver);
		
    }
	
    IEnumerator MicTimeOut() {
        yield return new WaitForSeconds(0.5f);

		record.StartRecord();
		
        yield return new WaitForSeconds(durationMic);
		StartCoroutine("MicCompleteTimeOut");
		bComplete = true;
    }
	
	
	public void StartSoundSenser(int type, float duration)
	{
		soundCheckType = type;
		isTimeOut = false;
		//Debug.Log ("soundCheckType " + soundCheckType );
		//obj_Record.SetActive(true);
		isSounded = false;
		bComplete = false;
		
		if( duration == 0 )
			duration = 5;
		durationMic = duration;
		//record.recLength = (int)durationMic;
		micSample = 0;
		
		obj_Mic.SetActive(true);
		
//		if( soundCheckType == 4 )
//			obj_MicWaiting.SetActive(false);
//		else
			obj_MicWaiting.SetActive(true);
		
		obj_MicLevels[0].SetActive(false);
		obj_MicLevels[1].SetActive(false);
		obj_MicLevels[2].SetActive(false);
		obj_MicLevels[3].SetActive(false);
		obj_MicComplete.SetActive(false);
		if (sound_SoundInput != null) 
			sound_SoundInput.Play();
		
		StartCoroutine("MicTimeOut");
		
//		record.recLength = 1;
//		record.StartRecord();
	}
	
	public void EndMic()
	{
		record.EndRecord();
		obj_MicLevels[0].SetActive(false);
		obj_MicLevels[1].SetActive(false);
		obj_MicLevels[2].SetActive(false);
		obj_MicLevels[3].SetActive(false);
		
		obj_MicWaiting.SetActive(false);
//		if( soundCheckType == 4 )
//			obj_MicComplete.SetActive(false);
//		else
//		{
			obj_MicComplete.SetActive(true);
			obj_MicComplete.SendMessage("StartEffect",SendMessageOptions.DontRequireReceiver);
//		}
		//obj_Record.SetActive(false);
		
		
		StopCoroutine("MicTimeOut");
	}
	
}
