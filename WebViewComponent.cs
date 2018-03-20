using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WebViewComponent : MonoBehaviour
{
	// ------------------------------------------------------------------------- PUBLIC PROPERTIES

	// Le sous-dossier où sont stockées les fichiers pour la webview
	public string streamingAssetsSubFolder = @"webview/";

	// La liste des fichiers à charger dans la mémoire permanante du device
	public string[] filesToLoad = new string[]{

		// IMPORTANT : Le fichier HTML en premier
		@"index.html",

		// Les fichiers chargés par l'index
		// ...
	};

	// Si la Webview est transparente
	public bool transparent = true;


	// ------------------------------------------------------------------------- LOCALS

	// La webview principale
	protected WebViewObject webViewObject;


	// ------------------------------------------------------------------------- INIT

	/**
	 * Démarrage du composant
	 */
	void Start ()
	{
    	// Initialiser le composant WebView
		webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
		webViewObject.Init(

			// Message reçu de la part de la web view
			cb: (msg) =>
			{
				Debug.Log(@"Message from WebView : " + msg);

				// Les & contenus dans les paramètres ont été encodés
				// Découper les paramètres avec le &.
				string[] parameters = msg.Split('&');

				// Décoder chaque paramètre
				for (int i = 0; i < parameters.Length; i++)
				{
				    parameters[i] = WWW.UnEscapeURL( parameters[i] );
				}

				// Interprêter le message de la WebView
				string response = messageFromWebView( parameters );

				// Si on a une réponse synchrone, on répond directement
				if (response != null)
				{
					// Retourner le résultat en différé pour laisser respirer
					webViewDirectHandler( response );
				}
			},

			// Erreur
			err: (msg) =>
			{
				Debug.Log(string.Format("CallOnError[{0}]", msg));
			},

			// La page a chargé
			ld: (msg) =>
			{
				Debug.Log(string.Format("CallOnLoaded[{0}]", msg));

				// Signaler qu'on est bien dans Unity
				webViewObject.EvaluateJS(@"window.__isUnity = true;"); 

				// Afficher la webview
				webViewObject.SetVisibility(true);
			},

			// Activer WebKitWebView sur iOS
			enableWKWebView: true,

			// La WebView est transparente
			transparent: transparent
		);

		// Configuration pour le debugger unity
		#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			webViewObject.bitmapRefreshCycle = 1;
		#endif

		// Plein écran et afficher
		webViewObject.SetMargins(0, 0, 0, 0);

		// Préparer les fichiers pour les installer dans l'espace persistent
		PrepareFileForPersistentStorage( () =>
		{
			Debug.Log(@"Loading web view index ... ");

			// Charger le fichier principal dans la WebView
			webViewObject.LoadURL( GetIndexPath() );
		});
	}

    // ------------------------------------------------------------------------- WEB VIEW MESSAGES & COMMUNICATION

    /**
	 * Actions par rapport aux messages de la web view
	 */
    protected string messageFromWebView (string[] pParameters)
	{
		// --- Feed actions here
		// --- Return null if this is an async action, then call webViewDirectHandler
		// --- Return json in string if this is a sync action ( ex: @"{success: true}" )

		if (pParameters[0] == @"")
		{
			// ...
			// return "{result:...}"
		}
		/*
		else if (pParameters[0] == @"otherAction")
		{
			// ...
		}
		*/

		// Message inconnu
		else return "{error: true}";
		
		// Retourn par défaut
		return "";
	}

	/**
	 * Envoyer un message à la webview.
	 * Les paramètres sont à envoyer en string Javascript, comme suit :
	 * "true, 'ok', {object: true}"
	 */
	public void sendMessageToWebView (string pParams)
	{
		webViewObject.EvaluateJS(@"UnityGateway.callWebView(" + pParams + @")");
	}

	/**
	 * Réponse directe à un message de la WebView.
	 * Les paramètres sont à envoyer en string Javascript, comme suit :
	 * "true, 'ok', {object: true}"
	 */
	protected void webViewDirectHandler (string pParams)
	{
		webViewObject.EvaluateJS(@"UnityGateway.__pendingDirectHandler(" + pParams + @")");
	}


	// ------------------------------------------------------------------------- FILES TRANSFER

	/**
	 * Récupérer le chemin du fichier index.html installé sur le device
	 */
	protected string GetIndexPath ()
	{
		string dst = System.IO.Path.Combine(Application.persistentDataPath, filesToLoad[0]);
		return "file://" + dst.Replace(" ", "%20");
	}

	/**
	 * Copier les fichiers sur la mémoire du téléphone pour pouvoir les ouvrir dans la webview.
	 */
	protected void PrepareFileForPersistentStorage (Action callback)
	{
		// Parcourir les fichiers à copier sur la mémoire du téléphone
		foreach (var fileName in this.filesToLoad)
		{
			// Ajouter le sous-dossier au fichier que l'on cible
			string completeFileName = streamingAssetsSubFolder + fileName;

			// Calculer le chemin complet vers le fichier depuis l'app
			string src = System.IO.Path.Combine(Application.streamingAssetsPath, completeFileName);

			// Calculer le chemin complet où il faudra copier le fichier, sur la mémoire de device
			// Ne pas recréer l'architecture avec le sous-dossier
			string dst = System.IO.Path.Combine(Application.persistentDataPath, fileName);

			// Charger le fichier en mémoire et l'écrire sur le device
			// On lance l'appel en synchrone pour éviter d'appeler le callback
			// alors que les fichiers n'ont pas été écrits
			StartCoroutine(
				LoadAndWriteFile(src, dst)
			);
		}

        // FIXME : Soucis de délais ici ? Peut-être que les fichiers ne sont pas encore écrits à cet instant ...

		// Appeler le callback une fois que tout est transféré
		callback();
	}

	/**
	 * Ecrire sur le device un fichier qui est dans les StreamingAssets.
	 * La méthode est asynchrone
	 */
	protected IEnumerator LoadAndWriteFile ( string src, string dst )
	{
		// Lire le fichier
		byte[] result = null;
		if (src.Contains("://"))
		{
			// Pour android
			WWW data = new WWW( src );
			yield return data;
			result = data.bytes;
		}
		else
		{
			// Pour le reste
			result = System.IO.File.ReadAllBytes( src );
		}

		Debug.Log("Writing file ... " + dst);

		// L'enregistrer
		System.IO.File.WriteAllBytes(dst, result);
	}
}