<!-- PHP: AI muss sein oder? -->

##### PHP: AI muss sein oder? Dann nutze Slim PHP, Twig View, GuzzleHttp, und einen eigenen Ollama-Server
```php

// home.twig
{% extends "master.twig" %}
{% block content %}
    <p>
    {{witz}}
    </p>
{% endblock %}


// HomeAction.php
final class HomeAction extends BaseAction
{
...
public function __invoke(Request $request, Response $response): Response
    {
        $statusCode = 0;
        $body = "";

        $client = new Client();
        try {
            $responseGuzzleClient = $client->post(
                'http://myollama:11434/api/generate',
                [
                    'json' => ['prompt' => 'Please tell me a joke about three people go in a bar.', 'model' => 'llama3.2', 'stream' => false],
                    'headers' => ['Content-Type' => 'application/json']
                ]);

            $statusCode = $responseGuzzleClient->getStatusCode(); // HTTP status code
            $bodyJson = json_decode($responseGuzzleClient->getBody());
            $body = $bodyJson->response ?? '';
        } catch (\GuzzleHttp\Exception\RequestException $e) {
            $this->logger->info($e->getMessage());
        }

        $viewData = ['witz' => $body];
        return $this->twig->render($response, 'home.twig', $viewData);
}

// or use openrouter as as alternative to ollama

public function GetAJoke(): string
{

	$body = "";
    $model = "openrouter/free";

    $client = new Client();
    try {            
	    $responseGuzzleClient = $client->post(
            'https://openrouter.ai/api/v1/chat/completions',
            [
                'json' => [                        
                    'messages' => [ [ 'role' => 'user', 'content' => 'Please tell me another joke.' ] ],
                    'model' => $model
                ],
                'headers' => [
                    'Content-Type' => 'application/json',
                    'Authorization' => 'Bearer YOUR_OPENROUTER_API_KEY'
                ]
		    ]
	    ); 

        $bodyJson = json_decode($responseGuzzleClient->getBody());
        $body = $bodyJson->choices[0]->message->content ?? '';

    } catch (\GuzzleHttp\Exception\RequestException $e) {
        $this->logger->info($e->getMessage());
    }  

    return $body;
}


```