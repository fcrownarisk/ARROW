' ============================================================================
' ARROW: THE COMPLETE SAGA - ACTION & CRAFTING SYSTEM
' Enhanced VB.NET Game Engine with Character-Based Combat and Crafting
' Version 6.0 - "The Arsenal Edition"
' ============================================================================

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports System.Math
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks

Partial Public Class ArrowSagaEngine
    Inherits Form
    
    #Region "Action & Combat Enumerations"
    
    Public Enum CombatStyle
        Archery
        HandToHand
        Swordplay
        Stealth
        Acrobatic
        Tactical
        Mystic
        TechBased
    End Enum
    
    Public Enum SkillLevel
        Novice = 1
        Apprentice = 2
        Journeyman = 3
        Expert = 4
        Master = 5
        Legendary = 6
    End Enum
    
    Public Enum ArrowCraftingType
        Standard
        Explosive
        Incendiary
        Cryo
        Electric
        Sonic
        Smoke
        Flashbang
        Grappling
        Net
        BoxingGlove
        USB
        Tracker
        EMP
        Nerve
        Taser
        Boomerang
        Drill
        Magnetic
        Glue
        Acid
        Flare
        Tranquilizer
        Blunt
        Shrapnel
        Phantom
        Dragonfire
        Stun
        Disruption
        Polaris
    End Enum
    
    Public Enum GadgetType
        Flashbang
        SmokePellet
        ExplosiveCharge
        GrapplingHook
        Lockpick
        HackingDevice
        EMPGenerator
        SonicEmitter
        Decoy
        Trap
        Medkit
        Stimulant
        Antidote
    End Enum
    
    Public Enum CharacterClass
        Archer
        Brawler
        Assassin
        Tactician
        TechExpert
        Medic
        Acrobat
        Leader
    End Enum
    
    #End Region
    
    #Region "Action System Classes"
    
    ''' <summary>
    ''' Represents a character's combat abilities and fighting style
    ''' </summary>
    Public Class CombatProfile
        Public Property CharacterID As CharacterID
        Public Property PrimaryStyle As CombatStyle
        Public Property SecondaryStyles As List(Of CombatStyle)
        Public Property BaseDamage As Integer
        Public Property AttackSpeed As Double
        Public Property CriticalChance As Double
        Public Property CriticalDamage As Double
        Public Property ComboMeter As Integer = 0
        Public Property MaxCombo As Integer = 100
        Public Property SpecialMoves As List(Of SpecialMove)
        Public Property Finishers As List(Of FinisherMove)
        Public Property KillCount As Integer = 0
        Public Property NonLethalCount As Integer = 0
        
        Public Sub New()
            SecondaryStyles = New List(Of CombatStyle)
            SpecialMoves = New List(Of SpecialMove)
            Finishers = New List(Of FinisherMove)
        End Sub
    End Class
    
    ''' <summary>
    ''' Special combat moves unique to each character
    ''' </summary>
    Public Class SpecialMove
        Public Property Name As String
        Public Property CharacterID As CharacterID
        Public Property RequiredLevel As SkillLevel
        Public Property DamageMultiplier As Double
        Public Property Cooldown As Double
        Public Property CurrentCooldown As Double
        Public Property Animation As String
        Public Property SoundEffect As String
        Public Property VisualEffect As String
        Public Property Requirements As List(Of String)
        Public Property Description As String
        Public Property IsUnlocked As Boolean = False
        
        Public Function CanUse() As Boolean
            Return IsUnlocked AndAlso CurrentCooldown <= 0
        End Function
        
        Public Sub Use()
            CurrentCooldown = Cooldown
        End Sub
    End Class
    
    ''' <summary>
    ''' Epic finishing moves
    ''' </summary>
    Public Class FinisherMove
        Inherits SpecialMove
        
        Public Property RequiredCombo As Integer
        Public Property EnemyHealthThreshold As Double
        Public Property IsLethal As Boolean = True
        Public Property CinematicCamera As Boolean = True
        Public Property SlowMotion As Boolean = True
    End Class
    
    ''' <summary>
    ''' Combat encounter manager
    ''' </summary>
    Public Class CombatEncounter
        Public Property EncounterID As String
        Public Property Episode As Episode
        Public Property Enemies As List(Of CombatEnemy)
        Public Property Allies As List(Of CharacterID)
        Public Property Environment As String
        Public Property Objectives As List(Of String)
        Public Property IsBossFight As Boolean = False
        Public Property BossName As String
        Public Property TimeLimit As Double? = Nothing
        Public Property RequiredScore As Integer = 0
        Public Property BonusObjectives As List(Of String)
        Public Property Rewards As CombatRewards
        
        Public Sub New()
            Enemies = New List(Of CombatEnemy)
            Allies = New List(Of CharacterID)
            Objectives = New List(Of String)
            BonusObjectives = New List(Of String)
            Rewards = New CombatRewards()
        End Sub
    End Class
    
    ''' <summary>
    ''' Enemy in combat
    ''' </summary>
    Public Class CombatEnemy
        Public Property CharacterID As CharacterID
        Public Property Level As Integer
        Public Property Health As Integer
        Public Property MaxHealth As Integer
        Public Property Shield As Integer = 0
        Public Property Damage As Integer
        Public Property AttackPattern As List(Of String)
        Public Property Weaknesses As List(Of ArrowCraftingType)
        Public Property Resistances As List(Of ArrowCraftingType)
        Public Property IsBoss As Boolean = False
        Public Property Phase As Integer = 1
        Public Property MaxPhases As Integer = 1
        Public Property LootTable As List(Of CraftingMaterial)
        
        Public Sub New(id As CharacterID, level As Integer)
            Me.CharacterID = id
            Me.Level = level
            Me.MaxHealth = 50 + (level * 25)
            Me.Health = MaxHealth
            Me.Damage = 5 + (level * 3)
            AttackPattern = New List(Of String)
            Weaknesses = New List(Of ArrowCraftingType)
            Resistances = New List(Of ArrowCraftingType)
            LootTable = New List(Of CraftingMaterial)
        End Sub
    End Class
    
    ''' <summary>
    ''' Rewards for completing combat
    ''' </summary>
    Public Class CombatRewards
        Public Property Experience As Integer
        Public Property Materials As List(Of CraftingMaterial)
        Public Property UnlockedMoves As List(Of String)
        Public Property ReputationGains As Dictionary(Of Faction, Integer)
        Public Property Money As Integer
        Public Property SpecialItems As List(Of String)
        
        Public Sub New()
            Materials = New List(Of CraftingMaterial)
            ReputationGains = New Dictionary(Of Faction, Integer)
            SpecialItems = New List(Of String)
        End Sub
    End Class
    
    #End Region
    
    #Region "Crafting System Classes"
    
    ''' <summary>
    ''' Crafting material
    ''' </summary>
    Public Class CraftingMaterial
        Public Property MaterialID As String
        Public Property Name As String
        Public Property Description As String
        Public Property Rarity As Integer ' 1-5
        Public Property Source As String
        Public Property EpisodeSource As Episode
        Public Property CharacterSource As CharacterID
        Public Property Quantity As Integer = 0
        Public Property MaxQuantity As Integer = 99
        Public Property Icon As String
        Public Property Value As Integer
        
        Public Overrides Function ToString() As String
            Return $"{Name} x{Quantity}"
        End Function
    End Class
    
    ''' <summary>
    ''' Arrow recipe
    ''' </summary>
    Public Class ArrowRecipe
        Public Property ArrowType As ArrowCraftingType
        Public Property Name As String
        Public Property Description As String
        Public Property RequiredMaterials As Dictionary(Of String, Integer)
        Public Property RequiredLevel As SkillLevel
        Public Property RequiredEpisode As Episode
        Public Property CraftingTime As Double
        Public Property ResultCount As Integer = 1
        Public Property UnlockedBy As String
        Public Property CharacterSpecific As CharacterID? = Nothing
        
        Public Sub New()
            RequiredMaterials = New Dictionary(Of String, Integer)
        End Sub
        
        Public Function CanCraft(inventory As Dictionary(Of String, Integer)) As Boolean
            For Each material In RequiredMaterials
                If Not inventory.ContainsKey(material.Key) OrElse 
                   inventory(material.Key) < material.Value Then
                    Return False
                End If
            Next
            Return True
        End Function
    End Class
    
    ''' <summary>
    ''' Gadget recipe
    ''' </summary>
    Public Class GadgetRecipe
        Public Property GadgetType As GadgetType
        Public Property Name As String
        Public Property Description As String
        Public Property RequiredMaterials As Dictionary(Of String, Integer)
        Public Property RequiredLevel As SkillLevel
        Public Property RequiredEpisode As Episode
        Public Property CraftingTime As Double
        Public Property ResultCount As Integer = 1
        Public Property TechLevel As Integer = 1
        Public Property CreatedBy As CharacterID
        
        Public Sub New()
            RequiredMaterials = New Dictionary(Of String, Integer)
        End Sub
    End Class
    
    ''' <summary>
    ''' Character skill tree
    ''' </summary>
    Public Class SkillTree
        Public Property CharacterID As CharacterID
        Public Property SkillPoints As Integer = 0
        Public Property TotalSpent As Integer = 0
        Public Property ArcheryBranch As List(Of SkillNode)
        Public Property CombatBranch As List(Of SkillNode)
        Public Property StealthBranch As List(Of SkillNode)
        Public Property TechBranch As List(Of SkillNode)
        Public Property LeadershipBranch As List(Of SkillNode)
        
        Public Sub New()
            ArcheryBranch = New List(Of SkillNode)
            CombatBranch = New List(Of SkillNode)
            StealthBranch = New List(Of SkillNode)
            TechBranch = New List(Of SkillNode)
            LeadershipBranch = New List(Of SkillNode)
        End Sub
    End Class
    
    ''' <summary>
    ''' Individual skill node
    ''' </summary>
    Public Class SkillNode
        Public Property NodeID As String
        Public Property Name As String
        Public Property Description As String
        Public Property Level As Integer = 1
        Public Property MaxLevel As Integer = 5
        Public Property CostPerLevel As Integer = 1
        Public Property Prerequisites As List(Of String)
        Public Property UnlockedAbilities As List(Of String)
        Public Property UnlockedRecipes As List(Of ArrowCraftingType)
        Public Property StatBonuses As Dictionary(Of String, Integer)
        Public Property Position As Point
        Public Property IsUnlocked As Boolean = False
        
        Public Sub New()
            Prerequisites = New List(Of String)
            UnlockedAbilities = New List(Of String)
            UnlockedRecipes = New List(Of ArrowCraftingType)
            StatBonuses = New Dictionary(Of String, Integer)
        End Sub
    End Class
    
    ''' <summary>
    ''' Arrow inventory with special properties
    ''' </summary>
    Public Class ArrowInventory
        Public Property ArrowType As ArrowCraftingType
        Public Property Quantity As Integer = 0
        Public Property MaxQuantity As Integer = 50
        Public Property DamageBonus As Integer = 0
        Public Property EffectRadius As Double = 1.0
        Public Property EffectDuration As Double = 1.0
        Public Property SpecialProperties As List(Of String)
        Public Property UpgradeLevel As Integer = 1
        
        Public Sub New()
            SpecialProperties = New List(Of String)
        End Sub
    End Class
    
    ''' <summary>
    ''' Crafting station
    ''' </summary>
    Public Class CraftingStation
        Public Property StationID As String
        Public Property Name As String
        Public Property Location As String
        Public Property AllowedRecipes As List(Of ArrowCraftingType)
        Public Property AllowedGadgets As List(Of GadgetType)
        Public Property CraftingSpeed As Double = 1.0
        Public Property QualityBonus As Double = 1.0
        Public Property IsAvailable As Boolean = True
        Public Property OwnedBy As CharacterID? = Nothing
        
        Public Sub New()
            AllowedRecipes = New List(Of ArrowCraftingType)
            AllowedGadgets = New List(Of GadgetType)
        End Sub
    End Class
    
    #End Region
    
    #Region "Character-Specific Action Systems"
    
    ''' <summary>
    ''' Oliver Queen / Green Arrow - Master Archer
    ''' </summary>
    Public Class OliverQueenActions
        Private _combatProfile As CombatProfile
        Private _skillTree As SkillTree
        Private _specialMoves As Dictionary(Of String, SpecialMove)
        
        Public Sub New()
            _combatProfile = New CombatProfile() With {
                .CharacterID = CharacterID.OliverQueen,
                .PrimaryStyle = CombatStyle.Archery,
                .SecondaryStyles = New List(Of CombatStyle) From {
                    CombatStyle.HandToHand, CombatStyle.Stealth, CombatStyle.Acrobatic
                },
                .BaseDamage = 30,
                .AttackSpeed = 2.0,
                .CriticalChance = 0.25,
                .CriticalDamage = 2.5
            }
            
            InitializeSpecialMoves()
            InitializeSkillTree()
        End Sub
        
        Private Sub InitializeSpecialMoves()
            _specialMoves = New Dictionary(Of String, SpecialMove)
            
            ' Signature moves
            _specialMoves("TripleShot") = New SpecialMove() With {
                .Name = "Triple Shot",
                .CharacterID = CharacterID.OliverQueen,
                .RequiredLevel = SkillLevel.Expert,
                .DamageMultiplier = 0.7,
                .Cooldown = 5.0,
                .Description = "Fire three arrows simultaneously at different targets",
                .Animation = "triple_shot",
                .SoundEffect = "arrow_volley"
            }
            
            _specialMoves("TrickShot") = New SpecialMove() With {
                .Name = "Trick Shot",
                .CharacterID = CharacterID.OliverQueen,
                .RequiredLevel = SkillLevel.Master,
                .DamageMultiplier = 1.5,
                .Cooldown = 3.0,
                .Description = "Ricochet arrow off surfaces to hit behind cover",
                .Animation = "trick_shot",
                .SoundEffect = "arrow_ricochet"
            }
            
            _specialMoves("RapidFire") = New SpecialMove() With {
                .Name = "Rapid Fire",
                .CharacterID = CharacterID.OliverQueen,
                .RequiredLevel = SkillLevel.Journeyman,
                .DamageMultiplier = 0.4,
                .Cooldown = 8.0,
                .Description = "Fire a rapid succession of arrows",
                .Animation = "rapid_fire",
                .SoundEffect = "machine_gun_arrows"
            }
            
            _specialMoves("PrecisionShot") = New SpecialMove() With {
                .Name = "Precision Shot",
                .CharacterID = CharacterID.OliverQueen,
                .RequiredLevel = SkillLevel.Expert,
                .DamageMultiplier = 3.0,
                .Cooldown = 10.0,
                .Description = "Aim for weak points, guaranteed critical hit",
                .Animation = "precision_shot",
                .SoundEffect = "arrow_precision"
            }
            
            ' Finishers
            _specialMoves("OliverFinisher1") = New FinisherMove() With {
                .Name = "The Hood's Judgment",
                .CharacterID = CharacterID.OliverQueen,
                .RequiredLevel = SkillLevel.Legendary,
                .RequiredCombo = 50,
                .DamageMultiplier = 5.0,
                .EnemyHealthThreshold = 0.2,
                .Cooldown = 30.0,
                .Description = "A devastating finishing move using multiple trick arrows",
                .CinematicCamera = True,
                .SlowMotion = True
            }
        End Sub
        
        Private Sub InitializeSkillTree()
            _skillTree = New SkillTree() With {
                .CharacterID = CharacterID.OliverQueen
            }
            
            ' Archery Branch
            _skillTree.ArcheryBranch.Add(New SkillNode() With {
                .NodeID = "archery_1",
                .Name = "Basic Archery",
                .Description = "Improve accuracy and damage",
                .StatBonuses = New Dictionary(Of String, Integer) From {{"Damage", 5}, {"Accuracy", 10}}
            })
            
            _skillTree.ArcheryBranch.Add(New SkillNode() With {
                .NodeID = "archery_2",
                .Name = "Trick Shots",
                .Description = "Unlock ricochet ability",
                .Prerequisites = New List(Of String) From {"archery_1"},
                .UnlockedAbilities = New List(Of String) From {"TrickShot"}
            })
            
            _skillTree.ArcheryBranch.Add(New SkillNode() With {
                .NodeID = "archery_3",
                .Name = "Rapid Fire",
                .Description = "Increase attack speed",
                .Prerequisites = New List(Of String) From {"archery_2"},
                .StatBonuses = New Dictionary(Of String, Integer) From {{"AttackSpeed", 20}}
            })
            
            ' Combat Branch
            _skillTree.CombatBranch.Add(New SkillNode() With {
                .NodeID = "combat_1",
                .Name = "Hand-to-Hand",
                .Description = "Improve melee combat",
                .StatBonuses = New Dictionary(Of String, Integer) From {{"MeleeDamage", 10}}
            })
            
            _skillTree.CombatBranch.Add(New SkillNode() With {
                .NodeID = "combat_2",
                .Name = "Combo Master",
                .Description = "Extend combo duration",
                .StatBonuses = New Dictionary(Of String, Integer) From {{"ComboTime", 50}}
            })
            
            ' Stealth Branch
            _skillTree.StealthBranch.Add(New SkillNode() With {
                .NodeID = "stealth_1",
                .Name = "Silent Takedown",
                .Description = "Execute silent takedowns",
                .StatBonuses = New Dictionary(Of String, Integer) From {{"StealthDamage", 25}}
            })
        End Sub
        
        Public Function ExecuteSpecialMove(moveName As String, context As CombatContext) As CombatResult
            If Not _specialMoves.ContainsKey(moveName) Then
                Return New CombatResult() With {.Success = False}
            End If
            
            Dim move = _specialMoves(moveName)
            
            If Not move.CanUse() Then
                Return New CombatResult() With {.Success = False, .Message = "Move on cooldown"}
            End If
            
            move.Use()
            
            ' Calculate damage and effects
            Dim result As New CombatResult() With {
                .Success = True,
                .Damage = CInt(context.BaseDamage * move.DamageMultiplier),
                .Message = $"Executed {move.Name}!",
                .SpecialEffects = New List(Of String) From {move.VisualEffect}
            }
            
            ' Update combo meter
            _combatProfile.ComboMeter = Min(_combatProfile.MaxCombo, 
                                           _combatProfile.ComboMeter + 10)
            
            Return result
        End Function
    End Class
    
    ''' <summary>
    ''' John Diggle / Spartan - Heavy Combat Specialist
    ''' </summary>
    Public Class JohnDiggleActions
        Private _combatProfile As CombatProfile
        Private _specialMoves As Dictionary(Of String, SpecialMove)
        
        Public Sub New()
            _combatProfile = New CombatProfile() With {
                .CharacterID = CharacterID.JohnDiggle,
                .PrimaryStyle = CombatStyle.Tactical,
                .SecondaryStyles = New List(Of CombatStyle) From {
                    CombatStyle.HandToHand, CombatStyle.Archery
                },
                .BaseDamage = 35,
                .AttackSpeed = 1.2,
                .CriticalChance = 0.15,
                .CriticalDamage = 2.0
            }
            
            InitializeSpecialMoves()
        End Sub
        
        Private Sub InitializeSpecialMoves()
            _specialMoves = New Dictionary(Of String, SpecialMove)
            
            _specialMoves("TacticalStrike") = New SpecialMove() With {
                .Name = "Tactical Strike",
                .CharacterID = CharacterID.JohnDiggle,
                .DamageMultiplier = 2.0,
                .Cooldown = 4.0,
                .Description = "Coordinate attack with team members"
            }
            
            _specialMoves("SuppressingFire") = New SpecialMove() With {
                .Name = "Suppressing Fire",
                .CharacterID = CharacterID.JohnDiggle,
                .DamageMultiplier = 0.5,
                .Cooldown = 6.0,
                .Description = "Pin down enemies with sustained fire"
            }
            
            _specialMoves("Bodyguard") = New SpecialMove() With {
                .Name = "Bodyguard",
                .CharacterID = CharacterID.JohnDiggle,
                .DamageMultiplier = 0,
                .Cooldown = 15.0,
                .Description = "Protect an ally from incoming damage"
            }
        End Sub
    End Class
    
    ''' <summary>
    ''' Thea Queen / Speedy - Agile Acrobat
    ''' </summary>
    Public Class TheaQueenActions
        Private _combatProfile As CombatProfile
        
        Public Sub New()
            _combatProfile = New CombatProfile() With {
                .CharacterID = CharacterID.TheaQueen,
                .PrimaryStyle = CombatStyle.Acrobatic,
                .SecondaryStyles = New List(Of CombatStyle) From {
                    CombatStyle.Archery, CombatStyle.Stealth
                },
                .BaseDamage = 25,
                .AttackSpeed = 3.0,
                .CriticalChance = 0.3,
                .CriticalDamage = 2.2
            }
        End Sub
    End Class
    
    ''' <summary>
    ''' Roy Harper / Arsenal - Aggressive Brawler
    ''' </summary>
    Public Class RoyHarperActions
        Private _combatProfile As CombatProfile
        
        Public Sub New()
            _combatProfile = New CombatProfile() With {
                .CharacterID = CharacterID.RoyHarper,
                .PrimaryStyle = CombatStyle.HandToHand,
                .SecondaryStyles = New List(Of CombatStyle) From {
                    CombatStyle.Archery, CombatStyle.Acrobatic
                },
                .BaseDamage = 28,
                .AttackSpeed = 2.5,
                .CriticalChance = 0.2,
                .CriticalDamage = 2.3
            }
        End Sub
    End Class
    
    ''' <summary>
    ''' Sara Lance / White Canary - Lethal Assassin
    ''' </summary>
    Public Class SaraLanceActions
        Private _combatProfile As CombatProfile
        
        Public Sub New()
            _combatProfile = New CombatProfile() With {
                .CharacterID = CharacterID.SaraLance,
                .PrimaryStyle = CombatStyle.Swordplay,
                .SecondaryStyles = New List(Of CombatStyle) From {
                    CombatStyle.Stealth, CombatStyle.Acrobatic
                },
                .BaseDamage = 32,
                .AttackSpeed = 2.8,
                .CriticalChance = 0.35,
                .CriticalDamage = 2.4
            }
        End Sub
    End Class
    
    ''' <summary>
    ''' Laurel Lance / Black Canary - Sonic Fighter
    ''' </summary>
    Public Class LaurelLanceActions
        Private _combatProfile As CombatProfile
        
        Public Sub New()
            _combatProfile = New CombatProfile() With {
                .CharacterID = CharacterID.LaurelLance,
                .PrimaryStyle = CombatStyle.HandToHand,
                .SecondaryStyles = New List(Of CombatStyle) From {
                    CombatStyle.TechBased
                },
                .BaseDamage = 22,
                .AttackSpeed = 2.0,
                .CriticalChance = 0.2,
                .CriticalDamage = 2.0
            }
        End Sub
        
        Public Function CanaryCry() As CombatResult
            Return New CombatResult() With {
                .Success = True,
                .Damage = 15,
                .AoeRadius = 5.0,
                .Message = "Canary Cry unleashed!",
                .SpecialEffects = New List(Of String) From {"sonic_blast", "stun"}
            }
        End Function
    End Class
    
    ''' <summary>
    ''' Rene Ramirez / Wild Dog - Unorthodox Fighter
    ''' </summary>
    Public Class ReneRamirezActions
        Private _combatProfile As CombatProfile
        
        Public Sub New()
            _combatProfile = New CombatProfile() With {
                .CharacterID = CharacterID.ReneRamirez,
                .PrimaryStyle = CombatStyle.HandToHand,
                .SecondaryStyles = New List(Of CombatStyle) From {
                    CombatStyle.Tactical
                },
                .BaseDamage = 27,
                .AttackSpeed = 1.8,
                .CriticalChance = 0.18,
                .CriticalDamage = 2.1
            }
        End Sub
    End Class
    
    ''' <summary>
    ''' Dinah Drake / Black Canary II - Metahuman Fighter
    ''' </summary>
    Public Class DinahDrakeActions
        Private _combatProfile As CombatProfile
        
        Public Sub New()
            _combatProfile = New CombatProfile() With {
                .CharacterID = CharacterID.DinahDrake,
                .PrimaryStyle = CombatStyle.HandToHand,
                .SecondaryStyles = New List(Of CombatStyle) From {
                    CombatStyle.TechBased
                },
                .BaseDamage = 24,
                .AttackSpeed = 2.2,
                .CriticalChance = 0.22,
                .CriticalDamage = 2.15
            }
        End Sub
        
        Public Function MetahumanCry() As CombatResult
            Return New CombatResult() With {
                .Success = True,
                .Damage = 20,
                .AoeRadius = 6.0,
                .Message = "Metahuman Canary Cry!",
                .SpecialEffects = New List(Of String) From {"powerful_sonic", "knockback"}
            }
        End Function
    End Class
    
    ''' <summary>
    ''' Curtis Holt / Mister Terrific - Tech Genius
    ''' </summary>
    Public Class CurtisHoltActions
        Private _combatProfile As CombatProfile
        
        Public Sub New()
            _combatProfile = New CombatProfile() With {
                .CharacterID = CharacterID.CurtisHolt,
                .PrimaryStyle = CombatStyle.TechBased,
                .SecondaryStyles = New List(Of CombatStyle) From {
                    CombatStyle.Tactical
                },
                .BaseDamage = 18,
                .AttackSpeed = 1.5,
                .CriticalChance = 0.25,
                .CriticalDamage = 1.8
            }
        End Sub
        
        Public Function TSpheres() As CombatResult
            Return New CombatResult() With {
                .Success = True,
                .Damage = 10,
                .Message = "T-Spheres deployed!",
                .SpecialEffects = New List(Of String) From {"hologram", "stun", "surveillance"}
            }
        End Function
    End Class
    
    #End Region
    
    #Region "Crafting Recipe Database"
    
    Public Class CraftingDatabase
        Private _arrowRecipes As Dictionary(Of ArrowCraftingType, ArrowRecipe)
        Private _gadgetRecipes As Dictionary(Of GadgetType, GadgetRecipe)
        Private _materials As Dictionary(Of String, CraftingMaterial)
        
        Public Sub New()
            _arrowRecipes = New Dictionary(Of ArrowCraftingType, ArrowRecipe>
            _gadgetRecipes = New Dictionary(Of GadgetType, GadgetRecipe>
            _materials = New Dictionary(Of String, CraftingMaterial)
            
            InitializeMaterials()
            InitializeArrowRecipes()
            InitializeGadgetRecipes()
        End Sub
        
        Private Sub InitializeMaterials()
            ' Basic Materials
            _materials("Wood") = New CraftingMaterial() With {
                .MaterialID = "Wood",
                .Name = "Wood",
                .Description = "Basic wooden shafts for arrows",
                .Rarity = 1,
                .Source = "Found everywhere"
            }
            
            _materials("Aluminum") = New CraftingMaterial() With {
                .MaterialID = "Aluminum",
                .Name = "Aluminum",
                .Description = "Lightweight metal for arrow shafts",
                .Rarity = 2,
                .Source = "Industrial District"
            }
            
            _materials("CarbonFiber") = New CraftingMaterial() With {
                .MaterialID = "CarbonFiber",
                .Name = "Carbon Fiber",
                .Description = "High-tech material for precision arrows",
                .Rarity = 4,
                .Source = "Queen Consolidated R&D"
            }
            
            _materials("Steel") = New CraftingMaterial() With {
                .MaterialID = "Steel",
                .Name = "Steel",
                .Description = "Strong metal for arrowheads",
                .Rarity = 2,
                .Source = "Industrial District"
            }
            
            _materials("Titanium") = New CraftingMaterial() With {
                .MaterialID = "Titanium",
                .Name = "Titanium",
                .Description = "Ultra-strong lightweight metal",
                .Rarity = 5,
                .Source = "ARGUS facilities"
            }
            
            ' Explosive Materials
            _materials("Gunpowder") = New CraftingMaterial() With {
                .MaterialID = "Gunpowder",
                .Name = "Gunpowder",
                .Description = "Basic explosive compound",
                .Rarity = 3,
                .Source = "Criminal underworld"
            }
            
            _materials("C4") = New CraftingMaterial() With {
                .MaterialID = "C4",
                .Name = "C4 Plastic Explosive",
                .Description = "Military-grade explosive",
                .Rarity = 5,
                .Source = "Military surplus, ARGUS"
            }
            
            _materials("MercuryFulminate") = New CraftingMaterial() With {
                .MaterialID = "MercuryFulminate",
                .Name = "Mercury Fulminate",
                .Description = "Highly volatile primer compound",
                .Rarity = 4,
                .Source = "Chemical plant"
            }
            
            ' Chemical Materials
            _materials("Phosphorus") = New CraftingMaterial() With {
                .MaterialID = "Phosphorus",
                .Name = "White Phosphorus",
                .Description = "Creates intense smoke and fire",
                .Rarity = 4,
                .Source = "Chemical warehouse"
            }
            
            _materials("Sulfur") = New CraftingMaterial() With {
                .MaterialID = "Sulfur",
                .Name = "Sulfur",
                .Description = "Chemical compound for special effects",
                .Rarity = 2,
                .Source = "Industrial supply"
            }
            
            _materials("Potassium") = New CraftingMaterial() With {
                .MaterialID = "Potassium",
                .Name = "Potassium",
                .Description = "Reactive metal for flash effects",
                .Rarity = 3,
                .Source = "Science lab"
            }
            
            ' Electronic Materials
            _materials("CopperWire") = New CraftingMaterial() With {
                .MaterialID = "CopperWire",
                .Name = "Copper Wire",
                .Description = "For electrical components",
                .Rarity = 1,
                .Source = "Electronics store"
            }
            
            _materials("CircuitBoard") = New CraftingMaterial() With {
                .MaterialID = "CircuitBoard",
                .Name = "Circuit Board",
                .Description = "Basic electronic component",
                .Rarity = 2,
                .Source = "Tech store"
            }
            
            _materials("Microchip") = New CraftingMaterial() With {
                .MaterialID = "Microchip",
                .Name = "Microchip",
                .Description = "Advanced processor",
                .Rarity = 4,
                .Source = "Queen Consolidated Tech"
            }
            
            _materials("Battery") = New CraftingMaterial() With {
                .MaterialID = "Battery",
                .Name = "High-Voltage Battery",
                .Description = "Power source for electric arrows",
                .Rarity = 2,
                .Source = "Electronics store"
            }
            
            _materials("Capacitor") = New CraftingMaterial() With {
                .MaterialID = "Capacitor",
                .Name = "Capacitor",
                .Description = "Stores electrical charge",
                .Rarity = 3,
                .Source = "Tech lab"
            }
            
            ' Special Materials
            _materials("DragonBreath") = New CraftingMaterial() With {
                .MaterialID = "DragonBreath",
                .Name = "Dragon's Breath Powder",
                .Description = "Rare pyrotechnic compound",
                .Rarity = 5,
                .Source = "League of Assassins cache"
            }
            
            _materials("MirakuruSample") = New CraftingMaterial() With {
                .MaterialID = "MirakuruSample",
                .Name = "Mirakuru Sample",
                .Description = "Remnant of the super-soldier serum",
                .Rarity = 5,
                .Source = "Season 2 finale"
            }
            
            _materials("Nanites") = New CraftingMaterial() With {
                .MaterialID = "Nanites",
                .Name = "Nanites",
                .Description = "Microscopic robots for tech arrows",
                .Rarity = 5,
                .Source = "Palmer Technologies"
            }
        End Sub
        
        Private Sub InitializeArrowRecipes()
            ' Standard Arrow
            _arrowRecipes(ArrowCraftingType.Standard) = New ArrowRecipe() With {
                .ArrowType = ArrowCraftingType.Standard,
                .Name = "Standard Arrow",
                .Description = "Basic wooden arrow with steel tip",
                .RequiredLevel = SkillLevel.Novice,
                .CraftingTime = 1.0,
                .ResultCount = 5
            }
            _arrowRecipes(ArrowCraftingType.Standard).RequiredMaterials("Wood") = 1
            _arrowRecipes(ArrowCraftingType.Standard).RequiredMaterials("Steel") = 1
            
            ' Explosive Arrow
            _arrowRecipes(ArrowCraftingType.Explosive) = New ArrowRecipe() With {
                .ArrowType = ArrowCraftingType.Explosive,
                .Name = "Explosive Arrow",
                .Description = "Arrow that detonates on impact",
                .RequiredLevel = SkillLevel.Journeyman,
                .CraftingTime = 3.0,
                .ResultCount = 2,
                .RequiredEpisode = GetEpisode(2, 8)
            }
            _arrowRecipes(ArrowCraftingType.Explosive).RequiredMaterials("Aluminum") = 1
            _arrowRecipes(ArrowCraftingType.Explosive).RequiredMaterials("Gunpowder") = 2
            _arrowRecipes(ArrowCraftingType.Explosive).RequiredMaterials("Steel") = 1
            
            ' Incendiary Arrow
            _arrowRecipes(ArrowCraftingType.Incendiary) = New ArrowRecipe() With {
                .ArrowType = ArrowCraftingType.Incendiary,
                .Name = "Incendiary Arrow",
                .Description = "Sets targets on fire",
                .RequiredLevel = SkillLevel.Journeyman,
                .CraftingTime = 3.0,
                .ResultCount = 2,
                .RequiredEpisode = GetEpisode(2, 15)
            }
            _arrowRecipes(ArrowCraftingType.Incendiary).RequiredMaterials("Aluminum") = 1
            _arrowRecipes(ArrowCraftingType.Incendiary).RequiredMaterials("Phosphorus") = 1
            _arrowRecipes(ArrowCraftingType.Incendiary).RequiredMaterials("Sulfur") = 1
            
            ' Cryo Arrow
            _arrowRecipes(ArrowCraftingType.Cryo) = New ArrowRecipe() With {
                .ArrowType = ArrowCraftingType.Cryo,
                .Name = "Cryo Arrow",
                .Description = "Freezes targets on impact",
                .RequiredLevel = SkillLevel.Expert,
                .CraftingTime = 4.0,
                .ResultCount = 2,
                .RequiredEpisode = GetEpisode(3, 12)
            }
            _arrowRecipes(ArrowCraftingType.Cryo).RequiredMaterials("Aluminum") = 1
            _arrowRecipes(ArrowCraftingType.Cryo).RequiredMaterials("LiquidNitrogen") = 2
            
            ' Electric Arrow
            _arrowRecipes(ArrowCraftingType.Electric) = New ArrowRecipe() With {
                .ArrowType = ArrowCraftingType.Electric,
                .Name = "Electric Arrow",
                .Description = "Shocks targets with high voltage",
                .RequiredLevel = SkillLevel.Journeyman,
                .CraftingTime = 3.0,
                .ResultCount = 2,
                .RequiredEpisode = GetEpisode(3, 5)
            }
            _arrowRecipes(ArrowCraftingType.Electric).RequiredMaterials("Aluminum") = 1
            _arrowRecipes(ArrowCraftingType.Electric).RequiredMaterials("CopperWire") = 2
            _arrowRecipes(ArrowCraftingType.Electric).RequiredMaterials("Battery") = 1
            
            ' Sonic Arrow
            _arrowRecipes(ArrowCraftingType.Sonic) = New ArrowRecipe() With {
                .ArrowType = ArrowCraftingType.Sonic,
                .Name = "Sonic Arrow",
                .Description = "Emits high-frequency sound waves",
                .RequiredLevel = SkillLevel.Expert,
                .CraftingTime = 4.0,
                .ResultCount = 1,
                .RequiredEpisode = GetEpisode(4, 9)
            }
            _arrowRecipes(ArrowCraftingType.Sonic).RequiredMaterials("Aluminum") = 1
            _arrowRecipes(ArrowCraftingType.Sonic).RequiredMaterials("CircuitBoard") = 2
            _arrowRecipes(ArrowCraftingType.Sonic).RequiredMaterials("Microchip") = 1
            
            ' Smoke Arrow
            _arrowRecipes(ArrowCraftingType.Smoke) = New ArrowRecipe() With {
                .ArrowType = ArrowCraftingType.Smoke,
                .Name = "Smoke Arrow",
                .Description = "Creates thick smoke screen",
                .RequiredLevel = SkillLevel.Apprentice,
                .CraftingTime = 2.0,
                .ResultCount = 3,
                .RequiredEpisode = GetEpisode(1, 15)
            }
            _arrowRecipes(ArrowCraftingType.Smoke).RequiredMaterials("Aluminum") = 1
            _arrowRecipes(ArrowCraftingType.Smoke).RequiredMaterials("Phosphorus") = 1
            
            ' Flashbang Arrow
            _arrowRecipes(ArrowCraftingType.Flashbang) = New ArrowRecipe() With {
                .ArrowType = ArrowCraftingType.Flashbang,
                .Name = "Flashbang Arrow",
                .Description = "Blinds and disorients enemies",
                .RequiredLevel = SkillLevel.Journeyman,
                .CraftingTime = 3.0,
                .ResultCount = 2,
                .RequiredEpisode = GetEpisode(2, 12)
            }
            _arrowRecipes(ArrowCraftingType.Flashbang).RequiredMaterials("Aluminum") = 1
            _arrowRecipes(ArrowCraftingType.Flashbang).RequiredMaterials("Potassium") = 1
            _arrowRecipes(ArrowCraftingType.Flashbang).RequiredMaterials("Magnesium") = 1
            
            ' Grappling Arrow
            _arrowRecipes(ArrowCraftingType.Grappling) = New ArrowRecipe() With {
                .ArrowType = ArrowCraftingType.Grappling,
                .Name = "Grappling Arrow",
                .Description = "Arrow with reinforced cable for swinging",
                .RequiredLevel = SkillLevel.Apprentice,
                .CraftingTime = 3.0,
                .ResultCount = 1,
                .RequiredEpisode = GetEpisode(1, 3)
            }
            _arrowRecipes(ArrowCraftingType.Grappling).RequiredMaterials("Steel") = 2
            _arrowRecipes(ArrowCraftingType.Grappling).RequiredMaterials("KevlarCable") = 1
            
            ' Net Arrow
            _arrowRecipes(ArrowCraftingType.Net) = New ArrowRecipe() With {
                .ArrowType = ArrowCraftingType.Net,
                .Name = "Net Arrow",
                .Description = "Deploys entangling net",
                .RequiredLevel = SkillLevel.Journeyman,
                .CraftingTime = 3.0,
                .ResultCount = 1,
                .RequiredEpisode = GetEpisode(3, 8)
            }
            _arrowRecipes(ArrowCraftingType.Net).RequiredMaterials("Aluminum") = 1
            _arrowRecipes(ArrowCraftingType.Net).RequiredMaterials("KevlarThread") = 3
            
            ' Boxing Glove Arrow
            _arrowRecipes(ArrowCraftingType.BoxingGlove) = New ArrowRecipe() With {
                .ArrowType = ArrowCraftingType.BoxingGlove,
                .Name = "Boxing Glove Arrow",
                .Description = "Classic non-lethal takedown",
                .RequiredLevel = SkillLevel.Novice,
                .CraftingTime = 2.0,
                .ResultCount = 1,
                .RequiredEpisode = GetEpisode(1, 1)
            }
            _arrowRecipes(ArrowCraftingType.BoxingGlove).RequiredMaterials("Wood") = 1
            _arrowRecipes(ArrowCraftingType.BoxingGlove).RequiredMaterials("Leather") = 1
            _arrowRecipes(ArrowCraftingType.BoxingGlove).RequiredMaterials("Foam") = 1
            
            ' USB Arrow
            _arrowRecipes(ArrowCraftingType.USB) = New ArrowRecipe() With {
                .ArrowType = ArrowCraftingType.USB,
                .Name = "USB Arrow",
                .Description = "Arrow with data storage for hacking",
                .RequiredLevel = SkillLevel.Expert,
                .CraftingTime = 4.0,
                .ResultCount = 1,
                .RequiredEpisode = GetEpisode(2, 5)
            }
            _arrowRecipes(ArrowCraftingType.USB).RequiredMaterials("Aluminum") = 1
            _arrowRecipes(ArrowCraftingType.USB).RequiredMaterials("USBDrive") = 1
            _arrowRecipes(ArrowCraftingType.USB).RequiredMaterials("Microchip") = 1
            
            ' Tracker Arrow
            _arrowRecipes(ArrowCraftingType.Tracker) = New ArrowRecipe() With {
                .ArrowType = ArrowCraftingType.Tracker,
                .Name = "Tracker Arrow",
                .Description = "Arrow with GPS tracking device",
                .RequiredLevel = SkillLevel.Journeyman,
                .CraftingTime = 3.0,
                .ResultCount = 1,
                .RequiredEpisode = GetEpisode(2, 18)
            }
            _arrowRecipes(ArrowCraftingType.Tracker).RequiredMaterials("Aluminum") = 1
            _arrowRecipes(ArrowCraftingType.Tracker).RequiredMaterials("GPSChip") = 1
            _arrowRecipes(ArrowCraftingType.Tracker).RequiredMaterials("Battery") = 1
            
            ' EMP Arrow
            _arrowRecipes(ArrowCraftingType.EMP) = New ArrowRecipe() With {
                .ArrowType = ArrowCraftingType.EMP,
                .Name = "EMP Arrow",
                .Description = "Disables electronic devices",
                .RequiredLevel = SkillLevel.Master,
                .CraftingTime = 5.0,
                .ResultCount = 1,
                .RequiredEpisode = GetEpisode(4, 15)
            }
            _arrowRecipes(ArrowCraftingType.EMP).RequiredMaterials("Aluminum") = 1
            _arrowRecipes(ArrowCraftingType.EMP).RequiredMaterials("Capacitor") = 3
            _arrowRecipes(ArrowCraftingType.EMP).RequiredMaterials("CopperWire") = 3
            _arrowRecipes(ArrowCraftingType.EMP).RequiredMaterials("Microchip") = 2
            
            ' Nerve Arrow
            _arrowRecipes(ArrowCraftingType.Nerve) = New ArrowRecipe() With {
                .ArrowType = ArrowCraftingType.Nerve,
                .Name = "Nerve Arrow",
                .Description = "Delivers neurotoxin for non-lethal takedown",
                .RequiredLevel = SkillLevel.Master,
                .CraftingTime = 5.0,
                .ResultCount = 1,
                .RequiredEpisode = GetEpisode(5, 12)
            }
            _arrowRecipes(ArrowCraftingType.Nerve).RequiredMaterials("Aluminum") = 1
            _arrowRecipes(ArrowCraftingType.Nerve).RequiredMaterials("Neurotoxin") = 1
            _arrowRecipes(ArrowCraftingType.Nerve).RequiredMaterials("MicroSyringe") = 1
            
            ' Dragonfire Arrow
            _arrowRecipes(ArrowCraftingType.Dragonfire) = New ArrowRecipe() With {
                .ArrowType = ArrowCraftingType.Dragonfire,
                .Name = "Dragonfire Arrow",
                .Description = "Legendary arrow from League of Assassins",
                .RequiredLevel = SkillLevel.Legendary,
                .CraftingTime = 10.0,
                .ResultCount = 1,
                .RequiredEpisode = GetEpisode(3, 20),
                .CharacterSpecific = CharacterID.OliverQueen
            }
            _arrowRecipes(ArrowCraftingType.Dragonfire).RequiredMaterials("DragonBreath") = 2
            _arrowRecipes(ArrowCraftingType.Dragonfire).RequiredMaterials("CarbonFiber") = 2
            _arrowRecipes(ArrowCraftingType.Dragonfire).RequiredMaterials("Titanium") = 1
        End Sub
        
        Private Sub InitializeGadgetRecipes()
            _gadgetRecipes(GadgetType.Flashbang) = New GadgetRecipe() With {
                .GadgetType = GadgetType.Flashbang,
                .Name = "Flashbang Grenade",
                .Description = "Temporary blinding device",
                .RequiredLevel = SkillLevel.Journeyman,
                .CraftingTime = 2.0,
                .ResultCount = 2,
                .TechLevel = 2,
                .CreatedBy = CharacterID.CurtisHolt
            }
            _gadgetRecipes(GadgetType.Flashbang).RequiredMaterials("Potassium") = 1
            _gadgetRecipes(GadgetType.Flashbang).RequiredMaterials("Magnesium") = 1
            
            _gadgetRecipes(GadgetType.SmokePellet) = New GadgetRecipe() With {
                .GadgetType = GadgetType.SmokePellet,
                .Name = "Smoke Pellet",
                .Description = "Quick smoke screen",
                .RequiredLevel = SkillLevel.Apprentice,
                .CraftingTime = 1.0,
                .ResultCount = 5,
                .TechLevel = 1
            }
            _gadgetRecipes(GadgetType.SmokePellet).RequiredMaterials("Phosphorus") = 1
            
            _gadgetRecipes(GadgetType.ExplosiveCharge) = New GadgetRecipe() With {
                .GadgetType = GadgetType.ExplosiveCharge,
                .Name = "Explosive Charge",
                .Description = "Remote-detonated explosive",
                .RequiredLevel = SkillLevel.Expert,
                .CraftingTime = 4.0,
                .ResultCount = 1,
                .TechLevel = 3,
                .CreatedBy = CharacterID.JohnDiggle
            }
            _gadgetRecipes(GadgetType.ExplosiveCharge).RequiredMaterials("C4") = 2
            _gadgetRecipes(GadgetType.ExplosiveCharge).RequiredMaterials("Detonator") = 1
            
            _gadgetRecipes(GadgetType.HackingDevice) = New GadgetRecipe() With {
                .GadgetType = GadgetType.HackingDevice,
                .Name = "Hacking Device",
                .Description = "Bypasses electronic security",
                .RequiredLevel = SkillLevel.Expert,
                .CraftingTime = 3.0,
                .ResultCount = 1,
                .TechLevel = 4,
                .CreatedBy = CharacterID.FelicitySmoak
            }
            _gadgetRecipes(GadgetType.HackingDevice).RequiredMaterials("CircuitBoard") = 2
            _gadgetRecipes(GadgetType.HackingDevice).RequiredMaterials("Microchip") = 2
            _gadgetRecipes(GadgetType.HackingDevice).RequiredMaterials("CopperWire") = 3
        End Sub
        
        Private Function GetEpisode(season As Integer, episodeNum As Integer) As Episode
            ' This would return the actual episode object
            Return New Episode(season, episodeNum, "", "")
        End Function
        
        Public Function GetArrowRecipe(arrowType As ArrowCraftingType) As ArrowRecipe
            If _arrowRecipes.ContainsKey(arrowType) Then
                Return _arrowRecipes(arrowType)
            End If
            Return Nothing
        End Function
        
        Public Function GetAllArrowRecipes() As List(Of ArrowRecipe)
            Return _arrowRecipes.Values.ToList()
        End Function
        
        Public Function GetAvailableRecipes(skillLevel As SkillLevel, 
                                           episode As Episode,
                                           character As CharacterID) As List(Of ArrowRecipe)
            Return _arrowRecipes.Values.Where(Function(r) 
                Return r.RequiredLevel <= skillLevel AndAlso
                       (r.RequiredEpisode Is Nothing OrElse 
                        r.RequiredEpisode.EpisodeNumber <= episode.EpisodeNumber) AndAlso
                       (r.CharacterSpecific Is Nothing OrElse
                        r.CharacterSpecific = character)
            End Function).ToList()
        End Function
    End Class
    
    #End Region
    
    #Region "Combat System Manager"
    
    Public Class CombatManager
        Private _currentEncounter As CombatEncounter
        Private _playerCharacters As Dictionary(Of CharacterID, CombatProfile)
        Private _enemyCharacters As Dictionary(Of Integer, CombatEnemy)
        Private _combatLog As List(Of String)
        Private _turnOrder As List(Of Integer)
        Private _currentTurn As Integer = 0
        Private _combatTimer As Double = 0
        Private _isInCombat As Boolean = False
        Private _difficulty As String = "Normal"
        
        Public Event CombatStarted As EventHandler
        Public Event CombatEnded As EventHandler(Of CombatResultEventArgs)
        Public Event TurnChanged As EventHandler(Of Integer)
        Public Event EnemyDefeated As EventHandler(Of CharacterID)
        
        Public Sub New()
            _playerCharacters = New Dictionary(Of CharacterID, CombatProfile)
            _enemyCharacters = New Dictionary(Of Integer, CombatEnemy>
            _combatLog = New List(Of String)
        End Sub
        
        Public Sub StartCombat(encounter As CombatEncounter)
            _currentEncounter = encounter
            _isInCombat = True
            _combatTimer = 0
            _combatLog.Clear()
            
            ' Initialize enemies
            Dim enemyIndex As Integer = 0
            For Each enemy In encounter.Enemies
                _enemyCharacters.Add(enemyIndex, enemy)
                enemyIndex += 1
            Next
            
            ' Determine turn order based on speed
            CalculateTurnOrder()
            
            _combatLog.Add($"Combat started with {_enemyCharacters.Count} enemies")
            RaiseEvent CombatStarted(Me, EventArgs.Empty)
        End Sub
        
        Private Sub CalculateTurnOrder()
            ' Simple turn order - players go first, then enemies
            _turnOrder.Clear()
            
            ' Add players
            For i = 0 To _playerCharacters.Count - 1
                _turnOrder.Add(i)
            Next
            
            ' Add enemies
            For i = 0 To _enemyCharacters.Count - 1
                _turnOrder.Add(i + 100) ' Offset for enemies
            Next
        End Sub
        
        Public Function PlayerAttack(playerId As CharacterID, targetIndex As Integer, 
                                     moveName As String) As CombatResult
            If Not _isInCombat OrElse Not _playerCharacters.ContainsKey(playerId) Then
                Return New CombatResult() With {.Success = False}
            End If
            
            If targetIndex >= _enemyCharacters.Count Then
                Return New CombatResult() With {.Success = False, .Message = "Invalid target"}
            End If
            
            Dim player = _playerCharacters(playerId)
            Dim enemy = _enemyCharacters(targetIndex)
            Dim result As New CombatResult()
            
            ' Calculate hit chance
            Dim hitChance = 0.9 ' Base hit chance
            If Rnd() < hitChance Then
                ' Calculate damage
                Dim baseDamage = player.BaseDamage
                Dim isCritical = Rnd() < player.CriticalChance
                Dim damage = baseDamage * If(isCritical, player.CriticalDamage, 1.0)
                
                ' Apply enemy weaknesses/resistances
                If enemy.Weaknesses.Contains(ArrowCraftingType.Standard) Then
                    damage *= 1.5
                End If
                
                enemy.Health -= CInt(damage)
                
                result.Success = True
                result.Damage = CInt(damage)
                result.IsCritical = isCritical
                result.Message = $"Hit enemy for {CInt(damage)} damage!"
                
                _combatLog.Add(result.Message)
                
                ' Check if enemy defeated
                If enemy.Health <= 0 Then
                    enemy.Health = 0
                    result.Message &= " Enemy defeated!"
                    RaiseEvent EnemyDefeated(Me, enemy.CharacterID)
                    
                    ' Remove defeated enemy
                    _enemyCharacters.Remove(targetIndex)
                    
                    ' Check if combat ends
                    If _enemyCharacters.Count = 0 Then
                        EndCombat(True)
                    End If
                End If
            Else
                result.Success = False
                result.Message = "Attack missed!"
                _combatLog.Add(result.Message)
            End If
            
            Return result
        End Function
        
        Public Function UseSpecialMove(playerId As CharacterID, moveName As String) As CombatResult
            ' Implementation would call character-specific special moves
            Return New CombatResult() With {.Success = False}
        End Function
        
        Public Sub EndCombat(victory As Boolean)
            _isInCombat = False
            
            Dim args As New CombatResultEventArgs() With {
                .Victory = victory,
                .CombatLog = _combatLog
            }
            
            If victory AndAlso _currentEncounter IsNot Nothing Then
                ' Calculate rewards
                args.Rewards = _currentEncounter.Rewards
            End If
            
            RaiseEvent CombatEnded(Me, args)
        End Sub
        
        Public Function GetCombatStatus() As CombatStatus
            Return New CombatStatus() With {
                .EnemiesRemaining = _enemyCharacters.Count,
                .CurrentTurn = _currentTurn,
                .CombatTime = _combatTimer,
                .CombatLog = _combatLog.ToList()
            }
        End Function
    End Class
    
    Public Class CombatResult
        Public Property Success As Boolean
        Public Property Damage As Integer
        Public Property IsCritical As Boolean
        Public Property Message As String
        Public Property AoeRadius As Double = 0
        Public Property SpecialEffects As List(Of String)
        Public Property StatusEffects As List(Of String)
        
        Public Sub New()
            SpecialEffects = New List(Of String)
            StatusEffects = New List(Of String)
        End Sub
    End Class
    
    Public Class CombatStatus
        Public Property EnemiesRemaining As Integer
        Public Property CurrentTurn As Integer
        Public Property CombatTime As Double
        Public Property CombatLog As List(Of String)
    End Class
    
    Public Class CombatResultEventArgs
        Inherits EventArgs
        
        Public Property Victory As Boolean
        Public Property Rewards As CombatRewards
        Public Property CombatLog As List(Of String)
    End Class
    
    Public Class CombatContext
        Public Property BaseDamage As Integer
        Public Property TargetCount As Integer
        Public Property Environment As String
        Public Property TimeOfDay As Double
    End Class
    
    #End Region
    
    #Region "Crafting System Manager"
    
    Public Class CraftingManager
        Private _database As CraftingDatabase
        Private _inventory As Dictionary(Of String, Integer)
        Private _arrowInventory As Dictionary(Of ArrowCraftingType, Integer)
        Private _gadgetInventory As Dictionary(Of GadgetType, Integer)
        Private _unlockedRecipes As List(Of ArrowCraftingType)
        Private _craftingQueue As Queue(Of CraftingJob)
        Private _isCrafting As Boolean = False
        Private _currentCraftingJob As CraftingJob
        
        Public Event CraftingStarted As EventHandler(Of CraftingJob)
        Public Event CraftingCompleted As EventHandler(Of CraftingJob)
        Public Event CraftingFailed As EventHandler(Of String)
        Public Event MaterialsUpdated As EventHandler
        
        Public Sub New()
            _database = New CraftingDatabase()
            _inventory = New Dictionary(Of String, Integer)
            _arrowInventory = New Dictionary(Of ArrowCraftingType, Integer)
            _gadgetInventory = New Dictionary(Of GadgetType, Integer)
            _unlockedRecipes = New List(Of ArrowCraftingType)
            _craftingQueue = New Queue(Of CraftingJob)
            
            InitializeStartingInventory()
        End Sub
        
        Private Sub InitializeStartingInventory()
            ' Give player some starting materials
            _inventory("Wood") = 20
            _inventory("Steel") = 15
            _inventory("Aluminum") = 10
            _inventory("CopperWire") = 10
            
            ' Starting arrows
            _arrowInventory(ArrowCraftingType.Standard) = 20
            _arrowInventory(ArrowCraftingType.BoxingGlove) = 5
            
            ' Unlock basic recipes
            _unlockedRecipes.Add(ArrowCraftingType.Standard)
            _unlockedRecipes.Add(ArrowCraftingType.BoxingGlove)
            _unlockedRecipes.Add(ArrowCraftingType.Smoke)
        End Sub
        
        Public Function CanCraft(arrowType As ArrowCraftingType, 
                                 characterLevel As SkillLevel,
                                 currentEpisode As Episode,
                                 characterId As CharacterID) As Boolean
            Dim recipe = _database.GetArrowRecipe(arrowType)
            If recipe Is Nothing Then Return False
            
            ' Check if unlocked
            If Not _unlockedRecipes.Contains(arrowType) Then Return False
            
            ' Check level requirement
            If recipe.RequiredLevel > characterLevel Then Return False
            
            ' Check episode requirement
            If recipe.RequiredEpisode IsNot Nothing AndAlso
               recipe.RequiredEpisode.EpisodeNumber > currentEpisode.EpisodeNumber Then
                Return False
            End If
            
            ' Check character specific
            If recipe.CharacterSpecific.HasValue AndAlso
               recipe.CharacterSpecific.Value <> characterId Then
                Return False
            End If
            
            ' Check materials
            Return recipe.CanCraft(_inventory)
        End Function
        
        Public Function CraftArrow(arrowType As ArrowCraftingType, 
                                   quantity As Integer,
                                   station As CraftingStation) As Boolean
            Dim recipe = _database.GetArrowRecipe(arrowType)
            If recipe Is Nothing Then Return False
            
            ' Check if allowed at this station
            If Not station.AllowedRecipes.Contains(arrowType) Then
                RaiseEvent CraftingFailed(Me, "Recipe not allowed at this station")
                Return False
            End If
            
            ' Check if we have enough materials for the quantity
            For Each material In recipe.RequiredMaterials
                Dim required = material.Value * quantity
                If Not _inventory.ContainsKey(material.Key) OrElse
                   _inventory(material.Key) < required Then
                    RaiseEvent CraftingFailed(Me, $"Insufficient {material.Key}")
                    Return False
                End If
            Next
            
            ' Create crafting job
            Dim job As New CraftingJob() With {
                .JobID = Guid.NewGuid().ToString(),
                .ArrowType = arrowType,
                .Quantity = quantity,
                .Recipe = recipe,
                .TotalTime = recipe.CraftingTime * quantity / station.CraftingSpeed,
                .TimeRemaining = recipe.CraftingTime * quantity / station.CraftingSpeed,
                .Station = station,
                .Status = "Queued"
            }
            
            _craftingQueue.Enqueue(job)
            
            If Not _isCrafting Then
                StartNextCraftingJob()
            End If
            
            Return True
        End Function
        
        Private Sub StartNextCraftingJob()
            If _craftingQueue.Count = 0 Then
                _isCrafting = False
                Return
            End If
            
            _currentCraftingJob = _craftingQueue.Dequeue()
            _currentCraftingJob.Status = "In Progress"
            _isCrafting = True
            
            ' Consume materials
            For Each material In _currentCraftingJob.Recipe.RequiredMaterials
                Dim required = material.Value * _currentCraftingJob.Quantity
                _inventory(material.Key) -= required
            Next
            
            RaiseEvent CraftingStarted(Me, _currentCraftingJob)
        End Sub
        
        Public Sub UpdateCrafting(deltaTime As Double)
            If Not _isCrafting OrElse _currentCraftingJob Is Nothing Then Return
            
            _currentCraftingJob.TimeRemaining -= deltaTime
            
            If _currentCraftingJob.TimeRemaining <= 0 Then
                CompleteCraftingJob()
            End If
        End Sub
        
        Private Sub CompleteCraftingJob()
            ' Add crafted items to inventory
            Dim resultCount = _currentCraftingJob.Recipe.ResultCount * _currentCraftingJob.Quantity
            
            If _arrowInventory.ContainsKey(_currentCraftingJob.ArrowType) Then
                _arrowInventory(_currentCraftingJob.ArrowType) += resultCount
            Else
                _arrowInventory(_currentCraftingJob.ArrowType) = resultCount
            End If
            
            _currentCraftingJob.Status = "Completed"
            RaiseEvent CraftingCompleted(Me, _currentCraftingJob)
            
            ' Start next job
            StartNextCraftingJob()
        End Sub
        
        Public Function GetMaterialCount(materialId As String) As Integer
            If _inventory.ContainsKey(materialId) Then
                Return _inventory(materialId)
            End If
            Return 0
        End Function
        
        Public Function GetArrowCount(arrowType As ArrowCraftingType) As Integer
            If _arrowInventory.ContainsKey(arrowType) Then
                Return _arrowInventory(arrowType)
            End If
            Return 0
        End Function
        
        Public Sub AddMaterials(materialId As String, quantity As Integer)
            If _inventory.ContainsKey(materialId) Then
                _inventory(materialId) += quantity
            Else
                _inventory(materialId) = quantity
            End If
            
            RaiseEvent MaterialsUpdated(Me, EventArgs.Empty)
        End Sub
        
        Public Sub UseArrow(arrowType As ArrowCraftingType)
            If _arrowInventory.ContainsKey(arrowType) AndAlso
               _arrowInventory(arrowType) > 0 Then
                _arrowInventory(arrowType) -= 1
            End If
        End Sub
        
        Public Sub UnlockRecipe(arrowType As ArrowCraftingType)
            If Not _unlockedRecipes.Contains(arrowType) Then
                _unlockedRecipes.Add(arrowType)
            End If
        End Sub
    End Class
    
    Public Class CraftingJob
        Public Property JobID As String
        Public Property ArrowType As ArrowCraftingType
        Public Property Quantity As Integer
        Public Property Recipe As ArrowRecipe
        Public Property TotalTime As Double
        Public Property TimeRemaining As Double
        Public Property Station As CraftingStation
        Public Property Status As String
    End Class
    
    #End Region
    
    #Region "Action UI Forms"
    
    Public Class CombatForm
        Inherits Form
        
        Private _combatManager As CombatManager
        Private _gameEngine As ArrowSagaEngine
        
        Private WithEvents timer As New Timer()
        Private pbEnemyHealth As ProgressBar
        Private pbPlayerHealth As ProgressBar
        Private lblComboCounter As Label
        Private lstCombatLog As ListBox
        Private pnlEnemyInfo As Panel
        Private btnAttack As Button
        Private btnSpecial1 As Button
        Private btnSpecial2 As Button
        Private btnSpecial3 As Button
        Private btnUseItem As Button
        Private btnFlee As Button
        
        Public Sub New(engine As ArrowSagaEngine, encounter As CombatEncounter)
            _gameEngine = engine
            _combatManager = New CombatManager()
            
            InitializeComponent()
            SetupCombat(encounter)
        End Sub
        
        Private Sub InitializeComponent()
            Me.Text = "COMBAT MODE"
            Me.Size = New Size(1024, 768)
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.BackColor = Color.FromArgb(30, 30, 30)
            Me.FormBorderStyle = FormBorderStyle.None
            Me.TopMost = True
            
            ' Enemy Info Panel
            pnlEnemyInfo = New Panel() With {
                .Location = New Point(262, 20),
                .Size = New Size(500, 200),
                .BackColor = Color.FromArgb(50, 0, 0)
            }
            
            pbEnemyHealth = New ProgressBar() With {
                .Location = New Point(10, 150),
                .Size = New Size(480, 20),
                .Style = ProgressBarStyle.Continuous,
                .ForeColor = Color.Red
            }
            
            ' Player Health
            pbPlayerHealth = New ProgressBar() With {
                .Location = New Point(20, 650),
                .Size = New Size(400, 20),
                .Style = ProgressBarStyle.Continuous,
                .ForeColor = Color.Green
            }
            
            ' Combo Counter
            lblComboCounter = New Label() With {
                .Location = New Point(450, 650),
                .Size = New Size(100, 30),
                .ForeColor = Color.Yellow,
                .Font = New Font("Arial", 16, FontStyle.Bold),
                .Text = "COMBO x0"
            }
            
            ' Combat Log
            lstCombatLog = New ListBox() With {
                .Location = New Point(20, 500),
                .Size = New Size(400, 140),
                .BackColor = Color.Black,
                .ForeColor = Color.White,
                .Font = New Font("Courier New", 10)
            }
            
            ' Action Buttons
            btnAttack = CreateActionButton("ATTACK", New Point(700, 500), Color.Red)
            btnSpecial1 = CreateActionButton("TRIPLE SHOT", New Point(700, 550), Color.Orange)
            btnSpecial2 = CreateActionButton("TRICK SHOT", New Point(700, 600), Color.Orange)
            btnSpecial3 = CreateActionButton("PRECISION", New Point(700, 650), Color.Orange)
            btnUseItem = CreateActionButton("USE ITEM", New Point(820, 500), Color.Blue)
            btnFlee = CreateActionButton("FLEE", New Point(820, 600), Color.Gray)
            
            pnlEnemyInfo.Controls.Add(pbEnemyHealth)
            
            Me.Controls.AddRange(New Control() {
                pnlEnemyInfo, pbPlayerHealth, lblComboCounter,
                lstCombatLog, btnAttack, btnSpecial1, btnSpecial2,
                btnSpecial3, btnUseItem, btnFlee
            })
            
            timer.Interval = 100
            timer.Enabled = True
        End Sub
        
        Private Function CreateActionButton(text As String, location As Point, color As Color) As Button
            Return New Button() With {
                .Text = text,
                .Location = location,
                .Size = New Size(100, 40),
                .BackColor = color,
                .ForeColor = Color.White,
                .Font = New Font("Arial", 10, FontStyle.Bold),
                .FlatStyle = FlatStyle.Flat
            }
        End Function
        
        Private Sub SetupCombat(encounter As CombatEncounter)
            _combatManager.StartCombat(encounter)
            
            ' Setup enemy display
            If encounter.Enemies.Count > 0 Then
                Dim enemy = encounter.Enemies(0)
                pbEnemyHealth.Maximum = enemy.MaxHealth
                pbEnemyHealth.Value = enemy.Health
                
                ' Draw enemy image/name
                Using g = pnlEnemyInfo.CreateGraphics()
                    g.DrawString(enemy.CharacterID.ToString(), 
                               New Font("Arial", 20, FontStyle.Bold),
                               Brushes.White, 10, 10)
                End Using
            End If
            
            pbPlayerHealth.Maximum = 100
            pbPlayerHealth.Value = 100
        End Sub
        
        Private Sub timer_Tick(sender As Object, e As EventArgs) Handles timer.Tick
            ' Update UI
            If _combatManager IsNot Nothing Then
                Dim status = _combatManager.GetCombatStatus()
                
                ' Update combo counter
                lblComboCounter.Text = $"COMBO x{status.EnemiesRemaining}"
                
                ' Update combat log
                If status.CombatLog.Count > 0 Then
                    lstCombatLog.Items.Clear()
                    For Each log In status.CombatLog.TakeLast(5)
                        lstCombatLog.Items.Add(log)
                    Next
                End If
            End If
        End Sub
        
        Private Sub btnAttack_Click(sender As Object, e As EventArgs) Handles btnAttack.Click
            Dim result = _combatManager.PlayerAttack(
                CharacterID.OliverQueen, 0, "Basic Attack")
            
            If result.Success Then
                pbEnemyHealth.Value -= result.Damage
                
                If pbEnemyHealth.Value <= 0 Then
                    MessageBox.Show("Enemy Defeated!", "Victory!")
                    Me.Close()
                End If
            End If
        End Sub
        
        Private Sub btnFlee_Click(sender As Object, e As EventArgs) Handles btnFlee.Click
            If MessageBox.Show("Flee from combat?", "Flee", 
                              MessageBoxButtons.YesNo) = DialogResult.Yes Then
                Me.Close()
            End If
        End Sub
        
        Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
            If keyData = Keys.Escape Then
                btnFlee_Click(Nothing, Nothing)
                Return True
            End If
            Return MyBase.ProcessCmdKey(msg, keyData)
        End Function
    End Class
    
    Public Class CraftingForm
        Inherits Form
        
        Private _craftingManager As CraftingManager
        Private _currentStation As CraftingStation
        Private _characterLevel As SkillLevel
        Private _currentEpisode As Episode
        Private _characterId As CharacterID
        
        Private WithEvents lstRecipes As ListBox
        Private WithEvents btnCraft As Button
        Private txtRecipeInfo As TextBox
        Private lstMaterials As ListBox
        Private lstInventory As ListBox
        Private numQuantity As NumericUpDown
        Private pbCraftingProgress As ProgressBar
        Private WithEvents timer As New Timer()
        
        Public Sub New(manager As CraftingManager, station As CraftingStation,
                      level As SkillLevel, episode As Episode, character As CharacterID)
            _craftingManager = manager
            _currentStation = station
            _characterLevel = level
            _currentEpisode = episode
            _characterId = character
            
            InitializeComponent()
            LoadRecipes()
            LoadInventory()
        End Sub
        
        Private Sub InitializeComponent()
            Me.Text = $"Crafting Station - {_currentStation.Name}"
            Me.Size = New Size(1024, 768)
            Me.StartPosition = FormStartPosition.CenterParent
            Me.BackColor = Color.FromArgb(40, 40, 40)
            
            ' Recipe List
            lstRecipes = New ListBox() With {
                .Location = New Point(20, 20),
                .Size = New Size(300, 400),
                .BackColor = Color.FromArgb(50, 50, 50),
                .ForeColor = Color.White,
                .Font = New Font("Arial", 10)
            }
            
            ' Recipe Info
            txtRecipeInfo = New TextBox() With {
                .Location = New Point(340, 20),
                .Size = New Size(300, 150),
                .Multiline = True,
                .ReadOnly = True,
                .BackColor = Color.FromArgb(50, 50, 50),
                .ForeColor = Color.White,
                .Font = New Font("Arial", 10)
            }
            
            ' Materials List
            lstMaterials = New ListBox() With {
                .Location = New Point(340, 180),
                .Size = New Size(300, 200),
                .BackColor = Color.FromArgb(50, 50, 50),
                .ForeColor = Color.White,
                .Font = New Font("Arial", 10)
            }
            
            ' Inventory List
            lstInventory = New ListBox() With {
                .Location = New Point(660, 20),
                .Size = New Size(300, 400),
                .BackColor = Color.FromArgb(50, 50, 50),
                .ForeColor = Color.White,
                .Font = New Font("Arial", 10)
            }
            
            ' Quantity Selector
            numQuantity = New NumericUpDown() With {
                .Location = New Point(340, 390),
                .Size = New Size(100, 30),
                .Minimum = 1,
                .Maximum = 10,
                .Value = 1,
                .BackColor = Color.FromArgb(50, 50, 50),
                .ForeColor = Color.White
            }
            
            ' Craft Button
            btnCraft = New Button() With {
                .Text = "CRAFT",
                .Location = New Point(460, 390),
                .Size = New Size(180, 30),
                .BackColor = Color.Green,
                .ForeColor = Color.White,
                .Font = New Font("Arial", 12, FontStyle.Bold)
            }
            
            ' Progress Bar
            pbCraftingProgress = New ProgressBar() With {
                .Location = New Point(340, 430),
                .Size = New Size(300, 20),
                .Style = ProgressBarStyle.Continuous
            }
            
            Me.Controls.AddRange(New Control() {
                lstRecipes, txtRecipeInfo, lstMaterials, lstInventory,
                numQuantity, btnCraft, pbCraftingProgress
            })
            
            timer.Interval = 100
            timer.Enabled = True
        End Sub
        
        Private Sub LoadRecipes()
            lstRecipes.Items.Clear()
            Dim recipes = _craftingManager.GetAvailableRecipes(
                _characterLevel, _currentEpisode, _characterId)
            
            For Each recipe In recipes
                Dim canCraft = _craftingManager.CanCraft(
                    recipe.ArrowType, _characterLevel, 
                    _currentEpisode, _characterId)
                Dim status = If(canCraft, "✓", "✗")
                lstRecipes.Items.Add($"{status} {recipe.Name}")
            Next
        End Sub
        
        Private Sub LoadInventory()
            lstInventory.Items.Clear()
            lstInventory.Items.Add("--- MATERIALS ---")
            
            ' Show materials
            For Each material In {"Wood", "Steel", "Aluminum", "CarbonFiber",
                                   "Gunpowder", "CopperWire", "Battery", "Microchip"}
                Dim count = _craftingManager.GetMaterialCount(material)
                lstInventory.Items.Add($"{material}: {count}")
            Next
            
            lstInventory.Items.Add("")
            lstInventory.Items.Add("--- ARROWS ---")
            
            ' Show arrows
            For Each arrow In [Enum].GetValues(GetType(ArrowCraftingType))
                Dim count = _craftingManager.GetArrowCount(CType(arrow, ArrowCraftingType))
                If count > 0 Then
                    lstInventory.Items.Add($"{arrow}: {count}")
                End If
            Next
        End Sub
        
        Private Sub lstRecipes_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstRecipes.SelectedIndexChanged
            If lstRecipes.SelectedIndex >= 0 Then
                Dim recipes = _craftingManager.GetAvailableRecipes(
                    _characterLevel, _currentEpisode, _characterId)
                Dim recipe = recipes(lstRecipes.SelectedIndex)
                
                Dim info As New StringBuilder()
                info.AppendLine($"NAME: {recipe.Name}")
                info.AppendLine($"TYPE: {recipe.ArrowType}")
                info.AppendLine($"LEVEL: {recipe.RequiredLevel}")
                info.AppendLine($"TIME: {recipe.CraftingTime}s")
                info.AppendLine($"YIELD: {recipe.ResultCount}")
                info.AppendLine()
                info.AppendLine($"DESCRIPTION:")
                info.AppendLine(recipe.Description)
                
                txtRecipeInfo.Text = info.ToString()
                
                ' Show required materials
                lstMaterials.Items.Clear()
                For Each material In recipe.RequiredMaterials
                    Dim have = _craftingManager.GetMaterialCount(material.Key)
                    lstMaterials.Items.Add($"{material.Key}: {material.Value} (Have: {have})")
                Next
            End If
        End Sub
        
        Private Sub btnCraft_Click(sender As Object, e As EventArgs) Handles btnCraft.Click
            If lstRecipes.SelectedIndex < 0 Then
                MessageBox.Show("Select a recipe first!")
                Return
            End If
            
            Dim recipes = _craftingManager.GetAvailableRecipes(
                _characterLevel, _currentEpisode, _characterId)
            Dim recipe = recipes(lstRecipes.SelectedIndex)
            Dim quantity = CInt(numQuantity.Value)
            
            If _craftingManager.CraftArrow(recipe.ArrowType, quantity, _currentStation) Then
                MessageBox.Show($"Crafting {quantity}x {recipe.Name} started!")
                LoadInventory()
            Else
                MessageBox.Show("Cannot craft this item - missing materials or requirements!")
            End If
        End Sub
        
        Private Sub timer_Tick(sender As Object, e As EventArgs) Handles timer.Tick
            ' Update crafting progress
            ' This would be handled by the manager
            LoadInventory()
        End Sub
    End Class
    
    #End Region
    
    #Region "Integration with Main Game"
    
    Partial Public Class ArrowSagaGame
        Private _combatManager As CombatManager
        Private _craftingManager As CraftingManager
        Private _actionProfiles As Dictionary(Of CharacterID, Object)
        
        Public ReadOnly Property Combat As CombatManager
            Get
                Return _combatManager
            End Get
        End Property
        
        Public ReadOnly Property Crafting As CraftingManager
            Get
                Return _craftingManager
            End Get
        End Property
        
        Private Sub InitializeActionSystems()
            _combatManager = New CombatManager()
            _craftingManager = New CraftingManager()
            _actionProfiles = New Dictionary(Of CharacterID, Object>()
            
            ' Initialize character action profiles
            _actionProfiles(CharacterID.OliverQueen) = New OliverQueenActions()
            _actionProfiles(CharacterID.JohnDiggle) = New JohnDiggleActions()
            _actionProfiles(CharacterID.TheaQueen) = New TheaQueenActions()
            _actionProfiles(CharacterID.RoyHarper) = New RoyHarperActions()
            _actionProfiles(CharacterID.SaraLance) = New SaraLanceActions()
            _actionProfiles(CharacterID.LaurelLance) = New LaurelLanceActions()
            _actionProfiles(CharacterID.ReneRamirez) = New ReneRamirezActions()
            _actionProfiles(CharacterID.DinahDrake) = New DinahDrakeActions()
            _actionProfiles(CharacterID.CurtisHolt) = New CurtisHoltActions()
        End Sub
        
        Public Function GetCharacterActions(characterId As CharacterID) As Object
            If _actionProfiles.ContainsKey(characterId) Then
                Return _actionProfiles(characterId)
            End If
            Return Nothing
        End Function
        
        Public Sub StartBossFight(bossName As String, episode As Episode)
            Dim encounter As New CombatEncounter() With {
                .EncounterID = $"BOSS_{bossName}",
                .Episode = episode,
                .IsBossFight = True,
                .BossName = bossName
            }
            
            ' Add boss enemy
            Dim boss As New CombatEnemy(CharacterID.SladeWilson, 10) With {
                .IsBoss = True,
                .MaxPhases = 3,
                .Health = 1000,
                .MaxHealth = 1000
            }
            encounter.Enemies.Add(boss)
            
            ' Show combat form
            Dim combatForm As New CombatForm(Me, encounter)
            combatForm.ShowDialog()
        End Sub
    End Class
    
    #End Region
End Class