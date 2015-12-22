/*
Path editor
Written by Jong Lee
12/18/2012
*/

using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(LcPath))]
class LcPathEditor : Editor {
	SerializedProperty forms;
	SerializedProperty loop;
	
	[MenuItem ("Component/New Path")]
	public static void NewPath() {
		if (Selection.activeTransform != null) {
			string name;
			int i = 1;
			while (true) {
				name = string.Format("Path{0}", i);
				if (Selection.activeTransform.FindChild(name) == null)
					break;
				i++;
			}
			
			GameObject obj = new GameObject(name);
			obj.transform.position = Selection.activeTransform.position;
			obj.transform.parent = Selection.activeTransform;
			obj.AddComponent<LcPath>();
			Selection.activeObject = obj;
		}
	}
	
	void OnEnable () {
        forms = serializedObject.FindProperty ("forms");
		loop = serializedObject.FindProperty ("loop");
    }
	
    public override void OnInspectorGUI () {
		serializedObject.Update ();
		
		int idxRemove = -1;
		
		for (int i = 0; i < forms.arraySize; i++) {
			EditorGUILayout.BeginHorizontal();
			
			EditorGUILayout.PropertyField(forms.GetArrayElementAtIndex(i));
			if (GUILayout.Button("Remove")) {
				idxRemove = i;
			}
			
			EditorGUILayout.EndHorizontal();
		}
		
		if (idxRemove >= 0) {
			int countSave = forms.arraySize;
			if (forms.GetArrayElementAtIndex(idxRemove).objectReferenceValue != null) {
				GameObject.DestroyImmediate((forms.GetArrayElementAtIndex(idxRemove).objectReferenceValue as Transform).gameObject);
			}
			forms.DeleteArrayElementAtIndex(idxRemove);
			if (countSave == forms.arraySize)
				forms.DeleteArrayElementAtIndex(idxRemove);
		}

		if (GUILayout.Button("Add Node", GUILayout.Width(100))) {
			string name;
			int i = 1;
			while (true) {
				name = string.Format("{0}", i);
				if (Selection.activeTransform.FindChild(name) == null)
					break;
				i++;
			}
			
			GameObject obj = new GameObject(name);
			obj.transform.position = Selection.activeTransform.position;
			obj.transform.parent = Selection.activeTransform;
			int idx = forms.arraySize;
			forms.InsertArrayElementAtIndex(idx);
			forms.GetArrayElementAtIndex(idx).objectReferenceValue = obj.transform;
		}

		EditorGUILayout.PropertyField(loop);
		
		serializedObject.ApplyModifiedProperties ();
    }
}
