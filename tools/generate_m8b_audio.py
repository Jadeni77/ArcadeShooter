"""Generate the project-authored M8B placeholder WAVs; no samples are imported.

Run from the repository root with ``python3 tools/generate_m8b_audio.py``.
The files are deliberately simple and will be replaced during audio polish.
"""

from __future__ import annotations

import hashlib
import math
import random
import struct
import wave
from pathlib import Path


SAMPLE_RATE = 22050
OUTPUT = Path(__file__).resolve().parents[1] / "Assets/Audio/Placeholders"

# name: (duration, voice, peak level)
CLIPS = {
    "EmptyClick": (0.12, "click", 0.34),
    "MagazineOut": (0.25, "handling_out", 0.32),
    "MagazineIn": (0.32, "handling_in", 0.38),
    "ReloadComplete": (0.18, "complete", 0.32),
    "EnemyIdle": (0.72, "enemy_idle", 0.30),
    "EnemyAlert": (0.55, "enemy_alert", 0.40),
    "EnemyAttack": (0.32, "enemy_attack", 0.42),
    "EnemyDamage": (0.25, "enemy_damage", 0.36),
    "EnemyDeath": (0.54, "enemy_death", 0.42),
    "FootstepA": (0.19, "step_a", 0.30),
    "FootstepB": (0.20, "step_b", 0.30),
    "FootstepC": (0.18, "step_c", 0.30),
    "Jump": (0.22, "jump", 0.28),
    "Land": (0.28, "land", 0.36),
    "PlayerDamage": (0.35, "player_damage", 0.38),
    "WarningCue": (0.55, "warning_cue", 0.34),
    "AreaClearedCue": (0.68, "clear_cue", 0.35),
    "ExtractionActiveCue": (0.72, "extraction_cue", 0.36),
    "CountdownStartCue": (0.42, "countdown_start", 0.32),
    "CountdownTickCue": (0.14, "tick", 0.28),
    "SuccessCue": (1.15, "success_cue", 0.38),
    "FailureCue": (1.0, "failure_cue", 0.38),
    "FacilityAmbienceLoop": (8.0, "ambience", 0.17),
    "ExplorationLoop": (8.0, "exploration", 0.22),
    "WarningLoop": (6.0, "warning_loop", 0.27),
    "CombatLoop": (8.0, "combat_loop", 0.31),
}


def tone(frequency: float, time: float) -> float:
    return math.sin(2 * math.pi * frequency * time)


def edge(time: float, duration: float, attack: float = 0.01, release: float = 0.07) -> float:
    return min(1.0, time / attack, max(0.0, duration - time) / release)


def synthesize(name: str, duration: float, voice: str, peak: float) -> None:
    seed = int.from_bytes(hashlib.sha256(name.encode()).digest()[:8], "little")
    rng = random.Random(seed)
    count = round(duration * SAMPLE_RATE)
    samples: list[float] = []
    low_noise = 0.0

    for index in range(count):
        t = index / SAMPLE_RATE
        white = rng.uniform(-1.0, 1.0)
        low_noise = 0.98 * low_noise + 0.02 * white
        sound = 0.0
        if voice in {"click", "tick", "complete", "handling_out", "handling_in"}:
            frequency = {"click": 1250, "tick": 1150, "complete": 740,
                         "handling_out": 290, "handling_in": 420}[voice]
            snap = math.exp(-t * (46 if voice in {"click", "tick"} else 22))
            sound = (0.55 * white + 0.45 * tone(frequency, t)) * snap
            if voice == "handling_in":
                sound += 0.24 * tone(560, t - 0.18) * math.exp(-70 * abs(t - 0.18))
        elif voice.startswith("step_") or voice in {"jump", "land"}:
            base = {"step_a": 92, "step_b": 108, "step_c": 82,
                    "jump": 145, "land": 75}[voice]
            decay = math.exp(-t * (18 if voice != "land" else 12))
            sound = (0.55 * low_noise + 0.45 * tone(base, t)) * decay
            if voice == "jump":
                sound += 0.2 * tone(280 + 300 * t, t) * edge(t, duration)
        elif voice.startswith("enemy_"):
            start, end = {
                "enemy_idle": (105, 95), "enemy_alert": (155, 250),
                "enemy_attack": (210, 110), "enemy_damage": (170, 85),
                "enemy_death": (130, 42),
            }[voice]
            pitch = start + (end - start) * t / duration
            sound = (0.65 * tone(pitch, t) + 0.22 * tone(2.03 * pitch, t)
                     + 0.13 * low_noise) * edge(t, duration, 0.025, 0.12)
        elif voice == "player_damage":
            sound = (0.5 * low_noise + 0.5 * tone(190 - 80 * t, t)) * edge(t, duration)
        elif voice in {"warning_cue", "countdown_start", "extraction_cue"}:
            base = {"warning_cue": 440, "countdown_start": 660,
                    "extraction_cue": 523.25}[voice]
            sound = (0.65 * tone(base, t) + 0.35 * tone(base * 1.5, t)) * edge(t, duration)
            sound *= 0.8 + 0.2 * tone(7, t)
        elif voice in {"clear_cue", "success_cue", "failure_cue"}:
            notes = {
                "clear_cue": (392, 523.25),
                "success_cue": (392, 493.88, 587.33),
                "failure_cue": (392, 311.13, 233.08),
            }[voice]
            note = notes[min(len(notes) - 1, int(t / duration * len(notes)))]
            sound = (0.7 * tone(note, t) + 0.3 * tone(note / 2, t)) * edge(t, duration, 0.03, 0.14)
        elif voice == "ambience":
            sound = (0.55 * tone(55, t) + 0.25 * tone(82.5, t)
                     + 0.2 * low_noise) * (0.72 + 0.28 * tone(0.18, t))
        elif voice in {"exploration", "warning_loop", "combat_loop"}:
            root, fifth = {
                "exploration": (82.4, 123.6),
                "warning_loop": (110.0, 164.8),
                "combat_loop": (92.5, 138.75),
            }[voice]
            sound = 0.55 * tone(root, t) + 0.25 * tone(fifth, t)
            if voice == "exploration":
                sound += 0.16 * tone(329.6, t) * (0.5 + 0.5 * tone(0.125, t))
            elif voice == "warning_loop":
                sound += 0.2 * tone(220, t) * max(0.0, tone(1.5, t))
            else:
                beat = math.exp(-24 * (t % 0.25))
                sound += beat * (0.22 * tone(62, t) + 0.16 * low_noise)
        # The short fade avoids clicks when the clip loops or is stopped.
        if voice.endswith("_loop") or voice in {"ambience", "exploration"}:
            sound *= edge(t, duration, 0.12, 0.12)
        else:
            sound *= edge(t, duration, 0.002, 0.018)
        samples.append(sound)

    maximum = max(abs(value) for value in samples) or 1.0
    scale = peak / maximum
    pcm = struct.pack("<" + "h" * count, *(round(value * scale * 32767) for value in samples))
    with wave.open(str(OUTPUT / f"{name}.wav"), "wb") as output:
        output.setnchannels(1)
        output.setsampwidth(2)
        output.setframerate(SAMPLE_RATE)
        output.writeframes(pcm)


def main() -> None:
    OUTPUT.mkdir(parents=True, exist_ok=True)
    for name, (duration, voice, peak) in CLIPS.items():
        synthesize(name, duration, voice, peak)
    print(f"Generated {len(CLIPS)} project-authored placeholder WAVs in {OUTPUT}")


if __name__ == "__main__":
    main()
