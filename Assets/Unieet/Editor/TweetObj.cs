using UnityEngine;
using System;
using System.Collections.Generic;

namespace Unieet
{
	[Serializable]
	public class TweetObj : ScriptableObject
	{
		public List<Unieet.UnieetTweet> LastTweets;
	}
}