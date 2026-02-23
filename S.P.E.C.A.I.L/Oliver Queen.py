#!/usr/bin/env python3
"""
Oliver Queen (Stephen Amell) - S.P.E.C.I.A.L. Skill Tree
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
        self.special_requirement = special_requirement  # e.g. 'S'
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
class OliverQueen:
    """Represents Oliver Queen with his S.P.E.C.I.A.L. stats and perks."""
    def __init__(self):
        self.name = "Oliver Queen"
        self.actor = "Stephen Amell"
        self.level = 1
        self.xp = 0
        self.perk_points = 0

        # Define S.P.E.C.I.A.L. base values (based on lore)
        self.special = {
            'S': SpecialAttribute("Strength", "S", 7, self._create_strength_perks()),
            'P': SpecialAttribute("Perception", "P", 9, self._create_perception_perks()),
            'E': SpecialAttribute("Endurance", "E", 8, self._create_endurance_perks()),
            'C': SpecialAttribute("Charisma", "C", 6, self._create_charisma_perks()),
            'I': SpecialAttribute("Intelligence", "I", 7, self._create_intelligence_perks()),
            'A': SpecialAttribute("Agility", "A", 9, self._create_agility_perks()),
            'L': SpecialAttribute("Luck", "L", 5, self._create_luck_perks())
        }

    # ------------------------- Perk Definitions -------------------------
    def _create_strength_perks(self) -> List[Perk]:
        return [
            Perk("Heavy Draw", "Bows deal +10% damage per rank", 0, 3, 'S', 2, 2),
            Perk("Iron Grip", "Reduces weapon sway by 25% per rank", 0, 2, 'S', 4, 5),
            Perk("Unbroken", "Melee attacks with bow/staff do +20% damage", 0, 2, 'S', 6, 10),
            Perk("Arrow Breaker", "Chance to deflect incoming projectiles", 0, 2, 'S', 8, 15),
            Perk("Takedown Artist", "Silent takedowns cost 25% less AP", 0, 1, 'S', 9, 20),
        ]

    def _create_perception_perks(self) -> List[Perk]:
        return [
            Perk("Eagle Eye", "Zoom while aiming shows enemy level", 0, 2, 'P', 2, 1),
            Perk("Detective", "Highlights interactive objects", 0, 1, 'P', 4, 3),
            Perk("Sniper's Nest", "Headshots with arrows do +25% damage", 0, 2, 'P', 6, 8),
            Perk("Intuition", "Detect enemy weaknesses in VATS", 0, 2, 'P', 7, 12),
            Perk("Sixth Sense", "Detect hidden enemies automatically", 0, 1, 'P', 9, 18),
        ]

    def _create_endurance_perks(self) -> List[Perk]:
        return [
            Perk("Survivalist", "+10 HP per rank", 0, 5, 'E', 2, 1),
            Perk("Island Forged", "Resist poison and disease by 25% per rank", 0, 2, 'E', 4, 5),
            Perk("Never Give Up", "When health below 20%, +50 damage resistance", 0, 1, 'E', 6, 12),
            Perk("Adrenaline Rush", "Increased damage as health decreases", 0, 2, 'E', 8, 18),
            Perk("Immortal", "Once per day, survive a fatal blow", 0, 1, 'E', 10, 25),
        ]

    def _create_charisma_perks(self) -> List[Perk]:
        return [
            Perk("Team Leader", "Followers deal +10% damage per rank", 0, 3, 'C', 2, 2),
            Perk("Inspiration", "Followers gain +20 HP per rank", 0, 2, 'C', 4, 7),
            Perk("The Hood", "Intimidate enemies in dialogue", 0, 1, 'C', 6, 10),
            Perk("Ally", "Recruit special allies (e.g., Diggle, Felicity)", 0, 2, 'C', 7, 15),
            Perk("Mayor's Voice", "Succeed in hard persuasion checks", 0, 1, 'C', 9, 20),
        ]

    def _create_intelligence_perks(self) -> List[Perk]:
        return [
            Perk("Tactician", "VATS criticals build 15% faster per rank", 0, 2, 'I', 3, 2),
            Perk("Strategist", "+10% XP from combat per rank", 0, 3, 'I', 4, 5),
            Perk("Gadgeteer", "Craft trick arrows at workbenches", 0, 2, 'I', 6, 10),
            Perk("Master Planner", "Plan ambushes: +25% sneak attack damage", 0, 1, 'I', 8, 16),
            Perk("Mentor", "Train allies to gain perks faster", 0, 1, 'I', 9, 22),
        ]

    def _create_agility_perks(self) -> List[Perk]:
        return [
            Perk("Quick Draw", "Draw arrows 20% faster per rank", 0, 3, 'A', 2, 1),
            Perk("Acrobat", "Reduce falling damage and move faster while sneaking", 0, 2, 'A', 4, 4),
            Perk("Shadow", "Sneak attacks do 2.5x damage (instead of 2x)", 0, 1, 'A', 6, 8),
            Perk("Escape Artist", "Lose enemies while sprinting", 0, 1, 'A', 7, 14),
            Perk("Master Archer", "Arrows can ricochet to additional targets", 0, 2, 'A', 9, 20),
        ]

    def _create_luck_perks(self) -> List[Perk]:
        return [
            Perk("Fortune Finder", "Find more ammunition in containers", 0, 2, 'L', 2, 2),
            Perk("Scrounger", "Rare trick arrows appear more often", 0, 2, 'L', 4, 6),
            Perk("Mysterious Stranger", "Sometimes a mysterious helper appears in VATS", 0, 1, 'L', 6, 12),
            Perk("Better Criticals", "Criticals do +50% damage", 0, 2, 'L', 7, 17),
            Perk("V.A.T.S. Enhanced", "Reduced AP cost for all actions", 0, 2, 'L', 9, 23),
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
            print("PIP-BOY 3000 - OLIVER QUEEN")
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


# ------------------------- Main -------------------------
if __name__ == "__main__":
    oliver = OliverQueen()
    oliver.run_pipboy()