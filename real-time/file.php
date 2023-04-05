<?php

// Make sure to include a WebSocket client library like Textalk/websocket-php by running `composer require textalk/websocket`.
require 'vendor/autoload.php'; 

use WebSocket\Client;

$authHeader = array(
    "Authorization: {your_api_token}"
);

$sampleRate = 16000;
$wordBoost = array("HackerNews", "Twitter");
$wordBoostParam = urlencode(json_encode($wordBoost));
$url = "wss://api.assemblyai.com/v2/realtime/ws?word_boost={$wordBoostParam}&sample_rate={$sampleRate}";
$socket = new Client($url, array('headers' => $authHeader));

$socket->on('message', function ($data) {
    $message = json_decode($data, true);
    if ($message['message_type'] === 'SessionBegins') {
        $session_id = $message['session_id'];
        $expires_at = $message['expires_at'];
        echo "Session ID: $session_id\n";
        echo "Expires at: $expires_at\n";
    } elseif ($message["message_type"] === "PartialTranscript") {
        echo "Partial transcript received: " . $message["text"] . "\n";
    } elseif ($message['message_type'] === 'FinalTranscript') {
        echo "Final transcript received: " . $message["text"] . "\n";
    }
});

$socket->on('error', function ($error) {
    $errorObject = json_decode($error, true);
    if ($errorObject['error_code'] === 4001) {
        echo "Not Authorized";
    }
});

$socket->on('close', function () {
    echo "WebSocket closed";
});

function send_audio($socket, $audio_data) {
    $payload = array(
        "audio_data" => base64_encode($audio_data)
    );
    $socket->send(json_encode($payload));
}

function terminateSession($socket) {
    $payload = array("terminate_session" => true);
    $message = json_encode($payload);
    $socket->send($message);
    $socket->close();
}
