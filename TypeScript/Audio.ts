// audio.ts
// Manages all sound effects and background music using Web Audio API

export class AudioManager {
    private audioContext: AudioContext;
    private masterGain: GainNode;
    private sounds: Map<string, AudioBuffer> = new Map();
    private musicSource?: AudioBufferSourceNode;
    private musicGain: GainNode;
    private isMusicPlaying: boolean = false;

    constructor() {
        // Create audio context (user interaction required on most browsers)
        this.audioContext = new (window.AudioContext || (window as any).webkitAudioContext)();
        this.masterGain = this.audioContext.createGain();
        this.masterGain.connect(this.audioContext.destination);
        this.masterGain.gain.value = 0.5; // 50% volume

        // Separate gain for music to allow fading
        this.musicGain = this.audioContext.createGain();
        this.musicGain.connect(this.masterGain);
        this.musicGain.gain.value = 0.3; // Music a bit quieter
    }

    // Must be called after user interaction to unlock audio
    async resumeContext() {
        if (this.audioContext.state === 'suspended') {
            await this.audioContext.resume();
        }
    }

    // Load a sound effect from a URL (supports WAV, MP3, etc.)
    async loadSound(key: string, url: string): Promise<void> {
        const response = await fetch(url);
        const arrayBuffer = await response.arrayBuffer();
        const audioBuffer = await this.audioContext.decodeAudioData(arrayBuffer);
        this.sounds.set(key, audioBuffer);
    }

    // Play a sound effect once
    playSound(key: string, volume: number = 1.0, pitch: number = 1.0) {
        const buffer = this.sounds.get(key);
        if (!buffer) {
            console.warn(`Sound ${key} not loaded`);
            return;
        }

        const source = this.audioContext.createBufferSource();
        source.buffer = buffer;
        source.playbackRate.value = pitch;

        const gain = this.audioContext.createGain();
        gain.gain.value = volume;
        source.connect(gain).connect(this.masterGain);

        source.start();
    }

    // Play background music (looped)
    playMusic(url: string, fadeIn: number = 2.0) {
        if (this.isMusicPlaying) {
            this.stopMusic(fadeIn);
        }

        fetch(url)
            .then(response => response.arrayBuffer())
            .then(arrayBuffer => this.audioContext.decodeAudioData(arrayBuffer))
            .then(buffer => {
                this.musicSource = this.audioContext.createBufferSource();
                this.musicSource.buffer = buffer;
                this.musicSource.loop = true;
                this.musicSource.connect(this.musicGain);

                // Start with gain 0 and fade in
                this.musicGain.gain.setValueAtTime(0, this.audioContext.currentTime);
                this.musicGain.gain.linearRampToValueAtTime(0.3, this.audioContext.currentTime + fadeIn);
                this.musicSource.start();
                this.isMusicPlaying = true;
            });
    }

    // Stop background music with fade out
    stopMusic(fadeOut: number = 2.0) {
        if (!this.musicSource || !this.isMusicPlaying) return;

        this.musicGain.gain.setValueAtTime(this.musicGain.gain.value, this.audioContext.currentTime);
        this.musicGain.gain.linearRampToValueAtTime(0, this.audioContext.currentTime + fadeOut);
        this.musicSource.stop(this.audioContext.currentTime + fadeOut);
        this.isMusicPlaying = false;
        this.musicSource = undefined;
    }

    // Set master volume (0.0 to 1.0)
    setMasterVolume(vol: number) {
        this.masterGain.gain.value = Math.max(0, Math.min(1, vol));
    }
}