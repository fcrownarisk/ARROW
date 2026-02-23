"""
ARROW: Starling City Open 3D World Framework
Python + ModernGL + Pygame
Author: Generated per user request
"""

import pygame
import moderngl
import numpy as np
import glm
from pygame.locals import *

# ============================================
# 1. STARLING CITY WORLD GENERATOR
# ============================================

class StarlingCity:
    """Procedurally generates the Arrowverse open world environment"""
    
    def __init__(self, ctx):
        self.ctx = ctx
        self.buildings = []
        self.streets = []
        self.lampposts = []
        self._generate_city()
    
    def _generate_city(self):
        """Creates the Glades + Starling City downtown"""
        # Queen Consolidated Tower (centerpiece)
        self.buildings.append({
            'pos': (0, 0, 0),
            'scale': (15, 60, 15),
            'color': (0.2, 0.2, 0.25),
            'type': 'tower'
        })
        
        # The Glades - dense, run-down district
        for x in range(-50, 50, 8):
            for z in range(-50, 50, 8):
                if abs(x) < 10 and abs(z) < 10:  # Skip center for tower
                    continue
                height = np.random.randint(8, 25)
                self.buildings.append({
                    'pos': (x, height/2, z),
                    'scale': (6, height, 6),
                    'color': (0.35, 0.3, 0.3),
                    'type': 'tenement'
                })
        
        # Iron Heights / Industrial district
        for x in range(-80, -30, 12):
            for z in range(-40, 40, 12):
                self.buildings.append({
                    'pos': (x, 12, z),
                    'scale': (10, 24, 10),
                    'color': (0.4, 0.35, 0.4),
                    'type': 'industrial'
                })
        
        # Street grid with ambient occlusion shadows
        for i in range(-100, 100, 15):
            self.streets.append({'start': (i, 0.1, -100), 'end': (i, 0.1, 100)})
            self.streets.append({'start': (-100, 0.1, i), 'end': (100, 0.1, i)})
    
    def render(self, prog, camera_matrix):
        """Render all city geometry with shader"""
        # Simplified - full ModernGL implementation would batch render
        pass


# ============================================
# 2. OLIVER QUEEN PLAYER CONTROLLER
# ============================================

class OliverQueen:
    """First-person/third-person survivor of the Gambit"""
    
    def __init__(self):
        # Position - starts at the wreckage of Queen's Gambit (docks)
        self.position = glm.vec3(20.0, 2.0, 30.0)
        self.rotation = glm.vec2(0.0, 0.0)  # yaw, pitch
        self.velocity = glm.vec3(0.0, 0.0, 0.0)
        
        # Arrow TV show authentic attributes
        self.hood_raised = False
        self.bow_drawn = False
        self.current_arrow = 'standard'  # options: standard, explosive, grapple
        self.quiver = 40
        self.is_running = False
        
        # Movement physics
        self.speed = 8.0
        self.sprint_multiplier = 1.8
        self.jump_force = 12.0
        self.gravity = 28.0
        self.on_ground = True
    
    def handle_input(self, keys, dt):
        """Arrow-style movement: tactical, grounded, lethal"""
        # Forward/backward (WASD)
        move = glm.vec3(0.0)
        if keys[K_w]: move.z -= 1.0
        if keys[K_s]: move.z += 1.0
        if keys[K_a]: move.x -= 1.0
        if keys[K_d]: move.x += 1.0
        
        if glm.length(move) > 0:
            move = glm.normalize(move)
        
        # Sprint - Oliver's combat run
        current_speed = self.speed
        if keys[K_LSHIFT] and self.on_ground:
            current_speed *= self.sprint_multiplier
            self.is_running = True
        else:
            self.is_running = False
        
        # Apply movement in camera direction
        camera_dir = glm.vec3(
            glm.sin(self.rotation.x),
            0,
            glm.cos(self.rotation.x)
        )
        camera_right = glm.normalize(glm.cross(camera_dir, glm.vec3(0, 1, 0)))
        
        move_vector = (camera_dir * move.z + camera_right * move.x) * current_speed * dt
        self.velocity.x = move_vector.x
        self.velocity.z = move_vector.z
        
        # Jump
        if keys[K_SPACE] and self.on_ground:
            self.velocity.y = self.jump_force
            self.on_ground = False
        
        # Gravity
        self.velocity.y -= self.gravity * dt
        self.position += self.velocity * dt
        
        # Ground collision
        if self.position.y < 2.0:
            self.position.y = 2.0
            self.velocity.y = 0
            self.on_ground = True
    
    def aim_bow(self, mouse_dx, mouse_dy, sensitivity=0.15):
        """Mouse look for precise archery"""
        self.rotation.x += mouse_dx * sensitivity
        self.rotation.y += mouse_dy * sensitivity
        self.rotation.y = max(-1.4, min(1.4, self.rotation.y))  # Clamp vertical
        
    def release_arrow(self):
        """Fire an arrow with physics trajectory"""
        if self.quiver <= 0:
            return None
            
        self.quiver -= 1
        
        # Calculate arrow direction based on aim
        direction = glm.vec3(
            glm.sin(self.rotation.x) * glm.cos(self.rotation.y),
            glm.sin(self.rotation.y),
            glm.cos(self.rotation.x) * glm.cos(self.rotation.y)
        )
        
        # Create arrow entity
        arrow = {
            'position': self.position + glm.vec3(0, 1.8, 0) + direction * 0.5,
            'velocity': direction * 45.0,  # Fast arrow speed
            'gravity': 9.8,
            'lifetime': 5.0,
            'type': self.current_arrow
        }
        
        return arrow


# ============================================
# 3. ARROW PHYSICS & COMBAT SYSTEM
# ============================================

class ArrowPhysics:
    """Authentic archery simulation - Oliver's signature"""
    
    @staticmethod
    def update_arrow(arrow, dt):
        """Apply physics to arrow in flight"""
        arrow['velocity'].y -= arrow['gravity'] * dt
        arrow['position'] += arrow['velocity'] * dt
        arrow['lifetime'] -= dt
        return arrow['lifetime'] > 0
    
    @staticmethod
    def render_arrow_trail(arrow):
        """Draw the signature green blur effect"""
        # Implementation would create line segments
        pass


# ============================================
# 4. MAIN ENGINE INITIALIZATION
# ============================================

def main():
    """Initialize Pygame, ModernGL, and run the Arrow open world"""
    
    # Pygame setup
    pygame.init()
    pygame.display.set_mode((1280, 720), DOUBLEBUF | OPENGL)
    pygame.display.set_caption("ARROW: Starling City - Open World Framework")
    pygame.event.set_grab(True)
    pygame.mouse.set_visible(False)
    
    # ModernGL context
    ctx = moderngl.create_context()
    
    # Compile shaders (OpenGL 3.3+)
    vertex_shader = '''
    #version 330
    uniform mat4 camera;
    in vec3 in_position;
    in vec3 in_color;
    out vec3 v_color;
    void main() {
        gl_Position = camera * vec4(in_position, 1.0);
        v_color = in_color;
    }
    '''
    
    fragment_shader = '''
    #version 330
    in vec3 v_color;
    out vec4 f_color;
    void main() {
        f_color = vec4(v_color, 1.0);
    }
    '''
    
    prog = ctx.program(
        vertex_shader=vertex_shader,
        fragment_shader=fragment_shader
    )
    
    # Initialize world and player
    starling_city = StarlingCity(ctx)
    oliver = OliverQueen()
    arrows = []
    
    # Camera matrices
    proj = glm.perspective(glm.radians(65.0), 1280/720, 0.1, 500.0)
    
    clock = pygame.time.Clock()
    running = True
    
    # ========================================
    # 5. GAME LOOP - STARLING CITY NIGHT PATROL
    # ========================================
    
    while running:
        dt = clock.tick(60) / 1000.0  # Delta time (seconds)
        
        # Event handling
        for event in pygame.event.get():
            if event.type == QUIT:
                running = False
            elif event.type == KEYDOWN:
                if event.key == K_ESCAPE:
                    running = False
                elif event.key == K_f:
                    oliver.hood_raised = not oliver.hood_raised
                elif event.key == K_r:
                    # Reload quiver (find arrows in environment)
                    oliver.quiver += 5
            elif event.type == MOUSEMOTION:
                # Aim bow with mouse
                oliver.aim_bow(event.rel[0], event.rel[1])
            elif event.type == MOUSEBUTTONDOWN:
                if event.button == 1:  # Left click - fire
                    arrow = oliver.release_arrow()
                    if arrow:
                        arrows.append(arrow)
        
        # Continuous input
        keys = pygame.key.get_pressed()
        oliver.handle_input(keys, dt)
        
        # Update arrows
        arrows = [a for a in arrows if ArrowPhysics.update_arrow(a, dt)]
        
        # Camera view (first-person)
        view = glm.lookAt(
            oliver.position + glm.vec3(0, 1.8, 0),  # Eye level
            oliver.position + glm.vec3(0, 1.8, 0) + glm.vec3(
                glm.sin(oliver.rotation.x) * glm.cos(oliver.rotation.y),
                glm.sin(oliver.rotation.y),
                glm.cos(oliver.rotation.x) * glm.cos(oliver.rotation.y)
            ),
            glm.vec3(0, 1, 0)
        )
        
        camera_matrix = proj * view
        
        # Clear screen (gritty night vision)
        ctx.clear(0.08, 0.1, 0.15, 1.0)  # Dark blue-black - Starling City night
        
        # Render city
        prog['camera'].write(camera_matrix)
        
        # This is where you'd batch render all geometry
        # Full ModernGL VAO/VBO implementation would go here
        
        pygame.display.flip()
    
    pygame.quit()

if __name__ == '__main__':
    main()