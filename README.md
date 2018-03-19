# Unity WebView

Allow making hybrid Unity apps, based on web technologies.
Windows, MacOS, Android and iOS compatible.
With a communication gateway between Unity and Javascript.


# Usage

This will allow a transparent WebView to be on top of Unity graphics, and will manage communication between Javascript and Unity runtime.
Performances are pretty good on iOS and Android.


# The WebView

The WebView part is from [this unity package](https://github.com/gree/unity-webview).
To enable WebView capabilites, [download it](https://github.com/gree/unity-webview/blob/master/dist/unity-webview.unitypackage?raw=true) and install it into your Unity project.


# The WebView installer

To be able to use web view properly, html and other assets have to be copied to the device.
This is done automatically by `WebViewComponent.cs`.

### Installation

1. Copy `WebViewComponent.cs` into `Assets/App/Scripts/`
2. Create a new GameObject into your Unity scene.
3. Select `WebViewComponent` as a new behavior.
4. Enter web files to be copied on the device.
5. Keep `index.html` on top so it's automatically loaded on startup.

Web files will be copied from `Assets/StreamingAssets/webview/`.
Keep a flat file system (no folders).


# The Gateway

The communication Gateway is a javascript file to load from the WebView. This file is available as an [ES6 javascript module](https://github.com/zouloux/unity-web-view/blob/master/UnityGateway.js) and is also available if you use [Solidify lib](https://github.com/solid-js/solidify).


### Communicate from Javascript to Unity

#### In Unity

Edit directly method `messageFromWebView` into `WebViewComponent.cs` (at line 120) to implement your actions into Unity.

For ex :
```
if (pParameters[0] == @"myAction")
{
	gameObject.doThis( pParameters[1], pParameters[2] );
	return @"";
}
```

You can also respond to javascript by a return
```
if (pParameters[0] == @"myAction")
{
	return @"{json: 'yes !'}";
}
```

Note that you can send complex values thanks to JSON.

Return `null` if this is an async action. Then call `sendMessageToWebView` to send back answer to the WebView when it's done :
```
if (pParameters[0] == @"takeScreenshot")
{
	StartCoroutine (this.cameraObject.TakePhoto( (value) =>
	{
		this.sendMessageToWebView(
			@"{photo:" + value + "}"
		);
	}));
}
```


##### In javascript

1. Init your gateway `UnityGateway.initGateway()` an handler is also available because this method is async.
2. Call your action with `UnityGateway.callUnity('myAction', 'a string parameters', 5)`

*Important* : Javascript is sending commands to Unity through URL parameters. So this is not advice to send Huge data or files. Also, there is no JSON encoding when message are going this way. Only string are received into Unity, you have to parse them manually.


### Communicate from Unity to Javascript

##### In Unity

Call this method from Unity to send a message to the WebView `WebViewComponent.sendMessageToWebView(@"{message: 'hello from unity', value: 5}")`
This is also JSON here.

##### In Javascript

Listen to messages in javascript, with : 
```
UnityGateway.onMessage = (jsonObject) =>
{
	console.log('Message from Unity !', jsonObject);
};
```

If you are using Solidify lib, `onMessage` is a `Signal` :
```
UnityGateway.onMessage.add(this.myHandler, this);
```

Unity is sending commands to Javascript by injection, so there is nearly no technical limit about size or types.


### isUnity

To know if your webview is running into a Unity environment, check `UnityGateway.isUnity == true` .
If you are not in Unity environment, every `UnityGateway.callUnity` will respond automatically with a parameter `{isUnity: false}`, to avoid blocking.











