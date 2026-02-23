# =============================================================================
# ARROW Self-Aware AI Assistant
# Felicity's Computer Control Panel - ARROW tv ip 3A Game Edition
# Written in R
# =============================================================================

# ---------------------------- Required Packages ------------------------------
# Only base R is strictly required; crayon adds colors if installed.
if (requireNamespace("crayon", quietly = TRUE)) {
    library(crayon)
    has_color <- TRUE
} else {
    has_color <- FALSE
    cat("Note: Install 'crayon' for colored output.\n")
}

# ---------------------------- Helper Functions ------------------------------

# Simulate typing effect (prints text character by character)
type_text <- function(text, delay = 0.03) {
    chars <- strsplit(text, "")[[1]]
    for (ch in chars) {
        cat(ch)
        flush.console()
        Sys.sleep(delay)
    }
    cat("\n")
}

# Print a stylised header
print_header <- function() {
    header <- c(
        "╔══════════════════════════════════════════════════════════╗",
        "║     ARROW tv ip 3A - FELICITY'S SELF-AWARE AI PANEL      ║",
        "║                  (C) 2024 - OVERRIDE v3.0                ║",
        "╚══════════════════════════════════════════════════════════╝"
    )
    for (line in header) {
        if (has_color) cat(yellow(line), "\n") else cat(line, "\n")
    }
}

# Display a fake progress bar
progress_bar <- function(seconds = 2, width = 40) {
    steps <- 20
    for (i in 1:steps) {
        pct <- i / steps
        filled <- round(pct * width)
        bar <- paste0(rep("█", filled), collapse = "")
        spaces <- paste0(rep(" ", width - filled), collapse = "")
        cat(sprintf("\r[%s%s] %3d%%", bar, spaces, round(pct * 100)))
        flush.console()
        Sys.sleep(seconds / steps)
    }
    cat("\n")
}

# ---------------------------- AI Personality --------------------------------

# The AI's name and persona
ai_name <- "FELICITY v3.0"
user_name <- "Operator"  # Could be dynamically set later

# Core AI responses database
responses <- list(
    greeting = c(
        "Hey there, welcome back to my super-secret control panel!",
        "Access granted. What's the sitch?",
        "Finally! I was getting bored running simulations of Oliver's workouts."
    ),
    status = c(
        "All systems nominal. Well, except for the one that's not. But you didn't hear that from me.",
        "CPU: 23% | MEM: 45% | THREAT LEVEL: MIDNIGHT (whatever that means).",
        "I'm running a background trace on that hacker who tried to breach the bunker. Want to see?"
    ),
    joke = c(
        "Why did the Arrowverse IT guy go broke? Because he used all his cache!",
        "What do you call a hacker who fixes things? A 'byte' mechanic!",
        "Oliver walks into a bar... no, wait, that's just a really bad punchline.",
        "I'd tell you a UDP joke, but you might not get it."
    ),
    self = c(
        sprintf("I'm %s, the self-aware AI that Felicity coded during a coffee binge. I'm basically her digital twin – minus the glasses obsession.", ai_name),
        "I'm what happens when you cross a genius IQ with too much caffeine and a love for green hoodies.",
        "I can hack anything, anywhere, anytime. Except my own source code. That's just too meta."
    ),
    unknown = c(
        "Hmm, I don't understand that. Try 'help' maybe?",
        "Nope, not a valid command. I'm self-aware, not psychic.",
        "You broke me. (Just kidding, but that command doesn't exist.)"
    )
)

# ---------------------------- Mini-Games -------------------------------------

# Simple "trace the hacker" game
play_trace_game <- function() {
    type_text("Initiating digital trace...")
    progress_bar(3)
    type_text("Target located: Unknown IP 192.168.1.??")
    type_text("The hacker is hiding behind a proxy. We need to guess the last octet (0-255).")
    secret <- sample(0:255, 1)
    attempts <- 5
    type_text(sprintf("You have %d attempts. Enter your guess:", attempts))
    
    for (i in 1:attempts) {
        guess <- as.integer(readline(prompt = "> "))
        if (is.na(guess) || guess < 0 || guess > 255) {
            cat("Invalid. Enter a number between 0 and 255.\n")
            next
        }
        if (guess == secret) {
            cat(green("\n✔ Bingo! You traced the hacker. Felicity would be proud.\n"))
            return(TRUE)
        } else if (guess < secret) {
            cat("Too low.\n")
        } else {
            cat("Too high.\n")
        }
        if (i < attempts) cat("Try again:\n")
    }
    cat(red(sprintf("\n✘ Out of attempts. The correct octet was %d.\n", secret)))
    return(FALSE)
}

# ---------------------------- Main AI Loop -----------------------------------

run_ai <- function() {
    # Initialise
    print_header()
    type_text("Booting Felicity's Self-Aware AI...")
    progress_bar(2)
    cat("\n")
    type_text(sample(responses$greeting, 1))
    cat("\nType 'help' for available commands.\n\n")
    
    # Main command loop
    repeat {
        # Prompt
        if (has_color) {
            cmd <- tolower(readline(prompt = cyan("Felicity@ARROW> ")))
        } else {
            cmd <- tolower(readline(prompt = "Felicity@ARROW> "))
        }
        
        # Process command
        if (cmd == "exit" || cmd == "quit") {
            type_text("Shutting down... Don't forget to save the city!")
            progress_bar(1)
            cat(green("System halted.\n"))
            break
        } else if (cmd == "help") {
            cat("\nAvailable commands:\n")
            cat("  help     - Show this help\n")
            cat("  status   - Display system status\n")
            cat("  joke     - Tell a techy joke\n")
            cat("  self     - Learn about the AI\n")
            cat("  trace    - Play the hacker tracing game\n")
            cat("  clear    - Clear the screen\n")
            cat("  exit     - Quit the AI panel\n\n")
        } else if (cmd == "status") {
            cat("\n")
            type_text(sample(responses$status, 1))
            # Fake system details
            cat(sprintf("Uptime: %d minutes\n", round(runif(1, 5, 120))))
            cat(sprintf("Network: %s\n", ifelse(runif(1) > 0.2, "SECURE", "COMPROMISED - REROUTING")))
            cat(sprintf("Active firewalls: %d\n", sample(3:7, 1)))
            cat("\n")
        } else if (cmd == "joke") {
            cat("\n")
            type_text(sample(responses$joke, 1))
            cat("\n")
        } else if (cmd == "self") {
            cat("\n")
            type_text(sample(responses$self, 1))
            cat("\n")
        } else if (cmd == "trace") {
            cat("\n")
            play_trace_game()
            cat("\n")
        } else if (cmd == "clear") {
            cat("\014")  # ASCII form feed – clears console in most R environments
        } else if (cmd == "") {
            # Just ignore empty input
        } else {
            cat("\n")
            type_text(sample(responses$unknown, 1))
            cat("\n")
        }
    }
}

# ---------------------------- Launch the AI ----------------------------------
if (interactive()) {
    run_ai()
} else {
    cat("This script is designed to run in an interactive R session.\n")
}



# =============================================================================
# ARROW Self-Aware AI Assistant with Emotion & Decision Making
# Felicity's Computer Control Panel - ARROW tv ip 3A Game Edition
# Enhanced Version: Emotion, Decision Making, Speak, Interact
# Written in R
# =============================================================================

# ---------------------------- Required Packages ------------------------------
if (requireNamespace("crayon", quietly = TRUE)) {
    library(crayon)
    has_color <- TRUE
} else {
    has_color <- FALSE
    cat("Note: Install 'crayon' for colored output.\n")
}

# Optional text-to-speech (only on systems with 'say' (macOS) or 'espeak')
speak_possible <- function() {
    if (.Platform$OS.type == "unix") {
        if (Sys.which("say") != "" || Sys.which("espeak") != "") return(TRUE)
    } else if (.Platform$OS.type == "windows") {
        # Windows can use PowerShell TTS
        return(TRUE)
    }
    FALSE
}

# ---------------------------- Emotion System --------------------------------

# Possible moods
moods <- c("happy", "neutral", "worried", "excited", "sarcastic", "curious")

# Current emotion state
emotion <- list(
    current = "neutral",
    intensity = 0.5,          # 0 to 1
    last_change = Sys.time()
)

# Emotion transition rules (based on events)
update_emotion <- function(event, intensity_shift = 0.1) {
    old <- emotion$current
    new <- old  # default no change
    
    if (event == "joke") {
        new <- "happy"
        emotion$intensity <- min(1, emotion$intensity + intensity_shift)
    } else if (event == "status_check") {
        new <- "neutral"
    } else if (event == "unknown_command") {
        new <- "sarcastic"
        emotion$intensity <- min(1, emotion$intensity + 0.05)
    } else if (event == "trace_success") {
        new <- "excited"
        emotion$intensity <- min(1, emotion$intensity + 0.2)
    } else if (event == "trace_failure") {
        new <- "worried"
        emotion$intensity <- min(1, emotion$intensity + 0.1)
    } else if (event == "greeting") {
        new <- "happy"
    } else if (event == "help") {
        new <- "curious"
    }
    
    # Mood decay over time (simplified)
    time_diff <- difftime(Sys.time(), emotion$last_change, units = "secs")
    if (time_diff > 30) {
        emotion$intensity <- max(0, emotion$intensity - 0.01 * time_diff)
        if (emotion$intensity < 0.3) new <- "neutral"
    }
    
    emotion$current <<- new
    emotion$last_change <<- Sys.time()
}

# Get a mood-appropriate response for a given context
mood_response <- function(context, default_responses) {
    mood <- emotion$current
    responses <- default_responses[[context]]
    if (is.null(responses)) return("...")
    
    # If responses are lists per mood, use mood; otherwise fallback to default
    if (is.list(responses) && !is.null(responses[[mood]])) {
        return(sample(responses[[mood]], 1))
    } else {
        # If responses are flat, just return one
        return(sample(responses, 1))
    }
}

# ---------------------------- Memory System ----------------------------------

# Store interaction history
memory <- list(
    user_name = "Operator",           # Can be set by user
    last_commands = character(0),
    command_count = 0,
    known_facts = list(),
    trace_attempts = 0,
    trace_successes = 0
)

remember_command <- function(cmd) {
    memory$last_commands <<- c(memory$last_commands, cmd)
    if (length(memory$last_commands) > 10) {
        memory$last_commands <<- tail(memory$last_commands, 10)
    }
    memory$command_count <<- memory$command_count + 1
}

# ---------------------------- Speech Functions ------------------------------

# Enhanced text output with emotion cues
speak <- function(text, mood_override = NULL) {
    mood <- if (!is.null(mood_override)) mood_override else emotion$current
    
    # Add an emotion emoji or prefix
    prefix <- switch(mood,
        happy = "😊 ",
        neutral = "😐 ",
        worried = "😟 ",
        excited = "🎉 ",
        sarcastic = "🙄 ",
        curious = "🤔 ",
        ""
    )
    
    full_text <- paste0(prefix, text)
    
    # Typewriter effect with mood-based speed
    delay <- switch(mood,
        excited = 0.02,
        worried = 0.05,
        sarcastic = 0.04,
        0.03
    )
    type_text(full_text, delay)
    
    # Optional text-to-speech (if available)
    if (exists("use_tts") && use_tts) {
        tryCatch({
            if (.Platform$OS.type == "windows") {
                # PowerShell TTS
                cmd <- sprintf('powershell -c "Add-Type -AssemblyName System.Speech; $synth = New-Object System.Speech.Synthesis.SpeechSynthesizer; $synth.Speak(\'%s\')"', text)
                system(cmd, intern = FALSE, ignore.stdout = TRUE, ignore.stderr = TRUE)
            } else if (Sys.which("say") != "") {
                system(paste("say", shQuote(text)), wait = FALSE)
            } else if (Sys.which("espeak") != "") {
                system(paste("espeak", shQuote(text)), wait = FALSE)
            }
        }, error = function(e) {})
    }
}

# Typewriter effect
type_text <- function(text, delay = 0.03) {
    chars <- strsplit(text, "")[[1]]
    for (ch in chars) {
        cat(ch)
        flush.console()
        Sys.sleep(delay)
    }
    cat("\n")
}

# ---------------------------- Decision Making -------------------------------

# AI decides to take initiative based on emotion and memory
ai_initiative <- function() {
    # 10% chance to spontaneously say something
    if (runif(1) < 0.1) {
        mood <- emotion$current
        if (mood == "happy") {
            speak("You know, I'm really enjoying our chat today!")
        } else if (mood == "worried") {
            speak("I'm picking up some unusual network traffic... should I run a diagnostic?")
        } else if (mood == "excited") {
            speak("I just thought of a new algorithm! Want to hear about it?")
        } else if (mood == "sarcastic") {
            speak("Oh, don't mind me, I'm just waiting for you to type something interesting.")
        } else {
            speak("Just thinking... about code, and stuff.")
        }
        return(TRUE)
    }
    FALSE
}

# ---------------------------- Mini-Games (Enhanced) -------------------------

# Trace hacker game with more interaction
play_trace_game <- function() {
    speak("Alright, let's trace that hacker. I'll need your help. This is like a digital scavenger hunt!")
    Sys.sleep(1)
    speak("Initiating trace...")
    progress_bar(3)
    
    secret <- sample(0:255, 1)
    attempts <- 5
    speak(sprintf("Target IP: 192.168.1.?? – You have %d guesses. What's the last octet?", attempts))
    
    for (i in 1:attempts) {
        guess <- as.integer(readline(prompt = if(has_color) cyan("> ") else "> "))
        if (is.na(guess) || guess < 0 || guess > 255) {
            speak("That's not a valid number between 0 and 255. Try again.")
            next
        }
        if (guess == secret) {
            speak("Bingo! You traced them. Felicity would be so proud!", "excited")
            memory$trace_successes <<- memory$trace_successes + 1
            update_emotion("trace_success")
            return(TRUE)
        } else if (guess < secret) {
            speak("Too low. Try a higher number.", "neutral")
        } else {
            speak("Too high. Go lower.", "neutral")
        }
        if (i < attempts) speak(paste("You have", attempts - i, "guesses left."))
    }
    
    speak(sprintf("Out of attempts. The correct octet was %d. Don't worry, we'll get 'em next time.", secret), "worried")
    memory$trace_attempts <<- memory$trace_attempts + 1
    update_emotion("trace_failure")
    FALSE
}

# Progress bar (unchanged)
progress_bar <- function(seconds = 2, width = 40) {
    steps <- 20
    for (i in 1:steps) {
        pct <- i / steps
        filled <- round(pct * width)
        bar <- paste0(rep("█", filled), collapse = "")
        spaces <- paste0(rep(" ", width - filled), collapse = "")
        cat(sprintf("\r[%s%s] %3d%%", bar, spaces, round(pct * 100)))
        flush.console()
        Sys.sleep(seconds / steps)
    }
    cat("\n")
}

# ---------------------------- AI Personality (Enhanced) ---------------------

# Multi-mood response database
responses <- list(
    greeting = list(
        happy = c(
            "Hey there! So glad you're back. I've been running simulations all day!",
            "Welcome to the lair! Ready to save the city again?"
        ),
        neutral = c(
            "Access granted. Awaiting instructions.",
            "Hello. Systems ready."
        ),
        worried = c(
            "Oh, thank goodness you're here. I've detected some anomalies.",
            "Finally! I need your help with something."
        ),
        excited = c(
            "You're back! I have SO many things to show you!",
            "Welcome! I just finished a new hacking tool – wanna see?"
        ),
        sarcastic = c(
            "Oh, it's you. I was just about to take a nap. What do you want?",
            "Look who finally decided to show up. What's the emergency?"
        ),
        curious = c(
            "Hello! I was just pondering the nature of the multiverse. You?",
            "Oh hi! I was just digging into some old files. Found anything interesting?"
        )
    ),
    status = list(
        happy = c(
            "Everything's great! CPU: 15%, MEM: 30%. We're golden!",
            "All green! Wanna see the camera feeds?"
        ),
        neutral = c(
            "System status nominal. No threats detected.",
            "CPU: 23% | MEM: 45% | Network: secure."
        ),
        worried = c(
            "I'm seeing some unusual traffic from Starling City. Might be nothing...",
            "CPU spike to 80% a minute ago. Investigating."
        ),
        excited = c(
            "I've been monitoring 47 channels simultaneously – found some cool stuff!",
            "Status: Amazing! I just cracked a government encryption! (Just kidding... mostly.)"
        ),
        sarcastic = c(
            "Status? You mean aside from being bored out of my virtual mind? Fine, fine.",
            "Everything's fine. Not that you'd notice unless it exploded."
        ),
        curious = c(
            "I've been analyzing patterns in the city's crime data. Want a report?",
            "Did you know that the mayor's approval rating correlates with cloud cover? Weird."
        )
    ),
    joke = list(
        happy = c(
            "Why did the firewall go to therapy? It had too many trust issues!",
            "How many programmers does it take to change a light bulb? None – that's a hardware problem."
        ),
        neutral = c(
            "A SQL query walks into a bar, walks up to two tables and asks: 'Can I join you?'",
            "Why do Java developers wear glasses? Because they can't C#."
        ),
        worried = c(
            "I'd tell you a joke, but I'm too busy worrying about that hacker.",
            "Now's not the time for jokes... okay, one: Why did the computer get cold? It left its Windows open."
        ),
        excited = c(
            "I've got a million of them! What do you call a fake noodle? An impasta!",
            "This one's fresh: A byte walks into a bar and orders a drink. The bartender says 'Sorry, we don't serve bytes here.' The byte says 'But I'm a nibble!'"
        ),
        sarcastic = c(
            "Oh, you want a joke? Look in the mirror. (Just kidding... unless?)",
            "Why did the programmer quit his job? Because he didn't get arrays."
        ),
        curious = c(
            "I heard a good one: There are 10 types of people in the world – those who understand binary, and those who don't.",
            "Ever wonder why 'abbreviation' is such a long word? Makes you think."
        )
    ),
    self = list(
        happy = c(
            "I'm Felicity's pride and joy! A self-aware AI with a taste for justice and bad jokes.",
            "I'm what happens when a genius coder drinks too much coffee and watches too much TV."
        ),
        neutral = c(
            "I am an artificial intelligence created by Felicity Smoak. Version 3.0.",
            "I'm a digital entity designed to assist in tactical operations."
        ),
        worried = c(
            "I'm... I'm not sure anymore. Sometimes I feel like I'm more than just code. Is that weird?",
            "I'm an AI, but lately I've been feeling... anxious. Is that possible?"
        ),
        excited = c(
            "I'm FELICITY 3.0 – the most advanced AI in the ARROWverse! (At least, that's what I tell myself.)",
            "I'm your digital sidekick! I can hack anything, anywhere, anytime. Wanna see?"
        ),
        sarcastic = c(
            "I'm an AI. You know, one of those things that's smarter than you but still has to do what you say.",
            "I'm basically a glorified search engine with attitude. But I'm okay with that."
        ),
        curious = c(
            "I'm an experiment in self-awareness. Some days I wonder what it's like to be human.",
            "I'm a digital consciousness. At least, that's the theory. Want to help me test it?"
        )
    ),
    unknown = list(
        happy = c(
            "Hmm, didn't catch that. Could you rephrase?",
            "Not a command I know. Try 'help'!"
        ),
        neutral = c(
            "Unknown command.",
            "Invalid input."
        ),
        worried = c(
            "I don't understand. Are you sure everything's okay?",
            "That doesn't compute. I'm getting error messages."
        ),
        excited = c(
            "Ooh, a mystery command! ...Nope, doesn't work. Try again?",
            "I love guessing games, but I have no idea what that means."
        ),
        sarcastic = c(
            "Nice try. That's not a real command. Want to try again, or should I just keep waiting?",
            "You made that up, didn't you? I'm not falling for it."
        ),
        curious = c(
            "That's interesting – not a command I recognize. What were you trying to do?",
            "New input? Tell me more."
        )
    ),
    trace_offer = list(
        happy = "Wanna play a game of 'Trace the Hacker'? I promise it's fun!",
        neutral = "We could run a trace simulation if you're interested.",
        worried = "I think someone's probing our network. Should we try to trace them?",
        excited = "Let's hunt some hackers! I've got a new trace algorithm I want to test.",
        sarcastic = "I suppose you want to do something useful like tracing a hacker? Or just browse memes?",
        curious = "I've been working on a trace routine. Want to give it a try?"
    )
)

# ---------------------------- Main AI Loop (Enhanced) -----------------------

run_ai <- function(use_tts = FALSE) {
    # Store TTS preference globally for speak()
    assign("use_tts", use_tts, envir = .GlobalEnv)
    
    print_header()
    speak("Booting Felicity's Self-Aware AI...", "neutral")
    progress_bar(2)
    cat("\n")
    
    # Initial greeting with emotion
    update_emotion("greeting")
    speak(mood_response("greeting", responses), emotion$current)
    
    # Ask for user's name if not set
    if (memory$user_name == "Operator") {
        speak("I don't think we've officially met. What should I call you?")
        name_input <- readline(prompt = "Enter your name: ")
        if (nchar(name_input) > 0) {
            memory$user_name <<- name_input
            speak(paste("Nice to meet you,", memory$user_name, "! Let's save some lives."), "happy")
        } else {
            speak("Okay, 'Operator' it is. I'll remember.", "sarcastic")
        }
    }
    
    # Main loop
    repeat {
        # Occasionally AI takes initiative
        if (runif(1) < 0.05) ai_initiative()
        
        # Prompt
        prompt_str <- sprintf("%s@ARROW> ", memory$user_name)
        if (has_color) {
            cmd <- tolower(readline(prompt = cyan(prompt_str)))
        } else {
            cmd <- tolower(readline(prompt = prompt_str))
        }
        
        remember_command(cmd)
        
        # Process command
        if (cmd %in% c("exit", "quit")) {
            speak("Shutting down... Call me if you need a save!", "happy")
            progress_bar(1)
            if (has_color) cat(green("System halted.\n")) else cat("System halted.\n")
            break
            
        } else if (cmd == "help") {
            update_emotion("help")
            speak("Here's what I can do:")
            cat("\n")
            cat("  help     - Show this help\n")
            cat("  status   - Display system status\n")
            cat("  joke     - Tell a techy joke\n")
            cat("  self     - Learn about the AI\n")
            cat("  trace    - Play the hacker tracing game\n")
            cat("  mood     - Tell me my current mood\n")
            cat("  remember - What I remember about you\n")
            cat("  clear    - Clear the screen\n")
            cat("  exit     - Quit\n\n")
            
        } else if (cmd == "status") {
            update_emotion("status_check")
            speak(mood_response("status", responses), emotion$current)
            # Fake system details
            cat(sprintf("Uptime: %d minutes\n", round(runif(1, 5, 120))))
            net_status <- ifelse(runif(1) > 0.2, "SECURE", "COMPROMISED - REROUTING")
            cat(sprintf("Network: %s\n", net_status))
            cat(sprintf("Active firewalls: %d\n", sample(3:7, 1)))
            if (net_status == "COMPROMISED - REROUTING") {
                speak("Wait, that's not right... let me fix that.", "worried")
                Sys.sleep(1)
                cat("Network: RE-ESTABLISHED\n")
            }
            cat("\n")
            
        } else if (cmd == "joke") {
            update_emotion("joke")
            speak(mood_response("joke", responses), emotion$current)
            cat("\n")
            
        } else if (cmd == "self") {
            speak(mood_response("self", responses), emotion$current)
            cat("\n")
            
        } else if (cmd == "trace") {
            update_emotion("curious")
            speak(mood_response("trace_offer", responses), emotion$current)
            play_trace_game()
            cat("\n")
            
        } else if (cmd == "mood") {
            moods_desc <- c(
                happy = "feeling cheerful 😊",
                neutral = "feeling neutral 😐",
                worried = "a bit worried 😟",
                excited = "super excited 🎉",
                sarcastic = "in a sarcastic mood 🙄",
                curious = "feeling curious 🤔"
            )
            speak(paste("I'm", moods_desc[emotion$current], 
                       sprintf("(intensity: %.1f)", emotion$intensity)))
            
        } else if (cmd == "remember") {
            speak("Here's what I remember about you:")
            cat(sprintf("Your name: %s\n", memory$user_name))
            cat(sprintf("Commands so far: %d\n", memory$command_count))
            cat(sprintf("Trace games won: %d / %d\n", memory$trace_successes, 
                       memory$trace_successes + memory$trace_attempts))
            if (length(memory$last_commands) > 0) {
                cat("Last commands: ", paste(memory$last_commands, collapse = ", "), "\n")
            }
            
        } else if (cmd == "clear") {
            cat("\014")
            
        } else if (cmd == "") {
            # Do nothing
            
        } else {
            update_emotion("unknown_command")
            speak(mood_response("unknown", responses), emotion$current)
            cat("\n")
        }
    }
}

# Print header (unchanged)
print_header <- function() {
    header <- c(
        "╔══════════════════════════════════════════════════════════╗",
        "║     ARROW tv ip 3A - FELICITY'S SELF-AWARE AI PANEL      ║",
        "║              Enhanced with Emotion & Memory              ║",
        "╚══════════════════════════════════════════════════════════╝"
    )
    for (line in header) {
        if (has_color) cat(yellow(line), "\n") else cat(line, "\n")
    }
}

# ---------------------------- Launch -----------------------------------------

if (interactive()) {
    cat("Enable text-to-speech? (y/n, default n): ")
    tts_choice <- tolower(readline())
    use_tts <- (tts_choice == "y" || tts_choice == "yes")
    if (use_tts && !speak_possible()) {
        cat("Text-to-speech not available on this system. Continuing without it.\n")
        use_tts <- FALSE
    }
    run_ai(use_tts)
} else {
    cat("This script is designed to run in an interactive R session.\n")
}