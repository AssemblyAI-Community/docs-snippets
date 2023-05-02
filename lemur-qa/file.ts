import axios from 'axios';

const API_TOKEN = 'your_api_token';
const TRANSCRIPT_ID = 'transcript_id';

// Function that sends a request to the Lemur API
async function lemurPost(api_token: string, transcriptId: string) {
  const headers = {
    authorization: api_token,
    'content-type': 'application/json',
  };

  // Send a POST request to the LeMUR API with the transcript ID in the request body
  const response = await axios.post('https://api.staging.assemblyai-labs.com/beta/generate/question-answer', {
    "transcript_ids": [
      transcriptId
    ],
    "questions": [
      {
        "question": "Is this caller a qualified buyer?",
        "answer_options": [
          "Yes",
          "No"
        ]
      },
      {
        "question": "Classify the call into one of the following scenarios",
        "answer_options": [
          "Follow-up",
          "2nd Call"
        ],
        "context": "Anytime it is clear that the caller is calling back a second time about the same topic"
      },
      {
        "question": "What is the caller's mood?",
        "answer_format": "Short sentence"
      }
    ]
  }, { headers });

  // Retrieve the LeMUR API response
  return response.data["response"];
}

async function main() {
  const lemurResponse = await lemurPost(API_TOKEN, TRANSCRIPT_ID);
  console.log(lemurResponse);
}

main();
