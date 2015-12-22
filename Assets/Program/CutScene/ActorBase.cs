/*
Actor Base
Written by Jong Lee
11/14/2012
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ActorBase : MonoBehaviour {
	
	public Transform headForm;
	public bool useGravity = true;
	public Color symbolColor = Color.white;
	public Transform preWaitTouch;
	
	public ActorBase objIncludeActor = null;
	
	
	protected bool needBaseRotation = false;
	protected Vector3 baseRotation;
	protected Transform baseLookAt = null;
	
	protected Vector3 posToGo;
	protected bool needLookAt = false;
	protected Transform formToLookAt;
	protected Vector3 posToLookAt;
	protected string tagToLookAt;
	protected Transform formToFollow;
	protected bool waiting;
	protected AudioClip audioClip;
	protected Transform formToTalkTo;
	protected Vector3 posBak;
	protected bool waitingTouch = false;
	float waitTouchDelay;
	Transform waitTouchEffect;
	protected float moveSpeed = 0;
	protected CharacterController charConn;
	protected Animator animator;
	protected AudioSource audioSrc;
	protected float lipDuration;
	protected Transform moveDummy;
	protected bool bIgnoreBlink = false;
	protected Transform myTransform;
	
	
	protected bool bMoveWithAction = false;

	protected CutsceneStep.Action lastAudioVerb = CutsceneStep.Action.NoOp;
	
	public virtual bool isTalking {
		get { return isSounding && (lastAudioVerb == CutsceneStep.Action.TalkTo 
				|| lastAudioVerb == CutsceneStep.Action.SpeechTo); }
	}
	public virtual bool isSpeeching {
		get { return isSounding && lastAudioVerb == CutsceneStep.Action.SpeechTo; }
	}
	public virtual bool isSounding {
		get { return lipDuration > 0 || (audioSrc != null && audioSrc.isPlaying); }
	}
	public virtual bool isIdle {
		get { return !waitingTouch && formToFollow == null; }
	}
	public virtual bool isMoving {
		get { return moveSpeed > 0; }
	}
	public virtual bool isFollowing {
		get { return formToFollow != null; }
	}
	public virtual bool isStill {
		get { return !isMoving; }
	}
	
	public virtual void Stop() {
		StopMoveTo();
		StopScaleTo();
		
		if (audio != null)
			audio.Stop();
		if (audioSrc != null)
			audioSrc.Stop ();
		
		moveSpeed = 0;
		waitingTouch = false;
		needLookAt = false;
		formToFollow = null;
	}
	
	public virtual void StopMoving() {
		StopMoveTo ();
		formToFollow = null;
	}	
	
	
	public virtual void MoveTo(Vector3 pos, float speed, CutsceneStep step) {
		StopMoveTo ();
//		Stop();
		
		if( step != null )
			bMoveWithAction = step.boolVal ;
		
		posBak = transform.position;
		posToGo = pos;
		
		moveSpeed = speed;
//		jumping = false;
//		moving = true;
//		Debug.Log("========= position : "+posToGo);
		iTween.EaseType easeType = iTween.EaseType.linear;
//			Hashtable hash = iTween.Hash("from", transform.position, "to", posToGo, "speed", speed, "oncomplete"
//			, "OnITweenComplete", "oncompleteparams", CutsceneStep.Action.MoveTo, "onupdate", "OnITweenUpdate");
		Hashtable hash = iTween.Hash("position", posToGo, "speed", speed);
		
		if (step != null) {
			easeType = step.easeType;
			
			if (step.obj != null) {
				LcPath path1 = step.obj as LcPath;
				if (path1.loop) {
					Transform[] forms = path1.forms;
					
					Vector3[] path = new Vector3[forms.Length + 1];
					int i;
					for (i = 0; i < forms.Length; i++) {
						path[i] = forms[i].position;
					}
					path[i] = forms[0].position;
					
					hash.Add("path", path);
					
					hash.Add("movetopath", false);
					hash.Add ("looptype", iTween.LoopType.loop);
				} else 
					hash.Add("path", path1.forms);
			}
		}
		
		hash.Add("easetype", easeType);
		
		if (moveDummy == null)
			moveDummy = (new GameObject("_Dummy")).transform;
		moveDummy.position = transform.position;
		iTween.MoveTo (moveDummy.gameObject, hash);
		
	}
	
		
	
	public virtual void ScaleTo(Vector3 scale, float speed, CutsceneStep step) {
		StopScaleTo();
		
		iTween.EaseType easeType = iTween.EaseType.linear;
		if (step != null) {
			easeType = step.easeType;
		}
		iTween.ScaleTo (gameObject, iTween.Hash("scale", scale, "speed", speed, "easetype", easeType));
	}

	void StopMoveTo() {
		if (moveDummy != null) {
			iTween.Stop (moveDummy.gameObject);
			DestroyObject(moveDummy.gameObject);
			moveDummy = null;
			moveSpeed = 0;
			
			if( bMoveWithAction )
				bMoveWithAction = false;
		}
	}
	
	void StopScaleTo() {
		iTween.Stop (gameObject, "scale");
	}
	
	protected void ResumeMoving() {
		if (moveDummy != null) {
			MoveTo (posToGo, moveSpeed, null);
		}
	}
	
	public virtual void JumpTo(Vector3 pos, float speed) {}
	public virtual void Follow(Transform form, Vector3 offset, bool vertical, float speed) {}
	public virtual void WaitFor(Transform form, Vector3 offset) {}
	public virtual void Action(int action,bool bBlink ,AudioClip clip) {}

	public virtual void NextLoopAction(bool bNextLoopAction) {
		if( animator != null )
			animator.SetBool ("NextLoopAction", bNextLoopAction);
	}
	
	public virtual void Rotate(Vector3 angle) {
		needBaseRotation = true;
		baseRotation = angle;
	}
	
	public virtual void LookAt(Transform form, Vector3 pos, int type) {
		needLookAt = true;
		
		switch (type) {
		case 0:
			tagToLookAt = null;
			formToLookAt = GetHeadForm(form);
			if (form == null)
				needLookAt = false;
			break;
			
		case 2:
			formToLookAt = null;
			tagToLookAt = "MainCamera";
			break;
			
		default:
			formToLookAt = null;
			tagToLookAt = null;
			posToLookAt = pos;
			break;
		}
	}
	
	public virtual void LookAt(Transform form, Vector3 pos, bool position)
	{
		needLookAt = true;
		tagToLookAt = null;
		
		if (position) {
			formToLookAt = null;
			posToLookAt = pos;
		} else {
			formToLookAt = GetHeadForm(form);
			if (form == null)
				needLookAt = false;
		}		
	}	
	
	public virtual void WaitTouch(float delay, bool enable) {
		if (enable) {
			waitingTouch = true;
			waitTouchDelay = delay;
		} else {
			CancelWaitTouch();
		}
	}
	
	public bool CancelWaitTouch() {
		if (waitingTouch) {
			waitingTouch = false;
			if (waitTouchEffect != null) {
				GameObject.Destroy(waitTouchEffect.gameObject);
				waitTouchEffect = null;
			}
			
			return true;
		}
		
		return false;
	}
	
	public virtual bool IsDone(CutsceneStep.Action action) {
		switch (action) {
		case CutsceneStep.Action.MoveTo:
		case CutsceneStep.Action.JumpTo:
		case CutsceneStep.Action.Rotate:
		case CutsceneStep.Action.GoBack:
			return !isMoving;
			
		case CutsceneStep.Action.TalkTo:
		case CutsceneStep.Action.SpeechTo:
			return !isTalking;
		case CutsceneStep.Action.Action:
			
			//ONEWAY Check
#if TestTalk
			if( audioSrc.clip != null )
			{
				return !isTalking;
			}
#endif
			return isIdle;
		case CutsceneStep.Action.WaitTouch:
			return isIdle;

		case CutsceneStep.Action.WaitFor:
		case CutsceneStep.Action.Follow:
			return !isFollowing;
			
		case CutsceneStep.Action.Sound:
			return !isSounding;
			
		case CutsceneStep.Action.ScaleTo:
			return iTween.Count (gameObject, "scale") == 0;
		}
		
		return true;
	}
	
	void Awake() {
		if( objIncludeActor != null )
			this.enabled = false;
		else
		{
			charConn = GetComponent<CharacterController>();
			animator = GetComponentInChildren<Animator>();
			audioSrc = GetComponentInChildren<AudioSource>();
			
		}
	}
	
	protected virtual void Start() {
		myTransform = transform;
	}
	
	protected virtual void Update() {
		if(Time.timeScale == 0f) 
			return; 
		
		if (lipDuration > 0) {
			lipDuration -= Time.deltaTime;
		}
		
		if (waitingTouch) {
			if (waitTouchEffect == null && preWaitTouch != null && waitTouchDelay >= 0) {
				waitTouchDelay -= Time.deltaTime;
				if (waitTouchDelay <= 0) {
					Vector3 pos = transform.position;
					if (charConn != null)
						pos = transform.TransformPoint(charConn.center);
					else if (collider != null) {
						BoxCollider box = GetComponent<BoxCollider>();
						if (box != null)
							pos = transform.TransformPoint(box.center);
						else {
							SphereCollider sphere = GetComponent<SphereCollider>();
							if (sphere != null)
								pos = transform.TransformPoint (sphere.center);	
						}
					}
					waitTouchEffect = GameObject.Instantiate(preWaitTouch, pos, Quaternion.identity) as Transform;
					waitTouchEffect.parent = transform;
				}
			}
		}
		
		ApplyTransform();
		
		if (moveDummy != null) {
			if (iTween.Count(moveDummy.gameObject) == 0) {
				StopMoveTo ();
			}
		}
		
	}
	
	// ONEWAY48
	// ActorChar is Error at moveTo(char rotate)  when TimeScale is higher than 1 
	// move to Update Function.
//	void FixedUpdate() {
//	
//		ApplyTransform();
//		
//		if (moveDummy != null) {
//			if (iTween.Count(moveDummy.gameObject) == 0) {
//				StopMoveTo ();
//			}
//		}
//
//	}
	
	void OnEnable() {
		if (animator != null) {
			for (int i = 0; i < animator.layerCount; i++) {
				animator.SetLayerWeight(i, 1);
			}
		}
	}
	
	public virtual Vector3 GetFollowVec(Transform form, Vector3 offset, bool vertical) {
		return Vector3.zero;
	}
	
	public virtual void Setup(Vector3 pos, Quaternion rota, Vector3 scale) {
		transform.position = pos;
		transform.rotation = rota;
		transform.localScale = scale;
		
		Rotate (rota.eulerAngles);
		ResumeMoving();
	}
	
	public void DoStep(CutsceneStep step) {
		switch (step.action) {
		case CutsceneStep.Action.MoveTo:
			MoveTo(step.pos, step.floatVal == 0 ? 1f : step.floatVal, step);
			break;

		case CutsceneStep.Action.GoBack:
			MoveTo(posBak, step.floatVal == 0 ? 1f : step.floatVal, step);
			break;
			
		case CutsceneStep.Action.JumpTo:
			JumpTo(step.pos, step.floatVal == 0 ? 1f : step.floatVal);
			break;
			
		case CutsceneStep.Action.Follow:
			Follow(step.target, step.pos, step.boolVal, step.floatVal);
			break;
			
		case CutsceneStep.Action.Rotate:
			Rotate(step.pos);
			break;
			
		case CutsceneStep.Action.WaitFor:
			WaitFor(step.target.transform, step.pos);
			break;
			
		case CutsceneStep.Action.WaitTouch:
			WaitTouch(step.floatVal, true);
			break;
			
		case CutsceneStep.Action.LookAt:
			LookAt(step.target, step.pos, step.intVal);
			break;
			
		case CutsceneStep.Action.Stop:
			Stop();
			break;
			
		case CutsceneStep.Action.Action:
			if (step.intVal2 > 0)
				lastAudioVerb = step.action;	
			break;
			
		case CutsceneStep.Action.Sound:
			Sound (step.obj as AudioClip, step.action, step.boolVal, step.floatVal);
			break;
		
		case CutsceneStep.Action.ChangeInto:
			if( step.intVal2 != 1 )// Not Restore
				ChangeInto(step.obj as Transform, step.intVal);
			break;
			
		case CutsceneStep.Action.UniqueAction:
			UniqueAction(step.strVal, step.intVal);
			break;
			
		case CutsceneStep.Action.LoopAction:
			LoopAction(step.intVal, step.boolVal);
			break;
			
		case CutsceneStep.Action.Mood:
			Mood(step.intVal);
			break;
			
		case CutsceneStep.Action.ScaleTo:
			ScaleTo (step.pos, step.floatVal == 0 ? 1f : step.floatVal, step);
			break;
		}
		
	}
	
	public Transform GetFormToTalkTo() {
		return formToTalkTo;
	}
	
	public virtual bool TouchDown(Vector3 point) {
		return CancelWaitTouch();
	}
	
	public Transform GetHeadForm() {
		return GetHeadForm(transform);
	}
	
	public static Transform GetHeadForm(Transform form) {
		if (form == null)
			return form;
		
		Transform target = null;
		
		ActorBase actor = form.GetComponent<ActorBase>();
		if (actor != null)
			target = actor.headForm;
		
		if (target == null)
			target = form;
		
		return target;
	}
	
	public void Sound(AudioClip clip, CutsceneStep.Action action, bool loop, float volume) {
		if( audioSrc != null )
			audioSrc.Stop ();
		else
			return;
		
		if (clip != null) {
			lastAudioVerb = action;
			
			audioSrc.clip = clip;
			audioSrc.loop = loop;
			if (volume == 0)
				volume = 1;
			audioSrc.volume = volume;
			audioSrc.Play();
		}
	}
	
	public virtual void TalkTo(AudioClip clip, Transform form, CutsceneStep.Action action) {
		formToTalkTo = GetHeadForm(form);
		Sound(clip, action, false, 1);
	}

	public virtual void LipTo(float duration, Transform form, CutsceneStep.Action action) {
		formToTalkTo = GetHeadForm(form);
		if( audioSrc != null )
			audioSrc.Stop ();
		else
			return;
		
		if (duration > 0) {
			lastAudioVerb = action;
			lipDuration = duration;
		}
	}
	
	public virtual void ChangeInto(Transform form, int prop) {
		if (prop > 0) {
			string propName = string.Format("Prop{0}", prop);
			Transform[] bones0 = transform.GetComponentsInChildren<Transform>();
			foreach (Transform bone0 in bones0) {
				if (bone0.name.EndsWith(propName)) {
					Transform[] children = bone0.GetComponentsInChildren<Transform>();
					foreach (Transform child in children) {
						if (child.parent == bone0)
							GameObject.Destroy(child.gameObject);
					}
					if (form != null) {
						Transform newForm = Instantiate(form, bone0.position, bone0.rotation) as Transform;
						newForm.parent = bone0;
						newForm.localPosition = Vector3.zero;
						newForm.localRotation = Quaternion.identity;
						newForm.localScale = Vector3.one;
						newForm.gameObject.renderer.receiveShadows = false;
						newForm.gameObject.layer = 8;//"Actor"
					}
					break;
				}
			}
		} else {
			Debug.Log("NPC _ Test _ Zone");
			
			Transform formNew = GameObject.Instantiate(form, transform.position, transform.rotation) as Transform;
			
			List<string> deleted = new List<string>();
			
			Renderer[] rens = formNew.GetComponentsInChildren<Renderer>();
			foreach (Renderer ren in rens) {
				string sub = Regex.Match(ren.name, "^[^_]+_([^_]+)_").Groups[1].Value;
				
				
				if (sub.Length > 0) {
					if (!deleted.Exists(item => item.Equals(sub))) {
						deleted.Add (sub);
						
						Renderer[] rens0 = transform.GetComponentsInChildren<Renderer>();
						foreach (Renderer ren0 in rens0) {
							
							Debug.Log("ren0 names : " + ren0.gameObject.name);
							
							//							string sub0 = Regex.Match(ren0.name, "^[^_]+_([^_]+)_").Groups[1].Value;		// 2014.07.03 Boots PutOn 문제로 주석처리, 아래코드로 대치 (Blooming)
							string sub0 = ren0.name.Split('_')[1].ToString();
							
							//Debug.Log("ren0.name.Split('_').Length : " + ren0.name.Split('_').Length);
							if( ren0.name.Split('_').Length != 1 )
							{
								//Boots State
								if(!(sub0 == "" && (ren0.name.Split('_')[1].Equals("eye") || (ren0.name.Split('_')[1].Equals("foot"))))){
									//NPC's
									Debug.Log("inthe NPC state " + sub0);
									if(sub0 == ""){
										//when [puton target transform] 2nd index is null
										sub0 = "dress";
										string sub1 = "";
										if(ren.name.Split('_').Length > 2){
											sub1 = ren.name.Split('_')[2];
										}
										
										if(sub1.Equals("dress")){
										//when [parts transform] 2nd index name is equal "dress"
										sub = sub1;
										}
									}
								}
							}

							if (sub0.Equals(sub)) {
								GameObject.Destroy(ren0.gameObject);
							} else {
								switch (sub) {
								case "dress":
									if (sub0.Equals("upper") || sub0.Equals("lower")) {
										GameObject.Destroy(ren0.gameObject);
									}
									break;
									
								case "upper":
									if (sub0.Equals("dress")) {
										GameObject.Destroy(ren0.gameObject);
									}
									break;
								}
							}
						}
					}

					SkinnedMeshRenderer sren = ren as SkinnedMeshRenderer;
					if (sren != null) {
						Transform[] bones0 = transform.GetComponentsInChildren<Transform>();
						List<Transform> boneList = new List<Transform>();
						foreach (Transform bone in sren.bones) {
							foreach (Transform bone0 in bones0) {
								if (bone0.name == bone.name) {
									boneList.Add (bone0);
									break;
								}
							}
						}
						sren.bones = boneList.ToArray();
		
						foreach (Transform bone0 in bones0) {
							if (bone0.name == sren.rootBone.name) {
								sren.rootBone = bone0;
								break;
							}
						}

						ren.transform.parent = transform;
						ren.transform.gameObject.renderer.receiveShadows = false;
						ren.transform.gameObject.layer = 8;//"Actor"
						ren.enabled = true;
					} else {
						Vector3 pos = ren.transform.localPosition;
						Quaternion rotation = ren.transform.localRotation;
						string name = ren.transform.parent.name;
						Transform[] bones0 = transform.GetComponentsInChildren<Transform>();
						foreach (Transform bone0 in bones0) {
							if (bone0.name == name) {
								ren.transform.parent = bone0;
								ren.transform.localPosition = pos;
								ren.transform.localRotation = rotation;
								ren.transform.gameObject.renderer.receiveShadows = false;
								ren.transform.gameObject.layer = 8;//"Actor"
								
								break;
							}
						}
					}
				}
			}
			
			GameObject.Destroy(formNew.gameObject);
		}
	}
	
	public virtual void ClearAttach( Transform form ) {
		Renderer[] rens0 = transform.GetComponentsInChildren<Renderer>();
		foreach (Renderer ren0 in rens0) {
			if( ren0.name == form.name )
			{
				GameObject.Destroy(ren0.gameObject);
			}
//			string sub0 = Regex.Match(ren0.name, "^[^_]+_([^_]+)_").Groups[1].Value;
//			switch (sub0) {
//			case "cap":
//				GameObject.Destroy(ren0.gameObject);
//				break;
//			case "mask":
//				GameObject.Destroy(ren0.gameObject);
//				break;
//			}
		}
	}


	
	public virtual void UniqueAction(string name, int phase) {
		if (animator != null) {
			animator.SetInteger(name, phase);
		}
	}
	
	protected virtual void ApplyTransform() {
		if (moveDummy != null)
			transform.position = moveDummy.position;
		if (needBaseRotation) {
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(baseRotation)
				, Time.deltaTime * 5f);
		}
	}
	
	public virtual void LoopAction(int action, bool bBlink) {
		bIgnoreBlink = bBlink;
		if (animator != null) {
			if (action > 0)
				animator.SetFloat ("LoopActionType", action);
			animator.SetBool("LoopAction", action > 0);
		}
	}
	
	public virtual void Mood(int mood) {
		bIgnoreBlink = true;
		if (animator != null) {
			animator.SetFloat("Mood", mood);
		}
	}
	
	protected bool GetLookAt(ref Vector3 newPos) {
		bool looking = false;
		
		if (needLookAt) {
			if (formToLookAt != null) {
				looking = true;
				newPos = formToLookAt.position;
			} else if (tagToLookAt != null) {
				GameObject obj = GameObject.FindWithTag(tagToLookAt);
				if (obj != null) {
					looking = true;
					newPos = obj.transform.position;
				}
			}
			
			if (!looking) {
				looking = true;
				newPos = posToLookAt;
			}
		} else if (isTalking && formToTalkTo != null) {
			looking = true;
			newPos = formToTalkTo.position;
		}  
		else {
			GameObject[] objs = GameObject.FindGameObjectsWithTag("Player");
			foreach (GameObject obj in objs) {
				ActorBase actor = obj.GetComponent<ActorBase>();
				if (actor != null && actor != this) {
					if (actor.isSpeeching) {
						looking = true;
						if (actor.GetFormToTalkTo() != null && !actor.GetFormToTalkTo().IsChildOf(transform)) {
							newPos = actor.GetFormToTalkTo().position;
						} else
							newPos = actor.GetHeadForm().position;
						
						break;
					} else if (actor.isTalking) {
						looking = true;
						newPos = actor.GetHeadForm().position;

						break;
					}
				}
			}
		}
		
		return looking;
	}
}
