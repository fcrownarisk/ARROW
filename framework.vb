' Arrow TV Sandbox Game Framework
' Single-file summary for VB.NET

Imports System.Collections.Generic
Imports System.Linq

' ====================== Interfaces ======================
Public Interface IAttackable
    Property Health As Integer
    Sub TakeDamage(amount As Integer)
End Interface

Public Interface IUseable
    Sub Use(user As Character)
End Interface

Public Interface IEquipable
    Sub Equip(owner As Character)
    Sub Unequip(owner As Character)
End Interface

' ====================== Base GameObject ======================
Public MustInherit Class GameObject
    Public Property Name As String
    Public Property IsActive As Boolean = True

    Public Sub New(name As String)
        Me.Name = name
    End Sub

    Public Overridable Sub Update()
        ' Base update logic – to be overridden
    End Sub
End Class

' ====================== Character Classes ======================
Public MustInherit Class Character
    Inherits GameObject
    Implements IAttackable

    Public Property Health As Integer Implements IAttackable.Health
    Public Property MaxHealth As Integer
    Public Property Level As Integer
    Public Property Defense As Integer = 0   ' Added for armor

    Public ReadOnly Property IsAlive As Boolean
        Get
            Return Health > 0
        End Get
    End Property

    Protected _inventory As New List(Of Item)
    Protected _equippedWeapon As Weapon = Nothing

    Public Sub New(name As String, maxHealth As Integer, level As Integer)
        MyBase.New(name)
        Me.MaxHealth = maxHealth
        Me.Health = maxHealth
        Me.Level = level
    End Sub

    Public Overridable Sub TakeDamage(amount As Integer) Implements IAttackable.TakeDamage
        Dim netDamage = Math.Max(1, amount - Defense)   ' Defense reduces damage
        Health -= netDamage
        If Health < 0 Then Health = 0
        Console.WriteLine($"{Name} took {netDamage} damage. Health now {Health}.")
    End Sub

    Public Overridable Sub Attack(target As IAttackable)
        If _equippedWeapon IsNot Nothing Then
            ' Weapon.Use now requires a Character user and IAttackable target
            ' We'll need to cast – but in our design, Weapon.Use expects (Character, IAttackable)
            ' We'll handle it inside Weapon class.
            _equippedWeapon.Use(Me, target)
        Else
            Console.WriteLine($"{Name} has no weapon equipped!")
        End If
    End Sub

    Public Sub PickUpItem(item As Item)
        _inventory.Add(item)
        Console.WriteLine($"{Name} picked up {item.Name}.")
    End Sub

    Public Sub EquipWeapon(weapon As Weapon)
        _equippedWeapon = weapon
        weapon.Equip(Me)
    End Sub
End Class

Public Class Player
    Inherits Character

    Public Property Experience As Integer
    Public Property Reputation As Integer

    Public Sub New(name As String)
        MyBase.New(name, 100, 1)
    End Sub

    Public Overrides Sub Update()
        MyBase.Update()
        ' Player‑specific logic would go here
    End Sub

    Public Sub GainExperience(points As Integer)
        Experience += points
        While Experience >= Level * 100
            LevelUp()
        End While
    End Sub

    Private Sub LevelUp()
        Level += 1
        MaxHealth += 20
        Health = MaxHealth
        Console.WriteLine($"{Name} reached level {Level}!")
    End Sub
End Class

Public Class NPC
    Inherits Character

    Public Property Dialogue As String

    Public Sub New(name As String, dialogue As String)
        MyBase.New(name, 50, 1)
        Me.Dialogue = dialogue
    End Sub

    Public Sub Speak()
        Console.WriteLine($"{Name} says: ""{Dialogue}""")
    End Sub
End Class

Public Class Enemy
    Inherits Character

    Public Property AggroRange As Double
    Public Property Damage As Integer

    Public Sub New(name As String, maxHealth As Integer, damage As Integer, aggroRange As Double)
        MyBase.New(name, maxHealth, 1)
        Me.Damage = damage
        Me.AggroRange = aggroRange
    End Sub

    Public Overrides Sub Update()
        MyBase.Update()
        ' Simple AI stub
    End Sub
End Class

' ====================== Item Classes ======================
Public MustInherit Class Item
    Inherits GameObject

    Public Property Value As Integer
    Public Property Weight As Double

    Public Sub New(name As String, value As Integer, weight As Double)
        MyBase.New(name)
        Me.Value = value
        Me.Weight = weight
    End Sub
End Class

Public MustInherit Class Weapon
    Inherits Item
    Implements IEquipable

    Public Property Damage As Integer
    Public Property Range As Double
    Public Property Durability As Integer

    Protected _owner As Character = Nothing

    Public Sub New(name As String, damage As Integer, range As Double, durability As Integer, value As Integer, weight As Double)
        MyBase.New(name, value, weight)
        Me.Damage = damage
        Me.Range = range
        Me.Durability = durability
    End Sub

    ' Overridable method for attacking a target
    Public Overridable Sub Use(user As Character, target As IAttackable)
        If _owner Is Nothing Then
            Console.WriteLine($"{Name} is not equipped!")
            Return
        End If
        If Durability <= 0 Then
            Console.WriteLine($"{Name} is broken!")
            Return
        End If
        target.TakeDamage(Damage)
        Durability -= 1
        Console.WriteLine($"{user.Name} attacked {DirectCast(target, GameObject).Name} with {Name} for {Damage} damage.")
    End Sub

    Public Sub Equip(owner As Character) Implements IEquipable.Equip
        _owner = owner
        Console.WriteLine($"{owner.Name} equipped {Name}.")
    End Sub

    Public Sub Unequip(owner As Character) Implements IEquipable.Unequip
        If _owner Is owner Then
            _owner = Nothing
            Console.WriteLine($"{owner.Name} unequipped {Name}.")
        End If
    End Sub
End Class

Public Class Bow
    Inherits Weapon

    Public Property ArrowType As Arrow

    Public Sub New(name As String, damage As Integer, range As Double, durability As Integer, value As Integer, weight As Double)
        MyBase.New(name, damage, range, durability, value, weight)
    End Sub

    Public Overrides Sub Use(user As Character, target As IAttackable)
        Dim totalDamage = Damage
        If ArrowType IsNot Nothing Then
            totalDamage += ArrowType.AdditionalDamage
            Console.WriteLine($"Firing {ArrowType.Name}!")
        End If
        ' Apply damage
        If Durability <= 0 Then
            Console.WriteLine($"{Name} is broken!")
            Return
        End If
        target.TakeDamage(totalDamage)
        Durability -= 1
        Console.WriteLine($"{user.Name} shot {DirectCast(target, GameObject).Name} with {Name} for {totalDamage} damage.")
    End Sub

    Public Sub LoadArrow(arrow As Arrow)
        ArrowType = arrow
        Console.WriteLine($"{_owner.Name} loaded {arrow.Name} into {Name}.")
    End Sub
End Class

Public Class Arrow
    Inherits Item

    Public Property AdditionalDamage As Integer
    Public Property Effect As String

    Public Sub New(name As String, additionalDamage As Integer, effect As String, value As Integer, weight As Double)
        MyBase.New(name, value, weight)
        Me.AdditionalDamage = additionalDamage
        Me.Effect = effect
    End Sub
End Class

Public Class Armor
    Inherits Item
    Implements IEquipable

    Public Property Defense As Integer
    Private _owner As Character = Nothing

    Public Sub New(name As String, defense As Integer, value As Integer, weight As Double)
        MyBase.New(name, value, weight)
        Me.Defense = defense
    End Sub

    Public Sub Equip(owner As Character) Implements IEquipable.Equip
        _owner = owner
        owner.Defense += Defense
        Console.WriteLine($"{owner.Name} equipped {Name} (+{Defense} defense).")
    End Sub

    Public Sub Unequip(owner As Character) Implements IEquipable.Unequip
        If _owner Is owner Then
            owner.Defense -= Defense
            _owner = Nothing
            Console.WriteLine($"{owner.Name} unequipped {Name}.")
        End If
    End Sub
End Class

' ====================== Ability Class ======================
Public Class Ability
    Public Property Name As String
    Public Property Cooldown As Integer
    Public Property CurrentCooldown As Integer
    Public Property Effect As Action(Of Character, Character)

    Public Sub New(name As String, cooldown As Integer, effect As Action(Of Character, Character))
        Me.Name = name
        Me.Cooldown = cooldown
        Me.Effect = effect
        Me.CurrentCooldown = 0
    End Sub

    Public Function CanUse() As Boolean
        Return CurrentCooldown <= 0
    End Function

    Public Sub Use(user As Character, target As Character)
        If CanUse() Then
            Effect(user, target)
            CurrentCooldown = Cooldown
        Else
            Console.WriteLine($"{Name} is on cooldown.")
        End If
    End Sub

    Public Sub UpdateCooldown()
        If CurrentCooldown > 0 Then CurrentCooldown -= 1
    End Sub
End Class

' ====================== World and Location ======================
Public Class Location
    Inherits GameObject

    Public Property Description As String
    Public Property ConnectedLocations As List(Of Location)
    Public Property Enemies As List(Of Enemy)
    Public Property Items As List(Of Item)

    Public Sub New(name As String, description As String)
        MyBase.New(name)
        Me.Description = description
        ConnectedLocations = New List(Of Location)
        Enemies = New List(Of Enemy)
        Items = New List(Of Item)
    End Sub

    Public Overrides Sub Update()
        For Each enemy In Enemies
            enemy.Update()
        Next
    End Sub
End Class

' ====================== Quest System ======================
Public Class Quest
    Public Property Name As String
    Public Property Description As String
    Public Property IsCompleted As Boolean = False
    Public Property Objectives As List(Of QuestObjective)
    Public Property RewardExperience As Integer
    Public Property RewardItem As Item

    Public Sub CheckCompletion()
        If Objectives.All(Function(o) o.IsCompleted) Then
            IsCompleted = True
            Console.WriteLine($"Quest '{Name}' completed!")
        End If
    End Sub
End Class

Public Class QuestObjective
    Public Property Description As String
    Public Property Target As GameObject
    Public Property RequiredCount As Integer
    Public Property CurrentCount As Integer
    Public ReadOnly Property IsCompleted As Boolean
        Get
            Return CurrentCount >= RequiredCount
        End Get
    End Property

    Public Sub Progress(amount As Integer)
        CurrentCount += amount
        If IsCompleted Then
            Console.WriteLine($"Objective completed: {Description}")
        End If
    End Sub
End Class

' ====================== Game Manager ======================
Public Class Game
    Private _player As Player
    Private _currentLocation As Location
    Private _worldMap As Dictionary(Of String, Location)
    Private _quests As List(Of Quest)
    Private _isRunning As Boolean = True

    Public Sub New(playerName As String)
        _player = New Player(playerName)
        _worldMap = New Dictionary(Of String, Location)
        _quests = New List(Of Quest)
        InitializeWorld()
    End Sub

    Private Sub InitializeWorld()
        ' Create a couple of locations
        Dim starCity = New Location("Star City", "The glimmering city with dark alleys.")
        Dim glades = New Location("The Glades", "Rough neighborhood.")
        starCity.ConnectedLocations.Add(glades)
        glades.ConnectedLocations.Add(starCity)

        ' Add some items and enemies for demo
        Dim practiceArrow As New Arrow("Practice Arrow", 0, "Blunt", 5, 0.1)
        starCity.Items.Add(practiceArrow)

        Dim thug As New Enemy("Street Thug", 30, 5, 5.0)
        glades.Enemies.Add(thug)

        _worldMap.Add("StarCity", starCity)
        _worldMap.Add("Glades", glades)
        _currentLocation = starCity
    End Sub

    Public Sub Run()
        Console.WriteLine($"Welcome to Star City, {_player.Name}!")
        While _isRunning
            ' Simple command loop (just for demo, normally you'd have input handling)
            Console.WriteLine("Press Q to quit, any other key to simulate a game tick.")
            Dim key = Console.ReadKey(True)
            If key.KeyChar = "q" OrElse key.KeyChar = "Q" Then
                StopGame()
                Continue While
            End If

            ' Update game objects
            _currentLocation.Update()
            _player.Update()

            ' Demo action: player attacks first enemy if any
            If _currentLocation.Enemies.Count > 0 Then
                Dim enemy = _currentLocation.Enemies(0)
                If enemy.IsAlive Then
                    _player.Attack(enemy)
                Else
                    Console.WriteLine($"{enemy.Name} is already defeated.")
                End If
            Else
                Console.WriteLine("No enemies here.")
            End If
        End While
    End Sub

    Public Sub StopGame()
        _isRunning = False
    End Sub

    ' Expose player for demo (in real game, use methods)
    Public ReadOnly Property Player As Player
        Get
            Return _player
        End Get
    End Property

    Public ReadOnly Property CurrentLocation As Location
        Get
            Return _currentLocation
        End Get
    End Property
End Class

' ====================== Module Entry Point ======================
Module Program
    Sub Main()
        ' Create the game
        Dim game As New Game("Oliver Queen")

        ' Create items (normally these would be placed in world, but we'll manually equip)
        Dim bow As New Bow("Recurve Bow", 25, 20.0, 100, 500, 2.5)
        Dim arrow As New Arrow("Explosive Arrow", 15, "Explosion", 50, 0.2)
        Dim armor As New Armor("Green Hood", 10, 300, 3.0)

        ' Player picks up and equips
        game.Player.PickUpItem(bow)
        game.Player.PickUpItem(arrow)
        game.Player.PickUpItem(armor)
        game.Player.EquipWeapon(bow)
        bow.LoadArrow(arrow)
        armor.Equip(game.Player)

        ' Add an enemy to current location for testing
        Dim deathstroke As New Enemy("Deathstroke", 150, 20, 10.0)
        game.CurrentLocation.Enemies.Add(deathstroke)

        ' Run the game loop (simple demo)
        game.Run()

        Console.WriteLine("Game ended. Press any key to exit.")
        Console.ReadKey()
    End Sub
End Module