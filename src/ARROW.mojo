from math import sqrt, exp, sin, cos, tan, atan2, asin, acos, pi, abs, log, pow
from random import random, gauss, seed
from time import time, sleep
from tensor import Tensor, TensorShape
from algorithm import sort, map, reduce
from collections import List, Dict, Vector, Matrix
from sys import info

# ============================================================================
# PHYSICAL CONSTANTS
# ============================================================================

struct PhysicalConstants:
    var g: Float64 = 9.80665                     # Gravity (m/s²)
    var rho_sea_level: Float64 = 1.225           # Air density at sea level (kg/m³)
    var speed_sound: Float64 = 340.29            # Speed of sound (m/s)
    var earth_rotation: Float64 = 7.292115e-5    # Earth angular velocity (rad/s)
    var gas_constant_air: Float64 = 287.05       # Specific gas constant for air (J/kg·K)
    var lapse_rate: Float64 = 0.0065              # Temperature lapse rate (K/m)
    var sea_level_temp: Float64 = 288.15          # Sea level temperature (K)
    
    fn density_at_altitude(self, h: Float64, temp_offset: Float64 = 0.0) -> Float64:
        # International Standard Atmosphere model
        let T = self.sea_level_temp - self.lapse_rate * h + temp_offset
        let P = 101325 * (1 - self.lapse_rate * h / self.sea_level_temp) ** (self.g / (self.gas_constant_air * self.lapse_rate))
        return P / (self.gas_constant_air * T)

# ============================================================================
# ADVANCED ARROW MODEL
# ============================================================================

struct Arrow:
    # Physical properties
    var mass: Float64                           # kg
    var length: Float64                          # m
    var diameter: Float64                         # m
    var shaft_material: Material
    var tip_material: Material
    var fletching_type: FletchingType
    var nock_type: NockType
    
    # Aerodynamic coefficients
    var Cd0: Float64                              # Base drag coefficient
    var Cd_Mach_coeffs: Vector[Float64]           # Mach number correction
    var Cl_alpha: Float64                          # Lift curve slope
    var Cm_alpha: Float64                          # Pitch moment coefficient
    var Cn_beta: Float64                           # Yaw moment coefficient
    var Cl_p: Float64                              # Roll damping coefficient
    var Cm_q: Float64                               # Pitch damping coefficient
    var Cn_r: Float64                               # Yaw damping coefficient
    
    # Geometric properties
    var I_xx: Float64                               # Roll moment of inertia
    var I_yy: Float64                               # Pitch moment of inertia
    var I_zz: Float64                               # Yaw moment of inertia
    var CG_position: Float64                        # Center of gravity (from nock)
    var CP_position: Float64                        # Center of pressure (from nock)
    
    # Structural properties
    var flexural_rigidity: Float64                  # EI (N·m²)
    var torsional_rigidity: Float64                  # GJ (N·m²)
    var damping_coefficient: Float64                 # Structural damping
    
    # Fletching properties
    var fletching_count: Int
    var fletching_area: Float64                      # m² per vane
    var fletching_angle: Float64                      # Degrees offset
    var fletching_chord: Float64                      # Chord length
    
    # Tip properties
    var tip_mass: Float64
    var tip_length: Float64
    var tip_diameter: Float64
    var tip_ogive_radius: Float64
    
    fn new_standard_arrow() -> Arrow:
        return Arrow(
            mass = 0.025,
            length = 0.75,
            diameter = 0.006,
            shaft_material = Material.CARBON,
            tip_material = Material.STEEL,
            fletching_type = FletchingType.HELICAL,
            nock_type = NockType.LIGHTED,
            Cd0 = 0.4,
            Cd_Mach_coeffs = Vector[Float64]([1.0, 0.2, 0.05, 0.01]),
            Cl_alpha = 2.0 * pi,
            Cm_alpha = -0.5,
            Cn_beta = -0.5,
            Cl_p = -0.1,
            Cm_q = -0.2,
            Cn_r = -0.2,
            I_xx = 2.5e-7,
            I_yy = 1.2e-4,
            I_zz = 1.2e-4,
            CG_position = 0.4,
            CP_position = 0.55,
            flexural_rigidity = 150.0,
            torsional_rigidity = 100.0,
            damping_coefficient = 0.02,
            fletching_count = 3,
            fletching_area = 0.0015,
            fletching_angle = 3.0,
            fletching_chord = 0.03,
            tip_mass = 0.008,
            tip_length = 0.03,
            tip_diameter = 0.008,
            tip_ogive_radius = 0.02
        )
    
    fn area(self) -> Float64:
        return pi * (self.diameter * 0.5) ** 2
    
    fn reference_area(self) -> Float64:
        return self.area()
    
    fn aspect_ratio(self) -> Float64:
        return self.length / self.diameter
    
    fn volume(self) -> Float64:
        return self.area() * self.length
    
    fn average_density(self) -> Float64:
        return self.mass / self.volume()
    
    fn mach_correction(self, mach: Float64) -> Float64:
        # Pade approximant for transonic drag rise
        let numerator = self.Cd_Mach_coeffs[0] + self.Cd_Mach_coeffs[1] * mach + 
                        self.Cd_Mach_coeffs[2] * mach**2 + self.Cd_Mach_coeffs[3] * mach**3
        let denominator = 1.0 + 0.5 * mach + 0.1 * mach**2
        return numerator / denominator

# ============================================================================
# STATE VECTOR DEFINITION
# ============================================================================

struct StateVector:
    # Position
    var x: Float64
    var y: Float64
    var z: Float64
    
    # Velocity
    var vx: Float64
    var vy: Float64
    var vz: Float64
    
    # Angular position (Euler angles)
    var phi: Float64      # Roll
    var theta: Float64    # Pitch
    var psi: Float64      # Yaw
    
    # Angular velocity
    var p: Float64        # Roll rate
    var q: Float64        # Pitch rate
    var r: Float64        # Yaw rate
    
    # Structural deflection (modal amplitudes)
    var flex_mode1: Float64
    var flex_mode2: Float64
    var flex_mode3: Float64
    var flex_rate1: Float64
    var flex_rate2: Float64
    var flex_rate3: Float64
    
    fn zero() -> StateVector:
        return StateVector(
            x=0.0, y=0.0, z=0.0,
            vx=0.0, vy=0.0, vz=0.0,
            phi=0.0, theta=0.0, psi=0.0,
            p=0.0, q=0.0, r=0.0,
            flex_mode1=0.0, flex_mode2=0.0, flex_mode3=0.0,
            flex_rate1=0.0, flex_rate2=0.0, flex_rate3=0.0
        )
    
    fn to_vector(self) -> Vector[Float64]:
        return Vector[Float64]([
            self.x, self.y, self.z,
            self.vx, self.vy, self.vz,
            self.phi, self.theta, self.psi,
            self.p, self.q, self.r,
            self.flex_mode1, self.flex_mode2, self.flex_mode3,
            self.flex_rate1, self.flex_rate2, self.flex_rate3
        ])
    
    fn from_vector(v: Vector[Float64]) -> StateVector:
        return StateVector(
            x=v[0], y=v[1], z=v[2],
            vx=v[3], vy=v[4], vz=v[5],
            phi=v[6], theta=v[7], psi=v[8],
            p=v[9], q=v[10], r=v[11],
            flex_mode1=v[12], flex_mode2=v[13], flex_mode3=v[14],
            flex_rate1=v[15], flex_rate2=v[16], flex_rate3=v[17]
        )
    
    fn __add__(self, other: StateVector) -> StateVector:
        return StateVector(
            x=self.x+other.x, y=self.y+other.y, z=self.z+other.z,
            vx=self.vx+other.vx, vy=self.vy+other.vy, vz=self.vz+other.vz,
            phi=self.phi+other.phi, theta=self.theta+other.theta, psi=self.psi+other.psi,
            p=self.p+other.p, q=self.q+other.q, r=self.r+other.r,
            flex_mode1=self.flex_mode1+other.flex_mode1,
            flex_mode2=self.flex_mode2+other.flex_mode2,
            flex_mode3=self.flex_mode3+other.flex_mode3,
            flex_rate1=self.flex_rate1+other.flex_rate1,
            flex_rate2=self.flex_rate2+other.flex_rate2,
            flex_rate3=self.flex_rate3+other.flex_rate3
        )
    
    fn __mul__(self, scalar: Float64) -> StateVector:
        return StateVector(
            x=self.x*scalar, y=self.y*scalar, z=self.z*scalar,
            vx=self.vx*scalar, vy=self.vy*scalar, vz=self.vz*scalar,
            phi=self.phi*scalar, theta=self.theta*scalar, psi=self.psi*scalar,
            p=self.p*scalar, q=self.q*scalar, r=self.r*scalar,
            flex_mode1=self.flex_mode1*scalar,
            flex_mode2=self.flex_mode2*scalar,
            flex_mode3=self.flex_mode3*scalar,
            flex_rate1=self.flex_rate1*scalar,
            flex_rate2=self.flex_rate2*scalar,
            flex_rate3=self.flex_rate3*scalar
        )

# ============================================================================
# COMPREHENSIVE FORCE AND MOMENT CALCULATION
# ============================================================================

struct ForceModel:
    var constants: PhysicalConstants
    var turbulence: Optional[TurbulenceField]
    var wind_field: Optional[WindField]
    var magnetic_field: Optional[MagneticField]   # For trick arrows with magnetic tips
    
    fn compute_forces(self, state: StateVector, arrow: Arrow, time: Float64) -> Tuple[Vector[Float64], Vector[Float64]]:
        # Extract state variables
        let pos = Vector[Float64]([state.x, state.y, state.z])
        let vel = Vector[Float64]([state.vx, state.vy, state.vz])
        let omega = Vector[Float64]([state.p, state.q, state.r])
        let euler = Vector[Float64]([state.phi, state.theta, state.psi])
        
        # Get local atmospheric conditions
        let rho = self.constants.density_at_altitude(state.y)
        let wind = self.get_wind_velocity(pos, time)
        let T = self.get_temperature(state.y)
        let P = self.get_pressure(state.y)
        
        # Relative velocity (including wind)
        let vel_rel = vel - wind
        let V_rel = norm(vel_rel)
        let Mach = V_rel / self.constants.speed_sound
        
        # Angle of attack and sideslip
        let alpha = atan2(vel_rel.y, sqrt(vel_rel.x**2 + vel_rel.z**2))
        let beta = asin(vel_rel.z / V_rel) if V_rel > 1e-6 else 0.0
        
        # Compute body-axis velocities
        let vel_body = self.earth_to_body(vel_rel, euler)
        let u = vel_body.x  # Axial velocity
        let v = vel_body.y  # Lateral velocity
        let w = vel_body.z  # Normal velocity
        
        # ==================== AERODYNAMIC FORCES ====================
        
        # Dynamic pressure
        let qbar = 0.5 * rho * V_rel * V_rel
        let S_ref = arrow.reference_area()
        
        # Mach-corrected drag coefficient
        let Cd_mach = arrow.Cd0 * arrow.mach_correction(Mach)
        
        # Axial force (drag)
        let C_A = Cd_mach * cos(alpha) * cos(beta)
        let F_Ax = -qbar * S_ref * C_A
        
        # Normal force (lift)
        let C_N = arrow.Cl_alpha * sin(alpha) * cos(alpha)
        let F_Nz = -qbar * S_ref * C_N * sign(w)
        
        # Side force
        let C_Y = -arrow.Cn_beta * beta  # Simplified
        let F_Yy = qbar * S_ref * C_Y
        
        # ==================== MAGNUS FORCE ====================
        # Magnus force from spin
        let C_magnus = 0.5 * pi * arrow.diameter**2 * arrow.length / S_ref
        let F_magnus = C_magnus * qbar * (omega ^ vel_rel)
        
        # ==================== FLETCHING FORCES ====================
        var F_fletching = Vector[Float64](0.0, 0.0, 0.0)
        var M_fletching = Vector[Float64](0.0, 0.0, 0.0)
        
        for i in range(arrow.fletching_count):
            let vane_angle = arrow.fletching_angle * pi/180 + 2*pi*i/arrow.fletching_count
            let vane_normal = Vector[Float64](0.0, sin(vane_angle), cos(vane_angle))
            let vane_pos = Vector[Float64](-0.9*arrow.length, arrow.diameter*cos(vane_angle), arrow.diameter*sin(vane_angle))
            
            # Local velocity at vane
            let v_local = vel_rel + (omega ^ vane_pos)
            let v_local_mag = norm(v_local)
            
            if v_local_mag > 1e-6:
                let alpha_vane = asin(dot(v_local.normalize(), vane_normal))
                let C_vane = 2.0 * pi * alpha_vane * arrow.fletching_area
                let F_vane = 0.5 * rho * v_local_mag**2 * C_vane * vane_normal
                F_fletching += F_vane
                M_fletching += vane_pos ^ F_vane
        
        # ==================== TIP EFFECTS ====================
        # Tip vortex drag
        let tip_drag = 0.5 * rho * V_rel**2 * pi * (arrow.tip_diameter/2)**2 * 0.1
        let F_tip = -tip_drag * vel_rel.normalize()
        
        # ==================== GRAVITY ====================
        let F_gravity = Vector[Float64](0.0, -arrow.mass * self.constants.g, 0.0)
        
        # ==================== CORIOLIS FORCE ====================
        let F_coriolis = -2.0 * arrow.mass * (self.constants.earth_rotation ^ vel)
        
        # ==================== MAGNETIC FORCE (for trick arrows) ====================
        var F_magnetic = Vector[Float64](0.0, 0.0, 0.0)
        if self.magnetic_field.is_some():
            let B = self.magnetic_field.value().field_at(pos)
            let tip_moment = self.get_tip_magnetic_moment(arrow)
            F_magnetic = gradient(dot(tip_moment, B))
        
        # ==================== TOTAL FORCES ====================
        let F_total = Vector[Float64](
            F_Ax + F_magnus.x + F_fletching.x + F_tip.x + F_gravity.x + F_coriolis.x + F_magnetic.x,
            F_Yy + F_magnus.y + F_fletching.y + F_tip.y + F_gravity.y + F_coriolis.y + F_magnetic.y,
            F_Nz + F_magnus.z + F_fletching.z + F_tip.z + F_gravity.z + F_coriolis.z + F_magnetic.z
        )
        
        # ==================== AERODYNAMIC MOMENTS ====================
        
        # Pitch moment (due to angle of attack)
        let Cm = arrow.Cm_alpha * sin(alpha) + arrow.Cm_q * state.q * arrow.length / (2 * V_rel)
        let M_pitch = qbar * S_ref * arrow.length * Cm * Vector[Float64](0.0, 1.0, 0.0)
        
        # Yaw moment (due to sideslip)
        let Cn = arrow.Cn_beta * beta + arrow.Cn_r * state.r * arrow.length / (2 * V_rel)
        let M_yaw = qbar * S_ref * arrow.length * Cn * Vector[Float64](0.0, 0.0, 1.0)
        
        # Roll moment (due to spin and fletching)
        let Cl = arrow.Cl_p * state.p * arrow.diameter / (2 * V_rel)
        let M_roll = qbar * S_ref * arrow.diameter * Cl * Vector[Float64](1.0, 0.0, 0.0)
        
        # Add fletching moments
        M_roll += M_fletching
        
        # ==================== TOTAL MOMENTS ====================
        let M_total = M_pitch + M_yaw + M_roll
        
        return (F_total, M_total)

# ============================================================================
# COMPLETE DERIVATIVES FUNCTION
# ============================================================================

fn derivatives(state: StateVector, arrow: Arrow, force_model: ForceModel, time: Float64) -> StateVector:
    # Compute forces and moments
    let (F, M) = force_model.compute_forces(state, arrow, time)
    
    # Extract state
    let pos = Vector[Float64]([state.x, state.y, state.z])
    let vel = Vector[Float64]([state.vx, state.vy, state.vz])
    let omega = Vector[Float64]([state.p, state.q, state.r])
    let euler = Vector[Float64]([state.phi, state.theta, state.psi])
    
    # Translational acceleration
    let accel = F / arrow.mass
    
    # Rotational acceleration (Euler's equations for rigid body)
    # I * dω/dt + ω × (I·ω) = M
    let I = Matrix[Float64]([
        [arrow.I_xx, 0, 0],
        [0, arrow.I_yy, 0],
        [0, 0, arrow.I_zz]
    ])
    
    let I_omega = I * omega
    let omega_dot = I.inverse() * (M - (omega ^ I_omega))
    
    # Euler angle rates
    # Transformation from body rates to Euler angle rates
    let phi = state.phi
    let theta = state.theta
    let psi = state.psi
    
    let phi_dot = state.p + state.q * sin(phi) * tan(theta) + state.r * cos(phi) * tan(theta)
    let theta_dot = state.q * cos(phi) - state.r * sin(phi)
    let psi_dot = (state.q * sin(phi) + state.r * cos(phi)) / cos(theta)
    
    # Structural dynamics (modal amplitudes)
    # Simple harmonic oscillator for each mode
    let omega1 = 2*pi * 50.0  # First bending mode frequency (50 Hz)
    let omega2 = 2*pi * 150.0 # Second bending mode
    let omega3 = 2*pi * 300.0 # Third bending mode
    
    let zeta = arrow.damping_coefficient  # Damping ratio
    
    # Forcing from aerodynamics (simplified)
    let qbar = 0.5 * force_model.constants.density_at_altitude(state.y) * norm(vel)**2
    let F_flex1 = qbar * arrow.area() * 0.01 * sin(state.theta)  # Coupling with pitch
    
    let flex_ddot1 = -2*zeta*omega1*state.flex_rate1 - omega1**2*state.flex_mode1 + F_flex1/arrow.mass
    let flex_ddot2 = -2*zeta*omega2*state.flex_rate2 - omega2**2*state.flex_mode2
    let flex_ddot3 = -2*zeta*omega3*state.flex_rate3 - omega3**2*state.flex_mode3
    
    # Return derivatives
    return StateVector(
        x=vel.x, y=vel.y, z=vel.z,
        vx=accel.x, vy=accel.y, vz=accel.z,
        phi=phi_dot, theta=theta_dot, psi=psi_dot,
        p=omega_dot.x, q=omega_dot.y, r=omega_dot.z,
        flex_mode1=state.flex_rate1, flex_mode2=state.flex_rate2, flex_mode3=state.flex_rate3,
        flex_rate1=flex_ddot1, flex_rate2=flex_ddot2, flex_rate3=flex_ddot3
    )

# ============================================================================
# ADVANCED INTEGRATORS
# ============================================================================

struct Integrator:
    var method: IntegrationMethod
    var dt_min: Float64
    var dt_max: Float64
    var tolerance: Float64
    var max_iterations: Int
    
    fn integrate(self, arrow: Arrow, initial_state: StateVector, 
                 force_model: ForceModel, t_start: Float64, t_end: Float64,
                 output_callback: Optional[Fn(StateVector, Float64) -> None]) -> List[StateVector]:
        
        var trajectory = List[StateVector]()
        var state = initial_state
        var t = t_start
        var dt = self.dt_max
        
        while t < t_end:
            match self.method:
                case IntegrationMethod.RKF45:
                    # Adaptive step
                    let (new_state, new_dt, success) = self.rkf45_step(state, arrow, force_model, t, dt)
                    if success:
                        trajectory.append(new_state)
                        state = new_state
                        t += dt
                        dt = min(self.dt_max, new_dt)
                    else:
                        dt = max(self.dt_min, dt * 0.5)
                
                case IntegrationMethod.DORMAND_PRINCE:
                    # DOPRI5 method (even higher order)
                    let (new_state, new_dt, success) = self.dopri5_step(state, arrow, force_model, t, dt)
                    if success:
                        trajectory.append(new_state)
                        state = new_state
                        t += dt
                        dt = min(self.dt_max, new_dt)
                    else:
                        dt = max(self.dt_min, dt * 0.5)
                
                case IntegrationMethod.SYMPLECTIC_EULER:
                    # Symplectic method for energy conservation
                    state = self.symplectic_step(state, arrow, force_model, t, dt)
                    trajectory.append(state)
                    t += dt
            
            if output_callback.is_some():
                output_callback.value()(state, t)
        
        return trajectory
    
    fn rkf45_step(self, state: StateVector, arrow: Arrow, force_model: ForceModel,
                  t: Float64, dt: Float64) -> Tuple[StateVector, Float64, Bool]:
        
        # RKF45 coefficients (as before but with full state)
        # ... (implementation from earlier)
        
        pass

# ============================================================================
# TRICK ARROW SPECIALIZATIONS
# ============================================================================

struct TrickArrow:
    var base_arrow: Arrow
    var trick_type: TrickType
    var timer: Float64
    var proximity_sensor: Optional[ProximitySensor]
    var guidance_system: Optional[GuidanceSystem]
    var payload: Optional[Payload]

enum TrickType:
    case EXPLOSIVE
    case GRAPPLING
    case ELECTRIC
    case SMOKE
    case FLARE
    case SONIC
    case EMP
    case TRACKER
    case NET
    case BOXING_GLOVE   # Classic!

struct GuidanceSystem:
    var target_position: Optional[Vector[Float64]]
    var target_velocity: Optional[Vector[Float64]]
    var guidance_law: GuidanceLaw
    var max_accel: Float64
    var seeker_fov: Float64
    
    fn compute_guidance(self, state: StateVector, arrow: Arrow, dt: Float64) -> Vector[Float64]:
        if self.target_position.is_none():
            return Vector[Float64](0.0, 0.0, 0.0)
        
        let target_pos = self.target_position.value()
        let target_vel = self.target_velocity.value_or(Vector[Float64](0.0, 0.0, 0.0))
        
        match self.guidance_law:
            case GuidanceLaw.PROPORTIONAL_NAVIGATION:
                # Proportional navigation (PN)
                let LOS = target_pos - Vector[Float64](state.x, state.y, state.z)
                let LOS_rate = (target_vel - Vector[Float64](state.vx, state.vy, state.vz)) ^ LOS / dot(LOS, LOS)
                let N = 3.0  # Navigation constant
                let accel_cmd = N * (Vector[Float64](state.vx, state.vy, state.vz) ^ LOS_rate)
                return accel_cmd.clamp(self.max_accel)
            
            case GuidanceLaw.PURSUIT:
                # Pure pursuit
                let rel_pos = target_pos - Vector[Float64](state.x, state.y, state.z)
                return rel_pos.normalize() * self.max_accel
            
            case GuidanceLaw.AUGMENTED_PN:
                # APN with target maneuver estimation
                # ... complex implementation
                pass

# ============================================================================
# ENVIRONMENTAL MODELING
# ============================================================================

struct TurbulenceField:
    var intensity: Float64
    var scale: Float64
    var spectrum: SpectrumType
    var seed: Int
    var rng: RandomGenerator
    
    fn velocity(self, pos: Vector[Float64], time: Float64) -> Vector[Float64]:
        # Von Karman turbulence model
        let L = self.scale  # Turbulence length scale
        let sigma = self.intensity  # Turbulence intensity
        
        # Generate random phases
        let kx = 2*pi / L * pos.x
        let ky = 2*pi / L * pos.y
        let kz = 2*pi / L * pos.z
        
        # Von Karman spectrum
        let E_k = sigma**2 * (L**5 * k**4) / ( (1 + (L*k)**2)**(17/6) )
        
        # Convert to velocity field
        # ... implementation
        
        return Vector[Float64](0.0, 0.0, 0.0)

# ============================================================================
# COMPLETE SIMULATION ENGINE
# ============================================================================

struct ArrowSimulationEngine:
    var arrows: List[Arrow]
    var states: List[StateVector]
    var force_models: List[ForceModel]
    var integrator: Integrator
    var environment: Environment
    var collision_detector: CollisionDetector
    var visualization: Optional[VisualizationEngine]
    
    fn simulate_all(self, t_start: Float64, t_end: Float64, dt_out: Float64) -> Dict[Int, List[StateVector]]:
        var results = Dict[Int, List[StateVector]]()
        let n_steps = Int((t_end - t_start) / dt_out)
        
        for arrow_idx in range(len(self.arrows)):
            let arrow = self.arrows[arrow_idx]
            let state = self.states[arrow_idx]
            let force_model = self.force_models[arrow_idx]
            
            var trajectory = List[StateVector]()
            var current_state = state
            var t = t_start
            var next_output = t_start + dt_out
            
            while t < t_end:
                # Integrate to next output time
                let segment = self.integrator.integrate(
                    arrow, current_state, force_model, 
                    t, next_output, None
                )
                
                if len(segment) > 0:
                    current_state = segment[-1]
                    trajectory.append(current_state)
                    
                    # Check collisions
                    let collision = self.collision_detector.check(current_state, arrow)
                    if collision.happened:
                        self.handle_collision(arrow_idx, current_state, collision)
                        break
                
                t = next_output
                next_output += dt_out
            
            results[arrow_idx] = trajectory
        
        return results
    
    fn handle_collision(self, arrow_idx: Int, state: StateVector, collision: Collision):
        # Handle arrow impact
        let arrow = self.arrows[arrow_idx]
        
        match arrow.trick_type:
            case TrickType.EXPLOSIVE:
                self.create_explosion(collision.point, collision.normal, arrow.payload)
            
            case TrickType.GRAPPLING:
                self.create_tether(collision.point, arrow.payload)
            
            case TrickType.ELECTRIC:
                self.create_electric_arc(collision.point, arrow.payload)
            
            case TrickType.SMOKE:
                self.create_smoke_cloud(collision.point, arrow.payload)
            
            case _:
                # Standard impact
                self.create_impact_effect(collision.point, collision.normal)

# ============================================================================
# VISUALIZATION AND OUTPUT
# ============================================================================

struct TrajectoryVisualizer:
    var width: Int = 1024
    var height: Int = 768
    var camera: Camera
    var lighting: Lighting
    
    fn render_frame(self, states: List[StateVector], arrow: Arrow, time: Float64) -> Image:
        var image = Image(self.width, self.height)
        
        # Clear background
        image.clear(Color.SKY_BLUE)
        
        # Draw terrain
        self.draw_terrain(image)
        
        # Draw arrow for each state (trail)
        for i in range(len(states)):
            let state = states[i]
            let alpha = Float64(i) / len(states)  # Fade older positions
            self.draw_arrow(image, state, arrow, alpha)
        
        # Draw current position prominently
        if len(states) > 0:
            self.draw_arrow(image, states[-1], arrow, 1.0, highlight=True)
        
        # Add UI elements
        self.draw_info_panel(image, states[-1], arrow, time)
        
        return image
    
    fn draw_arrow(self, image: Image, state: StateVector, arrow: Arrow, alpha: Float64, highlight: Bool = False):
        # Project 3D position to 2D screen
        let screen_pos = self.camera.project(Vector[Float64](state.x, state.y, state.z))
        
        # Draw arrow body (oriented by Euler angles)
        let direction = Vector[Float64](
            cos(state.theta) * cos(state.psi),
            sin(state.theta),
            cos(state.theta) * sin(state.psi)
        )
        
        let screen_dir = self.camera.project_direction(direction)
        
        # Draw line for arrow
        let tip = screen_pos + screen_dir * 20.0
        let fletching = screen_pos - screen_dir * 10.0
        
        image.draw_line(tip, fletching, Color.RED.mix(Color.WHITE, alpha))
        
        # Draw fletching
        for i in range(3):
            let angle = 2*pi*i/3 + state.phi
            let vane_offset = Vector[Float64](sin(angle), cos(angle)) * 5.0
            image.draw_line(fletching, fletching + vane_offset, Color.BLUE.mix(Color.WHITE, alpha))
        
        if highlight:
            image.draw_circle(screen_pos, 5, Color.YELLOW)

# ============================================================================
# MAIN SIMULATION EXAMPLE
# ============================================================================

fn main():
    print("=" * 60)
    print("ARROW TRAJECTORY DYNAMICS - COMPLETE PHYSICS SIMULATION")
    print("Version 3.0 - The Green Arrow Edition")
    print("=" * 60)
    
    # Initialize physics constants
    let constants = PhysicalConstants()
    
    # Create standard arrow
    let arrow = Arrow.new_standard_arrow()
    
    # Create specialized trick arrows
    let explosive_arrow = TrickArrow(
        base_arrow = arrow,
        trick_type = TrickType.EXPLOSIVE,
        timer = 2.5,  # Detonate after 2.5 seconds
        proximity_sensor = None,
        guidance_system = None,
        payload = Payload(
            explosive_mass = 0.05,
            blast_radius = 5.0,
            fragment_count = 100
        )
    )
    
    let grappling_arrow = TrickArrow(
        base_arrow = arrow,
        trick_type = TrickType.GRAPPLING,
        timer = 0.0,
        proximity_sensor = None,
        guidance_system = Some(GuidanceSystem(
            target_position = Some(Vector[Float64](50.0, 30.0, 10.0)),
            target_velocity = None,
            guidance_law = GuidanceLaw.PROPORTIONAL_NAVIGATION,
            max_accel = 50.0,
            seeker_fov = 30.0 * pi/180
        )),
        payload = Payload(
            cable_length = 30.0,
            cable_strength = 5000.0,
            winch_speed = 10.0
        )
    )
    
    # Create force models with different environmental conditions
    let calm_force_model = ForceModel(
        constants = constants,
        turbulence = None,
        wind_field = Some(WindField.constant(Vector[Float64](2.0, 0.0, 0.0))),
        magnetic_field = None
    )
    
    let storm_force_model = ForceModel(
        constants = constants,
        turbulence = Some(TurbulenceField(
            intensity = 5.0,
            scale = 100.0,
            spectrum = SpectrumType.VON_KARMAN,
            seed = 42
        )),
        wind_field = Some(WindField.gusty(
            mean = Vector[Float64](15.0, 0.0, 5.0),
            gust_amplitude = 8.0,
            gust_frequency = 0.5
        )),
        magnetic_field = None
    )
    
    # Create integrator
    let integrator = Integrator(
        method = IntegrationMethod.DORMAND_PRINCE,
        dt_min = 0.0001,
        dt_max = 0.01,
        tolerance = 1e-8,
        max_iterations = 1000
    )
    
    # Initial states for different scenarios
    let state1 = StateVector.zero()
    state1.vx = 60.0 * cos(30.0 * pi/180)
    state1.vy = 60.0 * sin(30.0 * pi/180)
    state1.vz = 2.0  # Slight cross-range velocity
    
    let state2 = StateVector.zero()
    state2.vx = 80.0 * cos(45.0 * pi/180)
    state2.vy = 80.0 * sin(45.0 * pi/180)
    state2.vz = -5.0
    
    # Create simulation engine
    var engine = ArrowSimulationEngine(
        arrows = List[Arrow]([arrow, arrow, explosive_arrow.base_arrow, grappling_arrow.base_arrow]),
        states = List[StateVector]([state1, state2, state1, state2]),
        force_models = List[ForceModel]([calm_force_model, storm_force_model, calm_force_model, calm_force_model]),
        integrator = integrator,
        environment = Environment(
            gravity = constants.g,
            ground_level = 0.0,
            obstacles = List[Obstacle]([
                Obstacle.sphere(Vector[Float64](30.0, 5.0, 0.0), 2.0),
                Obstacle.box(Vector[Float64](40.0, 0.0, 10.0), Vector[Float64](5.0, 10.0, 5.0))
            ])
        ),
        collision_detector = CollisionDetector(
            method = CollisionMethod.CONTINUOUS,
            accuracy = 1e-6
        ),
        visualization = Some(VisualizationEngine(
            visualizer = TrajectoryVisualizer(),
            output_fps = 60
        ))
    )
    
    # Run simulation
    print("\n[1/4] Initializing simulation engine...")
    print(f"  - Arrows: {len(engine.arrows)}")
    print(f"  - Integration method: {integrator.method}")
    print(f"  - Time step range: {integrator.dt_min:.6f} - {integrator.dt_max:.3f} s")
    
    print("\n[2/4] Running trajectory calculations...")
    let start_time = time()
    let results = engine.simulate_all(0.0, 5.0, 0.05)  # 5 seconds, output every 0.05s
    let end_time = time()
    
    print(f"  - Simulation completed in {(end_time - start_time)*1000:.2f} ms")
    print(f"  - Trajectory points generated: {sum(len(v) for v in results.values())}")
    
    print("\n[3/4] Analyzing results...")
    for arrow_idx in range(len(engine.arrows)):
        let trajectory = results[arrow_idx]
        if len(trajectory) > 0:
            let final = trajectory[-1]
            let max_height = max(t.y for t in trajectory)
            let range = final.x
            let flight_time = len(trajectory) * 0.05
            
            print(f"\n  Arrow {arrow_idx + 1}:")
            print(f"    - Range: {range:.2f} m")
            print(f"    - Max height: {max_height:.2f} m")
            print(f"    - Flight time: {flight_time:.2f} s")
            print(f"    - Impact velocity: {sqrt(final.vx**2 + final.vy**2 + final.vz**2):.2f} m/s")
            print(f"    - Final orientation: pitch={final.theta*180/pi:.1f}°, yaw={final.psi*180/pi:.1f}°")
    
    print("\n[4/4] Generating visualization...")
    if engine.visualization.is_some():
        let viz = engine.visualization.value()
        for i in range(100):  # Generate 100 frames
            let time_idx = i * 0.05
            var frame_states = List[StateVector]()
            for trajectory in results.values():
                let idx = min(i, len(trajectory) - 1)
                if idx >= 0:
                    frame_states.append(trajectory[idx])
            
            let image = viz.visualizer.render_frame(frame_states, arrow, time_idx)
            image.save(f"frame_{i:04d}.png")
        
        print("  - 100 frames generated (frame_0000.png to frame_0099.png)")
    
    print("\n" + "=" * 60)
    print("SIMULATION COMPLETE")
    print("=" * 60)
    
    # Interactive mode
    print("\nEntering interactive analysis mode...")
    print("Commands: 'range [angle]' 'compare' 'wind' 'trick' 'quit'")
    
    while True:
        print("\n> ", end="")
        let command = input()
        
        if command == "quit":
            break
        elif command.startswith("range"):
            # Calculate range for different launch angles
            let angle = Float64(command.split()[1]) if len(command.split()) > 1 else 30.0
            let test_state = StateVector.zero()
            test_state.vx = 60.0 * cos(angle * pi/180)
            test_state.vy = 60.0 * sin(angle * pi/180)
            
            let test_traj = integrator.integrate(arrow, test_state, calm_force_model, 0.0, 10.0, None)
            let final_pos = test_traj[-1]
            print(f"Range at {angle}°: {final_pos.x:.2f} m")
        
        elif command == "compare":
            # Compare with and without wind
            print("\nCalm conditions:")
            let calm_traj = results[0]
            let calm_range = calm_traj[-1].x
            print(f"  Range: {calm_range:.2f} m")
            
            print("\nStorm conditions:")
            let storm_traj = results[1]
            let storm_range = storm_traj[-1].x
            let storm_drift = storm_traj[-1].z
            print(f"  Range: {storm_range:.2f} m")
            print(f"  Crosswind drift: {storm_drift:.2f} m")
        
        elif command == "trick":
            print("\nTrick arrow simulation:")
            print("  Explosive arrow: Detonates at t=2.5s")
            print("  Grappling arrow: Guided to target at (50,30,10)")
    
    print("\nThank you for using ARROW Trajectory Dynamics!")



struct ExplosivePayload:
    var explosive_type: ExplosiveType
    var mass: Float64
    var detonation_mechanism: DetonationMechanism
    var fragment_mass: Float64
    var fragment_count: Int
    var blast_overpressure: Float64
    
    fn detonate(self, position: Vector[Float64]) -> ExplosionEffect:
        # Calculate blast wave propagation
        let blast_radius = (self.mass * self.explosive_type.energy_density) ** (1/3)
        
        # Fragment trajectories
        var fragments = List[Fragment]()
        for i in range(self.fragment_count):
            let direction = random_sphere_direction()
            let velocity = sqrt(2 * self.explosive_type.gurney_energy / self.fragment_mass)
            fragments.append(Fragment(
                mass = self.fragment_mass,
                position = position,
                velocity = direction * velocity
            ))
        
        return ExplosionEffect(
            blast_wave = BlastWave(overpressure = self.blast_overpressure, radius = blast_radius),
            fragments = fragments,
            thermal = ThermalPulse(temperature = 3000.0, duration = 0.1)
        )


# Explosive Arrows
struct GrapplingSystem:
    var cable: Cable
    var winch: Winch
    var anchor: Optional[AnchorPoint]
    var tension: Float64
    
    fn deploy(self, arrow_state: StateVector, impact_point: Vector[Float64]) -> GrapplingState:
        self.anchor = AnchorPoint(position = impact_point, strength = self.cable.strength)
        
        # Calculate cable dynamics
        let cable_length = norm(impact_point - Vector[Float64](arrow_state.x, arrow_state.y, arrow_state.z))
        let cable_direction = (impact_point - arrow_state.position).normalize()
        
        # Apply tension to arrow
        let tension_force = self.calculate_tension(cable_length) * cable_direction
        
        # Update arrow dynamics with tension force
        return GrapplingState(
            anchored = True,
            cable_length = cable_length,
            tension = tension_force,
            winch_status = self.winch.status
        )


# Neural Network for Drag Prediction
struct ElectricPayload:
    var voltage: Float64
    var current: Float64
    var duration: Float64
    var arc_length: Float64
    
    fn discharge(self, position: Vector[Float64], target: Optional[GameObject]) -> ElectricEffect:
        if target.is_some():
            # Conduct through target
            let resistance = target.value().electrical_resistance
            let power = self.voltage * self.current
            let damage = power * self.duration / resistance
            
            # Create arc visualization
            let arc_points = self.calculate_arc_path(position, target.value().position)
            
            return ElectricEffect(
                arcs = arc_points,
                damage = damage,
                emp_radius = self.voltage / 1000000  # EMP effect
            )


# Reinforcement Learning for Optimal Aim
fn simulate_batch(arrows: Vector[Arrow], states: Vector[StateVector], dt: Float64) -> Vector[StateVector]:
    # SIMD-optimized batch simulation
    let batch_size = 8  # Process 8 arrows at once
    
    # Vectorized computations
    @parameter
    for i in range(0, len(arrows), batch_size):
        let batch_arrows = arrows[i:i+batch_size]
        let batch_states = states[i:i+batch_size]
        
        # Load into SIMD registers
        let pos_x = SIMD[Float64, batch_size]([s.x for s in batch_states])
        let pos_y = SIMD[Float64, batch_size]([s.y for s in batch_states])
        # ... load other components
        
        # Compute in parallel
        let v_mag = sqrt(vx*vx + vy*vy + vz*vz)
        let drag = 0.5 * rho * Cd * area * v_mag
        
        # Store results
        # ...
    
    return new_states



# Parallel Simulation with SIMD
fn simulate_batch(arrows: Vector[Arrow], states: Vector[StateVector], dt: Float64) -> Vector[StateVector]:
    # SIMD-optimized batch simulation
    let batch_size = 8  # Process 8 arrows at once
    
    # Vectorized computations
    @parameter
    for i in range(0, len(arrows), batch_size):
        let batch_arrows = arrows[i:i+batch_size]
        let batch_states = states[i:i+batch_size]
        
        # Load into SIMD registers
        let pos_x = SIMD[Float64, batch_size]([s.x for s in batch_states])
        let pos_y = SIMD[Float64, batch_size]([s.y for s in batch_states])
        # ... load other components
        
        # Compute in parallel
        let v_mag = sqrt(vx*vx + vy*vy + vz*vz)
        let drag = 0.5 * rho * Cd * area * v_mag
        
        # Store results
        # ...
    
    return new_states


# GPU Acceleration
@always_inline
fn launch_cuda_kernel(states: DevicePtr[StateVector], results: DevicePtr[StateVector], n: Int):
    # CUDA kernel for trajectory simulation
    @parameter
    fn kernel(tid: Int):
        let state = states[tid]
        # Simulate one arrow per thread
        let result = rk4_step(state, dt)
        results[tid] = result
    
    # Launch grid of threads
    cuda_launch(kernel, n, blocks=256)


# Neural Network for Drag Prediction
struct NeuralDragModel:
    var network: NeuralNetwork
    var input_normalizer: Normalizer
    var output_normalizer: Normalizer
    
    fn predict_Cd(self, mach: Float64, alpha: Float64, reynolds: Float64) -> Float64:
        # Normalize inputs
        let inputs = Vector[Float64]([mach, alpha, reynolds])
        let normalized = self.input_normalizer(inputs)
        
        # Forward pass through network
        let output = self.network.forward(normalized)
        
        # Denormalize output
        return self.output_normalizer.inverse(output)[0]



# Reinforcement Learning for Optimal Aim
struct AimOptimizer:
    var environment: SimulationEnvironment
    var agent: PPOAgent
    var training_history: List[TrainingStep]
    
    fn find_optimal_angle(self, target_distance: Float64, wind: Vector[Float64]) -> Float64:
        # Use trained RL agent to find best launch angle
        let state = self.environment.get_state(target_distance, wind)
        let action = self.agent.act(state)
        return action.angle



# Validation and Testing
struct TestSuite:
    var engine: ArrowSimulationEngine
    var test_results: List[TestCase]
    
    fn run_all_tests(self) -> TestReport:
        self.test_conservation_laws()
        self.test_symmetry()
        self.test_convergence()
        self.test_against_analytical()
        self.test_stochastic_behavior()
        
        return TestReport(self.test_results)
    
    fn test_conservation_laws(self):
        # Test energy conservation in vacuum
        let vacuum_force = ForceModel.with_no_drag()
        let state = StateVector.with_initial_velocity(50.0, 45.0)
        
        let trajectory = self.engine.integrate(state, vacuum_force, 0.0, 10.0)
        
        # Check energy conservation
        let initial_energy = 0.5 * arrow.mass * 50.0**2
        for s in trajectory:
            let energy = 0.5 * arrow.mass * (s.vx**2 + s.vy**2 + s.vz**2) + arrow.mass * 9.81 * s.y
            assert(abs(energy - initial_energy) < 1e-6)
        self.test_results.append(TestCase("Energy Conservation", True))