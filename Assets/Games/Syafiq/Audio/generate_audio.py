import wave
import struct
import math
import random
import os

SAMPLE_RATE = 44100

def save_wav(filepath, samples):
    with wave.open(filepath, 'w') as wav_file:
        wav_file.setnchannels(1) # Mono
        wav_file.setsampwidth(2) # 16-bit
        wav_file.setframerate(SAMPLE_RATE)
        # Normalize and clamp to 16-bit signed integer (-32767 to 32767)
        max_val = max(max(abs(s) for s in samples), 0.001)
        # Target peak around 88% full scale to prevent clipping
        scaling = 0.88 / max_val
        frames = bytearray()
        for s in samples:
            val = int(s * scaling * 32767.0)
            val = max(-32767, min(32767, val))
            frames.extend(struct.pack('<h', val))
        wav_file.writeframes(frames)
    print(f"Generated: {filepath} ({len(samples)/SAMPLE_RATE:.2f}s)")

output_dir = r"d:\UNITY\MultiverseArcade\Assets\Games\Syafiq\Audio"
os.makedirs(output_dir, exist_ok=True)

# 1. SFX: Laser Shoot (Classic Arcade Sci-Fi Laser Blaster)
def gen_laser_shoot():
    dur = 0.14
    total_samples = int(SAMPLE_RATE * dur)
    samples = []
    phase = 0.0
    for i in range(total_samples):
        t = i / total_samples # 0 to 1
        # Frequency sweeps down from 1200 Hz to 180 Hz exponentially
        freq = 1200.0 * math.exp(-2.2 * t)
        phase += 2.0 * math.pi * freq / SAMPLE_RATE
        # Envelope: immediate attack, exponential decay
        env = math.exp(-3.5 * t)
        # Mix of sine + subtle 2nd harmonic
        sig = math.sin(phase) + 0.3 * math.sin(2.0 * phase)
        samples.append(sig * env)
    return samples

# 2. SFX: Enemy Hit (Crisp armor/shield impact click)
def gen_enemy_hit():
    dur = 0.09
    total_samples = int(SAMPLE_RATE * dur)
    samples = []
    phase = 0.0
    for i in range(total_samples):
        t = i / total_samples
        freq = 750.0 * (1.0 - 0.7 * t)
        phase += 2.0 * math.pi * freq / SAMPLE_RATE
        env = math.exp(-6.0 * t)
        noise = (random.random() * 2.0 - 1.0) * 0.4
        sig = (math.sin(phase) * 0.7 + noise * 0.3) * env
        samples.append(sig)
    return samples

# 3. SFX: Player Hurt (Heavy visceral thud & pain crack)
def gen_player_hurt():
    dur = 0.22
    total_samples = int(SAMPLE_RATE * dur)
    samples = []
    phase = 0.0
    for i in range(total_samples):
        t = i / total_samples
        # Pitch drops from 160Hz to 40Hz
        freq = 160.0 * (1.0 - 0.75 * t)
        phase += 2.0 * math.pi * freq / SAMPLE_RATE
        env = math.exp(-4.5 * t)
        noise = (random.random() * 2.0 - 1.0) * math.exp(-8.0 * t) * 0.6
        sig = (math.sin(phase) + noise) * env
        samples.append(sig)
    return samples

# 4. SFX: Explosion (Deep arcade rumble crunch)
def gen_explosion():
    dur = 0.48
    total_samples = int(SAMPLE_RATE * dur)
    samples = []
    phase = 0.0
    filt = 0.0
    for i in range(total_samples):
        t = i / total_samples
        # Sub-bass thump
        bass_freq = 90.0 * (1.0 - 0.7 * t)
        phase += 2.0 * math.pi * bass_freq / SAMPLE_RATE
        bass = math.sin(phase) * math.exp(-4.0 * t)
        
        # Noise rumble with low-pass filter
        raw_noise = random.random() * 2.0 - 1.0
        filt = filt * 0.85 + raw_noise * 0.15 # Low-pass filter
        noise_env = math.exp(-3.2 * t)
        
        sig = (bass * 0.5 + filt * 0.8) * noise_env
        samples.append(sig)
    return samples

# 5. SFX: Game Over (Descending retro synth chord)
def gen_game_over():
    dur = 1.6
    total_samples = int(SAMPLE_RATE * dur)
    samples = [0.0] * total_samples
    # Notes: E4 (329.6), C4 (261.6), A3 (220.0), F3 (174.6)
    notes = [329.6, 261.6, 220.0, 174.6]
    note_dur = dur / len(notes)
    
    for note_idx, freq in enumerate(notes):
        start_sample = int(note_idx * note_dur * SAMPLE_RATE)
        note_samples = int((dur - note_idx * note_dur) * SAMPLE_RATE)
        phase = 0.0
        for i in range(note_samples):
            idx = start_sample + i
            if idx >= total_samples:
                break
            t = i / SAMPLE_RATE
            phase += 2.0 * math.pi * freq / SAMPLE_RATE
            env = math.exp(-1.8 * t)
            # Warm retro saw-ish tone
            sig = (math.sin(phase) + 0.4 * math.sin(2*phase) + 0.2 * math.sin(3*phase)) * env * 0.4
            samples[idx] += sig
    return samples

# 6. BGM: Arcade Action Theme (Looping Cyberpunk Synthwave - 125 BPM, 8 bars)
def gen_arcade_bgm():
    bpm = 125.0
    beat_dur = 60.0 / bpm # 0.48 sec per beat
    total_beats = 32 # 8 measures * 4 beats = 32 beats
    total_dur = total_beats * beat_dur # ~15.36 seconds
    total_samples = int(SAMPLE_RATE * total_dur)
    samples = [0.0] * total_samples

    # Bassline notes (Root: A1=55Hz, F1=43.65Hz, G1=49Hz, E1=41.2Hz)
    bass_chords = [55.0, 55.0, 43.65, 43.65, 49.0, 49.0, 41.2, 55.0] # 8 measures
    
    # 1. Four-on-the-floor Kick Drum & Snare & Hi-Hats
    for beat in range(total_beats):
        beat_start = int(beat * beat_dur * SAMPLE_RATE)
        
        # Kick (every beat)
        kick_samples = int(0.2 * SAMPLE_RATE)
        kick_phase = 0.0
        for i in range(kick_samples):
            idx = beat_start + i
            if idx < total_samples:
                t = i / kick_samples
                kfreq = 130.0 * math.exp(-5.0 * t)
                kick_phase += 2.0 * math.pi * kfreq / SAMPLE_RATE
                kenv = math.exp(-4.5 * t)
                samples[idx] += math.sin(kick_phase) * kenv * 0.65

        # Snare (beats 2 and 4, i.e. beat % 2 == 1)
        if beat % 2 == 1:
            snare_samples = int(0.18 * SAMPLE_RATE)
            snare_phase = 0.0
            for i in range(snare_samples):
                idx = beat_start + i
                if idx < total_samples:
                    t = i / snare_samples
                    sfreq = 180.0 * (1.0 - 0.5 * t)
                    snare_phase += 2.0 * math.pi * sfreq / SAMPLE_RATE
                    noise = (random.random() * 2.0 - 1.0) * 0.7
                    senv = math.exp(-5.5 * t)
                    samples[idx] += (math.sin(snare_phase) * 0.4 + noise * 0.6) * senv * 0.45

        # Hi-Hat (every 8th note, i.e. off-beats)
        hat_start = beat_start + int(0.5 * beat_dur * SAMPLE_RATE)
        hat_samples = int(0.06 * SAMPLE_RATE)
        for i in range(hat_samples):
            idx = hat_start + i
            if idx < total_samples:
                t = i / hat_samples
                noise = (random.random() * 2.0 - 1.0)
                henv = math.exp(-12.0 * t)
                samples[idx] += noise * henv * 0.22

    # 2. Driving 16th-note Synth Bass
    sub_beats = total_beats * 4 # 16th notes
    sub_dur = beat_dur / 4.0
    for sb in range(sub_beats):
        measure = (sb // 16) % len(bass_chords)
        root_freq = bass_chords[measure]
        # Alternate octave for driving rhythm
        freq = root_freq * (2.0 if (sb % 2 == 1) else 1.0)
        sb_start = int(sb * sub_dur * SAMPLE_RATE)
        note_len = int(sub_dur * 0.85 * SAMPLE_RATE)
        phase = 0.0
        for i in range(note_len):
            idx = sb_start + i
            if idx < total_samples:
                t = i / note_len
                phase += 2.0 * math.pi * freq / SAMPLE_RATE
                env = math.exp(-3.5 * t)
                # Funky square-saw bass
                sig = (math.sin(phase) + 0.4 * math.sin(2*phase) + 0.25 * math.sin(3*phase)) * env * 0.35
                samples[idx] += sig

    # 3. Catchy Neon Arpeggio Lead (Pentatonic A minor: A4, C5, D5, E5, G5)
    arp_scale = [440.0, 523.25, 587.33, 659.25, 783.99, 880.0]
    for sb in range(sub_beats):
        # Arp pattern
        note_freq = arp_scale[(sb * 3) % len(arp_scale)]
        sb_start = int(sb * sub_dur * SAMPLE_RATE)
        note_len = int(sub_dur * 0.75 * SAMPLE_RATE)
        phase = 0.0
        for i in range(note_len):
            idx = sb_start + i
            if idx < total_samples:
                t = i / note_len
                phase += 2.0 * math.pi * note_freq / SAMPLE_RATE
                env = math.exp(-4.5 * t)
                sig = math.sin(phase) * env * 0.25
                samples[idx] += sig

    return samples

print("Synthesizing Arcade Audio Assets...")
save_wav(os.path.join(output_dir, "sfx_laser_shoot.wav"), gen_laser_shoot())
save_wav(os.path.join(output_dir, "sfx_enemy_hit.wav"), gen_enemy_hit())
save_wav(os.path.join(output_dir, "sfx_player_hurt.wav"), gen_player_hurt())
save_wav(os.path.join(output_dir, "sfx_explosion.wav"), gen_explosion())
save_wav(os.path.join(output_dir, "sfx_game_over.wav"), gen_game_over())
save_wav(os.path.join(output_dir, "bgm_arcade_action.wav"), gen_arcade_bgm())
print("All Audio Assets Successfully Generated!")
