using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ActorData {
	public ActorBase actor;
	public Vector3 pos;
	public Quaternion rota;
	public Vector3 scale;
}

[System.Serializable]
public class CutsceneStep
{
    public enum Action {NoOp
    	, MoveTo, JumpTo, Action, TalkTo, LookAt, Rotate, Follow, WaitFor, WaitTouch, GoBack
		, Activate, Setup, Stop, SpeechTo, LoopAction, Proc, Sound, ChangeInto, Mood, PlayMovie
		, UniqueAction, WaitSound, ScaleTo, Caption, GameLog, EndCutscene = 900, LoadScene = 999};

    public enum ActionDisplay {NoOp
		, Action, Activate, Caption, ChangeInto, EndCutscene, Follow, GameLog, GoBack, JumpTo, LoadScene
		, LookAt,  LoopAction, Mood, MoveTo, PlayMovie, Proc, Rotate, ScaleTo, Setup, Sound
		, SpeechTo, Stop, TalkTo, UniqueAction, WaitFor, WaitSound, WaitTouch  };
	
	public static string[] moods = { "Indifferent", "Smile", "Smile 2", "Smile 3", "Disappointed", "Disappointed 2"
		, "Surprised", "Surprised 2", "Curiosity" };
	public int stepId;
	public int preId;
	public float delay;
	public ActorBase actor;
	public Action action = Action.NoOp;
	//public ActionDisplay actionDisplay = ActionDisplay.NoOp;
	public Transform target;
	public Vector3 pos;
	public float floatVal;
	public bool boolVal;
	public bool boolVal2;
	public bool boolVal3;
	public int intVal;
	public int intVal2;
	public string strVal;
	public string strVal2;
	public iTween.EaseType easeType = iTween.EaseType.linear;
	public ActorData[] actorData;
	public Object obj;
#if UNITY_EDITOR
	public CutsceneStep prevStep;
	public string note;
	public bool bCurrentStep= false;
#endif
	
	public bool isDone {
		get {
			switch (action) {
			case Action.LoadScene:
				return false;
				
			case Action.Activate: {
				bool activate = true;
				switch (intVal) {
				case 0:
					activate = !boolVal2;
					break;
					
				case 1:
					activate = false;
					break;
				}
				
				if (activate && boolVal) {
					return !target.gameObject.activeInHierarchy;
				}
			}
				return true;
				
			case Action.Proc:
				return !boolVal;
				
			default:
				if (actor != null)
					return actor.IsDone (action);
				else
					return true;
			}
		}
	}
	
	public static ActionDisplay ConverterActionIDToActionDisplayID( Action actionid )
	{
		ActionDisplay tempAciton  = CutsceneStep.ActionDisplay.NoOp;
		if( actionid == CutsceneStep.Action.EndCutscene )
			tempAciton = CutsceneStep.ActionDisplay.EndCutscene;
		else if(actionid == CutsceneStep.Action.LoadScene)
			tempAciton = CutsceneStep.ActionDisplay.LoadScene;
		else if(actionid == CutsceneStep.Action.NoOp)
			tempAciton = CutsceneStep.ActionDisplay.NoOp;
		else if(actionid == CutsceneStep.Action.MoveTo)
			tempAciton = CutsceneStep.ActionDisplay.MoveTo;
		else if(actionid == CutsceneStep.Action.JumpTo)
			tempAciton = CutsceneStep.ActionDisplay.JumpTo;
		else if(actionid == CutsceneStep.Action.Action)
			tempAciton = CutsceneStep.ActionDisplay.Action;
		else if(actionid == CutsceneStep.Action.TalkTo)
			tempAciton = CutsceneStep.ActionDisplay.TalkTo;
		else if(actionid == CutsceneStep.Action.LookAt)
			tempAciton = CutsceneStep.ActionDisplay.LookAt;
		else if(actionid == CutsceneStep.Action.Rotate)
			tempAciton = CutsceneStep.ActionDisplay.Rotate;
		else if(actionid == CutsceneStep.Action.Follow)
			tempAciton = CutsceneStep.ActionDisplay.Follow;
		else if(actionid == CutsceneStep.Action.WaitFor)
			tempAciton = CutsceneStep.ActionDisplay.WaitFor;
		else if(actionid == CutsceneStep.Action.WaitTouch)
			tempAciton = CutsceneStep.ActionDisplay.WaitTouch;
		else if(actionid == CutsceneStep.Action.GoBack)
			tempAciton = CutsceneStep.ActionDisplay.GoBack;
		else if(actionid == CutsceneStep.Action.Activate)
			tempAciton = CutsceneStep.ActionDisplay.Activate;
		else if(actionid == CutsceneStep.Action.Setup)
			tempAciton = CutsceneStep.ActionDisplay.Setup;
		else if(actionid == CutsceneStep.Action.Stop)
			tempAciton = CutsceneStep.ActionDisplay.Stop;
		else if(actionid == CutsceneStep.Action.SpeechTo)
			tempAciton = CutsceneStep.ActionDisplay.SpeechTo;
		else if(actionid == CutsceneStep.Action.LoopAction)
			tempAciton = CutsceneStep.ActionDisplay.LoopAction;
		else if(actionid == CutsceneStep.Action.Proc)
			tempAciton = CutsceneStep.ActionDisplay.Proc;
		else if(actionid == CutsceneStep.Action.Sound)
			tempAciton = CutsceneStep.ActionDisplay.Sound;
		else if(actionid == CutsceneStep.Action.ChangeInto)
			tempAciton = CutsceneStep.ActionDisplay.ChangeInto;
		else if(actionid == CutsceneStep.Action.Mood)
			tempAciton = CutsceneStep.ActionDisplay.Mood;
		else if(actionid == CutsceneStep.Action.PlayMovie)
			tempAciton = CutsceneStep.ActionDisplay.PlayMovie;
		else if(actionid == CutsceneStep.Action.UniqueAction)
			tempAciton = CutsceneStep.ActionDisplay.UniqueAction;
		else if(actionid == CutsceneStep.Action.WaitSound)
			tempAciton = CutsceneStep.ActionDisplay.WaitSound;
		else if(actionid == CutsceneStep.Action.ScaleTo)
			tempAciton = CutsceneStep.ActionDisplay.ScaleTo;
		else if(actionid == CutsceneStep.Action.Caption)
			tempAciton = CutsceneStep.ActionDisplay.Caption;
		else if(actionid == CutsceneStep.Action.GameLog)
			tempAciton = CutsceneStep.ActionDisplay.GameLog;
		
		return tempAciton;
	}
	
	public static Action ConverterActionDisplayIDToActionID( ActionDisplay actionid )
	{
		Action tempAciton =  CutsceneStep.Action.NoOp;
		if( actionid == CutsceneStep.ActionDisplay.EndCutscene )
			tempAciton= CutsceneStep.Action.EndCutscene;
		else if(actionid == CutsceneStep.ActionDisplay.LoadScene)
			tempAciton= CutsceneStep.Action.LoadScene;
		else if(actionid == CutsceneStep.ActionDisplay.NoOp)
			tempAciton= CutsceneStep.Action.NoOp;
		else if(actionid == CutsceneStep.ActionDisplay.MoveTo)
			tempAciton= CutsceneStep.Action.MoveTo;
		else if(actionid == CutsceneStep.ActionDisplay.JumpTo)
			tempAciton= CutsceneStep.Action.JumpTo;
		else if(actionid == CutsceneStep.ActionDisplay.Action)
			tempAciton= CutsceneStep.Action.Action;
		else if(actionid == CutsceneStep.ActionDisplay.TalkTo)
			tempAciton= CutsceneStep.Action.TalkTo;
		else if(actionid == CutsceneStep.ActionDisplay.LookAt)
			tempAciton= CutsceneStep.Action.LookAt;
		else if(actionid == CutsceneStep.ActionDisplay.Rotate)
			tempAciton= CutsceneStep.Action.Rotate;
		else if(actionid == CutsceneStep.ActionDisplay.Follow)
			tempAciton= CutsceneStep.Action.Follow;
		else if(actionid == CutsceneStep.ActionDisplay.WaitFor)
			tempAciton= CutsceneStep.Action.WaitFor;
		else if(actionid == CutsceneStep.ActionDisplay.WaitTouch)
			tempAciton= CutsceneStep.Action.WaitTouch;
		else if(actionid == CutsceneStep.ActionDisplay.GoBack)
			tempAciton= CutsceneStep.Action.GoBack;
		else if(actionid == CutsceneStep.ActionDisplay.Activate)
			tempAciton= CutsceneStep.Action.Activate;
		else if(actionid == CutsceneStep.ActionDisplay.Setup)
			tempAciton= CutsceneStep.Action.Setup;
		else if(actionid == CutsceneStep.ActionDisplay.Stop)
			tempAciton= CutsceneStep.Action.Stop;
		else if(actionid == CutsceneStep.ActionDisplay.SpeechTo)
			tempAciton= CutsceneStep.Action.SpeechTo;
		else if(actionid == CutsceneStep.ActionDisplay.LoopAction)
			tempAciton= CutsceneStep.Action.LoopAction;
		else if(actionid == CutsceneStep.ActionDisplay.Proc)
			tempAciton= CutsceneStep.Action.Proc;
		else if(actionid == CutsceneStep.ActionDisplay.Sound)
			tempAciton= CutsceneStep.Action.Sound;
		else if(actionid == CutsceneStep.ActionDisplay.ChangeInto)
			tempAciton= CutsceneStep.Action.ChangeInto;
		else if(actionid == CutsceneStep.ActionDisplay.Mood)
			tempAciton= CutsceneStep.Action.Mood;
		else if(actionid == CutsceneStep.ActionDisplay.PlayMovie)
			tempAciton= CutsceneStep.Action.PlayMovie;
		else if(actionid == CutsceneStep.ActionDisplay.UniqueAction)
			tempAciton= CutsceneStep.Action.UniqueAction;
		else if(actionid == CutsceneStep.ActionDisplay.WaitSound)
			tempAciton= CutsceneStep.Action.WaitSound;
		else if(actionid == CutsceneStep.ActionDisplay.ScaleTo)
			tempAciton= CutsceneStep.Action.ScaleTo;
		else if(actionid == CutsceneStep.ActionDisplay.Caption)
			tempAciton= CutsceneStep.Action.Caption;
		else if(actionid == CutsceneStep.ActionDisplay.GameLog)
			tempAciton= CutsceneStep.Action.GameLog;
		
		return tempAciton;
	}
}
