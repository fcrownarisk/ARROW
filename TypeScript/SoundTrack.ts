// soundtrack.ts
// Official ARROW tv Soundtrack Manager
// Handles loading, playing, and mixing of the official game soundtrack

export interface TrackMetadata {
    id: string;
    title: string;
    composer: string;
    duration: number;
    bpm: number;
    key: string;
    mood: 'upbeat' | 'tense' | 'calm' | 'epic' | 'mysterious';
    scene: 'menu' | 'shooter' | 'path_integral' | 'boss' | 'victory';
}

export interface Track {
    id: string;
    url: string;
    metadata: TrackMetadata;
    audioBuffer?: AudioBuffer;
}

export class SoundtrackManager {
    private audioContext: AudioContext;
    private masterGain: GainNode;
    private musicGain: GainNode;
    private currentSource?: AudioBufferSourceNode;
    private nextSource?: AudioBufferSourceNode;
    private tracks: Map<string, Track> = new Map();
    private currentTrackId: string = '';
    private isTransitioning: boolean = false;
    private volume: number = 0.5;
    private crossfadeDuration: number = 3.0; // seconds

    // Official ARROW tv Soundtrack URLs
    // These would be replaced with actual hosted audio files
    private readonly SOUNDTRACK_URLS = {
        // Main Theme
        MENU_THEME: 'https://assets.arrowtv.com/soundtrack/menu_theme.mp3',
        ARROW_MAIN_THEME: 'https://assets.arrowtv.com/soundtrack/arrow_main_theme.mp3',
        
        // Shooter Game Themes
        SHOOTER_ACTION: 'https://assets.arrowtv.com/soundtrack/shooter_action.mp3',
        SHOOTER_BATTLE: 'https://assets.arrowtv.com/soundtrack/shooter_battle.mp3',
        SHOOTER_BOSS: 'https://assets.arrowtv.com/soundtrack/shooter_boss.mp3',
        
        // Path Integral Themes
        PATH_AMBIENT: 'https://assets.arrowtv.com/soundtrack/path_ambient.mp3',
        PATH_SUSPENSE: 'https://assets.arrowtv.com/soundtrack/path_suspense.mp3',
        PATH_CALM: 'https://assets.arrowtv.com/soundtrack/path_calm.mp3',
        
        // Special Themes
        VICTORY_FANFARE: 'https://assets.arrowtv.com/soundtrack/victory_fanfare.mp3',
        GAME_OVER: 'https://assets.arrowtv.com/soundtrack/game_over.mp3',
    };

    constructor() {
        this.audioContext = new (window.AudioContext || (window as any).webkitAudioContext)();
        this.masterGain = this.audioContext.createGain();
        this.musicGain = this.audioContext.createGain();
        
        this.masterGain.connect(this.audioContext.destination);
        this.musicGain.connect(this.masterGain);
        
        this.masterGain.gain.value = 0.7; // Master volume
        this.musicGain.gain.value = this.volume;
        
        this.initializeSoundtrack();
    }

    // Initialize all soundtrack tracks with metadata
    private initializeSoundtrack() {
        // Main Menu Theme
        this.registerTrack({
            id: 'menu_theme',
            url: this.SOUNDTRACK_URLS.MENU_THEME,
            metadata: {
                id: 'menu_theme',
                title: 'ARROW tv Main Menu',
                composer: 'ARROW Sound Team',
                duration: 180,
                bpm: 120,
                key: 'C# minor',
                mood: 'epic',
                scene: 'menu'
            }
        });

        // Shooter Action Theme
        this.registerTrack({
            id: 'shooter_action',
            url: this.SOUNDTRACK_URLS.SHOOTER_ACTION,
            metadata: {
                id: 'shooter_action',
                title: 'Neon Assault',
                composer: 'ARROW Sound Team',
                duration: 240,
                bpm: 140,
                key: 'E minor',
                mood: 'upbeat',
                scene: 'shooter'
            }
        });

        // Shooter Battle Theme
        this.registerTrack({
            id: 'shooter_battle',
            url: this.SOUNDTRACK_URLS.SHOOTER_BATTLE,
            metadata: {
                id: 'shooter_battle',
                title: 'Digital Warfare',
                composer: 'ARROW Sound Team',
                duration: 210,
                bpm: 160,
                key: 'F# minor',
                mood: 'tense',
                scene: 'shooter'
            }
        });

        // Boss Battle Theme
        this.registerTrack({
            id: 'shooter_boss',
            url: this.SOUNDTRACK_URLS.SHOOTER_BOSS,
            metadata: {
                id: 'shooter_boss',
                title: 'Final Stand',
                composer: 'ARROW Sound Team',
                duration: 300,
                bpm: 170,
                key: 'G minor',
                mood: 'epic',
                scene: 'boss'
            }
        });

        // Path Integral Ambient Theme
        this.registerTrack({
            id: 'path_ambient',
            url: this.SOUNDTRACK_URLS.PATH_AMBIENT,
            metadata: {
                id: 'path_ambient',
                title: 'Quantum Field',
                composer: 'ARROW Sound Team',
                duration: 360,
                bpm: 90,
                key: 'A minor',
                mood: 'mysterious',
                scene: 'path_integral'
            }
        });

        // Path Integral Suspense Theme
        this.registerTrack({
            id: 'path_suspense',
            url: this.SOUNDTRACK_URLS.PATH_SUSPENSE,
            metadata: {
                id: 'path_suspense',
                title: 'Integration Echoes',
                composer: 'ARROW Sound Team',
                duration: 280,
                bpm: 100,
                key: 'D minor',
                mood: 'tense',
                scene: 'path_integral'
            }
        });

        // Path Integral Calm Theme
        this.registerTrack({
            id: 'path_calm',
            url: this.SOUNDTRACK_URLS.PATH_CALM,
            metadata: {
                id: 'path_calm',
                title: 'Wave Function',
                composer: 'ARROW Sound Team',
                duration: 320,
                bpm: 80,
                key: 'F major',
                mood: 'calm',
                scene: 'path_integral'
            }
        });

        // Victory Fanfare
        this.registerTrack({
            id: 'victory_fanfare',
            url: this.SOUNDTRACK_URLS.VICTORY_FANFARE,
            metadata: {
                id: 'victory_fanfare',
                title: 'Triumph',
                composer: 'ARROW Sound Team',
                duration: 30,
                bpm: 150,
                key: 'C major',
                mood: 'epic',
                scene: 'victory'
            }
        });

        // Game Over Theme
        this.registerTrack({
            id: 'game_over',
            url: this.SOUNDTRACK_URLS.GAME_OVER,
            metadata: {
                id: 'game_over',
                title: 'System Failure',
                composer: 'ARROW Sound Team',
                duration: 45,
                bpm: 60,
                key: 'C# minor',
                mood: 'tense',
                scene: 'victory'
            }
        });

        // ARROW Main Theme (Official)
        this.registerTrack({
            id: 'arrow_main_theme',
            url: this.SOUNDTRACK_URLS.ARROW_MAIN_THEME,
            metadata: {
                id: 'arrow_main_theme',
                title: 'ARROW tv Main Theme',
                composer: 'Official ARROW Sound Team',
                duration: 210,
                bpm: 130,
                key: 'E minor',
                mood: 'epic',
                scene: 'menu'
            }
        });
    }

    private registerTrack(track: Track) {
        this.tracks.set(track.id, track);
    }

    // Load a track into memory
    async loadTrack(trackId: string): Promise<AudioBuffer> {
        const track = this.tracks.get(trackId);
        if (!track) {
            throw new Error(`Track ${trackId} not found`);
        }

        // If already loaded, return the buffer
        if (track.audioBuffer) {
            return track.audioBuffer;
        }

        try {
            const response = await fetch(track.url);
            const arrayBuffer = await response.arrayBuffer();
            const audioBuffer = await this.audioContext.decodeAudioData(arrayBuffer);
            track.audioBuffer = audioBuffer;
            return audioBuffer;
        } catch (error) {
            console.error(`Failed to load track ${trackId}:`, error);
            throw error;
        }
    }

    // Play a track with optional crossfade
    async playTrack(trackId: string, fadeIn: boolean = true, crossfade: boolean = true) {
        await this.resumeContext();

        const track = this.tracks.get(trackId);
        if (!track) {
            console.error(`Track ${trackId} not found`);
            return;
        }

        // If it's the same track, don't restart
        if (this.currentTrackId === trackId && this.currentSource) {
            return;
        }

        // Load the track if not already loaded
        if (!track.audioBuffer) {
            await this.loadTrack(trackId);
        }

        const source = this.audioContext.createBufferSource();
        source.buffer = track.audioBuffer!;
        source.loop = true;

        // Create a gain node for this source
        const gainNode = this.audioContext.createGain();
        source.connect(gainNode);
        gainNode.connect(this.musicGain);

        // Start with zero gain if fading in
        if (fadeIn) {
            gainNode.gain.setValueAtTime(0, this.audioContext.currentTime);
            gainNode.gain.linearRampToValueAtTime(this.volume, this.audioContext.currentTime + 2.0);
        } else {
            gainNode.gain.setValueAtTime(this.volume, this.audioContext.currentTime);
        }

        source.start(0);

        // Handle crossfade from current track
        if (crossfade && this.currentSource) {
            this.isTransitioning = true;
            
            // Find the gain node connected to the current source
            // This is a simplified version; in production you'd track gain nodes
            const oldGain = this.musicGain;
            
            // Fade out old track
            oldGain.gain.setValueAtTime(this.volume, this.audioContext.currentTime);
            oldGain.gain.linearRampToValueAtTime(0, this.audioContext.currentTime + this.crossfadeDuration);
            
            // Stop old source after fade
            setTimeout(() => {
                if (this.currentSource) {
                    this.currentSource.stop();
                    this.currentSource.disconnect();
                }
                this.isTransitioning = false;
            }, this.crossfadeDuration * 1000);
        } else if (this.currentSource) {
            // Immediate stop without fade
            this.currentSource.stop();
            this.currentSource.disconnect();
        }

        this.currentSource = source;
        this.currentTrackId = trackId;

        // Log which track is playing
        console.log(`🎵 Now Playing: ${track.metadata.title} by ${track.metadata.composer} [${track.metadata.mood}]`);
    }

    // Play a track based on game scene and intensity
    async playSceneMusic(scene: string, intensity: number = 0.5) {
        let trackId = '';

        switch (scene) {
            case 'menu':
                trackId = intensity > 0.7 ? 'arrow_main_theme' : 'menu_theme';
                break;
            case 'shooter':
                if (intensity > 0.8) {
                    trackId = 'shooter_boss';
                } else if (intensity > 0.4) {
                    trackId = 'shooter_battle';
                } else {
                    trackId = 'shooter_action';
                }
                break;
            case 'path_integral':
                if (intensity > 0.7) {
                    trackId = 'path_suspense';
                } else if (intensity > 0.3) {
                    trackId = 'path_ambient';
                } else {
                    trackId = 'path_calm';
                }
                break;
            case 'victory':
                trackId = 'victory_fanfare';
                break;
            case 'gameover':
                trackId = 'game_over';
                break;
            default:
                trackId = 'arrow_main_theme';
        }

        await this.playTrack(trackId, true, true);
    }

    // Stop current track with fade out
    stopTrack(fadeOut: boolean = true) {
        if (!this.currentSource) return;

        if (fadeOut) {
            this.musicGain.gain.setValueAtTime(this.volume, this.audioContext.currentTime);
            this.musicGain.gain.linearRampToValueAtTime(0, this.audioContext.currentTime + 2.0);
            
            setTimeout(() => {
                if (this.currentSource) {
                    this.currentSource.stop();
                    this.currentSource.disconnect();
                    this.currentSource = undefined;
                    this.currentTrackId = '';
                }
                this.musicGain.gain.setValueAtTime(this.volume, this.audioContext.currentTime);
            }, 2000);
        } else {
            this.currentSource.stop();
            this.currentSource.disconnect();
            this.currentSource = undefined;
            this.currentTrackId = '';
        }
    }

    // Pause/Resume audio context
    async resumeContext() {
        if (this.audioContext.state === 'suspended') {
            await this.audioContext.resume();
        }
    }

    // Set volume (0.0 to 1.0)
    setVolume(volume: number) {
        this.volume = Math.max(0, Math.min(1, volume));
        this.musicGain.gain.setValueAtTime(this.volume, this.audioContext.currentTime);
    }

    // Get current track metadata
    getCurrentTrack(): Track | undefined {
        return this.tracks.get(this.currentTrackId);
    }

    // Get all tracks for a specific scene
    getTracksByScene(scene: string): Track[] {
        return Array.from(this.tracks.values()).filter(
            track => track.metadata.scene === scene
        );
    }

    // Preload all tracks for a scene
    async preloadSceneTracks(scene: string) {
        const tracks = this.getTracksByScene(scene);
        await Promise.all(tracks.map(track => this.loadTrack(track.id)));
        console.log(`✅ Preloaded ${tracks.length} tracks for ${scene} scene`);
    }

    // Create a seamless loop by scheduling the next track
    scheduleNextTrack(trackId: string, delay: number) {
        setTimeout(async () => {
            await this.playTrack(trackId, true, true);
        }, delay * 1000);
    }
}

// Export a singleton instance
export const soundtrack = new SoundtrackManager();
// Export track IDs for easy reference
export const TRACKS = {
    MENU_THEME: 'menu_theme',
    ARROW_MAIN_THEME: 'arrow_main_theme',
    SHOOTER_ACTION: 'shooter_action',
    SHOOTER_BATTLE: 'shooter_battle',
    SHOOTER_BOSS: 'shooter_boss',
    PATH_AMBIENT: 'path_ambient',
    PATH_SUSPENSE: 'path_suspense',
    PATH_CALM: 'path_calm',
    VICTORY_FANFARE: 'victory_fanfare',
    GAME_OVER: 'game_over',
} as const;





export class DevSoundtrackManager extends SoundtrackManager {
    constructor() {
        super();
        this.generatePlaceholderTracks();
    }

    private generatePlaceholderTracks() {
        // Generate simple sine wave tones as placeholders
        const ctx = this['audioContext'];
        
        const generateTone = (freq: number, duration: number): AudioBuffer => {
            const sampleRate = ctx.sampleRate;
            const frames = duration * sampleRate;
            const buffer = ctx.createBuffer(1, frames, sampleRate);
            const data = buffer.getChannelData(0);
            
            for (let i = 0; i < frames; i++) {
                // Simple melody pattern
                const t = i / sampleRate;
                const melody = Math.sin(2 * Math.PI * freq * t) * 
                              Math.exp(-2 * t / duration) * 0.5;
                data[i] = melody;
            }
            return buffer;
        };

        // Assign placeholder buffers to tracks
        const tracks = this['tracks'];
        tracks.get('menu_theme')!.audioBuffer = generateTone(440, 10);
        tracks.get('shooter_action')!.audioBuffer = generateTone(880, 10);
        tracks.get('path_ambient')!.audioBuffer = generateTone(220, 10);
        // ... etc
    }
}