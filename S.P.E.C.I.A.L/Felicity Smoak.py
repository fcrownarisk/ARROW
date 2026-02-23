#!/usr/bin/env python3
"""
Felicity Smoak (Emily Bett Rickards) - S.P.E.C.I.A.L. Skill Tree
Inspired by Fallout 4's Pip-Boy
"""

import sys
from typing import List, Dict, Optional

# ------------------------- Perk Class -------------------------
class Perk:
    """A single perk in the skill tree."""
    def __init__(self, name: str, description: str, rank: int, max_rank: int,
                 special_requirement: str, special_value: int,
                 level_requirement: int = 1,
                 required_perk: Optional['Perk'] = None):
        self.name = name
        self.description = description
        self.rank = rank                # current rank (0 if not taken)
        self.max_rank = max_rank
        self.special_requirement = special_requirement  # e.g. 'I'
        self.special_value = special_value              # required attribute value
        self.level_requirement = level_requirement
        self.required_perk = required_perk

    def can_take(self, character) -> bool:
        """Check if character meets requirements for next rank."""
        if self.rank >= self.max_rank:
            return False
        attr_value = character.special[self.special_requirement].value
        if attr_value < self.special_value:
            return False
        if character.level < self.level_requirement:
            return False
        if self.required_perk and self.required_perk.rank < 1:
            return False
        return True

    def take_rank(self):
        """Increase rank by one."""
        if self.rank < self.max_rank:
            self.rank += 1

    def __str__(self):
        stars = '*' * self.rank + 'o' * (self.max_rank - self.rank)
        return f"{self.name} [{stars}] {self.description}"


# ------------------------- SpecialAttribute Class -------------------------
class SpecialAttribute:
    """One of the seven S.P.E.C.I.A.L. attributes."""
    def __init__(self, name: str, short: str, value: int, perks: List[Perk]):
        self.name = name
        self.short = short
        self.value = value
        self.perks = perks              # list of perks under this attribute

    def increase(self):
        """Increase attribute by one (max 10)."""
        if self.value < 10:
            self.value += 1

    def __str__(self):
        return f"{self.short}: {self.value}"


# ------------------------- Character Class -------------------------
class FelicitySmoak:
    """Represents Felicity Smoak with her S.P.E.C.I.A.L. stats and perks."""
    def __init__(self):
        self.name = "Felicity Smoak"
        self.actor = "Emily Bett Rickards"
        self.level = 1
        self.xp = 0
        self.perk_points = 0

        # Define S.P.E.C.I.A.L. base values (based on character)
        self.special = {
            'S': SpecialAttribute("Strength", "S", 3, self._create_strength_perks()),
            'P': SpecialAttribute("Perception", "P", 8, self._create_perception_perks()),
            'E': SpecialAttribute("Endurance", "E", 4, self._create_endurance_perks()),
            'C': SpecialAttribute("Charisma", "C", 7, self._create_charisma_perks()),
            'I': SpecialAttribute("Intelligence", "I", 10, self._create_intelligence_perks()),
            'A': SpecialAttribute("Agility", "A", 5, self._create_agility_perks()),
            'L': SpecialAttribute("Luck", "L", 6, self._create_luck_perks())
        }

    # ------------------------- Perk Definitions -------------------------
    def _create_strength_perks(self) -> List[Perk]:
        return [
            Perk("Light Load", "Carry weight increased by +10 per rank", 0, 2, 'S', 2, 2),
            Perk("Coffee Run", "Sprinting costs 20% less AP", 0, 1, 'S', 3, 5),
            Perk("Unbreakable", "Equipment degrades 30% slower", 0, 1, 'S', 4, 10),
            Perk("Tough as Nails", "You can endure one extra hit before going down", 0, 1, 'S', 5, 15),
            Perk("Not a Fighter", "Unarmed attacks now do +50% damage (but why?)", 0, 1, 'S', 6, 20),
        ]

    def _create_perception_perks(self) -> List[Perk]:
        return [
            Perk("Detail Oriented", "Highlight interactive objects in the environment", 0, 1, 'P', 3, 1),
            Perk("Network Eyes", "Detect enemies through walls if they are on a network", 0, 2, 'P', 5, 4),
            Perk("Code Sense", "Identify security levels of terminals from a distance", 0, 1, 'P', 6, 8),
            Perk("Pattern Recognition", "Spot anomalies in data 25% faster per rank", 0, 2, 'P', 7, 12),
            Perk("Sixth Sense", "Occasionally predict enemy movements", 0, 1, 'P', 9, 18),
        ]

    def _create_endurance_perks(self) -> List[Perk]:
        return [
            Perk("Caffeine Dependency", "Coffee now restores +20 HP", 0, 1, 'E', 2, 2),
            Perk("Late Night Hacker", "Sleep deprivation penalties reduced by 50%", 0, 1, 'E', 3, 5),
            Perk("Resilient Spirit", "+10 HP per rank", 0, 3, 'E', 4, 8),
            Perk("Adrenaline Spike", "When health below 30%, hacking speed doubles", 0, 1, 'E', 5, 12),
            Perk("Indomitable", "Ignore pain effects while focused on a terminal", 0, 1, 'E', 6, 18),
        ]

    def _create_charisma_perks(self) -> List[Perk]:
        return [
            Perk("Geek Speak", "Better dialogue options with tech‑savvy NPCs", 0, 1, 'C', 3, 1),
            Perk("Motivator", "Allies gain +5% damage when you're in the party", 0, 2, 'C', 4, 4),
            Perk("Sarcastic Wit", "Sarcastic dialogue options become 20% more effective", 0, 1, 'C', 5, 7),
            Perk("Team Player", "Followers gain +50% XP from your hacking successes", 0, 1, 'C', 6, 10),
            Perk("Heart of Gold", "Persuasion chance increased by 25%", 0, 1, 'C', 8, 15),
        ]

    def _create_intelligence_perks(self) -> List[Perk]:
        return [
            Perk("Hacker", "Bypass novice terminals", 0, 1, 'I', 4, 2),
            Perk("Expert Hacker", "Bypass advanced terminals", 0, 1, 'I', 6, 5, self.special['I'].perks[0]),
            Perk("Master Hacker", "Bypass expert terminals", 0, 1, 'I', 8, 10, self.special['I'].perks[1]),
            Perk("Overclocker", "Hacking minigame speed increased by 50%", 0, 2, 'I', 5, 4),
            Perk("Quantum Processor", "Simulate multiple code paths – reroll failed hacks once per day", 0, 1, 'I', 9, 15),
            Perk("Tech Savvy", "Repair items for 50% fewer resources", 0, 2, 'I', 4, 3),
            Perk("Guru", "All intelligence‑based skill checks are easier", 0, 1, 'I', 10, 20),
        ]

    def _create_agility_perks(self) -> List[Perk]:
        return [
            Perk("Quick Fingers", "Lockpicking and hacking done 25% faster per rank", 0, 2, 'A', 3, 2),
            Perk("Stealthy Typing", "Terminal use does not alert nearby enemies", 0, 1, 'A', 4, 6),
            Perk("Escape Plan", "After hacking a terminal, gain 50% movement speed for 5 seconds", 0, 1, 'A', 5, 10),
            Perk("Dexterous", "Reload weapons 20% faster", 0, 1, 'A', 6, 14),
            Perk("Catlike", "Reduce falling damage and move silently", 0, 1, 'A', 7, 18),
        ]

    def _create_luck_perks(self) -> List[Perk]:
        return [
            Perk("Serendipity", "Find extra caps and ammo in containers", 0, 2, 'L', 3, 2),
            Perk("Mysterious Savior", "Sometimes a mysterious figure helps in combat (Diggle?)", 0, 1, 'L', 5, 8),
            Perk("Better Odds", "Increase critical hit chance by 5% per rank", 0, 2, 'L', 6, 12),
            Perk("Fortune's Favor", "Once per day, reroll a failed skill check", 0, 1, 'L', 7, 16),
            Perk("Perfect Timing", "Enemies sometimes drop valuable tech components", 0, 1, 'L', 8, 20),
        ]

    # ------------------------- Methods -------------------------
    def level_up(self):
        """Increase level by one and grant a perk point."""
        self.level += 1
        self.perk_points += 1
        print(f"\n>>> Level up! Now level {self.level}. You have {self.perk_points} perk point(s).")

    def show_special(self):
        """Display S.P.E.C.I.A.L. stats."""
        print("\n" + "="*50)
        print(f"{self.name} (played by {self.actor}) - Level {self.level}")
        print("="*50)
        for key in ['S','P','E','C','I','A','L']:
            print(f"  {self.special[key]}")
        print(f"\nPerk Points: {self.perk_points}")

    def show_perks(self, attr_short: Optional[str] = None):
        """Show perk tree for given attribute, or all if None."""
        if attr_short:
            attr = self.special.get(attr_short.upper())
            if attr:
                self._display_perk_tree(attr)
            else:
                print("Invalid attribute. Use S,P,E,C,I,A,L")
        else:
            for key in ['S','P','E','C','I','A','L']:
                self._display_perk_tree(self.special[key])

    def _display_perk_tree(self, attr: SpecialAttribute):
        """Helper to display perks under one attribute."""
        print(f"\n--- {attr.name} ({attr.short}: {attr.value}) ---")
        for perk in attr.perks:
            req = f"[{perk.special_requirement}:{perk.special_value}] Lv.{perk.level_requirement}"
            if perk.required_perk:
                req += f" requires {perk.required_perk.name}"
            print(f"  {perk} {req}")

    def assign_perk(self, attr_short: str, perk_index: int):
        """
        Attempt to assign a perk point to the given perk.
        perk_index: 0-based index within that attribute's perk list.
        """
        attr = self.special.get(attr_short.upper())
        if not attr:
            print("Invalid attribute.")
            return False
        if perk_index < 0 or perk_index >= len(attr.perks):
            print("Invalid perk index.")
            return False

        perk = attr.perks[perk_index]
        if perk.can_take(self):
            perk.take_rank()
            self.perk_points -= 1
            print(f"Perk assigned: {perk.name} now at rank {perk.rank}/{perk.max_rank}")
            return True
        else:
            print("Cannot take this perk. Check requirements.")
            return False

    # ------------------------- Pip-Boy Simulation -------------------------
    def run_pipboy(self):
        """Simple interactive menu."""
        while True:
            print("\n" + "="*50)
            print("PIP-BOY 3000 - FELICITY SMOAK")
            print("="*50)
            print("1. Show S.P.E.C.I.A.L.")
            print("2. Show all perks")
            print("3. Show perks for an attribute")
            print("4. Level up (simulate)")
            print("5. Assign perk point")
            print("6. Exit")
            choice = input("Choose an option: ").strip()

            if choice == '1':
                self.show_special()
            elif choice == '2':
                self.show_perks()
            elif choice == '3':
                a = input("Enter attribute (S,P,E,C,I,A,L): ").upper()
                self.show_perks(a)
            elif choice == '4':
                self.level_up()
            elif choice == '5':
                if self.perk_points <= 0:
                    print("You have no perk points. Level up first.")
                    continue
                attr = input("Attribute (S,P,E,C,I,A,L): ").upper()
                if attr not in self.special:
                    print("Invalid.")
                    continue
                print("Available perks:")
                for idx, p in enumerate(self.special[attr].perks):
                    print(f"  {idx}: {p}")
                try:
                    idx = int(input("Enter perk number: "))
                except ValueError:
                    print("Invalid number.")
                    continue
                self.assign_perk(attr, idx)
            elif choice == '6':
                print("Returning to the real world...")
                break
            else:
                print("Invalid choice.")


# ------------------------- Personality Description -------------------------
def personality_profile():
    print("\n" + "="*50)
    print("FELICITY SMOAK - PERSONALITY PROFILE")
    print("="*50)
    print("""
Felicity Megan Smoak (portrayed by Emily Bett Rickards) is the brilliant
tech expert and hacker of Team Arrow. Her personality is a delightful mix of:

- **Genius‑level intellect**: She can hack anything, often quipping about
  ones and zeroes while doing so.
- **Endearing awkwardness**: Her rapid‑fire speech and tendency to ramble
  when nervous make her incredibly relatable.
- **Optimistic spirit**: Even in dire situations, she maintains hope and a
  positive outlook, often lightening the mood with a well‑timed joke.
- **Loyalty**: She is fiercely protective of her friends, especially Oliver,
  and will risk everything to help them.
- **Sarcastic wit**: Her snappy comebacks and pop‑culture references are a
  staple of the Arrowverse.

In the field, she is usually behind a computer, but when necessary, she can
handle herself with surprising resourcefulness.
    """)


# ------------------------- Main -------------------------
if __name__ == "__main__":
    felicity = FelicitySmoak()
    personality_profile()
    felicity.run_pipboy()