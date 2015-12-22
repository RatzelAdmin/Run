/*
World - the global class compiled early
Written by Jong Lee
3/7/2013
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class World : MonoBehaviour {
#if UNITY_ANDROID
	#if EnglishVersion 
		public static readonly string bundleVersion = "1.1.0";
	#else
		public static readonly string bundleVersion = "1.1.0";
	#endif
#else //UNITY_IPHONE
	#if EnglishVersion 
		public static readonly string bundleVersion = "1.2.0";
	#else
		public static readonly string bundleVersion = "1.1.0";
	#endif
#endif
	public static bool bForceUpdate = false;
	public static string bundleVersion_Server;
	public static string nextEnglishEpisodeName_01;
	public static string nextEnglishEpisodeName_02;
	public class DoraXmlData {
		public int id;
		public string name;
		public int enable;								// 0 : EPISODE_COME, 1: EPISODE_SELL, 2: EPISODE_FREE
		public int version;
		public int buyEpisode;							// 0: NOT BUY, 1: BUY
		
		public string name_English;
		public int version_English;
	}
	public class DoraXmlEnglishPackageData {
		public int id;
		public string englishEpisodeName_Level1;
		public string englishEpisodeName_Level2;
	}
	
	
#if EnglsihVersion 
	
#if DLA04 
	public static string url_DLAData = "http://14.63.169.205/DLA_Data/DLA04_Sing_Data.xml";
#elif DLA05
	public static string url_DLAData = "http://14.63.169.205/DLA_Data/DLA05_Sing_Data.xml";
#else
	public static string url_DLAData = "http://14.63.169.205/DLA_Data/DLA04_Sing_Data.xml";
#endif
	
#else //KoreaVersion
	
#if DLA04 
	public static string url_DLAData = "http://14.63.169.205/DLA_Data/DLA04_Data.xml";
#elif DLA05
	public static string url_DLAData = "http://14.63.169.205/DLA_Data/DLA05_Data.xml";
#else
	public static string url_DLAData = "http://14.63.169.205/DLA_Data/DLA04_Data.xml";
#endif
	
#endif
	
	public static string url_DLAEnglishPackageList = "http://14.63.169.205/DLA_Data/DLA_EnglishPackageList.xml";
#if DevVersion

#if TaiwanVersion
	public static string url_BundleBase  = "https://download.blueark.com/dla/DLA_Assetbundles_DEV_Taiwan/";
#else
	public static string url_BundleBase  = "http://op01-ws3869.ktics.co.kr/";		// Blooming Modify 15.10.19

#endif

#else //DevVersion
#if TaiwanVersion
	
#if NewUnityVersion
	public static string url_BundleBase  = "https://download.blueark.com/dla/DLA_Assetbundles_new/";
#else
	public static string url_BundleBase  = "https://download.blueark.com/dla/DLA_Assetbundles_Taiwan/";
#endif

#else
	//public static string url_BundleBase  = "http://14.63.169.205/DLA_Assetbundles_DEV/";
	public static string url_BundleBase  = "http://op01-ws3869.ktics.co.kr/";
	//public static string url_BundleBase  = "http://14.63.169.205/DLA_Assetbundles/";
#endif

#endif



#if TestVersion_API
	public static string url_ApiBase  = "http://14.63.171.143/";
#else
	public static string url_ApiBase  = "https://api.doralab.co.kr/";
#endif
	
	
	public static int currentEnglishIndex;
	public static int currentEpisodeIndex;
	public static int currentEpisodeIndex_Server;
	public static string currentEpisodeName_Client;
	public static int englishLevel; 					// 0 : not selected, 1 or 2
	
	public static bool bDataRecieve = false;
	public static bool bDontQuit = false;
	public static string sceneParam;
	public static string loadedLevelName = "";
	public static List<DoraXmlData> DoraXmlInfos = new List<DoraXmlData>();
	public static List<DoraXmlEnglishPackageData> DoraXmlEnglishPackageInfos = new List<DoraXmlEnglishPackageData>();
	
	public static bool bSceneChangeEffect = false;
	
	public static int clickPackageIndex;
#if EnglishVersion
	public static int langIdx = 1;
#else
	//SLP Verion Taiwan <--> English
	public static int langIdx = 1;
	//public static int langIdx = 0;
#endif
	public static int langCaptionIdx = -1;
	
	public static bool bPurchaseItem = false;
	public static string ProductID;
	
	public static bool bRestore = false;
	public static int totalCountRestoreItem = 0;
	public static int countRestoreItem = 0;
	
	public static string selectedEpisodeName;
	public static string selectedEnglishEpisodeName;

#if EnglishVersion && DLA04 && UNITY_IPHONE
	public static string sharedSecretKey = "";
#elif EnglishVersion && DLA05 && UNITY_IPHONE
	public static string sharedSecretKey = "";
#elif KoreaVersion && DLA04 && UNITY_IPHONE
	public static string sharedSecretKey ="3db5339cacbe4d2594bfa0fd58c0e236";
#elif KoreaVersion && DLA05 && UNITY_IPHONE
	public static string sharedSecretKey = "";
#else 
	public static string sharedSecretKey = "";
#endif
	
	
#if EnglishVersion && DLA04 && UNITY_ANDROID
	public static string GoogleKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA1Xx8Yx9jESxFgwzOZhAE6TXAFu5AXXALR7+Iplv9pgbwO0DJgRfMMb2YHCo3W5HpI5tg4iGFVuvNNGN8uEtrmhpu9NC55Aa8FB8rue6X/QuSEWRIkdaPjX7DGojINfHcvKb1Fa0uA7A4PIZCVlWE/lH1esxfK1+MsO0DLIOZVqzDySrDIt/injugJG3mZP3a3YXwQfbDV3wMhtRiJYNK7wIbg5VHuA23IZ8jXmuqaSlFz1h6Bv2XWKb84lg0m57YR4AI4zkQow48ob40EftlzS+TSd+cIR80Cu72swS/17EoxeJoxYPml2wVcKjGPJKZBkfKdNCdfpiA8wA2TAgXFQIDAQAB";
	public static string GoogleAccountCode = "";
	public static string GoogleAccountClientID = "";
#elif EnglishVersion && DLA05 && UNITY_ANDROID
	public static string GoogleKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAr0cHkTQA4s2zfnhTQVxyoni5tJRixMZq1GxAMyzXbPSgPYyguqg/ioICYJcKxn0cVUCkl9HeLJjL9GrJ0srwMDG+HCOliC9CPOvb5SWaW/rflKrDwxW8EiQdhwV2FL2FrdqnCbuaCuQujdWARsZ0Pf+9tbdn2A9yXiV8HrgToOPp0Cz+DNriidz5wrgDGSFOW/to2ZYZkdPe9cWIhP5+G2fLULH5aStmR7fONYJmbYdSeN6gEs5FDfhP9GD13ZmqGn/DjP3ue7uDDJVuoP1hByFOgTBGAdf9t+buIKc+y1P4+NiV7RRSCfu7ClCdWCf160DdBEMaM6pk4f40vTzooQIDAQAB";
	public static string GoogleAccountCode = "";
	public static string GoogleAccountClientID = "";
#elif KoreaVersion && DLA04 && UNITY_ANDROID
	public static string GoogleKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA0z0B1LnLrJc5c2dSsvHc3wu4pdvxszhYWiwJxKwm9Tpp7gS6D1tMJQBw6WMmovSQNm4ob0Lp11ep25zUa1XOKM99WhQQ/L3yIfeAaFEUebqBpcMLhLwXDSH/BSDgMFsJgtwMDleu39TXInsLOow07x+ta0gaGuQ5fxQY7fN5Vnhpb43z4ZrHelXlbQSn+nnOaqnmINns5W4oAeZeTsJPFF2l0xwQUH9/elPl3IMJn8rYFg0s4ZO2ebCnmMb2RqJATNBjm+exWi5xgPBsxTwWsbByiRa+RCzLejdf4qYLLr8h4ifxskIBX04FSF18OW10uEbln3hS6cpmhm5k6BxcqwIDAQAB";

	public static string GoogleAccountCode = "4/be93N4M2DdHva-iOzMC3G4IYteG2.kgikVDlduu8SXE-sT2ZLcbTLtRYfhwI";
	public static string GoogleAccountClientID = "490934392553.apps.googleusercontent.com";
#elif KoreaVersion && DLA05 && UNITY_ANDROID
	public static string GoogleKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAhq8SmUWpn2MxFOeQ3dcjEbYaAtpsjSq84by32AV7lcTqjUa9oQlHD1cww12E33Q6DIA/l9cMH/gWXgzTwQcIdKZOenS2zHrb1iwEIj9bnY6eXbdZKWDpNUNRfIBw2aRw11j/BF2gsqyf6/C+QQLBDiA0QNykqWQJ0DP0YmKuYq7qluBIoa7Lk+j5H1Onu2z2m7mJOjJTZuBvMzUXemkCoIsCJYCzMv3/Xm1FzigsQb2kB2UAKLxT9j19D3vZtW51rw0T7ORIP4FCmjHnbWm3W09zFHcTK6C/5QhOneITEjVc6UgIxhTWox+aWLkPMhrTLIeDh0baU2u6sIn+5pZdSwIDAQAB";
	public static string GoogleAccountCode = "";
	public static string GoogleAccountClientID = "";
#else 
	public static string GoogleKey = "";
	public static string GoogleAccountCode = "";
	public static string GoogleAccountClientID = "";
#endif
	public static string userID = "";
	public static string login_AccountID = "";
	public static string login_Password = ""; 
	public static string login_accesstoken = ""; 
	public static string purchasekey = "";
	
	public static int index_Category = -1;
	public static int index_Episode = 0;
	
	
	void Awake() {
		currentEpisodeIndex = PlayerPrefs.GetInt("currentEpisodeIndex",0);
		currentEpisodeIndex_Server = PlayerPrefs.GetInt("currentEpisodeIndex_Server",0);
		currentEpisodeName_Client = PlayerPrefs.GetString("currentEpisodeName_Client","episodeS101"); 
		englishLevel = PlayerPrefs.GetInt("englishLevel",0);
		userID = PlayerPrefs.GetString("userID");
		
		langCaptionIdx = PlayerPrefs.GetInt("World.langCaptionIdx", -1);
	}
	
	
	public static int GetEnglishIndexFromName( string name )
	{
		int index = 0;

		foreach( DoraXmlEnglishPackageData info01 in DoraXmlEnglishPackageInfos )
		{					
			if( name == info01.englishEpisodeName_Level1 
				|| name == info01.englishEpisodeName_Level2 )
			{
				index = info01.id;
				break;
			}
			
		}
		
		return index;
	}
	
	public static int GetEnglishIndexFromEpisodeIndex( int index_Episode )
	{
		int index = 0;
		foreach( DoraXmlData info in DoraXmlInfos)
		{
			if( info.id == index_Episode )
			{
				
				foreach( DoraXmlEnglishPackageData info01 in DoraXmlEnglishPackageInfos )
				{					
					if( info.name_English == info01.englishEpisodeName_Level1 
						|| info.name_English == info01.englishEpisodeName_Level2 )
					{
						index = info01.id;
						break;
					}
					
				}
				break;
			}
		}
		
		
//		int index = PlayerPrefs.GetInt("EnglishIndex_Episode"+index_Episode.ToString(),-1);
//		Debug.Log ("EnglishIndex_Episode"+index_Episode.ToString() +" "+index);
//		Debug.Log ("GetEnglishIndexFromEpisodeIndex "+index);
		
		return index;
	}
	
	public static void SetBuyEpisode( int index_Episode )
	{
		foreach( DoraXmlData info in DoraXmlInfos)
		{
			if( index_Episode == info.id )
			{
				info.buyEpisode = 1;
				break;
			}
		}
		PlayerPrefs.SetInt ("buyEpisode"+index_Episode.ToString(), 1 );
		
		
		GameObject AppMgr = GameObject.FindWithTag("AppMgr");
		if( AppMgr != null )
		{
			AppMgr.SendMessage("SetBuyEpisode",index_Episode, SendMessageOptions.DontRequireReceiver);
		}
	}
	public static void AddEngilshIndex( int index_Episode )
	{
		int index = PlayerPrefs.GetInt("EnglishIndex_Episode"+index_Episode.ToString(),-1);
		if( index == -1 )
		{
			++currentEnglishIndex;
			PlayerPrefs.SetInt("currentEnglishIndex",currentEnglishIndex);
			PlayerPrefs.SetInt("EnglishIndex_Episode"+index_Episode.ToString(), currentEnglishIndex);
		}
		Debug.Log ("EnglishIndex_Episode"+index_Episode.ToString() +" "+currentEnglishIndex);
		Debug.Log ("AddEngilshIndex "+currentEnglishIndex);
	}
	public static void AddEngilshIndexRestoreVersion( int index_Episode )
	{
//		int index = PlayerPrefs.GetInt("EnglishIndex_Episode"+index_Episode.ToString(),-1);
//		if( index == -1 )
//		{
//			++currentEnglishIndex;
//			PlayerPrefs.SetInt("currentEnglishIndex",index_Episode);
//			PlayerPrefs.SetInt("EnglishIndex_Episode"+index_Episode.ToString(), index_Episode);
//		}
		++currentEnglishIndex;
		PlayerPrefs.SetInt("currentEnglishIndex",index_Episode);
		PlayerPrefs.SetInt("EnglishIndex_Episode"+index_Episode.ToString(), currentEnglishIndex);
		Debug.Log ("EnglishIndex_Episode"+index_Episode.ToString() +" "+currentEnglishIndex);
		Debug.Log ("AddEngilshIndexRestoreVersion "+currentEnglishIndex);
	}
	
	public static void SetEngilshIndexRestoreVersion()
	{
		currentEnglishIndex = 0;
		foreach( DoraXmlData info in DoraXmlInfos)
		{
			if( info.buyEpisode == 1 )
			{
				++currentEnglishIndex;
				PlayerPrefs.SetInt("EnglishIndex_Episode"+info.id.ToString(), currentEnglishIndex);
				Debug.Log ("EnglishIndex_Episode"+info.id.ToString() +" "+currentEnglishIndex);
				Debug.Log ("AddEngilshIndexRestoreVersion "+currentEnglishIndex);
			}
		}
	}
	public static bool CheckExist_Assetbundle( string episodename )
	{
		Debug.Log ("CheckExist_Assetbundle "+episodename);
	#if UNITY_ANDROID
		var path = Application.persistentDataPath  + "/UnityCache/Shared" ; 
	#elif UNITY_IPHONE
		string path = Application.temporaryCachePath + "/../UnityCache/Shared";
	#else
		string path = Application.persistentDataPath  + "/UnityCache/Shared" ; 	// blooming
	#endif
		//var path = Application.temporaryCachePath + "/../UnityCache/Shared";
		Debug.Log ("path " + path);
		if (!Directory.Exists(path)) 
		{
			Debug.Log ("Directory.Exists false");
			return false; 
		}
		//Debug.Log ("Directory.Exists true");
		
		DirectoryInfo info = new DirectoryInfo(path);
		var bfinddirectory = false;
			
		DirectoryInfo[] directoryInfo = info.GetDirectories();
		foreach ( DirectoryInfo directory in directoryInfo) 
		{
			//Debug.Log ("========================================");
			//Debug.Log (directory);
			string[] fileinfo  = Directory.GetFiles(directory.ToString(), "*.*");
			foreach (string file in fileinfo) 
			{
				//Debug.Log  (file);
				if( file.Contains( episodename ) )
				{
					bfinddirectory = true;
					Debug.Log ( "Contains True "+ episodename );
					break;
				}
			}
			if( bfinddirectory )
				break;
		}
		//Debug.Log ("========================================");
		 
		return  bfinddirectory;
	}
	
	public static void SendGameLog( string type , int id, bool bSuccess  )
	{
		string sceneName = World.loadedLevelName;
		string chapterName = "";
		if( sceneName == "")
			sceneName = Application.loadedLevelName;
		
		if( !sceneName.Contains("episode_e") && !sceneName.Contains("review_e") )
		{
			if( sceneName.Contains("chapter0") )
				chapterName = "C0";
			else if( sceneName.Contains("chapter1") )
				chapterName = "C1";
			else if( sceneName.Contains("chapter2") )
				chapterName = "C2";
			else if( sceneName.Contains("chapter3") )
				chapterName = "C3";
			
			int idx = sceneName.IndexOf('_');
			sceneName = sceneName.Substring(0, idx);
			
			sceneName = sceneName.Replace("episode", "");
			
			GameObject dla = GameObject.Find("DLAPlatform");
			if( dla != null )
			{
				string message ;
				if( bSuccess )
				{
					message = sceneName+"_"+chapterName+"_"+type+"_"+ id+"_T";
				}
				else
				{
					message = sceneName+"_"+chapterName+"_"+type+"_"+ id+"_F";
				}
				Debug.Log ("GameLog : "+message);
				dla.SendMessage("GameLog",message, SendMessageOptions.DontRequireReceiver);
			}
		}
		
	}
	public static void EpisodeChapterState( bool bStart  )
	{
		string sceneName = World.loadedLevelName;
		string chapterName = "";
		if( sceneName == "")
			sceneName = Application.loadedLevelName;
		
		Debug.Log ("EpisodeChapterState sceneName : "+sceneName);
		if( sceneName != "" && !bStart)
			PlayerPrefs.SetString("SuccessChapterName",sceneName);

		if( !sceneName.Contains("episode_e") && !sceneName.Contains("review_e") && !sceneName.Contains("GoToStart") )
		{
			if( sceneName.Contains("chapter0") )
				chapterName = "C0";
			else if( sceneName.Contains("chapter1") )
				chapterName = "C1";
			else if( sceneName.Contains("chapter2") )
				chapterName = "C2";
			else if( sceneName.Contains("chapter3") )
				chapterName = "C3";
			
			int idx = sceneName.IndexOf('_');
			sceneName = sceneName.Substring(0, idx);
			
			//sceneName = sceneName.Replace("episode", "");
			
			if( bStart )
			{
				Debug.Log ("GameLog : "+sceneName+"_"+chapterName+"_Start");
				//ChapterStart
				sceneName = sceneName+"_"+chapterName;
				Debug.Log ("GameLog : "+sceneName+" Start");
				GameObject dla = GameObject.Find("DLAPlatform");
				if( dla != null )
					dla.SendMessage("StartChapter",sceneName, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				Debug.Log ("GameLog : "+sceneName+"_"+chapterName+"_End");
				//ChapterEnd
				sceneName = sceneName+"_"+chapterName;
				Debug.Log ("GameLog : "+sceneName+" End");
				GameObject dla = GameObject.Find("DLAPlatform");
				if( dla != null )
					dla.SendMessage("FinishChapter",sceneName, SendMessageOptions.DontRequireReceiver);
				//dla.SendMessage("FinishEpisode",sceneName, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
	public static void EpisodeFinish()
	{
		string sceneName = World.loadedLevelName;
		if( sceneName == "")
			sceneName = Application.loadedLevelName;
		
		if( !sceneName.Contains("episode_e") && !sceneName.Contains("review_e") )
		{
			int idx = sceneName.IndexOf('_');
			sceneName = sceneName.Substring(0, idx);
			
			Debug.Log ("GameLog : "+sceneName+" EpisodeFinish");
			GameObject dla = GameObject.Find("DLAPlatform");
			if( dla != null )
				dla.SendMessage("FinishEpisode",sceneName, SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			if( sceneName.Contains("episode_e") )
			{
				int idx = sceneName.IndexOf('w');
				sceneName = sceneName.Substring(0, idx+2);
				
				Debug.Log ("GameLog : "+sceneName+" EnglishEpisodeFinish");
				GameObject dla = GameObject.Find("DLAPlatform");
				if( dla != null )
					dla.SendMessage("FinishEpisode",sceneName, SendMessageOptions.DontRequireReceiver);
			}
			else if( sceneName.Contains("review_e") )
			{
				int idx = sceneName.IndexOf('_');
				sceneName = sceneName.Substring(0, idx+5);
				
				Debug.Log ("GameLog : "+sceneName+" EnglishReviewEpisodeFinish");
				GameObject dla = GameObject.Find("DLAPlatform");
				if( dla != null )
					dla.SendMessage("FinishEpisode",sceneName, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
