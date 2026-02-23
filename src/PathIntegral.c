/**
 * ARROW tv 3A Game Kernel – Complete with Path Integral Module
 * 
 * This program demonstrates a retro console game engine with two scenes:
 * 1) A classic shooter game (player vs enemies)
 * 2) A path integral visualizer where you draw a path on a scalar field
 * 
 * Controls:
 *   - Menu: 1 or 2 to select scene, ESC to quit
 *   - Shooter: Arrow keys move, Space shoots, ESC exits to menu
 *   - Path Integral: Arrow keys move cursor, Space adds point,
 *                    C clears path, I toggles integral display, ESC to menu
 * 
 * Compile on Windows (MinGW or Visual Studio):
 *   gcc -o arrow_tv arrow_tv.c -lwinmm -lgdi32
 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdbool.h>
#include <time.h>
#include <math.h>
#include <conio.h>
#include <windows.h>

// ==================== CONSTANTS & MACROS ====================

#define SCREEN_WIDTH 80
#define SCREEN_HEIGHT 25
#define MAX_SPRITES 100
#define MAX_SOUNDS 10
#define MAX_GAME_OBJECTS 50
#define MAX_INPUT_BUFFER 256
#define FRAME_RATE 60
#define FRAME_TIME (1000 / FRAME_RATE)

// Color definitions (console attributes)
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

// Vector2 structure
typedef struct {
    int x;
    int y;
} Vector2;

// Color structure (RGB)
typedef struct {
    byte r;
    byte g;
    byte b;
} Color;

// Sprite structure
typedef struct {
    char* data;      // ASCII art
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

// Game state
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

void console_init() {
    console_handle = GetStdHandle(STD_OUTPUT_HANDLE);
    GetConsoleScreenBufferInfo(console_handle, &console_info);
    
    console_buffer = (CHAR_INFO*)malloc(sizeof(CHAR_INFO) * SCREEN_WIDTH * SCREEN_HEIGHT);
    
    CONSOLE_CURSOR_INFO cursor_info;
    GetConsoleCursorInfo(console_handle, &cursor_info);
    cursor_info.bVisible = FALSE;
    SetConsoleCursorInfo(console_handle, &cursor_info);
}

void console_clear() {
    for (int i = 0; i < SCREEN_WIDTH * SCREEN_HEIGHT; i++) {
        console_buffer[i].Char.AsciiChar = ' ';
        console_buffer[i].Attributes = COLOR_WHITE;
    }
}

void console_set_pixel(int x, int y, char c, Color color) {
    if (x >= 0 && x < SCREEN_WIDTH && y >= 0 && y < SCREEN_HEIGHT) {
        int index = y * SCREEN_WIDTH + x;
        console_buffer[index].Char.AsciiChar = c;
        
        byte attr = 0;
        if (color.r > 128) attr |= FOREGROUND_RED;
        if (color.g > 128) attr |= FOREGROUND_GREEN;
        if (color.b > 128) attr |= FOREGROUND_BLUE;
        
        console_buffer[index].Attributes = attr;
    }
}

void console_draw_sprite(Sprite* sprite, int x, int y) {
    if (!sprite || !sprite->is_visible) return;
    
    for (int sy = 0; sy < sprite->height; sy++) {
        for (int sx = 0; sx < sprite->width; sx++) {
            int index = sy * sprite->width + sx;
            if (sprite->data[index] != ' ') {
                console_set_pixel(x + sx, y + sy, sprite->data[index], sprite->color);
            }
        }
    }
}

void console_draw_text(const char* text, int x, int y, Color color) {
    int len = strlen(text);
    for (int i = 0; i < len; i++) {
        console_set_pixel(x + i, y, text[i], color);
    }
}

void console_flip() {
    COORD buffer_size = {SCREEN_WIDTH, SCREEN_HEIGHT};
    COORD buffer_coord = {0, 0};
    SMALL_RECT write_region = {0, 0, SCREEN_WIDTH - 1, SCREEN_HEIGHT - 1};
    
    WriteConsoleOutput(console_handle, console_buffer, buffer_size, 
                      buffer_coord, &write_region);
}

void console_cleanup() {
    free(console_buffer);
    
    CONSOLE_CURSOR_INFO cursor_info;
    GetConsoleCursorInfo(console_handle, &cursor_info);
    cursor_info.bVisible = TRUE;
    SetConsoleCursorInfo(console_handle, &cursor_info);
}

// ==================== INPUT FUNCTIONS ====================

void input_update() {
    for (int i = 0; i < 256; i++) {
        game_state.input.keys[i] = (GetAsyncKeyState(i) & 0x8000) != 0;
    }
    
    POINT mouse_point;
    GetCursorPos(&mouse_point);
    ScreenToClient(GetConsoleWindow(), &mouse_point);
    game_state.input.mouse_pos.x = mouse_point.x;
    game_state.input.mouse_pos.y = mouse_point.y;
    
    game_state.input.mouse_buttons[0] = (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0;
    game_state.input.mouse_buttons[1] = (GetAsyncKeyState(VK_RBUTTON) & 0x8000) != 0;
    game_state.input.mouse_buttons[2] = (GetAsyncKeyState(VK_MBUTTON) & 0x8000) != 0;
}

bool input_key_pressed(int key) {
    return game_state.input.keys[key];
}

// ==================== GAME OBJECT FUNCTIONS ====================

GameObject* game_object_create(int x, int y, Sprite* sprite) {
    if (game_state.object_count >= MAX_GAME_OBJECTS) return NULL;
    
    GameObject* obj = (GameObject*)malloc(sizeof(GameObject));
    if (!obj) return NULL;
    
    obj->position.x = x;
    obj->position.y = y;
    obj->velocity.x = 0;
    obj->velocity.y = 0;
    obj->sprite = sprite;
    obj->active = true;
    obj->type = 0;
    obj->health = 100;
    obj->update = NULL;
    obj->render = NULL;
    obj->collide = NULL;
    
    game_state.game_objects[game_state.object_count++] = obj;
    return obj;
}

void game_object_destroy(GameObject* obj) {
    for (int i = 0; i < game_state.object_count; i++) {
        if (game_state.game_objects[i] == obj) {
            free(obj);
            for (int j = i; j < game_state.object_count - 1; j++) {
                game_state.game_objects[j] = game_state.game_objects[j + 1];
            }
            game_state.object_count--;
            break;
        }
    }
}

void game_objects_update() {
    for (int i = 0; i < game_state.object_count; i++) {
        GameObject* obj = game_state.game_objects[i];
        if (obj->active) {
            obj->position.x += obj->velocity.x;
            obj->position.y += obj->velocity.y;
            if (obj->update) obj->update(obj);
        }
    }
}

void game_objects_check_collisions() {
    for (int i = 0; i < game_state.object_count; i++) {
        for (int j = i + 1; j < game_state.object_count; j++) {
            GameObject* a = game_state.game_objects[i];
            GameObject* b = game_state.game_objects[j];
            
            if (a->active && b->active && a->sprite && b->sprite) {
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

void sound_play(int frequency, int duration) {
    Beep(frequency, duration);
}

void sound_init() {
    for (int i = 0; i < MAX_SOUNDS; i++) {
        game_state.sounds[i].is_playing = false;
    }
}

// ==================== SCENE FUNCTIONS ====================

void scene_change(Scene* new_scene) {
    if (game_state.current_scene && game_state.current_scene->cleanup) {
        game_state.current_scene->cleanup();
    }
    
    game_state.current_scene = new_scene;
    
    if (game_state.current_scene && game_state.current_scene->init) {
        game_state.current_scene->init();
    }
}

// ==================== SPRITE FUNCTIONS ====================

Sprite* sprite_create(const char* art[], int lines, Color color) {
    Sprite* sprite = (Sprite*)malloc(sizeof(Sprite));
    if (!sprite) return NULL;
    
    sprite->height = lines;
    sprite->width = 0;
    for (int i = 0; i < lines; i++) {
        int len = strlen(art[i]);
        if (len > sprite->width) sprite->width = len;
    }
    
    sprite->data = (char*)malloc(sprite->width * sprite->height);
    if (!sprite->data) {
        free(sprite);
        return NULL;
    }
    
    memset(sprite->data, ' ', sprite->width * sprite->height);
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

void sprite_destroy(Sprite* sprite) {
    if (sprite) {
        if (sprite->data) free(sprite->data);
        free(sprite);
    }
}

// ==================== EXAMPLE SHOOTER GAME ====================

// Sprites
const char* player_art[] = { " O ", "/|\\", "/ \\" };
const char* enemy_art[] =   { "\\O/", " | ", "/ \\" };
const char* bullet_art[] =  { "!" };

// Player update
void player_update(GameObject* self) {
    if (input_key_pressed(KEY_LEFT) && self->position.x > 0) self->position.x -= 2;
    if (input_key_pressed(KEY_RIGHT) && self->position.x < SCREEN_WIDTH - 3) self->position.x += 2;
    if (input_key_pressed(KEY_UP) && self->position.y > 0) self->position.y -= 1;
    if (input_key_pressed(KEY_DOWN) && self->position.y < SCREEN_HEIGHT - 3) self->position.y += 1;
    
    static int shoot_timer = 0;
    if (input_key_pressed(KEY_SPACE) && shoot_timer <= 0) {
        GameObject* bullet = game_object_create(
            self->position.x + 1, self->position.y - 1,
            sprite_create(bullet_art, 1, (Color){255,255,255})
        );
        if (bullet) {
            bullet->velocity.y = -2;
            bullet->type = 1;  // player bullet
            sound_play(800, 50);
        }
        shoot_timer = 10;
    }
    if (shoot_timer > 0) shoot_timer--;
}

// Enemy update
void enemy_update(GameObject* self) {
    self->position.y += 1;
    if (self->position.y >= SCREEN_HEIGHT) self->active = false;
}

// Bullet update
void bullet_update(GameObject* self) {
    if (self->position.y < 0 || self->position.y >= SCREEN_HEIGHT) self->active = false;
}

// Collision: bullet vs enemy
void bullet_collide(GameObject* self, GameObject* other) {
    if (self->type == 1 && other->type == 2) {
        other->health -= 10;
        self->active = false;
        if (other->health <= 0) {
            other->active = false;
            sound_play(400, 100);
        }
    }
}

// Shooter scene
void shooter_scene_init() {
    // Player
    Sprite* player_sprite = sprite_create(player_art, 3, (Color){0,255,0});
    GameObject* player = game_object_create(SCREEN_WIDTH/2, SCREEN_HEIGHT-5, player_sprite);
    if (player) {
        player->update = player_update;
        player->type = 0;
    }
    
    // Initial enemies
    for (int i = 0; i < 5; i++) {
        Sprite* enemy_sprite = sprite_create(enemy_art, 3, (Color){255,0,0});
        GameObject* enemy = game_object_create(5 + i*8, 5, enemy_sprite);
        if (enemy) {
            enemy->update = enemy_update;
            enemy->collide = bullet_collide;
            enemy->type = 2;
            enemy->health = 30;
        }
    }
}

void shooter_scene_update(float delta_time) {
    // Random enemy spawn
    if (rand() % 100 < 5) {
        int x = rand() % (SCREEN_WIDTH - 5);
        Sprite* enemy_sprite = sprite_create(enemy_art, 3, (Color){255,0,0});
        GameObject* enemy = game_object_create(x, 0, enemy_sprite);
        if (enemy) {
            enemy->update = enemy_update;
            enemy->collide = bullet_collide;
            enemy->type = 2;
            enemy->velocity.y = 1;
            enemy->health = 30;
        }
    }
    
    game_objects_update();
    game_objects_check_collisions();
}

void shooter_scene_render() {
    console_clear();
    
    // Border
    Color border = {128,128,128};
    for (int x = 0; x < SCREEN_WIDTH; x++) {
        console_set_pixel(x, 0, '-', border);
        console_set_pixel(x, SCREEN_HEIGHT-1, '-', border);
    }
    for (int y = 0; y < SCREEN_HEIGHT; y++) {
        console_set_pixel(0, y, '|', border);
        console_set_pixel(SCREEN_WIDTH-1, y, '|', border);
    }
    
    game_objects_render();
    
    char score[32];
    sprintf(score, "Score: %d", game_state.frame_count);
    Color white = {255,255,255};
    console_draw_text(score, 2, 1, white);
    
    console_flip();
}

void shooter_scene_cleanup() {
    while (game_state.object_count > 0) {
        GameObject* obj = game_state.game_objects[0];
        if (obj->sprite) sprite_destroy(obj->sprite);
        game_object_destroy(obj);
    }
}

// ==================== PATH INTEGRAL MODULE ====================

#define FIELD_SIZE_X 60
#define FIELD_SIZE_Y 20
#define MAX_PATH_LENGTH 200
#define FIELD_OFFSET_X 10
#define FIELD_OFFSET_Y 3

typedef struct {
    float value;
    Color color;
} FieldCell;

static FieldCell field[FIELD_SIZE_Y][FIELD_SIZE_X];
static Vector2 path[MAX_PATH_LENGTH];
static int path_length = 0;
static float integral_sum = 0.0f;
static bool show_integral = true;
static Vector2 cursor_pos = {FIELD_SIZE_X/2, FIELD_SIZE_Y/2};
static int move_cooldown = 0;

void field_init() {
    for (int y = 0; y < FIELD_SIZE_Y; y++) {
        for (int x = 0; x < FIELD_SIZE_X; x++) {
            float nx = (float)x / FIELD_SIZE_X - 0.5f;
            float ny = (float)y / FIELD_SIZE_Y - 0.5f;
            
            float gaussian = expf(-(nx*nx + ny*ny) * 10.0f);
            float wave = sinf(nx * 10.0f) * 0.5f + sinf(ny * 15.0f) * 0.3f;
            field[y][x].value = gaussian * 2.0f + wave;
            
            float val = field[y][x].value;
            if (val < -1.0f) val = -1.0f;
            if (val > 1.0f) val = 1.0f;
            val = (val + 1.0f) * 0.5f;
            
            field[y][x].color.r = (byte)(255 * val);
            field[y][x].color.g = (byte)(255 * (1.0f - fabs(val - 0.5f) * 2.0f));
            field[y][x].color.b = (byte)(255 * (1.0f - val));
        }
    }
}

void path_compute_integral() {
    integral_sum = 0.0f;
    for (int i = 0; i < path_length; i++) {
        int x = path[i].x;
        int y = path[i].y;
        if (x >= 0 && x < FIELD_SIZE_X && y >= 0 && y < FIELD_SIZE_Y) {
            integral_sum += field[y][x].value;
        }
    }
}

void path_add_point(int x, int y) {
    if (x < 0 || x >= FIELD_SIZE_X || y < 0 || y >= FIELD_SIZE_Y) return;
    if (path_length > 0 && path[path_length-1].x == x && path[path_length-1].y == y) return;
    if (path_length < MAX_PATH_LENGTH) {
        path[path_length].x = x;
        path[path_length].y = y;
        path_length++;
        path_compute_integral();
    }
}

void path_clear() {
    path_length = 0;
    integral_sum = 0.0f;
}

void field_render() {
    for (int y = 0; y < FIELD_SIZE_Y; y++) {
        for (int x = 0; x < FIELD_SIZE_X; x++) {
            float val = fabs(field[y][x].value);
            char c;
            if (val < 0.2) c = ' ';
            else if (val < 0.4) c = '.';
            else if (val < 0.6) c = '-';
            else if (val < 0.8) c = '+';
            else if (val < 1.0) c = '*';
            else if (val < 1.5) c = '#';
            else c = '@';
            
            console_set_pixel(FIELD_OFFSET_X + x, FIELD_OFFSET_Y + y, c, field[y][x].color);
        }
    }
}

void path_render() {
    for (int i = 0; i < path_length; i++) {
        int x = path[i].x;
        int y = path[i].y;
        if (x >= 0 && x < FIELD_SIZE_X && y >= 0 && y < FIELD_SIZE_Y) {
            char c = (i == 0) ? 'X' : 'O';
            Color white = {255,255,255};
            console_set_pixel(FIELD_OFFSET_X + x, FIELD_OFFSET_Y + y, c, white);
        }
    }
}

void ui_render() {
    Color white = {255,255,255};
    Color yellow = {255,255,0};
    
    char buffer[64];
    if (show_integral) {
        sprintf(buffer, "Path Integral: %.3f", integral_sum);
        console_draw_text(buffer, 2, 1, yellow);
    } else {
        console_draw_text("Path Integral: (hidden)", 2, 1, yellow);
    }
    
    console_draw_text("ARROW KEYS: move cursor", 2, 23, white);
    console_draw_text("SPACE: add point   C: clear   I: toggle integral", 2, 24, white);
    console_draw_text("ESC: exit to menu", 2, 25, white);
    
    // Draw cursor (using global cursor_pos)
    Color cyan = {0,255,255};
    console_set_pixel(FIELD_OFFSET_X + cursor_pos.x, FIELD_OFFSET_Y + cursor_pos.y, '+', cyan);
}

void path_integral_scene_init() {
    field_init();
    path_clear();
    cursor_pos.x = FIELD_SIZE_X/2;
    cursor_pos.y = FIELD_SIZE_Y/2;
    move_cooldown = 0;
    show_integral = true;
}

void path_integral_scene_update(float delta_time) {
    if (move_cooldown <= 0) {
        if (input_key_pressed(KEY_LEFT) && cursor_pos.x > 0) {
            cursor_pos.x--;
            move_cooldown = 5;
        }
        if (input_key_pressed(KEY_RIGHT) && cursor_pos.x < FIELD_SIZE_X-1) {
            cursor_pos.x++;
            move_cooldown = 5;
        }
        if (input_key_pressed(KEY_UP) && cursor_pos.y > 0) {
            cursor_pos.y--;
            move_cooldown = 5;
        }
        if (input_key_pressed(KEY_DOWN) && cursor_pos.y < FIELD_SIZE_Y-1) {
            cursor_pos.y++;
            move_cooldown = 5;
        }
    } else {
        move_cooldown--;
    }
    
    if (input_key_pressed(KEY_SPACE)) {
        path_add_point(cursor_pos.x, cursor_pos.y);
        Sleep(100);
    }
    
    if (input_key_pressed('C') || input_key_pressed('c')) {
        path_clear();
        Sleep(200);
    }
    
    if (input_key_pressed('I') || input_key_pressed('i')) {
        show_integral = !show_integral;
        Sleep(200);
    }
    
    // ESC is handled in main loop to return to menu
}

void path_integral_scene_render() {
    console_clear();
    field_render();
    path_render();
    ui_render();
    console_flip();
}

void path_integral_scene_cleanup() {
    // Nothing to clean
}

// ==================== MENU SCENE ====================

void menu_scene_init() {
    // No special init needed
}

void menu_scene_update(float delta_time) {
    if (input_key_pressed('1')) {
        scene_change(&(Scene){
            .name = "Shooter",
            .init = shooter_scene_init,
            .update = shooter_scene_update,
            .render = shooter_scene_render,
            .cleanup = shooter_scene_cleanup
        });
        Sleep(300);
    }
    if (input_key_pressed('2')) {
        scene_change(&(Scene){
            .name = "PathIntegral",
            .init = path_integral_scene_init,
            .update = path_integral_scene_update,
            .render = path_integral_scene_render,
            .cleanup = path_integral_scene_cleanup
        });
        Sleep(300);
    }
    if (input_key_pressed(KEY_ESC)) {
        game_state.running = false;
    }
}

void menu_scene_render() {
    console_clear();
    Color title_color = {255,255,0};
    Color option_color = {0,255,255};
    
    console_draw_text("ARROW tv 3A Game Kernel", 28, 5, title_color);
    console_draw_text("=========================", 25, 6, title_color);
    
    console_draw_text("Select a scene:", 30, 10, option_color);
    console_draw_text("1 - Shooter Game", 30, 12, option_color);
    console_draw_text("2 - Path Integral Visualizer", 30, 13, option_color);
    console_draw_text("ESC - Exit", 30, 15, option_color);
    
    console_flip();
}

void menu_scene_cleanup() {
    // Nothing to clean
}

// ==================== GAME LOOP & MAIN ====================

void game_init() {
    console_init();
    sound_init();
    game_state.running = true;
    game_state.paused = false;
    game_state.frame_count = 0;
    game_state.object_count = 0;
    srand(time(NULL));
}

void game_shutdown() {
    while (game_state.object_count > 0) {
        game_object_destroy(game_state.game_objects[0]);
    }
    console_cleanup();
}

void game_run() {
    clock_t last_time = clock();
    
    while (game_state.running) {
        clock_t current_time = clock();
        float delta_time = (float)(current_time - last_time) / CLOCKS_PER_SEC;
        last_time = current_time;
        
        input_update();
        
        if (!game_state.paused && game_state.current_scene) {
            if (game_state.current_scene->update)
                game_state.current_scene->update(delta_time);
        }
        
        if (game_state.current_scene && game_state.current_scene->render)
            game_state.current_scene->render();
        
        DWORD frame_time = GetTickCount();
        DWORD elapsed = frame_time - last_time;
        if (elapsed < FRAME_TIME) {
            Sleep(FRAME_TIME - elapsed);
        }
        
        game_state.frame_count++;
    }
}

int main() {
    printf("ARROW tv 3A Game Kernel - Loading...\n");
    game_init();
    
    // Create menu scene
    Scene menu_scene = {
        .name = "Menu",
        .init = menu_scene_init,
        .update = menu_scene_update,
        .render = menu_scene_render,
        .cleanup = menu_scene_cleanup
    };
    
    scene_change(&menu_scene);
    game_run();
    
    game_shutdown();
    printf("\nGame ended. Press any key to exit...\n");
    getchar();
    return 0;
}