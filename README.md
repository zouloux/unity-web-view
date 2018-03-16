# Unity WebView

Allow making hybrid Unity apps, based on web technologies.
Windows, MacOS, Android and iOS compatible.
With a communication gateway between Unity and Javascript.


# Usage

This will allow a transparent WebView to be on top of Unity graphics, and will allow communication between Javascript and Unity C-sharp.
Performances are really good on iOS and Android.


# The WebView

The WebView part is from this [package](https://github.com/gree/unity-webview)
To enable WebView capabilites, [install this unity package](https://github.com/gree/unity-webview/blob/master/dist/unity-webview.unitypackage?raw=true)


# The WebView installer

To be able to use web view properly, html and assets have to be copied to the device.
This is done with the help of `WebViewComponent.cs`.

1. Copy `WebViewComponent.cs` into `Assets/App/Scripts/`
2. Create a new GameObject into your Unity scene.
3. Select WebViewComponent as a behavior.
4. Enter assets to be copied on the device. Keep a flat file system (no folders)

Files will be copied from `Assets/StreamingAssets/webview/`.


# The Gateway

The communication Gateway is a javascript file to load from the WebView. This file is available as an [ES6 javascript module](https://github.com/zouloux/unity-web-view/blob/master/UnityGateway.js) and is also available if you use [Solidify lib](https://github.com/solid-js/solidify).


### Communicate from Javascript to Unity

#### In Unity

Edit directly method `messageFromWebView` into `WebViewComponent.cs` (at line 117) to implement your actions into Unity.

For ex :
```
if (pParameters[0] == @"myAction")
{
	gameObject.doThis( pParameters[1], pParameters[2] );
}
```

You can also respond to javascript by a return
```
if (pParameters[0] == @"myAction")
{
	return @"{json: 'yes !'}";
}
```

Note that you can send complexe value thanks to JSON.


##### In javascript

1. Init your gateway `UnityGateway.initGateway()` an handler is also available because this method is async.
2. Call your action UnityGateway.callUnity('myAction', 'a string parameters', 5)

*Important* : Javascript is sending commands to Unity through URL parameters. So this is not advice to send Huge data or files. Also, there is no JSON encoding when message are going this way. Only string are received into Unity, you have to parse them manually.


### Communicate from Unity to Javascript

Call this method from Unity to send a message to the WebView.
- `WebViewComponent.sendMessageToWebView(@"{message: 'hello from unity', value: 5}")`
This is also JSON here.

Unity is sending commands to Javascript by injection, so there is nearly no technical limit about size or types.
















