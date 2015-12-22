/*
Cutscene runtime manager
Written by Jong Lee
11/12/2012
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

public class CutsceneMgr : MonoBehaviour {
	[System.Serializable]
	public class Dialog {
		public Dialog(AudioClip c, string a) {
			clip = c;
			caption = a;
		}
		
		public AudioClip clip;
		public string caption;
	}
	[System.Serializable]
	public class BGM {
		public BGM(AudioClip c) {
			clip = c;
		}
		
		public AudioClip clip;
	}
	
	[HideInInspector]
	public List<CutsceneStep> steps;
/*
	class StepDone {
		public StepDone(int id, float time) {
			stepId = id;
			done = time;
		}
		
		public int stepId;
		public float done;
	}
*/
	List<CutsceneStep> progSteps;
	Dictionary<int, float> doneSteps;
	List<CutsceneStep> stepsToRun;
	
	//public Transform prePlugin;
	public CutsceneMgr[] nextCutscenes;
	public DialogData dialogData;
	public BGMData BGMData;
	//MicComponent plugin;
	
//	public AudioClip testSound;
//	public AudioSource testAudioSrc;
#if UNITY_EDITOR
	public string note;
#endif
	
	Dialog[][] dialogs;
	BGM[][] bgms;
	float timeElapsed = 0;
	LcChoice choice = null;
	public float talkPostDelay = 0.5f;
	//Global gbl;
	GameObject QuickMenu;
	
	// Use this for initialization
	void Start() {
		QuickMenu = GameObject.FindGameObjectWithTag("QuickMenu");
		//plugin = FindObjectOfType(typeof(MicComponent)) as MicComponent;
		if (QuickMenu == null) {
//			if (prePlugin != null) {
//				Transform obj = Instantiate(prePlugin) as Transform;
//				obj.name = "Plugin";
//				DontDestroyOnLoad(obj);
//				plugin = obj.GetComponent<MicComponent>();
//			} else {
    /*
#if EnglishVersion 
				Transform obj = (Transform)Instantiate(Resources.Load("Plugin_Quick_English", typeof(Transform))) ;
#elif TaiwanVersion
				Transform obj = (Transform)Instantiate(Resources.Load("Plugin_Quick_Taiwan", typeof(Transform))) ;
#else
				Transform obj = (Transform)Instantiate(Resources.Load("Plugin_Quick_Korea", typeof(Transform))) ;
#endif
*/
				// obj.name = "Plugin";
				// //DontDestroyOnLoad(obj);
				// plugin = obj.GetComponent<MicComponent>();
				// gbl = obj.GetComponent<Global>();
//			}
		}
		else
		{
			//plugin = QuickMenu.GetComponent<MicComponent>();
			//gbl = QuickMenu.GetComponent<Global>();
		}
		
		
		
		GameObject setup = GameObject.Find ("_SceneSetup");
		if (setup == null) {
			setup = new GameObject("_SceneSetup");

			string resumeCutscene = Global.GetScene();
			//Debug.Log ( "CutSceneMgr ResumeCutscene = "+ resumeCutscene);
			if (resumeCutscene != null) {
				CutsceneMgr[] objs = Resources.FindObjectsOfTypeAll(typeof(CutsceneMgr)) as CutsceneMgr[];
				foreach (CutsceneMgr mgr in objs) {
					//Debug.Log ( "CutSceneMgr Name = "+ mgr.name);
					if (mgr.name == resumeCutscene) {
						mgr.gameObject.SetActive(true);
					} else {
						mgr.gameObject.SetActive(false);
					}
				}
			}
		}
		
//		Debug.Log ("Cutscene GameObject "+this.gameObject);
//		Debug.Log ("dialogData "+dialogData);
		if (dialogData != null) {
			dialogs = new Dialog[2][];
			//Debug.Log ("dialogs "+dialogs);
			for (int i = 0; i < 2; i++) {
				dialogs[i] = dialogData.GetDialogs(i);
				//Debug.Log ("dialogs["+i+"].Length= "+dialogs[i].Length);

			}
		}
		if (BGMData != null) {
			bgms = new BGM[2][];
			for (int i = 0; i < 2; i++) {
				bgms[i] = BGMData.GetBGMs(i);
			}
		}
		Restart();
	}
	
	void Restart() {	// restart the cutscene
		timeElapsed = 0;
		progSteps = new List<CutsceneStep>();
		doneSteps = new Dictionary<int, float>();
		stepsToRun = new List<CutsceneStep>(steps);
	}
	
	// Update is called once per frame
	void Update () {
		timeElapsed += Time.deltaTime;
		
		int doneChoiceTouch = 0;
		for (int i = 0; i < progSteps.Count; ) {
			CutsceneStep step = progSteps[i];
			
#if UNITY_EDITOR
			step.bCurrentStep = true;
#endif
			if (IsDone (step)) {
				if ( step.action == CutsceneStep.Action.WaitTouch ) 
				{
					if( step.boolVal )// Send GameLog
					{
						World.SendGameLog("T",step.intVal2,true );//type, id, bSuccess
					}
					
					if( step.intVal > 0 )
					{
						SetBool (step.intVal, step.actor.name);
						doneChoiceTouch = step.intVal;
					}
					
				}
				else if( step.action == CutsceneStep.Action.WaitSound )
				{
					if( step.boolVal )// Send GameLog
					{
                        /*
						if( !plugin.isTimeOut )
							World.SendGameLog("V",step.intVal2,true );//type, id, bSuccess
						else
							World.SendGameLog("V",step.intVal2,false );//type, id, bSuccess
                         */
					}
				}
				
#if UNITY_EDITOR
				step.bCurrentStep = false;
#endif
				doneSteps.Add (step.stepId, timeElapsed);
				progSteps.RemoveAt(i);
			} else
				i++;
		}
		
		for (int i = 0; i < progSteps.Count; ) {
			CutsceneStep step = progSteps[i];
			
			if (step.action == CutsceneStep.Action.WaitTouch) {
				if (doneChoiceTouch > 0 && step.intVal == doneChoiceTouch) {
					progSteps.RemoveAt(i);
					step.actor.CancelWaitTouch();
				} else {
					//gbl.waitingTouch = true;
					i++;
				}
			} else {
				i++;
			}
		}
		
	
	}
	void FixedUpdate () {
		
		if (stepsToRun.Count == 0 && progSteps.Count == 0) {Debug.Log("end");
			EndCutscene();
			return;
		}
		
		for (int i = 0; i < stepsToRun.Count; ) {
			CutsceneStep step = stepsToRun[i];
			bool found = false;
			
			if (step.preId == 0) {
				if (step.delay <= timeElapsed) {
					found = true;
				}
			} else {
				if (doneSteps.ContainsKey(step.preId)) {
					float doneTime = doneSteps[step.preId];
					float delay = step.delay;
					if (step.action == CutsceneStep.Action.TalkTo || step.action == CutsceneStep.Action.SpeechTo)
						delay += talkPostDelay;
					if (doneTime + delay <= timeElapsed) {
						found = true;
					}
				}
			}
			
			if (found) {
				if (step.action == CutsceneStep.Action.EndCutscene) {
					EndCutscene();
					return;
				}

				if (DoStep(step))
					progSteps.Add(step);
				stepsToRun.RemoveAt(i);
			} else
				i++;
		}

//		int doneChoiceTouch = 0;
//		for (int i = 0; i < progSteps.Count; ) {
//			CutsceneStep step = progSteps[i];
//			
//#if UNITY_EDITOR
//			step.bCurrentStep = true;
//#endif
//			if (IsDone (step)) {
//				if (step.action == CutsceneStep.Action.WaitTouch && step.intVal > 0) {
//					SetBool (step.intVal, step.actor.name);
//					doneChoiceTouch = step.intVal;
//				}
//				
//#if UNITY_EDITOR
//				step.bCurrentStep = false;
//#endif
//				doneSteps.Add (step.stepId, timeElapsed);
//				progSteps.RemoveAt(i);
//			} else
//				i++;
//		}
//		
//		for (int i = 0; i < progSteps.Count; ) {
//			CutsceneStep step = progSteps[i];
//			
//			if (step.action == CutsceneStep.Action.WaitTouch) {
//				if (doneChoiceTouch > 0 && step.intVal == doneChoiceTouch) {
//					progSteps.RemoveAt(i);
//					step.actor.CancelWaitTouch();
//				} else {
//					gbl.waitingTouch = true;
//					i++;
//				}
//			} else {
//				i++;
//			}
//		}
	}
	
	public static string GetAssetName(int id) {
		return string.Format("Cutscene/Cutscene{0}", id);
	}
	
	public bool DoStep(CutsceneStep step) {
		if (step == null) {
			Debug.Log ("DoStep step "+step);
			return true;
		}

		if (step.action == CutsceneStep.Action.WaitTouch && step.intVal > 0) {
			if (IsBool(step.intVal, step.actor.name)) {
				step.actor.WaitTouch(0, false);
				return false;
			}
		}
		
		Debug.Log ("DoStep step.action "+step.action);
		switch (step.action) {	
		case CutsceneStep.Action.GameLog://GameStart, GameEnd, ChapterStart, ChapterEnd
			if( step.intVal == 0 )//GameStart
			{
				World.SendGameLog("G",step.intVal2,true );//type, id, bSuccess
			}
			else if( step.intVal == 1 )//GameEnd
			{
				World.SendGameLog("G",step.intVal2,false );//type, id, bSuccess
			}
			else if( step.intVal == 2 )//ChapterStart
			{
				World.EpisodeChapterState( true );
			}
			else if( step.intVal == 3 )//ChapterEnd
			{
				World.EpisodeChapterState( false );
			}
			break;
		case CutsceneStep.Action.Caption:
			if( World.langCaptionIdx != -1 )
			{
				if (step.intVal > 0 && dialogs[World.langCaptionIdx].Length > step.intVal) {
					Debug.Log ("======World.langCaptionIdx = " + World.langCaptionIdx);
					//gbl.DisplaySubtitle(dialogs[World.langCaptionIdx][step.intVal].caption, step.actor);
				}
			}
			break;
		case CutsceneStep.Action.TalkTo:
		case CutsceneStep.Action.SpeechTo:

			if (step.intVal2 > 0)
				step.actor.Action(step.intVal2, false ,null);
			if( World.langCaptionIdx != -1 )
			{
//				Debug.Log ("World.langCaptionIdx "+World.langCaptionIdx);
//				Debug.Log ("step.intVal "+step.intVal);
//				Debug.Log ("dialogs "+dialogs);
				if( dialogs != null )
				{
					if (step.intVal > 0 && dialogs[World.langCaptionIdx].Length > step.intVal) {
						
//						Debug.Log ("gbl.DisplaySubtitle ");
						Debug.Log ("======World.langCaptionIdx = " + World.langCaptionIdx);
						//gbl.DisplaySubtitle(dialogs[World.langCaptionIdx][step.intVal].caption, step.actor);
						if (step.floatVal == 0)
						{
							if( World.langIdx != -1 )
							{
//								Debug.Log ("World.langIdx "+World.langIdx);
//								Debug.Log ("step.intVal "+step.intVal);
								if (step.intVal > 0 && dialogs[World.langIdx].Length > step.intVal) 
									step.actor.TalkTo(dialogs[World.langIdx][step.intVal].clip, step.target, step.action);
							}
						}
					}
				}
			}
			else
			{
				if (step.intVal > 0 ) {
					if (step.floatVal == 0)
						step.actor.TalkTo(dialogs[World.langIdx][step.intVal].clip, step.target, step.action);
				}
			}
			if (step.floatVal > 0) {
//				Debug.Log ("step.actor.LipTo ");
				step.actor.LipTo(step.floatVal, step.target, step.action);
			}
			step.actor.NextLoopAction(step.boolVal);
			break;
			
		case CutsceneStep.Action.Action:
			AudioClip clip = null;
			if (step.intVal2 > 0 && dialogs[World.langIdx].Length > step.intVal2)
				clip = dialogs[World.langIdx][step.intVal2].clip;
			step.actor.Action(step.intVal,step.boolVal2, clip);
			step.actor.NextLoopAction(step.boolVal);
			break;
			
		case CutsceneStep.Action.LoadScene:
			//World.loadedLevelName = step.strVal;
			
			
			Debug.Log ("CutsceneStep.Action.LoadScene : "+step.strVal);
			//Episode Ending Check
			if( World.loadedLevelName.Contains("_ending") )
				World.EpisodeFinish();
			
			//English Episode Ending Check
			if( step.strVal.Contains("GoToStart") )
			{
				if( World.loadedLevelName.Contains("episode_e") ||World.loadedLevelName.Contains("review_e") )
					World.EpisodeFinish();
				else if( Application.loadedLevelName.Contains("episode_e") || Application.loadedLevelName.Contains("review_e"))
					World.EpisodeFinish();
			}
			if(step.obj != null )
				Global.SetScene(step.obj as CutsceneMgr); Debug.Log("step.obj = "+step.obj);
			World.sceneParam = step.strVal2;Debug.Log("step.strval2 : "+step.strVal2);
			Global.LoadScene(step.strVal, step.intVal);Debug.Log("step.strval : "+step.strVal+", step.intval : "+step.intVal);
			break;
			
		case CutsceneStep.Action.Setup:
			foreach (ActorData data in step.actorData) {
				if (data.actor != null) {
					data.actor.Setup (data.pos, data.rota, data.scale);
				}
			}
			break;
			
		case CutsceneStep.Action.Activate: {
			bool activate = true;
			switch (step.intVal) {
			case 0:
				activate = !step.boolVal2;
				break;
				
			case 1:
				activate = false;
				break;
			}
			
			if (activate) {
				if( step.target == null )
				{
					Debug.LogError("CutsceneStep.Action.Activate Target Null");
				}
				else{
					step.target.gameObject.SetActive(true);
					ActorCam cam = step.target.gameObject.GetComponent<ActorCam>();
					if (cam != null) {
						ActorCam[] objs = FindObjectsOfType(typeof(ActorCam)) as ActorCam[];
						foreach (ActorCam o in objs) {
							if (o != cam)
								o.gameObject.SetActive(false);
						}
					}
				}
			} else
				step.target.gameObject.SetActive(false);
			
			if (step.intVal == 2) {Debug.Log("send message : "+step.strVal+", "+step.strVal2);
				Debug.Log ("step.target "+step.target);
				if(!step.target.gameObject.activeSelf )
					step.target.gameObject.SetActive(true);
				step.target.SendMessage(step.strVal, step.strVal2, SendMessageOptions.DontRequireReceiver);
			}
		}
			break;
			
		case CutsceneStep.Action.Proc:
			if( step.strVal == "BeginLive" )
			{
				GameObject intro = GameObject.Find("IntroSystem");
				if( intro != null )
					intro.SendMessage(step.strVal, step.strVal2);
				else
					SendMessage(step.strVal, step.strVal2);
			}
			else
				SendMessage(step.strVal, step.strVal2);
			break;
			
		case CutsceneStep.Action.PlayMovie:
			/*
			if( step.strVal.Contains("DLA_DummyOpenning" ) )
				DoralabAPI.AddBgmPlayCount(0);

			Handheld.PlayFullScreenMovie(step.strVal, Color.black
				, step.boolVal ? FullScreenMovieControlMode.CancelOnInput : FullScreenMovieControlMode.Hidden);
			*/
			break;
			
		case CutsceneStep.Action.WaitTouch:
			//gbl.StartWaitTouchSound();
			break;
		case CutsceneStep.Action.WaitSound:
			//plugin.StartSoundInputSound();
			//plugin.StartSoundSenser(step.intVal, step.floatVal == 0 ? float.MaxValue : step.floatVal);
			break;
			
		case CutsceneStep.Action.Sound:

			if (step.intVal == 0) {
				if (step.actor == null) {

					if( step.obj != null )
					{
						/*if( step.obj.name.Contains("song_map_short") )
							//DoralabAPI.AddBgmPlayCount(1);
						else if( step.obj.name.Contains("We did it"))
							DoralabAPI.AddBgmPlayCount(2);
						else if( step.obj.name.Contains("we did it"))
							DoralabAPI.AddBgmPlayCount(2);
						else if( step.obj.name.Contains("wedidit"))
							DoralabAPI.AddBgmPlayCount(2);
						else if( step.obj.name.Contains("song_where are we going"))
							DoralabAPI.AddBgmPlayCount(3);
						else if( step.obj.name.Contains("song_backpack"))
							DoralabAPI.AddBgmPlayCount(4);
                        */

						PlayBgm(step.obj as AudioClip, step.boolVal, step.floatVal);
					}
					else{
						StopBgm();
					}
				}
			}
			else if(step.intVal == 2)
			{
				AudioClip bgmclip = bgms[World.langIdx][step.intVal2].clip;
				if(step.intVal2 == 0)
				{
					StopBgm();
				}
				else
				{
					if( bgmclip != null && step.actor == null )
					{
                        /*
						if( bgmclip.name.Contains("song_map_short") )
							DoralabAPI.AddBgmPlayCount(1);
						else if( bgmclip.name.Contains("We did it"))
							DoralabAPI.AddBgmPlayCount(2);
						else if( bgmclip.name.Contains("we did it"))
							DoralabAPI.AddBgmPlayCount(2);
						else if( bgmclip.name.Contains("wedidit"))
							DoralabAPI.AddBgmPlayCount(2);
						else if( bgmclip.name.Contains("song_where are we going"))
							DoralabAPI.AddBgmPlayCount(3);
						else if( bgmclip.name.Contains("song_backpack"))
							DoralabAPI.AddBgmPlayCount(4);
                        */
						PlayBgm(bgmclip, step.boolVal, step.floatVal);
					}
					else if( bgmclip == null )
					{
						StopBgm();
					}
				}
			}
			else {
				Vector3 pos = Vector3.zero;
				if (step.actor != null)
					pos = step.actor.transform.position;
				if( step.obj == null )
				{
					Debug.LogError("Audio Clip is Null");
				}
				else
					AudioSource.PlayClipAtPoint(step.obj as AudioClip, pos, step.floatVal == 0 ? 1 : step.floatVal);
				
				return true;
			}
			break;
			
		case CutsceneStep.Action.ChangeInto:
			switch (step.intVal2) {
			case 0:
				if (step.intVal == 0)
					Global.AddParts(step.actor.name, step.obj as Transform);
				break;
				
			case 1:
				Global.RestoreParts(step.actor);
				return true;

			case 2:
				Global.ClearParts(step.actor);
				return true;
				
			case 3:
				Global.ClearAttach(step.actor, step.obj as Transform);
				return true;
			}
			break;
		}
		
		if (step.actor != null)
			step.actor.DoStep(step);
		
		return true;
	}
	
	void DoneMessage(string name) {
		foreach (CutsceneStep step in progSteps) {
			if (step.action == CutsceneStep.Action.Proc) {
				if (step.strVal == name) {
					doneSteps.Add (step.stepId, timeElapsed);
					progSteps.Remove (step);
					break;
				}
			}
		}
	}
/*	
	void RunStep(int stepId) {
		foreach (CutsceneStep step in steps) {
			if (step.stepId == stepId) {
				DoStep (step);
				break;
			}
		}
	}
		 */
	public void MakeTalk(ActorBase actor, int dialog, Transform target) {
		if (dialog > 0) {
			//gbl.DisplaySubtitle(dialogs[World.langIdx][dialog].caption, actor);
			actor.TalkTo(dialogs[World.langIdx][dialog].clip, target, CutsceneStep.Action.TalkTo);
		}
	}

	public void MakeTalk(ActorBase actor, AudioClip clip, string dialog, Transform target) {
		//gbl.DisplaySubtitle(dialog, actor);
		actor.TalkTo(clip, target, CutsceneStep.Action.TalkTo);
	}
	
	LcChoice GetChoice() {
		if (choice == null)
			choice = GameObject.FindObjectOfType(typeof(LcChoice)) as LcChoice;
		
		return choice;
	}
	
	bool IsBool(int idx, string name) {
		if (GetChoice() != null) {
			return choice.IsBool(idx, name);
		}
		
		return false;
	}

	void SetBool(int idx, string name) {
		if (GetChoice() != null) {
			choice.SetBool(idx, name);
		}
	}
	
	bool IsDone(CutsceneStep step) {
		if (Time.timeScale == 0)
			return false;
		
		switch (step.action) {
		// case CutsceneStep.Action.WaitSound:
		// 	//plugin.EndSoundInputSound();
		// 	return plugin.isSounded;
			
		default:
			return step.isDone;
		}
	}
	
	void StopBgm() {
		AudioSource audioSrc;
		GameObject obj = GameObject.Find ("Bgm");
		if (obj == null) {
				obj = new GameObject ("Bgm");
				audioSrc = obj.AddComponent<AudioSource> ();
		} else 
				audioSrc = obj.GetComponent<AudioSource> ();
		audioSrc.Stop ();
	}
	void PlayBgm(AudioClip clip, bool loop, float volume) {
		AudioSource audioSrc;
		GameObject obj = GameObject.Find ("Bgm");
		if (obj == null) {
			obj = new GameObject("Bgm");
			audioSrc = obj.AddComponent<AudioSource>();
		} else 
			audioSrc = obj.GetComponent<AudioSource>();
		audioSrc.Stop ();
		if (clip != null) {
			if (volume == 0)
				volume = 1;
			
			audioSrc.loop = loop;
			audioSrc.volume = volume;
			audioSrc.clip = clip;
			audioSrc.Play ();

			string bgmName = clip.name;
			bool bExist = false;
			if( bgmName.Contains( "song_map_short" ) )
			{
				bExist = true;
			}
			else if( bgmName.Contains( "We did it" ) )
			{
				bExist = true;
			}
			else if( bgmName.Contains( "we did it" ) )
			{
				bExist = true;
			}
			else if( bgmName.Contains( "where are we going" ) )
			{
				bExist = true;
			}
			else if( bgmName.Contains( "song_backpack_" ) )
			{
				bExist = true;
			}
			if( bExist )
			{
				//Play License Sound
			}
		}
	}
	
	void EndCutscene() {
		if (nextCutscenes != null && nextCutscenes.Length > 0) {
			nextCutscenes[Random.Range(0, nextCutscenes.Length)].gameObject.SetActive(true);
		}
		Restart();
		gameObject.SetActive(false);
	}
}
