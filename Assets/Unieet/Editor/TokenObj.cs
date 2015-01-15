using UnityEngine;
using System;

namespace Unieet
{
	[Serializable]
	public class TokenObj : ScriptableObject
	{
		public string Token;
		public string TokenSecret;
	}
}