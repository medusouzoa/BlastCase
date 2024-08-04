# BlastCase

BlastCase is a tile-matching game with a collapse/blast mechanic. Players match and remove groups of same-colored blocks. The game board adjusts as blocks are removed and new ones fall into place. Features include different block colors, varying icon displays based on group sizes, and fixed obstacles.

# Features

- Board Configuration: Up to 10 rows and 10 columns.
- Block Colors: Supports 1 to 6 colors, each with distinct icons.
- Group Icons: Icons change based on group size:
    - Default icon for groups smaller than A blocks.
    - First icon for groups between A and B blocks.
    - Second icon for groups between B and C blocks.
    - Third icon for groups larger than C blocks.
- Box Obstacles: Fixed blocks with 2 health points. They do not fall and block the falling of other blocks. They take damage when adjacent groups are blasted.
- Deadlock Detection: Implements a shuffling solution to handle deadlocks without random shuffling.
  
# Gameplay

- Match Groups: Tap or click on groups of 2 or more same-colored blocks to remove them from the board.
- Gravity Effect: After removal, blocks above fall to fill empty spaces. Extra blocks are created outside the board and drop into columns as needed.
- Icon Display: Icons on blocks change based on the total number of blocks in the group.
- Obstacles: Fixed obstacles with 2 health points affect gameplay and do not fall.

# Game Visuals
You can find the game visuals under the "Recordings" folder.
