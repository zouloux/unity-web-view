// ----------------------------------------------------------------------------- STRUCT

// Fonction ne faisant rien pour éviter de call un null
const _noop = () => {};

/**
 * La passerelle entre Unity et Javascript
 */
export class UnityGateway
{
	// ------------------------------------------------------------------------- INIT
	/**
	 * Initialiser la passerelle entre Unity et Javascript
	 */
	static initGateway (pHandler)
	{
		// Exposer l'API en global pour que Unity puisse y accéder
		window['UnityGateway'] = UnityGateway;

		// La réponse en attente de Unity après le dernier appel
		UnityGateway.__pendingDirectHandler = _noop;

		// Lorsque l'application Unity envoie un message
		UnityGateway.onMessage = _noop;

		// Si on est connecté à Unity, ou si on est en mode simulé
		UnityGateway.isUnity = false;

		// Détecter si on est dans Unity
		// On attend un petit peu
		window.setTimeout(() =>
		{
			UnityGateway.isUnity = !!window['__isUnity'];
			pHandler && pHandler();
		}, 500);
	}

	/**
	 * Faire un appel vers Unity
	 * @param rest
	 * @returns Promise
	 */
	static callUnity (...rest)
	{
		return new Promise( resolve =>
		{
			console.info('UnityGateway.callUnity', rest);

			// Encoder chaque paramètre, dont les & au format URL
			rest = rest.map( param => encodeURIComponent(param) );

			// Ne pas faire l'appel si on est dans un browser, pour éviter l'alert d'erreur
			if ( UnityGateway.isUnity )
			{
				// Créer le handler pour que Unity puisse répondre
				UnityGateway.__pendingDirectHandler = ( ...rest ) =>
				{
					// Exécuter la promesse et passer les paramètres
					resolve({
						...rest,
						isUnity: true
					});

					// Repasser le handler de réponse à noop pour éviter que la promesse reste en mémoire
					UnityGateway.__pendingDirectHandler = _noop;
				};

				// Modifier l'URL avec le scheme unity pour envoyer la commande
				// On met un séparateur & entre chaque paramètre car on sait qu'ils ont été encodé juste avant
				window.location.href = `unity:${ rest.join('&') }`;
			}

			// Si on n'est pas dans Unity
			else
			{
				// Attendre que la promesse soit passé
				window.setTimeout(() =>
				{
					// Appeler la réponse en spécifiant qu'on est pas Unity
					resolve({
						isUnity: false
					});
				}, 100);
			}
		});
	}

	/**
	 * Méthode pour que Unity puisse appeler Javascript.
	 * A écouter sur le handler UnityGateway.onMessage
	 * @param rest
	 */
	static callWebView (...rest)
	{
		console.info('UnityGateway.callWebView', rest);

		// Relayer au signal
		UnityGateway.onMessage( ...rest );
	}
}