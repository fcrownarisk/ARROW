Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

Public Class Arrow4DInventoryForm
    Inherits Form

    ' ----- 4D TESSERACT DEFINITION -----
    Private ReadOnly vertices4D As Double()() = {
        New Double() {-1, -1, -1, -1}, New Double() {1, -1, -1, -1},
        New Double() {1, -1, 1, -1}, New Double() {-1, -1, 1, -1},
        New Double() {-1, 1, -1, -1}, New Double() {1, 1, -1, -1},
        New Double() {1, 1, 1, -1}, New Double() {-1, 1, 1, -1},
        New Double() {-1, -1, -1, 1}, New Double() {1, -1, -1, 1},
        New Double() {1, -1, 1, 1}, New Double() {-1, -1, 1, 1},
        New Double() {-1, 1, -1, 1}, New Double() {1, 1, -1, 1},
        New Double() {1, 1, 1, 1}, New Double() {-1, 1, 1, 1}
    }

    Private ReadOnly edges As Integer()() = {
        New Integer() {0,1}, New Integer() {1,2}, New Integer() {2,3}, New Integer() {3,0},
        New Integer() {4,5}, New Integer() {5,6}, New Integer() {6,7}, New Integer() {7,4},
        New Integer() {0,4}, New Integer() {1,5}, New Integer() {2,6}, New Integer() {3,7},
        New Integer() {8,9}, New Integer() {9,10}, New Integer() {10,11}, New Integer() {11,8},
        New Integer() {12,13}, New Integer() {13,14}, New Integer() {14,15}, New Integer() {15,12},
        New Integer() {8,12}, New Integer() {9,13}, New Integer() {10,14}, New Integer() {11,15},
        New Integer() {0,8}, New Integer() {1,9}, New Integer() {2,10}, New Integer() {3,11},
        New Integer() {4,12}, New Integer() {5,13}, New Integer() {6,14}, New Integer() {7,15}
    }

    ' ----- ARROW EQUIPMENT DATABASE -----
    Private Class Equipment
        Public Property Name As String
        Public Property Type As String
        Public Property IconChar As String
        Public Property IsEquipped As Boolean = False
        Public Property VertexIndex As Integer   ' which tesseract vertex owns this item
        Public Sub New(name As String, type As String, icon As String, vertex As Integer)
            Me.Name = name
            Me.Type = type
            Me.IconChar = icon
            Me.VertexIndex = vertex
        End Sub
    End Class

    Private inventory As New List(Of Equipment) From {
        New Equipment("Bow (Queen's Gambit)", "Weapon", "🏹", 0),
        New Equipment("Standard Arrow", "Ammo", "➹", 1),
        New Equipment("Explosive Arrow", "Ammo", "💥", 2),
        New Equipment("Grapple Arrow", "Tool", "🪢", 3),
        New Equipment("Hood (Season 1)", "Armor", "🧥", 4),
        New Equipment("Sword (Sakai)", "Weapon", "⚔️", 5),
        New Equipment("Medkit", "Supply", "💊", 6),
        New Equipment("Comms (Felicity)", "Tool", "📡", 7),
        New Equipment("Boxing Glove Arrow", "Ammo", "🥊", 8),
        New Equipment("Flashbang Arrow", "Ammo", "💫", 9),
        New Equipment("Smoke Pellet", "Tool", "🌫️", 10),
        New Equipment("Lockpick", "Tool", "🔓", 11),
        New Equipment("Digital Watch (Diggle)", "Quest", "⌚", 12),
        New Equipment("Lian Yu Knife", "Weapon", "🔪", 13),
        New Equipment("The List", "Lore", "📜", 14),
        New Equipment("Photo of Shado", "Emotional", "🖼️", 15)
    }

    ' ----- 4D ROTATION STATE -----
    Private angleXY As Double = 0.0
    Private angleZW As Double = 0.3
    Private timer As Timer

    ' ----- INTERACTION STATE -----
    Private mousePos As Point
    Private hoveredSlotIndex As Integer = -1
    Private selectedSlotIndex As Integer = -1
    Private equippedItems As New List(Of Equipment)

    ' ----- CONSTRUCTOR -----
    Public Sub New()
        Me.Text = "ARROW: Oliver Queen's 4D Spacetime Inventory"
        Me.Size = New Size(1000, 700)
        Me.DoubleBuffered = True
        Me.BackColor = Color.Black
        Me.ForeColor = Color.Lime

        ' Setup animation timer
        timer = New Timer()
        timer.Interval = 30
        AddHandler timer.Tick, AddressOf Timer_Tick
        timer.Start()

        ' Mouse event handlers
        AddHandler Me.MouseMove, AddressOf Form_MouseMove
        AddHandler Me.MouseClick, AddressOf Form_MouseClick
        AddHandler Me.Paint, AddressOf OnPaint
    End Sub

    ' ----- 4D → 2D PROJECTION (perspective, double rotation) -----
    Private Function Project4DTo2D(v4 As Double(), angleXY As Double, angleZW As Double) As PointF
        ' Rotate in XY and ZW planes
        Dim rotated = Rotate4D(v4, angleXY, angleZW)

        ' Perspective projection: first from 4D to 3D (along w)
        Dim w = rotated(3)
        Dim factor3D = 3.0 / (w + 4.0)  ' safe divisor
        Dim x3 = rotated(0) * factor3D
        Dim y3 = rotated(1) * factor3D
        Dim z3 = rotated(2) * factor3D

        ' Perspective projection: 3D to 2D (along z)
        Dim factor2D = 2.0 / (z3 + 5.0)
        Dim x2 = x3 * factor2D
        Dim y2 = y3 * factor2D

        ' Scale and center on form
        Return New PointF(CSng(x2 * 180 + 400), CSng(y2 * 180 + 300))
    End Function

    ' 4D rotation (two independent planes)
    Private Function Rotate4D(vec As Double(), angleXY As Double, angleZW As Double) As Double()
        Dim result = DirectCast(vec.Clone(), Double())
        ' XY rotation
        Dim x = result(0)
        Dim y = result(1)
        result(0) = x * Math.Cos(angleXY) - y * Math.Sin(angleXY)
        result(1) = x * Math.Sin(angleXY) + y * Math.Cos(angleXY)
        ' ZW rotation
        Dim z = result(2)
        Dim w = result(3)
        result(2) = z * Math.Cos(angleZW) - w * Math.Sin(angleZW)
        result(3) = z * Math.Sin(angleZW) + w * Math.Cos(angleZW)
        Return result
    End Function

    ' ----- TIMER ANIMATION -----
    Private Sub Timer_Tick(sender As Object, e As EventArgs)
        angleXY += 0.02
        angleZW += 0.01
        Me.Invalidate()   ' force repaint
    End Sub

    ' ----- MOUSE INTERACTION -----
    Private Sub Form_MouseMove(sender As Object, e As MouseEventArgs)
        mousePos = e.Location
        ' Find nearest inventory slot (closest to a projected vertex)
        Dim minDist = 20.0
        hoveredSlotIndex = -1
        For i = 0 To vertices4D.Length - 1
            Dim proj = Project4DTo2D(vertices4D(i), angleXY, angleZW)
            Dim dist = Math.Sqrt((mousePos.X - proj.X) ^ 2 + (mousePos.Y - proj.Y) ^ 2)
            If dist < minDist Then
                minDist = dist
                hoveredSlotIndex = i
            End If
        Next
        Me.Invalidate()
    End Sub

    Private Sub Form_MouseClick(sender As Object, e As MouseEventArgs)
        If hoveredSlotIndex >= 0 Then
            ' Find the inventory item at this vertex
            Dim item = inventory.FirstOrDefault(Function(it) it.VertexIndex = hoveredSlotIndex)
            If item IsNot Nothing Then
                ' Toggle equip state
                If item.IsEquipped Then
                    item.IsEquipped = False
                    equippedItems.Remove(item)
                Else
                    item.IsEquipped = True
                    equippedItems.Add(item)
                End If
                selectedSlotIndex = hoveredSlotIndex
            End If
        End If
        Me.Invalidate()
    End Sub

    ' ----- MAIN DRAWING ROUTINE -----
    Private Sub OnPaint(sender As Object, e As PaintEventArgs)
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAlias
        g.Clear(Me.BackColor)

        ' ----- 1. Draw the 4D spacetime grid (tesseract) -----
        Dim penGrid = New Pen(Color.FromArgb(80, 0, 255, 0), 1) ' faint green
        For Each edge In edges
            Dim p1 = Project4DTo2D(vertices4D(edge(0)), angleXY, angleZW)
            Dim p2 = Project4DTo2D(vertices4D(edge(1)), angleXY, angleZW)
            g.DrawLine(penGrid, p1, p2)
        Next

        ' ----- 2. Draw Minkowski light cone (stylised) -----
        DrawLightCone(g)

        ' ----- 3. Draw inventory slots (at vertices) -----
        For i = 0 To vertices4D.Length - 1
            Dim proj = Project4DTo2D(vertices4D(i), angleXY, angleZW)
            Dim rect = New RectangleF(proj.X - 20, proj.Y - 20, 40, 40)

            ' Find the item at this vertex
            Dim item = inventory.FirstOrDefault(Function(it) it.VertexIndex = i)
            Dim slotColor = Color.DarkSlateGray
            Dim isHovered = (i = hoveredSlotIndex)
            Dim isEquipped = item?.IsEquipped ?? False

            ' Determine slot appearance
            If isEquipped Then
                slotColor = Color.LimeGreen
            ElseIf isHovered Then
                slotColor = Color.YellowGreen
            Else
                slotColor = Color.FromArgb(30, 50, 30)
            End If

            ' Draw slot background (circle)
            Using brush = New SolidBrush(Color.FromArgb(220, slotColor))
                g.FillEllipse(brush, rect)
            End Using
            g.DrawEllipse(Pens.LightGreen, rect)

            ' Draw item icon (emoji or text)
            If item IsNot Nothing Then
                Dim font = New Font("Segoe UI", 14, FontStyle.Bold)
                g.DrawString(item.IconChar, font, Brushes.White, rect.X + 8, rect.Y + 6)
            End If

            ' Draw small vertex index (4D coordinate hint)
            Dim idxFont = New Font("Consolas", 6)
            g.DrawString(i.ToString, idxFont, Brushes.Gray, rect.X + 2, rect.Y + 25)
        Next

        ' ----- 4. Draw UI overlay (title, loadout, 4D coordinates) -----
        DrawUI(g)

        ' ----- 5. Draw Minkowski metric signature -----
        DrawMetricSignature(g)
    End Sub

    ' Stylised light cone (hyperbolic curves)
    Private Sub DrawLightCone(g As Graphics)
        Dim penCone = New Pen(Color.FromArgb(40, 100, 255, 100), 1) {
            DashStyle = DashStyle.Dash
        }
        ' Draw two crossing lines representing future/past light cone
        g.DrawLine(penCone, 400, 0, 400, 600)
        g.DrawLine(penCone, 0, 300, 800, 300)
        ' Hyperbolic arcs (simplified)
        For i = -3 To 3
            Dim offset = i * 30
            g.DrawArc(penCone, 400 - 50 - offset, 300 - 50 - offset, 100 + offset * 2, 100 + offset * 2, 0, 360)
        Next
    End Sub

    Private Sub DrawUI(g As Graphics)
        ' Title
        Dim titleFont = New Font("Arial", 16, FontStyle.Bold)
        g.DrawString("OLIVER QUEEN • 4D SPACETIME INVENTORY", titleFont, Brushes.LimeGreen, 20, 20)

        ' Instructions
        Dim instFont = New Font("Segoe UI", 9)
        g.DrawString("Click a slot to equip/unequip. Tesseract rotates in 4D.", instFont, Brushes.LightGray, 20, 60)

        ' Current loadout
        Dim loadoutFont = New Font("Segoe UI", 10, FontStyle.Bold)
        g.DrawString("EQUIPPED:", loadoutFont, Brushes.Cyan, 20, 100)
        Dim yPos = 130
        For Each eq In equippedItems
            g.DrawString($"• {eq.Name} [{eq.Type}]", loadoutFont, Brushes.White, 30, yPos)
            yPos += 20
        Next

        ' 4D coordinates of hovered item
        If hoveredSlotIndex >= 0 Then
            Dim v = vertices4D(hoveredSlotIndex)
            Dim coordStr = String.Format("4D Coordinates: ({0:F2}, {1:F2}, {2:F2}, {3:F2})", v(0), v(1), v(2), v(3))
            g.DrawString(coordStr, New Font("Consolas", 10), Brushes.Yellow, 20, 600)
        Else
            g.DrawString("Hover over a slot to see its 4D vertex", New Font("Consolas", 10), Brushes.Gray, 20, 600)
        End If
    End Sub

    Private Sub DrawMetricSignature(g As Graphics)
        ' Minkowski signature ( + - - - ) – decorative
        Dim sigFont = New Font("Courier New", 9)
        g.DrawString("Minkowski Metric η = diag(−,+,+,+)", sigFont, Brushes.DarkCyan, 780, 650)
        g.DrawString("Spacetime Interval ds² = -dt² + dx² + dy² + dz²", sigFont, Brushes.DarkCyan, 780, 670)
    End Sub

    ' ----- ENTRY POINT -----
    Public Shared Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New Arrow4DInventoryForm())
    End Sub
End Class