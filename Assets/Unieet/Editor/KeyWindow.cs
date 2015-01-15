using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
//using CoreTweet;

namespace Unieet
{
	public class KeyWindow : EditorWindow
	{
		public static bool S_OPENED = false;
		private string m_ck = "";
		private string m_cs = "";
		
		public static void OpenWindow()
		{
			KeyWindow.S_OPENED = true;
			KeyWindow _window = (KeyWindow)EditorWindow.GetWindow(typeof(KeyWindow));
			_window.position = new Rect(Screen.width / 2, Screen.height / 2, 300, 60);
			_window.title = "KeyWindow";
		}
		
		void OnGUI()
		{
			GUILayout.Label("Consumer Key");
			this.m_ck = GUILayout.TextField(this.m_ck);
			GUILayout.Label("Consumer Secret");
			this.m_cs = GUILayout.TextField(this.m_cs);

			GUILayout.BeginHorizontal();
			
			if (GUILayout.Button("Generate key file"))
			{
				if (this.m_ck.Length + this.m_cs.Length > 0)
				{
					UnieetMain.GenerateKeyFile(this.m_ck, this.m_cs);
					this.Close();
				}
			}
			
			GUILayout.EndHorizontal();

		}

		void OnDestroy()
		{
			KeyWindow.S_OPENED = false;
		}
	}
}
