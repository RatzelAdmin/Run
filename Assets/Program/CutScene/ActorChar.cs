/*
Actor - Character variant
Written by Jong Lee
11/7/2012
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

public class ActorChar : ActorBase {
	public bool persona;
	public int[] headTouchActions;
	public int[] bodyTouchActions;
	public AudioClip touchSound;

	public override bool isIdle {
		get { return base.isIdle && !animator.IsInTransition(0) && animator.GetCurrentAnimatorStateInfo(0).IsTag("idle"); }
	}
	public override bool isMoving {
		get { return moveSpeed > 0f || jumping; }
	}
	public override bool isStill {
		get { return !isMoving && !jumping; }
	}
	public bool isAwake {
		get { return animator.GetFloat("Mood") != 9; }
	}
	
	bool jumping = false;
	bool acting = false;
	float blinkDelay = 1;
	float actionType = 0.0f;
	float randomIdleDelay;
	CollisionFlags collisionFlags;
	Vector3 jumpSpeed;
	public int numRandomIdle;
	float lookAtWeight = 0f;
	Vector3 lookAtPos;
	Vector3 lookAtPosBak;
	int lookAtLinger = 9999;
	public TextAsset actions;
	public TextAsset loopActions;
	Quaternion initHeadform;
	float rangeValue45;
	float rangeValue180;
	float rangeValue315;
	private float headAngle = 0;
	private bool bHasHeadAngle = true;
#if UNITY_EDITOR
	[HideInInspector]
	public string[] actionNames;
	[HideInInspector]
	public int[] actionIds;
	
	[HideInInspector]
	public string[] loopActionNames;
	[HideInInspector]
	public int[] loopActionIds;
	
	
#endif
	
	// Use this for initialization
	protected override void Start () {
		base.Start ();
		if( headForm != null )
		{
			if( headForm.name != "Point_head_client" )
			{
				headForm = null;
//				Transform[] transforms = transform.GetComponentsInChildren<Transform>();
//				
//		        foreach (Transform tf in transforms) {
//		            if( tf.name == "Point_head_client" )
//					{
//						headForm = tf;
//						break;
//					}
//		        }
			}
		}
//		else
//		{
//				Transform[] transforms = transform.GetComponentsInChildren<Transform>();
//		        foreach (Transform tf in transforms) {
//		            if( tf.name == "Point_head_client" )
//					{
//						headForm = tf;
//						break;
//					}
//		        }
//		}
		
		randomIdleDelay = Random.Range(2f, 4f);
//		if(  persona && !animator.isHuman && headForm != null )
//		{
//			//initHeadform = Quaternion.Euler(0,0,0);
//			initHeadform = headForm.rotation;
//			//Debug.Log ("Head Control initHeadform "+ initHeadform);
////			Debug.Log ("initHeadform.eulerAngles.y = "+ initHeadform.eulerAngles.y);
//			
//			rangeValue45  = ((initHeadform.eulerAngles.y + 45.0f)>360.0f)?(initHeadform.eulerAngles.y-315.0f):(initHeadform.eulerAngles.y+45.0f);
//			rangeValue180  = ((initHeadform.eulerAngles.y + 180.0f)>360.0f)?(initHeadform.eulerAngles.y-180.0f):(initHeadform.eulerAngles.y+180.0f);
//			rangeValue315  = ((initHeadform.eulerAngles.y + 315.0f)>360.0f)?(initHeadform.eulerAngles.y-45.0f):(initHeadform.eulerAngles.y+315.0f);
//			
////			Debug.Log ("rangeValue45 = "+ rangeValue45);
////			Debug.Log ("rangeValue180 = "+ rangeValue180);
////			Debug.Log ("rangeValue315 = "+ rangeValue315);
//		}
		if (animator != null) 
		{
			animator.SetFloat ("HeadAngle", 300.0f);
			if (animator.GetFloat ("HeadAngle") != 300.0f) {
					Debug.Log ("Set Angle " + gameObject);
					bHasHeadAngle = false;
			}
			else
				animator.SetFloat ("HeadAngle", 0.0f);
		}
	}
	
	// Update is called once per frame
	protected override void Update () {
		base.Update();
		
		if (acting) {
			if (animator.GetCurrentAnimatorStateInfo(0).IsTag("action")) {
				if (animator.GetBool("Action")) {
					animator.SetBool("Action", false);
				}
			} else {
				if (!animator.GetBool("Action")) {
					acting = false;
				}
			}
		}

		ApplyRandomIdle();
		
		if( animator != null )
		{
			if (persona || animator.isHuman) {
				
				// if bMoveWithAction is true, Don't Play MoveAnim.
				if( !bMoveWithAction ) 
				{
					animator.SetFloat("Speed", moveSpeed / 2f);
				}
				
				animator.SetBool("Talk", isTalking);
		
				if( !bIgnoreBlink )
					ApplyBlink();
			}
			
			if (persona || animator.isHuman) {
				animator.SetBool("Jump", jumping);
				
//				bool looking = false;
//				Vector3 newPos = lookAtPos;
//				
//				if (isAwake) {
//					GameObject ufo = GameObject.FindGameObjectWithTag("UFO");
//					if (ufo != null) {
//						looking = true;
//						newPos = ufo.transform.position;
//					}
//					else
//					{
//						looking = GetLookAt(ref newPos);
//						if(  persona && !animator.isHuman && headForm != null && !looking )
//						{
//							Quaternion rotation = Quaternion.Euler(
//								transform.rotation.eulerAngles.x
//								, transform.rotation.eulerAngles.y-90.0f
//								, transform.rotation.eulerAngles.z-90.0f);
//							headForm.rotation = Quaternion.Slerp(headForm.rotation, rotation, Time.deltaTime*6.0f);
//						}
//					}
//				}
//	
//				if (looking) {
//					lookAtLinger = 0;
//					lookAtPosBak = newPos;
//				} else {
//					//Debug.Log ("lookAtLinger = "+lookAtLinger);
//					if (lookAtLinger > 100) {
//	/*					if (Camera.main != null ) {
//							newPos = Camera.main.transform.position;
//						}*/
//					} else {
//						lookAtLinger++;
//						looking = true;
//						newPos = lookAtPosBak;
//						
//						if( persona && !animator.isHuman && headForm != null )
//							looking = false;
//					}
//				}
//	
//				if (lookAtWeight == 0)
//					lookAtPos = newPos;
//				else
//					lookAtPos = Vector3.Lerp(lookAtPos, newPos, Time.deltaTime * 5f);
//				
//				if(  persona && !animator.isHuman && headForm != null )
//				{
//					if( looking )
//					{
//						ApplyHeadForm();
//					}
//	
//					lookAtWeight = Mathf.Lerp(lookAtWeight, looking ? 1 : 0, Time.deltaTime * 5f);
//					animator.SetLookAtWeight(lookAtWeight, 0, 0.6f, 0.4f, 0.65f);
//	
//				}
//				else
//				{
//					if (looking)
//					{
//						//Debug.Log (" animator.SetLookAtPosition "+lookAtPos);
//						animator.SetLookAtPosition(lookAtPos);
//						
//					}
//				
//					lookAtWeight = Mathf.Lerp(lookAtWeight, looking ? 1 : 0, Time.deltaTime * 5f);
//					animator.SetLookAtWeight(lookAtWeight, 0, 0.6f, 0.4f, 0.65f);
//				}
				
			}
		}
	}
	
	void LateUpdate () {			
		
		if( animator != null )
		{
			if (persona || animator.isHuman) {
				bool looking = false;
				Vector3 newPos = lookAtPos;
				
				if (isAwake) {
					looking = GetLookAt(ref newPos);
//					if(  persona && !animator.isHuman && headForm != null && !looking )
//					{
//						Quaternion rotation = Quaternion.Euler(
//							transform.rotation.eulerAngles.x
//							, transform.rotation.eulerAngles.y-90.0f
//							, transform.rotation.eulerAngles.z-90.0f);
//						headForm.rotation = Quaternion.Slerp(headForm.rotation, rotation, Time.deltaTime*6.0f);
//					}
				}
				
				if (looking) {
					lookAtLinger = 0;
					lookAtPosBak = newPos;
				} else {
					//Debug.Log ("lookAtLinger = "+lookAtLinger);
					if (lookAtLinger > 100) {
						/*					if (Camera.main != null ) {
								newPos = Camera.main.transform.position;
							}*/
					} else {
						lookAtLinger++;
						looking = true;
						newPos = lookAtPosBak;
						
						if( persona && !animator.isHuman && headForm != null )
							looking = false;
					}
				}
				
				if (lookAtWeight == 0)
					lookAtPos = newPos;
				else
					lookAtPos = Vector3.Lerp(lookAtPos, newPos, Time.deltaTime * 5f);
				
				if(  persona && !animator.isHuman && headForm != null )
				{
					if( looking )
					{
						//ApplyHeadForm();			
						Vector3 _direction = Vector3.zero;
						_direction = lookAtPos - headForm.position;
						_direction = _direction.normalized;
						Quaternion rotation = Quaternion.LookRotation(_direction) ;
						//Quaternion baserotation = Quaternion.Euler(baseRotation);
						//Debug.Log ("_direction "+_direction);
						//headForm.rotation = Quaternion.Slerp(headForm.rotation, rotation, Time.deltaTime*6.0f);
						if( bHasHeadAngle )
						{
//							float baseAngle = Quaternion.Euler(baseRotation).eulerAngles.y;
//							Debug.Log ("baseAngle "+baseAngle);
//							if( baseAngle == 0 ) baseAngle = 180.0f; 
							//Debug.Log ("transform.rotation.eulerAngles.y "+transform.rotation.eulerAngles.y);
							animator.SetBool("LookAt",true);
							headAngle = Mathf.LerpAngle(headAngle, myTransform.rotation.eulerAngles.y - rotation.eulerAngles.y , Time.deltaTime * 5f);
							animator.SetFloat("HeadAngle",headAngle);
						}
						//animator.GetFloat( "HeadAngle" );
						//Debug.Log ("headAngle = "+ animator.GetFloat( "HeadAngle" ));
					}
					else
					{
						if( bHasHeadAngle )
						{
							if( headAngle < -1.0f || headAngle > 1.0f )
							{
								headAngle = Mathf.LerpAngle(headAngle, 0, Time.deltaTime * 5f);

								if( headAngle >= -0.2f && headAngle <= 0.1f )
								{
									headAngle = 0.0f;
									animator.SetBool("LookAt",false);
								}

								animator.SetFloat("HeadAngle",headAngle);
								//Debug.Log ("headAngle = "+ headAngle);
							}
						}

					}
					
					//lookAtWeight = Mathf.Lerp(lookAtWeight, looking ? 1 : 0, Time.deltaTime * 5f);
					//animator.SetLookAtWeight(lookAtWeight, 0, 0.6f, 0.4f, 0.65f);
					
				}
				else
				{
					if (looking)
					{
						//Debug.Log (" animator.SetLookAtPosition "+lookAtPos);
						animator.SetLookAtPosition(lookAtPos);
						
					}
					
					lookAtWeight = Mathf.Lerp(lookAtWeight, looking ? 1 : 0, Time.deltaTime * 5f);
					animator.SetLookAtWeight(lookAtWeight, 0, 0.6f, 0.4f, 0.65f);
				}
				
			}
		}
	}
	
	protected void ApplyHeadForm( ) {
		
		if( needLookAt )
		{
//			float y = transform.eulerAngles.y;
//			y -= baseRotation.y;
//			if (y < -180)
//				y += 360;
			initHeadform = Quaternion.Euler(
				initHeadform.eulerAngles.x, headForm.eulerAngles.y, headForm.eulerAngles.z);
			
			rangeValue45  = ((initHeadform.eulerAngles.y + 45.0f)>360.0f)?(initHeadform.eulerAngles.y-315.0f):(initHeadform.eulerAngles.y+45.0f);
			rangeValue180  = ((initHeadform.eulerAngles.y + 180.0f)>360.0f)?(initHeadform.eulerAngles.y-180.0f):(initHeadform.eulerAngles.y+180.0f);
			rangeValue315  = ((initHeadform.eulerAngles.y + 315.0f)>360.0f)?(initHeadform.eulerAngles.y-45.0f):(initHeadform.eulerAngles.y+315.0f);
			Vector3 _direction = Vector3.zero;
			//_direction = pos - transform.position;
			//_direction = pos - headForm.position;
			//_direction = newPos - headForm.position;
			_direction = lookAtPos - headForm.position;
			_direction = _direction.normalized;
			Quaternion rotation = Quaternion.LookRotation(_direction);
			
			rotation = Quaternion.Euler(
				rotation.eulerAngles.x, rotation.eulerAngles.y-90.0f, rotation.eulerAngles.z-90.0f);
	
	
			if( initHeadform.eulerAngles.y < 45.0f 
				|| (initHeadform.eulerAngles.y >= 315.0f && initHeadform.eulerAngles.y < 360.0f) )
			{
				if( rotation.eulerAngles.y >= rangeValue45
					&& rotation.eulerAngles.y < rangeValue180 )
				{
	//						Debug.Log ("initHeadform.eulerAngles.y + 45.0f = "+ (initHeadform.eulerAngles.y + 45.0f));
	//						Debug.Log ("initHeadform.eulerAngles.y + 180.0f = "+ (initHeadform.eulerAngles.y + 180.0f));
	//						Debug.Log ("45 rotation.eulerAngles.y = "+ rotation.eulerAngles.y);
					rotation = Quaternion.Euler(
						rotation.eulerAngles.x, initHeadform.eulerAngles.y + 45.0f, rotation.eulerAngles.z);
				}
				else if(rangeValue315 > rangeValue180)
				{
					if( rotation.eulerAngles.y < rangeValue315
						&& rotation.eulerAngles.y >= rangeValue180 )
					{
						rotation = Quaternion.Euler(
							rotation.eulerAngles.x, initHeadform.eulerAngles.y + 315.0f, rotation.eulerAngles.z);
					}
				}
			}
			else if(initHeadform.eulerAngles.y >= 45.0f && initHeadform.eulerAngles.y < 180.0f)
			{
				if( rotation.eulerAngles.y >= rangeValue45
					&& rotation.eulerAngles.y < rangeValue180 )
				{
					rotation = Quaternion.Euler(
						rotation.eulerAngles.x, initHeadform.eulerAngles.y + 45.0f, rotation.eulerAngles.z);
				}
				else if( (rotation.eulerAngles.y < rangeValue315 && rotation.eulerAngles.y >= 0)
					|| (rotation.eulerAngles.y >= rangeValue180 && rotation.eulerAngles.y < 360) )
				{
					rotation = Quaternion.Euler(
						rotation.eulerAngles.x, initHeadform.eulerAngles.y + 315.0f, rotation.eulerAngles.z);
				}
			}
			else if(initHeadform.eulerAngles.y >= 180.0f && initHeadform.eulerAngles.y < 315.0f)
			{
				if( (rotation.eulerAngles.y >= rangeValue45 && rotation.eulerAngles.y < 360)
					|| (rotation.eulerAngles.y < rangeValue180 &&  rotation.eulerAngles.y >= 0))
				{
					rotation = Quaternion.Euler(
						rotation.eulerAngles.x, initHeadform.eulerAngles.y + 45.0f, rotation.eulerAngles.z);
				}
				else if( rotation.eulerAngles.y < rangeValue315 && rotation.eulerAngles.y >= rangeValue180 )
				{
					rotation = Quaternion.Euler(
						rotation.eulerAngles.x, initHeadform.eulerAngles.y + 315.0f, rotation.eulerAngles.z);
				}
			}
		
			headForm.rotation = Quaternion.Slerp(headForm.rotation, rotation, Time.deltaTime*6.0f);
			headForm.rotation = Quaternion.Euler(
				initHeadform.eulerAngles.x
				,headForm.rotation.eulerAngles.y
				,headForm.rotation.eulerAngles.z);
		}
		else
		{
			initHeadform = Quaternion.Euler(
				initHeadform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z);
			
			rangeValue45  = ((initHeadform.eulerAngles.y + 45.0f)>360.0f)?(initHeadform.eulerAngles.y-315.0f):(initHeadform.eulerAngles.y+45.0f);
			rangeValue180  = ((initHeadform.eulerAngles.y + 180.0f)>360.0f)?(initHeadform.eulerAngles.y-180.0f):(initHeadform.eulerAngles.y+180.0f);
			rangeValue315  = ((initHeadform.eulerAngles.y + 315.0f)>360.0f)?(initHeadform.eulerAngles.y-45.0f):(initHeadform.eulerAngles.y+315.0f);
		
			Vector3 _direction = Vector3.zero;
			//_direction = pos - transform.position;
			//_direction = pos - headForm.position;
			//_direction = newPos - headForm.position;
			_direction = lookAtPos - headForm.position;
			_direction = _direction.normalized;
			Quaternion rotation = Quaternion.LookRotation(_direction);
			
	
	
			if( initHeadform.eulerAngles.y < 45.0f 
				|| (initHeadform.eulerAngles.y >= 315.0f && initHeadform.eulerAngles.y < 360.0f) )
			{
				if( rotation.eulerAngles.y >= rangeValue45
					&& rotation.eulerAngles.y < rangeValue180 )
				{
	//						Debug.Log ("initHeadform.eulerAngles.y + 45.0f = "+ (initHeadform.eulerAngles.y + 45.0f));
	//						Debug.Log ("initHeadform.eulerAngles.y + 180.0f = "+ (initHeadform.eulerAngles.y + 180.0f));
	//						Debug.Log ("45 rotation.eulerAngles.y = "+ rotation.eulerAngles.y);
					rotation = Quaternion.Euler(
						rotation.eulerAngles.x, initHeadform.eulerAngles.y + 45.0f-90.0f, rotation.eulerAngles.z-90.0f);
				}
				else if(rangeValue315 > rangeValue180)
				{
					if( rotation.eulerAngles.y < rangeValue315
						&& rotation.eulerAngles.y >= rangeValue180 )
					{
						rotation = Quaternion.Euler(
							rotation.eulerAngles.x, initHeadform.eulerAngles.y + 315.0f-90.0f, rotation.eulerAngles.z-90.0f);
					}
				}
				else
				{
					rotation = Quaternion.Euler(
						rotation.eulerAngles.x, rotation.eulerAngles.y-90.0f, rotation.eulerAngles.z-90.0f);
				}
			}
			else if(initHeadform.eulerAngles.y >= 45.0f && initHeadform.eulerAngles.y < 180.0f)
			{
				if( rotation.eulerAngles.y >= rangeValue45
					&& rotation.eulerAngles.y < rangeValue180 )
				{
					rotation = Quaternion.Euler(
						rotation.eulerAngles.x, initHeadform.eulerAngles.y + 45.0f-90.0f, rotation.eulerAngles.z-90.0f);
				}
				else if( (rotation.eulerAngles.y < rangeValue315 && rotation.eulerAngles.y >= 0)
					|| (rotation.eulerAngles.y >= rangeValue180 && rotation.eulerAngles.y < 360) )
				{
					rotation = Quaternion.Euler(
						rotation.eulerAngles.x, initHeadform.eulerAngles.y + 315.0f-90.0f, rotation.eulerAngles.z-90.0f);
				}
				else
				{
					rotation = Quaternion.Euler(
						rotation.eulerAngles.x, rotation.eulerAngles.y-90.0f, rotation.eulerAngles.z-90.0f);
				}
			}
			else if(initHeadform.eulerAngles.y >= 180.0f && initHeadform.eulerAngles.y < 315.0f)
			{
				if( (rotation.eulerAngles.y >= rangeValue45 && rotation.eulerAngles.y < 360)
					|| (rotation.eulerAngles.y < rangeValue180 &&  rotation.eulerAngles.y >= 0))
				{
					rotation = Quaternion.Euler(
						rotation.eulerAngles.x, initHeadform.eulerAngles.y + 45.0f-90.0f, rotation.eulerAngles.z-90.0f);
				}
				else if( rotation.eulerAngles.y < rangeValue315 && rotation.eulerAngles.y >= rangeValue180 )
				{
					rotation = Quaternion.Euler(
						rotation.eulerAngles.x, initHeadform.eulerAngles.y + 315.0f-90.0f, rotation.eulerAngles.z-90.0f);
				}
				else
				{
					rotation = Quaternion.Euler(
						rotation.eulerAngles.x, rotation.eulerAngles.y-90.0f, rotation.eulerAngles.z-90.0f);
				}
			}
			headForm.rotation = Quaternion.Slerp(headForm.rotation, rotation, Time.deltaTime*6.0f);
			headForm.rotation = Quaternion.Euler(
				initHeadform.eulerAngles.x
				,headForm.rotation.eulerAngles.y
				,headForm.rotation.eulerAngles.z);
		}
			
	}
	
	public override void Stop() {
		base.Stop ();
	}	
	
	public override void JumpTo(Vector3 pos, float speed) {
		if (jumping)
			return;
		jumping = true;
		
		jumpSpeed = (pos - transform.position);
		float time = jumpSpeed.magnitude / speed;
		jumpSpeed = jumpSpeed.normalized * speed;
		
		jumpSpeed += -Physics.gravity * time * 0.56f;
	}
	protected void ApplyBlink() {
		if (isAwake) {
			blinkDelay -= Time.deltaTime;
			if (blinkDelay <= 0) {
				blinkDelay = Random.Range(0.5f, 5);
				animator.SetBool("Blink", true);
			} else
				animator.SetBool("Blink", false);
		}
	}
	
	public override void Action(int action, bool bBlink ,AudioClip clip) {
		//if (isIdle) {
			bIgnoreBlink = bBlink;
			acting = true;
			actionType = action;
			animator.SetFloat ("ActionType", actionType);
			animator.SetBool ("Action", true);
			animator.SetBool ("LoopAction", false);
		//}
		if (clip != null) {
			TalkTo(clip, null, CutsceneStep.Action.Action);
		}
			
	}
	
	
	public void SetMoveType(float movetype) 
	{
			animator.SetFloat ("MoveType", movetype);
	}
	
	void ApplyRandomIdle() {
		if (numRandomIdle == 0)
			return;
		
		if (animator.GetBool ("RandomIdle")) {
			animator.SetBool("RandomIdle", false);
		} else
		if (isIdle && animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Idle")) {
			randomIdleDelay -= Time.deltaTime;
			if (randomIdleDelay <= 0) {
				randomIdleDelay = Random.Range(2f, 4f);
				animator.SetFloat("RandomIdleType", Random.Range (0, numRandomIdle) + 1);
				animator.SetBool("RandomIdle", true);
			}
		} else 
			randomIdleDelay = Random.Range(2f, 4f);
	}
	
	public override void Setup(Vector3 pos, Quaternion rota, Vector3 scale) {
		base.Setup(pos, rota, scale);
		if (useGravity && charConn != null) {
			Vector3 moveDirection = new Vector3(1, -1, 0);
			charConn.Move (moveDirection* 0.0001f);
			moveDirection.x = -1;
			charConn.Move (moveDirection* 0.0001f);
			//charConn.Move (Physics.gravity);
		}
	}

	public override bool TouchDown(Vector3 point) {
		if (!waitingTouch && isIdle) {
			if (headForm != null && point.y > headForm.position.y && headTouchActions.Length > 0) {
				Action (headTouchActions[Random.Range(0, headTouchActions.Length)], false ,touchSound);
			} else if (bodyTouchActions.Length > 0) {
				Action (bodyTouchActions[Random.Range(0, bodyTouchActions.Length)], false, touchSound);
			}
		}
		else if( waitingTouch ) 
		{
			if( animator != null )
			{
				if( animator.GetBool("LoopAction") )
				{
					animator.SetBool("LoopAction", false);
				}
			}
		}
		
		return base.TouchDown(point);
	}
	
	bool IsPerfectIdle() {
		return !animator.IsInTransition(0) && animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Idle");
	}

	public void SetCatching(bool cc) {
		animator.SetBool("Catch", cc);
	}
	
#if UNITY_EDITOR
	class ActionItem {
		public int index;
		public string category;
		public string filename;
		public string caption;
	}
	
	public void ReadActionData() {
		if (actions == null) {
			actionNames = null;
			return;
		}
		
		List<ActionItem> list = new List<ActionItem>();
		
		ActionItem item = new ActionItem();
		item.index = 0;
		item.caption = "(None)";
		list.Add(item);
		
		if (actions != null) {
			XmlReader reader = XmlReader.Create(new StringReader(actions.text));
	
			while (reader.Read())
		    {
				// Only detect start elements.
				if (reader.IsStartElement())
				{
					switch (reader.Name) {
					case "Item":
						if (item != null && item.index > 0) {
							list.Add(item);
						}
						item = new ActionItem();
						break;
						
					case "index":
						if (reader.Read() && reader.NodeType == XmlNodeType.Text) {
							item.index = int.Parse(reader.Value);
						}
						break;
	
					case "category":
						if (reader.Read() && reader.NodeType == XmlNodeType.Text) {
							item.category = reader.Value;
						}
						break;
	
					case "filename":
						if (reader.Read() && reader.NodeType == XmlNodeType.Text) {
							item.filename = reader.Value;
						}
						break;
	
					case "caption":
						if (reader.Read() && reader.NodeType == XmlNodeType.Text) {
								item.caption = reader.Value;
						}
						break;
					}
			    }
			}
			
			if (item != null && item.index > 0)
				list.Add(item);
		}
		
		actionNames = new string[list.Count];
		actionIds = new int[list.Count];
		
		for (int i = 0; i < list.Count; i++) {
			item = list[i];
			
			if (i == 0)
				actionNames[i] = item.caption;
			else {
				
				if( item.caption == null )
				{
					//actionNames[i] = string.Format("{0}/{1}. {2} ", item.category, item.index,item.filename);
					actionNames[i] = item.category + "/" + item.index + "." + item.filename;
				}
				else
				{
					//actionNames[i] = string.Format("{0}/{1}.{2} ", item.category, item.index, item.filename);
					actionNames[i] = item.category + "/" + item.index + "." + item.filename;
					
					
					char[] temp = item.caption.ToCharArray();
					int length = 40;
					if( item.caption.Length < length )
						length = item.caption.Length;
						
					
					for(int count=0; count < length; count++ ) {
						actionNames[i] += temp[count];
					}
						
						
					
//					Debug.Log ("item.caption.Length "+item.caption.Length);
//					Debug.Log ("actionNames "+actionNames[i]);
				}
				//actionNames[i] = string.Format("{0}/{1}. {2} {3}", item.category, item.index, item.filename, item.caption);
				//actionNames[i] = string.Format("{0}/{1}. {2} ", item.category, item.index,item.filename);
			}
			actionIds[i] = item.index;
		}
	}
	
	public void ReadLoopActionData() {
		if (loopActions == null) {
			loopActionNames = null;
			return;
		}		
		
		List<ActionItem> list = new List<ActionItem>();
		
		ActionItem item = new ActionItem();
		item.index = 0;
		item.caption = "(None)";
		list.Add(item);
		
		if (loopActions != null) {
			XmlReader reader = XmlReader.Create(new StringReader(loopActions.text));
	
			while (reader.Read())
		    {
				// Only detect start elements.
				if (reader.IsStartElement())
				{
					switch (reader.Name) {
					case "Item":
						if (item != null && item.index > 0) {
							list.Add(item);
						}
						item = new ActionItem();
						break;
						
					case "index":
						if (reader.Read() && reader.NodeType == XmlNodeType.Text) {
							item.index = int.Parse(reader.Value);
						}
						break;
	
					case "category":
						if (reader.Read() && reader.NodeType == XmlNodeType.Text) {
							item.category = reader.Value;
						}
						break;
	
					case "filename":
						if (reader.Read() && reader.NodeType == XmlNodeType.Text) {
							item.filename = reader.Value;
						}
						break;
	
					case "caption":
						if (reader.Read() && reader.NodeType == XmlNodeType.Text) {
							item.caption = reader.Value;
						}
						break;
					}
			    }
			}
			
			if (item != null && item.index > 0)
				list.Add(item);
		}
		
		loopActionNames = new string[list.Count];
		loopActionIds = new int[list.Count];
		
		for (int i = 0; i < list.Count; i++) {
			item = list[i];
			
			if (i == 0)
				loopActionNames[i] = item.caption;
			else {
				
				
				loopActionNames[i] = string.Format("{0}/{1}. {2} ", item.category, item.index, item.filename);
				
				
				char[] temp = item.caption.ToCharArray();
				int length = 40;
				if( item.caption.Length < length )
					length = item.caption.Length;
					
				
				for(int count=0; count < length ;count++ ) {
					loopActionNames[i] += temp[count];
				}
				//loopActionNames[i] = string.Format("{0}/{1}. {2} {3}", item.category, item.index, item.filename, item.caption);
			}
			loopActionIds[i] = item.index;
		}
	}	
#endif	
	
	protected override void ApplyTransform() {
		if (charConn == null) {
			base.ApplyTransform();
			return;
		}

		
		Vector3 speed = Vector3.zero;
		
		if (jumping) {
			jumpSpeed += Physics.gravity * Time.deltaTime;
			speed = jumpSpeed;
			
			Vector3 dir = jumpSpeed;
			dir.y = 0;
			transform.rotation = Quaternion.LookRotation(dir);
		} else {
			if (moveDummy != null) {
				speed = moveDummy.position - transform.position;
				speed.y = 0;
				if (speed != Vector3.zero) {
					if( Time.deltaTime > 0 )
					{
						speed /= Time.deltaTime;
						transform.rotation = Quaternion.LookRotation(speed);
					}
					//Debug.Log ("transform.rotation.eulerAngles : "+transform.rotation.eulerAngles);
				}
			} 
			else if (needBaseRotation) {
				transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(baseRotation), Time.deltaTime * 5f);
				float y = transform.eulerAngles.y;
				y -= baseRotation.y;
				if (y < -180)
					y += 360;
				animator.SetFloat("Angle", y * 0.03f);
				
			}
			
			if (useGravity)
				speed += Physics.gravity;
		}
		if( Time.deltaTime > 0 )
			collisionFlags = charConn.Move(speed * Time.deltaTime);
		
		if (jumping && (collisionFlags & CollisionFlags.CollidedBelow) != 0) {
			jumping = false;
			ResumeMoving();
		}
	}
	
	public override void UniqueAction(string name, int phase) {
		if (numRandomIdle > 0)
			animator.SetFloat("RandomIdleType", 0);
		base.UniqueAction(name, phase);
	}	
}
