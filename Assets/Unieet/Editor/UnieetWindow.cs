using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Unieet
{
	public class UnieetWindow : EditorWindow
	{
		public static UnieetWindow S_OpenedWindow;
		public static List<UnieetTweet> S_Tweet;
		private static bool S_FlagClose = false;
		private GUIStyle m_styleBoxMessage;
		private GUIStyle m_styleBoxProfileImage;
		private Texture2D m_normalBackground;
		private Texture2D m_lockBackground;
		private bool m_flagTakeScreenShot = false;
		private Texture2D m_screenShot;
		private byte[] m_screenShotBytes;
		private Font m_font;
		private int m_lastTweetCount = 0;
		private Vector2 m_scrollPos;
		private int m_selectedTabIndex = 0;
		private Dictionary<string, Texture> m_profileImages = new Dictionary<string, Texture>();
		private Dictionary<UnieetTweet, Rect> m_profileImagesRect = new Dictionary<UnieetTweet, Rect>();
		//
		private string m_tweetMessage = "";

		public static void OpenWindow()
		{
			if(UnieetWindow.S_OpenedWindow == null)
			{
				UnieetWindow.S_FlagClose = false;
				UnieetWindow.S_Tweet = new List<UnieetTweet>();
				UnieetWindow.S_OpenedWindow = (UnieetWindow)EditorWindow.GetWindow(typeof(UnieetWindow));
				UnieetWindow.S_OpenedWindow.position = new Rect(Screen.width / 2, Screen.height / 2, 400, 500);
				UnieetWindow.S_OpenedWindow.title = "Unieet";
			}


		}

		public static void CloseWindow()
		{
			if(UnieetWindow.S_OpenedWindow != null)
			{
				UnieetWindow.S_FlagClose = true;
			}
		}

		void OnGUI()
		{
			if(UnieetWindow.S_OpenedWindow != null)
			{

				if(this.m_font == null)
				{
					string[] _unitterPathes = Directory.GetDirectories(Application.dataPath, "*Unieet*", SearchOption.AllDirectories);
					string _unitterPath = Array.Find(_unitterPathes, s => s.Contains("Unieet"));
					if(!string.IsNullOrEmpty(_unitterPath))
					{
						string[] _splitPath = _unitterPath.Split(new string[]{ "Assets" }, StringSplitOptions.RemoveEmptyEntries);
						if(File.Exists(Path.Combine(_unitterPath, "Editor/mplus-2c-regular.ttf")))
						{
							this.m_font = AssetDatabase.LoadAssetAtPath("Assets" + _splitPath[1] + "/Editor/mplus-2c-regular.ttf", typeof(Font)) as Font;
						}
					}
				}


				if(this.m_styleBoxMessage == null)
				{
					this.m_styleBoxMessage = new GUIStyle(GUI.skin.box);
					this.m_styleBoxMessage.alignment = TextAnchor.UpperLeft;
					this.m_styleBoxMessage.richText = true;
					//this.m_styleBoxMessage.wordWrap = true;
					this.m_styleBoxMessage.fontSize = 11;
					this.m_styleBoxMessage.normal.textColor = Color.gray;
					if(this.m_font != null)
					{
						this.m_styleBoxMessage.font = this.m_font;
					}
				}

				if(this.m_styleBoxProfileImage == null)
				{
					this.m_styleBoxProfileImage = new GUIStyle(GUI.skin.box);
				}

				if(this.m_normalBackground == null)
				{
					this.m_normalBackground = GUI.skin.box.normal.background;
				}

				if(this.m_lockBackground == null)
				{
					this.m_lockBackground = new Texture2D(60, 60);
					for(int _i = 0; _i<60; ++_i)
					{
						for(int _j = 0; _j<60; ++_j)
						{
							this.m_lockBackground.SetPixel(_i, _j, Color.red);
						}
					}
					this.m_lockBackground.Apply();
				}


				this.m_selectedTabIndex = GUILayout.Toolbar(this.m_selectedTabIndex, new string[] {
					"Streaming",
					"Tweet"
				});

				switch(this.m_selectedTabIndex)
				{
				case 0:
					this.selectedStreaming();
					break;
				case 1:
					this.selectedTweet();
					break;
				}

			}
		}

		private void selectedTweet()
		{
			Input.imeCompositionMode = IMECompositionMode.On;

			this.m_styleBoxMessage.richText = false;
			this.m_styleBoxMessage.fontSize = 13;
			this.m_styleBoxMessage.normal.textColor = Color.white;
			this.m_tweetMessage = GUILayout.TextArea(this.m_tweetMessage, this.m_styleBoxMessage, GUILayout.Height(120), GUILayout.ExpandWidth(true));
			this.m_styleBoxMessage.richText = true;
			this.m_styleBoxMessage.fontSize = 11;
			this.m_styleBoxMessage.normal.textColor = Color.gray;

			//
			GUIStyle _style = new GUIStyle();
			_style.richText = true;
			_style.alignment = TextAnchor.MiddleRight;
			_style.margin.right = 10;
			int _messageLength = 140 - this.m_tweetMessage.Length;
			string _messageLengthStr = "<size=12><color=white>" + _messageLength.ToString() + "</color></size>";
			GUILayout.Label(_messageLengthStr, _style);


			if(this.m_screenShotBytes != null)
			{
				if(this.m_screenShotBytes.Length > 0)
				{
					this.m_screenShot = new Texture2D(0, 0);
					this.m_screenShot.LoadImage(this.m_screenShotBytes);
					//
					int _width = Mathf.RoundToInt((float)this.m_screenShot.width * 0.1f);
					int _height = Mathf.RoundToInt((float)this.m_screenShot.height * 0.1f);
					GUILayout.Button(this.m_screenShot, this.m_styleBoxProfileImage, GUILayout.Width(_width), GUILayout.Height(_height));
				}
			}

			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("Tweet", GUILayout.MinWidth(120), GUILayout.ExpandWidth(true)))
			{
				if(this.m_screenShotBytes == null)
				{
					UnieetMain.Tweet(this.m_tweetMessage);
				}
				else
				{
					UnieetMain.Tweet(this.m_tweetMessage, this.m_screenShotBytes, this.m_screenShot.width, this.m_screenShot.height);
				}
				this.m_tweetMessage = "";
				this.m_screenShotBytes = null;
			}
			
			if(GUILayout.Button("Add Image", GUILayout.MinWidth(120), GUILayout.ExpandWidth(true)))
			{

			}

			if(GUILayout.Button("Take a Screenshot", GUILayout.MinWidth(120), GUILayout.ExpandWidth(true)))
			{
				UnieetMain.TakeScreenshot();
				this.m_flagTakeScreenShot = true;
			}


			EditorGUILayout.EndHorizontal();
		}

		private void selectedStreaming()
		{
			this.m_scrollPos = EditorGUILayout.BeginScrollView(this.m_scrollPos, GUILayout.Width(this.position.width), GUILayout.Height(this.position.height));
			
			for(int _t = 0; _t < UnieetWindow.S_Tweet.Count; ++_t)
			{
				UnieetTweet tweet = UnieetWindow.S_Tweet[_t];
				
				if(!string.IsNullOrEmpty(tweet.profile_image_url))
				{
					if(!this.m_profileImages.ContainsKey(tweet.profile_image_url))
					{
						if(tweet.profile_image_buf != null)
						{
							Texture2D _image = new Texture2D(0, 0);
							_image.LoadImage(tweet.profile_image_buf);
							this.m_profileImages.Add(tweet.profile_image_url, _image);
						}
					}
				}
				
				if(!string.IsNullOrEmpty(tweet.retweet_profile_image_url))
				{
					if(!this.m_profileImages.ContainsKey(tweet.retweet_profile_image_url))
					{
						if(tweet.retweet_profile_image_buf != null)
						{
							Texture2D _image = new Texture2D(0, 0);
							_image.LoadImage(tweet.retweet_profile_image_buf);
							this.m_profileImages.Add(tweet.retweet_profile_image_url, _image);
						}
					}
				}
				
				EditorGUILayout.BeginHorizontal();
				
				if(tweet.reTweet)
				{
					if(!string.IsNullOrEmpty(tweet.retweet_profile_image_url))
					{
						if(this.m_profileImages.ContainsKey(tweet.retweet_profile_image_url))
						{
							if(tweet.retweet_userProtected)
							{
								this.m_styleBoxProfileImage.normal.background = this.m_lockBackground;
								GUILayout.Button(this.m_profileImages[tweet.retweet_profile_image_url], this.m_styleBoxProfileImage, GUILayout.Width(48), GUILayout.Height(48));
								this.m_styleBoxProfileImage.normal.background = this.m_normalBackground;
							}
							else
							{
								GUILayout.Button(this.m_profileImages[tweet.retweet_profile_image_url], this.m_styleBoxProfileImage, GUILayout.Width(48), GUILayout.Height(48));
							}
						}
					}
					
					if(!string.IsNullOrEmpty(tweet.profile_image_url))
					{
						if(this.m_profileImages.ContainsKey(tweet.profile_image_url))
						{
							if(tweet.userProtected)
							{
								this.m_styleBoxProfileImage.normal.background = this.m_lockBackground;
								GUILayout.Button(this.m_profileImages[tweet.profile_image_url], this.m_styleBoxProfileImage, GUILayout.Width(32), GUILayout.Height(32));
								this.m_styleBoxProfileImage.normal.background = this.m_normalBackground;
							}
							else
							{
								GUILayout.Button(this.m_profileImages[tweet.profile_image_url], this.m_styleBoxProfileImage, GUILayout.Width(32), GUILayout.Height(32));
							}
							
							this.m_styleBoxProfileImage.contentOffset = Vector2.zero;
						}
					}
				}
				else
				{
					if(!string.IsNullOrEmpty(tweet.profile_image_url))
					{
						if(this.m_profileImages.ContainsKey(tweet.profile_image_url))
						{
							if(tweet.userProtected)
							{
								this.m_styleBoxProfileImage.normal.background = this.m_lockBackground;
								GUILayout.Button(this.m_profileImages[tweet.profile_image_url], this.m_styleBoxProfileImage, GUILayout.Width(48), GUILayout.Height(48));
								this.m_styleBoxProfileImage.normal.background = this.m_normalBackground;
							}
							else
							{
								GUILayout.Button(this.m_profileImages[tweet.profile_image_url], this.m_styleBoxProfileImage, GUILayout.Width(48), GUILayout.Height(48));
							}
						}
					}
				}
				
               
				string _desc = "";
				string _message = "";
				DateTime _date = DateTime.FromBinary(long.Parse(tweet.created_at));
				string _dateStr = _date.ToString("yyyy/MM/dd HH:mm:ss");
				if(!tweet.reTweet)
				{
                    
					_desc = /*tweet.dataCount +*/ "<b><color=white>@" + tweet.screen_name + "</color></b> " + tweet.name + "\n";
					_desc += "<size=9>" + _dateStr + "</size>\n\n";
					_message = "<color=white>" + tweet.text + "</color>";
				}
				else
				{
					_desc = /*tweet.dataCount +*/ "<b><color=white>@" + tweet.retweet_screen_name + "</color></b> " + tweet.retweet_name + "\n";
					_desc += "<size=9>" + _dateStr + "</size>\n";
                    
					_desc += "<size=9>retweet: <color=white>" + tweet.retweet_count + "</color>";
                    
					if(tweet.retweet_favorite_count > 0)
					{
						_desc += " fav: <color=white>" + tweet.retweet_favorite_count + "</color>";
					}
                    
					_desc += "</size>\n\n";
                    
					_message = "<color=white>" + tweet.retweet_text + "</color>";
				}
                
				int _lineCount = (_desc + _message).ToList().Where(c => c.Equals('\n')).Count() + 1;
                
                
				float _width = this.position.width - 80;
                
				if(tweet.reTweet)
				{
					_width -= 36;
				}
                
				GUILayout.TextArea((_desc + _message), this.m_styleBoxMessage, GUILayout.ExpandWidth(true));
                
				EditorGUILayout.EndHorizontal();
			}
            
			EditorGUILayout.EndScrollView();
		}
        
		void Update()
		{
			if(this.m_flagTakeScreenShot)
			{
				string _parentDir = Directory.GetParent(Application.dataPath).ToString();
				string _path = _parentDir + "/unieet_screenshot.png";
				if(File.Exists(_path))
				{
					this.m_screenShotBytes = UnieetMain.SkimScreenshot();
					if(this.m_screenShotBytes != null)
					{
						this.m_flagTakeScreenShot = false;
					}
				}
			}
            
            
			if(UnieetWindow.S_Tweet != null)
			{
				if(this.m_lastTweetCount != UnieetWindow.S_Tweet.Count)
				{
					this.Repaint();
					this.m_lastTweetCount = UnieetWindow.S_Tweet.Count;
					Unieet.UnieetMain.SaveTweets();
				}
			}

			if(UnieetWindow.S_FlagClose)
			{
				this.Close();
			}
		}
		
		void OnDestroy()
		{
			foreach(Texture _texture in  this.m_profileImages.Values)
			{
				GameObject.DestroyImmediate(_texture);
			}

			GameObject.DestroyImmediate(this.m_lockBackground);
			this.m_lockBackground = null;

			//GameObject.DestroyImmediate(this.m_normalBackground);
			this.m_normalBackground = null;

			if(this.m_screenShot != null)
			{
				GameObject.DestroyImmediate(this.m_screenShot);
				this.m_screenShot = null;

			}

			this.m_screenShotBytes = null;

			UnieetCore.Instance.Dispose();
		}
	}
}
