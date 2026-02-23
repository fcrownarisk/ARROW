/**
 * ARROW tv 3A Game Kernel
 * A lightweight game engine for embedded gaming systems
 * 
 * Author: Game Developer
 * Version: 1.0
 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdbool.h>
#include <time.h>
#include <conio.h>  // For console I/O (Windows)
#include <windows.h> // For Sleep function

// ==================== CONSTANTS AND MACROS ====================

#define SCREEN_WIDTH 80
#define SCREEN_HEIGHT 25
#define MAX_SPRITES 100
#define MAX_SOUNDS 10
#define MAX_GAME_OBJECTS 50
#define MAX_INPUT_BUFFER 256
#define FRAME_RATE 60
#define FRAME_TIME (1000 / FRAME_RATE)  // Milliseconds per frame

// Color definitions
#define COLOR_BLACK 0
#define COLOR_RED 1
#define COLOR_GREEN 2
#define COLOR_YELLOW 3
#define COLOR_BLUE 4
#define COLOR_MAGENTA 5
#define COLOR_CYAN 6
#define COLOR_WHITE 7

// Input key codes
#define KEY_UP 72
#define KEY_DOWN 80
#define KEY_LEFT 75
#define KEY_RIGHT 77
#define KEY_SPACE 32
#define KEY_ENTER 13
#define KEY_ESC 27

// ==================== TYPE DEFINITIONS ====================

typedef unsigned char byte;
typedef unsigned int uint;

// Vector2 structure for 2D coordinates
typedef struct {
    int x;
    int y;
} Vector2;

// Color structure
typedef struct {
    byte r;
    byte g;
    byte b;
} Color;

// Sprite structure
typedef struct {
    char* data;      // ASCII art representation
    int width;
    int height;
    Color color;
    bool is_visible;
} Sprite;

// GameObject structure
typedef struct GameObject {
    Vector2 position;
    Vector2 velocity;
    Sprite* sprite;
    bool active;
    int type;
    int health;
    void (*update)(struct GameObject* self);
    void (*render)(struct GameObject* self);
    void (*collide)(struct GameObject* self, struct GameObject* other);
} GameObject;

// Sound structure (simplified)
typedef struct {
    int frequency;
    int duration;
    bool is_playing;
} Sound;

// Input structure
typedef struct {
    bool keys[256];
    Vector2 mouse_pos;
    bool mouse_buttons[3];
} Input;

// Scene structure
typedef struct Scene {
    char name[32];
    void (*init)(void);
    void (*update)(float delta_time);
    void (*render)(void);
    void (*cleanup)(void);
    struct Scene* next;
} Scene;

// Game state structure
typedef struct {
    bool running;
    bool paused;
    float delta_time;
    uint frame_count;
    Scene* current_scene;
    Input input;
    GameObject* game_objects[MAX_GAME_OBJECTS];
    int object_count;
    Sound sounds[MAX_SOUNDS];
} GameState;

// ==================== GLOBAL VARIABLES ====================

static GameState game_state = {0};
static HANDLE console_handle;
static CONSOLE_SCREEN_BUFFER_INFO console_info;
static CHAR_INFO* console_buffer;

// ==================== CONSOLE FUNCTIONS ====================

/**
 * Initialize console for double buffering
 */
void console_init() {
    console_handle = GetStdHandle(STD_OUTPUT_HANDLE);
    GetConsoleScreenBufferInfo(console_handle, &console_info);
    
    // Allocate console buffer
    console_buffer = (CHAR_INFO*)malloc(sizeof(CHAR_INFO) * SCREEN_WIDTH * SCREEN_HEIGHT);
    
    // Hide cursor
    CONSOLE_CURSOR_INFO cursor_info;
    GetConsoleCursorInfo(console_handle, &cursor_info);
    cursor_info.bVisible = FALSE;
    SetConsoleCursorInfo(console_handle, &cursor_info);
}

/**
 * Clear the console buffer
 */
void console_clear() {
    for (int i = 0; i < SCREEN_WIDTH * SCREEN_HEIGHT; i++) {
        console_buffer[i].Char.AsciiChar = ' ';
        console_buffer[i].Attributes = COLOR_WHITE;
    }
}

/**
 * Set character at position with color
 */
void console_set_pixel(int x, int y, char c, Color color) {
    if (x >= 0 && x < SCREEN_WIDTH && y >= 0 && y < SCREEN_HEIGHT) {
        int index = y * SCREEN_WIDTH + x;
        console_buffer[index].Char.AsciiChar = c;
        
        // Convert RGB to console attributes (simplified)
        byte attr = 0;
        if (color.r > 128) attr |= FOREGROUND_RED;
        if (color.g > 128) attr |= FOREGROUND_GREEN;
        if (color.b > 128) attr |= FOREGROUND_BLUE;
        
        console_buffer[index].Attributes = attr;
    }
}

/**
 * Render sprite at position
 */
void console_draw_sprite(Sprite* sprite, int x, int y) {
    if (!sprite || !sprite->is_visible) return;
    
    for (int sy = 0; sy < sprite->height; sy++) {
        for (int sx = 0; sx < sprite->width; sx++) {
            int index = sy * sprite->width + sx;
            if (sprite->data[index] != ' ') {  // Skip transparent pixels
                console_set_pixel(x + sx, y + sy, sprite->data[index], sprite->color);
            }
        }
    }
}

/**
 * Draw text at position
 */
void console_draw_text(const char* text, int x, int y, Color color) {
    int len = strlen(text);
    for (int i = 0; i < len; i++) {
        console_set_pixel(x + i, y, text[i], color);
    }
}

/**
 * Flip buffer to screen
 */
void console_flip() {
    COORD buffer_size = {SCREEN_WIDTH, SCREEN_HEIGHT};
    COORD buffer_coord = {0, 0};
    SMALL_RECT write_region = {0, 0, SCREEN_WIDTH - 1, SCREEN_HEIGHT - 1};
    
    WriteConsoleOutput(console_handle, console_buffer, buffer_size, 
                      buffer_coord, &write_region);
}

/**
 * Cleanup console
 */
void console_cleanup() {
    free(console_buffer);
    
    // Show cursor again
    CONSOLE_CURSOR_INFO cursor_info;
    GetConsoleCursorInfo(console_handle, &cursor_info);
    cursor_info.bVisible = TRUE;
    SetConsoleCursorInfo(console_handle, &cursor_info);
}

// ==================== INPUT FUNCTIONS ====================

/**
 * Update input state
 */
void input_update() {
    // Check keyboard
    for (int i = 0; i < 256; i++) {
        game_state.input.keys[i] = (GetAsyncKeyState(i) & 0x8000) != 0;
    }
    
    // Check mouse
    POINT mouse_point;
    GetCursorPos(&mouse_point);
    ScreenToClient(GetConsoleWindow(), &mouse_point);
    game_state.input.mouse_pos.x = mouse_point.x;
    game_state.input.mouse_pos.y = mouse_point.y;
    
    game_state.input.mouse_buttons[0] = (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0;
    game_state.input.mouse_buttons[1] = (GetAsyncKeyState(VK_RBUTTON) & 0x8000) != 0;
    game_state.input.mouse_buttons[2] = (GetAsyncKeyState(VK_MBUTTON) & 0x8000) != 0;
}

/**
 * Check if key is pressed
 */
bool input_key_pressed(int key) {
    return game_state.input.keys[key];
}

// ==================== GAME OBJECT FUNCTIONS ====================

/**
 * Create a new game object
 */
GameObject* game_object_create(int x, int y, Sprite* sprite) {
    if (game_state.object_count >= MAX_GAME_OBJECTS) {
        return NULL;
    }
    
    GameObject* obj = (GameObject*)malloc(sizeof(GameObject));
    if (!obj) return NULL;
    
    obj->position.x = x;
    obj->position.y = y;
    obj->velocity.x = 0;
    obj->velocity.y = 0;
    obj->sprite = sprite;
    obj->active = true;
    obj->type = 0;
    obj->health = 250;
    obj->update = NULL;
    obj->render = NULL;
    obj->collide = NULL;
    
    game_state.game_objects[game_state.object_count++] = obj;
    
    return obj;
}

/**
 * Destroy game object
 */
void game_object_destroy(GameObject* obj) {
    for (int i = 0; i < game_state.object_count; i++) {
        if (game_state.game_objects[i] == obj) {
            free(obj);
            
            // Shift remaining objects
            for (int j = i; j < game_state.object_count - 1; j++) {
                game_state.game_objects[j] = game_state.game_objects[j + 1];
            }
            
            game_state.object_count--;
            break;
        }
    }
}

/**
 * Update all game objects
 */
void game_objects_update() {
    for (int i = 0; i < game_state.object_count; i++) {
        GameObject* obj = game_state.game_objects[i];
        if (obj->active) {
            // Update position based on velocity
            obj->position.x += obj->velocity.x;
            obj->position.y += obj->velocity.y;
            
            // Call custom update if exists
            if (obj->update) {
                obj->update(obj);
            }
        }
    }
}

/**
 * Check collisions between objects
 */
void game_objects_check_collisions() {
    for (int i = 0; i < game_state.object_count; i++) {
        for (int j = i + 1; j < game_state.object_count; j++) {
            GameObject* a = game_state.game_objects[i];
            GameObject* b = game_state.game_objects[j];
            
            if (a->active && b->active && a->sprite && b->sprite) {
                // Simple AABB collision detection
                bool collision = (a->position.x < b->position.x + b->sprite->width &&
                                 a->position.x + a->sprite->width > b->position.x &&
                                 a->position.y < b->position.y + b->sprite->height &&
                                 a->position.y + a->sprite->height > b->position.y);
                
                if (collision) {
                    if (a->collide) a->collide(a, b);
                    if (b->collide) b->collide(b, a);
                }
            }
        }
    }
}

/**
 * Render all game objects
 */
void game_objects_render() {
    for (int i = 0; i < game_state.object_count; i++) {
        GameObject* obj = game_state.game_objects[i];
        if (obj->active && obj->sprite) {
            if (obj->render) {
                obj->render(obj);
            } else {
                console_draw_sprite(obj->sprite, obj->position.x, obj->position.y);
            }
        }
    }
}

// ==================== SOUND FUNCTIONS ====================

/**
 * Play a sound
 */
void sound_play(int frequency, int duration) {
    Beep(frequency, duration);
}

/**
 * Initialize sound
 */
void sound_init() {
    for (int i = 0; i < MAX_SOUNDS; i++) {
        game_state.sounds[i].is_playing = false;
    }
}

// ==================== SCENE FUNCTIONS ====================

/**
 * Change to a different scene
 */
void scene_change(Scene* new_scene) {
    if (game_state.current_scene && game_state.current_scene->cleanup) {
        game_state.current_scene->cleanup();
    }
    
    game_state.current_scene = new_scene;
    
    if (game_state.current_scene && game_state.current_scene->init) {
        game_state.current_scene->init();
    }
}

// ==================== SPRITE CREATION FUNCTIONS ====================

/**
 * Create a sprite from ASCII art
 */
Sprite* sprite_create(const char* art[], int lines, Color color) {
    Sprite* sprite = (Sprite*)malloc(sizeof(Sprite));
    if (!sprite) return NULL;
    
    // Calculate dimensions
    sprite->height = lines;
    sprite->width = 0;
    for (int i = 0; i < lines; i++) {
        int len = strlen(art[i]);
        if (len > sprite->width) {
            sprite->width = len;
        }
    }
    
    // Allocate and copy data
    sprite->data = (char*)malloc(sprite->width * sprite->height);
    if (!sprite->data) {
        free(sprite);
        return NULL;
    }
    
    // Fill with spaces initially
    memset(sprite->data, ' ', sprite->width * sprite->height);
    
    // Copy the ASCII art
    for (int y = 0; y < lines; y++) {
        int len = strlen(art[y]);
        for (int x = 0; x < len; x++) {
            sprite->data[y * sprite->width + x] = art[y][x];
        }
    }
    
    sprite->color = color;
    sprite->is_visible = true;
    
    return sprite;
}

/**
 * Destroy a sprite
 */
void sprite_destroy(Sprite* sprite) {
    if (sprite) {
        if (sprite->data) {
            free(sprite->data);
        }
        free(sprite);
    }
}

// ==================== GAME INITIALIZATION ====================

/**
 * Initialize the game kernel
 */
void game_init() {
    console_init();
    sound_init();
    
    game_state.running = true;
    game_state.paused = false;
    game_state.frame_count = 0;
    game_state.object_count = 0;
    
    srand(time(NULL));
}

/**
 * Shutdown the game kernel
 */
void game_shutdown() {
    // Destroy all game objects
    while (game_state.object_count > 0) {
        game_object_destroy(game_state.game_objects[0]);
    }
    
    console_cleanup();
}

// ==================== EXAMPLE GAME IMPLEMENTATION ====================

// Example sprites
const char* player_art[] = {
    " O ",
    "/|\\",
    "/ \\"
};

const char* enemy_art[] = {
    "\\O/",
    " | ",
    "/ \\"
};

const char* bullet_art[] = {
    "!"
};

// Player object update function
void player_update(GameObject* self) {
    // Move player with arrow keys
    if (input_key_pressed(KEY_LEFT) && self->position.x > 0) {
        self->position.x -= 2;
    }
    if (input_key_pressed(KEY_RIGHT) && self->position.x < SCREEN_WIDTH - 3) {
        self->position.x += 2;
    }
    if (input_key_pressed(KEY_UP) && self->position.y > 0) {
        self->position.y -= 1;
    }
    if (input_key_pressed(KEY_DOWN) && self->position.y < SCREEN_HEIGHT - 3) {
        self->position.y += 1;
    }
    
    // Shoot with space
    static int shoot_timer = 0;
    if (input_key_pressed(KEY_SPACE) && shoot_timer <= 0) {
        // Create bullet
        GameObject* bullet = game_object_create(
            self->position.x + 1, 
            self->position.y - 1, 
            sprite_create(bullet_art, 1, (Color){255, 255, 255})
        );
        if (bullet) {
            bullet->velocity.y = -2;
            bullet->type = 1;  // Player bullet
            sound_play(800, 50);
        }
        shoot_timer = 10;
    }
    
    if (shoot_timer > 0) shoot_timer--;
}

// Enemy update function
void enemy_update(GameObject* self) {
    // Move enemy down
    self->position.y += 1;
    
    // Remove if off screen
    if (self->position.y >= SCREEN_HEIGHT) {
        self->active = false;
    }
}

// Bullet update function
void bullet_update(GameObject* self) {
    // Remove if off screen
    if (self->position.y < 0 || self->position.y >= SCREEN_HEIGHT) {
        self->active = false;
    }
}

// Collision handling
void bullet_collide(GameObject* self, GameObject* other) {
    if (self->type == 1 && other->type == 2) {  // Player bullet hits enemy
        other->health -= 10;
        self->active = false;  // Remove bullet
        
        if (other->health <= 0) {
            other->active = false;  // Remove enemy
            sound_play(400, 100);
        }
    }
}

// Scene initialization
void game_scene_init() {
    // Create player
    Sprite* player_sprite = sprite_create(player_art, 3, (Color){0, 255, 0});
    GameObject* player = game_object_create(SCREEN_WIDTH / 2, SCREEN_HEIGHT - 5, player_sprite);
    if (player) {
        player->update = player_update;
        player->type = 0;  // Player type
    }
    
    // Create initial enemies
    for (int i = 0; i < 5; i++) {
        Sprite* enemy_sprite = sprite_create(enemy_art, 3, (Color){255, 0, 0});
        GameObject* enemy = game_object_create(5 + i * 8, 5, enemy_sprite);
        if (enemy) {
            enemy->update = enemy_update;
            enemy->collide = bullet_collide;
            enemy->type = 2;  // Enemy type
            enemy->health = 30;
        }
    }
}

void game_scene_update(float delta_time) {
    // Spawn random enemies
    if (rand() % 100 < 5) {  // 5% chance per frame
        int x = rand() % (SCREEN_WIDTH - 5);
        Sprite* enemy_sprite = sprite_create(enemy_art, 3, (Color){255, 0, 0});
        GameObject* enemy = game_object_create(x, 0, enemy_sprite);
        if (enemy) {
            enemy->update = enemy_update;
            enemy->collide = bullet_collide;
            enemy->type = 2;
            enemy->velocity.y = 1;
            enemy->health = 30;
        }
    }
    
    // Update all game objects
    game_objects_update();
    
    // Check collisions
    game_objects_check_collisions();
}

void game_scene_render() {
    console_clear();
    
    // Draw border
    Color border_color = {128, 128, 128};
    for (int x = 0; x < SCREEN_WIDTH; x++) {
        console_set_pixel(x, 0, '-', border_color);
        console_set_pixel(x, SCREEN_HEIGHT - 1, '-', border_color);
    }
    for (int y = 0; y < SCREEN_HEIGHT; y++) {
        console_set_pixel(0, y, '|', border_color);
        console_set_pixel(SCREEN_WIDTH - 1, y, '|', border_color);
    }
    
    // Draw game objects
    game_objects_render();
    
    // Draw HUD
    char score_text[32];
    sprintf(score_text, "Score: %d", game_state.frame_count);
    Color white = {255, 255, 255};
    console_draw_text(score_text, 2, 1, white);
    
    console_flip();
}

void game_scene_cleanup() {
    // Clean up all game objects
    while (game_state.object_count > 0) {
        GameObject* obj = game_state.game_objects[0];
        if (obj->sprite) {
            sprite_destroy(obj->sprite);
        }
        game_object_destroy(obj);
    }
}

// ==================== MAIN GAME LOOP ====================

/**
 * Main game loop
 */
void game_run() {
    clock_t last_time = clock();
    float delta_time = 0;
    
    // Create main game scene
    Scene main_scene = {
        .name = "MainGame",
        .init = game_scene_init,
        .update = game_scene_update,
        .render = game_scene_render,
        .cleanup = game_scene_cleanup,
        .next = NULL
    };
    
    // Set initial scene
    scene_change(&main_scene);
    
    // Main loop
    while (game_state.running) {
        clock_t current_time = clock();
        delta_time = (float)(current_time - last_time) / CLOCKS_PER_SEC;
        last_time = current_time;
        
        // Update input
        input_update();
        
        // Check for exit
        if (input_key_pressed(KEY_ESC)) {
            game_state.running = false;
        }
        
        // Update game if not paused
        if (!game_state.paused && game_state.current_scene) {
            if (game_state.current_scene->update) {
                game_state.current_scene->update(delta_time);
            }
        }
        
        // Render
        if (game_state.current_scene && game_state.current_scene->render) {
            game_state.current_scene->render();
        }
        
        // Frame rate control
        DWORD frame_time = GetTickCount();
        DWORD elapsed = frame_time - last_time;
        if (elapsed < FRAME_TIME) {
            Sleep(FRAME_TIME - elapsed);
        }
        
        game_state.frame_count++;
    }
}

// ==================== ENTRY POINT ====================

int main() {
    printf("ARROW tv 3A Game Kernel\n");
    printf("Loading...\n");
    
    // Initialize game
    game_init();
    
    // Run game
    game_run();
    
    // Cleanup
    game_shutdown();
    
    printf("\nGame ended. Press any key to exit...\n");
    getchar();
    
    return 0;
}