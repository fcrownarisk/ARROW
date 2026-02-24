// game.ts
// ARROW tv-style game with shooter mechanics and path integral visualizer (simplified)
// Uses AudioManager for sound effects and music

import 'Audio.ts'

// -------------------- Types & Constants --------------------
const CANVAS_WIDTH = 800;
const CANVAS_HEIGHT = 600;
const PLAYER_SPEED = 5;
const ENEMY_SPEED = 2;
const BULLET_SPEED = 8;

enum GameState {
    MENU,
    SHOOTER,
    PATH_INTEGRAL,
}

interface Vector2 {
    x: number;
    y: number;
}

interface GameObject {
    pos: Vector2;
    vel: Vector2;
    width: number;
    height: number;
    active: boolean;
    type: 'player' | 'enemy' | 'bullet' | 'cursor';
    update?: (dt: number) => void;
    render?: (ctx: CanvasRenderingContext2D) => void;
}

// -------------------- Game Class --------------------
export class ArrowTVGame {
    private canvas: HTMLCanvasElement;
    private ctx: CanvasRenderingContext2D;
    private audio: AudioManager;
    private state: GameState = GameState.MENU;
    private objects: GameObject[] = [];
    private keys: Set<string> = new Set();
    private lastTime: number = 0;
    private frameCount: number = 0;

    // Path integral specific
    private field: number[][] = [];
    private path: Vector2[] = [];
    private integralSum: number = 0;
    private showIntegral: boolean = true;
    private cursor: Vector2 = { x: 30, y: 15 };
    private moveCooldown: number = 0;

    constructor(canvasId: string) {
        this.canvas = document.getElementById(canvasId) as HTMLCanvasElement;
        this.ctx = this.canvas.getContext('2d')!;
        this.audio = new AudioManager();

        // Set canvas size
        this.canvas.width = CANVAS_WIDTH;
        this.canvas.height = CANVAS_HEIGHT;

        // Initialize field for path integral
        this.initField();

        // Event listeners
        window.addEventListener('keydown', (e) => this.keys.add(e.key));
        window.addEventListener('keyup', (e) => this.keys.delete(e.key));
        window.addEventListener('click', () => this.audio.resumeContext()); // Unlock audio on first click

        // Load sounds and music
        this.loadAssets();

        // Start game loop
        requestAnimationFrame((t) => this.gameLoop(t));
    }

    private async loadAssets() {
        // In a real project, you'd host actual sound files.
        // For demo, we'll create simple beep sounds using Web Audio API directly.
        // But to keep it simple, we'll use the Web Audio API to generate tones.
        // However, the AudioManager expects loaded buffers. We'll generate them programmatically.
        // For brevity, I'll skip actual loading and just use the playSound method with generated tones.
        // In practice, you'd have .wav or .mp3 files.

        // Generate a simple shoot sound (sine wave)
        this.generateSound('shoot', 600, 0.1);
        this.generateSound('explosion', 200, 0.3);
        this.generateSound('hit', 300, 0.1);

        // Start background music (using a generated loop)
        this.audio.playMusic('data:audio/wav;base64,UklGR...'); // Would need base64 of a WAV file
        // For demo, we'll skip actual music loading.
    }

    // Helper to generate a simple tone (for demo)
    private generateSound(key: string, freq: number, duration: number) {
        const sampleRate = 44100;
        const frameCount = sampleRate * duration;
        const audioBuffer = this.audio['audioContext'].createBuffer(1, frameCount, sampleRate);
        const channelData = audioBuffer.getChannelData(0);
        for (let i = 0; i < frameCount; i++) {
            channelData[i] = Math.sin(2 * Math.PI * freq * i / sampleRate) * Math.exp(-3 * i / frameCount);
        }
        this.audio['sounds'].set(key, audioBuffer);
    }

    private initField() {
        const w = 60, h = 20;
        for (let y = 0; y < h; y++) {
            const row: number[] = [];
            for (let x = 0; x < w; x++) {
                const nx = x / w - 0.5;
                const ny = y / h - 0.5;
                const gaussian = Math.exp(-(nx*nx + ny*ny) * 10);
                const wave = Math.sin(nx * 10) * 0.5 + Math.sin(ny * 15) * 0.3;
                row.push(gaussian * 2 + wave);
            }
            this.field.push(row);
        }
    }

    private gameLoop(now: number) {
        const dt = (now - (this.lastTime || now)) / 1000; // in seconds
        this.lastTime = now;

        this.update(dt);
        this.render();

        requestAnimationFrame((t) => this.gameLoop(t));
    }

    private update(dt: number) {
        // Handle state switching
        if (this.state === GameState.MENU) {
            if (this.keys.has('1')) {
                this.state = GameState.SHOOTER;
                this.initShooter();
                this.keys.delete('1');
            }
            if (this.keys.has('2')) {
                this.state = GameState.PATH_INTEGRAL;
                this.initPathIntegral();
                this.keys.delete('2');
            }
            if (this.keys.has('Escape')) {
                // Exit not applicable in browser
            }
        } else {
            // Common back to menu
            if (this.keys.has('Escape')) {
                this.state = GameState.MENU;
                this.clearObjects();
                this.keys.delete('Escape');
            }
        }

        if (this.state === GameState.SHOOTER) {
            this.updateShooter(dt);
        } else if (this.state === GameState.PATH_INTEGRAL) {
            this.updatePathIntegral(dt);
        }

        this.frameCount++;
    }

    private render() {
        this.ctx.clearRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

        if (this.state === GameState.MENU) {
            this.renderMenu();
        } else if (this.state === GameState.SHOOTER) {
            this.renderShooter();
        } else if (this.state === GameState.PATH_INTEGRAL) {
            this.renderPathIntegral();
        }
    }

    // -------------------- Menu --------------------
    private renderMenu() {
        this.ctx.fillStyle = '#000';
        this.ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);
        this.ctx.fillStyle = '#ff0';
        this.ctx.font = '30px monospace';
        this.ctx.textAlign = 'center';
        this.ctx.fillText('ARROW tv 3A', CANVAS_WIDTH/2, 150);
        this.ctx.font = '20px monospace';
        this.ctx.fillStyle = '#0ff';
        this.ctx.fillText('1 - Shooter Game', CANVAS_WIDTH/2, 250);
        this.ctx.fillText('2 - Path Integral', CANVAS_WIDTH/2, 300);
        this.ctx.fillText('ESC - Back to menu (in game)', CANVAS_WIDTH/2, 350);
    }

    // -------------------- Shooter Game --------------------
    private initShooter() {
        this.clearObjects();

        // Player
        this.objects.push({
            pos: { x: CANVAS_WIDTH/2, y: CANVAS_HEIGHT-50 },
            vel: { x: 0, y: 0 },
            width: 20, height: 20,
            active: true,
            type: 'player',
            update: (dt) => {
                const p = this.objects.find(o => o.type === 'player')!;
                if (this.keys.has('ArrowLeft')) p.pos.x -= PLAYER_SPEED;
                if (this.keys.has('ArrowRight')) p.pos.x += PLAYER_SPEED;
                if (this.keys.has('ArrowUp')) p.pos.y -= PLAYER_SPEED;
                if (this.keys.has('ArrowDown')) p.pos.y += PLAYER_SPEED;
                p.pos.x = Math.max(0, Math.min(CANVAS_WIDTH - p.width, p.pos.x));
                p.pos.y = Math.max(0, Math.min(CANVAS_HEIGHT - p.height, p.pos.y));

                // Shooting
                if (this.keys.has(' ')) {
                    this.shootBullet(p.pos.x + p.width/2, p.pos.y);
                    this.keys.delete(' '); // prevent rapid fire
                }
            }
        });

        // Initial enemies
        for (let i = 0; i < 5; i++) {
            this.spawnEnemy(100 + i*120, 50);
        }
    }

    private shootBullet(x: number, y: number) {
        this.objects.push({
            pos: { x: x-2, y: y },
            vel: { x: 0, y: -BULLET_SPEED },
            width: 4, height: 8,
            active: true,
            type: 'bullet',
        });
        this.audio.playSound('shoot', 0.3);
    }

    private spawnEnemy(x: number, y: number) {
        this.objects.push({
            pos: { x, y },
            vel: { x: 0, y: ENEMY_SPEED },
            width: 20, height: 20,
            active: true,
            type: 'enemy',
            health: 30,
        });
    }

    private updateShooter(dt: number) {
        // Update objects
        for (let obj of this.objects) {
            if (!obj.active) continue;
            obj.pos.x += obj.vel.x;
            obj.pos.y += obj.vel.y;

            if (obj.type === 'enemy' && obj.pos.y > CANVAS_HEIGHT) {
                obj.active = false;
            }
            if (obj.type === 'bullet' && obj.pos.y < 0) {
                obj.active = false;
            }

            if (obj.update) obj.update(dt);
        }

        // Collision detection (simple)
        for (let i = 0; i < this.objects.length; i++) {
            for (let j = i+1; j < this.objects.length; j++) {
                const a = this.objects[i];
                const b = this.objects[j];
                if (!a.active || !b.active) continue;

                if (a.type === 'bullet' && b.type === 'enemy') {
                    if (this.rectCollide(a, b)) {
                        a.active = false;
                        b.active = false;
                        this.audio.playSound('explosion', 0.5);
                    }
                }
                if (a.type === 'enemy' && b.type === 'bullet') {
                    if (this.rectCollide(a, b)) {
                        a.active = false;
                        b.active = false;
                        this.audio.playSound('explosion', 0.5);
                    }
                }
                if (a.type === 'player' && b.type === 'enemy') {
                    if (this.rectCollide(a, b)) {
                        // Game over? For simplicity, just remove enemy and play hit
                        b.active = false;
                        this.audio.playSound('hit', 0.5);
                    }
                }
            }
        }

        // Remove inactive objects
        this.objects = this.objects.filter(o => o.active);

        // Spawn enemies randomly
        if (Math.random() < 0.02) {
            this.spawnEnemy(Math.random() * (CANVAS_WIDTH-20), 20);
        }
    }

    private rectCollide(a: GameObject, b: GameObject): boolean {
        return a.pos.x < b.pos.x + b.width &&
               a.pos.x + a.width > b.pos.x &&
               a.pos.y < b.pos.y + b.height &&
               a.pos.y + a.height > b.pos.y;
    }

    private renderShooter() {
        // Background
        this.ctx.fillStyle = '#111';
        this.ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

        // Draw objects
        for (let obj of this.objects) {
            if (!obj.active) continue;
            this.ctx.fillStyle = obj.type === 'player' ? '#0f0' : (obj.type === 'enemy' ? '#f00' : '#ff0');
            this.ctx.fillRect(obj.pos.x, obj.pos.y, obj.width, obj.height);
        }

        // Score
        this.ctx.fillStyle = '#fff';
        this.ctx.font = '16px monospace';
        this.ctx.fillText(`Score: ${this.frameCount}`, 10, 20);
    }

    // -------------------- Path Integral --------------------
    private initPathIntegral() {
        this.path = [];
        this.integralSum = 0;
        this.cursor = { x: 30, y: 15 };
        this.moveCooldown = 0;
        this.showIntegral = true;
    }

    private updatePathIntegral(dt: number) {
        const FIELD_W = 60, FIELD_H = 20;

        // Cursor movement with cooldown
        if (this.moveCooldown <= 0) {
            if (this.keys.has('ArrowLeft') && this.cursor.x > 0) {
                this.cursor.x--;
                this.moveCooldown = 5;
            }
            if (this.keys.has('ArrowRight') && this.cursor.x < FIELD_W-1) {
                this.cursor.x++;
                this.moveCooldown = 5;
            }
            if (this.keys.has('ArrowUp') && this.cursor.y > 0) {
                this.cursor.y--;
                this.moveCooldown = 5;
            }
            if (this.keys.has('ArrowDown') && this.cursor.y < FIELD_H-1) {
                this.cursor.y++;
                this.moveCooldown = 5;
            }
        } else {
            this.moveCooldown--;
        }

        if (this.keys.has(' ')) {
            this.addPathPoint(this.cursor.x, this.cursor.y);
            this.keys.delete(' ');
        }

        if (this.keys.has('c') || this.keys.has('C')) {
            this.path = [];
            this.integralSum = 0;
            this.keys.delete('c');
            this.keys.delete('C');
        }

        if (this.keys.has('i') || this.keys.has('I')) {
            this.showIntegral = !this.showIntegral;
            this.keys.delete('i');
            this.keys.delete('I');
        }
    }

    private addPathPoint(x: number, y: number) {
        if (x < 0 || x >= 60 || y < 0 || y >= 20) return;
        if (this.path.length > 0 && this.path[this.path.length-1].x === x && this.path[this.path.length-1].y === y) return;
        if (this.path.length < 200) {
            this.path.push({ x, y });
            this.integralSum += this.field[y][x];
        }
    }

    private renderPathIntegral() {
        const CELL_SIZE = 10;
        const OFFSET_X = 50;
        const OFFSET_Y = 50;

        // Draw field
        for (let y = 0; y < 20; y++) {
            for (let x = 0; x < 60; x++) {
                const val = this.field[y][x];
                // Map to grayscale (0-255)
                const gray = Math.floor((val + 1) * 127.5);
                this.ctx.fillStyle = `rgb(${gray}, ${gray}, ${gray})`;
                this.ctx.fillRect(OFFSET_X + x*CELL_SIZE, OFFSET_Y + y*CELL_SIZE, CELL_SIZE-1, CELL_SIZE-1);
            }
        }

        // Draw path
        this.ctx.fillStyle = '#fff';
        for (let i = 0; i < this.path.length; i++) {
            const p = this.path[i];
            this.ctx.fillStyle = i === 0 ? '#ff0' : '#fff';
            this.ctx.beginPath();
            this.ctx.arc(OFFSET_X + p.x*CELL_SIZE + CELL_SIZE/2, OFFSET_Y + p.y*CELL_SIZE + CELL_SIZE/2, 4, 0, 2*Math.PI);
            this.ctx.fill();
        }

        // Draw cursor
        this.ctx.strokeStyle = '#0ff';
        this.ctx.lineWidth = 2;
        this.ctx.strokeRect(OFFSET_X + this.cursor.x*CELL_SIZE, OFFSET_Y + this.cursor.y*CELL_SIZE, CELL_SIZE, CELL_SIZE);

        // UI
        this.ctx.fillStyle = '#fff';
        this.ctx.font = '16px monospace';
        if (this.showIntegral) {
            this.ctx.fillText(`Integral: ${this.integralSum.toFixed(3)}`, 50, 30);
        } else {
            this.ctx.fillText(`Integral: (hidden)`, 50, 30);
        }
        this.ctx.fillText('ARROW: move cursor | SPACE: add point | C: clear | I: toggle | ESC: menu', 50, CANVAS_HEIGHT-20);
    }

    private clearObjects() {
        this.objects = [];
    }
}

// -------------------- Start the game --------------------
window.onload = () => {
    new ArrowTVGame('gameCanvas');
};



