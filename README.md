# dse-parse-unity
DSE Parse Unity sample project with extendable UI. 

## Setup

I am assuming you already have a Facebook Application with iOS, Android and Canvas configured, as well as a Parse application with your Facebook Application credentials set up.

1. In Unity, configure the Facebook Settings with your App Name, and your App Id.
2. Open `Scene`, and on the Hierarchy Viewer, select `Parse Initializer`. Set your Parse Application ID and Dotnet Key
3. Save, run on the Editor, export to Android, iOS or Web and test away!

## Extending the UI

Open `ParseTestBehavior.cs` and look for `void OnGUI()`. This is the default method Unity uses to Draw it's basic UI. There are three sections on this project's UI: `Login`, `Parse` and `Javascript`. You will mostly work with `Login` and `Parse`. Each of these has a special method where their UI is encapsulated, these are called `RenderLogin()` and `RenderParse`. Go and take a look.

Let's assume you want to add a New Awesome Sample under `RenderParse`. At the end of this method, you could add the following:

```csharp
private void RenderParse() {
    GUILayout.BeginHorizontal();
    if (Button ("My New Awesome Sample")) {
        MyNewAwesomeSample();
    }
    GUILayout.EndHoriziontal();
}
```

Then, you would have to create a new method called `MyNewAwesomeSample`:

```csharp
private void MyNewAwesomeSample() {
    Debug.Log("Hello World!");
}
```

Run. You should see your new Button created on the UI, and when clicking / tapping on it, you will get a Log saying `Hello World!`
