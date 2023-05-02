<?php

// Function that sends a request to the LeMUR API
function lemur_post($api_token, $transcript_id) {
    // Set the API endpoint URL
    $url = "https://api.staging.assemblyai-labs.com/beta/generate/summary";

    // Set the request headers for the API
    $headers = [
        "authorization: " . $api_token,
        "content-type: application/json"
    ];

    // Set the data to be sent in the API request
    $data = [
        "transcript_ids" => [$transcript_id],
        "context" => "this is a sales call",
        "answer_format" => "short summary"
    ];

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

    // Get the LeMUR output from the API response
    $lemur_response = $response['response'];
}

try {
    $api_token = "YOUR-API-TOKEN";
    $transcript_id = "TRANSCRIPT-ID";

    $lemur_output = lemur_post($api_token, $transcript_id);
    echo $lemur_output;
} catch (Exception $e) {
    echo 'Error: ' . $e->getMessage();
}

?>
