# Earth audio update

Earth previously reused the same Scyther miss folder for all five cues. Its sound bank now layers actual Core stone hits, mining strikes, loose stone and rock collapse with eight original short gravel/pressure/rumble support clips.

- Faultline Cleave: dry stone fracture with a short grit tail.
- Stonebreaker Advance: rolling gravel on launch, a separate dense pick/stone impact on arrival.
- Both ground stabs: a short crescendo of grinding stone before commitment.
- Seismic Quake: a low circular seismic thump, rolling rubble and a longer settling tail.
- Spire Eruption: a sharper stone-splitting impact with a shorter, crunchier debris tail.
- Earth transformation: a quieter stone-lock cue using the existing generic impact hook.

All cues are finite one-shots, with at most two layers, three simultaneous copies per definition, spatial attenuation and the unchanged shared pawn/map/camera/fog audio budget. Support WAVs have two deterministic variants, mono 44.1 kHz 16-bit PCM and short faded endpoints. Core clips are referenced at runtime, not copied or redistributed. Lightning, thunder and all other elemental banks are unchanged. No saved state or gameplay tuning changes.

## Validation and reproduction

The local audio integrity check passed 191 cases for clip uniqueness, PCM format, finite duration, peaks, DC bias, faded endpoints, source references, voice bounds and preservation of the other elemental banks. The 5,592 Earth regressions and Earth definition/asset checks passed against the rebuilt assembly. An isolated graphics-enabled native sound fixture passed 62 checks for resolved Core grains, both support variants, channel/rate/duration, cached definitions, cue aliases, per-cue throttles and native one-shot submission.

The original support layers can be reproduced with Python's standard library using `Tools/GenerateEarthAudio.py`. No network access, external audio library or game-audio extraction is needed. `Docs/Audio/EarthAudioManifest.json` records hashes and the sound-layer mapping. The dry support clips are only one layer of the final sound; native Core stone/collapse layers are mixed at runtime. Final perceived loudness remains dependent on the player's game volume, camera distance and speakers.
