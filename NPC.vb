Imports OpenTK
Imports OpenTK.Graphics.OpenGL
Imports System.Drawing
Imports System.Windows.Forms

Public Class ArrowSandboxForm
    Inherits Form

    Private glControl As GLControl
    Private timer As Timer
    Private player As Player
    Private npcs As New List(Of Npc)
    Private waypoints As New List(Of Vector3)
    Private rnd As New Random()

    ' Camera
    Private camPos As Vector3 = New Vector3(0, 5, 15)
    Private camTarget As Vector3 = New Vector3(0, 2, 0)

    Public Sub New()
        InitializeComponent()
        InitGame()
    End Sub

    Private Sub InitializeComponent()
        Me.Text = "ARROW 3D Sandbox - Active NPCs"
        Me.ClientSize = New Size(1024, 768)
        Me.StartPosition = FormStartPosition.CenterScreen

        glControl = New GLControl() With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.Black
        }
        AddHandler glControl.Load, AddressOf GlControl_Load
        AddHandler glControl.Paint, AddressOf GlControl_Paint
        AddHandler glControl.Resize, AddressOf GlControl_Resize
        AddHandler glControl.KeyDown, AddressOf GlControl_KeyDown
        AddHandler glControl.KeyUp, AddressOf GlControl_KeyUp
        Me.Controls.Add(glControl)

        ' Focus for keyboard input
        glControl.Select()
    End Sub

    Private Sub InitGame()
        ' Player
        player = New Player(New Vector3(0, 1, 0))

        ' Create some waypoints around the world
        waypoints.Add(New Vector3(-5, 1, -5))
        waypoints.Add(New Vector3(5, 1, -5))
        waypoints.Add(New Vector3(5, 1, 5))
        waypoints.Add(New Vector3(-5, 1, 5))

        ' Create NPCs with patrol routes
        For i As Integer = 0 To 3
            Dim startPos = waypoints(i)
            Dim npc = New Npc(startPos, waypoints)
            npcs.Add(npc)
        Next

        ' Timer for game loop
        timer = New Timer() With {.Interval = 16}
        AddHandler timer.Tick, AddressOf Timer_Tick
        timer.Start()
    End Sub

    Private Sub GlControl_Load(sender As Object, e As EventArgs)
        GL.ClearColor(Color.SkyBlue)
        GL.Enable(EnableCap.DepthTest)
        GL.Enable(EnableCap.Lighting)
        GL.Enable(EnableCap.Light0)
        GL.Enable(EnableCap.ColorMaterial)
        GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse)

        Dim lightPos As Single() = {0, 10, 0, 1}
        GL.Light(LightName.Light0, LightParameter.Position, lightPos)
        GL.Light(LightName.Light0, LightParameter.Ambient, New Single() {0.2, 0.2, 0.2, 1})
        GL.Light(LightName.Light0, LightParameter.Diffuse, New Single() {0.8, 0.8, 0.8, 1})
    End Sub

    Private Sub GlControl_Resize(sender As Object, e As EventArgs)
        GL.Viewport(0, 0, glControl.Width, glControl.Height)
        Dim aspect As Double = glControl.Width / glControl.Height
        GL.MatrixMode(MatrixMode.Projection)
        GL.LoadIdentity()
        Matrix4.CreatePerspectiveFieldOfView(CSng(Math.PI / 3), CSng(aspect), 1.0F, 100.0F, Matrix4.op_Explicit)
        GL.MatrixMode(MatrixMode.Modelview)
    End Sub

    Private Sub Timer_Tick(sender As Object, e As EventArgs)
        ' Update player movement
        player.Update()

        ' Update NPCs (pass player position)
        For Each npc In npcs
            npc.Update(player.Position, npcs)
        Next

        ' Simple camera follow
        camTarget = player.Position
        camPos = player.Position + New Vector3(0, 5, 10)

        glControl.Invalidate()
    End Sub

    Private Sub GlControl_Paint(sender As Object, e As PaintEventArgs)
        GL.Clear(ClearBufferMask.ColorBufferBit Or ClearBufferMask.DepthBufferBit)

        ' Set camera
        GL.LoadIdentity()
        Matrix4.LookAt(camPos.X, camPos.Y, camPos.Z, camTarget.X, camTarget.Y, camTarget.Z, 0, 1, 0, Matrix4.op_Explicit)

        ' Draw ground
        DrawGround()

        ' Draw buildings (simple blocks)
        DrawBuildings()

        ' Draw player
        player.Draw()

        ' Draw NPCs
        For Each npc In npcs
            npc.Draw()
        Next

        glControl.SwapBuffers()
    End Sub

    Private Sub DrawGround()
        GL.Color3(0.3, 0.3, 0.3)
        GL.Begin(PrimitiveType.Quads)
        GL.Normal3(0, 1, 0)
        For x As Integer = -10 To 9
            For z As Integer = -10 To 9
                If (x + z) Mod 2 = 0 Then
                    GL.Color3(0.3, 0.3, 0.3)
                Else
                    GL.Color3(0.2, 0.2, 0.2)
                End If
                GL.Vertex3(x, 0, z)
                GL.Vertex3(x + 1, 0, z)
                GL.Vertex3(x + 1, 0, z + 1)
                GL.Vertex3(x, 0, z + 1)
            Next
        Next
        GL.End()
    End Sub

    Private Sub DrawBuildings()
        Dim positions As Vector3() = {
            New Vector3(-3, 0, -3),
            New Vector3(4, 0, -2),
            New Vector3(-2, 0, 4),
            New Vector3(5, 0, 3)
        }
        For Each pos In positions
            DrawCube(pos, 1.5F, 3.0F, Color.FromArgb(100, 100, 150))
        Next
    End Sub

    Private Sub DrawCube(center As Vector3, width As Single, height As Single, col As Color)
        Dim halfW = width / 2
        GL.Color3(col)
        ' Front
        GL.Begin(PrimitiveType.Quads)
        GL.Vertex3(center.X - halfW, center.Y, center.Z + halfW)
        GL.Vertex3(center.X + halfW, center.Y, center.Z + halfW)
        GL.Vertex3(center.X + halfW, center.Y + height, center.Z + halfW)
        GL.Vertex3(center.X - halfW, center.Y + height, center.Z + halfW)
        ' Back
        GL.Vertex3(center.X - halfW, center.Y, center.Z - halfW)
        GL.Vertex3(center.X - halfW, center.Y + height, center.Z - halfW)
        GL.Vertex3(center.X + halfW, center.Y + height, center.Z - halfW)
        GL.Vertex3(center.X + halfW, center.Y, center.Z - halfW)
        ' Left
        GL.Vertex3(center.X - halfW, center.Y, center.Z - halfW)
        GL.Vertex3(center.X - halfW, center.Y, center.Z + halfW)
        GL.Vertex3(center.X - halfW, center.Y + height, center.Z + halfW)
        GL.Vertex3(center.X - halfW, center.Y + height, center.Z - halfW)
        ' Right
        GL.Vertex3(center.X + halfW, center.Y, center.Z - halfW)
        GL.Vertex3(center.X + halfW, center.Y + height, center.Z - halfW)
        GL.Vertex3(center.X + halfW, center.Y + height, center.Z + halfW)
        GL.Vertex3(center.X + halfW, center.Y, center.Z + halfW)
        ' Top
        GL.Vertex3(center.X - halfW, center.Y + height, center.Z - halfW)
        GL.Vertex3(center.X - halfW, center.Y + height, center.Z + halfW)
        GL.Vertex3(center.X + halfW, center.Y + height, center.Z + halfW)
        GL.Vertex3(center.X + halfW, center.Y + height, center.Z - halfW)
        GL.End()
    End Sub

    Private Sub GlControl_KeyDown(sender As Object, e As KeyEventArgs)
        Select Case e.KeyCode
            Case Keys.W : player.MoveForward = True
            Case Keys.S : player.MoveBackward = True
            Case Keys.A : player.MoveLeft = True
            Case Keys.D : player.MoveRight = True
        End Select
    End Sub

    Private Sub GlControl_KeyUp(sender As Object, e As KeyEventArgs)
        Select Case e.KeyCode
            Case Keys.W : player.MoveForward = False
            Case Keys.S : player.MoveBackward = False
            Case Keys.A : player.MoveLeft = False
            Case Keys.D : player.MoveRight = False
        End Select
    End Sub
End Class

' Player class
Public Class Player
    Public Position As Vector3
    Public MoveForward As Boolean = False
    Public MoveBackward As Boolean = False
    Public MoveLeft As Boolean = False
    Public MoveRight As Boolean = False
    Private speed As Single = 0.2F

    Public Sub New(startPos As Vector3)
        Position = startPos
    End Sub

    Public Sub Update()
        Dim move As Vector3 = Vector3.Zero
        If MoveForward Then move += New Vector3(0, 0, -1)
        If MoveBackward Then move += New Vector3(0, 0, 1)
        If MoveLeft Then move += New Vector3(-1, 0, 0)
        If MoveRight Then move += New Vector3(1, 0, 0)
        If move.Length > 0 Then
            move.Normalize()
            Position += move * speed
        End If
    End Sub

    Public Sub Draw()
        GL.PushMatrix()
        GL.Translate(Position.X, Position.Y, Position.Z)
        ' Body (green)
        GL.Color3(0, 0.8, 0)
        DrawCylinder(0.3, 1.5)
        ' Head
        GL.Translate(0, 1.5, 0)
        GL.Color3(1, 0.8, 0.6)
        DrawSphere(0.25)
        ' Hood
        GL.Color3(0, 0.5, 0)
        DrawCone(0.3, 0.5)
        GL.PopMatrix()
    End Sub

    Private Sub DrawCylinder(radius As Single, height As Single)
        Dim segments = 16
        GL.Begin(PrimitiveType.Quads)
        For i As Integer = 0 To segments - 1
            Dim theta0 = 2 * Math.PI * i / segments
            Dim theta1 = 2 * Math.PI * (i + 1) / segments
            Dim x0 = radius * Math.Cos(theta0)
            Dim z0 = radius * Math.Sin(theta0)
            Dim x1 = radius * Math.Cos(theta1)
            Dim z1 = radius * Math.Sin(theta1)

            GL.Normal3(x0, 0, z0) : GL.Vertex3(x0, 0, z0)
            GL.Normal3(x1, 0, z1) : GL.Vertex3(x1, 0, z1)
            GL.Normal3(x1, 0, z1) : GL.Vertex3(x1, height, z1)
            GL.Normal3(x0, 0, z0) : GL.Vertex3(x0, height, z0)
        Next
        GL.End()
    End Sub

    Private Sub DrawSphere(radius As Single)
        ' Simple icosahedron approximation
        GL.Begin(PrimitiveType.Triangles)
        Dim t As Single = (1.0 + Math.Sqrt(5.0)) / 2.0
        Dim verts As Vector3() = {
            New Vector3(-1, t, 0), New Vector3(1, t, 0), New Vector3(-1, -t, 0), New Vector3(1, -t, 0),
            New Vector3(0, -1, t), New Vector3(0, 1, t), New Vector3(0, -1, -t), New Vector3(0, 1, -t),
            New Vector3(t, 0, -1), New Vector3(t, 0, 1), New Vector3(-t, 0, -1), New Vector3(-t, 0, 1)
        }
        For i = 0 To verts.Length - 1
            verts(i).Normalize()
            verts(i) *= radius
        Next
        ' Indices for triangles (20 faces)
        Dim faces As Integer()() = {
            New Integer() {0, 11, 5}, New Integer() {0, 5, 1}, New Integer() {0, 1, 7}, New Integer() {0, 7, 10}, New Integer() {0, 10, 11},
            New Integer() {1, 5, 9}, New Integer() {5, 11, 4}, New Integer() {11, 10, 2}, New Integer() {10, 7, 6}, New Integer() {7, 1, 8},
            New Integer() {3, 9, 4}, New Integer() {3, 4, 2}, New Integer() {3, 2, 6}, New Integer() {3, 6, 8}, New Integer() {3, 8, 9},
            New Integer() {4, 9, 5}, New Integer() {2, 4, 11}, New Integer() {6, 2, 10}, New Integer() {8, 6, 7}, New Integer() {9, 8, 1}
        }
        For Each f In faces
            For Each idx In f
                GL.Normal3(verts(idx).X, verts(idx).Y, verts(idx).Z)
                GL.Vertex3(verts(idx).X, verts(idx).Y, verts(idx).Z)
            Next
        Next
        GL.End()
    End Sub

    Private Sub DrawCone(radius As Single, height As Single)
        Dim segments = 16
        GL.Begin(PrimitiveType.Triangles)
        For i As Integer = 0 To segments - 1
            Dim theta0 = 2 * Math.PI * i / segments
            Dim theta1 = 2 * Math.PI * (i + 1) / segments
            Dim x0 = radius * Math.Cos(theta0)
            Dim z0 = radius * Math.Sin(theta0)
            Dim x1 = radius * Math.Cos(theta1)
            Dim z1 = radius * Math.Sin(theta1)

            GL.Normal3(x0, 0, z0) : GL.Vertex3(x0, 0, z0)
            GL.Normal3(x1, 0, z1) : GL.Vertex3(x1, 0, z1)
            GL.Normal3(0, 1, 0) : GL.Vertex3(0, height, 0)
        Next
        GL.End()
    End Sub
End Class

' NPC States
Public Enum NpcState
    Idle
    Patrol
    Chase
    Attack
End Enum

' NPC class
Public Class Npc
    Public Position As Vector3
    Public Velocity As Vector3
    Public State As NpcState = NpcState.Patrol
    Private waypoints As List(Of Vector3)
    Private currentWaypointIndex As Integer = 0
    Private detectionRange As Single = 5.0F
    Private attackRange As Single = 1.5F
    Private speed As Single = 0.1F
    Private patrolWaitTimer As Integer = 0
    Private rnd As New Random()

    Public Sub New(startPos As Vector3, wpList As List(Of Vector3))
        Position = startPos
        waypoints = New List(Of Vector3)(wpList)
        ' Shuffle waypoints for variety
        For i As Integer = 0 To waypoints.Count - 1
            Dim j = rnd.Next(i, waypoints.Count)
            Dim temp = waypoints(i)
            waypoints(i) = waypoints(j)
            waypoints(j) = temp
        Next
    End Sub

    Public Sub Update(playerPos As Vector3, allNpcs As List(Of Npc))
        Dim toPlayer = playerPos - Position
        Dim distToPlayer = toPlayer.Length

        ' State transitions
        Select Case State
            Case NpcState.Patrol
                If distToPlayer < detectionRange Then
                    State = NpcState.Chase
                Else
                    PatrolBehavior()
                End If

            Case NpcState.Chase
                If distToPlayer < attackRange Then
                    State = NpcState.Attack
                ElseIf distToPlayer > detectionRange * 1.5F Then
                    State = NpcState.Patrol
                Else
                    ChaseBehavior(playerPos)
                End If

            Case NpcState.Attack
                If distToPlayer > attackRange Then
                    State = NpcState.Chase
                Else
                    AttackBehavior()
                End If
        End Select

        ' Apply velocity and simple collision with other NPCs
        Position += Velocity
        Velocity *= 0.9F ' damping

        ' Keep on ground
        Position = New Vector3(Position.X, 1, Position.Z)
    End Sub

    Private Sub PatrolBehavior()
        ' Move toward current waypoint
        Dim target = waypoints(currentWaypointIndex)
        Dim dir = target - Position
        If dir.Length < 0.5 Then
            ' Reached waypoint, pick next
            currentWaypointIndex = (currentWaypointIndex + 1) Mod waypoints.Count
            patrolWaitTimer = 30 ' wait a bit
        Else
            dir.Normalize()
            Velocity += dir * speed
        End If
    End Sub

    Private Sub ChaseBehavior(playerPos As Vector3)
        Dim dir = playerPos - Position
        dir.Normalize()
        Velocity += dir * (speed * 1.5F)
    End Sub

    Private Sub AttackBehavior()
        ' In attack state, NPC just stays close and "attacks" (we'll just make them vibrate slightly)
        Velocity += New Vector3(rnd.NextSingle() * 0.2F - 0.1F, 0, rnd.NextSingle() * 0.2F - 0.1F)
    End Sub

    Public Sub Draw()
        GL.PushMatrix()
        GL.Translate(Position.X, Position.Y, Position.Z)

        ' Color based on state
        Select Case State
            Case NpcState.Patrol
                GL.Color3(0.5, 0.5, 0.5) ' grey
            Case NpcState.Chase
                GL.Color3(1, 0.5, 0) ' orange
            Case NpcState.Attack
                GL.Color3(1, 0, 0) ' red
        End Select

        ' Simple body: cylinder
        DrawCylinder(0.3, 1.5)
        ' Head
        GL.Translate(0, 1.5, 0)
        GL.Color3(0.8, 0.6, 0.4)
        DrawSphere(0.25)
        GL.PopMatrix()
    End Sub

    ' Drawing helpers (same as Player's, duplicated for simplicity)
    Private Sub DrawCylinder(radius As Single, height As Single)
        Dim segments = 16
        GL.Begin(PrimitiveType.Quads)
        For i As Integer = 0 To segments - 1
            Dim theta0 = 2 * Math.PI * i / segments
            Dim theta1 = 2 * Math.PI * (i + 1) / segments
            Dim x0 = radius * Math.Cos(theta0)
            Dim z0 = radius * Math.Sin(theta0)
            Dim x1 = radius * Math.Cos(theta1)
            Dim z1 = radius * Math.Sin(theta1)

            GL.Normal3(x0, 0, z0) : GL.Vertex3(x0, 0, z0)
            GL.Normal3(x1, 0, z1) : GL.Vertex3(x1, 0, z1)
            GL.Normal3(x1, 0, z1) : GL.Vertex3(x1, height, z1)
            GL.Normal3(x0, 0, z0) : GL.Vertex3(x0, height, z0)
        Next
        GL.End()
    End Sub

    Private Sub DrawSphere(radius As Single)
        ' Simple icosahedron (same as Player's)
        GL.Begin(PrimitiveType.Triangles)
        Dim t As Single = (1.0 + Math.Sqrt(5.0)) / 2.0
        Dim verts As Vector3() = {
            New Vector3(-1, t, 0), New Vector3(1, t, 0), New Vector3(-1, -t, 0), New Vector3(1, -t, 0),
            New Vector3(0, -1, t), New Vector3(0, 1, t), New Vector3(0, -1, -t), New Vector3(0, 1, -t),
            New Vector3(t, 0, -1), New Vector3(t, 0, 1), New Vector3(-t, 0, -1), New Vector3(-t, 0, 1)
        }
        For i = 0 To verts.Length - 1
            verts(i).Normalize()
            verts(i) *= radius
        Next
        Dim faces As Integer()() = {
            New Integer() {0, 11, 5}, New Integer() {0, 5, 1}, New Integer() {0, 1, 7}, New Integer() {0, 7, 10}, New Integer() {0, 10, 11},
            New Integer() {1, 5, 9}, New Integer() {5, 11, 4}, New Integer() {11, 10, 2}, New Integer() {10, 7, 6}, New Integer() {7, 1, 8},
            New Integer() {3, 9, 4}, New Integer() {3, 4, 2}, New Integer() {3, 2, 6}, New Integer() {3, 6, 8}, New Integer() {3, 8, 9},
            New Integer() {4, 9, 5}, New Integer() {2, 4, 11}, New Integer() {6, 2, 10}, New Integer() {8, 6, 7}, New Integer() {9, 8, 1}
        }
        For Each f In faces
            For Each idx In f
                GL.Normal3(verts(idx).X, verts(idx).Y, verts(idx).Z)
                GL.Vertex3(verts(idx).X, verts(idx).Y, verts(idx).Z)
            Next
        Next
        GL.End()
    End Sub
End Class

' Entry point
Module Program
    <STAThread>
    Public Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New ArrowSandboxForm())
    End Sub
End Module