using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public class LcActorTexEditor {
	[MenuItem ("Assets/Create/Image Actor")]
    static void CreateImageActor () {
		if (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(Texture2D)) {
			Texture2D tex = Selection.activeObject as Texture2D;
			
			Material mat = new Material(Shader.Find ("Unlit/Transparent"));
			mat.mainTexture = tex;
			AssetDatabase.CreateAsset(mat, Path.ChangeExtension(AssetDatabase.GetAssetPath(tex), ".mat"));
			
			GameObject go = AssetDatabase.LoadAssetAtPath("Assets/Program/_Resources/Prefabs/TexActor.prefab", typeof(GameObject)) as GameObject;
			GameObject prefab = PrefabUtility.CreatePrefab(string.Format ("Assets/Art/Pref/Image/{0}.prefab", tex.name), go);
			prefab.renderer.material = mat;
			
			AssetDatabase.SaveAssets();
			Selection.activeGameObject = prefab;
		}
    }

	[MenuItem ("Assets/Create/Object Actor")]
    static void CreateObjectActor () {
		if (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(GameObject)) {
			GameObject go = GameObject.Instantiate(Selection.activeGameObject) as GameObject;
			ActorObject actor = go.AddComponent<ActorObject>();
			actor.useGravity = false;
			//actor.headForm = go.transform;
			actor.preWaitTouch = AssetDatabase.LoadAssetAtPath(Path.Combine(EditorGlobal.effectPath, "Dora_Touch_Wait_eff.prefab"), typeof(Transform)) as Transform;
			Animator animator = go.GetComponent<Animator>();
			if (animator != null)
				animator.applyRootMotion = false;
			BoxCollider box = go.AddComponent<BoxCollider>();
			box.isTrigger = true;
			box.size = new Vector3(0.5f, 0.5f, 0.5f);
			
			GameObject prefab = PrefabUtility.CreatePrefab(string.Format ("Assets/Art/Pref/Object/{0}.prefab", Selection.activeObject.name), go);
			
			GameObject.DestroyImmediate(go);
			
			AssetDatabase.SaveAssets();
			Selection.activeGameObject = prefab;
		}
    }
	
	[MenuItem ("Assets/Create/Generic Actor")]
    static void CreateGenericActor () {
		if (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(GameObject)) {
			
			
			GameObject go = GameObject.Instantiate(Selection.activeGameObject) as GameObject;
			go.tag = "Player";
			ActorChar actor = go.AddComponent<ActorChar>();
			actor.useGravity = true;
			
			//actor.headForm = actor.transform.FindChild( "Point_head_client" );
			Transform[] transforms = actor.transform.GetComponentsInChildren<Transform>();
			
			actor.headForm = null;
	        foreach (Transform tf in transforms) {
	            if( tf.name == "Point_head_client" )
				{
					actor.headForm = tf;
					break;
				}
	        }
			
			Debug.Log ("actor.headForm = "+actor.headForm);
			
			actor.preWaitTouch = AssetDatabase.LoadAssetAtPath(Path.Combine(EditorGlobal.effectPath, "Dora_Touch_Wait_eff.prefab"), typeof(Transform)) as Transform;
			Animator animator = go.GetComponent<Animator>();
			if (animator != null) {
				animator.applyRootMotion = false;
			}
			BoxCollider box = go.AddComponent<BoxCollider>();
			box.isTrigger = true;
			box.size = new Vector3(0.5f, 0.5f, 0.5f);
			AudioSource audioSrc = go.AddComponent<AudioSource>();
			audioSrc.loop = false;
			
			
			GameObject baseObject = AssetDatabase.LoadAssetAtPath("Assets/Program/_Resources/Prefabs/ActorBase.prefab", typeof(GameObject)) as GameObject;
			//baseObject.name = go.name;
			ActorBase actorbase =  baseObject.GetComponent<ActorBase>();
			actorbase.objIncludeActor = go.GetComponent<ActorBase>();
			
			GameObject go1 = GameObject.Instantiate(baseObject) as GameObject;
			go.transform.parent = go1.transform;
			go.name = Selection.activeObject.name;
			
			GameObject prefab = PrefabUtility.CreatePrefab(string.Format ("Assets/Art/Pref/Charactor/{0}.prefab", Selection.activeObject.name), go1);
			//GameObject prefab = PrefabUtility.CreatePrefab(string.Format ("Assets/Design/Actor/Generic/{0}.prefab", Selection.activeObject.name), go);
			
			
			
			//GameObject.DestroyImmediate(go);
			GameObject.DestroyImmediate(go1);
			
			AssetDatabase.SaveAssets();
			Selection.activeGameObject = prefab;
		}
    }	
}
