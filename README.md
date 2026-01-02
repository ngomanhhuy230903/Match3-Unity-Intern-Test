Unity Developer Test - Fish Match 3
Here is my submission for the Unity Developer Test. The project is a Match-3 Tile Puzzle game.

Environment & Info
Unity Version: 2022.3.62f2

Language: C#

Time spent: ~2.5 Hours

Implementation Details
1. Re-skin & Setup
Replaced all default sprites with the Fish assets provided.

Setup basic UI for Home, Game, and Win/Lose screens.

2. Gameplay Logic
Board Generation: Implemented logic to ensure the total count of each fish type is divisible by 3, guaranteeing the level is solvable.

Controls: Tapping an item moves it to the Bottom Bar (capacity: 5).

Matching:

The Bottom Bar automatically groups identical items together.

When 3 identical items match, they are cleared with a scaling animation.

Game Loop:

Win: Clear the board completely.

Lose: The Bottom Bar is full (5 items) and no match is possible.

3. Extra Modes (Autoplay & Time Attack)
-Autoplay Logic (Win Mode) I implemented a simple bot that scans the board every 0.5s. It prioritizes moves in this order:

Complete a match: Looks for an item that completes a set of 3 (if 2 are already in the bar).

Form a pair: Looks for an item that makes a pair (if 1 is already in the bar).

Pick common items: Otherwise, it picks the most common item type available on the board.

-Auto Lose Logic This bot tries to fill the bar without matching:

It specifically targets items that correspond to types not currently in the bar.

It avoids picking items that would create a pair or a triplet.

-Time Attack Mode

Timer: Added a 60-second countdown.

Undo Mechanic: In this mode, clicking an item in the Bottom Bar returns it to its original position on the Grid (cached using a dictionary).

Condition: The "Bar Full" lose condition is disabled for this mode.