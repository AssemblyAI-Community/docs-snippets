import ballerina/io;
import ballerina/http;
import ballerina/lang.runtime;

configurable string API_TOKEN = ?;

http:Client albumClient = check new ("https://api.assemblyai.com/v2", auth = {
    token: API_TOKEN
});

public function uploadFile(string path) returns string|error {
    http:Request request = new;
    // Sets the file as the request payload.
    request.setFileAsPayload(path);

    //Sends the request to the receiver service with the file content.
    json res = check albumClient->/upload.post(request);
    string uploadUrl = check res?.upload_url;
    return uploadUrl;
}

public function createTranscript(string audio_url) returns Transcription|error {
    json result = check albumClient->/transcript.post({
        audio_url
    });

    string transcriptId = check result?.id;
    while true {
        json pollingJson = check albumClient->/transcript/[transcriptId].get();
        string status = check pollingJson?.status;
        if status == "completed" {
            return pollingJson.cloneWithType();
        } else if status == "error" {
            string cause = check pollingJson?.'error;
            panic error(string `Transcription failed due to ${cause}`);
        } else {
            runtime:sleep(5);
        }
    }
}

type Transcription record {
    string text;
    float confidence;
};

public function main(string filePath) returns error? {
    string uploadUrl = check uploadFile(filePath);
    Transcription transcript = check createTranscript(uploadUrl);
    io:println(transcript);
}
