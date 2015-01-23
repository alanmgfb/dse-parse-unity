using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Parse;
using System;

public class Score {

	public delegate void RetrievalDelegate(bool success);

	private ParseUser owner;
	private ParseObject pScore;
	public ParseObject Data {
		get {
			return pScore;
		}
	}

	public int Value {
		get {
			if (pScore == null) {
				Debug.LogError("Score isn't loaded yet. Can't set value!");
			}
			return Convert.ToInt32(pScore["score"]);
		}

		set {
			if(pScore == null) return;
			pScore["score"] = value;
		}
	}

	private RetrievalDelegate retrievalDelegate;

	public Score(int value, ParseUser user, RetrievalDelegate retDel) {
		owner = user;
		retrievalDelegate = retDel;

		ParseQuery<ParseObject> sQuery = ParseObject.GetQuery("Score")
			.WhereEqualTo("parent", owner);

		sQuery.FirstAsync().ContinueWith(t => {

			if(t.IsFaulted || t.IsCanceled) {
				pScore = new ParseObject("Score");
				pScore["score"] = value;
				pScore["parent"] = owner;
				pScore.SaveAsync();
				
				ParseTestBehavior.Status("Creating score on the DB.");
				retrievalDelegate(true);
			} else {
				ParseObject result = t.Result;
				pScore = result;
				if (value != Convert.ToInt32(pScore["score"])) {
					pScore["score"] = value;
					pScore.SaveAsync();
				}

				ParseTestBehavior.Status("Got score from DB.");
				retrievalDelegate(true);
			}
		});
	}

	public void StressTest() {

	}
}
