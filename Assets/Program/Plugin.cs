using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class Plugin : MonoBehaviour {
	const string bundleId = "com.doralab.doraplugin";
	[HideInInspector]
	public bool isSounded = false;
	float timeSoundEnd;
//	float[] thresholds = { 1, 5, 2, 10 };
//	float[] samples = { 0, 0.8f, 1, 0 };
	public bool soundDetecting = false;
	public Texture2D texSoundInput;
	public AudioSource sound_SoundInput;//oneway
	public AudioSource sound_SoundComplete;//oneway
	float soundDetectedTimer = 0;
	float screenScale;
	public bool PlayInputSound = false;
	
#if UNITY_ANDROID
	AndroidJavaObject soundObj = null;
#elif UNITY_IPHONE
	[DllImport ("__Internal")]
	private static extern void _InitSoundSenser ();
	[DllImport ("__Internal")]
	private static extern void _StartSoundSenser (int type, float threashold, float time);
	[DllImport ("__Internal")]
	private static extern void _StopSoundSenser ();
#endif
	
	public void InitSoundSenser() {
		switch (Application.platform) {
#if UNITY_ANDROID
		case RuntimePlatform.Android:
			break;
#elif UNITY_IPHONE
		case RuntimePlatform.IPhonePlayer:
			_InitSoundSenser();
			break;
#endif			
		default:
			break;
		}		
	}
	
	public void StartSoundSenser(int type, float duration) {
		StopSoundSenser();
		
		// BGM 볼륨 줄이기 (13.09.12 박종권)
		AudioListener.volume = 0.6f;
		
		isSounded = false;
		timeSoundEnd = Time.time + duration;
		
		switch (Application.platform) {
#if UNITY_ANDROID
		case RuntimePlatform.Android:
			soundObj = new AndroidJavaObject(bundleId + ".SoundPlugin");
			if (soundObj != null) {
				soundDetecting = true;
			//	soundObj.Call ("start", new object[] { type, thresholds[type], samples[type] });
			}
			break;
#elif UNITY_IPHONE
		case RuntimePlatform.IPhonePlayer:
			soundDetecting = true;
			//_StartSoundSenser(type, thresholds[type], samples[type]);
			break;
#endif			
		default:
			Sound("Not Supported");
			break;
		}
	}

	public void StopSoundSenser() {
		soundDetecting = false;
		
		//사용자 음성 입력을 다 받았으면, BGM 볼륨 원래데로 (13.09.12 박종권)
		AudioListener.volume = 1.0f;

			
		switch (Application.platform) {
#if UNITY_ANDROID
		case RuntimePlatform.Android:
			if (soundObj != null) {
				soundObj.Call("stop");
				soundObj = null;
			}
			break;
#elif UNITY_IPHONE
		case RuntimePlatform.IPhonePlayer:
			_StopSoundSenser();
			break;
#endif
		default:
			break;
		}
	}
	
	// Use this for initialization
	void Start () {
		InitSoundSenser();
		
		screenScale = Global.GetScreenScaleUI();
		
	}
	void OnDestroy() {
		StopSoundSenser();
	}
	
	// Update is called once per frame
	void Update () {
		if (soundDetecting && Time.time >= timeSoundEnd) {
			Sound("Expired");
		}
		
		if (soundDetectedTimer > 0)
			soundDetectedTimer -= Time.deltaTime;
	}
	
	void OnGUI() {
		if (texSoundInput != null && (soundDetecting || soundDetectedTimer > 0)) {
			
			if( soundDetecting )
			{
				if (soundDetectedTimer > 0)
				{
					GUI.color = Color.yellow;
					Global.DrawInputIcon(texSoundInput, screenScale*1.1f);
				}
				else
					Global.DrawInputIcon(texSoundInput, screenScale);
			}
			if( !PlayInputSound )
			{
				//StartSoundInputSound();
				PlayInputSound = true;
			}
		}
	}
				
	void Sound(string param) {
		isSounded = true;
		if (param == "Sound")
		{
			soundDetectedTimer = 1f;
		}
		EndSoundInputSound();
		StopSoundSenser();
		Debug.Log (param);
		
	}
	
	public void StartSoundInputSound()
	{
		if (sound_SoundInput != null) 
		{
			if( !sound_SoundInput.isPlaying )
				sound_SoundInput.Play();
			//AudioSource.PlayClipAtPoint(sound_SoundInput, Vector3.zero);
		}
	}
	public void EndSoundInputSound()
	{
		if (sound_SoundComplete != null) 
		{
			if( !sound_SoundComplete.isPlaying )
				sound_SoundComplete.Play();
			//AudioSource.PlayClipAtPoint(sound_SoundComplete, Vector3.zero);
		}
		
		PlayInputSound = false;
	}
}
