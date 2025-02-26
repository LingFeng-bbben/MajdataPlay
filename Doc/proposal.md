## Majdata Play API
The idea is: chart editor could use this api to control majplay, to view or test the chart.

### States
The viewer should have these states.
1. **Idle**: The viewer scene is loaded and ready to preload assets.
2. **Loaded**: The viewer is preloaded with song assets.
3. **Ready**: The viewer has done with deserialization of simai notes and note generation. Ready to play.
4. **Error**: The uploaded maidata is not valid to play.
4. **Playing**: Chart is playing.
5. **Paused**: Chart paused.

```
                                      ^-maidata-v----> [Error] -maidata--v
                                      |v--------<------------------------<
[Idle] -load-> [Loaded] -maidata-> [Ready] -play-> [Playing] -pause-> [Paused] -resume -> |
                                      ^                ^     -stop------>|                |
                                      <----------------|-----------------v                |
                                                       ^----------------------------------<
```

### Play setting
All play setting should be adjusted in view.

There should be a setting buttion to access setting scene in Ready state.

Also, there might be a autoplay button to quickly turn on/off autoplay.

All mods should be disabled, and the playback speed should be set by api call.

### Animation Behaviour
For the intro animation and AP animation...

Let's say we dont make that in view mode now. (to make things simple)

### GET /api/state
Get the player state.

### POST /api/load
Post the files using formdata.
* ```[FormData]``` track
* ```[FormData]``` bg
* ```[FormData]``` bga

Upload the chart assets into majplay. Returns 200 if success, 500 + message if fail.

### POST /api/maidata
* text of inner simai.

Refresh the chart content. This should trigger serialization and note generation.

Majplay should not care the difficulty of the chart, since it is acting as a viewer.

When encountering errors, majplay should popup a error dialog, as well as return 400 with message.

### GET /api/timestamp
Get the timestamp in server system. Client can use this to calculate network latency and sync with server time.

### POST /api/play
JSON
- Play start time
- Playback speed
- Timestamp to start playing (for sync)
- isRecording

Majplay should be all prepared for playing, and wait untill the given timestamp.

If it is recording, render audio, start ffmpeg and pipe screenshots out.

### GET /api/pause
pause the playback

### GET /api/resume
resume the playback

### GET /api/stop
Stop the playback

### Get /apt/reset/
Unload all assets and force the player return to the Idle state.
