' ============================================================================
' ARROW: Dark City - Complete Open World Game
' A VB.NET Masterpiece Inspired by the Arrow TV Series
' Version 4.0 - "The Vigilante Edition"
' ============================================================================

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Drawing.Text
Imports System.Windows.Forms
Imports System.Math
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Linq
Imports System.Text
Imports System.Runtime.InteropServices

Public Class ArrowDarkCityGame
    Inherits Form
    
    #Region "Windows API Declarations for Advanced Graphics"
    
    <DllImport("gdi32.dll")>
    Private Shared Function BitBlt(ByVal hdcDest As IntPtr, ByVal nXDest As Integer, ByVal nYDest As Integer,
                                    ByVal nWidth As Integer, ByVal nHeight As Integer, ByVal hdcSrc As IntPtr,
                                    ByVal nXSrc As Integer, ByVal nYSrc As Integer, ByVal dwRop As Integer) As Boolean
    End Function
    
    <DllImport("user32.dll")>
    Private Shared Function GetDC(ByVal hWnd As IntPtr) As IntPtr
    End Function
    
    <DllImport("user32.dll")>
    Private Shared Function ReleaseDC(ByVal hWnd As IntPtr, ByVal hDC As IntPtr) As Integer
    End Function
    
    <DllImport("kernel32.dll")>
    Private Shared Function QueryPerformanceCounter(ByRef lpPerformanceCount As Long) As Boolean
    End Function
    
    <DllImport("kernel32.dll")>
    Private Shared Function QueryPerformanceFrequency(ByRef lpFrequency As Long) As Boolean
    End Function
    
    #End Region
    
    #Region "Game Constants"
    
    Private Const GAME_TITLE As String = "ARROW: DARK CITY - VIGILANTE EDITION"
    Private Const VERSION As String = "4.0.0"
    Private Const SCREEN_WIDTH As Integer = 1280
    Private Const SCREEN_HEIGHT As Integer = 720
    Private Const TARGET_FPS As Integer = 60
    Private Const FRAME_TIME As Double = 1000.0 / TARGET_FPS
    
    ' World Dimensions
    Private Const WORLD_WIDTH As Integer = 5000
    Private Const WORLD_HEIGHT As Integer = 1000
    Private Const WORLD_DEPTH As Integer = 5000
    
    ' Physics Constants
    Private Const GRAVITY As Double = 9.81
    Private Const MAX_ARROW_SPEED As Double = 90.0
    Private Const PLAYER_SPEED As Double = 5.0
    Private Const PLAYER_ROTATION_SPEED As Double = 0.05
    
    ' Visual Constants
    Private Const MAX_PARTICLES As Integer = 10000
    Private Const MAX_LIGHTS As Integer = 256
    Private Const SHADOW_MAP_SIZE As Integer = 2048
    
    #End Region
    
    #Region "Enumerations"
    
    Public Enum GameState
        SplashScreen
        MainMenu
        Loading
        Playing
        Paused
        Inventory
        Map
        Dialogue
        Combat
        Cutscene
        GameOver
        MissionComplete
    End Enum
    
    Public Enum DistrictType
        TheGlades
        Downtown
        IronHeights
        IndustrialDistrict
        StarlingHeights
        FoundersIsland
        TheQuiver
        CriminalUnderworld
        SCPD
        QueenConsolidated
    End Enum
    
    Public Enum Faction
        Vigilante
        SCPD
        Criminal
        LeagueOfAssassins
        ARGUS
        QueenFamily
        MerlynGroup
        TheUnderground
    End Enum
    
    Public Enum WeatherType
        Clear
        Cloudy
        Rain
        HeavyRain
        Storm
        Fog
        Night
        BloodMoon
        ParticleEffects
    End Enum
    
    Public Enum ArrowType
        Standard
        Explosive
        Grappling
        Electric
        Smoke
        Flashbang
        Sonic
        EMP
        Cryo
        Incendiary
        BoxingGlove
        Net
        Tracker
        USB
        Key
    End Enum
    
    #End Region
    
    #Region "Structures and Classes"
    
    Public Structure Vector3D
        Public X As Double
        Public Y As Double
        Public Z As Double
        
        Public Sub New(x As Double, y As Double, z As Double)
            Me.X = x
            Me.Y = y
            Me.Z = z
        End Sub
        
        Public Shared Operator +(v1 As Vector3D, v2 As Vector3D) As Vector3D
            Return New Vector3D(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z)
        End Operator
        
        Public Shared Operator -(v1 As Vector3D, v2 As Vector3D) As Vector3D
            Return New Vector3D(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z)
        End Operator
        
        Public Shared Operator *(v As Vector3D, scalar As Double) As Vector3D
            Return New Vector3D(v.X * scalar, v.Y * scalar, v.Z * scalar)
        End Operator
        
        Public Function Length() As Double
            Return Sqrt(X * X + Y * Y + Z * Z)
        End Function
        
        Public Function Normalize() As Vector3D
            Dim len = Length()
            If len > 0 Then
                Return New Vector3D(X / len, Y / len, Z / len)
            End If
            Return Me
        End Function
        
        Public Function Dot(v As Vector3D) As Double
            Return X * v.X + Y * v.Y + Z * v.Z
        End Function
        
        Public Function Cross(v As Vector3D) As Vector3D
            Return New Vector3D(
                Y * v.Z - Z * v.Y,
                Z * v.X - X * v.Z,
                X * v.Y - Y * v.X
            )
        End Function
        
        Public Overrides Function ToString() As String
            Return $"({X:F1}, {Y:F1}, {Z:F1})"
        End Function
    End Structure
    
    Public Class GameObject
        Public Property ID As Integer
        Public Property Name As String
        Public Property Position As Vector3D
        Public Property Rotation As Vector3D
        Public Property Scale As Vector3D
        Public Property IsActive As Boolean = True
        Public Property IsVisible As Boolean = True
        Public Property IsCollidable As Boolean = True
        Public Property BoundingBox As BoundingBox
        Public Property MeshID As Integer
        Public Property TextureID As Integer
        Public Property ShaderID As Integer
        Public Property Tag As String
        Public Property Health As Integer = 100
        Public Property MaxHealth As Integer = 100
        
        Public Sub New()
            Position = New Vector3D(0, 0, 0)
            Rotation = New Vector3D(0, 0, 0)
            Scale = New Vector3D(1, 1, 1)
        End Sub
        
        Public Overridable Sub Update(deltaTime As Double)
            ' Base update logic
        End Sub
        
        Public Overridable Sub Render(g As Graphics, camera As Camera)
            ' Base render logic
        End Sub
    End Class
    
    Public Class Character
        Inherits GameObject
        
        Public Property Level As Integer = 1
        Public Property Experience As Integer = 0
        Public Property Strength As Integer = 10
        Public Property Agility As Integer = 10
        Public Property Intelligence As Integer = 10
        Public Property Speed As Double = 5.0
        Public Property Faction As Faction
        Public Property Reputation As Dictionary(Of Faction, Integer)
        Public Property Inventory As List(Of Item)
        Public Property Equipment As Dictionary(Of String, Item)
        Public Property Skills As List(Of Skill)
        Public Property Quests As List(Of Quest)
        Public Property Dialogue As String
        Public Property Relationship As Integer = 0
        
        Public Sub New()
            MyBase.New()
            Reputation = New Dictionary(Of Faction, Integer)
            Inventory = New List(Of Item)
            Equipment = New Dictionary(Of String, Item)
            Skills = New List(Of Skill)
            Quests = New List(Of Quest)
        End Sub
        
        Public Sub AddExperience(amount As Integer)
            Experience += amount
            While Experience >= Level * 100
                LevelUp()
            End While
        End Sub
        
        Private Sub LevelUp()
            Level += 1
            MaxHealth += 20
            Health = MaxHealth
            Strength += 2
            Agility += 2
            Intelligence += 1
        End Sub
    End Class
    
    Public Class Player
        Inherits Character
        
        Public Property Camera As Camera
        Public Property CurrentDistrict As District
        Public Property CompletedMissions As List(Of Mission)
        Public Property KillCount As Integer = 0
        Public Property NonLethalCount As Integer = 0
        Public Property CityReputation As Integer = 0
        Public Property HoodLevel As Integer = 1
        Public Property Arrows As Dictionary(Of ArrowType, Integer)
        Public Property CurrentArrowType As ArrowType = ArrowType.Standard
        
        Public Sub New(name As String)
            MyBase.New()
            Me.Name = name
            Health = 200
            MaxHealth = 200
            CompletedMissions = New List(Of Mission)
            Arrows = New Dictionary(Of ArrowType, Integer)
            
            ' Initialize with basic arrows
            Arrows(ArrowType.Standard) = 50
            Arrows(ArrowType.Explosive) = 5
            Arrows(ArrowType.Grappling) = 3
            
            ' Starting equipment
            Equipment("Bow") = New BowItem("Recurve Bow", 25, 50)
            Equipment("Armor") = New ArmorItem("Green Hood", 15)
        End Sub
        
        Public Sub FireArrow(target As Vector3D)
            If Arrows(CurrentArrowType) > 0 Then
                Arrows(CurrentArrowType) -= 1
                Dim arrow As New ArrowProjectile(Me, CurrentArrowType, target)
                GameWorld.AddProjectile(arrow)
            End If
        End Sub
        
        Public Sub ToggleAimMode()
            Camera.IsAiming = Not Camera.IsAiming
        End Sub
    End Class
    
    Public Class NPC
        Inherits Character
        
        Public Property Routine As List(Of RoutineAction)
        Public Property CurrentAction As RoutineAction
        Public Property DialogueTree As DialogueNode
        Public Property IsHostile As Boolean = False
        Public Property DetectionRange As Double = 20.0
        Public Property PatrolPath As List(Of Vector3D)
        Public Property CurrentPatrolIndex As Integer = 0
        
        Public Sub New(name As String, faction As Faction, dialogue As String)
            MyBase.New()
            Me.Name = name
            Me.Faction = faction
            Me.Dialogue = dialogue
            Routine = New List(Of RoutineAction)
        End Sub
        
        Public Overrides Sub Update(deltaTime As Double)
            MyBase.Update(deltaTime)
            
            ' Simple AI routine
            If Routine.Count > 0 Then
                ' Follow routine logic
            End If
            
            ' Check for player detection
            Dim player = GameWorld.Player
            Dim distanceToPlayer = (player.Position - Me.Position).Length()
            
            If distanceToPlayer < DetectionRange Then
                If IsHostile Then
                    EnterCombatMode(player)
                Else
                    GreetPlayer(player)
                End If
            End If
        End Sub
        
        Private Sub EnterCombatMode(target As Player)
            ' Combat AI logic
            If Me.Health > 0 Then
                ' Attack player
                Dim damage = Strength * 2
                target.Health -= damage
                GameWorld.AddMessage($"{Name} hit you for {damage} damage!")
            End If
        End Sub
        
        Private Sub GreetPlayer(target As Player)
            If Not String.IsNullOrEmpty(Dialogue) Then
                GameWorld.AddDialogue(Me, Dialogue)
            End If
        End Sub
    End Class
    
    Public Class Enemy
        Inherits Character
        
        Public Property EnemyType As String
        Public Property Difficulty As Integer
        Public Property LootTable As List(Of Item)
        Public Property ExperienceReward As Integer
        Public Property DetectionMeter As Double = 0
        Public Property AlertLevel As Integer = 0 ' 0=unaware, 1=suspicious, 2=alerted, 3=combat
        
        Public Sub New(name As String, enemyType As String, difficulty As Integer)
            MyBase.New()
            Me.Name = name
            Me.EnemyType = enemyType
            Me.Difficulty = difficulty
            Me.Health = 50 + difficulty * 20
            Me.MaxHealth = Me.Health
            Me.Strength = 5 + difficulty * 2
            Me.Agility = 5 + difficulty * 2
            Me.ExperienceReward = 20 * difficulty
        End Sub
        
        Public Overrides Sub Update(deltaTime As Double)
            MyBase.Update(deltaTime)
            
            Dim player = GameWorld.Player
            Dim distanceToPlayer = (player.Position - Me.Position).Length()
            
            ' Detection logic
            If distanceToPlayer < 30 Then
                DetectionMeter += deltaTime * 10
                If DetectionMeter > 100 Then
                    AlertLevel = 2
                ElseIf DetectionMeter > 50 Then
                    AlertLevel = 1
                End If
            Else
                DetectionMeter = Max(0, DetectionMeter - deltaTime * 5)
                If DetectionMeter < 20 Then AlertLevel = 0
            End If
            
            ' Combat logic
            If AlertLevel = 3 Then
                ' Attack player
                Dim attackCooldown As Double = 1.0
                ' ... combat logic
            End If
        End Sub
    End Class
    
    Public Class Item
        Inherits GameObject
        
        Public Property Value As Integer
        Public Property Weight As Double
        Public Property IsConsumable As Boolean = False
        Public Property IsEquippable As Boolean = False
        Public Property Description As String
        Public Property IconID As Integer
        
        Public Overridable Sub Use(user As Character)
            ' Base use logic
        End Sub
    End Class
    
    Public Class WeaponItem
        Inherits Item
        
        Public Property Damage As Integer
        Public Property Range As Double
        Public Property Durability As Integer
        Public Property AttackSpeed As Double
        
        Public Sub New(name As String, damage As Integer, durability As Integer)
            Me.Name = name
            Me.Damage = damage
            Me.Durability = durability
            Me.IsEquippable = True
        End Sub
    End Class
    
    Public Class BowItem
        Inherits WeaponItem
        
        Public Property DrawWeight As Integer
        Public Property Accuracy As Double
        Public Property ArrowCapacity As Integer = 20
        
        Public Sub New(name As String, damage As Integer, durability As Integer)
            MyBase.New(name, damage, durability)
        End Sub
    End Class
    
    Public Class ArmorItem
        Inherits Item
        
        Public Property Defense As Integer
        Public Property Durability As Integer
        
        Public Sub New(name As String, defense As Integer)
            Me.Name = name
            Me.Defense = defense
            Me.IsEquippable = True
        End Sub
    End Class
    
    Public Class ArrowProjectile
        Inherits GameObject
        
        Public Property Owner As Character
        Public Property ArrowType As ArrowType
        Public Property Velocity As Vector3D
        Public Property Target As Vector3D
        Public Property Damage As Integer
        Public Property IsActive As Boolean = True
        Public Property HasExploded As Boolean = False
        Public Property Trail As List(Of Vector3D)
        
        Public Sub New(owner As Character, arrowType As ArrowType, target As Vector3D)
            Me.Owner = owner
            Me.ArrowType = arrowType
            Me.Target = target
            Me.Position = owner.Position
            Me.Damage = 20 + CInt(owner.Strength * 1.5)
            Me.Velocity = (target - owner.Position).Normalize() * MAX_ARROW_SPEED
            Me.Trail = New List(Of Vector3D)
        End Sub
        
        Public Overrides Sub Update(deltaTime As Double)
            MyBase.Update(deltaTime)
            
            ' Store trail for visualization
            Trail.Add(Position)
            If Trail.Count > 20 Then Trail.RemoveAt(0)
            
            ' Update position
            Position += Velocity * deltaTime
            
            ' Apply gravity
            Velocity = New Vector3D(Velocity.X, Velocity.Y - GRAVITY * deltaTime, Velocity.Z)
            
            ' Check for collision
            CheckCollision()
        End Sub
        
        Private Sub CheckCollision()
            ' Check collision with world objects
            For Each obj In GameWorld.GameObjects
                If obj.IsCollidable AndAlso obj.ID <> Owner.ID Then
                    If IsPointInBoundingBox(Position, obj.BoundingBox) Then
                        OnHit(obj)
                        Exit For
                    End If
                End If
            Next
            
            ' Check ground collision
            If Position.Y <= 0 Then
                OnHit(Nothing)
            End If
        End Sub
        
        Private Sub OnHit(target As GameObject)
            If Not IsActive Then Return
            
            IsActive = False
            
            If target IsNot Nothing Then
                ' Apply damage
                If TypeOf target Is Character Then
                    Dim character As Character = DirectCast(target, Character)
                    character.Health -= Damage
                    
                    ' Special arrow effects
                    Select Case ArrowType
                        Case ArrowType.Explosive
                            CreateExplosion(target.Position)
                        Case ArrowType.Electric
                            CreateElectricArc(target.Position)
                        Case ArrowType.Smoke
                            CreateSmokeCloud(target.Position)
                        Case ArrowType.Grappling
                            CreateGrapplingHook(target.Position)
                    End Select
                End If
            End If
        End Sub
        
        Private Sub CreateExplosion(position As Vector3D)
            ' Create explosion effect
            For i = 1 To 20
                Dim particle As New Particle(
                    position,
                    New Vector3D((Rnd() * 2 - 1) * 10, (Rnd() * 2 - 1) * 10, (Rnd() * 2 - 1) * 10),
                    Color.Orange,
                    2.0
                )
                GameWorld.AddParticle(particle)
            Next
            
            ' Damage nearby enemies
            For Each obj In GameWorld.GameObjects
                If TypeOf obj Is Enemy Then
                    Dim enemy As Enemy = DirectCast(obj, Enemy)
                    Dim distance = (enemy.Position - position).Length()
                    If distance < 10 Then
                        enemy.Health -= 50
                        GameWorld.AddMessage($"Enemy hit by explosion for 50 damage!")
                    End If
                End If
            Next
        End Sub
        
        Private Sub CreateElectricArc(position As Vector3D)
            ' Electric arc effect
            For i = 1 To 10
                Dim particle As New Particle(
                    position,
                    New Vector3D((Rnd() * 2 - 1) * 5, (Rnd() * 2 - 1) * 5, (Rnd() * 2 - 1) * 5),
                    Color.Cyan,
                    0.5
                )
                GameWorld.AddParticle(particle)
            Next
        End Sub
        
        Private Sub CreateSmokeCloud(position As Vector3D)
            ' Smoke effect
            For i = 1 To 50
                Dim particle As New Particle(
                    position,
                    New Vector3D((Rnd() * 2 - 1) * 2, Rnd() * 5, (Rnd() * 2 - 1) * 2),
                    Color.Gray,
                    5.0
                )
                GameWorld.AddParticle(particle)
            Next
        End Sub
        
        Private Sub CreateGrapplingHook(position As Vector3D)
            ' Grappling hook effect
            GameWorld.AddMessage("Grappling hook deployed!")
            ' Implement swinging mechanics
        End Sub
        
        Private Function IsPointInBoundingBox(point As Vector3D, bbox As BoundingBox) As Boolean
            Return point.X >= bbox.MinX AndAlso point.X <= bbox.MaxX AndAlso
                   point.Y >= bbox.MinY AndAlso point.Y <= bbox.MaxY AndAlso
                   point.Z >= bbox.MinZ AndAlso point.Z <= bbox.MaxZ
        End Function
    End Class
    
    Public Class Particle
        Public Property Position As Vector3D
        Public Property Velocity As Vector3D
        Public Property Color As Color
        Public Property Size As Single
        Public Property Life As Double
        Public Property MaxLife As Double
        
        Public Sub New(position As Vector3D, velocity As Vector3D, color As Color, life As Double)
            Me.Position = position
            Me.Velocity = velocity
            Me.Color = color
            Me.Life = life
            Me.MaxLife = life
            Me.Size = 2.0F
        End Sub
        
        Public Sub Update(deltaTime As Double)
            Position += Velocity * deltaTime
            Velocity = New Vector3D(Velocity.X, Velocity.Y - GRAVITY * deltaTime * 0.1, Velocity.Z)
            Life -= deltaTime
        End Sub
    End Class
    
    Public Class Light
        Public Property Position As Vector3D
        Public Property Color As Color
        Public Property Intensity As Single
        Public Property Range As Single
        Public Property IsDynamic As Boolean = False
        
        Public Sub New(position As Vector3D, color As Color, intensity As Single, range As Single)
            Me.Position = position
            Me.Color = color
            Me.Intensity = intensity
            Me.Range = range
        End Sub
    End Class
    
    Public Class Building
        Inherits GameObject
        
        Public Property Floors As Integer
        Public Property IsEnterable As Boolean = False
        Public Property Interior As List(Of GameObject)
        Public Property RoofAccess As Boolean = False
        Public Property Windows As List(Of Window)
        Public Property Lights As List(Of Light)
        Public Property District As DistrictType
        
        Public Sub New(name As String, position As Vector3D, floors As Integer)
            Me.Name = name
            Me.Position = position
            Me.Floors = floors
            Me.Scale = New Vector3D(20, floors * 5, 20)
            Me.Windows = New List(Of Window)
            Me.Lights = New List(Of Light)
            
            ' Create windows
            For floor = 1 To floors
                For side = 1 To 4
                    For w = 1 To 3
                        Windows.Add(New Window(position, floor, side, w))
                    Next
                Next
            Next
            
            ' Create lights
            For floor = 1 To floors
                Lights.Add(New Light(
                    New Vector3D(position.X, position.Y + floor * 5, position.Z),
                    Color.Yellow,
                    1.0F,
                    10.0F
                ))
            Next
        End Sub
        
        Public Overrides Sub Render(g As Graphics, camera As Camera)
            MyBase.Render(g, camera)
            
            ' Render windows with lighting effects
            For Each window In Windows
                If window.IsLit Then
                    window.Render(g, camera)
                End If
            Next
        End Sub
    End Class
    
    Public Class Window
        Public Property BuildingPosition As Vector3D
        Public Property Floor As Integer
        Public Property Side As Integer
        Public Property Index As Integer
        Public Property IsLit As Boolean = True
        Public Property LightColor As Color = Color.Yellow
        
        Public Sub New(buildingPos As Vector3D, floor As Integer, side As Integer, index As Integer)
            Me.BuildingPosition = buildingPos
            Me.Floor = floor
            Me.Side = side
            Me.Index = index
        End Sub
        
        Public Sub Render(g As Graphics, camera As Camera)
            ' Window rendering logic
            Dim screenPos = camera.WorldToScreen(GetPosition())
            If screenPos.Z > 0 Then
                Using brush As New SolidBrush(Color.FromArgb(200, LightColor))
                    g.FillRectangle(brush, screenPos.X - 2, screenPos.Y - 2, 4, 4)
                End Using
            End If
        End Sub
        
        Private Function GetPosition() As Vector3D
            ' Calculate window position based on building
            Dim xOffset = (Index - 2) * 3
            Dim yOffset = BuildingPosition.Y + Floor * 5
            Dim zOffset = 10
            
            Select Case Side
                Case 1 : Return New Vector3D(BuildingPosition.X + 10, yOffset, BuildingPosition.Z + xOffset)
                Case 2 : Return New Vector3D(BuildingPosition.X - 10, yOffset, BuildingPosition.Z + xOffset)
                Case 3 : Return New Vector3D(BuildingPosition.X + xOffset, yOffset, BuildingPosition.Z + 10)
                Case 4 : Return New Vector3D(BuildingPosition.X + xOffset, yOffset, BuildingPosition.Z - 10)
            End Select
            
            Return BuildingPosition
        End Function
    End Class
    
    Public Class Vehicle
        Inherits GameObject
        
        Public Property Speed As Double
        Public Property Direction As Vector3D
        Public Property IsMoving As Boolean = False
        Public Property VehicleType As String
        Public Property SirenOn As Boolean = False
        
        Public Sub New(name As String, position As Vector3D, vehicleType As String)
            Me.Name = name
            Me.Position = position
            Me.VehicleType = vehicleType
            Me.Speed = 10.0
        End Sub
        
        Public Overrides Sub Update(deltaTime As Double)
            MyBase.Update(deltaTime)
            
            If IsMoving Then
                Position += Direction * Speed * deltaTime
                
                ' Simple traffic pattern
                If Position.X > WORLD_WIDTH OrElse Position.X < -WORLD_WIDTH Then
                    Direction = New Vector3D(-Direction.X, 0, Direction.Z)
                End If
                If Position.Z > WORLD_DEPTH OrElse Position.Z < -WORLD_DEPTH Then
                    Direction = New Vector3D(Direction.X, 0, -Direction.Z)
                End If
            End If
        End Sub
    End Class
    
    Public Class District
        Public Property Name As String
        Public Property Type As DistrictType
        Public Property Bounds As BoundingBox
        Public Property Buildings As List(Of Building)
        Public Property NPCs As List(Of NPC)
        Public Property Enemies As List(Of Enemy)
        Public Property CrimeRate As Integer
        Public Property Wealth As Integer
        Public Property Music As String
        Public Property AmbientColor As Color
        
        Public Sub New(name As String, type As DistrictType, bounds As BoundingBox)
            Me.Name = name
            Me.Type = type
            Me.Bounds = bounds
            Me.Buildings = New List(Of Building)
            Me.NPCs = New List(Of NPC)
            Me.Enemies = New List(Of Enemy)
            
            ' Set district properties
            Select Case type
                Case DistrictType.TheGlades
                    CrimeRate = 90
                    Wealth = 10
                    AmbientColor = Color.FromArgb(100, 50, 50, 50)
                Case DistrictType.Downtown
                    CrimeRate = 30
                    Wealth = 80
                    AmbientColor = Color.FromArgb(100, 100, 100, 150)
                Case DistrictType.StarlingHeights
                    CrimeRate = 20
                    Wealth = 95
                    AmbientColor = Color.FromArgb(100, 150, 150, 200)
                Case Else
                    CrimeRate = 50
                    Wealth = 50
                    AmbientColor = Color.FromArgb(100, 80, 80, 100)
            End Select
        End Sub
        
        Public Sub GenerateContent()
            ' Generate buildings based on district type
            Dim buildingCount = 20 + CrimeRate / 2
            
            For i = 1 To buildingCount
                Dim x = Rnd() * (Bounds.MaxX - Bounds.MinX) + Bounds.MinX
                Dim z = Rnd() * (Bounds.MaxZ - Bounds.MinZ) + Bounds.MinZ
                Dim floors = 2 + CInt(Rnd() * 8)
                
                Dim building As New Building($"Building_{i}", New Vector3D(x, 0, z), floors)
                building.District = Type
                Buildings.Add(building)
            Next
            
            ' Generate NPCs based on wealth
            Dim npcCount = Wealth / 10
            
            For i = 1 To npcCount
                Dim x = Rnd() * (Bounds.MaxX - Bounds.MinX) + Bounds.MinX
                Dim z = Rnd() * (Bounds.MaxZ - Bounds.MinZ) + Bounds.MinZ
                
                Dim npc As New NPC(
                    $"Citizen_{i}",
                    Faction.SCPD,
                    "This city needs a hero..."
                )
                npc.Position = New Vector3D(x, 0, z)
                NPCs.Add(npc)
            Next
            
            ' Generate enemies based on crime rate
            Dim enemyCount = CrimeRate / 10
            
            For i = 1 To enemyCount
                Dim x = Rnd() * (Bounds.MaxX - Bounds.MinX) + Bounds.MinX
                Dim z = Rnd() * (Bounds.MaxZ - Bounds.MinZ) + Bounds.MinZ
                
                Dim enemy As New Enemy(
                    $"Thug_{i}",
                    "Street Thug",
                    CrimeRate / 20
                )
                enemy.Position = New Vector3D(x, 0, z)
                enemy.IsHostile = True
                Enemies.Add(enemy)
            Next
        End Sub
    End Class
    
    Public Class Quest
        Public Property ID As Integer
        Public Property Name As String
        Public Property Description As String
        Public Property Giver As NPC
        Public Property Objectives As List(Of QuestObjective)
        Public Property Rewards As QuestRewards
        Public Property IsActive As Boolean = False
        Public Property IsCompleted As Boolean = False
        Public Property TimeLimit As Double = -1
        Public Property CurrentTime As Double = 0
        Public Property RequiredLevel As Integer = 1
        
        Public Sub CheckCompletion()
            Dim allComplete = True
            For Each obj In Objectives
                If Not obj.IsComplete Then
                    allComplete = False
                    Exit For
                End If
            Next
            
            If allComplete Then
                IsCompleted = True
                IsActive = False
            End If
        End Sub
    End Class
    
    Public Class QuestObjective
        Public Property Description As String
        Public Property Type As String
        Public Property Target As String
        Public Property RequiredCount As Integer
        Public Property CurrentCount As Integer
        Public Property IsComplete As Boolean
            Get
                Return CurrentCount >= RequiredCount
            End Get
        End Property
        
        Public Sub Update(progress As Integer)
            CurrentCount += progress
        End Sub
    End Class
    
    Public Class QuestRewards
        Public Property Experience As Integer = 0
        Public Property Reputation As Dictionary(Of Faction, Integer)
        Public Property Items As List(Of Item)
        Public Property Money As Integer = 0
        
        Public Sub New()
            Reputation = New Dictionary(Of Faction, Integer)
            Items = New List(Of Item)
        End Sub
    End Class
    
    Public Class Skill
        Public Property Name As String
        Public Property Level As Integer = 1
        Public Property MaxLevel As Integer = 5
        Public Property Description As String
        Public Property Cooldown As Double = 0
        Public Property CurrentCooldown As Double = 0
        
        Public Sub Use(user As Character)
            If CurrentCooldown <= 0 Then
                ' Skill logic
                CurrentCooldown = Cooldown
            End If
        End Sub
    End Class
    
    Public Class DialogueNode
        Public Property Text As String
        Public Property Responses As List(Of DialogueResponse)
        Public Property NextNode As DialogueNode
        Public Property QuestTrigger As Quest
        Public Property ReputationChange As Integer = 0
    End Class
    
    Public Class DialogueResponse
        Public Property Text As String
        Public Property NextNode As DialogueNode
        Public Property RequiredSkill As Skill
        Public Property RequiredFaction As Faction
    End Class
    
    Public Class Mission
        Public Property Name As String
        Public Property Description As String
        Public Property Location As String
        Public Property RequiredKills As Integer
        Public Property CurrentKills As Integer
        Public Property IsCompleted As Boolean = False
        Public Property RewardXP As Integer
        Public Property RewardRep As Integer
        
        Public Sub New(name As String, description As String, location As String, kills As Integer, xp As Integer, rep As Integer)
            Me.Name = name
            Me.Description = description
            Me.Location = location
            Me.RequiredKills = kills
            Me.RewardXP = xp
            Me.RewardRep = rep
        End Sub
    End Class
    
    Public Class BoundingBox
        Public Property MinX As Double
        Public Property MinY As Double
        Public Property MinZ As Double
        Public Property MaxX As Double
        Public Property MaxY As Double
        Public Property MaxZ As Double
        
        Public Sub New(minX As Double, minY As Double, minZ As Double, maxX As Double, maxY As Double, maxZ As Double)
            Me.MinX = minX
            Me.MinY = minY
            Me.MinZ = minZ
            Me.MaxX = maxX
            Me.MaxY = maxY
            Me.MaxZ = maxZ
        End Sub
    End Class
    
    Public Class Camera
        Public Property Position As Vector3D
        Public Property Target As Vector3D
        Public Property Up As Vector3D
        Public Property FieldOfView As Double = 60.0
        Public Property NearPlane As Double = 0.1
        Public Property FarPlane As Double = 1000.0
        Public Property IsAiming As Boolean = False
        Public Property ShakeIntensity As Double = 0
        Public Property ShakeDuration As Double = 0
        
        Public Sub New()
            Position = New Vector3D(0, 10, -20)
            Target = New Vector3D(0, 0, 0)
            Up = New Vector3D(0, 1, 0)
        End Sub
        
        Public Sub Update(playerPos As Vector3D, playerRot As Vector3D)
            If IsAiming Then
                ' Aim mode - camera behind shoulder
                Dim offset = New Vector3D(
                    -Sin(playerRot.Y) * 2,
                    2,
                    -Cos(playerRot.Y) * 2
                )
                Position = playerPos + offset
                Target = playerPos + New Vector3D(
                    Sin(playerRot.Y) * 10,
                    0,
                    Cos(playerRot.Y) * 10
                )
            Else
                ' Third person - camera behind player
                Dim offset = New Vector3D(
                    -Sin(playerRot.Y) * 5,
                    3,
                    -Cos(playerRot.Y) * 5
                )
                Position = playerPos + offset
                Target = playerPos + New Vector3D(
                    Sin(playerRot.Y) * 5,
                    0,
                    Cos(playerRot.Y) * 5
                )
            End If
            
            ' Apply camera shake
            If ShakeDuration > 0 Then
                Dim shakeX = (Rnd() * 2 - 1) * ShakeIntensity
                Dim shakeY = (Rnd() * 2 - 1) * ShakeIntensity
                Dim shakeZ = (Rnd() * 2 - 1) * ShakeIntensity
                Position = New Vector3D(
                    Position.X + shakeX,
                    Position.Y + shakeY,
                    Position.Z + shakeZ
                )
                ShakeDuration -= 0.016 ' Assuming 60 FPS
            End If
        End Sub
        
        Public Function WorldToScreen(worldPos As Vector3D) As Vector3D
            ' Simple perspective projection
            Dim relative = worldPos - Position
            Dim distance = relative.Length()
            
            If distance = 0 Then Return New Vector3D(SCREEN_WIDTH / 2, SCREEN_HEIGHT / 2, 0)
            
            ' Basic perspective
            Dim scale = 500 / distance
            Dim screenX = SCREEN_WIDTH / 2 + relative.X * scale
            Dim screenY = SCREEN_HEIGHT / 2 - relative.Y * scale
            Dim screenZ = distance
            
            Return New Vector3D(screenX, screenY, screenZ)
        End Function
        
        Public Sub Shake(intensity As Double, duration As Double)
            ShakeIntensity = intensity
            ShakeDuration = duration
        End Sub
    End Class
    
    Public Class WeatherSystem
        Public Property CurrentWeather As WeatherType = WeatherType.Clear
        Public Property RainIntensity As Single = 0
        Public Property WindDirection As Vector3D
        Public Property WindSpeed As Single
        Public Property FogDensity As Single = 0
        Public Property CloudCoverage As Single = 0
        Public Property TimeOfDay As Single = 12 ' 24-hour clock
        Public Property DayNightCycle As Boolean = True
        
        Private _transitionSpeed As Single = 0.1
        Private _targetWeather As WeatherType
        
        Public Sub New()
            WindDirection = New Vector3D(1, 0, 0)
            WindSpeed = 2.0F
        End Sub
        
        Public Sub Update(deltaTime As Double)
            If DayNightCycle Then
                TimeOfDay += deltaTime * 0.1 ' 10x speed for demo
                If TimeOfDay >= 24 Then TimeOfDay = 0
            End If
            
            ' Weather transitions
            If CurrentWeather <> _targetWeather Then
                TransitionWeather(deltaTime)
            End If
            
            ' Update effects
            UpdateRain(deltaTime)
            UpdateWind(deltaTime)
            UpdateFog(deltaTime)
        End Sub
        
        Private Sub UpdateRain(deltaTime As Double)
            If CurrentWeather = WeatherType.Rain OrElse CurrentWeather = WeatherType.HeavyRain OrElse CurrentWeather = WeatherType.Storm Then
                RainIntensity = Min(1.0F, RainIntensity + _transitionSpeed * CSng(deltaTime))
            Else
                RainIntensity = Max(0.0F, RainIntensity - _transitionSpeed * CSng(deltaTime))
            End If
        End Sub
        
        Private Sub UpdateWind(deltaTime As Double)
            ' Wind gusts
            If Rnd() < 0.01 Then
                WindDirection = New Vector3D(
                    (Rnd() * 2 - 1),
                    0,
                    (Rnd() * 2 - 1)
                ).Normalize()
                WindSpeed = 2.0F + Rnd() * 8.0F
            End If
        End Sub
        
        Private Sub UpdateFog(deltaTime As Double)
            If CurrentWeather = WeatherType.Fog Then
                FogDensity = Min(0.8F, FogDensity + _transitionSpeed * CSng(deltaTime))
            Else
                FogDensity = Max(0.0F, FogDensity - _transitionSpeed * CSng(deltaTime))
            End If
        End Sub
        
        Private Sub TransitionWeather(deltaTime As Double)
            ' Smooth transition between weather types
            ' Implementation would gradually change parameters
        End Sub
        
        Public Sub SetWeather(weather As WeatherType)
            _targetWeather = weather
        End Sub
        
        Public Function GetAmbientColor() As Color
            ' Time of day color
            Dim r, g, b As Integer
            
            If TimeOfDay < 6 OrElse TimeOfDay > 20 Then
                ' Night
                r = 20 : g = 20 : b = 40
            ElseIf TimeOfDay < 7 Then
                ' Dawn
                r = 100 : g = 70 : b = 70
            ElseIf TimeOfDay > 19 Then
                ' Dusk
                r = 150 : g = 80 : b = 80
            Else
                ' Day
                r = 255 : g = 255 : b = 255
            End If
            
            ' Weather modification
            Select Case CurrentWeather
                Case WeatherType.Rain
                    r = CInt(r * 0.7) : g = CInt(g * 0.7) : b = CInt(b * 0.9)
                Case WeatherType.Fog
                    r = CInt(r * 0.8) : g = CInt(g * 0.8) : b = CInt(b * 0.8)
                Case WeatherType.Storm
                    r = CInt(r * 0.5) : g = CInt(g * 0.5) : b = CInt(b * 0.6)
            End Select
            
            Return Color.FromArgb(r, g, b)
        End Function
    End Class
    
    Public Class SoundSystem
        Public Property MasterVolume As Single = 1.0F
        Public Property MusicVolume As Single = 0.7F
        Public Property SFXVolume As Single = 0.8F
        
        Private _currentMusic As String
        Private _sounds As Dictionary(Of String, IO.Stream)
        
        Public Sub PlaySound(soundName As String, loop As Boolean)
            ' In a real implementation, this would use DirectX or OpenAL
            Console.Beep(1000, 100) ' Placeholder
        End Sub
        
        Public Sub PlayMusic(musicName As String)
            _currentMusic = musicName
            ' Load and play music
        End Sub
        
        Public Sub StopAllSounds()
            ' Stop all audio
        End Sub
    End Class
    
    Public Class ParticleSystem
        Private _particles As List(Of Particle)
        
        Public Sub New()
            _particles = New List(Of Particle)
        End Sub
        
        Public Sub AddParticle(particle As Particle)
            If _particles.Count < MAX_PARTICLES Then
                _particles.Add(particle)
            End If
        End Sub
        
        Public Sub Update(deltaTime As Double)
            For i = _particles.Count - 1 To 0 Step -1
                _particles(i).Update(deltaTime)
                If _particles(i).Life <= 0 Then
                    _particles.RemoveAt(i)
                End If
            Next
        End Sub
        
        Public Sub Render(g As Graphics, camera As Camera)
            For Each particle In _particles
                Dim screenPos = camera.WorldToScreen(particle.Position)
                If screenPos.Z > 0 Then
                    Dim alpha = CInt(255 * (particle.Life / particle.MaxLife))
                    Using brush As New SolidBrush(Color.FromArgb(alpha, particle.Color))
                        Dim size = particle.Size * CSng(100 / screenPos.Z)
                        g.FillEllipse(brush, screenPos.X - size / 2, screenPos.Y - size / 2, size, size)
                    End Using
                End If
            Next
        End Sub
    End Class
    
    Public Class GameWorld
        Public Shared Property Player As Player
        Public Shared Property GameObjects As List(Of GameObject)
        Public Shared Property Districts As Dictionary(Of DistrictType, District)
        Public Shared Property Projectiles As List(Of ArrowProjectile)
        Public Shared Property Particles As ParticleSystem
        Public Shared Property Lights As List(Of Light)
        Public Shared Property CurrentDistrict As District
        Public Shared Property Messages As List(Of String)
        Public Shared Property DialogueQueue As Queue(Of Tuple(Of NPC, String))
        
        Public Shared Sub Initialize()
            GameObjects = New List(Of GameObject)
            Districts = New Dictionary(Of DistrictType, District)
            Projectiles = New List(Of ArrowProjectile)
            Particles = New ParticleSystem()
            Lights = New List(Of Light)
            Messages = New List(Of String)
            DialogueQueue = New Queue(Of Tuple(Of NPC, String))
            
            CreateWorld()
        End Sub
        
        Private Shared Sub CreateWorld()
            ' Create The Glades
            Dim gladesBounds As New BoundingBox(-1000, 0, -1000, 0, 500, 1000)
            Dim glades As New District("The Glades", DistrictType.TheGlades, gladesBounds)
            glades.GenerateContent()
            Districts(DistrictType.TheGlades) = glades
            
            ' Create Downtown
            Dim downtownBounds As New BoundingBox(0, 0, -500, 1000, 500, 500)
            Dim downtown As New District("Downtown", DistrictType.Downtown, downtownBounds)
            downtown.GenerateContent()
            Districts(DistrictType.Downtown) = downtown
            
            ' Create Starling Heights
            Dim heightsBounds As New BoundingBox(500, 0, 500, 1500, 500, 1500)
            Dim heights As New District("Starling Heights", DistrictType.StarlingHeights, heightsBounds)
            heights.GenerateContent()
            Districts(DistrictType.StarlingHeights) = heights
            
            ' Create Industrial District
            Dim industrialBounds As New BoundingBox(-1500, 0, 500, -500, 500, 1500)
            Dim industrial As New District("Industrial District", DistrictType.IndustrialDistrict, industrialBounds)
            industrial.GenerateContent()
            Districts(DistrictType.IndustrialDistrict) = industrial
            
            ' Create Iron Heights
            Dim ironBounds As New BoundingBox(-500, 0, -1500, 500, 500, -500)
            Dim iron As New District("Iron Heights", DistrictType.IronHeights, ironBounds)
            iron.GenerateContent()
            Districts(DistrictType.IronHeights) = iron
            
            ' Create The Quiver (base)
            Dim quiverBounds As New BoundingBox(200, 0, 200, 400, 50, 400)
            Dim quiver As New District("The Quiver", DistrictType.TheQuiver, quiverBounds)
            quiver.GenerateContent()
            Districts(DistrictType.TheQuiver) = quiver
            
            ' Set current district
            CurrentDistrict = glades
            
            ' Add street lights throughout the city
            For x = -2000 To 2000 Step 100
                For z = -2000 To 2000 Step 100
                    If Rnd() < 0.3 Then
                        Dim light As New Light(
                            New Vector3D(x, 5, z),
                            Color.FromArgb(255, 255, 200, 150),
                            2.0F,
                            30.0F
                        )
                        Lights.Add(light)
                    End If
                Next
            Next
            
            ' Add vehicles
            For i = 1 To 50
                Dim x = Rnd() * 4000 - 2000
                Dim z = Rnd() * 4000 - 2000
                Dim vehicle As New Vehicle(
                    $"Car_{i}",
                    New Vector3D(x, 0, z),
                    If(Rnd() < 0.5, "Police", "Civilian")
                )
                vehicle.Direction = New Vector3D(Rnd() * 2 - 1, 0, Rnd() * 2 - 1).Normalize()
                vehicle.IsMoving = True
                GameObjects.Add(vehicle)
            Next
        End Sub
        
        Public Shared Sub AddProjectile(arrow As ArrowProjectile)
            Projectiles.Add(arrow)
            GameObjects.Add(arrow)
        End Sub
        
        Public Shared Sub AddParticle(particle As Particle)
            Particles.AddParticle(particle)
        End Sub
        
        Public Shared Sub AddMessage(message As String)
            Messages.Add(message)
            If Messages.Count > 10 Then
                Messages.RemoveAt(0)
            End If
        End Sub
        
        Public Shared Sub AddDialogue(npc As NPC, text As String)
            DialogueQueue.Enqueue(Tuple.Create(npc, text))
        End Sub
        
        Public Shared Sub Update(deltaTime As Double)
            ' Update game objects
            For Each obj In GameObjects.ToList()
                If obj.IsActive Then
                    obj.Update(deltaTime)
                End If
            Next
            
            ' Update projectiles
            For Each proj In Projectiles.ToList()
                proj.Update(deltaTime)
                If Not proj.IsActive Then
                    Projectiles.Remove(proj)
                    GameObjects.Remove(proj)
                End If
            Next
            
            ' Update particles
            Particles.Update(deltaTime)
            
            ' Update district based on player position
            For Each district In Districts.Values
                If Player.Position.X >= district.Bounds.MinX AndAlso
                   Player.Position.X <= district.Bounds.MaxX AndAlso
                   Player.Position.Z >= district.Bounds.MinZ AndAlso
                   Player.Position.Z <= district.Bounds.MaxZ Then
                    CurrentDistrict = district
                    Exit For
                End If
            Next
            
            ' Spawn enemies based on crime rate
            If CurrentDistrict.CrimeRate > 50 AndAlso Rnd() < 0.01 Then
                SpawnRandomEnemy()
            End If
        End Sub
        
        Private Shared Sub SpawnRandomEnemy()
            Dim x = Player.Position.X + (Rnd() * 40 - 20)
            Dim z = Player.Position.Z + (Rnd() * 40 - 20)
            
            Dim enemy As New Enemy(
                $"Street Thug",
                "Thug",
                CurrentDistrict.CrimeRate / 20
            )
            enemy.Position = New Vector3D(x, 0, z)
            enemy.IsHostile = True
            GameObjects.Add(enemy)
            AddMessage("Enemy spotted!")
        End Sub
    End Class
    
    #End Region
    
    #Region "Form Variables"
    
    Private WithEvents gameTimer As New Timer()
    Private fpsTimer As New Stopwatch()
    Private frameCount As Integer = 0
    Private fps As Integer = 0
    Private lastTime As Long = 0
    Private frequency As Long = 0
    Private gameState As GameState = GameState.SplashScreen
    Private currentPlayer As Player
    Private gameCamera As New Camera()
    Private weather As New WeatherSystem()
    Private sound As New SoundSystem()
    Private gameFonts As New Dictionary(Of String, Font)
    Private gameBrushes As New Dictionary(Of String, Brush)
    Private gamePens As New Dictionary(Of String, Pen)
    Private splashAlpha As Integer = 0
    Private splashDirection As Integer = 1
    Private menuSelectedIndex As Integer = 0
    Private menuItems As New List(Of String) From {"NEW GAME", "CONTINUE", "OPTIONS", "EXIT"}
    Private isLoading As Boolean = False
    Private loadingProgress As Integer = 0
    Private lastMousePos As Point
    Private isMouseLookEnabled As Boolean = False
    Private keyStates As New Dictionary(Of Keys, Boolean)
    Private inputEnabled As Boolean = True
    
    #End Region
    
    #Region "Form Initialization"
    
    Public Sub New()
        InitializeComponent()
        SetupForm()
        InitializeGame()
    End Sub
    
    Private Sub InitializeComponent()
        Me.Text = GAME_TITLE
        Me.ClientSize = New Size(SCREEN_WIDTH, SCREEN_HEIGHT)
        Me.FormBorderStyle = FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.DoubleBuffered = True
        Me.KeyPreview = True
        Me.BackColor = Color.Black
        
        gameTimer.Interval = 1
        gameTimer.Enabled = True
    End Sub
    
    Private Sub SetupForm()
        ' Set form style for better performance
        Me.SetStyle(ControlStyles.AllPaintingInWmPaint Or
                    ControlStyles.UserPaint Or
                    ControlStyles.DoubleBuffer Or
                    ControlStyles.OptimizedDoubleBuffer, True)
        
        ' Initialize high-performance timer
        QueryPerformanceFrequency(frequency)
        QueryPerformanceCounter(lastTime)
    End Sub
    
    Private Sub InitializeGame()
        ' Initialize fonts
        gameFonts("Title") = New Font("Arial Black", 48, FontStyle.Bold)
        gameFonts("Menu") = New Font("Arial", 24, FontStyle.Bold)
        gameFonts("Normal") = New Font("Arial", 12)
        gameFonts("Small") = New Font("Arial", 10)
        gameFonts("HUD") = New Font("Courier New", 10, FontStyle.Bold)
        
        ' Initialize brushes
        gameBrushes("White") = New SolidBrush(Color.White)
        gameBrushes("Black") = New SolidBrush(Color.Black)
        gameBrushes("Red") = New SolidBrush(Color.Red)
        gameBrushes("Green") = New SolidBrush(Color.Green)
        gameBrushes("Blue") = New SolidBrush(Color.Blue)
        gameBrushes("Yellow") = New SolidBrush(Color.Yellow)
        gameBrushes("TransparentBlack") = New SolidBrush(Color.FromArgb(128, 0, 0, 0))
        
        ' Initialize pens
        gamePens("White") = New Pen(Color.White)
        gamePens("Red") = New Pen(Color.Red)
        gamePens("Green") = New Pen(Color.Green)
        
        ' Initialize key states
        For Each key In [Enum].GetValues(GetType(Keys))
            keyStates(CType(key, Keys)) = False
        Next
        
        ' Create the game world
        GameWorld.Initialize()
        
        ' Create player
        currentPlayer = New Player("Oliver Queen")
        currentPlayer.Position = New Vector3D(0, 0, 0)
        currentPlayer.Camera = gameCamera
        GameWorld.Player = currentPlayer
        GameWorld.GameObjects.Add(currentPlayer)
        
        ' Add key NPCs
        AddKeyCharacters()
    End Sub
    
    Private Sub AddKeyCharacters()
        ' Felicity Smoak
        Dim felicity As New NPC("Felicity Smoak", Faction.QueenFamily,
            "Oliver, I've detected unusual activity in the Glades. You should check it out.")
        felicity.Position = New Vector3D(300, 0, 300) ' The Quiver
        felicity.Intelligence = 100
        GameWorld.GameObjects.Add(felicity)
        
        ' John Diggle
        Dim diggle As New NPC("John Diggle", Faction.ARGUS,
            "Watch your six, Oliver. The city's getting more dangerous by the day.")
        diggle.Position = New Vector3D(320, 0, 280)
        diggle.Strength = 90
        GameWorld.GameObjects.Add(diggle)
        
        ' Quentin Lance
        Dim lance As New NPC("Quentin Lance", Faction.SCPD,
            "The SCPD could use your help. The Glades are becoming a war zone.")
        lance.Position = New Vector3D(-500, 0, -500) ' SCPD Headquarters
        GameWorld.GameObjects.Add(lance)
        
        ' Slade Wilson (enemy)
        Dim slade As New Enemy("Slade Wilson", "Deathstroke", 10)
        slade.Position = New Vector3D(-800, 0, 800) ' Industrial District
        slade.IsHostile = True
        GameWorld.GameObjects.Add(slade)
    End Sub
    
    #End Region
    
    #Region "Game Loop"
    
    Private Sub gameTimer_Tick(sender As Object, e As EventArgs) Handles gameTimer.Tick
        ' Calculate delta time
        Dim currentTime As Long = 0
        QueryPerformanceCounter(currentTime)
        Dim deltaTime As Double = (currentTime - lastTime) / frequency
        lastTime = currentTime
        
        ' Cap delta time
        If deltaTime > 0.1 Then deltaTime = 0.1
        
        ' Update game logic
        UpdateGame(deltaTime)
        
        ' Render frame
        Me.Invalidate()
        
        ' Calculate FPS
        frameCount += 1
        If fpsTimer.ElapsedMilliseconds >= 1000 Then
            fps = frameCount
            frameCount = 0
            fpsTimer.Restart()
        End If
    End Sub
    
    Private Sub UpdateGame(deltaTime As Double)
        Select Case gameState
            Case GameState.SplashScreen
                UpdateSplashScreen(deltaTime)
                
            Case GameState.MainMenu
                UpdateMainMenu(deltaTime)
                
            Case GameState.Loading
                UpdateLoading(deltaTime)
                
            Case GameState.Playing
                UpdatePlaying(deltaTime)
                
            Case GameState.Paused
                UpdatePaused(deltaTime)
                
            Case GameState.Inventory
                UpdateInventory(deltaTime)
                
            Case GameState.Map
                UpdateMap(deltaTime)
                
            Case GameState.Dialogue
                UpdateDialogue(deltaTime)
                
            Case GameState.Combat
                UpdateCombat(deltaTime)
        End Select
    End Sub
    
    Private Sub UpdateSplashScreen(deltaTime As Double)
        splashAlpha += splashDirection * CInt(255 * deltaTime)
        
        If splashAlpha >= 255 Then
            splashAlpha = 255
            splashDirection = -1
        ElseIf splashAlpha <= 0 Then
            splashAlpha = 0
            gameState = GameState.MainMenu
        End If
    End Sub
    
    Private Sub UpdateMainMenu(deltaTime As Double)
        ' Menu updates
    End Sub
    
    Private Sub UpdateLoading(deltaTime As Double)
        loadingProgress += CInt(50 * deltaTime)
        If loadingProgress >= 100 Then
            loadingProgress = 100
            gameState = GameState.Playing
            isLoading = False
        End If
    End Sub
    
    Private Sub UpdatePlaying(deltaTime As Double)
        ' Handle player input
        HandleInput(deltaTime)
        
        ' Update camera
        gameCamera.Update(currentPlayer.Position, currentPlayer.Rotation)
        
        ' Update weather
        weather.Update(deltaTime)
        
        ' Update game world
        GameWorld.Update(deltaTime)
        
        ' Check for district transition
        Dim oldDistrict = GameWorld.CurrentDistrict
        If oldDistrict IsNot Nothing AndAlso oldDistrict.Name <> GameWorld.CurrentDistrict.Name Then
            GameWorld.AddMessage($"Entering {GameWorld.CurrentDistrict.Name}")
            sound.PlaySound("district_transition", False)
        End If
        
        ' Check for enemy proximity
        For Each obj In GameWorld.GameObjects
            If TypeOf obj Is Enemy Then
                Dim enemy As Enemy = DirectCast(obj, Enemy)
                Dim distance = (enemy.Position - currentPlayer.Position).Length()
                If distance < 20 AndAlso enemy.IsHostile Then
                    gameState = GameState.Combat
                    sound.PlaySound("combat_start", False)
                    Exit For
                End If
            End If
        Next
    End Sub
    
    Private Sub UpdatePaused(deltaTime As Double)
        ' Pause menu logic
    End Sub
    
    Private Sub UpdateInventory(deltaTime As Double)
        ' Inventory screen logic
    End Sub
    
    Private Sub UpdateMap(deltaTime As Double)
        ' Map screen logic
    End Sub
    
    Private Sub UpdateDialogue(deltaTime As Double)
        ' Dialogue system logic
    End Sub
    
    Private Sub UpdateCombat(deltaTime As Double)
        ' Combat-specific logic
        ' Arrow time effect
        deltaTime *= 0.5 ' Slow motion
        UpdatePlaying(deltaTime)
    End Sub
    
    #End Region
    
    #Region "Input Handling"
    
    Private Sub HandleInput(deltaTime As Double)
        If Not inputEnabled Then Return
        
        Dim moveSpeed = PLAYER_SPEED * deltaTime
        Dim rotSpeed = PLAYER_ROTATION_SPEED * deltaTime
        
        ' Movement
        If keyStates(Keys.W) Then
            currentPlayer.Position += New Vector3D(
                -Sin(currentPlayer.Rotation.Y) * moveSpeed,
                0,
                -Cos(currentPlayer.Rotation.Y) * moveSpeed
            )
        End If
        If keyStates(Keys.S) Then
            currentPlayer.Position -= New Vector3D(
                -Sin(currentPlayer.Rotation.Y) * moveSpeed,
                0,
                -Cos(currentPlayer.Rotation.Y) * moveSpeed
            )
        End If
        If keyStates(Keys.A) Then
            currentPlayer.Position += New Vector3D(
                -Cos(currentPlayer.Rotation.Y) * moveSpeed,
                0,
                Sin(currentPlayer.Rotation.Y) * moveSpeed
            )
        End If
        If keyStates(Keys.D) Then
            currentPlayer.Position -= New Vector3D(
                -Cos(currentPlayer.Rotation.Y) * moveSpeed,
                0,
                Sin(currentPlayer.Rotation.Y) * moveSpeed
            )
        End If
        
        ' Jump
        If keyStates(Keys.Space) AndAlso currentPlayer.Position.Y <= 0 Then
            currentPlayer.Velocity = New Vector3D(
                currentPlayer.Velocity.X,
                10.0,
                currentPlayer.Velocity.Z
            )
        End If
        
        ' Apply gravity
        currentPlayer.Velocity = New Vector3D(
            currentPlayer.Velocity.X,
            currentPlayer.Velocity.Y - GRAVITY * deltaTime,
            currentPlayer.Velocity.Z
        )
        currentPlayer.Position += currentPlayer.Velocity * deltaTime
        
        ' Ground collision
        If currentPlayer.Position.Y < 0 Then
            currentPlayer.Position = New Vector3D(
                currentPlayer.Position.X,
                0,
                currentPlayer.Position.Z
            )
            currentPlayer.Velocity = New Vector3D(
                currentPlayer.Velocity.X,
                0,
                currentPlayer.Velocity.Z
            )
        End If
        
        ' Rotation (mouse look)
        If isMouseLookEnabled Then
            Dim mouseDelta = MousePosition - lastMousePos
            currentPlayer.Rotation = New Vector3D(
                currentPlayer.Rotation.X,
                currentPlayer.Rotation.Y - mouseDelta.X * rotSpeed,
                currentPlayer.Rotation.Z
            )
            Cursor.Position = New Point(Me.Location.X + SCREEN_WIDTH / 2, Me.Location.Y + SCREEN_HEIGHT / 2)
            lastMousePos = New Point(SCREEN_WIDTH / 2, SCREEN_HEIGHT / 2)
        End If
        
        ' Actions
        If keyStates(Keys.F) Then
            currentPlayer.FireArrow(GetAimTarget())
            keyStates(Keys.F) = False ' Single shot
        End If
        
        If keyStates(Keys.R) Then
            ' Reload/switch arrow type
            Dim types = [Enum].GetValues(GetType(ArrowType))
            Dim currentIndex = Array.IndexOf(types, currentPlayer.CurrentArrowType)
            currentIndex = (currentIndex + 1) Mod types.Length
            currentPlayer.CurrentArrowType = CType(types.GetValue(currentIndex), ArrowType)
            GameWorld.AddMessage($"Switched to {currentPlayer.CurrentArrowType} arrows")
            keyStates(Keys.R) = False
        End If
        
        If keyStates(Keys.Tab) Then
            gameState = GameState.Inventory
            keyStates(Keys.Tab) = False
        End If
        
        If keyStates(Keys.M) Then
            gameState = GameState.Map
            keyStates(Keys.M) = False
        End If
        
        If keyStates(Keys.Escape) Then
            If gameState = GameState.Playing Then
                gameState = GameState.Paused
            ElseIf gameState = GameState.Paused Then
                gameState = GameState.Playing
            End If
            keyStates(Keys.Escape) = False
        End If
    End Sub
    
    Private Function GetAimTarget() As Vector3D
        ' Calculate aim point based on camera and mouse
        Return gameCamera.Target
    End Function
    
    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        If inputEnabled AndAlso Not e.Handled Then
            keyStates(e.KeyCode) = True
        End If
        
        ' Special keys
        If e.KeyCode = Keys.F1 Then
            ' Toggle mouse look
            isMouseLookEnabled = Not isMouseLookEnabled
            If isMouseLookEnabled Then
                Cursor.Hide()
                Cursor.Position = New Point(Me.Location.X + SCREEN_WIDTH / 2, Me.Location.Y + SCREEN_HEIGHT / 2)
                lastMousePos = New Point(SCREEN_WIDTH / 2, SCREEN_HEIGHT / 2)
            Else
                Cursor.Show()
            End If
        End If
    End Sub
    
    Protected Overrides Sub OnKeyUp(e As KeyEventArgs)
        MyBase.OnKeyUp(e)
        If inputEnabled Then
            keyStates(e.KeyCode) = False
        End If
    End Sub
    
    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        If Not isMouseLookEnabled Then
            lastMousePos = e.Location
        End If
    End Sub
    
    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        If e.Button = MouseButtons.Left AndAlso gameState = GameState.Playing Then
            currentPlayer.FireArrow(GetAimTarget())
        End If
    End Sub
    
    #End Region
    
    #Region "Rendering"
    
    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.HighQuality
        g.TextRenderingHint = TextRenderingHint.AntiAlias
        g.InterpolationMode = InterpolationMode.Bicubic
        
        Select Case gameState
            Case GameState.SplashScreen
                RenderSplashScreen(g)
            Case GameState.MainMenu
                RenderMainMenu(g)
            Case GameState.Loading
                RenderLoading(g)
            Case GameState.Playing, GameState.Combat
                RenderGame(g)
                RenderHUD(g)
            Case GameState.Paused
                RenderGame(g)
                RenderPauseMenu(g)
            Case GameState.Inventory
                RenderInventory(g)
            Case GameState.Map
                RenderMap(g)
            Case GameState.Dialogue
                RenderGame(g)
                RenderDialogue(g)
        End Select
        
        ' Always render FPS
        RenderFPS(g)
    End Sub
    
    Private Sub RenderSplashScreen(g As Graphics)
        Dim rect As New Rectangle(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT)
        
        ' Draw gradient background
        Using gradientBrush As New LinearGradientBrush(
            rect,
            Color.FromArgb(0, 20, 0),
            Color.FromArgb(0, 80, 0),
            LinearGradientMode.Vertical)
            g.FillRectangle(gradientBrush, rect)
        End Using
        
        ' Draw title with fade effect
        Dim title = "ARROW"
        Dim subtitle = "DARK CITY"
        Dim titleSize = g.MeasureString(title, gameFonts("Title"))
        Dim subtitleSize = g.MeasureString(subtitle, gameFonts("Menu"))
        
        Dim titleX = (SCREEN_WIDTH - titleSize.Width) / 2
        Dim titleY = (SCREEN_HEIGHT - titleSize.Height - subtitleSize.Height) / 2
        
        Using brush As New SolidBrush(Color.FromArgb(splashAlpha, 0, 255, 0))
            g.DrawString(title, gameFonts("Title"), brush, titleX, titleY)
            g.DrawString(subtitle, gameFonts("Menu"), brush, titleX + 50, titleY + titleSize.Height)
        End Using
        
        ' Draw version
        g.DrawString($"Version {VERSION}", gameFonts("Small"), gameBrushes("White"), 10, SCREEN_HEIGHT - 30)
        
        ' Draw copyright
        g.DrawString("© 2024 Queen Consolidated", gameFonts("Small"), gameBrushes("White"), SCREEN_WIDTH - 200, SCREEN_HEIGHT - 30)
    End Sub
    
    Private Sub RenderMainMenu(g As Graphics)
        Dim rect As New Rectangle(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT)
        
        ' Draw background image (simulated with gradient)
        Using gradientBrush As New LinearGradientBrush(
            rect,
            Color.FromArgb(0, 30, 0),
            Color.Black,
            LinearGradientMode.ForwardDiagonal)
            g.FillRectangle(gradientBrush, rect)
        End Using
        
        ' Draw title
        Dim titleSize = g.MeasureString("ARROW", gameFonts("Title"))
        g.DrawString("ARROW", gameFonts("Title"), gameBrushes("Green"), (SCREEN_WIDTH - titleSize.Width) / 2, 100)
        
        ' Draw menu items
        Dim startY = 300
        For i = 0 To menuItems.Count - 1
            Dim color = If(i = menuSelectedIndex, Color.Yellow, Color.White)
            Using brush As New SolidBrush(color)
                Dim size = g.MeasureString(menuItems(i), gameFonts("Menu"))
                g.DrawString(menuItems(i), gameFonts("Menu"), brush, (SCREEN_WIDTH - size.Width) / 2, startY + i * 60)
            End Using
            
            ' Draw selection arrow
            If i = menuSelectedIndex Then
                g.DrawString(">", gameFonts("Menu"), Brushes.Yellow, (SCREEN_WIDTH - 200) / 2, startY + i * 60)
            End If
        Next
        
        ' Draw instructions
        g.DrawString("Use ARROW KEYS to navigate, ENTER to select", gameFonts("Small"), gameBrushes("White"), 20, SCREEN_HEIGHT - 50)
    End Sub
    
    Private Sub RenderLoading(g As Graphics)
        Dim rect As New Rectangle(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT)
        g.FillRectangle(Brushes.Black, rect)
        
        g.DrawString("LOADING STAR CITY...", gameFonts("Title"), gameBrushes("Green"), 200, 300)
        
        ' Progress bar
        Dim barRect As New Rectangle(200, 400, 800, 40)
        g.DrawRectangle(Pens.White, barRect)
        g.FillRectangle(Brushes.Green, barRect.X, barRect.Y, barRect.Width * loadingProgress / 100, barRect.Height)
        
        g.DrawString($"{loadingProgress}%", gameFonts("Menu"), gameBrushes("White"), 550, 450)
    End Sub
    
    Private Sub RenderGame(g As Graphics)
        ' Clear screen
        g.Clear(Color.Black)
        
        ' Get ambient color from weather
        Dim ambientColor = weather.GetAmbientColor()
        
        ' Draw sky gradient
        Dim skyRect As New Rectangle(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT)
        Using gradientBrush As New LinearGradientBrush(
            skyRect,
            ambientColor,
            Color.FromArgb(ambientColor.R \ 3, ambientColor.G \ 3, ambientColor.B \ 3),
            LinearGradientMode.Vertical)
            g.FillRectangle(gradientBrush, skyRect)
        End Using
        
        ' Draw ground grid
        DrawGroundGrid(g)
        
        ' Draw buildings
        For Each building In GameWorld.Districts(GameWorld.CurrentDistrict.Type).Buildings
            DrawBuilding(g, building)
        Next
        
        ' Draw NPCs and enemies
        For Each obj In GameWorld.GameObjects
            If TypeOf obj Is NPC OrElse TypeOf obj Is Enemy Then
                DrawCharacter(g, DirectCast(obj, Character))
            End If
        Next
        
        ' Draw projectiles
        For Each proj In GameWorld.Projectiles
            DrawProjectile(g, proj)
        Next
        
        ' Draw particles
        GameWorld.Particles.Render(g, gameCamera)
        
        ' Draw lights
        DrawLights(g)
        
        ' Draw rain if needed
        If weather.RainIntensity > 0 Then
            DrawRain(g)
        End If
        
        ' Draw fog if needed
        If weather.FogDensity > 0 Then
            DrawFog(g)
        End If
        
        ' Draw crosshair if aiming
        If gameCamera.IsAiming Then
            DrawCrosshair(g)
        End If
    End Sub
    
    Private Sub DrawGroundGrid(g As Graphics)
        Dim gridSize = 50
        Dim gridRange = 2000
        
        Using pen As New Pen(Color.FromArgb(50, 0, 255, 0))
            For x = -gridRange To gridRange Step gridSize
                Dim startPos = New Vector3D(x, 0, -gridRange)
                Dim endPos = New Vector3D(x, 0, gridRange)
                
                Dim startScreen = gameCamera.WorldToScreen(startPos)
                Dim endScreen = gameCamera.WorldToScreen(endPos)
                
                If startScreen.Z > 0 AndAlso endScreen.Z > 0 Then
                    g.DrawLine(pen, startScreen.X, startScreen.Y, endScreen.X, endScreen.Y)
                End If
            Next
            
            For z = -gridRange To gridRange Step gridSize
                Dim startPos = New Vector3D(-gridRange, 0, z)
                Dim endPos = New Vector3D(gridRange, 0, z)
                
                Dim startScreen = gameCamera.WorldToScreen(startPos)
                Dim endScreen = gameCamera.WorldToScreen(endPos)
                
                If startScreen.Z > 0 AndAlso endScreen.Z > 0 Then
                    g.DrawLine(pen, startScreen.X, startScreen.Y, endScreen.X, endScreen.Y)
                End If
            Next
        End Using
    End Sub
    
    Private Sub DrawBuilding(g As Graphics, building As Building)
        Dim screenPos = gameCamera.WorldToScreen(building.Position)
        
        If screenPos.Z > 0 AndAlso screenPos.Z < 1000 Then
            Dim scale = 500 / screenPos.Z
            Dim width = building.Scale.X * scale
            Dim height = building.Scale.Y * scale
            
            Dim rect As New Rectangle(
                CInt(screenPos.X - width / 2),
                CInt(screenPos.Y - height),
                CInt(width),
                CInt(height)
            )
            
            ' Draw building silhouette
            Using brush As New SolidBrush(Color.FromArgb(100, 30, 30, 30))
                g.FillRectangle(brush, rect)
            End Using
            
            ' Draw outline
            Using pen As New Pen(Color.FromArgb(100, 0, 255, 0))
                g.DrawRectangle(pen, rect)
            End Using
            
            ' Draw windows
            For Each window In building.Windows
                If window.IsLit Then
                    Dim windowPos = gameCamera.WorldToScreen(window.GetPosition())
                    If windowPos.Z > 0 Then
                        Dim windowSize = 2 * scale
                        Using brush As New SolidBrush(Color.FromArgb(200, window.LightColor))
                            g.FillRectangle(brush, windowPos.X - windowSize / 2, windowPos.Y - windowSize / 2, windowSize, windowSize)
                        End Using
                    End If
                End If
            Next
        End If
    End Sub
    
    Private Sub DrawCharacter(g As Graphics, character As Character)
        Dim screenPos = gameCamera.WorldToScreen(character.Position)
        
        If screenPos.Z > 0 AndAlso screenPos.Z < 500 Then
            Dim scale = 300 / screenPos.Z
            Dim size = 10 * scale
            
            ' Determine color based on character type
            Dim color As Color
            If TypeOf character Is Player Then
                color = Color.Green
            ElseIf TypeOf character Is Enemy Then
                color = Color.Red
            Else
                color = Color.Blue
            End If
            
            ' Draw character
            Using brush As New SolidBrush(color)
                g.FillEllipse(brush, screenPos.X - size / 2, screenPos.Y - size, size, size * 2)
            End Using
            
            ' Draw health bar
            If character.Health < character.MaxHealth Then
                Dim healthPercent = character.Health / character.MaxHealth
                Dim barWidth = size * 2
                Dim barHeight = 3 * scale
                
                Using brush As New SolidBrush(Color.Red)
                    g.FillRectangle(brush, screenPos.X - barWidth / 2, screenPos.Y - size * 2, barWidth, barHeight)
                End Using
                
                Using brush As New SolidBrush(Color.Green)
                    g.FillRectangle(brush, screenPos.X - barWidth / 2, screenPos.Y - size * 2, barWidth * healthPercent, barHeight)
                End Using
            End If
            
            ' Draw name
            If Not String.IsNullOrEmpty(character.Name) Then
                g.DrawString(character.Name, gameFonts("Small"), Brushes.White, screenPos.X - 30, screenPos.Y - size * 2.5F)
            End If
        End If
    End Sub
    
    Private Sub DrawProjectile(g As Graphics, proj As ArrowProjectile)
        Dim screenPos = gameCamera.WorldToScreen(proj.Position)
        
        If screenPos.Z > 0 Then
            Dim scale = 200 / screenPos.Z
            Dim size = 3 * scale
            
            ' Determine color based on arrow type
            Dim color As Color
            Select Case proj.ArrowType
                Case ArrowType.Standard
                    color = Color.White
                Case ArrowType.Explosive
                    color = Color.Orange
                Case ArrowType.Electric
                    color = Color.Cyan
                Case ArrowType.Smoke
                    color = Color.Gray
                Case Else
                    color = Color.Yellow
            End Select
            
            Using brush As New SolidBrush(color)
                g.FillRectangle(brush, screenPos.X - size / 2, screenPos.Y - size / 2, size, size * 3)
            End Using
            
            ' Draw trail
            If proj.Trail.Count > 1 Then
                Using pen As New Pen(Color.FromArgb(100, color))
                    For i = 0 To proj.Trail.Count - 2
                        Dim p1 = gameCamera.WorldToScreen(proj.Trail(i))
                        Dim p2 = gameCamera.WorldToScreen(proj.Trail(i + 1))
                        If p1.Z > 0 AndAlso p2.Z > 0 Then
                            g.DrawLine(pen, p1.X, p1.Y, p2.X, p2.Y)
                        End If
                    Next
                End Using
            End If
        End If
    End Sub
    
    Private Sub DrawLights(g As Graphics)
        For Each light In GameWorld.Lights
            Dim screenPos = gameCamera.WorldToScreen(light.Position)
            
            If screenPos.Z > 0 AndAlso screenPos.Z < light.Range * 2 Then
                Dim distance = screenPos.Z
                Dim intensity = light.Intensity * (1 - distance / light.Range)
                
                If intensity > 0 Then
                    Dim radius = 20 * intensity
                    Using brush As New SolidBrush(Color.FromArgb(CInt(50 * intensity), light.Color))
                        g.FillEllipse(brush, screenPos.X - radius, screenPos.Y - radius, radius * 2, radius * 2)
                    End Using
                End If
            End If
        Next
    End Sub
    
    Private Sub DrawRain(g As Graphics)
        Dim rainCount = CInt(100 * weather.RainIntensity)
        
        Using pen As New Pen(Color.FromArgb(100, 150, 150, 255))
            For i = 1 To rainCount
                Dim x = Rnd() * SCREEN_WIDTH
                Dim y = Rnd() * SCREEN_HEIGHT
                Dim length = 10 + Rnd() * 20
                
                ' Apply wind effect
                Dim windOffset = weather.WindSpeed * 2
                g.DrawLine(pen, x, y, x + windOffset, y + length)
            Next
        End Using
    End Sub
    
    Private Sub DrawFog(g As Graphics)
        Dim fogRect As New Rectangle(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT)
        Dim alpha = CInt(100 * weather.FogDensity)
        Using brush As New SolidBrush(Color.FromArgb(alpha, 200, 200, 200))
            g.FillRectangle(brush, fogRect)
        End Using
    End Sub
    
    Private Sub DrawCrosshair(g As Graphics)
        Dim centerX = SCREEN_WIDTH / 2
        Dim centerY = SCREEN_HEIGHT / 2
        Dim size = 20
        
        Using pen As New Pen(Color.FromArgb(200, 0, 255, 0))
            g.DrawLine(pen, centerX - size, centerY, centerX + size, centerY)
            g.DrawLine(pen, centerX, centerY - size, centerX, centerY + size)
            
            ' Draw outer circle
            g.DrawEllipse(pen, centerX - size / 2, centerY - size / 2, size, size)
        End Using
    End Sub
    
    Private Sub RenderHUD(g As Graphics)
        ' Health bar
        Dim healthPercent = currentPlayer.Health / currentPlayer.MaxHealth
        Dim healthBarWidth = 200
        Dim healthBarHeight = 20
        Dim healthX = 20
        Dim healthY = 20
        
        ' Health background
        g.FillRectangle(Brushes.DarkRed, healthX, healthY, healthBarWidth, healthBarHeight)
        g.FillRectangle(Brushes.Red, healthX, healthY, healthBarWidth * healthPercent, healthBarHeight)
        g.DrawRectangle(Pens.White, healthX, healthY, healthBarWidth, healthBarHeight)
        
        g.DrawString($"Health: {currentPlayer.Health}/{currentPlayer.MaxHealth}", gameFonts("HUD"), Brushes.White, healthX, healthY + healthBarHeight + 5)
        
        ' Arrow count
        Dim arrowY = healthY + healthBarHeight + 30
        For Each kvp In currentPlayer.Arrows
            Dim color = If(kvp.Key = currentPlayer.CurrentArrowType, Color.Yellow, Color.White)
            Using brush As New SolidBrush(color)
                g.DrawString($"{kvp.Key}: {kvp.Value}", gameFonts("HUD"), brush, healthX, arrowY)
                arrowY += 20
            End Using
        Next
        
        ' Experience bar
        Dim expPercent = currentPlayer.Experience / (currentPlayer.Level * 100.0)
        Dim expBarWidth = 200
        Dim expBarHeight = 10
        Dim expX = SCREEN_WIDTH - expBarWidth - 20
        Dim expY = 20
        
        g.FillRectangle(Brushes.DarkBlue, expX, expY, expBarWidth, expBarHeight)
        g.FillRectangle(Brushes.Cyan, expX, expY, expBarWidth * expPercent, expBarHeight)
        g.DrawRectangle(Pens.White, expX, expY, expBarWidth, expBarHeight)
        
        g.DrawString($"Level {currentPlayer.Level}", gameFonts("HUD"), Brushes.White, expX, expY - 20)
        
        ' District info
        If GameWorld.CurrentDistrict IsNot Nothing Then
            g.DrawString($"District: {GameWorld.CurrentDistrict.Name}", gameFonts("HUD"), Brushes.White, SCREEN_WIDTH / 2 - 100, 20)
            g.DrawString($"Crime Rate: {GameWorld.CurrentDistrict.CrimeRate}%", gameFonts("HUD"), Brushes.White, SCREEN_WIDTH / 2 - 100, 40)
        End If
        
        ' Weather info
        g.DrawString($"Weather: {weather.CurrentWeather}", gameFonts("HUD"), Brushes.White, SCREEN_WIDTH / 2 - 100, 60)
        g.DrawString($"Time: {weather.TimeOfDay:F1}:00", gameFonts("HUD"), Brushes.White, SCREEN_WIDTH / 2 - 100, 80)
        
        ' Recent messages
        Dim msgY = SCREEN_HEIGHT - 200
        For i = GameWorld.Messages.Count - 1 To 0 Step -1
            Dim alpha = 255 - (GameWorld.Messages.Count - 1 - i) * 50
            Using brush As New SolidBrush(Color.FromArgb(alpha, 255, 255, 255))
                g.DrawString(GameWorld.Messages(i), gameFonts("Normal"), brush, 20, msgY)
                msgY -= 25
            End Using
        Next
        
        ' Controls hint
        g.DrawString("F1: Toggle Mouse Look | F: Fire | R: Switch Arrows | Tab: Inventory | M: Map | ESC: Pause",
                    gameFonts("Small"), Brushes.White, 20, SCREEN_HEIGHT - 30)
    End Sub
    
    Private Sub RenderPauseMenu(g As Graphics)
        ' Darken screen
        Using brush As New SolidBrush(Color.FromArgb(128, 0, 0, 0))
            g.FillRectangle(brush, 0, 0, SCREEN_WIDTH, SCREEN_HEIGHT)
        End Using
        
        ' Draw pause menu
        g.DrawString("PAUSED", gameFonts("Title"), Brushes.White, SCREEN_WIDTH / 2 - 150, 200)
        
        Dim menuY = 300
        Dim pauseMenuItems = {"RESUME", "OPTIONS", "SAVE GAME", "LOAD GAME", "QUIT TO MENU"}
        
        For i = 0 To pauseMenuItems.Length - 1
            g.DrawString(pauseMenuItems[i], gameFonts("Menu"), Brushes.White, SCREEN_WIDTH / 2 - 100, menuY + i * 50)
        Next
        
        g.DrawString("Press ESC to resume", gameFonts("Normal"), Brushes.White, SCREEN_WIDTH / 2 - 100, 600)
    End Sub
    
    Private Sub RenderInventory(g As Graphics)
        ' Darken screen
        Using brush As New SolidBrush(Color.FromArgb(200, 0, 0, 0))
            g.FillRectangle(brush, 0, 0, SCREEN_WIDTH, SCREEN_HEIGHT)
        End Using
        
        g.DrawString("INVENTORY", gameFonts("Title"), Brushes.Green, 50, 50)
        
        ' Draw inventory grid
        Dim startX = 100
        Dim startY = 150
        Dim slotSize = 80
        Dim slotsPerRow = 8
        
        For i = 0 To currentPlayer.Inventory.Count - 1
            Dim row = i \ slotsPerRow
            Dim col = i Mod slotsPerRow
            
            Dim x = startX + col * (slotSize + 10)
            Dim y = startY + row * (slotSize + 10)
            
            ' Draw slot background
            g.FillRectangle(Brushes.DarkGray, x, y, slotSize, slotSize)
            g.DrawRectangle(Pens.White, x, y, slotSize, slotSize)
            
            ' Draw item name (simplified)
            g.DrawString(currentPlayer.Inventory(i).Name, gameFonts("Small"), Brushes.White, x + 5, y + 5)
        Next
        
        ' Equipment panel
        Dim equipX = SCREEN_WIDTH - 300
        Dim equipY = 150
        
        g.DrawString("EQUIPMENT", gameFonts("Menu"), Brushes.Green, equipX, equipY)
        equipY += 50
        
        For Each kvp In currentPlayer.Equipment
            g.DrawString($"{kvp.Key}: {kvp.Value.Name}", gameFonts("Normal"), Brushes.White, equipX, equipY)
            equipY += 30
        Next
        
        g.DrawString("Press TAB to close", gameFonts("Normal"), Brushes.White, 50, SCREEN_HEIGHT - 50)
    End Sub
    
    Private Sub RenderMap(g As Graphics)
        ' Darken screen
        Using brush As New SolidBrush(Color.FromArgb(200, 0, 0, 20))
            g.FillRectangle(brush, 0, 0, SCREEN_WIDTH, SCREEN_HEIGHT)
        End Using
        
        g.DrawString("STAR CITY MAP", gameFonts("Title"), Brushes.Green, 50, 50)
        
        ' Draw minimap
        Dim mapX = 100
        Dim mapY = 150
        Dim mapWidth = 800
        Dim mapHeight = 400
        Dim mapCenter As New Point(mapX + mapWidth / 2, mapY + mapHeight / 2)
        
        ' Map background
        g.FillRectangle(Brushes.DarkSlateGray, mapX, mapY, mapWidth, mapHeight)
        g.DrawRectangle(Pens.White, mapX, mapY, mapWidth, mapHeight)
        
        ' Draw districts
        For Each district In GameWorld.Districts.Values
            Dim districtColor As Color
            Select Case district.Type
                Case DistrictType.TheGlades
                    districtColor = Color.DarkRed
                Case DistrictType.Downtown
                    districtColor = Color.DarkBlue
                Case DistrictType.StarlingHeights
                    districtColor = Color.DarkGreen
                Case Else
                    districtColor = Color.Gray
            End