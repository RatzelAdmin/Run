/*
Camara Manager
Written by Jong Lee
11/26/2012
*/

using UnityEngine;
using System.Collections;

public class CamMgr : MonoBehaviour {
	public float horiLimit = 8f;
	public float vertLimit = 8f;
	public float focusDist = 3f;
	float baseX;
	Vector3 dirSave = Vector3.zero;
	Vector3 angle;
	public float minMag = 0.01f;
	float timeLastFilter = 0f;
	bool updating = false;
	
	private Vector3 lowPassValue = Vector3.zero;
	Vector3 velocity = Vector3.zero;
	
	// Use this for initialization
	void Start () {
		baseX = -Input.acceleration.y;
	}
	
	// Update is called once per frame
	void Update () {
		GetDir();
	
		angle = LowPassFilterAccelerometer(dirSave);
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.RotateAround(transform.position + transform.forward * focusDist, transform.up, angle.y);
		transform.RotateAround(transform.position + transform.forward * focusDist, transform.right, angle.x);
	}
	
	bool GetDir() {
		Vector3 dirCurr = Vector3.zero;
		dirCurr.x = -Input.acceleration.y - baseX;
		dirCurr.y = Input.acceleration.x;
		dirCurr.z = 0;
		
		if ((dirCurr - dirSave).magnitude < minMag) {
			if (!updating)
				return false;
			else if (Time.time - timeLastFilter > 2f) {
				updating = false;
				baseX = -Input.acceleration.y;
				dirCurr.x = -Input.acceleration.y - baseX;
			}
		} else {
			updating = true;
			timeLastFilter = Time.time;
		}

		dirSave = dirCurr;
		
		return true;
	}
	
	Vector3 LowPassFilterAccelerometer(Vector3 dir) {
		dir *= 180f;
		dir.x = Mathf.Max (Mathf.Min (vertLimit, dir.x), -vertLimit);
		dir.y = Mathf.Max (Mathf.Min (horiLimit, dir.y), -horiLimit);
//		dir.x += baseX;
//		lowPassValue = Vector3.Lerp(lowPassValue, dir, Time.deltaTime * 3f);
//		lowPassValue = Vector3.Lerp(lowPassValue, Vector3.Lerp(lowPassValue, dir, 0.5f), Time.deltaTime * 3f);
		lowPassValue = Vector3.SmoothDamp(lowPassValue, dir, ref velocity, 0.4f);
		return lowPassValue;
	}
}
