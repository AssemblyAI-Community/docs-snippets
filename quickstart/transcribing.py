import json, requests, time

ASSEMBLYAI_API_TOKEN = '' # find your API token: https://www.assemblyai.com/app
AUDIO_URL = 'https://storage.googleapis.com/aai-web-samples/news.mp4'

response = requests.post("https://api.assemblyai.com/v2/transcript",                                                                                              
                         json={"audio_url":AUDIO_URL},
                         headers={"authorization": ASSEMBLYAI_API_TOKEN})

transcript_id = response.json()['id']
print("transcript id is {}".format(transcript_id))
polling_endpoint = f"https://api.assemblyai.com/v2/transcript/{transcript_id}"

# you can also use a webhook to find out when the transcription is ready
# https://www.assemblyai.com/docs/walkthroughs#using-webhooks
transcription_finished = False
while not transcription_finished:
    transcription_result = requests.get(polling_endpoint,
                                      headers={"authorization": ASSEMBLYAI_API_TOKEN}).json()

    if transcription_result['status'] == 'completed':
        print(transcription_result['text'])
        transcription_finished = True

    elif transcription_result['status'] == 'error':
      raise RuntimeError(f"Transcription failed: {transcription_result['error']}")

    else:
      time.sleep(4)
      print("transcript is processing...")

print("""
Congratulations, you just transcribed your first audio file! Check out
our video series for what to do next:
https://www.youtube.com/playlist?list=PLcWfeUsAys2lUkQ-s5wtyLhYPJ3B4qUK2
""")