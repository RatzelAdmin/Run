using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Controller_SongSystem : MonoBehaviour {
	
	public GameObject NotePrefab;
	public SongData[] Playlist;
	protected SongPlayer Player;
	protected List<GameObject> NoteObjects;
	protected CutsceneMgr.Dialog[][] dialogs;
	protected CutsceneMgr.BGM[][] bgms;
	Global gbl;
	MicComponent plugin;
	// Use this for initialization
	IEnumerator Start () {
	
		Player = GetComponent<SongPlayer>();
		NoteObjects = new List<GameObject>();
		
		
		plugin = FindObjectOfType(typeof(MicComponent)) as MicComponent;
		if (plugin == null) {
//			if (prePlugin != null) {
//				Transform obj = Instantiate(prePlugin) as Transform;
//				obj.name = "Plugin";
//				DontDestroyOnLoad(obj);
//				plugin = obj.GetComponent<Plugin>();
			//			} else {
#if EnglishVersion 
				Transform obj = (Transform)Instantiate(Resources.Load("Plugin_Quick_English", typeof(Transform))) ;
#elif TaiwanVersion
				Transform obj = (Transform)Instantiate(Resources.Load("Plugin_Quick_Taiwan", typeof(Transform))) ;
#else
				Transform obj = (Transform)Instantiate(Resources.Load("Plugin_Quick_Korea", typeof(Transform))) ;
#endif
				//Transform obj = (Transform)Instantiate(Resources.Load("Plugin_Quick_Korea", typeof(Transform))) ;
				obj.name = "Plugin";
				DontDestroyOnLoad(obj);
				plugin = obj.GetComponent<MicComponent>();
//				Transform obj = (Transform)Instantiate(Resources.Load("Plugin", typeof(Transform))) ;
//				obj.name = "Plugin";
//				DontDestroyOnLoad(obj);
//				plugin = obj.GetComponent<Plugin>();
//			}
		}
		
		gbl = plugin.GetComponent<Global>();
		
		dialogs = new CutsceneMgr.Dialog[2][];
		bgms = new CutsceneMgr.BGM[2][];
		
		yield return new WaitForSeconds(0.5f);
		StartPlaying(0);
	}
	
	// Update is called once per frame
	void Update () {
	
		if( Player.IsPlaying() )
		{
//			//Check for ESC and possibly show in game menu
//			ShowInGameMenuOnKeypress();
//
//			//No note has been hit int this frame yet
//			ResetHasHitNoteOnStringIndexArray();
//
//			UpdateNeckTextureOffset();
			UpdateNotes();

//			UpdateGuiScore();
//			UpdateGuiMultiplier();
		}
	}
	
	public void StartPlaying( int playlistIndex )
	{
		//ResetGameStateValues();

		Player.SetSong( Playlist[ playlistIndex ] );
		
		if ( Playlist[ playlistIndex ].dialogData != null) {
			for (int i = 0; i < 2; i++) {
				dialogs[i] = Playlist[ playlistIndex ].dialogData.GetDialogs(i);
			}
		}
		if ( Playlist[ playlistIndex ].BGMData != null) {
			for (int i = 0; i < 2; i++) {
				bgms[i] = Playlist[ playlistIndex ].BGMData.GetBGMs(i);
			}
		}
		
		CreateNoteObjects();

		Player.Play();
		//StartCoroutine( "DisplayCountdown" );
	}
	
	public void StopPlaying()
	{

		DestroyNoteObjects();

		StopAllCoroutines();
	}
	
	protected void CreateNoteObjects()
	{
		NoteObjects.Clear();

		for( int i = 0; i < Player.Song.Notes.Count; ++i )
		{
			//if( Player.Song.Notes[ i ].StringIndex != 0 && Player.Song.Notes[ i ].StringIndex != 4 )
			{
				//Create note and trail objects
				GameObject note = InstantiateNoteFromPrefab( Player.Song.Notes[ i ].StringIndex );
				//CreateTrailObject( note, Player.Song.Notes[ i ] );

				//Hide object on start, they will be shown - when appropriate - in the UpdateNotes routine
				note.renderer.enabled = false;

				note.SetActive( false );
				NoteObjects.Add( note );
			}
		}
	}
	
	protected void DestroyNoteObjects()
	{
		for( int i = 0; i < NoteObjects.Count; ++i )
		{
			Destroy( NoteObjects[ i ] );
		}

		NoteObjects.Clear();
	}
	protected GameObject InstantiateNoteFromPrefab( int stringIndex )
	{
		GameObject note = Instantiate( NotePrefab
									 , Vector3.zero
									 , Quaternion.identity
									 ) as GameObject;

		//note.renderer.material.color = Colors[ stringIndex ];
		note.transform.Rotate( new Vector3( -90, 0, 0 ) );

		return note;
	}
	protected void UpdateNotes()
	{
		for( int i = 0; i < NoteObjects.Count; ++i )
		{
			UpdateNotePosition( i );

		}
	}
	protected void UpdateNotePosition( int index )
	{
		Note note = Player.Song.Notes[ index ];

		//If the note is farther away then 6 beats, its not visible on the neck and we dont have to update it
		if( note.Time < Player.GetCurrentBeat()+1.3 )// + 6 )
		{
			//If the note is not active, it is visible on the neck for the first time
			
			if( !NoteObjects[ index ].activeSelf )
			{
				//Activate and show the note
				NoteObjects[ index ].SetActive( true );
				NoteObjects[ index ].renderer.enabled = false;
				//Debug.Log ("Show Note");
				
				DoStep(note.Step);
//				//If there is a trail, show that aswell
//				if( Player.Song.Notes[ index ].Length > 0f )
//				{
//					NoteObjects[ index ].transform.Find( "Trail" ).renderer.enabled = true;
//				}
			}


			//Calculate how far the note has progressed on the neck
//			float progress = ( note.Time - Player.GetCurrentBeat() - 0.5f ) / 6f;
//
//			//Update its position
//			Vector3 position = NoteObjects[ index ].transform.position;
//			position.z = progress * GetGuitarNeckLength();
//			NoteObjects[ index ].transform.position = position;
		}
	}
	
	
	public bool DoStep(CutsceneStep step) {
//		if (step.action == CutsceneStep.Action.WaitTouch && step.intVal > 0) {
//			if (IsBool(step.intVal, step.actor.name)) {
//				step.actor.WaitTouch(0, false);
//				return false;
//			}
//		}
		
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
					gbl.DisplaySubtitle(dialogs[World.langCaptionIdx][step.intVal].caption, step.actor);
				}
			}

			break;
		case CutsceneStep.Action.TalkTo:
		case CutsceneStep.Action.SpeechTo:
			if (step.intVal2 > 0)
				step.actor.Action(step.intVal2, false, null);
			
			if( World.langCaptionIdx != -1 )
			{
				if (step.intVal > 0 && dialogs[World.langIdx].Length > step.intVal) {
					gbl.DisplaySubtitle(dialogs[World.langCaptionIdx][step.intVal].caption, step.actor);
					if (step.floatVal == 0)
						step.actor.TalkTo(dialogs[World.langIdx][step.intVal].clip, step.target, step.action);
				}
			}
			else
			{
				if (step.intVal > 0) {
					if (step.floatVal == 0)
						step.actor.TalkTo(dialogs[World.langIdx][step.intVal].clip, step.target, step.action);
				}
			}
			if (step.floatVal > 0) {
				step.actor.LipTo(step.floatVal, step.target, step.action);
			}
			step.actor.NextLoopAction(step.boolVal);
			break;
			
		case CutsceneStep.Action.Action:
			AudioClip clip = null;
			if (step.intVal2 > 0 && dialogs[World.langIdx].Length > step.intVal2)
				clip = dialogs[World.langIdx][step.intVal2].clip;
			step.actor.Action(step.intVal, false,  clip);
			step.actor.NextLoopAction(step.boolVal);
			break;
			
		case CutsceneStep.Action.LoadScene:
			//World.loadedLevelName = step.strVal;
			if( World.loadedLevelName == "")
				World.loadedLevelName = Application.loadedLevelName;
			
			Debug.Log("LoadScene Pre loadedName = "+World.loadedLevelName);
			
			if( World.loadedLevelName.Contains("_ending") )
				World.EpisodeFinish();
			
			//English Episode Ending Check
			if( step.strVal.Contains("GoToStart") )
			{
				if( World.loadedLevelName.Contains("episode_e") )
					World.EpisodeFinish();
			}

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
				step.target.SendMessage(step.strVal, step.strVal2, SendMessageOptions.DontRequireReceiver);
			}
		}
			break;
			
		case CutsceneStep.Action.Proc:
			SendMessage(step.strVal, step.strVal2);
			break;
			
		case CutsceneStep.Action.PlayMovie:
			Handheld.PlayFullScreenMovie(step.strVal, Color.black
				, step.boolVal ? FullScreenMovieControlMode.CancelOnInput : FullScreenMovieControlMode.Hidden);
			break;
			
		case CutsceneStep.Action.WaitTouch:
			gbl.StartWaitTouchSound();
			break;
		case CutsceneStep.Action.WaitSound:
			//plugin.StartSoundInputSound();
			plugin.StartSoundSenser(step.intVal, step.floatVal == 0 ? float.MaxValue : step.floatVal);
			break;
			
		case CutsceneStep.Action.Sound:
			if (step.intVal == 0) {
				if (step.actor == null) {
					PlayBgm(step.obj as AudioClip, step.boolVal, step.floatVal);
				}
			}
			else if(step.intVal == 2)
			{
				AudioClip bgmclip = bgms[World.langIdx][step.intVal2].clip;
				if( bgmclip != null && step.actor == null )
					PlayBgm(bgmclip, step.boolVal, step.floatVal);
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
			}
			break;
		}
		
		if (step.actor != null)
			step.actor.DoStep(step);
		
		return true;
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
	
}
