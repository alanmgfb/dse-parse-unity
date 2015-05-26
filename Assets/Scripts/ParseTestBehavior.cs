using Facebook.MiniJSON;
using Facebook;
using Parse;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ParseTestBehavior : MonoBehaviour {

	int buttonHeight = 30;
	public Vector2 statusScrollPosition;
	public Vector2 scrollPosition;

	private string[] sections = new string[]{"Facebook & Parse Login", "Parse", "Javascript Console"};
	enum Toolbars {
		Login,
		Parse,
		Javascript
	};
	private static int selectedToolbar = 0;
	private Toolbars currentToolbar {
		get {
			return (Toolbars) selectedToolbar;
		}
	}

	private string[] snippets = new string[]{"Alert", "Add links to Canvas", "Post to Friends Wall"};
	private string[] jsSnippets = new string[] {
		"alert('Hello World!');",
		"function resizeUnity(){var unityEmbed=document.getElementsByTagName('embed')[0];if(unityEmbed){unityEmbed.style.display='block';unityEmbed.style.width='100%';unityEmbed.style.height='500px'}}function createLink(name,href,parentDiv){var links=document.getElementsByTagName('a');for(var i=0;i<links.length;i++){var currentLink=links[i];if(currentLink.href==href){return}}var link=document.createElement('a');link.href=href;link.target='_blank';link.innerHTML=name;link.style.color='#FF0000';link.style.fontFamily='Arial';link.style.fontSize='30px';parentDiv.appendChild(link)}function appendLinks(){var unityGameContainer=document.getElementsByClassName('content')[0];var divPresent=document.getElementById('footerContent');var linksDiv=null;if(!divPresent){linksDiv=document.createElement('div');linksDiv.id='footerContent';linksDiv.style.textAlign='center';unityGameContainer.appendChild(linksDiv)}else{linksDiv=divPresent}createLink(' Facebook ∙','https://www.facebook.com/',linksDiv);createLink(' Facebook Help ∙','https://www.facebook.com/help',linksDiv)}resizeUnity();appendLinks();",
		"var obj = {method: 'feed',to: 'friend_id',link: 'http://www.facebook.com/thepcwizardblog',picture: 'http://fbrell.com/f8.jpg',name: 'Feed Dialog',caption: 'Tagging Friends',description: 'Using Dialogs for posting to friends timeline.'};function callback(response) {console.log(response);alert('check your console bro');}FB.ui(obj, callback);"
	};
	enum Snippets {
		Alert,
		InjectLinksToDom
	};
	private int selectedGrid = 0;
	private Snippets currentSnippet {
		get {
			return (Snippets) selectedGrid;
		}
	}

	private string userName = "alanmgUnity";
	private string password = "n3verhardcode";
	private string fbPerms = "public_profile,user_friends,publish_actions";

	private static StringBuilder statusBuilder = new StringBuilder();
	private static string status = "Status: Waiting for User!";
	private static string textArea = "Text Area Text";
	private string javascriptCode = "alert('hello world');";

	private bool canLogIn 			= false;
	private bool isFacebookLogged	= false;
	private bool isParseLogged		= false;
	private bool isLinkingDone		= false;
	private bool isScoreLoaded		= false;
	private bool isStressTesting	= false;

	private ParseUser myUser;
	private Score score;
	private string otherUserId = null;

	private DateTime comparisonDate = DateTime.Now;

	private string inappUrl = "http://alanmg.dse.io/unity/opengraph/coin.html";
	private string paymentAction = "create_subscription";

	void Awake() {
#if UNITY_IOS
		NotificationServices.RegisterForRemoteNotificationTypes(RemoteNotificationType.Alert |
		                                                        RemoteNotificationType.Badge |
		                                                        RemoteNotificationType.Sound);
#endif
	}

	void Start() {
#if UNITY_ANDROID || UNITY_IPHONE
		buttonHeight = 70;
#endif
		FB.Init(FbInitDelegate, OnHideUnityDelegate, null);

		statusBuilder.AppendLine(status);
	}

	private void ParseInit() {
		StartCoroutine("CoroutineRetrieveParseUser");
	}

	private IEnumerator CoroutineRetrieveParseUser() {

		if (ParseUser.CurrentUser != null) {
			myUser = ParseUser.CurrentUser;
			Status ("Parse User Loaded " + myUser.ObjectId);
			isParseLogged = true;
		} else {
			myUser = new ParseUser() {
				Username = userName + SystemInfo.deviceUniqueIdentifier,
				Password = password
			};
			
			myUser["randomKey"] = "Local Test";
			Task signupTask = myUser.SignUpAsync();
			while(!signupTask.IsCompleted) yield return null;

			if (signupTask.IsFaulted || signupTask.IsCanceled) {
				ParseLogin();
			} else {
				Status ("Parse Login Successful! " + myUser.ObjectId);
				Debug.Log("Le Name " + myUser["randomKey"]);
				isParseLogged = true;
			}
		}
	}

	//Calling this from a task will make Unity cry
	private void ParseLogin() {

		ParseUser.LogInAsync(SystemInfo.deviceUniqueIdentifier, password).ContinueWith(t => {
			if (t.IsFaulted || t.IsCanceled) {
				Status("Something went wrong when logging into Parse :/");
				foreach(Exception ie in t.Exception.InnerExceptions) {
					ParseException pe = (ParseException)ie;
					if (pe != null) {
						Status(String.Format("Parse Exception code: {0}. Message: {1}", pe.Code, pe.Message));
					}
				}
			} else {
				Status("Parse Login Successful!");
				isParseLogged = true;
				myUser = ParseUser.CurrentUser;
			}
		});
	}

	private void FbInitDelegate() {

		Status ("Facebook Initted");
		canLogIn = true;
	}

	private void OnHideUnityDelegate(bool hidden) {
		Status("On Hide Unity: " + hidden);
	}

	private void FacebookLogin() {
		FB.Login(fbPerms, FacebookLoginCallback);
	}

	private void FacebookLoginCallback(FBResult result) {

		if (result.Error != null) {
			Status(result.Error);
		}
		isFacebookLogged = FB.IsLoggedIn;
		if (isFacebookLogged) {
			Status("Is Facebook Logged in!");
			isFacebookLogged = true;
		}
	}

	private IEnumerator LoginToParseUsingFacebook() {

		Task<ParseUser> loginTask = ParseFacebookUtils.LogInAsync(
			FB.UserId, 
			FB.AccessToken, 
			DateTime.Now
		);
		while (!loginTask.IsCompleted) yield return null;

		if(loginTask.IsFaulted || loginTask.IsCanceled) {
			Status("Something went wrong wile logging to parse through facebook");
			Status (String.Format("Task Status: Faulted {0} Canceled {1}", loginTask.IsFaulted, loginTask.IsCanceled));
			Status(loginTask.Exception.Message);
			Status(loginTask.Exception.StackTrace);

			foreach (ParseException pex in loginTask.Exception.InnerExceptions) {
				Status ("= = = = =");
				Status ("Message: " + pex.Message);
				Status ("Error Code: " + pex.Code);
				Status ("Error Data: " + pex.Data.Count);

				foreach(KeyValuePair<string, string> dataPair in pex.Data) {
					Status("Key: " + dataPair.Key + " Value: " + dataPair.Value);
				}
				Status ("Stack Trace: " + pex.StackTrace);
			}
		} else {
			Status("Logged into Parse with Facebook Credentials!");
			myUser = loginTask.Result;
			myUser["randomKey"] = "LeFacebookTest";
			myUser.SaveAsync();

			isParseLogged = true;
			isLinkingDone = true;
		}
	}

	private IEnumerator LinkParseFacebookUsers() {

		if(ParseFacebookUtils.IsLinked(myUser)) {
			Status("Parse Facebook was already linked!");
			isLinkingDone = true;
			return true;
		}

		Task linkTask = ParseFacebookUtils.LinkAsync(
			myUser,
			FB.UserId,
			FB.AccessToken,
			FB.AccessTokenExpiresAt
		);
		while(!linkTask.IsCompleted) yield return null;

		if(linkTask.IsFaulted || linkTask.IsCanceled) {
			string exceptions = "Something went wrong while Linking:";
			using (IEnumerator<System.Exception> enumerator = linkTask.Exception.InnerExceptions.GetEnumerator()) {
				if (enumerator.MoveNext()) {
					ParseException error = (ParseException) enumerator.Current;
					exceptions += error.Message + "\n";
					// error.Message will contain an error message
					// error.Code will return "OtherCause"
				}
			}
			Status(exceptions);
		} else {
			Status("Parse Facebook linking done!");
			isLinkingDone = true;
		}
	}

	private IEnumerator FetchLatestUser() {
		Status ("Old Key: " + ParseUser.CurrentUser["randomKey"]);

		Task<ParseUser> userTask = ParseUser.CurrentUser.FetchAsync();
		while(!userTask.IsCompleted) yield return null;

		if(userTask.IsFaulted || userTask.IsCanceled) {
			Status (userTask.Exception.ToString());
		} else {
			Status("New Key: " + ParseUser.CurrentUser["randomKey"]);
		}

	}

	private IEnumerator ModifySaveUser() {
		Status("Modify and Save CurrentUser");
		Status("Old Key " + ParseUser.CurrentUser["randomKey"]);
		ParseUser.CurrentUser["randomKey"] = "inClient " + DateTime.Now.ToString();

		Task saveTask = ParseUser.CurrentUser.SaveAsync();
		while (!saveTask.IsCompleted) yield return null;

		Status("Modified User Callback!");
		if (saveTask.IsFaulted || saveTask.IsCanceled) {
			Status(saveTask.Exception.ToString());
		} else {
			Status("Saved Key = " + ParseUser.CurrentUser["randomKey"]);
		}
	}

	private void LoadParseData() {
		score = new Score(1001, myUser, new Score.RetrievalDelegate(GotScore));
	}

	private void GotScore(bool success) {
		isScoreLoaded = true;
	}

	private int saveCounter = 0;
	private void CreateDummyScores() {
		saveCounter = UnityEngine.Random.Range(1, 10);
		for(int i = -5; i < 5; i++) {
			StartCoroutine(SaveScore(saveCounter + i, DateTime.Now.AddDays(i)));
		}
	}

	private IEnumerator SaveScore(int score, DateTime comparisonDate) {
		ParseObject tempScore = new ParseObject("Score");
		tempScore["score"] = score;
		tempScore["comparisonDate"] = comparisonDate;

		Task saveTask = tempScore.SaveAsync();
		while (!saveTask.IsCompleted) yield return null;

		if(saveTask.IsFaulted || saveTask.IsCanceled) {
			Status("Saving " + score + " " + comparisonDate.ToString() + " failed!");
		} else {
			Status(score + " saved!");
		}
	}

	private void ParseQueryScores(bool greater = true) {
		Status("Parsing Query Scores");
		var scoresQuery = ParseObject.GetQuery("Score");

		if (greater) 
			scoresQuery = scoresQuery.WhereGreaterThan("comparisonDate", comparisonDate);
		else scoresQuery = scoresQuery.WhereLessThan("comparisonDate", comparisonDate);

		ExecuteScoresQuery(scoresQuery);
	}
	
	private void QueryScores() {
		Status("Querying ALL Scores");
		var scoresQuery = ParseObject.GetQuery("Score")
			.OrderByDescending("score");
		ExecuteScoresQuery(scoresQuery);
	}

	private void ExecuteScoresQuery(ParseQuery<ParseObject> scoresQuery) {
		Status("ExecuteScoresQuery");
		scoresQuery.FindAsync().ContinueWith(t => {
			if(t.IsCanceled || t.IsFaulted) {
				Status("Query Execution Failed:");
				Status(t.Exception.Message);
				return;
			}
			
			StringBuilder scoresBuilder = new StringBuilder();
			IEnumerable<ParseObject> scores = t.Result;
			foreach(var score in scores) {
				scoresBuilder.AppendLine(String.Format("Score Found {0}", score["score"]));
			}
			Status(scoresBuilder.ToString());
		});
	}

	private void TestAction(bool testBool, string testMsg) {
		Debug.Log("TestAction!: " + testMsg + " result " + testBool);
	}

	private void PostToWall(string to) {
		Status("Posting to wall");
		string userId = to;
		string link = "https://itunes.apple.com/nz/app/best-fiends/id868013618?mt=8";
		string linkName = "Best Fiends";
		string message = "Descriptive message";
		string title = "Share Title";

		FB.Feed(userId, 
		        link,
		        linkName,
		        title,
		        message,
		        "",
		        "",
		        "",
		        "",
		        "",
		        null,
		        (FBResult result) => {
			Status("Post to wall finished!");
				}
		);
	}

	private void SendFriendRequest() {
		Status("Sending friend request...");

		FB.AppRequest("Request Message!", new String[]{otherUserId},null,null,10,"","Request Title!", FriendRequestCallback);
	}

	private void FriendRequestCallback(FBResult response) {
		Status(response.Text);
	}

	private void GetOtherUsersInfo() {
		if(otherUserId == null) {
			Status ("Retrieving list of friends...");
			FB.API("me/friends", Facebook.HttpMethod.GET, OnFriendsRetrieved);
			return;
		}

		string apiCall = string.Format("{0}?fields=picture,first_name", otherUserId);
		Status("Getting Other Users Info " + apiCall);
		FB.API(apiCall,
		       Facebook.HttpMethod.GET,
		       OnOtherUserDataRetrieved);
	}

	private void OnFriendsRetrieved(FBResult result) {
		Status ("Friends Retrieved!");

		if(result.Error != null) {
			Status(result.Error);
			return;
		}

		var dict = Json.Deserialize(result.Text) as Dictionary<string,object>;
		object friendsH;
		var friends = new List<object>();
		friends = (List<object>)(dict["data"]);
		if(friends.Count > 0) {
			//Getting first friend info
			var friendDict = ((Dictionary<string,object>)(friends[0]));
			var friend = new Dictionary<string, string>();
			friend["id"] = (string)friendDict["id"];
			Status (String.Format("Other friend ID: {0}", friend["id"]));

			otherUserId = friend["id"];
			GetOtherUsersInfo();
		}
	}	

	private void OnOtherUserDataRetrieved(FBResult result) {
		Status ("OnOtherUserDataRetrieved!");

		if (result.Error != null) {
			Status(result.Error);
			return;
		}

		Status(result.Text);
	}

	private void OnCreateSubscription(FBResult result) {
		StringBuilder subscriptionBuilder = new StringBuilder("OnCreateSubscription callback! " + result.Error);
		subscriptionBuilder.AppendLine("On Create Subscription!");
		subscriptionBuilder.AppendLine(result.ToString());
		Status(subscriptionBuilder.ToString());
	}

	private IEnumerator QueryObjectWithArray() {
		bool responseReceived = false;
		var q = ParseObject.GetQuery("XamarinArray");
		ParseObject xamarinResult = null;
		q.FirstAsync().ContinueWith(t => {
			if(!t.IsFaulted) {
				xamarinResult = t.Result;
			} else {
				Status("Could not find XamarinArray!");
			}

			responseReceived = true;
		});

		while(!responseReceived) yield return null;

		if(xamarinResult == null) Status("Something went wrong while retrieving object");
		else {
			responseReceived = false;

			Status("XamarinArray Received " + xamarinResult.CreatedAt + " " + xamarinResult.UpdatedAt);
			List<object> list = (List<object>)xamarinResult["list"];
			List<ParseObject>poList = list.ConvertAll<ParseObject>( po => (ParseObject)po);
			Status(poList);

			List<ParseObject> fetchedList = null;
			ParseObject.FetchAllIfNeededAsync(poList).ContinueWith( t => {
				if(t.IsFaulted) {
					Status("Algo salio mal al refrescar los items");
					Debug.LogException(t.Exception);
				} else {
					Debug.Log(t.Result);
					fetchedList = (List<ParseObject>)t.Result;
				}
				responseReceived = true;
			});

			while (!responseReceived) yield return null;
			if(fetchedList == null) Status("Something went wrong while retrieving list");
			else {
				Status(list.ConvertAll<ParseObject>( po => (ParseObject)po));
			}
		}
	}

	private IEnumerator QueryObjectWithDict() {
		Status("Query Object With Dictionary!");

		var query = ParseObject.GetQuery("DictionaryFieldTest");
		var queryTask = query.FirstAsync();

		while (!queryTask.IsCompleted) yield return null;

		if(queryTask.IsFaulted || queryTask.IsCanceled) {
			Status("Error querying object with dictionary: " + queryTask.Exception.Message);
		} else {
			Status("Querying object with Dictionary callback!");
			ParseObject objectWithDict = queryTask.Result;
			IDictionary<string, object> dict = objectWithDict.Get<Dictionary<string, object>>("dictionary");
			Status(String.Format("Stored value on {0} Dictionary's key 'test': {1}", objectWithDict.ObjectId, dict["test"]));
		}
	}

	private void GetDeepLink() {
		Status ("Getting Deep Link...");
		FB.GetDeepLink(DeepLinkCallback);
	}

	private void DeepLinkCallback(FBResult result) {
		Status ("Got Deep Link! Es: " + result.Text);
	} 

	private void LogRandomEvent() {
		Status("Logging random event!");
		FB.AppEvents.LogEvent("random_event", 1.0f);
		Status("Random event logged");
	}

	private void LogPurchase() {
		Status ("Logging purchase!");
		FB.AppEvents.LogPurchase(1.99f);
		Status("Purchase logged");
	}

	private void MePermissions() {
		Status("Requestion for Permissions!");
		FB.API("me/permissions", Facebook.HttpMethod.GET, MePermissionsCallback);
	}

	private void MePermissionsCallback(FBResult result) {
		Status(result.Text);
	}
	
	private void ClearData() {

		ParseUser.LogOut();
		FB.Logout();
		PlayerPrefs.DeleteAll();

		isFacebookLogged = false;
		isParseLogged = false;
		isLinkingDone = false;
		isScoreLoaded = false;
		isStressTesting	= false;

		Status("All data cleared!");
	}

	private void Snippet(Snippets snippet) {
		javascriptCode = jsSnippets[(int)snippet];
	}

	void OnGUI() {

		//Main Layout
		GUILayout.BeginVertical();
			GUILayout.Box(
				"Facebook Parse Test App", 
		        GUILayout.MinWidth(Screen.width)
			);

		selectedToolbar = GUILayout.Toolbar(selectedToolbar, sections, GUILayout.MinWidth(Screen.width));
		switch(currentToolbar) {
		case Toolbars.Login:
			RenderLogin();
			RenderStatus();
			break;

		case Toolbars.Parse:
			RenderParse();
			RenderStatus();
			break;
		case Toolbars.Javascript:
			RenderJavascriptConsole();
			break;

		default:
			Debug.Log("Unknown button clicked");
			break;
		}

		//End Main Layout
		GUILayout.EndVertical();
	}

	private void RenderLogin() {
		//Login Options
		GUILayout.BeginHorizontal();
		if (Button("Login To Facebook", canLogIn & !isFacebookLogged)) {
			FacebookLogin();
		}

		if (Button ("Login to Parse using Facebook", !isParseLogged && isFacebookLogged)) {
			StartCoroutine("LoginToParseUsingFacebook");
		}
		
		if (Button("Login to Parse Only", !isParseLogged)) {
			ParseInit();
		}
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		if (Button(
			"Link Parse and Facebook Users", 
			!isLinkingDone && isParseLogged && isFacebookLogged)
		) {
			StartCoroutine("LinkParseFacebookUsers");
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		if (Button ("Get other user's Info", isFacebookLogged)) {
			GetOtherUsersInfo();
		}

		if (Button ("Share on my Timeline", isFacebookLogged)) {
			PostToWall(FB.UserId);
		}

		if (Button ("Share on friend's Timeline", isFacebookLogged)) {
			PostToWall(otherUserId);
		}

		if (Button ("Send Friend Request", isFacebookLogged && otherUserId != null)) {
			SendFriendRequest();
		}

		if (Button ("FB Canvas Subscription", isFacebookLogged)) {
			FB.Canvas.Pay(inappUrl, paymentAction, 1, callback: OnCreateSubscription);
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		if (Button ("Get Deep Link")) {
			GetDeepLink();
		}

		if (Button ("Log Random Event", isFacebookLogged)) {
			LogRandomEvent();
		}

		if (Button ("Log Purchase", isFacebookLogged)) {
			LogPurchase();
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		if (Button ("me/permissions", isFacebookLogged)) {
			MePermissions();
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		if (Button ("Clear Data")) {
			ClearData();
		}
		if (Button ("Quit Game")) {
			Application.Quit();
		}
		GUILayout.EndHorizontal();		
	}

	private void RenderParse() {
		//Testing data Options
		GUILayout.BeginHorizontal();
		if (Button("Fetch Latest userData", isParseLogged)) {
			StartCoroutine("FetchLatestUser");
		}
		if (Button ("Modify and Save userData", isParseLogged)) {
			StartCoroutine("ModifySaveUser");
		}
		if (Button("Load User Score", isParseLogged && !isScoreLoaded)) {
			LoadParseData();
		}
		if (Button("Create Dummy Scores", isParseLogged)) {
			CreateDummyScores();
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if (Button("ParseQuery GreaterThan Scores", isParseLogged)) {
			ParseQueryScores();
		}
		if (Button("ParseQuery LessThan Scores", isParseLogged)) {
			ParseQueryScores(false);
		}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		if (Button ("Query All Scores", isParseLogged)) {
			QueryScores();
		}
		if (Button ("QueryObjectWithArray")) {
			StartCoroutine("QueryObjectWithArray");
		}
		if (Button ("Query Object with Dict Property")) {
			StartCoroutine("QueryObjectWithDict");
		}
		GUILayout.EndHorizontal();
	}

	private void RenderJavascriptConsole() {
		GUILayout.BeginVertical();

		bool canExecuteJavascript = false;
#if UNITY_WEBPLAYER
		canExecuteJavascript = true;
#endif

		GUILayout.Box(
			"Snippets:", 
			GUILayout.MinWidth(Screen.width)
		);

		selectedGrid = GUILayout.SelectionGrid(selectedGrid, snippets, snippets.Length);
		if(GUI.changed) {
			Snippet(currentSnippet);
		}

		if(Application.platform == RuntimePlatform.Android 
		   || Application.platform == RuntimePlatform.IPhonePlayer) {
			GUI.skin.verticalScrollbarThumb.fixedWidth = Screen.width * 0.05f;
		} else {
			GUI.skin.verticalScrollbar.fixedWidth = Screen.width * 0.03f;
		}
		scrollPosition = 
			GUILayout.BeginScrollView(
				scrollPosition, 
				GUILayout.MinWidth(Screen.width),
				GUILayout.MaxHeight(Screen.height)
			);
		javascriptCode = GUILayout.TextArea(
			javascriptCode, 
			GUILayout.MinWidth(Screen.width),
			GUILayout.MaxHeight(Screen.height));
		GUILayout.EndScrollView();

		if(Button ("Execute Javascript", canExecuteJavascript)) {
#if UNITY_WEBPLAYER
			Application.ExternalEval(javascriptCode);
#endif
		}

		GUILayout.EndVertical();
	}

	private void RenderStatus() {
		statusScrollPosition =
			GUILayout.BeginScrollView(
				statusScrollPosition,
				GUILayout.MinWidth(Screen.width),
				GUILayout.MaxHeight(Screen.height)
			);
		status = GUILayout.TextArea(status, GUILayout.MinWidth(Screen.width), GUILayout.MaxHeight(Screen.height));
		GUILayout.EndScrollView();

		GUILayout.BeginHorizontal();
		if (Button ("Clear Console")) {
			statusBuilder.Remove(0, statusBuilder.Length);
			status = statusBuilder.ToString();
		}
		GUILayout.EndHorizontal();
	}

	private bool Button(string label, bool shouldEnable = true) {
		GUI.enabled = shouldEnable;

		bool buttonResult = GUILayout.Button(
			label, 
			GUILayout.MinHeight(buttonHeight), 
			GUILayout.MaxWidth(Screen.width)
			);
		GUI.enabled = true;

		return buttonResult;
	}

	public static void Status(string message) {

		statusBuilder.AppendLine(message);
		status = statusBuilder.ToString();
	}

	private void Status(List<ParseObject>list) {
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("List has " + list.Count + " elements");
		list.ForEach(po => {
			sb.AppendLine(po.CreatedAt + " : " + po.UpdatedAt + ". " + po.IsDataAvailable);
		});
		sb.AppendLine("End of List");
		
		Status(sb.ToString());
	}
}
