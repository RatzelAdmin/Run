using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

public class ChangeDialogScript : ScriptableObject {
	
	[MenuItem ("Custom/ChangeDialog/ChangeDialog_KoreanToTaiwan")]
	static void ChangeDialog01() {
		FindAndChangeDialog(0);
	}
	[MenuItem ("Custom/ChangeDialog/ChangeDialog_TaiwanToKorean")]
	static void ChangeDialog02() {
		FindAndChangeDialog(1);
	}
	
	private static void FindAndChangeDialog( int type )
	{
		// type : 0 Korean -> Taiwan
		// type : 1 Taiwan -> Korean
		CutsceneMgr _cutsceneScript ;
		string _pathName;

		// Find all of the GameObjects in the scene and sort them
		// by the "path" in the scene hierarchy
		List<GameObject> brokenObjects = Resources
			.FindObjectsOfTypeAll( typeof( GameObject ) )
				.Cast<GameObject>()
				.Where(x => true && x.GetComponents<CutsceneMgr>().Any( c => c != null ) ) // x => x.activeInHierarchy && 
				.OrderBy( x => getObjectPath( x ) )
				.ToList();
		for( int i = 0; i < brokenObjects.Count; ++i )
		{
			_cutsceneScript = brokenObjects[i].GetComponent<CutsceneMgr>() as CutsceneMgr;
			Debug.Log ( _cutsceneScript );
			Debug.Log ( _cutsceneScript.dialogData );
			if(  _cutsceneScript.dialogData != null )
			{
				Debug.Log ( _cutsceneScript.dialogData.name );
				Debug.Log(AssetDatabase.GetAssetPath(_cutsceneScript.dialogData));
				_pathName = AssetDatabase.GetAssetPath(_cutsceneScript.dialogData);
				if( type == 0 )
					_pathName = _pathName.Replace("/Korean/","/Taiwan/");
				else if( type == 1 )
					_pathName = _pathName.Replace("/Taiwan/","/Korean/");
				else 
					_pathName = _pathName.Replace("/Korean/","/Taiwan/");
				
				Debug.Log ( _pathName );
				DialogData dialog = AssetDatabase.LoadAssetAtPath( _pathName
					, typeof(DialogData)) as DialogData;
				_cutsceneScript.dialogData = dialog;
			}

			if( _cutsceneScript.BGMData != null )
			{
				_pathName = AssetDatabase.GetAssetPath(_cutsceneScript.BGMData);
				if( type == 0 )
					_pathName = _pathName.Replace("/Korean/","/Taiwan/");
				else if( type == 1 )
					_pathName = _pathName.Replace("/Taiwan/","/Korean/");
				else 
					_pathName = _pathName.Replace("/Korean/","/Taiwan/");
				
				Debug.Log ( _pathName );
				BGMData bgm = AssetDatabase.LoadAssetAtPath( _pathName
					, typeof(BGMData)) as BGMData;
				_cutsceneScript.BGMData = bgm;
			}
		}
		
		SongData _songScript ;
		// Find all of the GameObjects in the scene and sort them
		// by the "path" in the scene hierarchy
		List<GameObject> brokenSongObjects = Resources
			.FindObjectsOfTypeAll( typeof( GameObject ) )
				.Cast<GameObject>()
				.Where(x => true && x.GetComponents<SongData>().Any( c => c != null ) ) // x => x.activeInHierarchy && 
				.OrderBy( x => getObjectPath( x ) )
				.ToList();

		for( int i = 0; i < brokenSongObjects.Count; ++i )
		{
			_songScript = brokenSongObjects[i].GetComponent<SongData>() as SongData;
			
			if(  _songScript.BackgroundTrack != null )
			{
				_pathName = AssetDatabase.GetAssetPath(_songScript.BackgroundTrack);
				if( type == 0 )
					_pathName = _pathName.Replace("/Korean/","/Taiwan/");
				else if( type == 1 )
					_pathName = _pathName.Replace("/Taiwan/","/Korean/");
				else 
					_pathName = _pathName.Replace("/Korean/","/Taiwan/");
				
				Debug.Log ( _pathName );
				AudioClip sound = AssetDatabase.LoadAssetAtPath( _pathName
				                                                , typeof(AudioClip)) as AudioClip;
				_songScript.BackgroundTrack = sound;
			}

			if(  _songScript.dialogData != null )
			{
				_pathName = AssetDatabase.GetAssetPath(_songScript.dialogData);
				if( type == 0 )
					_pathName = _pathName.Replace("/Korean/","/Taiwan/");
				else if( type == 1 )
					_pathName = _pathName.Replace("/Taiwan/","/Korean/");
				else 
					_pathName = _pathName.Replace("/Korean/","/Taiwan/");
				
				Debug.Log ( _pathName );
				DialogData dialog = AssetDatabase.LoadAssetAtPath( _pathName
				                                                  , typeof(DialogData)) as DialogData;
				_songScript.dialogData = dialog;
			}
			
			if( _songScript.BGMData != null )
			{
				_pathName = AssetDatabase.GetAssetPath(_songScript.BGMData);
				if( type == 0 )
					_pathName = _pathName.Replace("/Korean/","/Taiwan/");
				else if( type == 1 )
					_pathName = _pathName.Replace("/Taiwan/","/Korean/");
				else 
					_pathName = _pathName.Replace("/Korean/","/Taiwan/");
				
				Debug.Log ( _pathName );
				BGMData bgm = AssetDatabase.LoadAssetAtPath( _pathName
				                                            , typeof(BGMData)) as BGMData;
				_songScript.BGMData = bgm;
			}
		}
	}

	
	/// <summary>
	/// Returns the nesting level of a GameObject
	/// </summary>
	/// <param name="x"></param>
	/// <returns></returns>
	private static string getObjectPath( GameObject x )
	{
		
		var path = new System.Text.StringBuilder( 1024 );
		
		var depth = 0;
		
		while( x != null && x.transform.parent != null )
		{
			path.Append( x.name );
			path.Append( "/" );
			x = x.transform.parent.gameObject;
			depth += 1;
		}
		
		return depth.ToString( "D12" ) + "/" + path.ToString();
		
	}
}
