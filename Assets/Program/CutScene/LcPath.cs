/*
Path class
Written by Jong Lee
12/18/2012
*/

using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LcPath : MonoBehaviour {
	public Transform[] forms;
	public bool loop;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
#if UNITY_EDITOR	
	void OnDrawGizmos(){
		if(enabled && forms != null && forms.Length > 0) {
			if (Selection.activeTransform != null && Selection.activeTransform.IsChildOf(transform)) {
				int count = loop ? forms.Length + 1 : forms.Length;
				Vector3[] path = new Vector3[count];
				for (int i = 0; i < forms.Length; i++) {
					path[i] = forms[i].position;
				}
				if (loop) {
					path[count - 1] = forms[0].position;
				}
				
				iTween.DrawPath(path, Color.cyan);
			}
		}
	}
#endif	
}
