# Animal Grid - AI Development Guide

## PROJECT
Original mobile N×N animal logic puzzle game.
Android-first, Unity + C#.

## GOLDEN RULE
**Puzzle logic MUST be independent from Unity presentation.**
The puzzle engine should work even if Unity UI is removed.

## CORE RULES
For an N×N grid:
- Grid size = N
- Number of animals = N
- Number of colors = N
- One animal per row
- One animal per column
- One animal per color
- NO touching (including diagonally)

## ARCHITECTURE PRINCIPLES
1. **NEVER hard-code grid size** - everything must be dynamic N×N
2. **Small, modular classes** - no giant GameManager monoliths
3. **Pure C# for puzzle logic** - no Unity dependencies in Core/
4. **Test everything** - write NUnit tests for every rule
5. **Data-driven progression** - no hard-coded level curves

## CODE STYLE
- Clear, documented code
- Interface-based design where helpful
- Deterministic algorithms
- No unnecessary dependencies

## TESTING REQUIREMENTS
- Run EditMode tests after puzzle-engine changes
- Run PlayMode tests after gameplay/UI changes
- Every bug becomes a regression test

## SAFETY RULES
- Do NOT overwrite working systems without understanding them
- Do NOT delete assets/code unless explicitly requested
- Before large changes, explain the plan first
- Show affected files when making changes

## DATA
- Use ScriptableObjects for level data
- Progression must be data-driven
- Save locally first (PlayerPrefs/JSON)

## GENERATOR QUALITY GATE
Every published puzzle MUST have:
✓ Valid solution
✓ Exactly ONE solution (use CountSolutions(puzzle, 2))
✓ All rules satisfied
✓ Known difficulty classification