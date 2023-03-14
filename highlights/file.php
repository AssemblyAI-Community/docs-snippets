<?php

// Function to create a transcript using AssemblyAI API
function create_transcript() {
    // Set the API endpoint URL
    $url = "https://api.assemblyai.com/v2/transcript";

    // Set the request headers for the API
    $headers = array(
        "authorization: {your_api_token}",
        "content-type: application/json"
    );

    // Set the data to be sent in the API request
    $data = array(
        "audio_url" => "https://bit.ly/3yxKEIY",
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

// Call the create_transcript function
try {
    $transcript = create_transcript();
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
