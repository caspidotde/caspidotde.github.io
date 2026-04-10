<!-- Gupta Team Developer -->

##### Einfaches Beispiel

````

Function: GetAJokeFromOpenrouter
	Description: Get a Joke from Openrouter API
	Returns
		String:
	Parameters
	Static Variables
	Local variables
		String: strOpenrouterUrl
		String: strOpenrouterAPIKey
		String: strResult
		String: strError
		String: arrHeaders[2,2]
		String: arrParams[1,2]
		fcOpenrouterChatCompletionRequest: aRequest
		fcOpenrouterChatCompletionResponse: aResponse
	Actions
		!
		Set strOpenrouterUrl = "https://openrouter.ai/api/v1/chat/completions"
		Set strOpenrouterAPIKey = "YOUR_API_KEY"
		!
		Set aRequest.model = "openrouter/free"
		Set aRequest.messages[0].role = "user"
		Set aRequest.messages[0].content = "Please tell me another joke."
		!
		Set arrHeaders[0,0] = "Content-Type"
		Set arrHeaders[0,1] = "application/json"
		Set arrHeaders[1,0] = "Authorization"
		Set arrHeaders[1,1] = "Bearer " || strOpenrouterAPIKey
		!
		Set arrParams[0,0] = ""
		Set arrParams[0,1] = ""
		!
		If SalJSONSerializeUDVEx( "aRequest", strOpenrouterUrl, HTTP_POST, strResult, "", "", arrHeaders, arrParams, strError )
			If SalJSONDeserializeUDV( "aResponse", strResult, "", "", strError )
				Return aResponse.choices[0].message.content
		!


````

![Beispielbild](~/images/Screenshot_Gupta_GetAJoke.png)
