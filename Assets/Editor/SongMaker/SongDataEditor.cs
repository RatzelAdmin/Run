using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor( typeof( SongData ) )]
public class SongDataEditor : Editor
{
	//Needed to draw 2D lines and rectangles
	private static Texture2D _coloredLineTexture;
	private static Color _coloredLineColor;

	private GameObject GuitarObject;
	//private AudioSource MetronomeSource;
	private SongPlayer SongPlayer;

	//Dimensions of the editor
	//Song View is the one with the black background, where you can add notes etc.
	//ProgressBar, or Progress View is the small preview on the right, where you can navigate through the song
	private Rect SongViewRect;
	private float SongViewProgressBarWidth = 20f;
	private float SongViewHeight = 400f;

	//Metronome Vars
	private static bool UseMetronome;
//	private float LastMetronomeBeat = Mathf.NegativeInfinity;

	//Helper vars to handle mouse clicks
	private Vector2 MouseUpPosition = Vector2.zero;
	private bool LastClickWasRightMouseButton;

	//Currently selected note index
	private int SelectedNote = -1;

	//Song overview texture (the one on the right which you can use to navigate)
	private Texture2D ProgressViewTexture;

	//Timer to calculate editor performance
	Timer PerformanceTimer = new Timer();
	
	
	
	CutsceneStep currentStep;
	//Vector2 scroll1 = Vector2.zero;
	
	int[] actionIds = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25,
		26, 27, 28, 29, 30 };
//	int[] actionDisplayIds = { 0, 3, 11, 24, 18, 26, 7, 25, 10, 2, 27, 5, 15, 19, 1, 20, 16, 6, 23, 12, 17, 14, 13, 4, 
//		21,	8, 22, 9, 28, 29, 30 };
	
	string[] moodNames = { "default_001", "smile_002", "smile_003", "smile_004", "disappoint_001", "disappoint_002",
		"surprised_001", "surprised_002", "curiosity_001", "sleep_001","sleep_002","sleep_003", };
	int[] moodIds = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
	string[] changeIntoNames = { "Put On", "Restore", "Clear", "ClearAttach"  };
	int[] changeIntoIds = { 0, 1, 2, 3 };
	string[] lookAtNames = { "Object", "Position", "Camera" };
	int[] lookAtIds = { 0, 1, 2 };
	string[] soundNames = { "BGM", "SE", "SONG" };
	string[] sendToNames = { "Activate", "Deactivate", "Message" };
	int[] gameLogIds = { 0, 1, 2, 3 };
	string[] gameLogNames = { "GameStart", "GameEnd", "ChapterStart", "ChapterEnd" };
	enum StepOp {NoOp, AddAfter, Del, Up, Down};
	
	
	string[] dialogs;
	int[] dialogIds;
	string[] bgmNames;
	int[] bgmIds;
	
	static Dictionary<ActorBase, ActorBase> actionReadDic;
	static Dictionary<ActorBase, ActorBase> loopActionReadDic;
	
	
//	int tempActionId = 0; 
//	int tempActionDisplayId = 0; 
	CutsceneStep.ActionDisplay tempAcitonDisplay;
	
	int testOffsetVal = 0;

	[MenuItem( "Assets/Create/Song" )]
	public static void CreateNewSongAsset()
	{
		GameObject obj = new GameObject ("SongData");
		obj.AddComponent ("SongData");
		if (Selection.activeTransform != null)
			obj.transform.parent = Selection.activeTransform;
		
		Selection.activeGameObject = obj;
		
		if( actionReadDic != null )
			actionReadDic.Clear();
		if( loopActionReadDic != null )
			loopActionReadDic.Clear();
		actionReadDic = new Dictionary<ActorBase, ActorBase>();
		loopActionReadDic = new Dictionary<ActorBase, ActorBase>();
		
//		SongData asset = ScriptableObject.CreateInstance<SongData>();
//		AssetDatabase.CreateAsset( asset, "Assets/NewSong.asset" );
//		
//		EditorUtility.FocusProjectWindow();
//		Selection.activeObject = asset;
	}

	protected void OnEnable()
	{
		//Setup object references
		GuitarObject = GameObject.Find( "SongSystem" );

		if( GuitarObject == null )
		{
			return;
		}

		SongPlayer = GuitarObject.GetComponent<SongPlayer>();
		//MetronomeSource = GameObject.Find( "Metronome" ).audio;

		//Prepare playback
		SongPlayer.SetSong( (SongData)target );
//		LastMetronomeBeat = -Mathf.Ceil( SongPlayer.Song.AudioStartBeatOffset );
		
		if( actionReadDic != null )
			actionReadDic.Clear();
		if( loopActionReadDic != null )
			loopActionReadDic.Clear();
		actionReadDic = new Dictionary<ActorBase, ActorBase>();
		loopActionReadDic = new Dictionary<ActorBase, ActorBase>();
		

		RedrawProgressViewTexture();
	}

	protected void RedrawProgressViewTexture()
	{
		int width = (int)SongViewProgressBarWidth;
		int height = (int)SongViewHeight;

		if( !ProgressViewTexture )
		{
			//Create empty texture if it doesn't exist
			ProgressViewTexture = new Texture2D( width, height );
			ProgressViewTexture.wrapMode = TextureWrapMode.Clamp;
			ProgressViewTexture.hideFlags = HideFlags.HideAndDontSave;
		}

		//Draw Background Color
		Color[] BackgroundColor = new Color[ width * height ];
		for( int i = 0; i < width * height; ++i )
		{
			BackgroundColor[ i ] = new Color( 0.13f, 0.1f, 0.26f );
		}

		ProgressViewTexture.SetPixels( 0, 0, width, height, BackgroundColor );

		//Calculate the scale in which the notes are drawn, so they all fit into the progress view
		float totalBeats = SongPlayer.Song.GetLengthInBeats();
		float heightOfOneBeat = 1f / totalBeats * (float)height;

		//Draw all notes
		for( int i = 0; i < SongPlayer.Song.Notes.Count; ++i )
		{
			//Which string does this note belong to?
			int stringIndex = SongPlayer.Song.Notes[ i ].StringIndex;

			//Which color does this string have?
			Color color = Color.yellow;// GuitarObject.GetComponent<GuitarGameplay>().GetColor( stringIndex );

			//Calculate position of the note
			int y = (int)Mathf.Round( ( ( SongPlayer.Song.Notes[ i ].Time + SongPlayer.Song.AudioStartBeatOffset - 1 ) / totalBeats ) * height );
			int x = (int)( width / 2 ) + ( stringIndex - 2 ) * 4;

			//Get the trail length (0 = no trail)
			float length = SongPlayer.Song.Notes[ i ].Length;

			//Draw 3x3 pixel rectangle
			for( int j = -1; j < 2; ++j )
			{
				for( int k = -1; k < 2; ++k )
				{
					ProgressViewTexture.SetPixel( x + j, y + k, color );
				}
			}

			//Draw trail
			if( length > 0 )
			{
				for( int lengthY = y; lengthY < y + length * heightOfOneBeat; ++lengthY )
				{
					ProgressViewTexture.SetPixel( x, lengthY, color );
				}
			}
		}
			
		ProgressViewTexture.Apply();
	}

	public override void OnInspectorGUI()
	{

		DrawInspector();
		//Check for mouse events
		if( Event.current.isMouse )
		{
			if( Event.current.type == EventType.mouseDown )
			{
				OnMouseDown( Event.current );
			}
			else if( Event.current.type == EventType.mouseUp )
			{
				OnMouseUp( Event.current );
			}
		}

		//Check for key input events
		if( Event.current.isKey )
		{
			if( Event.current.type == EventType.keyDown )
			{
				OnKeyDown( Event.current );
			}
		}

		if( Event.current.type == EventType.ValidateCommand )
		{
			switch( Event.current.commandName )
			{
				case "UndoRedoPerformed":
					RedrawProgressViewTexture();
					break;
			}
		}


		if( GUI.changed )
		{
			SongData targetData = target as SongData;
			if( targetData.BackgroundTrack != null && SongPlayer.Song != targetData )
			{
				SongPlayer.SetSong( targetData );
			}
		}

		//UpdateMetronome();
		RepaintGui();
		
	}

	protected void OnKeyDown( Event e )
	{
		switch( e.keyCode )
		{
			case KeyCode.UpArrow:
				//Up arrow advances the song by one beat

			if(GuitarObject.audio.time == 0){

				if(GuitarObject.audio.time == 0){
				Debug.Log("Upkey press testSet");
					//testOffsetVal = 1;

				}
				
			}
			GuitarObject.audio.time += MyMath.BeatsToSeconds( 1f, SongPlayer.Song.BeatsPerMinute );
		

			e.Use();
				break;
			case KeyCode.DownArrow:
				
				//Down arrow rewinds the song by one beat
				if( GuitarObject.audio.time >= MyMath.BeatsToSeconds( 1f, SongPlayer.Song.BeatsPerMinute ) )
				{
					GuitarObject.audio.time -= MyMath.BeatsToSeconds( 1f, SongPlayer.Song.BeatsPerMinute );
				}
				else
				{

					GuitarObject.audio.time = 0;
				}
				if(GuitarObject.audio.time == 0){
					Debug.Log("testSet");
					//testOffsetVal = 0;
					
				}
				e.Use();
				break;
			case KeyCode.RightControl:
				//Right CTRL plays/pauses the song
				OnPlayPauseClicked();
				e.Use();
				break;
			case KeyCode.LeftArrow:
				//Left arrow selects the previous note
				if( SelectedNote != -1 && SelectedNote > 0 )
				{
					SelectedNote--;
					Repaint();
				}
				break;
			case KeyCode.RightArrow:
				//Right arrow selects the next note
				if( SelectedNote != -1 && SelectedNote < SongPlayer.Song.Notes.Count )
				{
					SelectedNote++;
					Repaint();
				}
				break;
//			case KeyCode.Delete:
//				//DEL removes the currently selected note
//				if( SelectedNote != -1 )
//				{
//					Undo.RegisterUndo( SongPlayer.Song, "Remove Note" );
//
//					SongPlayer.Song.RemoveNote( SelectedNote );
//					SelectedNote = -1;
//					EditorUtility.SetDirty( target );
//					RedrawProgressViewTexture();
//
//					Repaint();
//				}
//				break;
			case KeyCode.Alpha1:
				AddNewNoteAtCurrentTime( 0 );
				break;
			case KeyCode.Alpha2:
				AddNewNoteAtCurrentTime( 1 );
				break;
			case KeyCode.Alpha3:
				AddNewNoteAtCurrentTime( 2 );
				break;
			case KeyCode.Alpha4:
				AddNewNoteAtCurrentTime( 3 );
				break;
			case KeyCode.Alpha5:
				AddNewNoteAtCurrentTime( 4 );
				break;
		}
	}

	void AddNewNoteAtCurrentTime( int stringIndex )
	{
		float currentBeat = Mathf.Round( ( SongPlayer.GetCurrentBeat( true ) + 1 ) * 4 ) / 4;

		Note note = SongPlayer.Song.Notes.Find( item => item.Time == currentBeat && item.StringIndex == stringIndex );

		if( note == null )
		{
			SongPlayer.Song.AddNote( currentBeat, stringIndex );
		}
		else
		{
			Debug.Log( "There is already a note at " + currentBeat + " on string " + stringIndex );
		}
	}

	protected GUIStyle GetWarningBoxStyle()
	{
		GUIStyle box = new GUIStyle( "box" );

		box.normal.textColor = EditorStyles.miniLabel.normal.textColor;
		box.imagePosition = ImagePosition.ImageLeft;
		box.stretchWidth = true;
		box.alignment = TextAnchor.UpperLeft;

		return box;
	}

	protected void WarningBox( string text, string tooltip = "" )
	{
		GUIStyle box = GetWarningBoxStyle();

		Texture2D warningIcon = (Texture2D)Resources.Load( "Warning", typeof( Texture2D ) );
		GUIContent content = new GUIContent( " " + text, warningIcon, tooltip );
		GUILayout.Label( content, box );
	}

	protected void DrawInspector()
	{
		if( GuitarObject == null )
		{
			WarningBox( "Guitar Object could not be found." );
			WarningBox( "Did you load the GuitarUnity scene?" );
			return;
		}

		//Time the performance of the editor window
		PerformanceTimer.Clear();
		PerformanceTimer.StartTimer( "Draw Inspector" );

		GUILayout.Label( "Song Data", EditorStyles.boldLabel );

		DrawDefaultInspector();

		if( SongPlayer.Song.BackgroundTrack == null )
		{
			WarningBox( "Please set a background track!" );
			return;
		}

		if( SongPlayer.Song.BeatsPerMinute == 0 )
		{
			WarningBox( "Please set the beats per minute!" );
		}
		

		if( dialogs == null )
		{
			CutsceneMgr.Dialog[] list = null;
			if (SongPlayer.Song.dialogData != null)
				list = SongPlayer.Song.dialogData.GetDialogs(EditorGlobal.langIdx);
			if (list != null) {
				dialogs = new string[list.Length];
				dialogIds = new int[list.Length];
				int i = 0;
				foreach (CutsceneMgr.Dialog item in list) {
					if (i == 0)
						dialogs[i] = item.caption;
					else
						dialogs[i] = string.Format ("{0}. {1}", i, item.caption);
					dialogIds[i] = i;
					i++;
				}
			}
		}
		if( bgmNames == null )
		{
			CutsceneMgr.BGM[] listBGM = null;
			
			if (SongPlayer.Song.BGMData != null)
				listBGM = SongPlayer.Song.BGMData.GetBGMs(EditorGlobal.langIdx);
			if (listBGM != null) {
				bgmNames = new string[listBGM.Length];
				bgmIds = new int[listBGM.Length];
				int i = 0;
				foreach (CutsceneMgr.BGM item in listBGM) {
					if (i == 0)
						bgmNames[i] = "(None)";
					else
						bgmNames[i] = string.Format ("{0}. {1}", i, item.clip.name);
					bgmIds[i] = i;
					i++;
				}
			}
		}
		
		
		
		if( SelectedNote >= SongPlayer.Song.Notes.Count )
		{
			SelectedNote = -1;
		}

		if( SelectedNote == -1 )
		{
			//If no note is selected, still draw greyed out inspector elements 
			//so the editor doesn't jump when notes are selected and deselected

			GUI.enabled = false;
			GUILayout.Label( "No Note selected", EditorStyles.boldLabel );

			EditorGUILayout.FloatField( "Time", 0 );
			EditorGUILayout.IntField( "String", 0 );
			EditorGUILayout.FloatField( "Length", 0 );

			EditorGUILayout.BeginHorizontal();
				GUILayout.Space( 15 );
				GUILayout.Button( "Remove Note" );
			EditorGUILayout.EndHorizontal();

			GUI.enabled = true;
		}
		else
		{
			//Draw Header and Next/Previous Note Buttons
			EditorGUILayout.BeginHorizontal();

				GUILayout.Label( "Note " + SelectedNote.ToString(), EditorStyles.boldLabel );
				
				if( SelectedNote == 0 )
				{
					GUI.enabled = false;
				}
				if( GUILayout.Button( "<" ) )
				{
					SelectedNote--;
				}
				GUI.enabled = true;

				if( SelectedNote == SongPlayer.Song.Notes.Count - 1 )
				{
					GUI.enabled = false;
				}
				if( GUILayout.Button( ">" ) )
				{
					SelectedNote++;
				}
				GUI.enabled = true;

			EditorGUILayout.EndHorizontal();

			//Draw note data
			float newTime = EditorGUILayout.FloatField( "Time", SongPlayer.Song.Notes[ SelectedNote ].Time );
			int newStringIndex = EditorGUILayout.IntField( "String", SongPlayer.Song.Notes[ SelectedNote ].StringIndex );
			float newLength = EditorGUILayout.FloatField( "Length", SongPlayer.Song.Notes[ SelectedNote ].Length );
			
//			SongPlayer.Song.Notes[ SelectedNote ].Step =  EditorGUILayout.ObjectField("SongStep:"
//				, SongPlayer.Song.Notes[ SelectedNote ].Step, typeof(SongStep),true);
			
			newStringIndex = Mathf.Clamp( newStringIndex, 0, 4 );

			//If note has changed, register undo, commit changes and redraw progress view
			if( newTime != SongPlayer.Song.Notes[ SelectedNote ].Time
				|| newStringIndex != SongPlayer.Song.Notes[ SelectedNote ].StringIndex
				|| newLength != SongPlayer.Song.Notes[ SelectedNote ].Length )
			{
//				Undo.RegisterUndo( SongPlayer.Song, "Edit Note" );

				SongPlayer.Song.Notes[ SelectedNote ].Time = newTime;
				SongPlayer.Song.Notes[ SelectedNote ].StringIndex = newStringIndex;
				SongPlayer.Song.Notes[ SelectedNote ].Length = newLength;
				//SongPlayer.Song.Notes[ SelectedNote ].Step = newStep;
				
				
				RedrawProgressViewTexture();

				Repaint();
			}
			
			//Remove Note Button
			//15px Space is added to the front to match the default unity style
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space( 15 );
			if( GUILayout.Button( "Remove Note" ) )
			{
	//			Undo.RegisterUndo( SongPlayer.Song, "Remove Note" );

				SongPlayer.Song.RemoveNote( SelectedNote );
				SelectedNote = -1;
				RedrawProgressViewTexture();
				EditorUtility.SetDirty( target );

				Repaint();
			}
			EditorGUILayout.EndHorizontal();
		}
			
		GUILayout.Label( "Song", EditorStyles.boldLabel );

		//Draw song player controls
		EditorGUILayout.BeginHorizontal();
			GUILayout.Space( 15 );

			string buttonLabel = "Play Song";
			if( IsPlaying() )
			{
				buttonLabel = "Pause Song";
			}

			if( GUILayout.Button( buttonLabel ) )
			{
				OnPlayPauseClicked();
			}
			if( GUILayout.Button( "Stop Song" ) )
			{
				OnStopClicked();
			}
		EditorGUILayout.EndHorizontal();

		//Add playback speed slider
		EditorGUILayout.BeginHorizontal();
			GUILayout.Space( 15 );
			GUILayout.Label( "Playback Speed", EditorStyles.label );
			GuitarObject.audio.pitch = GUILayout.HorizontalSlider( GuitarObject.audio.pitch, 0, 1 );
		EditorGUILayout.EndHorizontal();

		//Draw Use Metronome toggle
		UseMetronome = EditorGUILayout.Toggle( "Metronome", UseMetronome );

		//Draw song editor
		SongViewRect = GUILayoutUtility.GetRect( GUILayoutUtility.GetLastRect().width, SongViewHeight );

		PerformanceTimer.StartTimer( "Draw Background" );
		DrawRectangle( 0, SongViewRect.yMin, SongViewRect.width, SongViewRect.height, Color.black );

		PerformanceTimer.StartTimer( "Draw Progress View" );
		DrawProgressView();

		PerformanceTimer.StartTimer( "Draw Main View" );
		DrawMainView();

		PerformanceTimer.StopTimer();
		
		DrawCutsceneStep();//oneway48 : Draw CutsceneStep
		
		DrawAddNotesHint();
		DrawEditorPerformancePanel();
	}

	protected void DrawAddNotesHint()
	{
		WarningBox( "Use number keys 1-5 to add notes while playing" );
	}

	protected void DrawEditorPerformancePanel()
	{
		List<TimerData> Timers = PerformanceTimer.GetTimers();

		GUILayout.Label( "Editor Performance", EditorStyles.boldLabel );

		for( int i = 0; i < Timers.Count; ++i )
		{
			float displayMs = Mathf.Round( Timers[ i ].Time * 10000 ) / 10;
			GUILayout.Label( Timers[ i ].Name + " " + displayMs + "ms" );
		}

		float deltaTime = PerformanceTimer.GetTotalTime();
		float fps = Mathf.Round( 10 / deltaTime ) / 10;
		float msPerFrame = Mathf.Round( deltaTime * 10000 ) / 10;
		GUILayout.Label( "Total " + msPerFrame + "ms/frame (" + fps + "FPS)" );
	}

//	protected void UpdateMetronome()
//	{
//		if( !UseMetronome )
//		{
//			return;
//		}
//
//		if( !IsPlaying() )
//		{
//			return;
//		}
//
//		float currentWholeBeat = Mathf.Floor( SongPlayer.GetCurrentBeat( true ) + 0.05f );
//		if( currentWholeBeat > LastMetronomeBeat )
//		{
//			LastMetronomeBeat = currentWholeBeat;
//
//			MetronomeSource.Stop();
//			MetronomeSource.time = 0f;
//
//			MetronomeSource.Play();
//			MetronomeSource.Pause();
//			MetronomeSource.Play();
//		}
//	}

	protected void RepaintGui()
	{
		if( IsPlaying() )
		{
			Repaint();
		}
	}

	protected Rect GetProgressViewRect()
	{
		return new Rect( SongViewRect.width - SongViewProgressBarWidth, SongViewRect.yMin, SongViewProgressBarWidth, SongViewRect.height );
	}

	protected bool IsPlaying()
	{
		if( GuitarObject == null )
		{
			return false;
		}

		return GuitarObject.audio.isPlaying;
	}

	protected void DrawMainView()
	{
		float totalWidth = SongViewRect.width - SongViewProgressBarWidth;

		if( totalWidth < 0 )
		{
			return;
		}

		DrawBeats();
		DrawStrings();
		DrawTimeMarker();
		DrawGridNotesAndHandleMouseClicks();
		DrawNotes();
	}

	protected void DrawTimeMarker()
	{
		float heightOfOneBeat = SongViewRect.height / 6f;

		DrawLine( new Vector2( SongViewRect.xMin, SongViewRect.yMax - heightOfOneBeat )
				, new Vector2( SongViewRect.xMax - SongViewProgressBarWidth, SongViewRect.yMax - heightOfOneBeat )
				, new Color( 1f, 0f, 0f )
				, 4 );
	}

	protected void DrawStrings()
	{
		float totalWidth = SongViewRect.width - SongViewProgressBarWidth;
		float stringDistance = totalWidth / 6;

		for( int i = 0; i < 5; ++i )
		{
			float x = stringDistance * ( i + 1 );
			DrawVerticalLine( new Vector2( x, SongViewRect.yMin )
							, new Vector2( x, SongViewRect.yMax )
							, new Color( 0.4f, 0.4f, 0.4f )
							, 3 );
		}
	}

	protected void DrawNotes()
	{
		//Calculate positioning variables
		float heightOfOneBeat = SongViewRect.height / 6f;
		float totalWidth = SongViewRect.width - SongViewProgressBarWidth;
		float stringDistance = totalWidth / 6;
		
		Note note;

		for( int i = 0; i < SongPlayer.Song.Notes.Count; ++i )
		{
			note = SongPlayer.Song.Notes[ i ];

			if( note.Time > SongPlayer.GetCurrentBeat( true ) + 6.5f )
			{
				//If note is not visible, skip it and draw next note
				continue;
			}

			if( note.Time + note.Length < SongPlayer.GetCurrentBeat( true ) - 0.5f )
			{
				//If note is not visible, skip it and draw next note
				continue;
			}

			//How far has the note progressed
			float progressOnNeck = 1 - ( note.Time - SongPlayer.GetCurrentBeat( true ) ) / 6f;

			//Get note color
			Color color = Color.yellow;//GuitarObject.GetComponent<GuitarGameplay>().GetColor( note.StringIndex );

			//Get note position
			float y = SongViewRect.yMin + progressOnNeck * SongViewRect.height;
			float x = SongViewRect.xMin + ( note.StringIndex + 1 ) * stringDistance;

			//If note is selected, draw a white rectangle around it
			if( SelectedNote == i )
			{
				DrawRectangle( x - 22, y - 9, 120, 17, new Color( 0.95f, 0.95f, 0.95f ), SongViewRect );
			}

			//Draw note rectangle
			DrawRectangle( x - 20, y - 7, 13, 13, color, SongViewRect );

			//Draw trail
			if( note.Length > 0 )
			{
				float trailYTop = y - note.Length * heightOfOneBeat;
				float trailYBot = y;

				DrawVerticalLine( new Vector2( x, trailYBot ), new Vector2( x, trailYTop ), color, 7, SongViewRect );
			}
			
			
			Rect rect = new Rect( x, y-8, 50, 20 );
			GUIStyle textStyle = new GUIStyle();
			textStyle.normal.textColor = new Color(0.5f,0.5f,0.5f);
			switch (note.Step.action ) {
				case CutsceneStep.Action.EndCutscene:
					DrawRectangle( x - 20, y - 7, 13, 13, Color.gray, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "EndCutscene",textStyle);
					break;
				case CutsceneStep.Action.Stop:
				DrawRectangle( x - 20, y - 7, 13, 13, Color.gray, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "Stop",textStyle);
					break;
				case CutsceneStep.Action.MoveTo:
					DrawRectangle( x - 20, y - 7, 13, 13, Color.green, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "MoveTo",textStyle);
					break;
				case CutsceneStep.Action.JumpTo:
					DrawRectangle( x - 20, y - 7, 13, 13, Color.green, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "JumpTo",textStyle);
					break;
				case CutsceneStep.Action.GoBack:
					DrawRectangle( x - 20, y - 7, 13, 13, Color.blue, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "GoBack",textStyle);
					break;
				case CutsceneStep.Action.WaitFor:
				DrawRectangle( x - 20, y - 7, 13, 13, Color.gray, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "WaitFor",textStyle);
					break;
				case CutsceneStep.Action.Follow:
					DrawRectangle( x - 20, y - 7, 13, 13, Color.blue, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "Follow",textStyle);
					break;
				case CutsceneStep.Action.Rotate:
					DrawRectangle( x - 20, y - 7, 13, 13, Color.blue, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "Rotate",textStyle);
					break;
				case CutsceneStep.Action.Caption:
					DrawRectangle( x - 20, y - 7, 13, 13, Color.gray, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "Caption",textStyle);
					break;
				case CutsceneStep.Action.TalkTo:
					DrawRectangle( x - 20, y - 7, 13, 13, Color.green, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "TalkTo",textStyle);
					break;
				case CutsceneStep.Action.SpeechTo:
					DrawRectangle( x - 20, y - 7, 13, 13, Color.green, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "SpeechTo",textStyle);
					break;
				case CutsceneStep.Action.Action:
					DrawRectangle( x - 20, y - 7, 13, 13, Color.green, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "Action",textStyle);
					break;
				case CutsceneStep.Action.LoopAction:
					DrawRectangle( x - 20, y - 7, 13, 13, Color.green, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "LoopAction",textStyle);
					break;
				case CutsceneStep.Action.WaitTouch:
				DrawRectangle( x - 20, y - 7, 13, 13, Color.gray, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "WaitTouch",textStyle);
					break;
				case CutsceneStep.Action.LoadScene:
				DrawRectangle( x - 20, y - 7, 13, 13, Color.gray, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "LoadScene",textStyle);
					break;
				case CutsceneStep.Action.Setup:
				DrawRectangle( x - 20, y - 7, 13, 13, Color.gray, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "Setup",textStyle);
					break;
				case CutsceneStep.Action.LookAt:
					DrawRectangle( x - 20, y - 7, 13, 13, Color.green, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "LookAt",textStyle);
					break;
				case CutsceneStep.Action.Activate:
					DrawRectangle( x - 20, y - 7, 13, 13, Color.blue, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "Activate",textStyle);
					break;
				case CutsceneStep.Action.Proc:
				DrawRectangle( x - 20, y - 7, 13, 13, Color.gray, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "Proc",textStyle);
					break;
				case CutsceneStep.Action.Sound:
				DrawRectangle( x - 20, y - 7, 13, 13, Color.gray, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "Sound",textStyle);
					break;
				case CutsceneStep.Action.ChangeInto:
					DrawRectangle( x - 20, y - 7, 13, 13, Color.blue, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "ChangeInto",textStyle);
					break;
				case CutsceneStep.Action.PlayMovie:
				DrawRectangle( x - 20, y - 7, 13, 13, Color.gray, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "PlayMovie",textStyle);
					break;
				case CutsceneStep.Action.UniqueAction:
					DrawRectangle( x - 20, y - 7, 13, 13, Color.green, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "UniqueAction",textStyle);
					break;
				case CutsceneStep.Action.Mood:
					DrawRectangle( x - 20, y - 7, 13, 13, Color.green, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "Mood",textStyle);
					break;
				case CutsceneStep.Action.WaitSound:
				DrawRectangle( x - 20, y - 7, 13, 13, Color.gray, SongViewRect );
				GUI.Label( rect, i.ToString()+". "+ "WaitSound",textStyle);
					break;
				case CutsceneStep.Action.GameLog: //GameStart, GameEnd, ChapterStart, ChapterEnd
				DrawRectangle( x - 20, y - 7, 13, 13, Color.gray, SongViewRect );
					GUI.Label( rect, i.ToString()+". "+ "GameLog",textStyle);
					break;
				case CutsceneStep.Action.ScaleTo:
					DrawRectangle( x - 20, y - 7, 13, 13, Color.blue, SongViewRect );
					GUI.Label( rect, i.ToString()+". "+ "ScaleTo",textStyle);
					break;
				default:
					DrawRectangle( x - 20, y - 7, 13, 13, Color.red, SongViewRect );
					GUI.Label( rect, i.ToString()+". "+ "Nono",textStyle);
					break;
				}
			
		}
	}

	//This function is a little bit iffy, 
	//It not only draws the grey circles which you can click
	//but it also handles the mouse clicks which add/remove notes
	protected void DrawGridNotesAndHandleMouseClicks()
	{

		//Grid notes are only drawn when the song is paused
		if( IsPlaying() )
		{
			return;
		}

		float heightOfOneBeat = SongViewRect.height / 6f;
		float totalWidth = SongViewRect.width - SongViewProgressBarWidth;
		float stringDistance = totalWidth / 6;
		float numNotesPerBeat = 4f;

		//Calculate the offset (from 0 to 1) how far the current beat has progressed
		float beatOffset = SongPlayer.GetCurrentBeat( true );
		//Debug.Log("upper" + beatOffset);
		beatOffset -= (int)beatOffset;

		//Debug.Log("under" + beatOffset);
		beatOffset = beatOffset + testOffsetVal;
		//Get the texture of the grey circles
		Texture2D GridNoteTexture = (Texture2D)UnityEngine.Resources.Load( "GridNote", typeof( Texture2D ) );

		//Draw on each of the five strings
		for( int i = 0; i < 5; ++i )
		{
			float x = stringDistance * ( i + 1 );

			for( int j = 0; j < 7 * numNotesPerBeat; ++j )
			{
				float y = SongViewRect.yMax - ( j / numNotesPerBeat - beatOffset ) * heightOfOneBeat;

				//Calculate beat value of this grid position
				float beat = (float)j / numNotesPerBeat + Mathf.Ceil( SongPlayer.GetCurrentBeat( true ) );

				Rect rect = new Rect( x, y - 7, 100, 13 );

				if( beat > SongPlayer.Song.GetLengthInBeats() )
				{
					//Dont draw grid notes beyond song length
					continue;
				}

				if( rect.yMin < SongViewRect.yMin && rect.yMax < SongViewRect.yMin )
				{
					//Dont draw grid notes that are not visible in the current frame
					continue;
				}

				if( rect.yMin > SongViewRect.yMax && rect.yMax > SongViewRect.yMax )
				{
					//Dont draw grid notes that are not visible in the current frame
					continue;
				}
				
				//Clip the draw rectangle to the song view
				rect.yMin = Mathf.Clamp( rect.yMin, SongViewRect.yMin, SongViewRect.yMax );
				rect.yMax = Mathf.Clamp( rect.yMax, SongViewRect.yMin, SongViewRect.yMax );
				
				if( GridNoteTexture != null )
					GUI.DrawTexture( rect, GridNoteTexture, ScaleMode.ScaleAndCrop, true );

				//Correct mouse offset
				y -= heightOfOneBeat;

				//Check if current grid note contains the mouse position
				//MouseUpPosition is set to Vector2( -1337, -1337 ) if no click occured this frame
				if( rect.Contains( MouseUpPosition ) )
				{
					//Correct beat offset in positive space
					if( SongPlayer.GetCurrentBeat( true ) > 0 )
					{
						beat -= 1;
					}

					//Check if a note is already present
					SelectedNote = SongPlayer.Song.GetNoteIndex( beat, i );

					if( SelectedNote == -1 )
					{
						//If note wasn't present, add the note on left mouse button click
						if( LastClickWasRightMouseButton == false )
						{
		//					Undo.RegisterUndo( SongPlayer.Song, "Add Note" );

							SelectedNote = SongPlayer.Song.AddNote( beat, i );
							EditorUtility.SetDirty( target );
							RedrawProgressViewTexture();
						}
					}
					else
					{
						//If note is present, remove the note on right mouse button click
//						if( LastClickWasRightMouseButton )
//						{
//							Undo.RegisterUndo( SongPlayer.Song, "Remove Note" );
//
//							SongPlayer.Song.RemoveNote( SelectedNote );
//							SelectedNote = -1;
//							EditorUtility.SetDirty( target );
//							RedrawProgressViewTexture();
//						}
					}

					Repaint();
				}
			}
		}

		//Reset mouse values
		MouseUpPosition = new Vector2( -1337, -1337 );
		LastClickWasRightMouseButton = false;
	}

	protected void DrawBeats()
	{
		float heightOfOneBeat = SongViewRect.height / 6f;

		//Calculate the offset (from 0 to 1) how far the current beat has progressed
		float beatOffset = SongPlayer.GetCurrentBeat( true );
		//Debug.Log("---> beat offset" + beatOffset);
		beatOffset -= (int)beatOffset;

		for( int i = 0; i < 7; ++i )
		{
			float y = SongViewRect.yMax - ( i - beatOffset ) * heightOfOneBeat;
			DrawLine( new Vector2( SongViewRect.xMin, y )
					, new Vector2( SongViewRect.xMax - SongViewProgressBarWidth, y )
					, new Color( 0.1f, 0.1f, 0.1f )
					, 2, SongViewRect );
		}
	}

	protected void DrawProgressView()
	{
		GUI.DrawTexture( GetProgressViewRect(), ProgressViewTexture );
		DrawProgressViewTimeMarker();
	}

	protected void DrawProgressViewBackground()
	{
		Rect rect  = GetProgressViewRect();
		DrawRectangle( rect.xMin, rect.yMin, rect.width, rect.height, new Color( 0.13f, 0.1f, 0.26f ) );
	}

	protected void DrawProgressViewTimeMarker()
	{
		Rect rect  = GetProgressViewRect();

		float previewProgress = 0f;
		if( GuitarObject && GuitarObject.audio.clip )
		{
			previewProgress = GuitarObject.audio.time / GuitarObject.audio.clip.length;
		}

		float previewProgressTop = rect.yMin + rect.height * ( 1 - previewProgress );
		DrawLine( new Vector2( rect.xMin, previewProgressTop ), new Vector2( rect.xMax + rect.width, previewProgressTop ), Color.red, 2 );
	}

	protected void OnMouseDown( Event e )
	{
		if( GetProgressViewRect().Contains( e.mousePosition ) )
		{
			OnProgressViewClicked( e.mousePosition );
		}
	}

	protected void OnMouseUp( Event e )
	{
		if( SongViewRect.Contains( e.mousePosition ) && !GetProgressViewRect().Contains( e.mousePosition ) )
		{
			OnSongViewMouseUp( e.mousePosition );

			if( e.button == 1 )
			{
				LastClickWasRightMouseButton = true;
			}
		}
	}

	protected void OnSongViewMouseUp( Vector2 mousePosition )
	{
		MouseUpPosition = mousePosition;

		Repaint();
	}

	protected void OnProgressViewClicked( Vector2 mousePosition )
	{
		Debug.Log("progress Clicked");
		float progress = 1 - (float)( mousePosition.y - SongViewRect.yMin ) / SongViewRect.height;

		GuitarObject.audio.time = GuitarObject.audio.clip.length * progress;
		testOffsetVal = 0;
	}

	protected void OnPlayPauseClicked()
	{
		EditorGUIUtility.keyboardControl = 0;

		if( IsPlaying() )
		{
			GuitarObject.audio.Pause();
			EditorUtility.SetDirty( target );
		}
		else
		{
			GuitarObject.audio.Play();
			GuitarObject.audio.Pause();
			GuitarObject.audio.Play();
		}
	}

	protected void OnStopClicked()
	{
		if( !GuitarObject )
		{
			return;
		}

		GuitarObject.audio.Stop();
		GuitarObject.audio.time = 0f;
//		LastMetronomeBeat = -Mathf.Ceil( SongPlayer.Song.AudioStartBeatOffset );
		EditorUtility.SetDirty( target );
	}

	//2D Draw Functions
	//Found on the unity forums: http://forum.unity3d.com/threads/17066-How-to-draw-a-GUI-2D-quot-line-quot/page2
	//Added clipping rectangle
	public static void DrawLine( Vector2 lineStart, Vector2 lineEnd, Color color, int thickness, Rect clip )
	{
		if( ( lineStart.y < clip.yMin && lineEnd.y < clip.yMin )
		 || ( lineStart.y > clip.yMax && lineEnd.y > clip.yMax )
		 || ( lineStart.x < clip.xMin && lineEnd.x < clip.xMin )
		 || ( lineStart.x > clip.xMax && lineEnd.x > clip.xMax ) )
		{
			return;
		}

		lineStart.x = Mathf.Clamp( lineStart.x, clip.xMin, clip.xMax );
		lineStart.y = Mathf.Clamp( lineStart.y, clip.yMin, clip.yMax );

		lineEnd.x = Mathf.Clamp( lineEnd.x, clip.xMin, clip.xMax );
		lineEnd.y = Mathf.Clamp( lineEnd.y, clip.yMin, clip.yMax );

		DrawLine( lineStart, lineEnd, color, thickness );
	}

	public static void DrawLine( Vector2 lineStart, Vector2 lineEnd, Color color, int thickness )
	{
		if( lineStart.x == lineStart.y )
		{
			DrawVerticalLine( lineStart, lineEnd, color, thickness );
		}

		if( !_coloredLineTexture )
		{
			_coloredLineTexture = new Texture2D( 1, 1 );
			_coloredLineTexture.wrapMode = TextureWrapMode.Repeat;
			_coloredLineTexture.hideFlags = HideFlags.HideAndDontSave;
		}

		if( _coloredLineColor != color )
		{
			_coloredLineColor = color;
			_coloredLineTexture.SetPixel( 0, 0, _coloredLineColor );
			_coloredLineTexture.Apply();
		}
		DrawLineStretched( lineStart, lineEnd, _coloredLineTexture, thickness );
	}

	public static void DrawVerticalLine( Vector2 lineStart, Vector2 lineEnd, Color color, int thickness, Rect clip )
	{
		if( ( lineStart.y < clip.yMin && lineEnd.y < clip.yMin )
		 || ( lineStart.y > clip.yMax && lineEnd.y > clip.yMax )
		 || ( lineStart.x < clip.xMin && lineEnd.x < clip.xMin )
		 || ( lineStart.x > clip.xMax && lineEnd.x > clip.xMax ) )
		{
			return;
		}

		lineStart.x = Mathf.Clamp( lineStart.x, clip.xMin, clip.xMax );
		lineStart.y = Mathf.Clamp( lineStart.y, clip.yMin, clip.yMax );

		lineEnd.x = Mathf.Clamp( lineEnd.x, clip.xMin, clip.xMax );
		lineEnd.y = Mathf.Clamp( lineEnd.y, clip.yMin, clip.yMax );

		DrawVerticalLine( lineStart, lineEnd, color, thickness );
	}

	public static void DrawVerticalLine( Vector2 lineStart, Vector2 lineEnd, Color color, int thickness )
	{
		if( lineStart.x != lineEnd.x )
		{
			DrawLine( lineStart, lineEnd, color, thickness );
			return;
		}

		float x = lineStart.x;
		float xOffset = (float)thickness;
		float y = lineStart.y + ( lineEnd.y - lineStart.y ) / 2;
		int newThickness = (int)( Mathf.Abs( Mathf.Floor( lineStart.y - lineEnd.y ) ) );

		DrawLine( new Vector2( x - xOffset / 2, y ), new Vector2( x + xOffset / 2, y ), color, newThickness );
	}

	public static void DrawLineStretched( Vector2 lineStart, Vector2 lineEnd, Texture2D texture, int thickness )
	{
		Vector2 lineVector = lineEnd - lineStart;

		if( lineVector.x == 0 )
		{
			return;
		}

		float angle = Mathf.Rad2Deg * Mathf.Atan( lineVector.y / lineVector.x );

		if( lineVector.x < 0 )
		{
			angle += 180;
		}

		if( thickness < 1 )
		{
			thickness = 1;
		}

		// The center of the line will always be at the center
		// regardless of the thickness.
		int thicknessOffset = (int)Mathf.Ceil( thickness / 2 );

		GUIUtility.RotateAroundPivot( angle, lineStart );

		GUI.DrawTexture( new Rect( lineStart.x,
								 lineStart.y - thicknessOffset,
								 lineVector.magnitude,
								 thickness ),
						texture );

		GUIUtility.RotateAroundPivot( -angle, lineStart );
	}

	private void DrawRectangle( float left, float top, float width, float height, Color color )
	{
		DrawRectangle( new Rect( left, top, width, height ), color );
	}

	private void DrawRectangle( float left, float top, float width, float height, Color color, Rect clip )
	{
		DrawRectangle( new Rect( left, top, width, height ), color, clip );
	}

	private void DrawRectangle( Rect rect, Color color, Rect clip )
	{
		DrawVerticalLine( new Vector2( rect.xMin + rect.width / 2, rect.yMin ), new Vector2( rect.xMin + rect.width / 2, rect.yMax ), color, (int)rect.width, clip );
	}

	private void DrawRectangle( Rect rect, Color color )
	{
		DrawRectangle( rect, color, rect );
	}
	
	protected void DrawCutsceneStep()
	{
		//EDIT : San 20140513, edit Layout

		if( SelectedNote != -1 )
		{
			currentStep = SongPlayer.Song.Notes[ SelectedNote ].Step;
		}
		
		CutsceneStep step = currentStep;
		if( step != null)
		{
			
			Color bgrndBak = GUI.backgroundColor;
			//Color colorBak = GUI.color;
			GUI.backgroundColor = LcCutsceneEditor.selectedId == step.stepId ? Color.cyan : bgrndBak;
			
			
			
			//GUI.color = colorBak;
			
			
			//
			//			GUI.color = step.actor != null ? step.actor.symbolColor : colorBak;
			//			if (GUILayout.Button ("", GUILayout.Width(20))) {
			//				SelectStep(step.stepId);
			//			}
			
			//			step.stepId = EditorGUILayout.IntField(step.stepId);
			//			step.preId = EditorGUILayout.IntField("Prev Id:", step.preId);
			//			step.delay = EditorGUILayout.FloatField("Delay:", step.delay);
			
			
			EditorGUILayout.BeginVertical("button");
			{
				
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField("Actor: ",GUILayout.Width(50));
					step.actor = (ActorBase) EditorGUILayout.ObjectField(step.actor, typeof(ActorBase), true
					                                                     , GUILayout.Width(150));
					EditorGUILayout.LabelField("WithAction: ",GUILayout.Width(80));
					step.boolVal = EditorGUILayout.Toggle(step.boolVal,GUILayout.Width(50));
				}

				
				if( step.actor != null )
				{
					if( step.actor.objIncludeActor != null )
					{
						step.actor = step.actor.objIncludeActor;
					}
				}
				
				
				tempAcitonDisplay = CutsceneStep.ConverterActionIDToActionDisplayID(step.action);
				EditorGUILayout.LabelField("Action: ",GUILayout.Width(50));
				tempAcitonDisplay = (CutsceneStep.ActionDisplay) EditorGUILayout.EnumPopup(tempAcitonDisplay, GUILayout.Width(100));
				step.action = CutsceneStep.ConverterActionDisplayIDToActionID(tempAcitonDisplay);
				EditorGUILayout.EndHorizontal();

				ActorChar actorChar = null;
				if (step.actor != null){
					actorChar = step.actor.GetComponent<ActorChar>();
				}
				
				
				
				
				switch (step.action) {
				case CutsceneStep.Action.MoveTo:
					EditorGUILayout.BeginHorizontal();
					{

						EditorGUILayout.LabelField("Speed:",GUILayout.Width(50));
						step.floatVal = EditorGUILayout.FloatField(step.floatVal, GUILayout.Width(70));
					

						EditorGUILayout.LabelField("  Pos:",GUILayout.Width(40));
						step.pos = EditorGUILayout.Vector3Field("",step.pos);
						if (GUILayout.Button("Save", GUILayout.Width(80))) {
							step.pos = step.actor.transform.position;
						}
						if (GUILayout.Button("Apply", GUILayout.Width(80))) {
							step.actor.transform.position = step.pos;
						}
					}EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField("MovePosTransform : ",GUILayout.Width(130));
						step.target = (Transform) EditorGUILayout.ObjectField( step.target , typeof(Transform), true);
					

						if( step.target != null )
						{
							step.pos = step.target.position;
							//						step.pos.x = step.target.position. x;
							//						step.pos.x = step.target.position.x;
							//						step.pos.x = step.target.position.x;
						}
						
						
						
						EditorGUILayout.LabelField("Path:", GUILayout.Width(40));
						step.obj = EditorGUILayout.ObjectField( step.obj, typeof(LcPath), true);

						EditorGUILayout.LabelField("EaseType:", GUILayout.Width(60));
						step.easeType = (iTween.EaseType) EditorGUILayout.EnumPopup(step.easeType);
					}EditorGUILayout.EndHorizontal();
					break;
					
				case CutsceneStep.Action.JumpTo:
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField("Speed:",GUILayout.Width(50));
						step.floatVal = EditorGUILayout.FloatField(step.floatVal, GUILayout.Width(70));

						EditorGUILayout.LabelField("   Pos:",GUILayout.Width(40));
						step.pos = EditorGUILayout.Vector3Field("",step.pos);
						
						if (GUILayout.Button("Save", GUILayout.Width(80))) {
							step.pos = step.actor.transform.position;
						}
						if (GUILayout.Button("Apply", GUILayout.Width(80))) {
							step.actor.transform.position = step.pos;
						}
					}EditorGUILayout.EndHorizontal();

					break;
					
				case CutsceneStep.Action.GoBack:

					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField("Speed:",GUILayout.Width(50));
						step.floatVal = EditorGUILayout.FloatField(step.floatVal, GUILayout.Width(70));

					
						EditorGUILayout.LabelField("EaseType:", GUILayout.Width(60));
						step.easeType = (iTween.EaseType) EditorGUILayout.EnumPopup(step.easeType,GUILayout.Width(300));
					}
					EditorGUILayout.EndHorizontal();
					break;
					
				case CutsceneStep.Action.WaitFor:
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField("Target:", GUILayout.Width(50)); 
						step.target = (Transform) EditorGUILayout.ObjectField(step.target, typeof(Transform), true);
						if( step.target != null )
						{
							ActorBase temptarget =  step.target.gameObject.GetComponent<ActorBase>();
							if( temptarget != null )
							{
								if( temptarget.objIncludeActor != null )
								{
									step.target = temptarget.objIncludeActor.transform;
								}
							}
						}
						EditorGUILayout.LabelField("Offset:", GUILayout.Width(50)); 
						step.pos = EditorGUILayout.Vector3Field("",step.pos);

						if (GUILayout.Button("Save", GUILayout.Width(80))) {
							step.pos = step.actor.transform.position - step.target.position;
						}
						if (GUILayout.Button("Apply", GUILayout.Width(80))) {
							step.actor.transform.position = step.target.position + step.pos;
						}
					}
					EditorGUILayout.EndHorizontal();

					break;
					
				case CutsceneStep.Action.Follow:
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField("Target:", GUILayout.Width(50)); 
						step.target = (Transform) EditorGUILayout.ObjectField(step.target, typeof(Transform), true);
						if( step.target != null )
						{
							ActorBase temptarget =  step.target.gameObject.GetComponent<ActorBase>();
							if( temptarget != null )
							{
								if( temptarget.objIncludeActor != null )
								{
									step.target = temptarget.objIncludeActor.transform;
								}
							}
						}
						EditorGUILayout.LabelField("Offset:", GUILayout.Width(50)); 
						step.pos = EditorGUILayout.Vector3Field("",step.pos);
						
						if (GUILayout.Button("Save", GUILayout.Width(80))) {
							step.pos = step.actor.transform.position - step.target.position;
						}
						if (GUILayout.Button("Apply", GUILayout.Width(80))) {
							step.actor.transform.position = step.target.position + step.pos;
						}
					}
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField("Speed:",GUILayout.Width(50));
						step.floatVal = EditorGUILayout.FloatField(step.floatVal, GUILayout.Width(70));
					}
					EditorGUILayout.EndHorizontal();
					break;
					
				case CutsceneStep.Action.Rotate:
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField("Angle:", GUILayout.Width(50)); 
						step.pos = EditorGUILayout.Vector3Field("",step.pos);
						if (GUILayout.Button("Save", GUILayout.Width(80))) {
							step.pos = step.actor.transform.eulerAngles;
						}
						if (GUILayout.Button("Apply", GUILayout.Width(80))) {
							step.actor.transform.eulerAngles = step.pos;
						}
					}
					EditorGUILayout.EndHorizontal();

					break;
				case CutsceneStep.Action.Caption:
					EditorGUILayout.BeginHorizontal();
					{
					if (dialogs != null)

						EditorGUILayout.LabelField("Dialog:", GUILayout.Width(60)); 
						step.intVal = EditorGUILayout.IntPopup(step.intVal, dialogs, dialogIds);
						EditorGUILayout.LabelField("Duration:", GUILayout.Width(60)); 
						step.floatVal = EditorGUILayout.FloatField(step.floatVal,GUILayout.Width(50));
					}
					EditorGUILayout.EndHorizontal();
					break;
				case CutsceneStep.Action.TalkTo:
				case CutsceneStep.Action.SpeechTo:
					if (step.actor != null) {
						ReadActionData(step.actor);
						if (dialogs != null)
							EditorGUILayout.BeginHorizontal();
							{
								if (dialogs != null)
									
								EditorGUILayout.LabelField("Dialog:", GUILayout.Width(50)); 
								step.intVal = EditorGUILayout.IntPopup(step.intVal, dialogs, dialogIds);
								EditorGUILayout.LabelField("Duration:", GUILayout.Width(60)); 
								step.floatVal = EditorGUILayout.FloatField(step.floatVal,GUILayout.Width(50));

						
								EditorGUILayout.LabelField("NextLoopAction:", GUILayout.Width(100)); 
								step.boolVal = EditorGUILayout.Toggle( step.boolVal);	
							}
							EditorGUILayout.EndHorizontal();
							
							EditorGUILayout.BeginHorizontal();
							{
								EditorGUILayout.LabelField("Target:", GUILayout.Width(50)); 
								step.target = (Transform) EditorGUILayout.ObjectField(step.target, typeof(Transform), true);
								
								if( step.target != null )
								{
									ActorBase temptarget =  step.target.gameObject.GetComponent<ActorBase>();
									if( temptarget != null )
									{
										if( temptarget.objIncludeActor != null )
										{
											step.target = temptarget.objIncludeActor.transform;
										}
									}
								}
								if (actorChar != null && actorChar.actionNames != null){
									EditorGUILayout.LabelField("Action:", GUILayout.Width(50));
									step.intVal2 = EditorGUILayout.IntPopup( step.intVal2, actorChar.actionNames, actorChar.actionIds);

								}else{
									EditorGUILayout.LabelField("Action:", GUILayout.Width(50));
									step.intVal2 = EditorGUILayout.IntField( step.intVal2);
								}
									
							}
							EditorGUILayout.EndHorizontal();
					
						

					}
					break;
					
				case CutsceneStep.Action.Action:
					if (step.actor != null) {
						if (actorChar != null)
						{
							ReadActionData(step.actor);
						}
						EditorGUILayout.BeginHorizontal();
						{
							if (actorChar != null && actorChar.actionNames != null)
							{
								EditorGUILayout.LabelField("Action:", GUILayout.Width(50));
								//Debug.Log ("actorChar.actionNames = "+actorChar.actionNames);
								step.intVal = EditorGUILayout.IntPopup( step.intVal, actorChar.actionNames, 
								                                       actorChar.actionIds);
							}else{
								EditorGUILayout.LabelField("Action:", GUILayout.Width(50));
								step.intVal = EditorGUILayout.IntField( step.intVal);
							}

							if (dialogs != null){
								EditorGUILayout.LabelField("Dialog:", GUILayout.Width(50));
								step.intVal2 = EditorGUILayout.IntPopup( step.intVal2, dialogs, dialogIds);
							}
						}
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("NextLoopAction:", GUILayout.Width(95));
							step.boolVal = EditorGUILayout.Toggle(step.boolVal, GUILayout.Width(50));	
							EditorGUILayout.LabelField("Ignore Blink:", GUILayout.Width(70));
							step.boolVal2 = EditorGUILayout.Toggle( step.boolVal2, GUILayout.Width(10));
						}
						EditorGUILayout.EndHorizontal();
					}
					break;
					
				case CutsceneStep.Action.LoopAction:
					if (step.actor != null) {
						if (actorChar != null)
							ReadLoopActionData(step.actor);
						EditorGUILayout.BeginHorizontal();
						{
							if (actorChar != null && actorChar.loopActionNames != null){
								EditorGUILayout.LabelField("Loop Action:", GUILayout.Width(80));
								step.intVal = EditorGUILayout.IntPopup( step.intVal,actorChar.loopActionNames, actorChar.loopActionIds);
							}else{
								EditorGUILayout.LabelField("Loop Action:", GUILayout.Width(80));
								step.intVal = EditorGUILayout.IntField( step.intVal);
							}


						}
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("Ignore Blink:", GUILayout.Width(70));
							step.boolVal = EditorGUILayout.Toggle( step.boolVal, GUILayout.Width(10));
						}
						EditorGUILayout.EndHorizontal();
							
					}
					break;
					
				case CutsceneStep.Action.LoadScene:
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField("Name:", GUILayout.Width(40));
						step.strVal = EditorGUILayout.TextField( step.strVal);
						EditorGUILayout.LabelField("Effect:", GUILayout.Width(40));
						step.intVal = EditorGUILayout.IntField(step.intVal, GUILayout.Width(200));
						EditorGUILayout.LabelField("Param:", GUILayout.Width(40));
						step.strVal2 = EditorGUILayout.TextField( step.strVal2);
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField("Return Cutscene:", GUILayout.Width(100));
						step.obj = EditorGUILayout.ObjectField(step.obj, typeof(CutsceneMgr), true,GUILayout.Width(200));
					}
					EditorGUILayout.EndHorizontal();
					break;
					
				case CutsceneStep.Action.Setup:
					EditorGUILayout.BeginHorizontal();
					{
						if (GUILayout.Button("Save", GUILayout.Width(80))) {
							if (step.actor == null) {
								UnityEngine.Object[] objs = GameObject.FindObjectsOfType(typeof(ActorBase));
								step.actorData = new ActorData[objs.Length];
								for (int i = 0; i < objs.Length; i++) {
									ActorBase actor = (ActorBase) objs[i];
									ActorData data = new ActorData();
									data.actor = actor;
									data.pos = actor.transform.position;
									data.rota = actor.transform.rotation;
									data.scale = actor.transform.localScale;
									step.actorData[i] = data;
								}
							}  else {
								step.actorData = new ActorData[1];
								ActorBase actor = step.actor;
								ActorData data = new ActorData();
								data.actor = actor;
								data.pos = actor.transform.position;
								data.rota = actor.transform.rotation;
								data.scale = actor.transform.localScale;
								step.actorData[0] = data;
							}
						}
						if (GUILayout.Button("Apply", GUILayout.Width(80))) {
							foreach (ActorData data in step.actorData) {
								if (data.actor != null) {
									data.actor.transform.position = data.pos;
									data.actor.transform.rotation = data.rota;
									data.actor.transform.localScale = data.scale;
								}
							}
						}
					}
					EditorGUILayout.EndHorizontal();
					break;
					
				case CutsceneStep.Action.LookAt:
					step.intVal = EditorGUILayout.IntPopup(step.intVal, lookAtNames, lookAtIds, GUILayout.Width(100));
					switch (step.intVal) {
					case 0:
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("Target:", GUILayout.Width(50));
							step.target = (Transform) EditorGUILayout.ObjectField( step.target, typeof(Transform), true, GUILayout.Width(350));

							
							if( step.target != null )
							{
								ActorBase temptarget =  step.target.gameObject.GetComponent<ActorBase>();
								if( temptarget != null )
								{
									if( temptarget.objIncludeActor != null )
									{
										step.target = temptarget.objIncludeActor.transform;
									}
								}
							}
						}
						EditorGUILayout.EndHorizontal();
						break;
						
					case 1:
						EditorGUILayout.BeginHorizontal();
						{

							EditorGUILayout.LabelField("Target:", GUILayout.Width(50));
							step.target = (Transform) EditorGUILayout.ObjectField(step.target, typeof(Transform), true, GUILayout.Width(350));
						
						
							if( step.target != null )
							{
								ActorBase temptarget =  step.target.gameObject.GetComponent<ActorBase>();
								if( temptarget != null )
								{
									if( temptarget.objIncludeActor != null )
									{
										step.target = temptarget.objIncludeActor.transform;
									}
								}
							}
							EditorGUILayout.LabelField("pos:", GUILayout.Width(50));
							step.pos = EditorGUILayout.Vector3Field("", step.pos, GUILayout.Width(240));
							if (GUILayout.Button("Save", GUILayout.Width(80))) {
								if (step.target != null)
									step.pos = step.target.position;
							}
						}
						EditorGUILayout.EndHorizontal();
						break;
					}
					/*				step.target = (Transform) EditorGUILayout.ObjectField("Target:", step.target, typeof(Transform), true);
			step.pos = EditorGUILayout.Vector3Field("Pos:", step.pos, GUILayout.Width(240));
			if (GUILayout.Button("Current", GUILayout.Width(80))) {
				step.pos = step.target.position;
			}
			step.boolVal = EditorGUILayout.Toggle("Position", step.boolVal);*/
					break;
					
				case CutsceneStep.Action.Activate:
					step.intVal = EditorGUILayout.IntPopup(step.intVal, sendToNames, actionIds, GUILayout.Width(100));
					EditorGUILayout.BeginHorizontal();
					{

						EditorGUILayout.LabelField("Target:", GUILayout.Width(50));
						step.target = (Transform) EditorGUILayout.ObjectField( step.target, typeof(Transform), true);
					}
					EditorGUILayout.EndHorizontal();

					//					if( step.target != null )
					//					{
					//						ActorBase temptarget =  step.target.gameObject.GetComponent<ActorBase>();
					//						if( temptarget != null )
					//						{
					//							if( temptarget.objIncludeActor != null )
					//							{
					//								step.target = temptarget.objIncludeActor.transform;
					//							}
					//						}
					//					}
					switch (step.intVal) {
					case 0:
					case 2:
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("Wait", GUILayout.Width(50));
							step.boolVal = EditorGUILayout.Toggle( step.boolVal);
						}
						EditorGUILayout.EndHorizontal();
						break;
					}
					if (step.intVal == 2) {
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("Method:", GUILayout.Width(50));
							step.strVal = EditorGUILayout.TextField( step.strVal);
						}
						EditorGUILayout.EndHorizontal();

							
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("Param:", GUILayout.Width(50));
							step.strVal2 = EditorGUILayout.TextField( step.strVal2);
						}
						EditorGUILayout.EndHorizontal();
					}
					break;
					
				case CutsceneStep.Action.Proc:
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField("Name:", GUILayout.Width(50));
						step.strVal = EditorGUILayout.TextField( step.strVal,GUILayout.Width(150));
						EditorGUILayout.LabelField("Param:", GUILayout.Width(50));
						step.strVal2 = EditorGUILayout.TextField( step.strVal2,GUILayout.Width(300));
						EditorGUILayout.LabelField("Wait", GUILayout.Width(50));
						step.boolVal = EditorGUILayout.Toggle(step.boolVal);
					}
					EditorGUILayout.EndHorizontal();
					break;
					
				case CutsceneStep.Action.Sound:
					//				step.intVal = EditorGUILayout.IntPopup(step.intVal, soundNames, actionIds, GUILayout.Width(80));
					//				step.obj = EditorGUILayout.ObjectField("Clip:", step.obj, typeof(AudioClip), false);
					//				if (step.intVal == 0)
					//					step.boolVal = EditorGUILayout.Toggle("Loop", step.boolVal);
					//				
					//					else if (step.intVal == 1)
					//					{
					//						step.obj = EditorGUILayout.ObjectField("Clip:", step.obj, typeof(AudioClip), false);
					//					}
					//				step.floatVal = EditorGUILayout.FloatField("Volume:", step.floatVal, GUILayout.Width(200));
					
					step.intVal = EditorGUILayout.IntPopup(step.intVal, soundNames, actionIds, GUILayout.Width(80));

					if (step.intVal == 0)
					{
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("Clip:", GUILayout.Width(50));
							step.obj = EditorGUILayout.ObjectField(step.obj, typeof(AudioClip), false);
							EditorGUILayout.LabelField("Loop", GUILayout.Width(50));
							step.boolVal = EditorGUILayout.Toggle( step.boolVal);
						}
						EditorGUILayout.EndHorizontal();
					}
					else if (step.intVal == 1)
					{
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("Clip:", GUILayout.Width(50));
							step.obj = EditorGUILayout.ObjectField(step.obj, typeof(AudioClip), false);
						}
						EditorGUILayout.EndHorizontal();
					}
					else if (step.intVal == 2)
					{
						EditorGUILayout.BeginHorizontal();
						{
							if (bgmNames != null)
								EditorGUILayout.LabelField("Song:", GUILayout.Width(50));
								step.intVal2 = EditorGUILayout.IntPopup( step.intVal2, bgmNames, bgmIds);

							EditorGUILayout.LabelField("Loop", GUILayout.Width(50));
							step.boolVal = EditorGUILayout.Toggle( step.boolVal);
						}
						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField("Volume:", GUILayout.Width(50));
						step.floatVal = EditorGUILayout.FloatField(step.floatVal, GUILayout.Width(200));
					}
					EditorGUILayout.EndHorizontal();
					break;

				case CutsceneStep.Action.ChangeInto: 
					step.intVal2 =EditorGUILayout.IntPopup(step.intVal2, changeIntoNames, changeIntoIds, GUILayout.Width(100));

				

					if (step.intVal2 == 0) {
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("Parts:", GUILayout.Width(50));
							step.obj = EditorGUILayout.ObjectField( step.obj, typeof(Transform), false,GUILayout.Width(200));
							EditorGUILayout.LabelField("Prop:", GUILayout.Width(50));
							step.intVal = EditorGUILayout.IntField( step.intVal,GUILayout.Width(50));
						}
						EditorGUILayout.EndHorizontal();
					}
					else if (step.intVal2 == 3) {
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("Parts:", GUILayout.Width(50));
							step.obj = EditorGUILayout.ObjectField(step.obj, typeof(Transform), false);
						}
						EditorGUILayout.EndHorizontal();
					}
					break;
					
				case CutsceneStep.Action.PlayMovie:
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField("Movie:", GUILayout.Width(50));
						step.strVal = EditorGUILayout.TextField(step.strVal);
						EditorGUILayout.LabelField("Cancel On Input:", GUILayout.Width(100));
						step.boolVal = EditorGUILayout.Toggle( step.boolVal);
					}
					EditorGUILayout.EndHorizontal();
					break;
					
				case CutsceneStep.Action.UniqueAction:
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField("Name:",  GUILayout.Width(50));
						step.strVal = EditorGUILayout.TextField(step.strVal,GUILayout.Width(200));
						EditorGUILayout.LabelField("Phase :", GUILayout.Width(50));
						step.intVal = EditorGUILayout.IntField( step.intVal,GUILayout.Width(50));
					}
					EditorGUILayout.EndHorizontal();
					break;
					
				case CutsceneStep.Action.Mood:
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField("Mood:",  GUILayout.Width(50));
						step.intVal = EditorGUILayout.IntPopup( step.intVal, moodNames, moodIds,GUILayout.Width(300));
					}
					EditorGUILayout.EndHorizontal();
					break;
					
				case CutsceneStep.Action.ScaleTo:
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField("Speed:",GUILayout.Width(50));
						step.floatVal = EditorGUILayout.FloatField(step.floatVal, GUILayout.Width(70));

						EditorGUILayout.LabelField("  Scale:",GUILayout.Width(40));
						step.pos = EditorGUILayout.Vector3Field("", step.pos);
						
						
						if (GUILayout.Button("Save", GUILayout.Width(80))) {
							step.pos = step.actor.transform.localScale;
						}
						if (GUILayout.Button("Apply", GUILayout.Width(80))) {
							step.actor.transform.localScale = step.pos;
						}
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField("EaseType:", GUILayout.Width(60));
						step.easeType = (iTween.EaseType) EditorGUILayout.EnumPopup(step.easeType, GUILayout.Width(300));
					}
					EditorGUILayout.EndHorizontal();
				
				break;
				case CutsceneStep.Action.WaitTouch:
					step.floatVal = EditorGUILayout.FloatField("Prompt Delay:", step.floatVal, GUILayout.Width(200));
					step.intVal = EditorGUILayout.IntField("Choice:", step.intVal, GUILayout.Width(200));
					
					step.boolVal = EditorGUILayout.Toggle("Send GameLog Touch", step.boolVal,GUILayout.Width (200));
					if( step.boolVal )
					{
						step.intVal2 = EditorGUILayout.IntField("ID:", step.intVal2, GUILayout.Width(200));
					}
					
					
					break;
					
				case CutsceneStep.Action.WaitSound:
					EditorGUILayout.BeginHorizontal();
					{
						 
						EditorGUILayout.LabelField("For:", GUILayout.Width(50));
						step.intVal = EditorGUILayout.IntPopup(step.intVal, 
						                                       new string[] { "(Any)", "Sound", "Blow", "Clap","BlowNoDelay","ClapNoDelay" }, 
						new int[] { 0, 1, 2, 3, 4, 5 } , GUILayout.Width(300));

						EditorGUILayout.LabelField("Timeout:", GUILayout.Width(50));
						step.floatVal = EditorGUILayout.FloatField(step.floatVal, GUILayout.Width(70));


						if( step.boolVal )
						{
							step.intVal2 = EditorGUILayout.IntField("ID:", step.intVal2, GUILayout.Width(200));
						}
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField("Send GameLog Voice", GUILayout.Width(150));
						step.boolVal = EditorGUILayout.Toggle( step.boolVal);
					}
					EditorGUILayout.EndHorizontal();
					break;
					
				case CutsceneStep.Action.GameLog: //GameStart, GameEnd, ChapterStart, ChapterEnd
					step.intVal = EditorGUILayout.IntPopup(step.intVal, gameLogNames, gameLogIds, GUILayout.Width(100));
					if( step.intVal < 2 ) //GameStart, GameEnd
					{
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("ID:", GUILayout.Width(50));
							step.intVal2 = EditorGUILayout.IntField( step.intVal2, GUILayout.Width(200));
						}
						EditorGUILayout.EndHorizontal();
					}
					break;
				}
				
				/*				
			step.floatVal = EditorGUILayout.FloatField("Value:", step.floatVal);
			step.target = EditorGUILayout.ObjectField("Target:", step.target, typeof(UnityEngine.Object), true);
							 */
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField("Comment: ", GUILayout.Width(60));
					step.note = EditorGUILayout.TextField(step.note);
				}EditorGUILayout.EndHorizontal();

				GUILayout.Space(1);
			}
			EditorGUILayout.EndVertical();
		}

	}
	static void ReadActionData(ActorBase actor) {
		if( actionReadDic == null )
			return;
		
		if (actionReadDic.ContainsKey(actor))
			return;
		
		ActorChar actorChar = actor.GetComponent<ActorChar>();
		if (actorChar != null) {
			actorChar.ReadActionData();
		}
		
		actionReadDic[actor] = actor;
	}
	
	static void ReadLoopActionData(ActorBase actor) {
		if( loopActionReadDic == null )
			return;
		
		if (loopActionReadDic.ContainsKey(actor))
			return;
		
		ActorChar actorChar = actor.GetComponent<ActorChar>();
		if (actorChar != null) {
			actorChar.ReadLoopActionData();
		}
		
		loopActionReadDic[actor] = actor;
	}
	
}