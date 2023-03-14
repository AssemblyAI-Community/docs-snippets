import axios from 'axios';

// URL of the AssemblyAI transcription API
const url = 'https://api.assemblyai.com/v2/transcript';

// HTTP headers to be sent with the API requests
const headers = {
  authorization: '{your_api_token}',
  'content-type': 'application/json',
};

// Data to be sent with the API request, specifying the audio URL to be transcribed
const data = {
  audio_url: 'https://bit.ly/3yxKEIY',
};

// Async function that sends a request to the AssemblyAI transcription API and retrieves the transcript
async function transcribeAudio() {
  // Send a POST request to the transcription API with the audio URL in the request body
  const response = await axios.post(url, data, { headers });

  // Retrieve the ID of the transcript from the response data
  const transcriptId = response.data.id;

  // Construct the polling endpoint URL using the transcript ID
  const pollingEndpoint = `https://api.assemblyai.com/v2/transcript/${transcriptId}`;

  // Poll the transcription API until the transcript is ready
  while (true) {
    // Send a GET request to the polling endpoint to retrieve the status of the transcript
    const pollingResponse = await axios.get(pollingEndpoint, { headers });

    // Retrieve the transcription result from the response data
    const transcriptionResult = pollingResponse.data;

    // If the transcription is complete, return the transcript object
    if (transcriptionResult.status === 'completed') {
      return transcriptionResult;
    }
    // If the transcription has failed, throw an error with the error message
    else if (transcriptionResult.status === 'error') {
      throw new Error(`Transcription failed: ${transcriptionResult.error}`);
    }
    // If the transcription is still in progress, wait for a few seconds before polling again
    else {
      await new Promise((resolve) => setTimeout(resolve, 3000));
    }
  }
}

async function main() {
  // Call the transcribeAudio function to start the transcription process
  const transcript = await transcribeAudio();

  if (transcript.auto_highlights_result.status === 'success') {
    console.log('Auto Highlights Results:');
    for (const result of transcript.auto_highlights_result.results) {
      console.log('Text:', result.text);
      console.log('Count:', result.count);
      console.log('Rank:', result.rank);
      console.log('Timestamps:');
      for (const timestamp of result.timestamps) {
        console.log(`  Start: ${timestamp.start} End: ${timestamp.end}`);
      }
      console.log();
    }
  } else {
    console.log('Auto highlights results not available.');
  }
  }

main();
