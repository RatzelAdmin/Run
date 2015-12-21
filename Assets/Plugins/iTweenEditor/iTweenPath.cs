//by Bob Berkebile : Pixelplacement : http://www.pixelplacement.com

using UnityEngine;
using System.Collections.Generic;

public class iTweenPath : MonoBehaviour
{
	public string pathName ="";
	public Color pathColor = Color.cyan;
	public List<Vector3> nodes = new List<Vector3>(){Vector3.zero, Vector3.zero};
	public int nodeCount;
	public static Dictionary<string, iTweenPath> paths = new Dictionary<string, iTweenPath>();
	public bool initialized = false;
	public string initialName = "";
	
	void OnEnable(){

//		// Original
//		paths.Add(pathName.ToLower(), this);


//		// case 1
//		paths.Add(pathName.ToLower(),this);
//		//Test2
//		if(!paths.ContainsKey(pathName))
//		{
//			paths[pathName.ToLower()] = this;
//		}


		// case 2
		if(!paths.ContainsKey(pathName))
		{
			paths[pathName.ToLower()] = this;
		}
	}
	
	void OnDrawGizmosSelected(){
		if(enabled) { // dkoontz
			if(nodes.Count > 0){
				iTween.DrawPath(nodes.ToArray(), pathColor);
			}
		} // dkoontz
	}
	
	public static Vector3[] GetPath(string requestedName){
		requestedName = requestedName.ToLower();
		if(paths.ContainsKey(requestedName)){
			return paths[requestedName].nodes.ToArray();
		}else{
			Debug.Log("No path with that name exists! Are you sure you wrote it correctly?");
			return null;
		}
	}
}

