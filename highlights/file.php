<?php

// Function to upload a local file to the AssemblyAI API
function upload_file($api_token, $path) {
    $url = 'https://api.assemblyai.com/v2/upload';
    $data = file_get_contents($path);

    $options = [
        'http' => [
            'method' => 'POST',
            'header' => "Content-type: application/octet-stream\r\nAuthorization: $api_token",
            'content' => $data
        ]
    ];

    $context = stream_context_create($options);
    $response = file_get_contents($url, false, $context);

    if ($http_response_header[0] == 'HTTP/1.1 200 OK') {
        $json = json_decode($response, true);
        return $json['upload_url'];
    } else {
        echo "Error: " . $http_response_header[0] . " - $response";
        return null;
    }
}

// Function to create a transcript using AssemblyAI API
function create_transcript($api_token, $audio_url) {
    // Set the API endpoint URL
    $url = "https://api.assemblyai.com/v2/transcript";

    // Set the request headers for the API
    $headers = array(
        "authorization: " . $api_token,
        "content-type: application/json"
    );

    // Set the data to be sent in the API request
    $data = array(
        "audio_url" => $audio_url,
        "auto_highlights" => true
    );

    // Initialize a cURL session for the API endpoint
    $curl = curl_init($url);

    // Set the options for the cURL session for the API request
    curl_setopt($curl, CURLOPT_POST, true);
    curl_setopt($curl, CURLOPT_POSTFIELDS, json_encode($data));
    curl_setopt($curl, CURLOPT_HTTPHEADER, $headers);
    curl_setopt($curl, CURLOPT_RETURNTRANSFER, true);

    // Execute the cURL session and decode the response
    $response = json_decode(curl_exec($curl), true);

    // Close the cURL session
    curl_close($curl);

    // Get the transcript ID from the API response
    $transcript_id = $response['id'];

    // Set the polling endpoint URL for the API
    $polling_endpoint = "https://api.assemblyai.com/v2/transcript/" . $transcript_id;

    // Loop until the transcription is completed or an error occurs
    while (true) {
        // Initialize a cURL session for the polling endpoint
        $polling_response = curl_init($polling_endpoint);

        // Set the options for the cURL session for the polling request
        curl_setopt($polling_response, CURLOPT_HTTPHEADER, $headers);
        curl_setopt($polling_response, CURLOPT_RETURNTRANSFER, true);

        // Execute the cURL session and decode the polling response
        $transcription_result = json_decode(curl_exec($polling_response), true);

        // Check if the transcription is completed or an error occurred
        if ($transcription_result['status'] === "completed") {
            // Return the completed transcript object
            return $transcription_result;
        } else if ($transcription_result['status'] === "error") {
            // Throw an exception if an error occurred
            throw new Exception("Transcription failed: " . $transcription_result['error']);
        } else {
            // Wait for 3 seconds before polling again
            sleep(3);
        }
    }
}

try {
    $api_token = "YOUR-API-TOKEN";

    $path = "/path/to/foo.wav";
    $upload_url = upload_file($api_token, $path);

    $transcript = create_transcript($api_token, $upload_url);

    // Check if auto_highlights_result has a status of "success"
    if ($transcript['auto_highlights_result']['status'] == 'success') {
        // Output the auto_highlights_result results
        echo "Auto Highlights Results:\n";
        foreach ($transcript['auto_highlights_result']['results'] as $result) {
            echo "Text: " . $result['text'] . "\n";
            echo "Count: " . $result['count'] . "\n";
            echo "Rank: " . $result['rank'] . "\n";
            echo "Timestamps:\n";
            foreach ($result['timestamps'] as $timestamp) {
                echo "  Start: " . $timestamp['start'] . " End: " . $timestamp['end'] . "\n";
            }
            echo "\n";
        }
    } else {
        echo "Auto highlights results not available.\n";
    }    
} catch (Exception $e) {
    echo 'Error: ' . $e->getMessage();
}

?>

