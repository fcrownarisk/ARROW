' ARROW TV Game - Complete Simulation with User Interface
' Single file VB.NET console application

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading

Module ArrowGame
    ' ====================== GLOBAL VARIABLES ======================
    Private player As Player
    Private currentLocation As Location
    Private gameTime As Integer = 0
    Private gameRunning As Boolean = True
    Private random As New Random()
    Private locations As New Dictionary(Of String, Location)
    Private factions As New Dictionary(Of String, Faction)
    Private activeMissions As New List(Of Mission)
    Private completedMissions As New List(Of Mission)
    Private gameMessages As New List(Of String)
    Private声誉 As Integer = 0 ' Reputation
    Private dayCycle As Integer = 0 ' 0=Day, 1=Night
    
    ' ====================== MAIN ENTRY POINT ======================
    Sub Main()
        Console.Title = "ARROW: Star City Vigilante - v3.0"
        Console.SetWindowSize(120, 40)
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.Green
        Console.Clear()
        
        ShowTitleScreen()
        InitializeGame()
        MainGameLoop()
    End Sub
    
    ' ====================== TITLE SCREEN ======================
    Sub ShowTitleScreen()
        Console.Clear()
        Dim title As String = "
        ╔══════════════════════════════════════════════════════════════╗
        ║                                                              ║
        ║     █████╗ ██████╗ ██████╗  ██████╗ ██╗    ██╗               ║
        ║    ██╔══██╗██╔══██╗██╔══██╗██╔═══██╗██║    ██║               ║
        ║    ███████║██████╔╝██████╔╝██║   ██║██║ █╗ ██║               ║
        ║    ██╔══██║██╔══██╗██╔══██╗██║   ██║██║███╗██║               ║
        ║    ██║  ██║██║  ██║██║  ██║╚██████╔╝╚███╔███╔╝               ║
        ║    ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═╝ ╚═════╝  ╚══╝╚══╝                ║
        ║                                                              ║
        ║                    ████████╗██╗   ██╗                        ║
        ║                    ╚══██╔══╝██║   ██║                        ║
        ║                       ██║   ██║   ██║                        ║
        ║                       ██║   ╚██╗ ██╔╝                        ║
        ║                       ██║    ╚████╔╝                         ║
        ║                       ╚═╝     ╚═══╝                          ║
        ║                                                              ║
        ║              STAR CITY VIGILANTE - SANDBOX                   ║
        ║                    3A EDITION                                ║
        ║                                                              ║
        ╚══════════════════════════════════════════════════════════════╝"
        
        Console.WriteLine(title)
        Console.WriteLine()
        Console.ForegroundColor = ConsoleColor.Yellow
        Console.WriteLine("                    PRESS ANY KEY TO BEGIN")
        Console.ForegroundColor = ConsoleColor.DarkGray
        Console.WriteLine("                    Developed by: Queen Consolidated")
        Console.ReadKey(True)
    End Sub
    
    ' ====================== GAME INITIALIZATION ======================
    Sub InitializeGame()
        Console.Clear()
        Console.ForegroundColor = ConsoleColor.Cyan
        Console.WriteLine("Initializing Star City...")
        Thread.Sleep(500)
        
        ' Create factions
        InitializeFactions()
        
        ' Create locations
        InitializeLocations()
        
        ' Create player
        Console.Write("Enter your vigilante name (or press Enter for 'Oliver Queen'): ")
        Dim playerName As String = Console.ReadLine()
        If String.IsNullOrWhiteSpace(playerName) Then playerName = "Oliver Queen"
        
        player = New Player(playerName)
        player.Location = locations("Founder's Island")
        currentLocation = player.Location
        
        ' Give starter equipment
        SetupStarterEquipment()
        
        ' Generate initial missions
        GenerateMissions()
        
        Console.ForegroundColor = ConsoleColor.Green
        Console.WriteLine($"Welcome to Star City, {player.Name}...")
        Console.WriteLine("The city needs you.")
        Thread.Sleep(1500)
    End Sub
    
    Sub InitializeFactions()
        factions.Add("SCPD", New Faction("Star City Police Department", 0, "Law Enforcement"))
        factions.Add("Glades", New Faction("The Glades Community", 20, "Community"))
        factions.Add("QC", New Faction("Queen Consolidated", 50, "Corporate"))
        factions.Add("Underground", New Faction("Criminal Underground", -30, "Criminal"))
        factions.Add("League", New Faction("League of Assassins", 0, "Mystic"))
    End Sub
    
    Sub InitializeLocations()
        ' Star City Districts
        locations.Add("Founder's Island", New Location("Founder's Island", "Your base of operations. The old Queen mansion.", LocationType.Safe))
        locations.Add("The Glades", New Location("The Glades", "Rough neighborhood, high crime rate.", LocationType.Dangerous))
        locations.Add("Downtown", New Location("Downtown", "Financial district, Queen Consolidated tower.", LocationType.Normal))
        locations.Add("Iron Heights", New Location("Iron Heights", "Maximum security prison.", LocationType.HighSecurity))
        locations.Add("Industrial District", New Location("Industrial District", "Abandoned factories and warehouses.", LocationType.Dangerous))
        locations.Add("Starling Heights", New Location("Starling Heights", "Upscale residential area.", LocationType.Normal))
        
        ' Add connections
        locations("Founder's Island").AddConnection(locations("Downtown"))
        locations("Downtown").AddConnection(locations("The Glades"))
        locations("Downtown").AddConnection(locations("Starling Heights"))
        locations("The Glades").AddConnection(locations("Industrial District"))
        locations("Industrial District").AddConnection(locations("Iron Heights"))
        locations("Starling Heights").AddConnection(locations("Downtown"))
        
        ' Populate locations with NPCs and enemies
        PopulateLocations()
    End Sub
    
    Sub PopulateLocations()
        ' The Glades - Criminal activity
        locations("The Glades").AddEnemy(New Enemy("Street Thug", 30, 5, 8, "Human", 10))
        locations("The Glades").AddEnemy(New Enemy("Gang Member", 45, 8, 10, "Human", 25))
        locations("The Glades").AddEnemy(New Enemy("Drug Dealer", 35, 6, 7, "Human", 50))
        
        ' Industrial District - Organized crime
        locations("Industrial District").AddEnemy(New Enemy("Armed Guard", 60, 12, 15, "Human", 75))
        locations("Industrial District").AddEnemy(New Enemy("Hired Muscle", 80, 15, 12, "Human", 100))
        locations("Industrial District").AddEnemy(New Enemy("Boss", 150, 20, 20, "Human", 500))
        
        ' Add NPCs
        locations("Downtown").AddNPC(New NPC("Felicity Smoak", "Tech Expert", "You really should update your firewall, Oliver."))
        locations("Founder's Island").AddNPC(New NPC("John Diggle", "Bodyguard", "Stay focused, we have company."))
        locations("Starling Heights").AddNPC(New NPC("Thea Queen", "Sister", "I can take care of myself, Ollie."))
        locations("Iron Heights").AddNPC(New NPC("Guard", "Prison Guard", "No visitors without clearance."))
    End Sub
    
    Sub SetupStarterEquipment()
        ' Basic gear
        Dim starterBow As New Bow("Starter Recurve", 15, 15.0, 50, 100, 2.0)
        Dim arrows(5) As Arrow
        For i As Integer = 0 To 4
            arrows(i) = New Arrow($"Standard Arrow {i + 1}", 0, "Standard", 2, 0.1)
            player.Inventory.Add(arrows(i))
        Next
        
        Dim boxingGloves As New Weapon("Boxing Gloves", 8, 1.5, 100, 20, 1.0)
        
        player.Inventory.Add(starterBow)
        player.Inventory.Add(boxingGloves)
        player.EquippedWeapon = starterBow
        starterBow.LoadArrow(arrows(0))
        
        ' Starter armor
        Dim hood As New Armor("Green Hood", 5, 50, 1.0)
        player.Inventory.Add(hood)
        hood.Equip(player)
    End Sub
    
    ' ====================== MAIN GAME LOOP ======================
    Sub MainGameLoop()
        While gameRunning
            UpdateGameTime()
            ShowMainHUD()
            ProcessMainMenu()
        End While
        
        ShowGameOver()
    End Sub
    
    Sub UpdateGameTime()
        gameTime += 1
        If gameTime Mod 10 = 0 Then
            dayCycle = If(dayCycle = 0, 1, 0) ' Toggle day/night
        End If
        
        ' Update NPCs and enemies
        For Each loc In locations.Values
            loc.Update()
        Next
        
        ' Update missions
        For Each mission In activeMissions.ToList()
            mission.Update()
            If mission.IsCompleted Then
                CompleteMission(mission)
            End If
        Next
    End Sub
    
    ' ====================== USER INTERFACE ======================
    Sub ShowMainHUD()
        Console.Clear()
        
        ' Header with game info
        Console.BackgroundColor = ConsoleColor.DarkBlue
        Console.ForegroundColor = ConsoleColor.White
        Console.WriteLine($"╔════════════════════════════════════════════════════════════════════════════════════════════╗")
        Console.WriteLine($"║  ARROW: Star City Vigilante                    Time: {GetTimeString(),-15} Day: {GetDayCycle(),-10} ║")
        Console.WriteLine($"╚════════════════════════════════════════════════════════════════════════════════════════════╝")
        Console.ResetColor()
        
        ' Player stats panel
        Console.ForegroundColor = ConsoleColor.Cyan
        Console.WriteLine($"┌───────────────────── PLAYER STATUS ────────────────────┬──────────────── LOCATION ────────────────┐")
        Console.WriteLine($"│ Name: {player.Name,-30} Health: {player.Health,3}/{player.MaxHealth,-3} │ {currentLocation.Name,-35} │")
        Console.WriteLine($"│ Level: {player.Level,2}    XP: {player.Experience,4}/{player.Level * 100,-4}    Rep: {声誉,5} │ Type: {currentLocation.Type.ToString(),-20}      │")
        Console.WriteLine($"│ Weapon: {If(player.EquippedWeapon IsNot Nothing, player.EquippedWeapon.Name, "None"),-20}         │ Enemies: {currentLocation.Enemies.Count,2}                    │")
        Console.WriteLine($"│ Armor: {If(player.EquippedArmor IsNot Nothing, player.EquippedArmor.Name, "None"),-20}           │ Allies: {currentLocation.NPCs.Count,2}                      │")
        Console.WriteLine($"└─────────────────────────────────────────────────────────┴──────────────────────────────────────────┘")
        Console.ResetColor()
        
        ' Messages panel
        Console.ForegroundColor = ConsoleColor.Yellow
        Console.WriteLine($"┌───────────────────── RECENT EVENTS ─────────────────────┐")
        For i As Integer = Math.Max(0, gameMessages.Count - 3) To gameMessages.Count - 1
            If i >= 0 Then
                Console.WriteLine($"│ {gameMessages(i),-50} │")
            End If
        Next
        Console.WriteLine($"└──────────────────────────────────────────────────────────┘")
        Console.ResetColor()
        
        ' Active missions
        Console.ForegroundColor = ConsoleColor.Green
        Console.WriteLine($"┌───────────────────── ACTIVE MISSIONS ───────────────────┐")
        If activeMissions.Count = 0 Then
            Console.WriteLine($"│ No active missions. Visit NPCs for work.                │")
        Else
            For i As Integer = 0 To Math.Min(2, activeMissions.Count - 1)
                Dim mission = activeMissions(i)
                Dim status As String = If(mission.IsCompleted, "[DONE]", $"[{mission.Progress}%]")
                Console.WriteLine($"│ {i + 1}. {mission.Name,-35} {status,10} │")
            Next
        End If
        Console.WriteLine($"└──────────────────────────────────────────────────────────┘")
        Console.ResetColor()
        
        ' Main menu
        Console.WriteLine()
        Console.ForegroundColor = ConsoleColor.White
        Console.BackgroundColor = ConsoleColor.DarkGray
        Console.WriteLine($"  MAIN MENU  ".PadLeft(60))
        Console.ResetColor()
        Console.WriteLine()
        Console.WriteLine("  [1] Patrol Area     [2] Inventory     [3] Missions     [4] Factions")
        Console.WriteLine("  [5] Train           [6] Rest/Heal     [7] Travel       [8] Quick Combat")
        Console.WriteLine("  [9] Save Game       [0] Load Game     [Q] Quit Game")
        Console.WriteLine()
        Console.Write("Select option: ")
    End Sub
    
    Sub ProcessMainMenu()
        Dim key = Console.ReadKey(True)
        
        Select Case key.KeyChar
            Case "1"c : PatrolArea()
            Case "2"c : ShowInventory()
            Case "3"c : ShowMissions()
            Case "4"c : ShowFactions()
            Case "5"c : Train()
            Case "6"c : Rest()
            Case "7"c : Travel()
            Case "8"c : QuickCombat()
            Case "9"c : SaveGame()
            Case "0"c : LoadGame()
            Case "q"c, "Q"c : gameRunning = False
        End Select
    End Sub
    
    ' ====================== GAME MECHANICS ======================
    
    ' Combat System
    Sub PatrolArea()
        Console.Clear()
        Console.ForegroundColor = ConsoleColor.Red
        Console.WriteLine("══════════════════════ PATROLLING AREA ══════════════════════")
        Console.ResetColor()
        
        If currentLocation.Enemies.Count = 0 Then
            AddMessage("The area seems quiet... for now.")
            Console.WriteLine("No enemies found in this area.")
            Console.WriteLine("Press any key to continue...")
            Console.ReadKey()
            Return
        End If
        
        Console.WriteLine($"You encounter {currentLocation.Enemies.Count} enemies!")
        Console.WriteLine()
        
        Dim enemyList = currentLocation.Enemies.ToList()
        For i As Integer = 0 To enemyList.Count - 1
            Console.WriteLine($"{i + 1}. {enemyList(i).Name} (HP: {enemyList(i).Health})")
        Next
        
        Console.WriteLine()
        Console.WriteLine("Select enemy to engage (1-" & enemyList.Count & ") or 0 to retreat: ")
        
        Dim choice As Integer
        If Integer.TryParse(Console.ReadLine(), choice) AndAlso choice > 0 AndAlso choice <= enemyList.Count Then
            Dim target = enemyList(choice - 1)
            StartCombat(target)
        Else
            AddMessage("You retreat from combat.")
        End If
    End Sub
    
    Sub StartCombat(enemy As Enemy)
        Console.Clear()
        Console.ForegroundColor = ConsoleColor.Red
        Console.WriteLine("══════════════════════ COMBAT ENGAGED ══════════════════════")
        Console.ResetColor()
        Console.WriteLine()
        
        Dim combatLog As New List(Of String)
        Dim playerTurn As Boolean = True
        Dim combatRound As Integer = 1
        
        While player.Health > 0 AndAlso enemy.Health > 0
            Console.Clear()
            Console.ForegroundColor = ConsoleColor.Red
            Console.WriteLine($"╔══════════════════════ ROUND {combatRound} ══════════════════════╗")
            Console.ResetColor()
            
            ' Display health
            Console.WriteLine($"You: ❤️ {player.Health}/{player.MaxHealth}   {enemy.Name}: ❤️ {enemy.Health}/{enemy.MaxHealth}")
            Console.WriteLine()
            
            ' Show combat log
            For Each log In combatLog.TakeLast(5)
                Console.WriteLine($"  {log}")
            Next
            Console.WriteLine()
            
            If playerTurn Then
                ' Player turn
                Console.WriteLine("Choose action:")
                Console.WriteLine("  [1] Quick Shot (Light Damage)")
                Console.WriteLine("  [2] Heavy Shot (High Damage, -10 Accuracy)")
                Console.WriteLine("  [3] Trick Arrow (Special Effect)")
                Console.WriteLine("  [4] Use Item")
                Console.WriteLine("  [5] Attempt Escape")
                Console.Write("Action: ")
                
                Dim action = Console.ReadKey(True)
                Select Case action.KeyChar
                    Case "1"c
                        Dim damage = player.CalculateDamage() + random.Next(5, 15)
                        enemy.TakeDamage(damage)
                        combatLog.Add($"You hit for {damage} damage!")
                        
                    Case "2"c
                        If random.Next(100) < 70 ' 70% accuracy
                            Dim damage = player.CalculateDamage() + random.Next(15, 25)
                            enemy.TakeDamage(damage)
                            combatLog.Add($"Heavy shot hits for {damage} damage!")
                        Else
                            combatLog.Add("Heavy shot misses!")
                        End If
                        
                    Case "3"c
                        If player.Inventory.OfType(Of Arrow).Any(Function(a) a.Effect <> "Standard") Then
                            UseTrickArrow(enemy, combatLog)
                        Else
                            combatLog.Add("No trick arrows available!")
                        End If
                        
                    Case "4"c
                        UseCombatItem(combatLog)
                        
                    Case "5"c
                        If random.Next(100) < 40 ' 40% escape chance
                            combatLog.Add("You escaped successfully!")
                            Thread.Sleep(1000)
                            Return
                        Else
                            combatLog.Add("Escape failed!")
                        End If
                End Select
            Else
                ' Enemy turn
                Console.WriteLine($"{enemy.Name}'s turn...")
                Thread.Sleep(500)
                
                Dim enemyDamage = enemy.CalculateDamage()
                player.TakeDamage(enemyDamage)
                combatLog.Add($"{enemy.Name} hits you for {enemyDamage} damage!")
            End If
            
            playerTurn = Not playerTurn
            If playerTurn Then combatRound += 1
            Thread.Sleep(500)
        End While
        
        ' Combat resolution
        If player.Health <= 0 Then
            Console.Clear()
            Console.ForegroundColor = ConsoleColor.Red
            Console.WriteLine("══════════════════════ YOU DIED ══════════════════════")
            Console.ResetColor()
            Console.WriteLine("Game Over...")
            gameRunning = False
        Else
            Console.Clear()
            Console.ForegroundColor = ConsoleColor.Green
            Console.WriteLine("══════════════════════ VICTORY! ══════════════════════")
            Console.ResetColor()
            
            ' Rewards
            Dim expGain = enemy.ExperienceValue
            Dim moneyGain = random.Next(10, 50)
            
            player.Experience += expGain
           声誉 += enemy.ReputationValue
            
            AddMessage($"Defeated {enemy.Name}! +{expGain} XP, +{moneyGain} credits")
            
            ' Remove enemy
            currentLocation.Enemies.Remove(enemy)
            
            ' Check for level up
            CheckLevelUp()
        End If
        
        Console.WriteLine("Press any key to continue...")
        Console.ReadKey()
    End Sub
    
    Sub UseTrickArrow(enemy As Enemy, ByRef combatLog As List(Of String))
        Dim trickArrows = player.Inventory.OfType(Of Arrow).Where(Function(a) a.Effect <> "Standard").ToList()
        
        If trickArrows.Count = 0 Then
            combatLog.Add("No trick arrows!")
            Return
        End If
        
        Console.WriteLine("Select arrow:")
        For i As Integer = 0 To trickArrows.Count - 1
            Console.WriteLine($"{i + 1}. {trickArrows(i).Name} ({trickArrows(i).Effect})")
        Next
        
        Dim choice As Integer
        If Integer.TryParse(Console.ReadLine(), choice) AndAlso choice > 0 AndAlso choice <= trickArrows.Count Then
            Dim selectedArrow = trickArrows(choice - 1)
            
            Select Case selectedArrow.Effect.ToLower()
                Case "explosive"
                    Dim damage = 30
                    enemy.TakeDamage(damage)
                    combatLog.Add($"EXPLOSION! {damage} damage!")
                    
                Case "cryo"
                    enemy.Speed = Math.Max(1, enemy.Speed - 5)
                    combatLog.Add($"Enemy slowed!")
                    
                Case "shock"
                    Dim damage = 20
                    enemy.TakeDamage(damage)
                    combatLog.Add($"Electric shock for {damage} damage!")
                    
                Case "smoke"
                    enemy.Accuracy = Math.Max(10, enemy.Accuracy - 20)
                    combatLog.Add($"Smoke blinds enemy!")
            End Select
            
            player.Inventory.Remove(selectedArrow)
        End If
    End Sub
    
    Sub UseCombatItem(ByRef combatLog As List(Of String))
        Dim items = player.Inventory.Where(Function(i) TypeOf i Is Consumable).ToList()
        
        If items.Count = 0 Then
            combatLog.Add("No usable items!")
            Return
        End If
        
        Console.WriteLine("Select item:")
        For i As Integer = 0 To items.Count - 1
            Console.WriteLine($"{i + 1}. {items(i).Name}")
        Next
        
        Dim choice As Integer
        If Integer.TryParse(Console.ReadLine(), choice) AndAlso choice > 0 AndAlso choice <= items.Count Then
            Dim item = DirectCast(items(choice - 1), Consumable)
            item.Use(player)
            player.Inventory.Remove(item)
            combatLog.Add($"Used {item.Name}!")
        End If
    End Sub
    
    ' Inventory System
    Sub ShowInventory()
        While True
            Console.Clear()
            Console.ForegroundColor = ConsoleColor.Yellow
            Console.WriteLine("══════════════════════ INVENTORY ══════════════════════")
            Console.ResetColor()
            Console.WriteLine()
            
            Console.WriteLine($"Credits: {声誉} reputation")
            Console.WriteLine($"Weight: {CalculateInventoryWeight():F1}/50.0 kg")
            Console.WriteLine()
            
            ' Group items by type
            Dim weapons = player.Inventory.OfType(Of Weapon).ToList()
            Dim arrows = player.Inventory.OfType(Of Arrow).ToList()
            Dim armor = player.Inventory.OfType(Of Armor).ToList()
            Dim consumables = player.Inventory.OfType(Of Consumable).ToList()
            
            If weapons.Count > 0 Then
                Console.WriteLine("WEAPONS:")
                For Each w In weapons
                    Dim equipped As String = If(player.EquippedWeapon Is w, " [EQUIPPED]", "")
                    Console.WriteLine($"  {w.Name} - DMG: {w.Damage}, DUR: {w.Durability}{equipped}")
                Next
            End If
            
            If arrows.Count > 0 Then
                Console.WriteLine(vbLf & "ARROWS:")
                For Each a In arrows
                    Console.WriteLine($"  {a.Name} - {a.Effect} (+{a.AdditionalDamage} dmg)")
                Next
            End If
            
            If armor.Count > 0 Then
                Console.WriteLine(vbLf & "ARMOR:")
                For Each a In armor
                    Dim equipped As String = If(player.EquippedArmor Is a, " [EQUIPPED]", "")
                    Console.WriteLine($"  {a.Name} - DEF: {a.Defense}{equipped}")
                Next
            End If
            
            If consumables.Count > 0 Then
                Console.WriteLine(vbLf & "ITEMS:")
                For Each c In consumables
                    Console.WriteLine($"  {c.Name} - {c.Description}")
                Next
            End If
            
            Console.WriteLine()
            Console.WriteLine("[1] Equip Weapon    [2] Equip Armor    [3] Drop Item")
            Console.WriteLine("[4] Use Consumable  [5] Craft Arrows   [0] Back")
            Console.Write("Select: ")
            
            Dim key = Console.ReadKey(True)
            Select Case key.KeyChar
                Case "1"c : EquipWeaponMenu()
                Case "2"c : EquipArmorMenu()
                Case "3"c : DropItemMenu()
                Case "4"c : UseConsumableMenu()
                Case "5"c : CraftArrows()
                Case "0"c : Exit Select
            End Select
            
            If key.KeyChar = "0"c Then Exit While
        End While
    End Sub
    
    Sub EquipWeaponMenu()
        Dim weapons = player.Inventory.OfType(Of Weapon).ToList()
        If weapons.Count = 0 Then
            Console.WriteLine("No weapons to equip!")
            Thread.Sleep(1000)
            Return
        End If
        
        Console.WriteLine(vbLf & "Select weapon to equip:")
        For i As Integer = 0 To weapons.Count - 1
            Console.WriteLine($"{i + 1}. {weapons(i).Name}")
        Next
        
        Dim choice As Integer
        If Integer.TryParse(Console.ReadLine(), choice) AndAlso choice > 0 AndAlso choice <= weapons.Count Then
            player.EquippedWeapon = weapons(choice - 1)
            AddMessage($"Equipped {weapons(choice - 1).Name}")
        End If
    End Sub
    
    Sub EquipArmorMenu()
        Dim armors = player.Inventory.OfType(Of Armor).ToList()
        If armors.Count = 0 Then
            Console.WriteLine("No armor to equip!")
            Thread.Sleep(1000)
            Return
        End If
        
        Console.WriteLine(vbLf & "Select armor to equip:")
        For i As Integer = 0 To armors.Count - 1
            Console.WriteLine($"{i + 1}. {armors(i).Name} (DEF: {armors(i).Defense})")
        Next
        
        Dim choice As Integer
        If Integer.TryParse(Console.ReadLine(), choice) AndAlso choice > 0 AndAlso choice <= armors.Count Then
            If player.EquippedArmor IsNot Nothing Then
                player.EquippedArmor.Unequip(player)
            End If
            armors(choice - 1).Equip(player)
            player.EquippedArmor = armors(choice - 1)
            AddMessage($"Equipped {armors(choice - 1).Name}")
        End If
    End Sub
    
    Sub DropItemMenu()
        Console.WriteLine(vbLf & "Select item to drop:")
        For i As Integer = 0 To player.Inventory.Count - 1
            Console.WriteLine($"{i + 1}. {player.Inventory(i).Name}")
        Next
        
        Dim choice As Integer
        If Integer.TryParse(Console.ReadLine(), choice) AndAlso choice > 0 AndAlso choice <= player.Inventory.Count Then
            Dim item = player.Inventory(choice - 1)
            player.Inventory.RemoveAt(choice - 1)
            AddMessage($"Dropped {item.Name}")
        End If
    End Sub
    
    Sub UseConsumableMenu()
        Dim consumables = player.Inventory.OfType(Of Consumable).ToList()
        If consumables.Count = 0 Then
            Console.WriteLine("No consumables to use!")
            Thread.Sleep(1000)
            Return
        End If
        
        Console.WriteLine(vbLf & "Select consumable to use:")
        For i As Integer = 0 To consumables.Count - 1
            Console.WriteLine($"{i + 1}. {consumables(i).Name} - {consumables(i).Description}")
        Next
        
        Dim choice As Integer
        If Integer.TryParse(Console.ReadLine(), choice) AndAlso choice > 0 AndAlso choice <= consumables.Count Then
            Dim item = consumables(choice - 1)
            item.Use(player)
            player.Inventory.Remove(item)
            AddMessage($"Used {item.Name}")
        End If
    End Sub
    
    Sub CraftArrows()
        If 声誉 < 50 Then
            Console.WriteLine("Need 50 reputation to craft arrows!")
            Thread.Sleep(1000)
            Return
        End If
        
        Console.WriteLine(vbLf & "Craft arrows:")
        Console.WriteLine("[1] Standard Arrow (Cost: 10 rep)")
        Console.WriteLine("[2] Explosive Arrow (Cost: 30 rep)")
        Console.WriteLine("[3] Cryo Arrow (Cost: 30 rep)")
        Console.WriteLine("[4] Shock Arrow (Cost: 30 rep)")
        Console.Write("Select: ")
        
        Dim key = Console.ReadKey(True)
        Select Case key.KeyChar
            Case "1"c
                If 声誉 >= 10 Then
                    声誉 -= 10
                    player.Inventory.Add(New Arrow("Standard Arrow", 0, "Standard", 5, 0.1))
                    AddMessage("Crafted 1 Standard Arrow")
                End If
            Case "2"c
                If 声誉 >= 30 Then
                    声誉 -= 30
                    player.Inventory.Add(New Arrow("Explosive Arrow", 15, "Explosive", 25, 0.2))
                    AddMessage("Crafted 1 Explosive Arrow")
                End If
            Case "3"c
                If 声誉 >= 30 Then
                    声誉 -= 30
                    player.Inventory.Add(New Arrow("Cryo Arrow", 5, "Cryo", 20, 0.2))
                    AddMessage("Crafted 1 Cryo Arrow")
                End If
            Case "4"c
                If 声誉 >= 30 Then
                    声誉 -= 30
                    player.Inventory.Add(New Arrow("Shock Arrow", 10, "Shock", 25, 0.2))
                    AddMessage("Crafted 1 Shock Arrow")
                End If
        End Select
    End Sub
    
    ' Mission System
    Sub ShowMissions()
        While True
            Console.Clear()
            Console.ForegroundColor = ConsoleColor.Green
            Console.WriteLine("══════════════════════ MISSIONS ══════════════════════")
            Console.ResetColor()
            Console.WriteLine()
            
            If activeMissions.Count = 0 AndAlso completedMissions.Count = 0 Then
                Console.WriteLine("No missions available. Talk to NPCs in different districts.")
            Else
                If activeMissions.Count > 0 Then
                    Console.WriteLine("ACTIVE MISSIONS:")
                    For i As Integer = 0 To activeMissions.Count - 1
                        Dim m = activeMissions(i)
                        Console.WriteLine($"{i + 1}. {m.Name} - {m.Description}")
                        Console.WriteLine($"   Progress: {m.Progress}%  Reward: {m.RewardXP} XP, {m.RewardRep} rep")
                        Console.WriteLine()
                    Next
                End If
                
                If completedMissions.Count > 0 Then
                    Console.WriteLine("COMPLETED MISSIONS:")
                    For Each m In completedMissions
                        Console.WriteLine($"  ✓ {m.Name}")
                    Next
                End If
            End If
            
            Console.WriteLine()
            Console.WriteLine("[1] Accept New Mission   [2] Abandon Mission   [0] Back")
            Console.Write("Select: ")
            
            Dim key = Console.ReadKey(True)
            Select Case key.KeyChar
                Case "1"c : OfferNewMissions()
                Case "2"c : AbandonMission()
                Case "0"c : Exit Select
            End Select
            
            If key.KeyChar = "0"c Then Exit While
        End While
    End Sub
    
    Sub GenerateMissions()
        ' Generate some initial missions
        activeMissions.Add(New Mission("Clean Up The Glades", "Defeat 5 thugs in The Glades", "The Glades", 5, 100, 20))
        activeMissions.Add(New Mission("Arms Deal", "Stop the weapon shipment in Industrial District", "Industrial District", 3, 200, 40))
        activeMissions.Add(New Mission("Prison Break", "Prevent breakout at Iron Heights", "Iron Heights", 1, 300, 60))
    End Sub
    
    Sub OfferNewMissions()
        Console.Clear()
        Console.WriteLine("AVAILABLE MISSIONS:")
        Console.WriteLine()
        
        ' Generate random mission based on location
        Dim newMission As New Mission(
            $"Patrol {currentLocation.Name}",
            $"Clear {currentLocation.Name} of criminal activity",
            currentLocation.Name,
            random.Next(3, 8),
            random.Next(50, 150),
            random.Next(10, 30)
        )
        
        Console.WriteLine($"1. {newMission.Name}")
        Console.WriteLine($"   {newMission.Description}")
        Console.WriteLine($"   Location: {newMission.Location}")
        Console.WriteLine($"   Required: {newMission.RequiredKills} kills")
        Console.WriteLine($"   Rewards: {newMission.RewardXP} XP, {newMission.RewardRep} rep")
        Console.WriteLine()
        Console.Write("Accept mission? (Y/N): ")
        
        Dim key = Console.ReadKey(True)
        If key.KeyChar = "y"c OrElse key.KeyChar = "Y"c Then
            activeMissions.Add(newMission)
            AddMessage($"Accepted mission: {newMission.Name}")
        End If
    End Sub
    
    Sub AbandonMission()
        If activeMissions.Count = 0 Then
            Console.WriteLine("No active missions to abandon!")
            Thread.Sleep(1000)
            Return
        End If
        
        Console.WriteLine("Select mission to abandon:")
        For i As Integer = 0 To activeMissions.Count - 1
            Console.WriteLine($"{i + 1}. {activeMissions(i).Name}")
        Next
        
        Dim choice As Integer
        If Integer.TryParse(Console.ReadLine(), choice) AndAlso choice > 0 AndAlso choice <= activeMissions.Count Then
            Dim mission = activeMissions(choice - 1)
            activeMissions.RemoveAt(choice - 1)
            AddMessage($"Abandoned mission: {mission.Name}")
        End If
    End Sub
    
    Sub CompleteMission(mission As Mission)
        player.Experience += mission.RewardXP
        声誉 += mission.RewardRep
        completedMissions.Add(mission)
        activeMissions.Remove(mission)
        AddMessage($"Mission Complete: {mission.Name}! +{mission.RewardXP} XP, +{mission.RewardRep} rep")
        
        ' Check for level up
        CheckLevelUp()
    End Sub
    
    ' Faction System
    Sub ShowFactions()
        Console.Clear()
        Console.ForegroundColor = ConsoleColor.Magenta
        Console.WriteLine("══════════════════════ FACTIONS ══════════════════════")
        Console.ResetColor()
        Console.WriteLine()
        
        For Each faction In factions.Values
            Dim relationship As String
            If faction.Relationship >= 50 Then
                relationship = "Ally"
                Console.ForegroundColor = ConsoleColor.Green
            ElseIf faction.Relationship >= 0 Then
                relationship = "Neutral"
                Console.ForegroundColor = ConsoleColor.Yellow
            Else
                relationship = "Hostile"
                Console.ForegroundColor = ConsoleColor.Red
            End If
            
            Console.WriteLine($"{faction.Name} [{faction.Type}]")
            Console.WriteLine($"  Relationship: {faction.Relationship} ({relationship})")
            Console.ResetColor()
            Console.WriteLine($"  Reputation: {GetFactionRepDescription(faction.Relationship)}")
            Console.WriteLine()
        Next
        
        Console.WriteLine($"Your overall reputation: {声誉}")
        Console.WriteLine()
        Console.WriteLine("Press any key to continue...")
        Console.ReadKey()
    End Sub
    
    Function GetFactionRepDescription(rep As Integer) As String
        If rep >= 80 Then Return "Honored Member"
        If rep >= 50 Then Return "Respected"
        If rep >= 20 Then Return "Friendly"
        If rep >= 0 Then Return "Neutral"
        If rep >= -30 Then Return "Suspicious"
        If rep >= -60 Then Return "Unfriendly"
        Return "Enemy"
    End Function
    
    ' Training System
    Sub Train()
        Console.Clear()
        Console.ForegroundColor = ConsoleColor.Cyan
        Console.WriteLine("══════════════════════ TRAINING GROUNDS ══════════════════════")
        Console.ResetColor()
        Console.WriteLine()
        
        Console.WriteLine("Training Options:")
        Console.WriteLine("[1] Combat Training - 50 XP → +5 Max Health")
        Console.WriteLine("[2] Archery Practice - 75 XP → +3 Damage")
        Console.WriteLine("[3] Stealth Training - 60 XP → Better escape chance")
        Console.WriteLine("[4] Endurance - 80 XP → Faster healing")
        Console.WriteLine("[0] Cancel")
        Console.WriteLine()
        Console.WriteLine($"Available XP: {player.Experience}")
        Console.Write("Select: ")
        
        Dim key = Console.ReadKey(True)
        Select Case key.KeyChar
            Case "1"c
                If player.Experience >= 50 Then
                    player.Experience -= 50
                    player.MaxHealth += 5
                    player.Health = player.MaxHealth
                    AddMessage("Training complete! Max Health +5")
                End If
                
            Case "2"c
                If player.Experience >= 75 Then
                    player.Experience -= 75
                    player.DamageBonus += 3
                    AddMessage("Training complete! Damage +3")
                End If
                
            Case "3"c
                If player.Experience >= 60 Then
                    player.Experience -= 60
                    player.EscapeBonus += 10
                    AddMessage("Training complete! Escape chance +10%")
                End If
                
            Case "4"c
                If player.Experience >= 80 Then
                    player.Experience -= 80
                    player.HealingRate += 2
                    AddMessage("Training complete! Healing rate +2")
                End If
        End Select
    End Sub
    
    ' Rest/Heal System
    Sub Rest()
        Console.Clear()
        Console.ForegroundColor = ConsoleColor.Blue
        Console.WriteLine("══════════════════════ RESTING ══════════════════════")
        Console.ResetColor()
        Console.WriteLine()
        
        Dim healAmount As Integer = 20 + player.HealingRate
        If currentLocation.Type = LocationType.Safe Then
            healAmount *= 2
        End If
        
        Console.WriteLine($"You rest for a while...")
        Console.WriteLine($"Healed for {healAmount} health!")
        
        player.Health = Math.Min(player.MaxHealth, player.Health + healAmount)
        
        ' Random encounter chance when resting in dangerous areas
        If currentLocation.Type = LocationType.Dangerous AndAlso random.Next(100) < 30 Then
            Console.WriteLine("You're interrupted by enemies!")
            Thread.Sleep(1000)
            If currentLocation.Enemies.Count > 0 Then
                StartCombat(currentLocation.Enemies(random.Next(currentLocation.Enemies.Count)))
            End If
        End If
        
        Console.WriteLine("Press any key to continue...")
        Console.ReadKey()
    End Sub
    
    ' Travel System
    Sub Travel()
        Console.Clear()
        Console.ForegroundColor = ConsoleColor.Yellow
        Console.WriteLine("══════════════════════ TRAVEL ══════════════════════")
        Console.ResetColor()
        Console.WriteLine()
        
        Console.WriteLine($"Current Location: {currentLocation.Name}")
        Console.WriteLine("Available destinations:")
        Console.WriteLine()
        
        Dim destinations = currentLocation.Connections.ToList()
        For i As Integer = 0 To destinations.Count - 1
            Console.WriteLine($"{i + 1}. {destinations(i).Name} [{destinations(i).Type}]")
        Next
        Console.WriteLine("0. Cancel")
        Console.Write("Select destination: ")
        
        Dim choice As Integer
        If Integer.TryParse(Console.ReadLine(), choice) AndAlso choice > 0 AndAlso choice <= destinations.Count Then
            Dim newLocation = destinations(choice - 1)
            currentLocation = newLocation
            player.Location = newLocation
            AddMessage($"Traveled to {newLocation.Name}")
            
            ' Random encounter during travel
            If random.Next(100) < 20 Then
                Console.WriteLine("Enemies ambush you during travel!")
                Thread.Sleep(1000)
                If newLocation.Enemies.Count > 0 Then
                    StartCombat(newLocation.Enemies(random.Next(newLocation.Enemies.Count)))
                End If
            End If
        End If
    End Sub
    
    ' Quick Combat - for fast grinding
    Sub QuickCombat()
        If currentLocation.Enemies.Count = 0 Then
            AddMessage("No enemies here!")
            Return
        End If
        
        Dim enemy = currentLocation.Enemies(random.Next(currentLocation.Enemies.Count))
        StartCombat(enemy)
    End Sub
    
    ' Save/Load System
    Sub SaveGame()
        Console.Clear()
        Console.WriteLine("Saving game...")
        
        ' In a real game, this would serialize to a file
        ' For demo, we'll just show a message
        Thread.Sleep(500)
        AddMessage("Game saved successfully!")
        
        Console.WriteLine("Press any key to continue...")
        Console.ReadKey()
    End Sub
    
    Sub LoadGame()
        Console.Clear()
        Console.WriteLine("Loading game...")
        
        ' In a real game, this would deserialize from a file
        ' For demo, we'll just show a message
        Thread.Sleep(500)
        AddMessage("Game loaded successfully!")
        
        Console.WriteLine("Press any key to continue...")
        Console.ReadKey()
    End Sub
    
    ' Helper Functions
    Function GetTimeString() As String
        Dim hour = (gameTime Mod 24).ToString("00")
        Dim minute = (gameTime * 5 Mod 60).ToString("00")
        Return $"{hour}:{minute}"
    End Function
    
    Function GetDayCycle() As String
        Return If(dayCycle = 0, "DAY", "NIGHT")
    End Function
    
    Sub AddMessage(msg As String)
        gameMessages.Add(msg)
        If gameMessages.Count > 10 Then
            gameMessages.RemoveAt(0)
        End If
    End Sub
    
    Function CalculateInventoryWeight() As Double
        Return player.Inventory.Sum(Function(i) i.Weight)
    End Function
    
    Sub CheckLevelUp()
        While player.Experience >= player.Level * 100
            player.Level += 1
            player.MaxHealth += 10
            player.Health = player.MaxHealth
            AddMessage($"Level Up! Now level {player.Level}")
        End While
    End Sub
    
    Sub ShowGameOver()
        Console.Clear()
        Console.ForegroundColor = ConsoleColor.Red
        Console.WriteLine(@"
        ╔══════════════════════════════════════════════════╗
        ║                                                  ║
        ║     ██████╗  █████╗ ███╗   ███╗███████╗          ║
        ║    ██╔════╝ ██╔══██╗████╗ ████║██╔════╝          ║
        ║    ██║  ███╗███████║██╔████╔██║█████╗            ║
        ║    ██║   ██║██╔══██║██║╚██╔╝██║██╔══╝            ║
        ║    ╚██████╔╝██║  ██║██║ ╚═╝ ██║███████╗          ║
        ║     ╚═════╝ ╚═╝  ╚═╝╚═╝     ╚═╝╚══════╝          ║
        ║                                                  ║
        ║               GAME OVER                          ║
        ║                                                  ║
        ╚══════════════════════════════════════════════════╝")
        Console.ResetColor()
        Console.WriteLine()
        Console.WriteLine($"Final Stats:")
        Console.WriteLine($"Level: {player.Level}")
        Console.WriteLine($"Missions Completed: {completedMissions.Count}")
        Console.WriteLine($"Reputation: {声誉}")
        Console.WriteLine()
        Console.WriteLine("Thanks for playing ARROW: Star City Vigilante!")
        Console.WriteLine("Press any key to exit...")
        Console.ReadKey()
    End Sub
End Module

' ====================== CLASS DEFINITIONS ======================

Public Enum LocationType
    Safe
    Normal
    Dangerous
    HighSecurity
End Enum

Class Location
    Public Property Name As String
    Public Property Description As String
    Public Property Type As LocationType
    Public Property Enemies As New List(Of Enemy)
    Public Property NPCs As New List(Of NPC)
    Public Property Connections As New List(Of Location)
    
    Public Sub New(name As String, description As String, type As LocationType)
        Me.Name = name
        Me.Description = description
        Me.Type = type
    End Sub
    
    Public Sub AddConnection(location As Location)
        If Not Connections.Contains(location) Then
            Connections.Add(location)
        End If
    End Sub
    
    Public Sub AddEnemy(enemy As Enemy)
        Enemies.Add(enemy)
    End Sub
    
    Public Sub AddNPC(npc As NPC)
        NPCs.Add(npc)
    End Sub
    
    Public Sub Update()
        ' Regenerate enemies over time
        If Enemies.Count < 3 AndAlso Type = LocationType.Dangerous Then
            If New Random().Next(100) < 10 Then
                Enemies.Add(New Enemy("Street Thug", 30, 5, 8, "Human", 10))
            End If
        End If
    End Sub
End Class

Class Faction
    Public Property Name As String
    Public Property Relationship As Integer
    Public Property Type As String
    
    Public Sub New(name As String, relationship As Integer, type As String)
        Me.Name = name
        Me.Relationship = relationship
        Me.Type = type
    End Sub
End Class

Class Mission
    Public Property Name As String
    Public Property Description As String
    Public Property Location As String
    Public Property RequiredKills As Integer
    Public Property CurrentKills As Integer
    Public Property RewardXP As Integer
    Public Property RewardRep As Integer
    Public Property IsCompleted As Boolean
    
    Public ReadOnly Property Progress As Integer
        Get
            Return CInt((CurrentKills / RequiredKills) * 100)
        End Get
    End Property
    
    Public Sub New(name As String, description As String, location As String, 
                   requiredKills As Integer, rewardXP As Integer, rewardRep As Integer)
        Me.Name = name
        Me.Description = description
        Me.Location = location
        Me.RequiredKills = requiredKills
        Me.RewardXP = rewardXP
        Me.RewardRep = rewardRep
        Me.CurrentKills = 0
        Me.IsCompleted = False
    End Sub
    
    Public Sub Update()
        If CurrentKills >= RequiredKills AndAlso Not IsCompleted Then
            IsCompleted = True
        End If
    End Sub
    
    Public Sub AddKill()
        If Not IsCompleted Then
            CurrentKills += 1
        End If
    End Sub
End Class

Class Player
    Public Property Name As String
    Public Property Health As Integer
    Public Property MaxHealth As Integer
    Public Property Level As Integer
    Public Property Experience As Integer
    Private Property DamageBonus As Integer = 0
    Private Property EscapeBonus As Integer = 0
    Private Property HealingRate As Integer = 0
    Public Property Inventory As New List(Of Item)
    Public Property EquippedWeapon As Weapon
    Public Property EquippedArmor As Armor
    Public Property Location As Location
    
    Public Sub New(name As String)
        Me.Name = name
        Me.MaxHealth = 100
        Me.Health = MaxHealth
        Me.Level = 1
        Me.Experience = 0
    End Sub
    
    Public Function CalculateDamage() As Integer
        If EquippedWeapon IsNot Nothing Then
            Return EquippedWeapon.Damage + DamageBonus
        End If
        Return 5 + DamageBonus ' Base damage
    End Function
    
    Public Sub TakeDamage(amount As Integer)
        Dim defense As Integer = If(EquippedArmor IsNot Nothing, EquippedArmor.Defense, 0)
        Dim netDamage = Math.Max(1, amount - defense)
        Health -= netDamage
        If Health < 0 Then Health = 0
    End Sub
End Class

Class Enemy
    Public Property Name As String
    Private Property Health As Integer
    Private Property MaxHealth As Integer
    Private Property Damage As Integer
    Private Property Speed As Integer
    Private Property Accuracy As Integer = 80
    Public Property Type As String
    Public Property ExperienceValue As Integer
    Public Property ReputationValue As Integer
    
    Public Sub New(name As String, health As Integer, damage As Integer, speed As Integer, 
                   type As String, expValue As Integer)
        Me.Name = name
        Me.MaxHealth = health
        Me.Health = health
        Me.Damage = damage
        Me.Speed = speed
        Me.Type = type
        Me.ExperienceValue = expValue
        Me.ReputationValue = expValue \ 2
    End Sub
    
    Public Function CalculateDamage() As Integer
        Return Damage + New Random().Next(-2, 3)
    End Function
    
    Public Sub TakeDamage(amount As Integer)
        Health -= amount
        If Health < 0 Then Health = 0
    End Sub
End Class

Class NPC
    Public Property Name As String
    Public Property Role As String
    Public Property Dialogue As String
    
    Public Sub New(name As String, role As String, dialogue As String)
        Me.Name = name
        Me.Role = role
        Me.Dialogue = dialogue
    End Sub
End Class

Public MustInherit Class Item
    Public Property Name As String
    Public Property Weight As Double
    
    Public Sub New(name As String, weight As Double)
        Me.Name = name
        Me.Weight = weight
    End Sub
End Class

Class Weapon
    Inherits Item
    Public Property Damage As Integer
    Public Property Durability As Integer
    
    Public Sub New(name As String, damage As Integer, durability As Integer, weight As Double)
        MyBase.New(name, weight)
        Me.Damage = damage
        Me.Durability = durability
    End Sub
End Class

Class Bow
    Inherits Weapon
    Public Property LoadedArrow As Arrow
    
    Public Sub New(name As String, damage As Integer, durability As Integer, weight As Double)
        MyBase.New(name, damage, durability, weight)
    End Sub
    
    Public Sub LoadArrow(arrow As Arrow)
        LoadedArrow = arrow
    End Sub
End Class

Class Arrow
    Inherits Item
    Public Property AdditionalDamage As Integer
    Public Property Effect As String
    
    Public Sub New(name As String, additionalDamage As Integer, effect As String, value As Integer, weight As Double)
        MyBase.New(name, weight)
        Me.AdditionalDamage = additionalDamage
        Me.Effect = effect
    End Sub
End Class

Class Armor
    Inherits Item
    Public Property Defense As Integer
    Public Property IsEquipped As Boolean = False
    
    Public Sub New(name As String, defense As Integer, value As Integer, weight As Double)
        MyBase.New(name, weight)
        Me.Defense = defense
    End Sub
    
    Public Sub Equip(player As Player)
        IsEquipped = True
    End Sub
    
    Public Sub Unequip(player As Player)
        IsEquipped = False
    End Sub
End Class

Class Consumable
    Inherits Item
    Public Property Description As String
    Public Property HealAmount As Integer
    
    Public Sub New(name As String, description As String, healAmount As Integer, weight As Double)
        MyBase.New(name, weight)
        Me.Description = description
        Me.HealAmount = healAmount
    End Sub
    
    Public Sub Use(player As Player)
        player.Health = Math.Min(player.MaxHealth, player.Health + HealAmount)
    End Sub
End Class