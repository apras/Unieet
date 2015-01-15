using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Threading;
using LinqToTwitter;

//using CoreTweet;

namespace Unieet
{
	public class UnieetMain : Editor
	{
		[MenuItem("Unieet/Set Consumer Key and Consumer Secret")]
		private static void SetKey()
		{
			KeyWindow.OpenWindow();
		}

		[MenuItem("Unieet/Open")]
		private static void  Open()
		{
			System.Net.ServicePointManager.ServerCertificateValidationCallback += (s,ce,ca,p) => true;

			string[] _unitterPathes = Directory.GetDirectories(Application.dataPath, "*Unieet*", SearchOption.AllDirectories);
			string _unitterPath = Array.Find(_unitterPathes, s => s.Contains("Unieet"));

			if(!string.IsNullOrEmpty(_unitterPath))
			{
				KeyObj _keyObj;
				TokenObj _accessToken;
				string[] _splitPath = _unitterPath.Split(new string[]{ "Assets" }, StringSplitOptions.RemoveEmptyEntries);

				if(!File.Exists(Path.Combine(_unitterPath, "KeyObj.asset")))
				{
					Debug.LogError("Before Set Consumer Key and Consumer Secret.");
					return;
				}

				_keyObj = AssetDatabase.LoadAssetAtPath("Assets" + _splitPath[1] + "/KeyObj.asset", typeof(KeyObj)) as KeyObj;

				if(File.Exists(Path.Combine(_unitterPath, "TokenObj.asset")))
				{
					_accessToken = AssetDatabase.LoadAssetAtPath("Assets" + _splitPath[1] + "/TokenObj.asset", typeof(TokenObj)) as TokenObj;

					if(_accessToken != null)
					{
						UnieetCore.Instance.Init(_keyObj.ConsumerKey, _keyObj.ConsumerSecret);
						UnieetCore.Instance.UpdateTweetStream += new UnieetCore.TweetEventHandler(UnieetMain.updateTweetStream);
						UnieetCore.Instance.Authorize(_accessToken.Token, _accessToken.TokenSecret);
						UnieetCore.Instance.GetStreeming();
						UnieetWindow.OpenWindow();
						LoadTweets();
					}
				}
				else
				{
					UnieetCore.Instance.CallbackCompleteOAuth = () =>
					{
						_accessToken = ScriptableObject.CreateInstance<TokenObj>();
						string _path = AssetDatabase.GenerateUniqueAssetPath("Assets" + _splitPath[1] + "/TokenObj.asset");
						AssetDatabase.CreateAsset(_accessToken, _path);

						_accessToken.Token = UnieetCore.Instance.OAuthToken;
						_accessToken.TokenSecret = UnieetCore.Instance.OAuthTokenSecret;

						EditorUtility.SetDirty(_accessToken);
						AssetDatabase.SaveAssets();
						//
						UnieetCore.Instance.GetStreeming();
						UnieetWindow.OpenWindow();
					};

					UnieetCore.Instance.Init(_keyObj.ConsumerKey, _keyObj.ConsumerSecret);
					UnieetCore.Instance.UpdateTweetStream += new UnieetCore.TweetEventHandler(UnieetMain.updateTweetStream);
					Application.OpenURL(UnieetCore.Instance.GetAuthorizationLink());
					PinWindow.OpenWindow();
				}

			}
		}

		[MenuItem("Unieet/Close")]
		private static void  Close()
		{
			UnieetWindow.CloseWindow();
		}

		private static void updateTweetStream(object sender, UnieetCore.TweetEventArgs arg)
		{
			UnieetTweet _tweet = arg.Data[0];

			if(UnieetWindow.S_Tweet != null)
			{
				if(UnieetWindow.S_Tweet.FindIndex((tweet) => tweet.id_str == _tweet.id_str) == -1)
				{
					UnieetWindow.S_Tweet.Add(_tweet);
				}
			}

			UnieetWindow.S_Tweet.Sort(delegate(UnieetTweet a, UnieetTweet b)
			{
				DateTime _timeA = DateTime.FromBinary(long.Parse(a.created_at));
				DateTime _timeB = DateTime.FromBinary(long.Parse(b.created_at));

				return DateTime.Compare(_timeA, _timeB);
			});
			UnieetWindow.S_Tweet.Reverse();
		}

		public static void GenerateKeyFile(String ck, String cs)
		{
			string[] _unitterPathes = Directory.GetDirectories(Application.dataPath, "*Unieet*", SearchOption.AllDirectories);
			string _unitterPath = Array.Find(_unitterPathes, s => s.Contains("Unieet"));
			
			if(!string.IsNullOrEmpty(_unitterPath))
			{
				string[] _splitPath = _unitterPath.Split(new string[]{ "Assets" }, StringSplitOptions.RemoveEmptyEntries);

				if(File.Exists(Path.Combine(_unitterPath, "KeyObj.asset")))
				{
					File.Delete(Path.Combine(_unitterPath, "KeyObj.asset"));
				}

				KeyObj _keyObj = ScriptableObject.CreateInstance<KeyObj>();
				string _path = AssetDatabase.GenerateUniqueAssetPath("Assets" + _splitPath[1] + "/KeyObj.asset");
				AssetDatabase.CreateAsset(_keyObj, _path);
				
				_keyObj.ConsumerKey = UnieetCore.Instance.EncryptString(ck);
				_keyObj.ConsumerSecret = UnieetCore.Instance.EncryptString(cs);
				
				EditorUtility.SetDirty(_keyObj);
				AssetDatabase.SaveAssets();
			}
		}

		public static void Tweet(String message)
		{
			UnieetCore.Instance.Tweet(message);
		}

		public static void Tweet(String message, byte[] imageBytes, int width, int height)
		{
			UnieetCore.Instance.TweetWithMedia(message, "unieet_screenshot", width, height, imageBytes);
		}

		public static byte[] SkimScreenshot()
		{
			string _parentDir = Directory.GetParent( Application.dataPath ).ToString();
			string _path = _parentDir + "/unieet_screenshot.png";

			byte[] _bytes = File.ReadAllBytes(_path);
            File.Delete(_path);
            return _bytes;
        }
        
        public static void TakeScreenshot()
		{
			Application.CaptureScreenshot("unieet_screenshot.png");
        }

		public static void LoadTweets()
		{
			string[] _unitterPathes = Directory.GetDirectories(Application.dataPath, "*Unieet*", SearchOption.AllDirectories);
			string _unitterPath = Array.Find(_unitterPathes, s => s.Contains("Unieet"));
			
			if(!string.IsNullOrEmpty(_unitterPath))
			{
				TweetObj _tweetObj;
				string[] _splitPath = _unitterPath.Split(new string[]{ "Assets" }, StringSplitOptions.RemoveEmptyEntries);
				
				if(File.Exists(Path.Combine(_unitterPath, "TweetObj.asset")))
				{
					_tweetObj = AssetDatabase.LoadAssetAtPath("Assets" + _splitPath[1] + "/TweetObj.asset", typeof(TweetObj)) as TweetObj;
					if(_tweetObj != null)
					{
						if(_tweetObj.LastTweets.Count > 0)
						{
							UnieetWindow.S_Tweet = _tweetObj.LastTweets;
						}
					}
				}
			}
		}
                
		public static void SaveTweets()
		{
			string[] _unitterPathes = Directory.GetDirectories(Application.dataPath, "*Unieet*", SearchOption.AllDirectories);
			string _unitterPath = Array.Find(_unitterPathes, s => s.Contains("Unieet"));
			
			if(!string.IsNullOrEmpty(_unitterPath))
			{
				TweetObj _tweetObj;
				string[] _splitPath = _unitterPath.Split(new string[]{ "Assets" }, StringSplitOptions.RemoveEmptyEntries);
                
				if(File.Exists(Path.Combine(_unitterPath, "TweetObj.asset")))
				{
					_tweetObj = AssetDatabase.LoadAssetAtPath("Assets" + _splitPath[1] + "/TweetObj.asset", typeof(TweetObj)) as TweetObj;
				}
				else
				{
					_tweetObj = ScriptableObject.CreateInstance<TweetObj>();
					string _path = AssetDatabase.GenerateUniqueAssetPath("Assets" + _splitPath[1] + "/TweetObj.asset");
					AssetDatabase.CreateAsset(_tweetObj, _path);
				}

				if(_tweetObj != null)
				{
					_tweetObj.LastTweets = UnieetWindow.S_Tweet.Take(10).ToList();
                        
					//
					EditorUtility.SetDirty(_tweetObj);
					AssetDatabase.SaveAssets();
				}
			}
		}
	}
}