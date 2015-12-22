/*
Actor - Camera Variant
Written by Jong Lee
11/14/2012
*/

using UnityEngine;
using System.Collections;

public class ActorCam : ActorBase {
	
	Vector3 followOffset;
	float xDist = 0;
	bool followVertical;
//	float followY;
	public CamMgr camMgr;
	float followSpeed = 0;

	// Use this for initialization
	protected override void Start () {
		base.Start ();
/*		
		if (transform.parent != null) {
			ActorBase actor = transform.parent.GetComponent<ActorBase>();
			if (actor != null) {
				LookAt(actor.transform, Vector3.zero, 0);
			}
		}*/
	}
	
	// Update is called once per frame
	protected override void Update () {
		base.Update ();
		
		if (formToFollow != null) {
			if (waiting) {
				Vector3 posToFollow = formToFollow.position + followOffset;
				float dist = transform.position.x - posToFollow.x;
				if (dist == 0 || (Mathf.Abs (dist) > Mathf.Abs (xDist))) {
					formToFollow = null;
				} else 
					xDist = dist;
			} else {				
	//			transform.position += GetFollowVec(formToFollow, followOffset, followVertical) * 0.1f;
				Vector3 speed = GetFollowVec(formToFollow, followOffset, followVertical) * 0.1f;
				if (followSpeed > 0 && speed.magnitude > followSpeed * Time.deltaTime) {
					speed = speed.normalized * followSpeed * Time.deltaTime;
				}
				transform.position += speed;
				
				camMgr.focusDist = followOffset.magnitude;
			}
		}
		
		if (isFollowing) {
		}
		
		Vector3 pos = Vector3.zero;
		if (GetLookAt(ref pos)) {
			camMgr.focusDist = Vector3.Distance(transform.position, pos);
//			transform.LookAt(pos);
		}
	}
	
	public override void Stop() {
		base.Stop ();
		
		waiting = false;
	}
	
	public override void Follow(Transform form, Vector3 offset, bool vertical, float speed) {
		StopMoving();
		
		if (form != null) {
			waiting = false;
			posBak = transform.position;
			formToFollow = form;
			followOffset = offset;
			followVertical = vertical;
			followSpeed = speed;
//			if (form != null)
//				followY = form.position.y + offset.y;
		}
	}
	
	public override void WaitFor(Transform form, Vector3 offset) {
		StopMoving();
		
		if (form != null) {
			waiting = true;
			xDist = float.MaxValue;
			formToFollow = form;
			followOffset = offset;
		}
	}
	
	public static Vector3 GetClosestPointOnLineSegment(Vector3 A, Vector3 B, Vector3 P)
    {
        Vector3 AP = P - A;       //Vector from A to P   
        Vector3 AB = B - A;       //Vector from A to B  

        float magnitudeAB = AB.sqrMagnitude;     //Magnitude of AB vector (it's length squared)     
        float ABAPproduct = Vector3.Dot(AP, AB);    //The DOT product of a_to_p and a_to_b     
        float distance = ABAPproduct / magnitudeAB; //The normalized "distance" from a to your closest point  

        return A + AB * distance;
    }
	
	public override Vector3 GetFollowVec(Transform form, Vector3 offset, bool vertical) {
		Vector3 posToFollow = (form.position + offset);
/*		if (!vertical)
			posToFollow.y = followY;*/
		Vector3 speed = (posToFollow - transform.position);
//		if (!vertical)
//			speed.y = 0;
		return speed;
	}
}
