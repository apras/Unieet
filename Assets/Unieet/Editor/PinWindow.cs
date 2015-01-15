using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
//using CoreTweet;

namespace Unieet
{
	public class PinWindow : EditorWindow
	{
		public static bool S_OPENED = false;
		private string m_Pin = "";
		
		public static void OpenWindow()
		{
			PinWindow.S_OPENED = true;
			PinWindow _window = (PinWindow)EditorWindow.GetWindow(typeof(PinWindow));
			_window.position = new Rect(Screen.width / 2, Screen.height / 2, 200, 60);
			_window.title = "Enter Unieet's PIN.";
		}
		
		void OnGUI()
		{
			GUILayout.Label("PIN");
			this.m_Pin = GUILayout.TextArea(this.m_Pin);

			GUILayout.BeginHorizontal();
			
			if (GUILayout.Button("Authorize"))
			{
				if (this.m_Pin.Length > 0)
				{
					UnieetCore.Instance.Authorize(this.m_Pin);
					this.Close();
				}
			}
			
			GUILayout.EndHorizontal();

		}

		void OnDestroy()
		{
			PinWindow.S_OPENED = false;
		}
	}
}
