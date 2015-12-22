/*
Cutscene timeline
Written by Jong Lee
12/6/2012
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;

public class LcCutsceneTimeline : EditorWindow {
	public class ActorSteps {
		public ActorSteps(ActorBase _actor) {
			actor = _actor;
		}
		
		public ActorBase actor;
		public List<CutsceneStep> steps = new List<CutsceneStep>();
	}

	const int stepIdGap = 100;

	static CutsceneMgr mgr;
	CutsceneStep currentStep;
	//Vector2 scroll1 = Vector2.zero;
	
	int[] actionIds = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25,
		26, 27, 28, 29, 30 };
	//int[] actionDisplayIds = { 0, 3, 11, 24, 18, 26, 7, 25, 10, 2, 27, 5, 15, 19, 1, 20, 16, 6, 23, 12, 17, 14, 13, 4, 
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
	
//	int tempActionId = 0; 
//	int tempActionDisplayId = 0; 
	CutsceneStep.ActionDisplay tempAcitonDisplay;
	
//	private List<CutsceneStep> steps = null;
//	
//	private List<ActorSteps> actorList = new List<ActorSteps>();
//	private List<CutsceneStep> currSteps = new List<CutsceneStep>();
//	private List<CutsceneStep> nextSteps = new List<CutsceneStep>();
//	private CutsceneStep currStep = null;
	
	
	
	[MenuItem ("Window/Cutscene Timeline")]
    static void Init () {
//        EditorWindow wnd = EditorWindow.GetWindow (typeof (LcCutsceneTimeline));
//        wnd.Show();
		
		
        LcCutsceneTimeline window = (LcCutsceneTimeline)EditorWindow.GetWindow(typeof (LcCutsceneTimeline),false, "CutSceneEditor TimeLine");
        window.wantsMouseMove = true;
        window.Show();
        EditorWindow.FocusWindowIfItsOpen(typeof (LcCutsceneTimeline));
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
	
    private void HandleEvents()
    {
        // Allow adjusting the zoom with the mouse wheel as well. In this case, use the mouse coordinates
        // as the zoom center instead of the top left corner of the zoom area. This is achieved by
        // maintaining an origin that is used as offset when drawing any GUI elements in the zoom area.
        if (Event.current.type == EventType.ScrollWheel)
        {
            Vector2 screenCoordsMousePos = Event.current.mousePosition;
            Vector2 delta = Event.current.delta;
            Vector2 zoomCoordsMousePos = ConvertScreenCoordsToZoomCoords(screenCoordsMousePos);
            float zoomDelta = -delta.y / 150.0f;
            float oldZoom = _zoom;
            _zoom += zoomDelta;
            _zoom = Mathf.Clamp(_zoom, kZoomMin, kZoomMax);
            _zoomCoordsOrigin += (zoomCoordsMousePos - _zoomCoordsOrigin) - (oldZoom / _zoom) * (zoomCoordsMousePos - _zoomCoordsOrigin);
 
            Event.current.Use();
        }
 
        // Allow moving the zoom area's origin by dragging with the middle mouse button or dragging
        // with the left mouse button with Alt pressed.
        if (Event.current.type == EventType.MouseDrag &&
            (Event.current.button == 0 && Event.current.modifiers == EventModifiers.Alt) ||
            Event.current.button == 2)
        {
            Vector2 delta = Event.current.delta;
            delta /= _zoom;
            _zoomCoordsOrigin -= delta;
 
            Event.current.Use();
        }
    }
	void OnSelectionChange() {
		CutsceneMgr newMgr = null;
		if (Selection.activeGameObject != null) {
			newMgr = Selection.activeGameObject.GetComponent<CutsceneMgr>();
		}
		
		if (newMgr != null) {
			mgr = newMgr;
			title = Selection.activeGameObject.name;
		} 
//		else {
//			mgr = newMgr;
//			title = "Timeline";
//		}
		
		Repaint();
	}
	
    void OnEnable() {
        OnSelectionChange();
    }
	
	void OnHierarchyChange() {
		OnSelectionChange();
	}
	
	void OnFocus() {
        OnSelectionChange();
	}
	
	void OnInspectorUpdate() {
		Repaint();
	}	
	
	void OnGUI () {
		if (mgr == null) {
			EditorGUILayout.LabelField("Select Cutscene Object");
			return;
		}
		
		if (mgr.steps == null || mgr.steps.Count == 0) {
			return;
		}
		
		_zoomArea = new Rect(0.0f, 35.0f, position.width , position.height - 35.0f-150.0f);
		
		HandleEvents();
		EditorZoomArea.Begin(_zoom, _zoomArea);	
        GUILayout.BeginArea(new Rect(0.0f - _zoomCoordsOrigin.x , 35.0f - _zoomCoordsOrigin.y, 120.0f+mgr.steps.Count*120.0f , position.height-35.0f));
		
//		steps = mgr.steps;
//		currStep = null;
		List<CutsceneStep> steps = mgr.steps;
		
		List<ActorSteps> actorList = new List<ActorSteps>();
		List<CutsceneStep> currSteps = new List<CutsceneStep>();
		List<CutsceneStep> nextSteps = new List<CutsceneStep>();
		CutsceneStep currStep = null;
		
		int iLvl = 0;
		int iMaxLvl = 0;
		while (steps != null) {
			foreach (CutsceneStep step in steps) {
				
				List<CutsceneStep> list = null;
				foreach (ActorSteps actorSteps in actorList) {
					if (actorSteps.actor == step.actor) {
						list = actorSteps.steps;
						break;
					}
				}
				if (list == null) {
					ActorSteps actorSteps = new ActorSteps(step.actor);
					actorList.Add (actorSteps);
					list = actorSteps.steps;
				}
				
				if (!list.Contains(step)) {
					if (step.preId != step.stepId) {
						if ((currStep == null && step.preId == 0) || (currStep != null && currStep.stepId > 0 && step.preId == currStep.stepId)) {
							step.prevStep = currStep;
							while (list.Count < iLvl)
								list.Add(null);
							list.Add (step);
							iMaxLvl = Math.Max (iMaxLvl, list.Count);
							nextSteps.Add (step);
						}
					}
				}
			}

			if (currSteps.Count == 0) {
				if (nextSteps.Count == 0)
					break;
				
				currSteps = nextSteps;
				nextSteps = new List<CutsceneStep>();
				iLvl = iMaxLvl;
			}
			
			currStep = currSteps[0];
			currSteps.RemoveAt(0);
		}
/*
		if (GUILayout.Button("Add New Step", GUILayout.Width(120))) {
			AddStep(null);
		}
				 */
		EditorGUILayout.Space();
		
		//scroll1 = EditorGUILayout.BeginScrollView(scroll1);
		
		foreach (ActorSteps actorSteps in actorList) {
			EditorGUILayout.BeginHorizontal("Box");
			
			Color colorBak = GUI.color;
			GUI.color = actorSteps.actor == null ? colorBak : actorSteps.actor.symbolColor;
			GUILayout.Label (actorSteps.actor == null ? "None" : actorSteps.actor.name, GUILayout.Width(100));
			GUI.color = colorBak;
			
			foreach (CutsceneStep step in actorSteps.steps) {
				if (step == null) {
					GUILayout.Label ("", GUILayout.Width(120));
				} else {
					
#if UNITY_EDITOR
					if( step.bCurrentStep )
						GUI.backgroundColor = Color.red;
#endif
//					if (selectedStep == step)
					if (LcCutsceneEditor.selectedId == step.stepId)
						GUI.backgroundColor = Color.cyan;
					EditorGUILayout.BeginHorizontal("Box", GUILayout.Width(120));
					
					if (step.prevStep != null) {
						GUI.color = step.prevStep.actor == null ? colorBak : step.prevStep.actor.symbolColor;
						GUILayout.Label (step.prevStep.actor == null ? "N" : step.prevStep.actor.name.Substring(0, 1), GUILayout.Width(20));
						GUI.color = colorBak;
					} else {
						GUILayout.Label ("", GUILayout.Width(20));
					}
					
					if (GUILayout.Button (step.action.ToString(), GUILayout.Width(80))) {
						SelectStep(step.stepId);
						currentStep = step;
						dialogs = LcCutsceneEditor.dialogs;
						dialogIds = LcCutsceneEditor.dialogIds;
						bgmNames = LcCutsceneEditor.bgmNames;
						bgmIds = LcCutsceneEditor.bgmIds;
					}
					EditorGUILayout.EndHorizontal();
					
					GUI.backgroundColor = colorBak;
				}
			}
			
			EditorGUILayout.EndHorizontal();
			
//			EditorGUILayout.Space();
		}

		//EditorGUILayout.EndScrollView();
		
        GUILayout.EndArea();
        EditorZoomArea.End();
		
		DrawNonZoomArea();
	}
	
	void SelectStep(int id) {
		LcCutsceneEditor.selectedId = id;
	}
	
    private void DrawNonZoomArea()
    {
       // GUI.Box(new Rect(0.0f, 0.0f, 600.0f, 50.0f), "Adjust zoom of middle box with slider or mouse wheel.\nMove zoom area dragging with middle mouse button or Alt+left mouse button.");
        _zoom = EditorGUI.Slider(new Rect(20.0f, 20.0f, 600.0f, 15.0f), _zoom, kZoomMin, kZoomMax);
        //GUI.Box(new Rect(0.0f, 300.0f - 25.0f, 600.0f, 25.0f), "Unzoomed Box");
		
		
        GUI.Box(new Rect(0.0f, position.height - 150.0f, position.width, position.height), "Unzoomed Box");
   
        GUILayout.BeginArea(new Rect(0.0f, position.height - 145.0f, position.width, 145.0f));
        
		//CutsceneStep step1 = null;
		//StepOp stepOp = StepOp.NoOp;
		CutsceneStep step = currentStep;
		if( step != null )
		{
			
			Color bgrndBak = GUI.backgroundColor;
			Color colorBak = GUI.color;
			GUI.backgroundColor = LcCutsceneEditor.selectedId == step.stepId ? Color.cyan : bgrndBak;
			
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

//			if (GUILayout.Button("Up", GUILayout.Width(90))) {
//				SelectStep(step.stepId);
//				step1 = step;
//				stepOp = StepOp.Up;
//			}				
//			if (GUILayout.Button("Down", GUILayout.Width(90))) {
//				SelectStep(step.stepId);
//				step1 = step;
//				stepOp = StepOp.Down;
//			}				
//			if (GUILayout.Button("Delete", GUILayout.Width(90))) {
//				if( EditorUtility.DisplayDialog("This Step Delete?"
//					,"Are you sure you want to delete? ", "Yes", "No") )
//				{
//					step1 = step;
//					stepOp = StepOp.Del;
//				}
//					
//			}				
//			if (GUILayout.Button("Add Step", GUILayout.Width(90))) {
//				step1 = step;
//				stepOp = StepOp.AddAfter;
//			}				
//			if (GUILayout.Button("Run To", GUILayout.Width(90))) {
//				SelectStep(step.stepId);
//				RunTo (step.stepId);
//			}				
			
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
					ReadActionData(step.actor);
					if (dialogs != null)
						step.intVal = EditorGUILayout.IntPopup("Dialog:", step.intVal, dialogs, dialogIds);
					
					step.floatVal = EditorGUILayout.FloatField("Duration:", step.floatVal);

					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();				
					step.boolVal = EditorGUILayout.Toggle("NextLoopAction:", step.boolVal);	
					
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
						step.intVal2 = EditorGUILayout.IntPopup("Action:", step.intVal2, actorChar.actionNames, 
							actorChar.actionIds);
					else
						step.intVal2 = EditorGUILayout.IntField("Action:", step.intVal2);
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
	    GUILayout.EndArea();
	
	}
	
	void ReadActionData(ActorBase actor) {
		LcCutsceneEditor.ReadActionData(actor); 
	}
	
	void ReadLoopActionData(ActorBase actor) {
		LcCutsceneEditor.ReadLoopActionData(actor); 
	}
}
