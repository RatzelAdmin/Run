/*
Cutscene editor
Written by Jong Lee
11/9/2012
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;

public class LcCutsceneEditor : EditorWindow {
	const int stepIdGap = 1000;
	static public int selectedId = 0;

	static CutsceneMgr mgr = null;
	static public string[] dialogs;
	static public int[] dialogIds;
	static public string[] bgmNames;
	static public int[] bgmIds;
	int[] actionIds = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25,
		26, 27, 28, 29, 30 };
//	int[] actionDisplayIds = { 0, 3, 11, 24, 18, 26, 7, 25, 10, 2, 27, 5, 15, 19, 1, 20, 16, 6, 23, 12, 17, 14, 13, 4, 
//		21,	8, 22, 9, 28, 29, 30 };
	
	//Vector2 scroll1 = Vector2.zero;
	static Dictionary<ActorBase, ActorBase> actionReadDic;
	static Dictionary<ActorBase, ActorBase> loopActionReadDic;
	string[] moodNames = { "default_001", "smile_002", "smile_003", "smile_004", "disappoint_001", "disappoint_002",
		"surprised_001", "surprised_002", "curiosity_001", "sleep_001","sleep_002","sleep_003", };
	int[] moodIds = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
	string[] changeIntoNames = { "Put On", "Restore", "Clear", "ClearAttach" };
	int[] changeIntoIds = { 0, 1, 2, 3 };
	string[] lookAtNames = { "Object", "Position", "Camera" };
	int[] lookAtIds = { 0, 1, 2 };
	string[] soundNames = { "BGM", "SE", "SONG" };
	string[] sendToNames = { "Activate", "Deactivate", "Message" };
	int[] gameLogIds = { 0, 1, 2, 3 };
	string[] gameLogNames = { "GameStart", "GameEnd", "ChapterStart", "ChapterEnd" };

	enum StepOp {NoOp, AddAfter, Del, Up, Down};
	
//	int tempActionId = 0; 
//	int tempActionDisplayId = 0; 
	CutsceneStep.ActionDisplay tempAcitonDisplay;
	
	[MenuItem ("Window/Cutscene Editor")]
    static void Init () {
//        EditorWindow wnd = EditorWindow.GetWindow (typeof (LcCutsceneEditor));
//        wnd.Show();
		
        LcCutsceneEditor window = (LcCutsceneEditor)EditorWindow.GetWindow(typeof (LcCutsceneEditor),false, "CutSceneEditor");
        //LcCutsceneEditor window = EditorWindow.GetWindow<LcCutsceneEditor>(true, "CutSceneEditor",true);
//        window.minSize = new Vector2(600.0f, 300.0f);
//        window.minSize = new Vector2(600.0f, 300.0f);
        window.wantsMouseMove = true;
        window.Show();
        EditorWindow.FocusWindowIfItsOpen(typeof (LcCutsceneEditor));
    }
    private const float kZoomMin = 0.1f;
    private const float kZoomMax = 1.0f;
 
    //private readonly Rect _zoomArea = new Rect(0.0f, 35.0f, 600.0f, 300.0f - 100.0f);
	private Rect _zoomArea;// = new Rect(0.0f, 35.0f, position.width , position.height - 35.0f);
    private float _zoom = 1.0f;
    private Vector2 _zoomCoordsOrigin = Vector2.zero;
	
    private Vector2 ConvertScreenCoordsToZoomCoords(Vector2 screenCoords)
    {
        return (screenCoords - _zoomArea.TopLeft()) / _zoom + _zoomCoordsOrigin;
    }
 
    private void CheckDrawRect()
    {
		if(_zoomCoordsOrigin.y < 0 )
			_zoomCoordsOrigin.y = 0;
		
		if(_zoomCoordsOrigin.y > mgr.steps.Count*145.0f )
			_zoomCoordsOrigin.y = mgr.steps.Count*100.0f;
	}
    private void DrawNonZoomArea()
    {
       // GUI.Box(new Rect(0.0f, 0.0f, 600.0f, 50.0f), "Adjust zoom of middle box with slider or mouse wheel.\nMove zoom area dragging with middle mouse button or Alt+left mouse button.");
        _zoom = EditorGUI.Slider(new Rect(20.0f, 20.0f, 600.0f, 15.0f), _zoom, kZoomMin, kZoomMax);
        //GUI.Box(new Rect(0.0f, 300.0f - 25.0f, 600.0f, 25.0f), "Unzoomed Box");
    }
	
    private void HandleEvents()
    {
        // Allow adjusting the zoom with the mouse wheel as well. In this case, use the mouse coordinates
        // as the zoom center instead of the top left corner of the zoom area. This is achieved by
        // maintaining an origin that is used as offset when drawing any GUI elements in the zoom area.
        if (Event.current.type == EventType.ScrollWheel)
        {
            //Vector2 screenCoordsMousePos = Event.current.mousePosition;
            Vector2 delta = Event.current.delta;
           // Vector2 zoomCoordsMousePos = ConvertScreenCoordsToZoomCoords(screenCoordsMousePos);
            float zoomDelta = -delta.y*10.0f;
//            float oldZoom = _zoom;
//            _zoom += zoomDelta;
//            _zoom = Mathf.Clamp(_zoom, kZoomMin, kZoomMax);
//            _zoomCoordsOrigin += (zoomCoordsMousePos - _zoomCoordsOrigin) - (oldZoom / _zoom) * (zoomCoordsMousePos - _zoomCoordsOrigin);
// 
			
            _zoomCoordsOrigin.y -= zoomDelta;
			
			CheckDrawRect();
            Event.current.Use();
        }
 
        // Allow moving the zoom area's origin by dragging with the middle mouse button or dragging
        // with the left mouse button with Alt pressed.
//        if (Event.current.type == EventType.MouseDrag &&
//            (Event.current.button == 0 && Event.current.modifiers == EventModifiers.Alt) ||
//            Event.current.button == 2)
//        {
//            Vector2 delta = Event.current.delta;
//            delta /= _zoom;
//            _zoomCoordsOrigin += delta;
// 
//            Event.current.Use();
//        }
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.UpArrow )
        {
            _zoomCoordsOrigin.y -= 50.0f;
			
			CheckDrawRect();
			
            Event.current.Use();
        }
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.DownArrow )
        {
            _zoomCoordsOrigin.y += 50.0f;
			
			CheckDrawRect();
			
            Event.current.Use();
        }
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Home )
        {
			_zoomCoordsOrigin.y = 0;
			
            Event.current.Use();
        }
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.End )
        {
			if (mgr != null)
				_zoomCoordsOrigin.y = mgr.steps.Count*100.0f-position.height;
			
            Event.current.Use();
        }
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.PageUp )
        {
			_zoomCoordsOrigin.y -= position.height;
			
			CheckDrawRect();
			
            Event.current.Use();
        }
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.PageDown )
        {
			_zoomCoordsOrigin.y += position.height;
			
			CheckDrawRect();
			
            Event.current.Use();
        }
    }
 
	void OnSelectionChange() {
		//Debug.Log ("OnSelectionChange");
		CutsceneMgr newMgr = null;
		if (Selection.activeGameObject != null) {
			newMgr = Selection.activeGameObject.GetComponent<CutsceneMgr>();
		}
		
		if (newMgr != null) {
			dialogs = null;
			bgmNames = null;
			
			//position reset : oneway48
			if( mgr != null )
			{
				if( newMgr.gameObject.name != mgr.gameObject.name )
					_zoomCoordsOrigin.y = 0;
			}

			
			
			mgr = newMgr;
			title = mgr.transform.name;// Selection.activeGameObject.name;
//			List<CutsceneMgr.Dialog2> list = CutsceneMgr.ReadDialogData(mgr.dialogData);
			CutsceneMgr.Dialog[] list = null;
			if (mgr.dialogData != null)
				list = mgr.dialogData.GetDialogs(EditorGlobal.langIdx);
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
			
			CutsceneMgr.BGM[] listBGM = null;
			
			if (mgr.BGMData != null)
				listBGM = mgr.BGMData.GetBGMs(EditorGlobal.langIdx);
			if (listBGM != null) {
				bgmNames = new string[listBGM.Length];
				bgmIds = new int[listBGM.Length];
				int i = 0;
				foreach (CutsceneMgr.BGM item in listBGM) {
					if (i == 0)
						bgmNames[i] = "(None)";
					else
					{
						if( item.clip != null )
							bgmNames[i] = string.Format ("{0}. {1}", i, item.clip.name);
					}
					bgmIds[i] = i;
					i++;
				}
			}
			
			if( actionReadDic != null )
				actionReadDic.Clear();
			if( loopActionReadDic != null )
				loopActionReadDic.Clear();
			actionReadDic = new Dictionary<ActorBase, ActorBase>();
			loopActionReadDic = new Dictionary<ActorBase, ActorBase>();
			
		} 
//		else {
//			mgr = newMgr;
//			title = "Cutscene";
			
			
//			if( mgr != null )
//			{
//				CutsceneMgr.Dialog[] list = null;
//				
//				if (mgr.dialogData != null)
//					list = mgr.dialogData.GetDialogs(EditorGlobal.langIdx);
//				
//				if (list != null) {
//					dialogs = new string[list.Length];
//					dialogIds = new int[list.Length];
//					int i = 0;
//					foreach (CutsceneMgr.Dialog item in list) {
//						if (i == 0)
//							dialogs[i] = item.caption;
//						else
//							dialogs[i] = string.Format ("{0}. {1}", i, item.caption);
//						dialogIds[i] = i;
//						i++;
//					}
//				}
//				
//				actionReadDic = new Dictionary<ActorBase, ActorBase>();
//				loopActionReadDic = new Dictionary<ActorBase, ActorBase>();
//			}
//		}
		
		Repaint();
	}
	
    void OnEnable() {
        OnSelectionChange();
    }
	
	void OnFocus() {
        OnSelectionChange();
	}
	
	void OnHierarchyChange() {
		OnSelectionChange();
	}
	
	void OnInspectorUpdate() {
		Repaint();
	}
	
    void OnGUI () {
		
		if (mgr == null) {
			EditorGUILayout.LabelField("Select Cutscene Object Or");
			
			if (GUILayout.Button("Create New Cutscene")) {
				GameObject obj = new GameObject ("Cutscene");
				obj.AddComponent ("CutsceneMgr");
				if (Selection.activeTransform != null)
					obj.transform.parent = Selection.activeTransform;
				Selection.activeGameObject = obj;
				
//				CutsceneMgr mgr1 = obj.GetComponent<CutsceneMgr>();
/*				mgr1.touchEffects = new Transform[4];
			mgr1.touchEffects[0] = AssetDatabase.LoadAssetAtPath(
				"Assets/DoraEffect/EffectPrefab/Dora_Touch_Reaction_eff.prefab", typeof(Transform)) as Transform;
			mgr1.touchEffects[1] = AssetDatabase.LoadAssetAtPath(
				"Assets/DoraEffect/EffectPrefab/Dora_Touch_Reaction_eff_squre.prefab", typeof(Transform)) as Transform;
			mgr1.touchEffects[2] = AssetDatabase.LoadAssetAtPath(
				"Assets/DoraEffect/EffectPrefab/Dora_Touch_Reaction_eff_star.prefab", typeof(Transform)) as Transform;
			mgr1.touchEffects[3] = AssetDatabase.LoadAssetAtPath(
				"Assets/DoraEffect/EffectPrefab/Dora_Touch_Reaction_eff_tri.prefab", typeof(Transform)) as Transform;*/
			//	mgr1.prePlugin = AssetDatabase.LoadAssetAtPath("Assets/Resources/Plugin.prefab"
//					, typeof(Transform)) as Transform;
			}
			
			return;
		}
		
		
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Release Mgr",GUILayout.Width(90)))
		{
			mgr = null;
			title = "Cutscene";
			dialogs = null;
			bgmNames = null;
			return;
		}
		mgr.note = EditorGUILayout.TextField("Cutscene Description:", mgr.note);
		EditorGUILayout.EndHorizontal();
		
		//scroll1 = EditorGUILayout.BeginScrollView(scroll1);
		HandleEvents();
		
		if (mgr.steps == null || mgr.steps.Count == 0) {
			if (GUILayout.Button("New Step")) {
				AddStep(null);
			}
		} else {
		
			DrawNonZoomArea();
	        // Within the zoom area all coordinates are relative to the top left corner of the zoom area
	        // with the width and height being scaled versions of the original/unzoomed area's width and height.
			_zoomArea = new Rect(0.0f, 35.0f, position.width , position.height - 35.0f);
	   		
		    EditorZoomArea.Begin(_zoom, _zoomArea);	
        	//GUILayout.BeginArea(new Rect(0.0f - _zoomCoordsOrigin.x, 35.0f - _zoomCoordsOrigin.y, position.width , mgr.steps.Count*80.0f));
		
        	GUILayout.BeginArea(new Rect(0.0f , 35.0f - _zoomCoordsOrigin.y, 1000.0f , mgr.steps.Count*145.0f));
        	//GUILayout.BeginArea(new Rect(0.0f , 35.0f - _zoomCoordsOrigin.y, position.width , mgr.steps.Count*100.0f));
			

			
			CutsceneStep step1 = null;
			StepOp stepOp = StepOp.NoOp;
			
			Color bgrndBak = GUI.backgroundColor;
			Color colorBak = GUI.color;
			foreach (CutsceneStep step in mgr.steps) {
				
				GUI.backgroundColor = selectedId == step.stepId ? Color.cyan : bgrndBak;
				
#if UNITY_EDITOR
				if( step.bCurrentStep )
					GUI.backgroundColor = Color.red;
					
#endif
				EditorGUILayout.BeginVertical("Button");
				
				EditorGUILayout.BeginHorizontal();

				GUI.color = step.actor != null ? step.actor.symbolColor : colorBak;
				if (GUILayout.Button ("", GUILayout.Width(20))) {
					SelectStep(step.stepId);
				}
				GUI.color = colorBak;
				
				step.stepId = EditorGUILayout.IntField(step.stepId);
				step.preId = EditorGUILayout.IntField("Prev Id:", step.preId);
				step.delay = EditorGUILayout.FloatField("Delay:", step.delay);

				if (GUILayout.Button("Up", GUILayout.Width(90))) {
					SelectStep(step.stepId);
					step1 = step;
					stepOp = StepOp.Up;
				}				
				if (GUILayout.Button("Down", GUILayout.Width(90))) {
					SelectStep(step.stepId);
					step1 = step;
					stepOp = StepOp.Down;
				}				
				if (GUILayout.Button("Delete", GUILayout.Width(90))) {
					if( EditorUtility.DisplayDialog("This Step Delete?"
						,"Are you sure you want to delete? ", "Yes", "No") )
					{
						step1 = step;
						stepOp = StepOp.Del;
					}
						
				}				
				if (GUILayout.Button("Add Step", GUILayout.Width(90))) {
					step1 = step;
					stepOp = StepOp.AddAfter;
				}				
				if (GUILayout.Button("Run To", GUILayout.Width(90))) {
					SelectStep(step.stepId);
					RunTo (step.stepId);
				}				
				
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				
//				step.objectIncludeActor = (GameObject) EditorGUILayout.ObjectField(step.objectIncludeActor
//					, typeof(GameObject), true, GUILayout.Width(150));
//				
//				if (step.objectIncludeActor != null)
//					step.actor = step.objectIncludeActor.GetComponentInChildren<ActorBase>();
				
				
				step.actor = (ActorBase) EditorGUILayout.ObjectField(step.actor, typeof(ActorBase), true
					, GUILayout.Width(150));
				
				if( step.actor != null )
				{
					if( step.actor.objIncludeActor != null )
					{
						step.actor = step.actor.objIncludeActor;
					}
				}
				
				tempAcitonDisplay = CutsceneStep.ConverterActionIDToActionDisplayID(step.action);
				tempAcitonDisplay = (CutsceneStep.ActionDisplay) EditorGUILayout.EnumPopup(tempAcitonDisplay, GUILayout.Width(100));
				step.action = CutsceneStep.ConverterActionDisplayIDToActionID(tempAcitonDisplay);
				
				
				ActorChar actorChar = null;
				if (step.actor != null)
					actorChar = step.actor.GetComponent<ActorChar>();
				
				switch (step.action) {
				case CutsceneStep.Action.MoveTo:
					step.pos = EditorGUILayout.Vector3Field("Pos:", step.pos, GUILayout.Width(240));
					if (GUILayout.Button("Save", GUILayout.Width(80))) {
						step.pos = step.actor.transform.position;
					}
					if (GUILayout.Button("Apply", GUILayout.Width(80))) {
						step.actor.transform.position = step.pos;
					}
					step.floatVal = EditorGUILayout.FloatField("Speed:", step.floatVal, GUILayout.Width(210));
					
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.BeginHorizontal();
					step.target = (Transform) EditorGUILayout.ObjectField("MovePosTransform", step.target
					, typeof(Transform), true , GUILayout.Width(400));
					if( step.target != null )
					{
						step.pos = step.target.position;
//						step.pos.x = step.target.position.x;
//						step.pos.x = step.target.position.x;
//						step.pos.x = step.target.position.x;
					}
					EditorGUILayout.EndHorizontal();
					
					EditorGUILayout.BeginHorizontal();
					step.boolVal = EditorGUILayout.Toggle("WithAction", step.boolVal,GUILayout.Width (200));
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
					
					step.obj = EditorGUILayout.ObjectField("Path:", step.obj, typeof(LcPath), true
						, GUILayout.Width(400));
					step.easeType = (iTween.EaseType) EditorGUILayout.EnumPopup("EaseType:", step.easeType
						, GUILayout.Width(300));
					break;
					
				case CutsceneStep.Action.JumpTo:
					step.pos = EditorGUILayout.Vector3Field("Pos:", step.pos, GUILayout.Width(240));
					if (GUILayout.Button("Save", GUILayout.Width(80))) {
						step.pos = step.actor.transform.position;
					}
					if (GUILayout.Button("Apply", GUILayout.Width(80))) {
						step.actor.transform.position = step.pos;
					}
					step.floatVal = EditorGUILayout.FloatField("Speed:", step.floatVal, GUILayout.Width(210));
					break;

				case CutsceneStep.Action.GoBack:
					step.floatVal = EditorGUILayout.FloatField("Speed:", step.floatVal, GUILayout.Width(210));
					step.easeType = (iTween.EaseType) EditorGUILayout.EnumPopup("EaseType:", step.easeType);
					break;
					
				case CutsceneStep.Action.WaitFor:
					step.target = (Transform) EditorGUILayout.ObjectField("Target:", step.target, typeof(Transform),
						true);
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
					step.pos = EditorGUILayout.Vector3Field("Offset:", step.pos, GUILayout.Width(240));
					if (GUILayout.Button("Save", GUILayout.Width(80))) {
						step.pos = step.actor.transform.position - step.target.position;
					}
					if (GUILayout.Button("Apply", GUILayout.Width(80))) {
						step.actor.transform.position = step.target.position + step.pos;
					}
					break;
					
				case CutsceneStep.Action.Follow:
					step.target = (Transform) EditorGUILayout.ObjectField("Target:", step.target, typeof(Transform),
						true);
					
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
					
				
					step.floatVal = EditorGUILayout.FloatField("Speed:", step.floatVal, GUILayout.Width(200));
//					step.boolVal = EditorGUILayout.Toggle("Vertical:", step.boolVal, GUILayout.Width(180));
					step.pos = EditorGUILayout.Vector3Field("Offset:", step.pos, GUILayout.Width(240));
					if (GUILayout.Button("Save", GUILayout.Width(80))) {
						step.pos = step.actor.transform.position - step.target.position;
					}
					if (GUILayout.Button("Apply", GUILayout.Width(80))) {
						step.actor.transform.position = step.target.position + step.pos;
					}
					break;
					
				case CutsceneStep.Action.Rotate:
					step.pos = EditorGUILayout.Vector3Field("Angle:", step.pos, GUILayout.Width(240));
					if (GUILayout.Button("Save", GUILayout.Width(80))) {
						step.pos = step.actor.transform.eulerAngles;
					}
					if (GUILayout.Button("Apply", GUILayout.Width(80))) {
						step.actor.transform.eulerAngles = step.pos;
					}
					break;
					
				case CutsceneStep.Action.Caption:
						if (dialogs != null)
							step.intVal = EditorGUILayout.IntPopup("Dialog:", step.intVal, dialogs, dialogIds);
						
						step.floatVal = EditorGUILayout.FloatField("Duration:", step.floatVal);

					break;
				case CutsceneStep.Action.TalkTo:
				case CutsceneStep.Action.SpeechTo:
					if (step.actor != null) {
						if (actorChar != null)
							ReadActionData(step.actor);
//						ReadActionData(step.actor);
//						actorChar = step.actor.GetComponent<ActorChar>();
						
						if (dialogs != null)
							step.intVal = EditorGUILayout.IntPopup("Dialog:", step.intVal, dialogs, dialogIds);
						
						step.floatVal = EditorGUILayout.FloatField("Duration:", step.floatVal);

						EditorGUILayout.EndHorizontal();
						EditorGUILayout.BeginHorizontal();				
						step.boolVal = EditorGUILayout.Toggle("NextLoopAction:", step.boolVal,GUILayout.Width (200));	
						
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.BeginHorizontal();
						step.target = (Transform) EditorGUILayout.ObjectField("Target:", step.target, typeof(Transform), 
							true);
						
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
			
						
						if (actorChar != null && actorChar.actionNames != null)
						{
							if( selectedId == step.stepId )
							{
								//Debug.Log ("actorChar"+actorChar);
//								foreach( string a in actorChar.actionNames )
//								{
//									//Debug.Log ("actorChar.actionNames"+a);
//								}
								
//								Debug.Log ("actorChar.actionNames"+actorChar.actionNames);
//								Debug.Log ("actorChar.actionIds"+actorChar.actionIds);
							}
//							step.intVal2 = EditorGUILayout.IntPopup("Action:", step.intVal2, actorChar.actionNames, 
//								actorChar.actionIds);
//							string[] temp = actorChar.actionNames;
							step.intVal2 = EditorGUILayout.IntPopup("Action:", step.intVal2, actorChar.actionNames, 
								actorChar.actionIds);
						}
						else
						{
							step.intVal2 = EditorGUILayout.IntField("Action:", step.intVal2);
						}
					}
					break;
					
				case CutsceneStep.Action.Action:
					if (step.actor != null) {
						if (actorChar != null)
							ReadActionData(step.actor);
						if (actorChar != null && actorChar.actionNames != null)
							step.intVal = EditorGUILayout.IntPopup("Action:", step.intVal, actorChar.actionNames, 
								actorChar.actionIds);
						else
							step.intVal = EditorGUILayout.IntField("Action:", step.intVal);
						
						if (dialogs != null)
							step.intVal2 = EditorGUILayout.IntPopup("Dialog:", step.intVal2, dialogs, dialogIds);
						
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.BeginHorizontal();
						step.boolVal = EditorGUILayout.Toggle("NextLoopAction:", step.boolVal, GUILayout.Width(200));	
						step.boolVal2 = EditorGUILayout.Toggle("Ignore Blink:", step.boolVal2, GUILayout.Width(200));	
						
					}
					break;

				case CutsceneStep.Action.LoopAction:
					if (step.actor != null) {
						if (actorChar != null)
							ReadLoopActionData(step.actor);
						if (actorChar != null && actorChar.loopActionNames != null)
							step.intVal = EditorGUILayout.IntPopup("Loop Action:", step.intVal, 
								actorChar.loopActionNames, actorChar.loopActionIds);
						else
							step.intVal = EditorGUILayout.IntField("Loop Action:", step.intVal);
						
						step.boolVal = EditorGUILayout.Toggle("Ignore Blink:", step.boolVal, GUILayout.Width(200));	
					}
//					step.intVal = EditorGUILayout.IntField("No.:", step.intVal, GUILayout.Width(240));
//					step.floatVal = EditorGUILayout.FloatField("Duration:", step.floatVal, GUILayout.Width(200));
					break;
					
				case CutsceneStep.Action.LoadScene:
					step.strVal = EditorGUILayout.TextField("Name:", step.strVal);
					step.intVal = EditorGUILayout.IntField("Effect:", step.intVal, GUILayout.Width(200));
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();					
					step.strVal2 = EditorGUILayout.TextField("Param:", step.strVal2);
					step.obj = EditorGUILayout.ObjectField("Return Cutscene:", step.obj, typeof(CutsceneMgr), true);
					break;
					
				case CutsceneStep.Action.Setup:
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
						} else {
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
					break;
					
				case CutsceneStep.Action.LookAt:
					step.intVal = EditorGUILayout.IntPopup(step.intVal, lookAtNames, lookAtIds, GUILayout.Width(100));
					switch (step.intVal) {
					case 0:
						step.target = (Transform) EditorGUILayout.ObjectField("Target:", step.target, typeof(Transform), 
							true, GUILayout.Width(350));
						
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
						break;
						
					case 1:
						step.target = (Transform) EditorGUILayout.ObjectField("Target:", step.target, typeof(Transform), 
							true, GUILayout.Width(350));
						
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
						step.pos = EditorGUILayout.Vector3Field("Pos:", step.pos, GUILayout.Width(240));
						if (GUILayout.Button("Save", GUILayout.Width(80))) {
							if (step.target != null)
							step.pos = step.target.position;
						}
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
					step.target = (Transform) EditorGUILayout.ObjectField(
						"Target:", step.target, typeof(Transform), true);
					
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
						step.boolVal = EditorGUILayout.Toggle("Wait", step.boolVal);
						break;
					}
					if (step.intVal == 2) {
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.BeginHorizontal();					
						step.strVal = EditorGUILayout.TextField("Method:", step.strVal);
						step.strVal2 = EditorGUILayout.TextField("Param:", step.strVal2);
					}
					break;
					
				case CutsceneStep.Action.Proc:
					step.strVal = EditorGUILayout.TextField("Name:", step.strVal);
					step.strVal2 = EditorGUILayout.TextField("Param:", step.strVal2);
					step.boolVal = EditorGUILayout.Toggle("Wait", step.boolVal);
					break;
					
				case CutsceneStep.Action.Sound:
					step.intVal = EditorGUILayout.IntPopup(step.intVal, soundNames, actionIds, GUILayout.Width(80));
					if (step.intVal == 0)
					{
						step.obj = EditorGUILayout.ObjectField("Clip:", step.obj, typeof(AudioClip), false);
						step.boolVal = EditorGUILayout.Toggle("Loop", step.boolVal);
					}
					else if (step.intVal == 1)
					{
						step.obj = EditorGUILayout.ObjectField("Clip:", step.obj, typeof(AudioClip), false);
					}
					else if (step.intVal == 2)
					{
						if (bgmNames != null)
							step.intVal2 = EditorGUILayout.IntPopup("Song:", step.intVal2, bgmNames, bgmIds);
						
						step.boolVal = EditorGUILayout.Toggle("Loop", step.boolVal);
					}
					
					step.floatVal = EditorGUILayout.FloatField("Volume:", step.floatVal, GUILayout.Width(200));
					break;
					
				case CutsceneStep.Action.ChangeInto:
					step.intVal2 =EditorGUILayout.IntPopup(step.intVal2, changeIntoNames, changeIntoIds, 
						GUILayout.Width(100));
					if (step.intVal2 == 0) {
						step.obj = EditorGUILayout.ObjectField("Parts:", step.obj, typeof(Transform), false);
						step.intVal = EditorGUILayout.IntField("Prop:", step.intVal);
					}
					else if (step.intVal2 == 3) {
						step.obj = EditorGUILayout.ObjectField("Parts:", step.obj, typeof(Transform), false);
					}
					break;
					
				case CutsceneStep.Action.PlayMovie:
					step.strVal = EditorGUILayout.TextField("Movie:", step.strVal);
					step.boolVal = EditorGUILayout.Toggle("Cancel On Input:", step.boolVal);
					break;
					
				case CutsceneStep.Action.UniqueAction:
					step.strVal = EditorGUILayout.TextField("Name:", step.strVal);
					step.intVal = EditorGUILayout.IntField("Phase", step.intVal);
					break;
					
				case CutsceneStep.Action.Mood:
					step.intVal = EditorGUILayout.IntPopup("Mood:", step.intVal, moodNames, moodIds);
					break;
					
				case CutsceneStep.Action.ScaleTo:
					step.pos = EditorGUILayout.Vector3Field("Scale:", step.pos, GUILayout.Width(240));
					if (GUILayout.Button("Save", GUILayout.Width(80))) {
						step.pos = step.actor.transform.localScale;
					}
					if (GUILayout.Button("Apply", GUILayout.Width(80))) {
						step.actor.transform.localScale = step.pos;
					}
					step.floatVal = EditorGUILayout.FloatField("Speed:", step.floatVal, GUILayout.Width(210));
					
					EditorGUILayout.EndHorizontal();
	
					EditorGUILayout.BeginHorizontal();
					
					step.easeType = (iTween.EaseType) EditorGUILayout.EnumPopup("EaseType:", step.easeType, 
						GUILayout.Width(300));
					break;
					
				case CutsceneStep.Action.WaitTouch:
					step.floatVal = EditorGUILayout.FloatField("Prompt Delay:", step.floatVal, GUILayout.Width(200));
					step.intVal = EditorGUILayout.IntField("Choice:", step.intVal, GUILayout.Width(200));
				
					EditorGUILayout.EndHorizontal();
					step.boolVal = EditorGUILayout.Toggle("Send GameLog Touch", step.boolVal,GUILayout.Width (200));
					if( step.boolVal )
					{
						step.intVal2 = EditorGUILayout.IntField("ID:", step.intVal2, GUILayout.Width(200));
					}
					
					EditorGUILayout.BeginHorizontal();
					break;
					
				case CutsceneStep.Action.WaitSound:
					step.intVal = EditorGUILayout.IntPopup("For:", step.intVal, 
					new string[] { "(Any)", "Sound", "Blow", "Clap","BlowNoDelay","ClapNoDelay" }, 
					new int[] { 0, 1, 2, 3, 4, 5 } , GUILayout.Width(300));
					step.floatVal = EditorGUILayout.FloatField("Timeout:", step.floatVal, GUILayout.Width(200));
					
					EditorGUILayout.EndHorizontal();
					step.boolVal = EditorGUILayout.Toggle("Send GameLog Voice", step.boolVal,GUILayout.Width (200));
					if( step.boolVal )
					{
						step.intVal2 = EditorGUILayout.IntField("ID:", step.intVal2, GUILayout.Width(200));
					}
					
					EditorGUILayout.BeginHorizontal();
					break;
					
				case CutsceneStep.Action.GameLog: //GameStart, GameEnd, ChapterStart, ChapterEnd
					step.intVal = EditorGUILayout.IntPopup(step.intVal, gameLogNames, gameLogIds, GUILayout.Width(100));
					if( step.intVal < 2 ) //GameStart, GameEnd
					{
						step.intVal2 = EditorGUILayout.IntField("ID:", step.intVal2, GUILayout.Width(200));
					}
					break;
				}
				
/*				
				step.floatVal = EditorGUILayout.FloatField("Value:", step.floatVal);
				step.target = EditorGUILayout.ObjectField("Target:", step.target, typeof(UnityEngine.Object), true);
								 */
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();
				
				step.note = EditorGUILayout.TextField("Comment:", step.note);
				
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.EndVertical();
				
				GUILayout.Space(1);
			}
			
			switch (stepOp) {
			case StepOp.AddAfter:
				AddStep(step1);
				break;
				
			case StepOp.Del:
				RemoveStep (step1);
				break;
				
			case StepOp.Up:
				int idx = mgr.steps.IndexOf(step1);
				if (idx > 0) {
					mgr.steps.Remove(step1);
					mgr.steps.Insert(idx - 1, step1);
				}
				break;

			case StepOp.Down:
				idx = mgr.steps.IndexOf(step1);
				if (idx < mgr.steps.Count - 1) {
					mgr.steps.Remove(step1);
					mgr.steps.Insert(idx + 1, step1);
				}
				break;
			}
			
			
	        GUILayout.EndArea();
	        EditorZoomArea.End();
			
			
		}
		
		
		//EditorGUILayout.EndScrollView();
		
    }
	
	void Callback (object obj) {
        Debug.Log ("Selected: " + obj);
    }

	void AddStep(CutsceneStep before) {
		if (mgr.steps == null)
			mgr.steps = new List<CutsceneStep>();
		
		CutsceneStep newStep = new CutsceneStep();

		if (before == null) {
			newStep.stepId = stepIdGap;
			mgr.steps.Add (newStep);
			return;
		}
		
		if (Selection.activeTransform != null) {
			newStep.actor = before.actor;
		}

		int nextId = int.MaxValue;
		foreach (CutsceneStep step in mgr.steps) {
			if (step.stepId > before.stepId) {
				nextId = Math.Min (nextId, step.stepId);
			}
		}
		
		newStep.stepId = before.stepId + stepIdGap;
		if (nextId != int.MaxValue) {
			newStep.stepId = Math.Min(before.stepId + (nextId - before.stepId) / 2, newStep.stepId);
		}
		
		newStep.preId = before.stepId;
		
		for (int i = 0; i < mgr.steps.Count; i++) {
			if (mgr.steps[i] == before) {
				mgr.steps.Insert(i + 1, newStep);
				
				break;
			}
		}
		
		SelectStep(newStep.stepId);
	}
	
	void RemoveStep(CutsceneStep step) {
		mgr.steps.Remove(step);
	}
	
	Dictionary<ActorBase, CutsceneStep> followSteps;
	
	void RunTo(int stepId) {
		List<CutsceneStep> steps = new List<CutsceneStep>(mgr.steps);
		Dictionary<int, float> doneSteps = new Dictionary<int, float>();
		followSteps = new Dictionary<ActorBase, CutsceneStep>();
		
		bool loop = true;
		while (loop) {
			int cnt = steps.Count;
			
			for (int i = 0; i < steps.Count; ) {
				CutsceneStep step = steps[i];
				bool found = false;
				
				if (step.preId == 0) {
					found = true;
				} else {
					if (doneSteps.ContainsKey(step.preId)) {
						found = true;
					}
				}
				
				if (found) {
					DoStep(step);
					if (step.action == CutsceneStep.Action.Follow) {
						followSteps.Add (step.actor, step);
					}
					if (step.stepId == stepId) {
						loop = false;
						break;
					}
					steps.RemoveAt(i);
					doneSteps.Add (step.stepId, 0);
				} else
					i++;
			}
			
			foreach (var pair in followSteps) {
				CutsceneStep step = pair.Value;
				Vector3 speed = step.actor.GetFollowVec(step.target.transform, step.pos, step.boolVal);
				step.actor.transform.position += speed;
			}
			
			if (cnt == steps.Count)
				break;
		}
	}
	
	void DoStep(CutsceneStep step) {
		switch (step.action) {
		case CutsceneStep.Action.MoveTo:
		case CutsceneStep.Action.JumpTo:
			if (step.obj) {
				step.actor.transform.position = ((LcPath) step.obj).forms[0].position;
			} else
				step.actor.transform.position = step.pos;
			if (followSteps.ContainsKey(step.actor))
				followSteps.Remove(step.actor);
			break;
			
		case CutsceneStep.Action.Follow:
			if (followSteps.ContainsKey(step.actor))
				followSteps.Remove(step.actor);
			break;
			
		case CutsceneStep.Action.Rotate:
			step.actor.transform.eulerAngles = step.pos;
			break;
			
		case CutsceneStep.Action.TalkTo:
			break;
			
		case CutsceneStep.Action.WaitFor:
			break;
		
		case CutsceneStep.Action.Action:
			break;
			
		case CutsceneStep.Action.WaitTouch:
			break;
			
		case CutsceneStep.Action.LoadScene:
			break;
			
		case CutsceneStep.Action.Setup:
			foreach (ActorData data in step.actorData) {
				if (data.actor != null) {
					data.actor.transform.position = data.pos;
					data.actor.transform.rotation = data.rota;
					data.actor.transform.localScale = data.scale;
				}
			}
			break;
		}
	}
	
	void SelectStep(int id) {
		selectedId = id;
	}
	
	static public void ReadActionData(ActorBase actor) {
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
	
	static public void ReadLoopActionData(ActorBase actor) {
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
