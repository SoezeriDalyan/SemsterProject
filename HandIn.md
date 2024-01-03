# Project Documentation Protocol

## Project Overview

This project involves the development of a trading card game server. The key technical steps, designs, failures, and selected solutions are documented below. The time spent on each aspect is also tracked.

## 1. Familiarization with ServerSockets

- Created a dummy server to reacquaint myself with the functionality of ServerSockets.

## 2. User Class Implementation

- Created the `User` class with relevant properties.

## 3. Request Processing

- Retrieved the body of requests and deserialized the corresponding objects.

## 4. Initial Design of Packs

- Initially designed packs using a poorly structured approach.
- Realized the design was flawed and decided to change the design to improve the structure.

## 5. Testing Functionality with Lists

- Started testing functionality with simple lists before implementing it with a database to avoid potential issues later.

## 6. Adding Postgres Database
- Decided to store only Users, Cards, and Sessions in the database, generating packs in C#.

## 7. Database Table Creation

- Created the `Card` and `Package` tables.
- Initially populated the `Card` table with INSERT statements.
- Revised the design, realizing it made more sense to first create packs and then associate cards with packs.

## 8. Packs Redesign

- Redesigned packs and associated card IDs.
- Implemented the `buys pack` route.

## 9. Deck Table Addition

- Added the `Deck` table to the database.

## 10. User Profile Update

- Updated the user profile route.
- Extended database tables to include bio and image information.

## 11. Scoreboard and Points

- Added points to the database for scoreboard purposes.
- Initialized each player with 100 points.

## 12. ELO Rating System

- Added the ELO rating system to handle player statistics.
- Extended the database to include the ELO field.

## 13. Battle Implementation

- Encountered difficulties with the battle implementation.
- Optimized the code for players waiting for each other.
- Refactored the `Battle` class multiple times.

## 14. Integration of Battle Functionality

- Integrated the battle functionality into the program.

## 15. Introduction of Request and Response Classes

- Recognized the need for Request and Response classes.
- Ensured uniformity in method returns.
- Began improving code quality through a top-down approach.

## 16. Standardizing Method Outputs

- Ensured uniform outputs for all methods.
- Created a `Response` class for serialization purposes.
- Resolved various issues.

## 17. Battle Logic Refinement

- Refined the battle logic, especially handling card effects.
- Introduced a temporary damage variable for managing round-specific effects.

## 18. JSON Serialization Correction

- Rectified issues where JSON serialization was not consistent.

## 19. Trading Functionality

- Implemented the `Trading` class and corresponding database table.
- Integrated trading functionality into the program.

## 20. Code Refinement

- Continued refining and improving the overall code.

## 21. Unit Tests created
The class is marked with `[TestFixture]`, indicating it contains tests. `OneTimeTearDown` cleans up after all tests are done.

All of the tests follow a simple structure: arrange the test environment, perform an action, and verify the expected outcome also known as the Arrange-Act-Assert (AAA) pattern

## 22. My unique feature
My unique feature is that players can strengthen their cards, but it operates more like an evolution based on the player's performance.

For instance, if a player has an Elo rating over 100, they can activate the 'Plus Ultra' evolution. This allows them to select a card from their deck, which will receive a +8 increase in damage. Conversely, if the Elo rating is below 100, they can trigger the 'Get Back' evolution.

The highest possible evolution is applied, and I limit the complexity of this feature.

## Time Tracking

1. Familiarization with ServerSockets: 1 std
2. User Class Implementation: 1 std
3. Request Processing: 1 std
4. Initial Design of Packs: 1 std
5. Testing Functionality with Lists: 2 std
6. Adding Postgres Database: 2 std
7. Database Table Creation: 2 std
8. Packs Redesign: 2 std
9. Deck Table Addition: 2.5 std
10. User Profile Update: 2.5 std
11. Scoreboard and Points: 2.5 std
12. ELO Rating System: 2.5 std
13. Battle Implementation: 6 std
14. Integration of Battle Functionality: 3 std
15. Introduction of Request and Response Classes: 6.5 std
16. Standardizing Method Outputs: 3 std
17. Battle Logic Refinement: 3.3 std
18. JSON Serialization Correction: 2.3 std
19. Trading Functionality: 1 std
20. Code Refinement: 3 std
21. Unit Tests created 2.5 std
22. Unique feature created 3 std

**Total Hours Spent: 69***