Imports OpenTK
Imports OpenTK.Graphics.OpenGL
Imports System.Drawing
Imports System.Windows.Forms

Public Class StereoscopicArrowScene
    Inherits Form

    Private glControl As GLControl
    Private timer As Timer

    ' Camera parameters
    Private eyeDistance As Single = 0.3F        ' Distance between left/right eyes
    Private focalDistance As Single = 10.0F     ' Distance to focal plane
    Private rotationAngle As Single = 0.0F

    Public Sub New()
        InitializeComponent()
        SetupOpenGL()
        SetupTimer()
    End Sub

    Private Sub InitializeComponent()
        Me.Text = "ARROW TV - Stereoscopic 3D Scene"
        Me.ClientSize = New Size(1024, 768)
        Me.StartPosition = FormStartPosition.CenterScreen

        glControl = New GLControl() With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.Black
        }
        AddHandler glControl.Load, AddressOf GlControl_Load
        AddHandler glControl.Paint, AddressOf GlControl_Paint
        AddHandler glControl.Resize, AddressOf GlControl_Resize
        Me.Controls.Add(glControl)
    End Sub

    Private Sub SetupOpenGL()
        ' Basic OpenGL settings
        GL.Enable(EnableCap.DepthTest)
        GL.Enable(EnableCap.Lighting)
        GL.Enable(EnableCap.Light0)
        GL.Enable(EnableCap.ColorMaterial)
        GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse)

        ' Light position
        Dim lightPos As Single() = {5.0F, 10.0F, 5.0F, 1.0F}
        GL.Light(LightName.Light0, LightParameter.Position, lightPos)
        GL.Light(LightName.Light0, LightParameter.Ambient, New Single() {0.2F, 0.2F, 0.2F, 1.0F})
        GL.Light(LightName.Light0, LightParameter.Diffuse, New Single() {0.8F, 0.8F, 0.8F, 1.0F})
    End Sub

    Private Sub SetupTimer()
        timer = New Timer() With {.Interval = 16} ' ~60 FPS
        AddHandler timer.Tick, AddressOf Timer_Tick
        timer.Start()
    End Sub

    Private Sub GlControl_Load(sender As Object, e As EventArgs)
        GL.ClearColor(Color.SkyBlue)
        GL.Enable(EnableCap.DepthTest)
    End Sub

    Private Sub GlControl_Resize(sender As Object, e As EventArgs)
        GL.Viewport(0, 0, glControl.Width, glControl.Height)
        glControl.Invalidate()
    End Sub

    Private Sub Timer_Tick(sender As Object, e As EventArgs)
        rotationAngle += 1.0F ' Rotate scene slowly
        glControl.Invalidate()
    End Sub

    Private Sub GlControl_Paint(sender As Object, e As PaintEventArgs)
        RenderStereoscopic()
        glControl.SwapBuffers()
    End Sub

    Private Sub RenderStereoscopic()
        ' Clear both colour and depth buffers
        GL.Clear(ClearBufferMask.ColorBufferBit Or ClearBufferMask.DepthBufferBit)

        ' Store projection and modelview matrices
        GL.MatrixMode(MatrixMode.Projection)
        GL.PushMatrix()
        GL.MatrixMode(MatrixMode.Modelview)
        GL.PushMatrix()

        ' --- LEFT EYE (RED) ---
        SetupEye(eyeOffset:=-eyeDistance / 2, colorMask:=New Boolean() {True, False, False, True})
        DrawScene()

        ' Clear depth for right eye (preserve left eye colour)
        GL.Clear(ClearBufferMask.DepthBufferBit)

        ' --- RIGHT EYE (CYAN) ---
        SetupEye(eyeOffset:=eyeDistance / 2, colorMask:=New Boolean() {False, True, True, True})
        DrawScene()

        ' Restore matrices
        GL.MatrixMode(MatrixMode.Projection)
        GL.PopMatrix()
        GL.MatrixMode(MatrixMode.Modelview)
        GL.PopMatrix()

        ' Reset colour mask
        GL.ColorMask(True, True, True, True)
    End Sub

    Private Sub SetupEye(eyeOffset As Single, colorMask As Boolean())
        ' Set projection matrix (perspective)
        GL.MatrixMode(MatrixMode.Projection)
        GL.LoadIdentity()
        Dim aspect As Double = glControl.Width / glControl.Height
        Dim fovy As Double = 60.0
        Dim nearZ As Double = 1.0
        Dim farZ As Double = 100.0
        Dim top As Double = nearZ * Math.Tan(fovy * Math.PI / 360.0)
        Dim right As Double = top * aspect

        ' Apply off-axis projection for stereoscopy
        Dim frustumShift As Double = (eyeOffset * nearZ) / focalDistance
        GL.Frustum(-right - frustumShift, right - frustumShift, -top, top, nearZ, farZ)

        ' Modelview: position camera
        GL.MatrixMode(MatrixMode.Modelview)
        GL.LoadIdentity()
        GL.Translate(-eyeOffset, 0, 0) ' Move eye laterally
        GL.Translate(0, -2, -25)        ' Move back and down a bit
        GL.Rotate(rotationAngle, 0, 1, 0) ' Rotate around Y for dynamic view

        ' Set colour mask for anaglyph
        GL.ColorMask(colorMask(0), colorMask(1), colorMask(2), colorMask(3))
    End Sub

    Private Sub DrawScene()
        ' --- GROUND ---
        GL.Begin(PrimitiveType.Quads)
        GL.Color3(0.3, 0.3, 0.3) ' Grey ground
        GL.Normal3(0, 1, 0)
        GL.Vertex3(-10, -1, -10)
        GL.Vertex3(10, -1, -10)
        GL.Vertex3(10, -1, 10)
        GL.Vertex3(-10, -1, 10)
        GL.End()

        ' --- BUILDINGS (simulating Starling City) ---
        Dim buildingPositions As Single()() = {
            New Single() {-5, 0, -5},
            New Single() {2, 0, -3},
            New Single() {4, 0, 2},
            New Single() {-3, 0, 4},
            New Single() {0, 0, 0}
        }
        Dim buildingColors As Color() = {
            Color.FromArgb(100, 100, 150),
            Color.FromArgb(150, 100, 100),
            Color.FromArgb(100, 150, 100),
            Color.FromArgb(120, 80, 120),
            Color.FromArgb(80, 120, 120)
        }
        For i As Integer = 0 To buildingPositions.Length - 1
            DrawBuilding(buildingPositions(i)(0), buildingPositions(i)(1), buildingPositions(i)(2), 1.5F, 3.0F, buildingColors(i))
        Next

        ' --- CHARACTER (Arrow) ---
        DrawArrowCharacter(0, 1, -2)
    End Sub

    Private Sub DrawBuilding(x As Single, y As Single, z As Single, width As Single, height As Single, col As Color)
        Dim halfW As Single = width / 2
        GL.Color3(col)
        GL.Normal3(0, 1, 0)

        ' Front face
        GL.Begin(PrimitiveType.Quads)
        GL.Vertex3(x - halfW, y, z + halfW)
        GL.Vertex3(x + halfW, y, z + halfW)
        GL.Vertex3(x + halfW, y + height, z + halfW)
        GL.Vertex3(x - halfW, y + height, z + halfW)
        GL.End()

        ' Back face
        GL.Begin(PrimitiveType.Quads)
        GL.Vertex3(x - halfW, y, z - halfW)
        GL.Vertex3(x - halfW, y + height, z - halfW)
        GL.Vertex3(x + halfW, y + height, z - halfW)
        GL.Vertex3(x + halfW, y, z - halfW)
        GL.End()

        ' Left face
        GL.Begin(PrimitiveType.Quads)
        GL.Vertex3(x - halfW, y, z - halfW)
        GL.Vertex3(x - halfW, y, z + halfW)
        GL.Vertex3(x - halfW, y + height, z + halfW)
        GL.Vertex3(x - halfW, y + height, z - halfW)
        GL.End()

        ' Right face
        GL.Begin(PrimitiveType.Quads)
        GL.Vertex3(x + halfW, y, z - halfW)
        GL.Vertex3(x + halfW, y + height, z - halfW)
        GL.Vertex3(x + halfW, y + height, z + halfW)
        GL.Vertex3(x + halfW, y, z + halfW)
        GL.End()

        ' Top face
        GL.Begin(PrimitiveType.Quads)
        GL.Vertex3(x - halfW, y + height, z - halfW)
        GL.Vertex3(x + halfW, y + height, z - halfW)
        GL.Vertex3(x + halfW, y + height, z + halfW)
        GL.Vertex3(x - halfW, y + height, z + halfW)
        GL.End()
    End Sub

    Private Sub DrawArrowCharacter(x As Single, y As Single, z As Single)
        ' Body (green suit)
        GL.Color3(0.0, 0.6, 0.0) ' Green
        GL.Begin(PrimitiveType.Quads)
        ' Torso
        GL.Vertex3(x - 0.3, y, z)
        GL.Vertex3(x + 0.3, y, z)
        GL.Vertex3(x + 0.3, y + 1.5, z)
        GL.Vertex3(x - 0.3, y + 1.5, z)

        ' Left arm
        GL.Vertex3(x - 0.6, y + 0.3, z)
        GL.Vertex3(x - 0.3, y + 0.3, z)
        GL.Vertex3(x - 0.3, y + 1.2, z)
        GL.Vertex3(x - 0.6, y + 1.2, z)

        ' Right arm
        GL.Vertex3(x + 0.3, y + 0.3, z)
        GL.Vertex3(x + 0.6, y + 0.3, z)
        GL.Vertex3(x + 0.6, y + 1.2, z)
        GL.Vertex3(x + 0.3, y + 1.2, z)
        GL.End()

        ' Head
        GL.Color3(1.0, 0.8, 0.6) ' Skin tone
        GL.Begin(PrimitiveType.Quads)
        GL.Vertex3(x - 0.2, y + 1.5, z)
        GL.Vertex3(x + 0.2, y + 1.5, z)
        GL.Vertex3(x + 0.2, y + 1.9, z)
        GL.Vertex3(x - 0.2, y + 1.9, z)
        GL.End()

        ' Hood (green)
        GL.Color3(0.0, 0.5, 0.0)
        GL.Begin(PrimitiveType.Triangles)
        GL.Vertex3(x - 0.25, y + 1.9, z)
        GL.Vertex3(x + 0.25, y + 1.9, z)
        GL.Vertex3(x, y + 2.2, z)
        GL.End()

        ' Bow (brown)
        GL.Color3(0.5, 0.3, 0.1)
        GL.LineWidth(3.0F)
        GL.Begin(PrimitiveType.Lines)
        GL.Vertex3(x - 0.4, y + 0.8, z + 0.2)
        GL.Vertex3(x + 0.4, y + 0.8, z + 0.2)
        GL.End()
        GL.LineWidth(1.0F)
    End Sub

    Public Shared Sub Main()
        Application.Run(New StereoscopicArrowScene())
    End Sub
End Class

' Helper class for OpenTK GLControl (needed for WinForms integration)
Public Class GLControl
    Inherits OpenTK.GLControl

    Public Sub New()
        MyBase.New(OpenTK.Graphics.GraphicsMode.Default, 3, 0, OpenTK.Graphics.GraphicsContextFlags.Default)
    End Sub
End Class