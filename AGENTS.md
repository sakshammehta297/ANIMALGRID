# Animal Grid - AI Development Guide

PROJECT: Original mobile N×N animal logic puzzle.
RULE: Never hard-code a single grid size. All logic must be N×N.
ARCHITECTURE: Puzzle logic must be 100% independent from Unity presentation/UI.
REQUIREMENTS: 
- Every generated puzzle must have a valid solution.
- Published puzzles must have exactly ONE unique solution.
STYLE: Small, modular C# classes. No giant "GameManager" monoliths.
SAFETY: Do not overwrite working systems without understanding them. Explain plans before large changes.
TESTING: Write NUnit tests for every puzzle rule.
DATA: Progression must be data-driven (ScriptableObjects/JSON).