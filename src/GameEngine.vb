' ============================================================================
' ARROW: THE COMPLETE SAGA - STORY ENGINE
' A VB.NET Game Engine Covering Seasons 1-8
' Version 5.0 - "The Oliver Queen Story"
' ============================================================================

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Windows.Forms
Imports System.Math
Imports System.IO
Imports System.Threading
Imports System.Linq
Imports System.Text
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Xml.Serialization
Imports System.Security.Cryptography

Public Class ArrowSagaEngine
    Inherits Form
    
    #Region "Engine Constants"
    
    Private Const ENGINE_NAME As String = "ARROW SAGA ENGINE"
    Private Const ENGINE_VERSION As String = "5.0.0"
    Private Const SEASON_COUNT As Integer = 8
    Private Const EPISODES_PER_SEASON As Integer = 23
    Private Const TOTAL_EPISODES As Integer = 184
    Private Const CHARACTER_COUNT As Integer = 150
    Private Const LOCATION_COUNT As Integer = 75
    Private Const QUEST_COUNT As Integer = 500
    
    ' Screen Dimensions
    Private Const SCREEN_WIDTH As Integer = 1600
    Private Const SCREEN_HEIGHT As Integer = 900
    Private Const TARGET_FPS As Integer = 60
    
    #End Region
    
    #Region "Season & Episode Enumerations"
    
    Public Enum ArrowSeason
        Season1 = 1
        Season2 = 2
        Season3 = 3
        Season4 = 4
        Season5 = 5
        Season6 = 6
        Season7 = 7
        Season8 = 8
    End Enum
    
    Public Enum EpisodeStatus
        Locked
        Available
        InProgress
        Completed
        Mastered
    End Enum
    
    Public Enum StoryArc
        Origin
        ReturnToStarling
        Undertaking
        Mirakuru
        SuicideSquad
        LeagueOfAssassins
        RaAlGhul
        HIVE
        Dominators
        CaydenJames
        LongbowHunters
        NinthCircle
        GladesReedemption
        CrisisOnInfiniteEarths
    End Enum
    
    Public Enum CharacterID
        ' Main Characters
        OliverQueen
        JohnDiggle
        FelicitySmoak
        TheaQueen
        QuentinLance
        LaurelLance
        SaraLance
        RoyHarper
        SladeWilson
        MalcolmMerlyn
        MoiraQueen
        WalterSteele
        TommyMerlyn
        HelenaBertinelli
        McKennaHall
        
        ' Season 2
        SebastianBlood
        IsabelRocher
        Shado
        AnthonyIvo
        BrotherBlood
        
        ' Season 3
        RayPalmer
        RaAlGhul
        NyssaAlGhul
        MaseoYamashiro
        TatsuYamashiro
        WernerZytle
        
        ' Season 4
        DamienDarhk
        Anarky
        EvelynSharp
        CurtisHolt
        
        ' Season 5
        AdrianChase
        Prometheus
        TaliaAlGhul
        ReneRamirez
        DinahDrake
        VincentSobel
        
        ' Season 6
        RicardoDiaz
        AnatolyKnyazev
        CaydenJames
        BlackSiren
        
        ' Season 7
        EmikoQueen
        Dante
        StanleyDover
        
        ' Season 8
        WilliamClayton
        MiaSmoak
        ConnorHawke
        JJ
        ZoeRamirez
        
        ' Crisis Characters
        KaraDanvers
        BarryAllen
        ClarkKent
        KateKane
        JeffersonPierce
        JJonahJameson
    End Enum
    
    #End Region
    
    #Region "Core Story Classes"
    
    <Serializable>
    Public Class Episode
        Public Property Season As Integer
        Public Property EpisodeNumber As Integer
        Public Property Title As String
        Public Property Description As String
        Public Property AirDate As DateTime
        Public Property Status As EpisodeStatus
        Public Property IsCompleted As Boolean = False
        Public Property CompletionPercentage As Integer = 0
        Public Property UnlockedAchievements As List(Of String)
        Public Property CollectedItems As List(Of String)
        Public Property DiscoveredSecrets As List(Of String)
        Public Property DialogueCount As Integer = 0
        Public Property CombatEncounters As Integer = 0
        Public Property Choices As Dictionary(Of String, String)
        Public Property Consequences As List(Of String)
        
        Public Sub New()
            UnlockedAchievements = New List(Of String)
            CollectedItems = New List(Of String)
            DiscoveredSecrets = New List(Of String)
            Choices = New Dictionary(Of String, String)
            Consequences = New List(Of String)
        End Sub
        
        Public Sub New(season As Integer, episode As Integer, title As String, desc As String)
            Me.Season = season
            Me.EpisodeNumber = episode
            Me.Title = title
            Me.Description = desc
            Me.Status = EpisodeStatus.Locked
            UnlockedAchievements = New List(Of String)
            CollectedItems = New List(Of String)
            DiscoveredSecrets = New List(Of String)
            Choices = New Dictionary(Of String, String)
            Consequences = New List(Of String)
        End Sub
        
        Public Overrides Function ToString() As String
            Return $"S{Season:D2}E{EpisodeNumber:D2} - {Title}"
        End Function
    End Class
    
    <Serializable>
    Public Class Season
        Public Property SeasonNumber As Integer
        Public Property Name As String
        Public Property MainVillain As String
        Public Property PrimaryArc As StoryArc
        Public Property Episodes As List(Of Episode)
        Public Property IsUnlocked As Boolean = False
        Public Property CompletionPercentage As Integer = 0
        Public Property KeyMoments As List(Of String)
        Public Property CharacterDeaths As List(Of String)
        Public Property Relationships As Dictionary(Of String, String)
        
        Public Sub New()
            Episodes = New List(Of Episode)
            KeyMoments = New List(Of String)
            CharacterDeaths = New List(Of String)
            Relationships = New Dictionary(Of String, String)
        End Sub
        
        Public Sub New(seasonNum As Integer, name As String, villain As String, arc As StoryArc)
            Me.SeasonNumber = seasonNum
            Me.Name = name
            Me.MainVillain = villain
            Me.PrimaryArc = arc
            Me.Episodes = New List(Of Episode)
            Me.KeyMoments = New List(Of String)
            Me.CharacterDeaths = New List(Of String)
            Me.Relationships = New Dictionary(Of String, String)
        End Sub
    End Class
    
    <Serializable>
    Public Class StoryChoice
        Public Property ChoiceID As String
        Public Property Episode As Episode
        Public Property ChoiceText As String
        Public Property Options As List(Of ChoiceOption)
        Public Property TimeLimit As Double = -1
        Public Property AffectedCharacters As List(Of CharacterID)
        Public Property AffectedRelationships As Dictionary(Of CharacterID, Integer)
        Public Property UnlocksEpisode As Integer? = Nothing
        Public Property BlocksEpisode As Integer? = Nothing
        
        Public Sub New()
            Options = New List(Of ChoiceOption)
            AffectedCharacters = New List(Of CharacterID)
            AffectedRelationships = New Dictionary(Of CharacterID, Integer)
        End Sub
    End Class
    
    <Serializable>
    Public Class ChoiceOption
        Public Property OptionText As String
        Public Property DialogueID As String
        Public Property ReputationChange As Integer
        Public Property RelationshipChanges As Dictionary(Of CharacterID, Integer)
        Public Property UnlockedItems As List(Of String)
        Public Property UnlockedAbilities As List(Of String)
        Public Property QuestTriggers As List(Of String)
        Public Property NextEpisodeState As String
        Public Property IsCanon As Boolean = False
        
        Public Sub New()
            RelationshipChanges = New Dictionary(Of CharacterID, Integer)
            UnlockedItems = New List(Of String)
            UnlockedAbilities = New List(Of String)
            QuestTriggers = New List(Of String)
        End Sub
    End Class
    
    <Serializable>
    Public Class Character
        Public Property ID As CharacterID
        Public Property FirstName As String
        Public Property LastName As String
        Public Property Alias As String
        Public Property FirstAppearance As Episode
        Public Property LastAppearance As Episode
        Public Property IsAlive As Boolean = True
        Public Property IsPlayable As Boolean = False
        Public Property Faction As String
        Public Property Abilities As List(Of String)
        Public Property Relationships As Dictionary(Of CharacterID, Integer)
        Public Property StoryArc As List(Of String)
        Public Property KeyEpisodes As List(Of Episode)
        Public Property DeathEpisode As Episode
        Public Property ResurrectionEpisode As Episode
        Public Property CanonOutcome As String
        
        Public Sub New()
            Abilities = New List(Of String)
            Relationships = New Dictionary(Of CharacterID, Integer)
            StoryArc = New List(Of String)
            KeyEpisodes = New List(Of Episode)
        End Sub
        
        Public Function GetFullName() As String
            Return $"{FirstName} {LastName}"
        End Function
        
        Public Function GetDisplayName() As String
            If String.IsNullOrEmpty(Alias) Then
                Return GetFullName()
            Else
                Return $"{Alias} ({GetFullName()})"
            End If
        End Function
    End Class
    
    <Serializable>
    Public Class TimelineEvent
        Public Property EventID As String
        Public Property Season As Integer
        Public Property Episode As Integer
        Public Property Timestamp As TimeSpan
        Public Property Title As String
        Public Property Description As String
        Public Property InvolvedCharacters As List(Of CharacterID)
        Public Property Location As String
        Public Property Consequences As List(Of String)
        Public Property IsCanon As Boolean = True
        Public Property IsCrossover As Boolean = False
        Public Property CrossoverShow As String
        
        Public Sub New()
            InvolvedCharacters = New List(Of CharacterID)
            Consequences = New List(Of String)
        End Sub
    End Class
    
    <Serializable>
    Public Class Location
        Public Property LocationID As String
        Public Property Name As String
        Public Property District As String
        Public Property FirstAppearance As Episode
        Public Property SignificantEvents As List(Of TimelineEvent)
        Public Property IsDestroyed As Boolean = False
        Public Property DestructionEpisode As Episode
        Public Property RebuiltEpisode As Episode
        Public Property CharactersAssociated As List(Of CharacterID)
        Public Property SecretPassages As List(Of String)
        Public Property EasterEggs As List(Of String)
        
        Public Sub New()
            SignificantEvents = New List(Of TimelineEvent)
            CharactersAssociated = New List(Of CharacterID)
            SecretPassages = New List(Of String)
            EasterEggs = New List(Of String)
        End Sub
        
        Public Overrides Function ToString() As String
            Return Name
        End Function
    End Class
    
    <Serializable>
    Public Class CanonDatabase
        Public Property Seasons As Dictionary(Of Integer, Season)
        Public Property Characters As Dictionary(Of CharacterID, Character)
        Public Property Locations As Dictionary(Of String, Location)
        Public Property Timeline As List(Of TimelineEvent)
        Public Property Crossovers As List(Of CrossoverEvent)
        Public Property EasterEggs As List(Of EasterEgg)
        Public Property AlternateRealities As List(Of AlternateReality)
        Public Property CanonDecisions As Dictionary(Of String, String)
        
        Public Sub New()
            Seasons = New Dictionary(Of Integer, Season)
            Characters = New Dictionary(Of CharacterID, Character)
            Locations = New Dictionary(Of String, Location)
            Timeline = New List(Of TimelineEvent)
            Crossovers = New List(Of CrossoverEvent)
            EasterEggs = New List(Of EasterEgg)
            AlternateRealities = New List(Of AlternateReality)
            CanonDecisions = New Dictionary(Of String, String)
        End Sub
    End Class
    
    <Serializable>
    Public Class CrossoverEvent
        Public Property Name As String
        Public Property Year As Integer
        Public Property Episodes As List(Of Episode)
        Public Property ParticipatingShows As List(Of String)
        Public Property ParticipatingCharacters As List(Of CharacterID)
        Public Property MainVillain As String
        Public Property PermanentConsequences As List(Of String)
        
        Public Sub New()
            Episodes = New List(Of Episode)
            ParticipatingShows = New List(Of String)
            ParticipatingCharacters = New List(Of CharacterID)
            PermanentConsequences = New List(Of String)
        End Sub
    End Class
    
    <Serializable>
    Public Class EasterEgg
        Public Property Name As String
        Public Property Description As String
        Public Property Location As String
        Public Property Episode As Episode
        Public Property DiscoveryCondition As String
        Public Property Reference As String
        Public Property IsFound As Boolean = False
    End Class
    
    <Serializable>
    Public Class AlternateReality
        Public Property Name As String
        Public Property Trigger As String
        Public Property Changes As List(Of String)
        Public Property CharactersAffected As List(Of CharacterID)
        Public Property Episode As Episode
    End Class
    
    #End Region
    
    #Region "Game Progression Classes"
    
    Public Class PlayerProgress
        Public Property CurrentSeason As Integer = 1
        Public Property CurrentEpisode As Integer = 1
        Public Property CompletedEpisodes As List(Of String)
        Public Property UnlockedAchievements As List(Of String)
        Public Property CollectedCollectibles As List(Of String)
        Public Property CharacterKnowledge As Dictionary(Of CharacterID, Integer)
        Public Property TimelineUnlocked As List(Of Integer)
        Public Property Difficulty As String = "Normal"
        Public Property PlayTime As TimeSpan
        Public Property CanonChoices As Dictionary(Of String, String)
        Public Property AlternateRealityVisits As List(Of String)
        Public Property EasterEggsFound As List(Of String)
        
        Public Sub New()
            CompletedEpisodes = New List(Of String)
            UnlockedAchievements = New List(Of String)
            CollectedCollectibles = New List(Of String)
            CharacterKnowledge = New Dictionary(Of CharacterID, Integer)
            TimelineUnlocked = New List(Of Integer)
            CanonChoices = New Dictionary(Of String, String)
            AlternateRealityVisits = New List(Of String)
            EasterEggsFound = New List(Of String)
        End Sub
    End Class
    
    Public Class EpisodePlayer
        Public Property CurrentEpisode As Episode
        Public Property IsPlaying As Boolean = False
        Public Property IsPaused As Boolean = False
        Public Property CurrentScene As Integer = 0
        Public Property SceneProgress As Double = 0
        Public Property DialogueHistory As List(Of String)
        Public Property ActiveChoices As List(Of StoryChoice)
        Public Property CollectedInEpisode As List(Of String)
        Public Property DiscoveredInEpisode As List(Of String)
        Public Property CombatLog As List(Of String)
        
        Public Sub New()
            DialogueHistory = New List(Of String)
            ActiveChoices = New List(Of StoryChoice)
            CollectedInEpisode = New List(Of String)
            DiscoveredInEpisode = New List(Of String)
            CombatLog = New List(Of String)
        End Sub
    End Class
    
    Public Class TimelineViewer
        Public Property Events As SortedDictionary(Of DateTime, TimelineEvent)
        Public Property FilterSeason As Integer? = Nothing
        Public Property FilterCharacter As CharacterID? = Nothing
        Public Property ShowCrossoverOnly As Boolean = False
        Public Property SelectedEvent As TimelineEvent
        
        Public Sub New()
            Events = New SortedDictionary(Of DateTime, TimelineEvent)
        End Sub
    End Class
    
    #End Region
    
    #Region "Season Data Initialization"
    
    Public Class SeasonDataLoader
        
        Public Shared Function LoadAllSeasons() As CanonDatabase
            Dim database As New CanonDatabase()
            
            ' Initialize all characters
            InitializeCharacters(database)
            
            ' Initialize all locations
            InitializeLocations(database)
            
            ' Load Season 1
            database.Seasons(1) = CreateSeason1(database)
            
            ' Load Season 2
            database.Seasons(2) = CreateSeason2(database)
            
            ' Load Season 3
            database.Seasons(3) = CreateSeason3(database)
            
            ' Load Season 4
            database.Seasons(4) = CreateSeason4(database)
            
            ' Load Season 5
            database.Seasons(5) = CreateSeason5(database)
            
            ' Load Season 6
            database.Seasons(6) = CreateSeason6(database)
            
            ' Load Season 7
            database.Seasons(7) = CreateSeason7(database)
            
            ' Load Season 8
            database.Seasons(8) = CreateSeason8(database)
            
            ' Load Crossovers
            LoadCrossovers(database)
            
            ' Build complete timeline
            BuildTimeline(database)
            
            Return database
        End Function
        
        Private Shared Sub InitializeCharacters(database As CanonDatabase)
            ' Main Characters
            database.Characters(CharacterID.OliverQueen) = New Character() With {
                .ID = CharacterID.OliverQueen,
                .FirstName = "Oliver",
                .LastName = "Queen",
                .Alias = "The Green Arrow",
                .Faction = "Team Arrow",
                .IsPlayable = True,
                .Abilities = New List(Of String) From {
                    "Master Archer", "Hand-to-Hand Combat", "Detective Skills",
                    "Peak Human Conditioning", "Multilingual", "Business Acumen"
                }
            }
            
            database.Characters(CharacterID.JohnDiggle) = New Character() With {
                .ID = CharacterID.JohnDiggle,
                .FirstName = "John",
                .LastName = "Diggle",
                .Alias = "Spartan",
                .Faction = "Team Arrow",
                .IsPlayable = True,
                .Abilities = New List(Of String) From {
                    "Military Tactics", "Hand-to-Hand Combat", "Marksmanship",
                    "Strategic Planning", "Bodyguard Training"
                }
            }
            
            database.Characters(CharacterID.FelicitySmoak) = New Character() With {
                .ID = CharacterID.FelicitySmoak,
                .FirstName = "Felicity",
                .LastName = "Smoak",
                .Alias = "Overwatch",
                .Faction = "Team Arrow",
                .IsPlayable = True,
                .Abilities = New List(Of String) From {
                    "Genius-Level Intellect", "Hacking", "Computer Engineering",
                    "Quantum Mechanics", "Cyber Security"
                }
            }
            
            database.Characters(CharacterID.TheaQueen) = New Character() With {
                .ID = CharacterID.TheaQueen,
                .FirstName = "Thea",
                .LastName = "Queen",
                .Alias = "Speedy",
                .Faction = "Team Arrow",
                .IsPlayable = True,
                .Abilities = New List(Of String) From {
                    "Martial Arts", "Archery", "Business Management",
                    "League of Assassins Training"
                }
            }
            
            database.Characters(CharacterID.QuentinLance) = New Character() With {
                .ID = CharacterID.QuentinLance,
                .FirstName = "Quentin",
                .LastName = "Lance",
                .Alias = "Captain Lance",
                .Faction = "SCPD",
                .IsPlayable = False,
                .Abilities = New List(Of String) From {
                    "Police Procedures", "Investigation", "Firearms Training"
                }
            }
            
            database.Characters(CharacterID.LaurelLance) = New Character() With {
                .ID = CharacterID.LaurelLance,
                .FirstName = "Laurel",
                .LastName = "Lance",
                .Alias = "Black Canary",
                .Faction = "Team Arrow",
                .IsPlayable = True,
                .Abilities = New List(Of String) From {
                    "Martial Arts", "Cry", "Legal Expertise", "Investigation"
                }
            }
            
            database.Characters(CharacterID.SaraLance) = New Character() With {
                .ID = CharacterID.SaraLance,
                .FirstName = "Sara",
                .LastName = "Lance",
                .Alias = "White Canary",
                .Faction = "Legends",
                .IsPlayable = True,
                .Abilities = New List(Of String) From {
                    "League of Assassins Training", "Martial Arts", "Multilingual"
                }
            }
            
            database.Characters(CharacterID.RoyHarper) = New Character() With {
                .ID = CharacterID.RoyHarper,
                .FirstName = "Roy",
                .LastName = "Harper",
                .Alias = "Arsenal",
                .Faction = "Team Arrow",
                .IsPlayable = True,
                .Abilities = New List(Of String) From {
                    "Archery", "Acrobatics", "Mirakuru Enhanced", "Hand-to-Hand Combat"
                }
            }
            
            database.Characters(CharacterID.SladeWilson) = New Character() With {
                .ID = CharacterID.SladeWilson,
                .FirstName = "Slade",
                .LastName = "Wilson",
                .Alias = "Deathstroke",
                .Faction = "Villain",
                .IsPlayable = False,
                .Abilities = New List(Of String) From {
                    "Mirakuru Enhanced", "Master Tactician", "Expert Marksman",
                    "Hand-to-Hand Combat", "Swordsmanship"
                }
            }
            
            database.Characters(CharacterID.MalcolmMerlyn) = New Character() With {
                .ID = CharacterID.MalcolmMerlyn,
                .FirstName = "Malcolm",
                .LastName = "Merlyn",
                .Alias = "Dark Archer",
                .Faction = "Villain",
                .IsPlayable = False,
                .Abilities = New List(Of String) From {
                    "Master Archer", "League of Assassins Training", "Business Acumen"
                }
            }
            
            ' Add more characters as needed...
        End Sub
        
        Private Shared Sub InitializeLocations(database As CanonDatabase)
            database.Locations("StarCity") = New Location() With {
                .LocationID = "StarCity",
                .Name = "Star City",
                .District = "City"
            }
            
            database.Locations("TheGlades") = New Location() With {
                .LocationID = "TheGlades",
                .Name = "The Glades",
                .District = "Poor District"
            }
            
            database.Locations("QueenMansion") = New Location() With {
                .LocationID = "QueenMansion",
                .Name = "Queen Mansion",
                .District = "Starling Heights"
            }
            
            database.Locations("QueenConsolidated") = New Location() With {
                .LocationID = "QueenConsolidated",
                .Name = "Queen Consolidated Tower",
                .District = "Downtown"
            }
            
            database.Locations("TheQuiver") = New Location() With {
                .LocationID = "TheQuiver",
                .Name = "The Quiver",
                .District = "Founder's Island",
                .SecretPassages = New List(Of String) From {"Hidden Armory", "Training Room", "Med Bay"}
            }
            
            database.Locations("SCPD") = New Location() With {
                .LocationID = "SCPD",
                .Name = "Starling City Police Department",
                .District = "Downtown"
            }
            
            database.Locations("IronHeights") = New Location() With {
                .LocationID = "IronHeights",
                .Name = "Iron Heights Penitentiary",
                .District = "Industrial District"
            }
            
            database.Locations("LianYu") = New Location() With {
                .LocationID = "LianYu",
                .Name = "Lian Yu",
                .District = "North China Sea"
            }
            
            ' Add more locations...
        End Sub
        
        Private Shared Function CreateSeason1(database As CanonDatabase) As Season
            Dim season As New Season(1, "The Origin", "Malcolm Merlyn", StoryArc.Origin)
            
            ' Episode 1: Pilot
            Dim ep1 As New Episode(1, 1, "Pilot", "After five years on a hellish island, Oliver Queen returns home to find his city in peril and his family in crisis.")
            ep1.Status = EpisodeStatus.Available
            ep1.KeyMoments = New List(Of String) From {
                "Oliver returns to Starling City",
                "Discovery of the List",
                "First night as Hood",
                "Reunion with Thea and Moira",
                "Meeting with Laurel Lance"
            }
            ep1.Choices.Add("Save_Tommy", "Oliver must choose between saving Tommy or pursuing the List target")
            ep1.Consequences.Add("Relationship with Tommy affected")
            season.Episodes.Add(ep1)
            
            ' Episode 2: Honor Thy Father
            Dim ep2 As New Episode(1, 2, "Honor Thy Father", "Oliver targets a wealthy businessman on his father's list while dealing with Laurel's grief.")
            ep2.Status = EpisodeStatus.Locked
            season.Episodes.Add(ep2)
            
            ' Episode 3: Lone Gunmen
            Dim ep3 As New Episode(1, 3, "Lone Gunmen", "Oliver encounters a copycat vigilante with deadly methods.")
            season.Episodes.Add(ep3)
            
            ' Episode 4: An Innocent Man
            Dim ep4 As New Episode(1, 4, "An Innocent Man", "Oliver must prove a death row inmate's innocence before his execution.")
            season.Episodes.Add(ep4)
            
            ' Episode 5: Damaged
            Dim ep5 As New Episode(1, 5, "Damaged", "Oliver is forced to undergo a psychiatric evaluation after being suspected as the Hood.")
            season.Episodes.Add(ep5)
            
            ' Continue for all 23 episodes...
            ' For brevity, adding remaining episodes with basic info
            
            For epNum = 6 To 23
                Dim ep As New Episode(1, epNum, $"Episode {epNum}", $"Season 1 Episode {epNum}")
                season.Episodes.Add(ep)
            Next
            
            season.CharacterDeaths = New List(Of String) From {
                "Adam Hunt", "Martin Somers", "James Holder", "Noah Kuttler",
                "Billy Wintergreen", "Yao Fei", "Robert Queen (flashback)"
            }
            
            Return season
        End Function
        
        Private Shared Function CreateSeason2(database As CanonDatabase) As Season
            Dim season As New Season(2, "The Rise of Slade Wilson", "Slade Wilson", StoryArc.Mirakuru)
            
            ' Episode 1: City of Heroes
            Dim ep1 As New Episode(2, 1, "City of Heroes", "Oliver returns to find his city safer, but a new threat emerges.")
            ep1.Status = EpisodeStatus.Available
            ep1.KeyMoments = New List(Of String) From {
                "Introduction of Roy Harper as potential sidekick",
                "Return of Slade Wilson",
                "Discovery of Mirakuru"
            }
            season.Episodes.Add(ep1)
            
            ' Add all 23 episodes...
            For epNum = 2 To 23
                Dim ep As New Episode(2, epNum, $"Episode {epNum}", $"Season 2 Episode {epNum}")
                season.Episodes.Add(ep)
            Next
            
            season.CharacterDeaths = New List(Of String) From {
                "Moira Queen",
                "Shado",
                "Anthony Ivo",
                "Sebastian Blood"
            }
            
            Return season
        End Function
        
        Private Shared Function CreateSeason3(database As CanonDatabase) As Season
            Dim season As New Season(3, "The League of Assassins", "Ra's al Ghul", StoryArc.LeagueOfAssassins)
            
            ' Episode 1: The Calm
            Dim ep1 As New Episode(3, 1, "The Calm", "Oliver faces a new threat while balancing his role as CEO and vigilante.")
            season.Episodes.Add(ep1)
            
            For epNum = 2 To 23
                Dim ep As New Episode(3, epNum, $"Episode {epNum}", $"Season 3 Episode {epNum}")
                season.Episodes.Add(ep)
            Next
            
            season.CharacterDeaths = New List(Of String) From {
                "Sara Lance",
                "Ra's al Ghul",
                "Maseo Yamashiro"
            }
            
            Return season
        End Function
        
        Private Shared Function CreateSeason4(database As CanonDatabase) As Season
            Dim season As New Season(4, "HIVE and Magic", "Damien Darhk", StoryArc.HIVE)
            
            ' Episode 1: Green Arrow
            Dim ep1 As New Episode(4, 1, "Green Arrow", "Oliver embraces his new identity as Green Arrow while facing a mystical threat.")
            season.Episodes.Add(ep1)
            
            For epNum = 2 To 23
                Dim ep As New Episode(4, epNum, $"Episode {epNum}", $"Season 4 Episode {epNum}")
                season.Episodes.Add(ep)
            Next
            
            season.CharacterDeaths = New List(Of String) From {
                "Laurel Lance",
                "Damien Darhk"
            }
            
            Return season
        End Function
        
        Private Shared Function CreateSeason5(database As CanonDatabase) As Season
            Dim season As New Season(5, "Prometheus", "Adrian Chase", StoryArc.LongbowHunters)
            
            ' Episode 1: Legacy
            Dim ep1 As New Episode(5, 1, "Legacy", "Oliver recruits new team members while a new enemy targets his legacy.")
            season.Episodes.Add(ep1)
            
            For epNum = 2 To 23
                Dim ep As New Episode(5, epNum, $"Episode {epNum}", $"Season 5 Episode {epNum}")
                season.Episodes.Add(ep)
            Next
            
            season.CharacterDeaths = New List(Of String) From {
                "Adrian Chase",
                "Evelyn Sharp (betrayal)"
            }
            
            Return season
        End Function
        
        Private Shared Function CreateSeason6(database As CanonDatabase) As Season
            Dim season As New Season(6, "Dragon Rising", "Ricardo Diaz", StoryArc.LongbowHunters)
            
            ' Episode 1: Fallout
            Dim ep1 As New Episode(6, 1, "Fallout", "The team deals with the aftermath of Lian Yu's explosion.")
            season.Episodes.Add(ep1)
            
            For epNum = 2 To 23
                Dim ep As New Episode(6, epNum, $"Episode {epNum}", $"Season 6 Episode {epNum}")
                season.Episodes.Add(ep)
            Next
            
            season.CharacterDeaths = New List(Of String) From {
                "Quentin Lance",
                "Cayden James"
            }
            
            Return season
        End Function
        
        Private Shared Function CreateSeason7(database As CanonDatabase) As Season
            Dim season As New Season(7, "The Ninth Circle", "Emiko Queen", StoryArc.NinthCircle)
            
            ' Episode 1: Inmate 4587
            Dim ep1 As New Episode(7, 1, "Inmate 4587", "Oliver adapts to life in prison while his team operates outside.")
            season.Episodes.Add(ep1)
            
            For epNum = 2 To 22
                Dim ep As New Episode(7, epNum, $"Episode {epNum}", $"Season 7 Episode {epNum}")
                season.Episodes.Add(ep)
            Next
            
            season.CharacterDeaths = New List(Of String) From {
                "Emiko Queen",
                "Dante"
            }
            
            Return season
        End Function
        
        Private Shared Function CreateSeason8(database As CanonDatabase) As Season
            Dim season As New Season(8, "Crisis", "The Anti-Monitor", StoryArc.CrisisOnInfiniteEarths)
            
            ' Episode 1: Starling City
            Dim ep1 As New Episode(8, 1, "Starling City", "Oliver travels to an alternate Earth to recruit Adrian Chase.")
            season.Episodes.Add(ep1)
            
            ' Episode 2: Welcome to Hong Kong
            Dim ep2 As New Episode(8, 2, "Welcome to Hong Kong", "Oliver revisits his past in Hong Kong.")
            season.Episodes.Add(ep2)
            
            ' Episode 3: Leap of Faith
            Dim ep3 As New Episode(8, 3, "Leap of Faith", "Oliver faces Talia al Ghul in a battle for control.")
            season.Episodes.Add(ep3)
            
            ' Episode 4: Present Tense
            Dim ep4 As New Episode(8, 4, "Present Tense", "The Monitor sends Oliver to 2040 to meet his future children.")
            season.Episodes.Add(ep4)
            
            ' Episode 5: Prochnost
            Dim ep5 As New Episode(8, 5, "Prochnost", "Oliver confronts his past with the Bratva.")
            season.Episodes.Add(ep5)
            
            ' Episode 6: Reset
            Dim ep6 As New Episode(8, 6, "Reset", "Oliver repeats the same day until he can save everyone.")
            season.Episodes.Add(ep6)
            
            ' Episode 7: Purgatory
            Dim ep7 As New Episode(8, 7, "Purgatory", "Oliver returns to Lian Yu for a final test.")
            season.Episodes.Add(ep7)
            
            ' Episode 8: Crisis on Infinite Earths (Part 4)
            Dim ep8 As New Episode(8, 8, "Crisis on Infinite Earths: Part Four", "Oliver makes the ultimate sacrifice.")
            ep8.KeyMoments = New List(Of String) From {
                "Oliver becomes the Spectre",
                "Sacrifice to save the multiverse",
                "Final scene with Felicity"
            }
            season.Episodes.Add(ep8)
            
            season.CharacterDeaths = New List(Of String) From {
                "Oliver Queen (becomes Spectre)",
                "The Anti-Monitor"
            }
            
            Return season
        End Function
        
        Private Shared Sub LoadCrossovers(database As CanonDatabase)
            ' Flash vs Arrow
            Dim crossover1 As New CrossoverEvent() With {
                .Name = "Flash vs Arrow",
                .Year = 2014,
                .MainVillain = "Roy Bivolo/Rainbow Raider",
                .ParticipatingShows = New List(Of String) From {"Arrow", "The Flash"}
            }
            crossover1.ParticipatingCharacters.Add(CharacterID.OliverQueen)
            crossover1.ParticipatingCharacters.Add(CharacterID.BarryAllen)
            crossover1.ParticipatingCharacters.Add(CharacterID.FelicitySmoak)
            crossover1.ParticipatingCharacters.Add(CharacterID.JohnDiggle)
            database.Crossovers.Add(crossover1)
            
            ' Heroes Join Forces
            Dim crossover2 As New CrossoverEvent() With {
                .Name = "Heroes Join Forces",
                .Year = 2015,
                .MainVillain = "Vandal Savage",
                .ParticipatingShows = New List(Of String) From {"Arrow", "The Flash", "Legends of Tomorrow"}
            }
            database.Crossovers.Add(crossover2)
            
            ' Invasion!
            Dim crossover3 As New CrossoverEvent() With {
                .Name = "Invasion!",
                .Year = 2016,
                .MainVillain = "Dominators",
                .ParticipatingShows = New List(Of String) From {"Arrow", "The Flash", "Legends of Tomorrow", "Supergirl"}
            }
            database.Crossovers.Add(crossover3)
            
            ' Crisis on Earth-X
            Dim crossover4 As New CrossoverEvent() With {
                .Name = "Crisis on Earth-X",
                .Year = 2017,
                .MainVillain = "Dark Arrow/Overgirl",
                .ParticipatingShows = New List(Of String) From {"Arrow", "The Flash", "Legends of Tomorrow", "Supergirl"}
            }
            database.Crossovers.Add(crossover4)
            
            ' Elseworlds
            Dim crossover5 As New CrossoverEvent() With {
                .Name = "Elseworlds",
                .Year = 2018,
                .MainVillain = "John Deegan",
                .ParticipatingShows = New List(Of String) From {"Arrow", "The Flash", "Supergirl"}
            }
            database.Crossovers.Add(crossover5)
            
            ' Crisis on Infinite Earths
            Dim crossover6 As New CrossoverEvent() With {
                .Name = "Crisis on Infinite Earths",
                .Year = 2019,
                .MainVillain = "Anti-Monitor",
                .ParticipatingShows = New List(Of String) From {
                    "Arrow", "The Flash", "Supergirl", "Legends of Tomorrow",
                    "Batwoman", "Black Lightning", "Titans", "Doom Patrol",
                    "Smallville", "Birds of Prey", "Batman (1966)"
                }
            }
            crossover6.PermanentConsequences = New List(Of String) From {
                "Oliver Queen's sacrifice",
                "Birth of new multiverse",
                "Spectre Oliver watches over creation"
            }
            database.Crossovers.Add(crossover6)
        End Sub
        
        Private Shared Sub BuildTimeline(database As CanonDatabase)
            ' This would populate the complete timeline with all events from all seasons
            ' For brevity, showing key events
            
            Dim timeline = database.Timeline
            
            ' Season 1 Events
            timeline.Add(New TimelineEvent() With {
                .EventID = "S1E1_1",
                .Season = 1,
                .Episode = 1,
                .Title = "Oliver Returns",
                .Description = "Oliver Queen returns to Starling City after 5 years on Lian Yu",
                .Location = "Starling City"
            })
            
            ' Season 2 Events
            timeline.Add(New TimelineEvent() With {
                .EventID = "S2E20_1",
                .Season = 2,
                .Episode = 20,
                .Title = "Death of Moira Queen",
                .Description = "Moira Queen is killed by Slade Wilson during the siege",
                .Location = "Queen Mansion"
            })
            
            ' Season 3 Events
            timeline.Add(New TimelineEvent() With {
                .EventID = "S3E9_1",
                .Season = 3,
                .Episode = 9,
                .Title = "The Climb",
                .Description = "Oliver fights Ra's al Ghul on the mountain",
                .Location = "Nanda Parbat"
            })
            
            ' Season 4 Events
            timeline.Add(New TimelineEvent() With {
                .EventID = "S4E18_1",
                .Season = 4,
                .Episode = 18,
                .Title = "Death of Laurel Lance",
                .Description = "Laurel Lance is killed by Damien Darhk",
                .Location = "Star City"
            })
            
            ' Season 5 Events
            timeline.Add(New TimelineEvent() With {
                .EventID = "S5E23_1",
                .Season = 5,
                .Episode = 23,
                .Title = "Lian Yu Explodes",
                .Description = "Adrian Chase detonates explosives on Lian Yu",
                .Location = "Lian Yu"
            })
            
            ' Season 6 Events
            timeline.Add(New TimelineEvent() With {
                .EventID = "S6E23_1",
                .Season = 6,
                .Episode = 23,
                .Title = "Death of Quentin Lance",
                .Description = "Quentin Lance dies saving Black Siren",
                .Location = "Star City"
            })
            
            ' Season 7 Events
            timeline.Add(New TimelineEvent() With {
                .EventID = "S7E22_1",
                .Season = 7,
                .Episode = 22,
                .Title = "Emiko's Betrayal",
                .Description = "Emiko Queen reveals herself as the leader of the Ninth Circle",
                .Location = "Star City"
            })
            
            ' Season 8 Events
            timeline.Add(New TimelineEvent() With {
                .EventID = "S8E8_1",
                .Season = 8,
                .Episode = 8,
                .Title = "Crisis Sacrifice",
                .Description = "Oliver Queen becomes the Spectre and sacrifices himself to save the multiverse",
                .Location = "Vanishing Point"
            })
            
            ' Sort timeline
            database.Timeline = database.Timeline.OrderBy(Function(e) e.Season).ThenBy(Function(e) e.Episode).ToList()
        End Sub
    End Class
    
    #End Region
    
    #Region "Game Engine Core"
    
    Public Class ArrowSagaGame
        Private _database As CanonDatabase
        Private _progress As PlayerProgress
        Private _currentEpisode As EpisodePlayer
        Private _timelineViewer As TimelineViewer
        Private _gameState As String
        Private _currentSeason As Integer = 1
        Private _currentEpisodeNum As Integer = 1
        
        Public ReadOnly Property Database As CanonDatabase
            Get
                Return _database
            End Get
        End Property
        
        Public ReadOnly Property Progress As PlayerProgress
            Get
                Return _progress
            End Get
        End Property
        
        Public ReadOnly Property CurrentEpisode As EpisodePlayer
            Get
                Return _currentEpisode
            End Get
        End Property
        
        Public Sub New()
            _database = SeasonDataLoader.LoadAllSeasons()
            _progress = New PlayerProgress()
            _timelineViewer = New TimelineViewer()
            _currentEpisode = New EpisodePlayer()
        End Sub
        
        Public Sub StartNewGame()
            _progress = New PlayerProgress()
            _progress.CurrentSeason = 1
            _progress.CurrentEpisode = 1
            _database.Seasons(1).Episodes(0).Status = EpisodeStatus.Available
            LoadEpisode(1, 1)
        End Sub
        
        Public Sub LoadEpisode(season As Integer, episode As Integer)
            If _database.Seasons.ContainsKey(season) AndAlso 
               episode <= _database.Seasons(season).Episodes.Count Then
                
                _currentEpisode.CurrentEpisode = _database.Seasons(season).Episodes(episode - 1)
                _currentEpisode.IsPlaying = True
                _currentEpisode.CurrentScene = 0
                _currentEpisode.SceneProgress = 0
                _currentEpisode.DialogueHistory.Clear()
                _currentEpisode.ActiveChoices.Clear()
                
                _database.Seasons(season).Episodes(episode - 1).Status = EpisodeStatus.InProgress
            End If
        End Sub
        
        Public Sub CompleteCurrentEpisode()
            If _currentEpisode.CurrentEpisode IsNot Nothing Then
                _currentEpisode.CurrentEpisode.IsCompleted = True
                _currentEpisode.CurrentEpisode.Status = EpisodeStatus.Completed
                _currentEpisode.CurrentEpisode.CompletionPercentage = 100
                
                _progress.CompletedEpisodes.Add(_currentEpisode.CurrentEpisode.ToString())
                
                ' Unlock next episode
                If _currentEpisode.CurrentEpisode.EpisodeNumber < _database.Seasons(_currentEpisode.CurrentEpisode.Season).Episodes.Count Then
                    Dim nextEp = _database.Seasons(_currentEpisode.CurrentEpisode.Season).Episodes(_currentEpisode.CurrentEpisode.EpisodeNumber)
                    nextEp.Status = EpisodeStatus.Available
                End If
                
                _currentEpisode.IsPlaying = False
            End If
        End Sub
        
        Public Function GetEpisodeProgress(season As Integer, episode As Integer) As Double
            If _database.Seasons.ContainsKey(season) AndAlso 
               episode <= _database.Seasons(season).Episodes.Count Then
                Return _database.Seasons(season).Episodes(episode - 1).CompletionPercentage
            End If
            Return 0
        End Function
        
        Public Function GetSeasonCompletion(season As Integer) As Double
            If Not _database.Seasons.ContainsKey(season) Then Return 0
            
            Dim totalEpisodes = _database.Seasons(season).Episodes.Count
            Dim completedEpisodes = _database.Seasons(season).Episodes.Count(Function(e) e.IsCompleted)
            
            Return (completedEpisodes / totalEpisodes) * 100
        End Function
        
        Public Function GetOverallCompletion() As Double
            Dim totalEpisodes = _database.Seasons.Sum(Function(s) s.Value.Episodes.Count)
            Dim completedEpisodes = _progress.CompletedEpisodes.Count
            
            Return (completedEpisodes / totalEpisodes) * 100
        End Function
        
        Public Function GetCharacterRelationship(playerId As CharacterID, targetId As CharacterID) As Integer
            If _database.Characters.ContainsKey(playerId) AndAlso
               _database.Characters(playerId).Relationships.ContainsKey(targetId) Then
                Return _database.Characters(playerId).Relationships(targetId)
            End If
            Return 0
        End Function
        
        Public Sub UpdateRelationship(character1 As CharacterID, character2 As CharacterID, change As Integer)
            If Not _database.Characters.ContainsKey(character1) Then Return
            If Not _database.Characters(character1).Relationships.ContainsKey(character2) Then
                _database.Characters(character1).Relationships(character2) = 0
            End If
            
            _database.Characters(character1).Relationships(character2) += change
            
            ' Update reverse relationship
            If Not _database.Characters.ContainsKey(character2) Then Return
            If Not _database.Characters(character2).Relationships.ContainsKey(character1) Then
                _database.Characters(character2).Relationships(character1) = 0
            End If
            
            _database.Characters(character2).Relationships(character1) += change
        End Sub
        
        Public Function GetTimelineEvents(filterSeason As Integer?) As List(Of TimelineEvent>
            If filterSeason.HasValue Then
                Return _database.Timeline.Where(Function(e) e.Season = filterSeason.Value).ToList()
            End If
            Return _database.Timeline
        End Function
        
        Public Function SearchEasterEggs(searchTerm As String) As List(Of EasterEgg>
            Return _database.EasterEggs.Where(Function(e) 
                e.Name.ToLower().Contains(searchTerm.ToLower()) OrElse
                e.Description.ToLower().Contains(searchTerm.ToLower())
            ).ToList()
        End Function
        
        Public Function GetCanonDecision(decisionId As String) As String
            If _database.CanonDecisions.ContainsKey(decisionId) Then
                Return _database.CanonDecisions(decisionId)
            End If
            Return String.Empty
        End Function
        
        Public Sub MakeChoice(choiceId As String, optionIndex As Integer)
            If _currentEpisode.ActiveChoices Is Nothing Then Return
            
            Dim choice = _currentEpisode.ActiveChoices.FirstOrDefault(Function(c) c.ChoiceID = choiceId)
            If choice Is Nothing Then Return
            
            If optionIndex < 0 OrElse optionIndex >= choice.Options.Count Then Return
            
            Dim selectedOption = choice.Options(optionIndex)
            
            ' Record the choice
            If Not _progress.CanonChoices.ContainsKey(choiceId) Then
                _progress.CanonChoices.Add(choiceId, selectedOption.OptionText)
            Else
                _progress.CanonChoices(choiceId) = selectedOption.OptionText
            End If
            
            ' Apply relationship changes
            For Each relChange In selectedOption.RelationshipChanges
                UpdateRelationship(CharacterID.OliverQueen, relChange.Key, relChange.Value)
            Next
            
            ' Unlock items
            For Each item In selectedOption.UnlockedItems
                _currentEpisode.CollectedInEpisode.Add(item)
            Next
            
            ' Unlock abilities
            For Each ability In selectedOption.UnlockedAbilities
                If Not _database.Characters(CharacterID.OliverQueen).Abilities.Contains(ability) Then
                    _database.Characters(CharacterID.OliverQueen).Abilities.Add(ability)
                End If
            Next
            
            ' Trigger quests
            For Each quest In selectedOption.QuestTriggers
                ' Quest system integration
            Next
            
            ' Update episode state
            _currentEpisode.DialogueHistory.Add($"Choice made: {selectedOption.OptionText}")
        End Sub
    End Class
    
    #End Region
    
    #Region "UI Forms"
    
    Public Class MainMenuForm
        Inherits Form
        
        Private WithEvents btnNewGame As Button
        Private WithEvents btnContinue As Button
        Private WithEvents btnEpisodes As Button
        Private WithEvents btnTimeline As Button
        Private WithEvents btnCharacters As Button
        Private WithEvents btnAchievements As Button
        Private WithEvents btnOptions As Button
        Private WithEvents btnExit As Button
        
        Private lblTitle As Label
        Private lblVersion As Label
        Private pbLogo As PictureBox
        Private gameEngine As ArrowSagaGame
        
        Public Sub New(engine As ArrowSagaGame)
            Me.gameEngine = engine
            InitializeComponent()
            SetupForm()
        End Sub
        
        Private Sub InitializeComponent()
            Me.Text = "ARROW: The Complete Saga"
            Me.Size = New Size(1024, 768)
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.FormBorderStyle = FormBorderStyle.FixedSingle
            Me.MaximizeBox = False
            Me.BackColor = Color.FromArgb(30, 30, 30)
            
            ' Title Label
            lblTitle = New Label() With {
                .Text = "ARROW",
                .Font = New Font("Arial Black", 48, FontStyle.Bold),
                .ForeColor = Color.FromArgb(0, 255, 0),
                .Location = New Point(300, 50),
                .Size = New Size(500, 100),
                .TextAlign = ContentAlignment.MiddleCenter
            }
            
            lblVersion = New Label() With {
                .Text = "The Complete Saga Engine v" & ENGINE_VERSION,
                .Font = New Font("Arial", 10),
                .ForeColor = Color.Gray,
                .Location = New Point(400, 150),
                .Size = New Size(300, 20),
                .TextAlign = ContentAlignment.MiddleCenter
            }
            
            ' Buttons
            Dim buttonX = 412
            Dim buttonY = 250
            Dim buttonWidth = 200
            Dim buttonHeight = 40
            Dim buttonSpacing = 10
            
            btnNewGame = CreateMenuButton("NEW GAME", buttonX, buttonY, buttonWidth, buttonHeight)
            btnContinue = CreateMenuButton("CONTINUE", buttonX, buttonY + (buttonHeight + buttonSpacing) * 1, buttonWidth, buttonHeight)
            btnEpisodes = CreateMenuButton("EPISODES", buttonX, buttonY + (buttonHeight + buttonSpacing) * 2, buttonWidth, buttonHeight)
            btnTimeline = CreateMenuButton("TIMELINE", buttonX, buttonY + (buttonHeight + buttonSpacing) * 3, buttonWidth, buttonHeight)
            btnCharacters = CreateMenuButton("CHARACTERS", buttonX, buttonY + (buttonHeight + buttonSpacing) * 4, buttonWidth, buttonHeight)
            btnAchievements = CreateMenuButton("ACHIEVEMENTS", buttonX, buttonY + (buttonHeight + buttonSpacing) * 5, buttonWidth, buttonHeight)
            btnOptions = CreateMenuButton("OPTIONS", buttonX, buttonY + (buttonHeight + buttonSpacing) * 6, buttonWidth, buttonHeight)
            btnExit = CreateMenuButton("EXIT", buttonX, buttonY + (buttonHeight + buttonSpacing) * 7, buttonWidth, buttonHeight)
            
            Me.Controls.AddRange(New Control() {
                lblTitle, lblVersion,
                btnNewGame, btnContinue, btnEpisodes, btnTimeline,
                btnCharacters, btnAchievements, btnOptions, btnExit
            })
        End Sub
        
        Private Function CreateMenuButton(text As String, x As Integer, y As Integer, width As Integer, height As Integer) As Button
            Dim btn As New Button() With {
                .Text = text,
                .Location = New Point(x, y),
                .Size = New Size(width, height),
                .FlatStyle = FlatStyle.Flat,
                .FlatAppearance = New FlatButtonAppearance() With {
                    .BorderColor = Color.FromArgb(0, 255, 0),
                    .BorderSize = 2
                },
                .BackColor = Color.FromArgb(50, 50, 50),
                .ForeColor = Color.White,
                .Font = New Font("Arial", 12, FontStyle.Bold)
            }
            Return btn
        End Function
        
        Private Sub SetupForm()
            ' Additional form setup
        End Sub
        
        Private Sub btnNewGame_Click(sender As Object, e As EventArgs) Handles btnNewGame.Click
            gameEngine.StartNewGame()
            Dim episodeForm As New EpisodePlayerForm(gameEngine)
            episodeForm.Show()
            Me.Hide()
        End Sub
        
        Private Sub btnEpisodes_Click(sender As Object, e As EventArgs) Handles btnEpisodes.Click
            Dim episodeSelect As New EpisodeSelectionForm(gameEngine)
            episodeSelect.ShowDialog()
        End Sub
        
        Private Sub btnTimeline_Click(sender As Object, e As EventArgs) Handles btnTimeline.Click
            Dim timelineForm As New TimelineForm(gameEngine)
            timelineForm.ShowDialog()
        End Sub
        
        Private Sub btnCharacters_Click(sender As Object, e As EventArgs) Handles btnCharacters.Click
            Dim characterForm As New CharacterDatabaseForm(gameEngine)
            characterForm.ShowDialog()
        End Sub
        
        Private Sub btnExit_Click(sender As Object, e As EventArgs) Handles btnExit.Click
            Application.Exit()
        End Sub
    End Class
    
    Public Class EpisodeSelectionForm
        Inherits Form
        
        Private gameEngine As ArrowSagaGame
        Private WithEvents lstSeasons As ListBox
        Private WithEvents lstEpisodes As ListBox
        Private txtDescription As TextBox
        Private pbProgress As ProgressBar
        Private btnPlay As Button
        Private lblSeasonInfo As Label
        
        Public Sub New(engine As ArrowSagaGame)
            Me.gameEngine = engine
            InitializeComponent()
            LoadSeasons()
        End Sub
        
        Private Sub InitializeComponent()
            Me.Text = "Episode Selection"
            Me.Size = New Size(900, 600)
            Me.StartPosition = FormStartPosition.CenterParent
            Me.BackColor = Color.FromArgb(40, 40, 40)
            
            ' Season List
            lstSeasons = New ListBox() With {
                .Location = New Point(20, 20),
                .Size = New Size(200, 500),
                .BackColor = Color.FromArgb(50, 50, 50),
                .ForeColor = Color.White,
                .Font = New Font("Arial", 11)
            }
            
            ' Episode List
            lstEpisodes = New ListBox() With {
                .Location = New Point(240, 20),
                .Size = New Size(300, 500),
                .BackColor = Color.FromArgb(50, 50, 50),
                .ForeColor = Color.White,
                .Font = New Font("Arial", 10)
            }
            
            ' Description Box
            txtDescription = New TextBox() With {
                .Location = New Point(560, 20),
                .Size = New Size(300, 100),
                .Multiline = True,
                .ReadOnly = True,
                .BackColor = Color.FromArgb(50, 50, 50),
                .ForeColor = Color.White,
                .Font = New Font("Arial", 10)
            }
            
            ' Progress Bar
            pbProgress = New ProgressBar() With {
                .Location = New Point(560, 130),
                .Size = New Size(300, 20),
                .Style = ProgressBarStyle.Continuous
            }
            
            ' Season Info Label
            lblSeasonInfo = New Label() With {
                .Location = New Point(560, 160),
                .Size = New Size(300, 30),
                .ForeColor = Color.White,
                .Font = New Font("Arial", 10)
            }
            
            ' Play Button
            btnPlay = New Button() With {
                .Text = "PLAY EPISODE",
                .Location = New Point(560, 200),
                .Size = New Size(300, 40),
                .BackColor = Color.FromArgb(0, 100, 0),
                .ForeColor = Color.White,
                .Font = New Font("Arial", 12, FontStyle.Bold)
            }
            
            Me.Controls.AddRange(New Control() {
                lstSeasons, lstEpisodes, txtDescription,
                pbProgress, lblSeasonInfo, btnPlay
            })
        End Sub
        
        Private Sub LoadSeasons()
            For s = 1 To SEASON_COUNT
                Dim season = gameEngine.Database.Seasons(s)
                Dim completion = gameEngine.GetSeasonCompletion(s)
                lstSeasons.Items.Add($"Season {s}: {season.Name} ({completion:F0}%)")
            Next
        End Sub
        
        Private Sub lstSeasons_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstSeasons.SelectedIndexChanged
            If lstSeasons.SelectedIndex >= 0 Then
                Dim seasonNum = lstSeasons.SelectedIndex + 1
                LoadEpisodes(seasonNum)
            End If
        End Sub
        
        Private Sub LoadEpisodes(seasonNum As Integer)
            lstEpisodes.Items.Clear()
            Dim season = gameEngine.Database.Seasons(seasonNum)
            
            For Each episode In season.Episodes
                Dim status As String
                Select Case episode.Status
                    Case EpisodeStatus.Locked
                        status = "🔒"
                    Case EpisodeStatus.Available
                        status = "🔓"
                    Case EpisodeStatus.InProgress
                        status = "▶"
                    Case EpisodeStatus.Completed
                        status = "✓"
                    Case EpisodeStatus.Mastered
                        status = "★"
                End Select
                
                lstEpisodes.Items.Add($"{status} S{seasonNum:D2}E{episode.EpisodeNumber:D2} - {episode.Title}")
            Next
        End Sub
        
        Private Sub lstEpisodes_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstEpisodes.SelectedIndexChanged
            If lstEpisodes.SelectedIndex >= 0 AndAlso lstSeasons.SelectedIndex >= 0 Then
                Dim seasonNum = lstSeasons.SelectedIndex + 1
                Dim episode = gameEngine.Database.Seasons(seasonNum).Episodes(lstEpisodes.SelectedIndex)
                
                txtDescription.Text = episode.Description
                pbProgress.Value = episode.CompletionPercentage
                
                If episode.Status = EpisodeStatus.Locked Then
                    btnPlay.Enabled = False
                Else
                    btnPlay.Enabled = True
                End If
            End If
        End Sub
        
        Private Sub btnPlay_Click(sender As Object, e As EventArgs) Handles btnPlay.Click
            If lstSeasons.SelectedIndex >= 0 AndAlso lstEpisodes.SelectedIndex >= 0 Then
                Dim seasonNum = lstSeasons.SelectedIndex + 1
                Dim episodeNum = lstEpisodes.SelectedIndex + 1
                
                gameEngine.LoadEpisode(seasonNum, episodeNum)
                Dim playerForm As New EpisodePlayerForm(gameEngine)
                playerForm.Show()
                Me.Close()
            End If
        End Sub
    End Class
    
    Public Class EpisodePlayerForm
        Inherits Form
        
        Private gameEngine As ArrowSagaGame
        Private WithEvents timer As New Timer()
        Private lblTitle As Label
        Private lblScene As Label
        Private txtDialogue As TextBox
        Private pnlChoices As Panel
        Private pbProgress As ProgressBar
        Private btnPause As Button
        Private btnSkip As Button
        
        Public Sub New(engine As ArrowSagaGame)
            Me.gameEngine = engine
            InitializeComponent()
            SetupTimer()
            UpdateDisplay()
        End Sub
        
        Private Sub InitializeComponent()
            Me.Text = "Playing Episode"
            Me.Size = New Size(1024, 768)
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.BackColor = Color.FromArgb(20, 20, 20)
            
            ' Title Label
            lblTitle = New Label() With {
                .Location = New Point(20, 20),
                .Size = New Size(984, 40),
                .ForeColor = Color.FromArgb(0, 255, 0),
                .Font = New Font("Arial", 20, FontStyle.Bold),
                .TextAlign = ContentAlignment.MiddleCenter
            }
            
            ' Scene Label
            lblScene = New Label() With {
                .Location = New Point(20, 70),
                .Size = New Size(984, 30),
                .ForeColor = Color.White,
                .Font = New Font("Arial", 14),
                .TextAlign = ContentAlignment.MiddleCenter
            }
            
            ' Dialogue Box
            txtDialogue = New TextBox() With {
                .Location = New Point(20, 120),
                .Size = New Size(984, 400),
                .Multiline = True,
                .ReadOnly = True,
                .BackColor = Color.FromArgb(30, 30, 30),
                .ForeColor = Color.White,
                .Font = New Font("Arial", 12),
                .ScrollBars = ScrollBars.Vertical
            }
            
            ' Choices Panel
            pnlChoices = New Panel() With {
                .Location = New Point(20, 530),
                .Size = New Size(984, 150),
                .BackColor = Color.FromArgb(40, 40, 40)
            }
            
            ' Progress Bar
            pbProgress = New ProgressBar() With {
                .Location = New Point(20, 690),
                .Size = New Size(800, 20),
                .Style = ProgressBarStyle.Continuous
            }
            
            ' Control Buttons
            btnPause = New Button() With {
                .Text = "PAUSE",
                .Location = New Point(830, 690),
                .Size = New Size(80, 30),
                .BackColor = Color.FromArgb(100, 100, 100),
                .ForeColor = Color.White
            }
            
            btnSkip = New Button() With {
                .Text = "SKIP",
                .Location = New Point(920, 690),
                .Size = New Size(80, 30),
                .BackColor = Color.FromArgb(100, 100, 100),
                .ForeColor = Color.White
            }
            
            Me.Controls.AddRange(New Control() {
                lblTitle, lblScene, txtDialogue, pnlChoices,
                pbProgress, btnPause, btnSkip
            })
        End Sub
        
        Private Sub SetupTimer()
            timer.Interval = 1000 ' 1 second
            timer.Enabled = True
        End Sub
        
        Private Sub UpdateDisplay()
            If gameEngine.CurrentEpisode Is Nothing OrElse
               gameEngine.CurrentEpisode.CurrentEpisode Is Nothing Then Return
            
            Dim ep = gameEngine.CurrentEpisode.CurrentEpisode
            lblTitle.Text = $"S{ep.Season:D2}E{ep.EpisodeNumber:D2} - {ep.Title}"
            pbProgress.Value = ep.CompletionPercentage
            
            ' Simulated scene progression
            Dim sceneText As String = ""
            Select Case gameEngine.CurrentEpisode.CurrentScene
                Case 0
                    sceneText = "Opening Scene: " & ep.Description
                Case 1
                    sceneText = "The Hood takes to the streets..."
                Case 2
                    sceneText = "Face-off with enemy forces..."
                Case 3
                    sceneText = "Dramatic revelation..."
                Case 4
                    sceneText = "Climactic battle..."
                Case 5
                    sceneText = "Episode conclusion..."
            End Select
            
            lblScene.Text = sceneText
        End Sub
        
        Private Sub timer_Tick(sender As Object, e As EventArgs) Handles timer.Tick
            If gameEngine.CurrentEpisode.IsPlaying AndAlso Not gameEngine.CurrentEpisode.IsPaused Then
                gameEngine.CurrentEpisode.SceneProgress += 1
                
                ' Advance scene after 30 seconds
                If gameEngine.CurrentEpisode.SceneProgress >= 30 Then
                    gameEngine.CurrentEpisode.CurrentScene += 1
                    gameEngine.CurrentEpisode.SceneProgress = 0
                    
                    ' Update episode completion
                    gameEngine.CurrentEpisode.CurrentEpisode.CompletionPercentage = 
                        CInt((gameEngine.CurrentEpisode.CurrentScene / 6) * 100)
                    
                    ' Check if episode complete
                    If gameEngine.CurrentEpisode.CurrentScene >= 6 Then
                        gameEngine.CompleteCurrentEpisode()
                        MessageBox.Show("Episode Complete!", "ARROW Saga", 
                                      MessageBoxButtons.OK, MessageBoxIcon.Information)
                        Me.Close()
                    End If
                End If
                
                UpdateDisplay()
            End If
        End Sub
        
        Private Sub btnPause_Click(sender As Object, e As EventArgs) Handles btnPause.Click
            gameEngine.CurrentEpisode.IsPaused = Not gameEngine.CurrentEpisode.IsPaused
            btnPause.Text = If(gameEngine.CurrentEpisode.IsPaused, "RESUME", "PAUSE")
        End Sub
        
        Private Sub btnSkip_Click(sender As Object, e As EventArgs) Handles btnSkip.Click
            gameEngine.CompleteCurrentEpisode()
            Me.Close()
        End Sub
    End Class
    
    Public Class TimelineForm
        Inherits Form
        
        Private gameEngine As ArrowSagaGame
        Private WithEvents lstTimeline As ListBox
        Private txtEventDetails As TextBox
        Private cmbSeasonFilter As ComboBox
        Private btnClose As Button
        
        Public Sub New(engine As ArrowSagaGame)
            Me.gameEngine = engine
            InitializeComponent()
            LoadTimeline()
        End Sub
        
        Private Sub InitializeComponent()
            Me.Text = "Arrow Timeline - Complete Series Chronology"
            Me.Size = New Size(900, 700)
            Me.StartPosition = FormStartPosition.CenterParent
            Me.BackColor = Color.FromArgb(40, 40, 40)
            
            ' Filter Combo
            cmbSeasonFilter = New ComboBox() With {
                .Location = New Point(20, 20),
                .Size = New Size(200, 30),
                .BackColor = Color.FromArgb(50, 50, 50),
                .ForeColor = Color.White,
                .Font = New Font("Arial", 11)
            }
            cmbSeasonFilter.Items.Add("All Seasons")
            For s = 1 To SEASON_COUNT
                cmbSeasonFilter.Items.Add($"Season {s}")
            Next
            cmbSeasonFilter.SelectedIndex = 0
            
            ' Timeline List
            lstTimeline = New ListBox() With {
                .Location = New Point(20, 60),
                .Size = New Size(400, 550),
                .BackColor = Color.FromArgb(50, 50, 50),
                .ForeColor = Color.White,
                .Font = New Font("Arial", 10)
            }
            
            ' Event Details
            txtEventDetails = New TextBox() With {
                .Location = New Point(440, 60),
                .Size = New Size(420, 550),
                .Multiline = True,
                .ReadOnly = True,
                .BackColor = Color.FromArgb(50, 50, 50),
                .ForeColor = Color.White,
                .Font = New Font("Arial", 10),
                .ScrollBars = ScrollBars.Vertical
            }
            
            ' Close Button
            btnClose = New Button() With {
                .Text = "CLOSE",
                .Location = New Point(400, 620),
                .Size = New Size(100, 30),
                .BackColor = Color.FromArgb(100, 100, 100),
                .ForeColor = Color.White
            }
            
            Me.Controls.AddRange(New Control() {
                cmbSeasonFilter, lstTimeline, txtEventDetails, btnClose
            })
        End Sub
        
        Private Sub LoadTimeline()
            lstTimeline.Items.Clear()
            Dim filterSeason As Integer? = If(cmbSeasonFilter.SelectedIndex > 0, cmbSeasonFilter.SelectedIndex, Nothing)
            
            Dim events = gameEngine.GetTimelineEvents(filterSeason)
            
            For Each evt In events
                lstTimeline.Items.Add($"S{evt.Season:D2}E{evt.Episode:D2}: {evt.Title}")
            Next
        End Sub
        
        Private Sub cmbSeasonFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbSeasonFilter.SelectedIndexChanged
            LoadTimeline()
        End Sub
        
        Private Sub lstTimeline_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstTimeline.SelectedIndexChanged
            If lstTimeline.SelectedIndex >= 0 Then
                Dim filterSeason As Integer? = If(cmbSeasonFilter.SelectedIndex > 0, cmbSeasonFilter.SelectedIndex, Nothing)
                Dim events = gameEngine.GetTimelineEvents(filterSeason)
                
                If lstTimeline.SelectedIndex < events.Count Then
                    Dim evt = events(lstTimeline.SelectedIndex)
                    txtEventDetails.Text = 
                        $"EVENT: {evt.Title}{vbCrLf}{vbCrLf}" &
                        $"Season: {evt.Season}, Episode: {evt.Episode}{vbCrLf}{vbCrLf}" &
                        $"Description: {evt.Description}{vbCrLf}{vbCrLf}" &
                        $"Location: {evt.Location}{vbCrLf}{vbCrLf}" &
                        $"Characters Involved: {String.Join(", ", evt.InvolvedCharacters)}{vbCrLf}{vbCrLf}" &
                        $"Crossover: {If(evt.IsCrossover, evt.CrossoverShow, "No")}{vbCrLf}{vbCrLf}" &
                        $"Consequences:{vbCrLf}{String.Join(vbCrLf, evt.Consequences)}"
                End If
            End If
        End Sub
        
        Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
            Me.Close()
        End Sub
    End Class
    
    Public Class CharacterDatabaseForm
        Inherits Form
        
        Private gameEngine As ArrowSagaGame
        Private WithEvents lstCharacters As ListBox
        Private txtCharacterInfo As TextBox
        Private pbRelationship As ProgressBar
        Private cmbFactionFilter As ComboBox
        Private btnClose As Button
        
        Public Sub New(engine As ArrowSagaGame)
            Me.gameEngine = engine
            InitializeComponent()
            LoadCharacters()
        End Sub
        
        Private Sub InitializeComponent()
            Me.Text = "Character Database - The Heroes and Villains of Star City"
            Me.Size = New Size(1000, 700)
            Me.StartPosition = FormStartPosition.CenterParent
            Me.BackColor = Color.FromArgb(40, 40, 40)
            
            ' Faction Filter
            cmbFactionFilter = New ComboBox() With {
                .Location = New Point(20, 20),
                .Size = New Size(200, 30),
                .BackColor = Color.FromArgb(50, 50, 50),
                .ForeColor = Color.White,
                .Font = New Font("Arial", 11)
            }
            cmbFactionFilter.Items.Add("All Characters")
            cmbFactionFilter.Items.Add("Team Arrow")
            cmbFactionFilter.Items.Add("Villains")
            cmbFactionFilter.Items.Add("SCPD")
            cmbFactionFilter.Items.Add("Family")
            cmbFactionFilter.Items.Add("Legends")
            cmbFactionFilter.SelectedIndex = 0
            
            ' Character List
            lstCharacters = New ListBox() With {
                .Location = New Point(20, 60),
                .Size = New Size(300, 550),
                .BackColor = Color.FromArgb(50, 50, 50),
                .ForeColor = Color.White,
                .Font = New Font("Arial", 10)
            }
            
            ' Character Info
            txtCharacterInfo = New TextBox() With {
                .Location = New Point(340, 60),
                .Size = New Size(620, 500),
                .Multiline = True,
                .ReadOnly = True,
                .BackColor = Color.FromArgb(50, 50, 50),
                .ForeColor = Color.White,
                .Font = New Font("Arial", 10),
                .ScrollBars = ScrollBars.Vertical
            }
            
            ' Relationship Progress Bar
            pbRelationship = New ProgressBar() With {
                .Location = New Point(340, 570),
                .Size = New Size(500, 20),
                .Style = ProgressBarStyle.Continuous,
                .Maximum = 100
            }
            
            ' Close Button
            btnClose = New Button() With {
                .Text = "CLOSE",
                .Location = New Point(850, 620),
                .Size = New Size(100, 30),
                .BackColor = Color.FromArgb(100, 100, 100),
                .ForeColor = Color.White
            }
            
            Me.Controls.AddRange(New Control() {
                cmbFactionFilter, lstCharacters, txtCharacterInfo,
                pbRelationship, btnClose
            })
        End Sub
        
        Private Sub LoadCharacters()
            lstCharacters.Items.Clear()
            
            For Each character In gameEngine.Database.Characters.Values
                ' Apply filter if needed
                If cmbFactionFilter.SelectedIndex > 0 Then
                    Dim filterText = cmbFactionFilter.SelectedItem.ToString()
                    If Not character.Faction.Contains(filterText.Replace("Team Arrow", "Team")) Then
                        Continue For
                    End If
                End If
                
                Dim status As String = If(character.IsAlive, "♥", "💀")
                lstCharacters.Items.Add($"{status} {character.GetDisplayName()} [{character.Faction}]")
            Next
        End Sub
        
        Private Sub cmbFactionFilter_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbFactionFilter.SelectedIndexChanged
            LoadCharacters()
        End Sub
        
        Private Sub lstCharacters_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstCharacters.SelectedIndexChanged
            If lstCharacters.SelectedIndex >= 0 Then
                Dim character = gameEngine.Database.Characters.Values.ElementAt(lstCharacters.SelectedIndex)
                
                Dim info As New StringBuilder()
                info.AppendLine($"NAME: {character.GetFullName()}")
                info.AppendLine($"ALIAS: {character.Alias}")
                info.AppendLine($"FACTION: {character.Faction}")
                info.AppendLine($"STATUS: {If(character.IsAlive, "Alive", "Deceased")}")
                If Not character.IsAlive AndAlso character.DeathEpisode IsNot Nothing Then
                    info.AppendLine($"DIED: {character.DeathEpisode.Title}")
                End If
                info.AppendLine()
                info.AppendLine("ABILITIES:")
                For Each ability In character.Abilities
                    info.AppendLine($"  • {ability}")
                Next
                info.AppendLine()
                info.AppendLine("KEY RELATIONSHIPS:")
                For Each rel In character.Relationships.Take(10)
                    Dim otherChar = gameEngine.Database.Characters(rel.Key)
                    info.AppendLine($"  • {otherChar.GetDisplayName()}: {rel.Value}")
                Next
                
                txtCharacterInfo.Text = info.ToString()
                
                ' Update relationship with player
                Dim playerRel = gameEngine.GetCharacterRelationship(CharacterID.OliverQueen, character.ID)
                pbRelationship.Value = Math.Min(100, Math.Max(0, playerRel + 50))
            End If
        End Sub
        
        Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
            Me.Close()
        End Sub
    End Class
    
    #End Region
    
    #Region "Program Entry Point"
    
    Public Shared Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        
        ' Show splash screen
        Using splash As New SplashScreenForm()
            splash.ShowDialog()
        End Using
        
        ' Initialize game engine
        Dim engine As New ArrowSagaGame()
        
        ' Show main menu
        Application.Run(New MainMenuForm(engine))
    End Sub
    
    #End Region
End Class

Public Class SplashScreenForm
    Inherits Form
    
    Private WithEvents timer As New Timer()
    Private alpha As Integer = 0
    Private direction As Integer = 1
    
    Public Sub New()
        Me.FormBorderStyle = FormBorderStyle.None
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.Size = New Size(800, 400)
        Me.BackColor = Color.Black
        Me.TopMost = True
        
        timer.Interval = 30
        timer.Enabled = True
    End Sub
    
    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        
        Dim g = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = TextRenderingHint.AntiAlias
        
        ' Draw background
        Using gradientBrush As New LinearGradientBrush(
            Me.ClientRectangle,
            Color.FromArgb(0, 50, 0),
            Color.Black,
            LinearGradientMode.Vertical)
            g.FillRectangle(gradientBrush, Me.ClientRectangle)
        End Using
        
        ' Draw logo
        Dim titleFont As New Font("Arial Black", 48, FontStyle.Bold)
        Dim subtitleFont As New Font("Arial", 16)
        
        Dim titleSize = g.MeasureString("ARROW", titleFont)
        Dim titleX = (Me.Width - titleSize.Width) / 2
        Dim titleY = 100
        
        Using brush As New SolidBrush(Color.FromArgb(alpha, 0, 255, 0))
            g.DrawString("ARROW", titleFont, brush, titleX, titleY)
        End Using
        
        Dim subtitleSize = g.MeasureString("THE COMPLETE SAGA", subtitleFont)
        Dim subtitleX = (Me.Width - subtitleSize.Width) / 2
        
        Using brush As New SolidBrush(Color.FromArgb(alpha, 255, 255, 255))
            g.DrawString("THE COMPLETE SAGA", subtitleFont, brush, subtitleX, titleY + 80)
        End Using
        
        ' Draw loading text
        Dim loadingFont As New Font("Arial", 10)
        Dim loadingText = "Loading Star City..."
        Dim loadingSize = g.MeasureString(loadingText, loadingFont)
        Dim loadingX = (Me.Width - loadingSize.Width) / 2
        
        Using brush As New SolidBrush(Color.FromArgb(alpha, 150, 150, 150))
            g.DrawString(loadingText, loadingFont, brush, loadingX, Me.Height - 50)
        End Using
        
        ' Draw version
        Dim versionText = "Engine Version " & ArrowSagaEngine.ENGINE_VERSION
        Dim versionSize = g.MeasureString(versionText, loadingFont)
        Using brush As New SolidBrush(Color.FromArgb(alpha, 100, 100, 100))
            g.DrawString(versionText, loadingFont, brush, Me.Width - versionSize.Width - 20, Me.Height - 30)
        End Using
    End Sub
    
    Private Sub timer_Tick(sender As Object, e As EventArgs) Handles timer.Tick
        alpha += direction * 5
        
        If alpha >= 255 Then
            alpha = 255
            direction = -1
        ElseIf alpha <= 0 Then
            timer.Enabled = False
            Me.Close()
        End If
        
        Me.Invalidate()
    End Sub
End Class