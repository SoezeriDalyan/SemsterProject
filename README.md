# Monster Trading Cards Game (MTCG)

This HTTP/REST-based server serves as a platform for trading and battling magical cards in a vibrant card-game world. Users can register, manage their cards, create decks, and engage in battles against each other. The game mechanics involve acquiring packages, building decks, and competing to climb the scoreboard.

## Features

- **User Management:**
  - Users are registered players with unique credentials (username, password).
  - Users can manage their cards, which include both spell and monster cards.

- **Card Attributes:**
  - Each card has a name and multiple attributes, including damage and element type.
  - Damage of a card is constant and doesn't change.

- **Packages and Coins:**
  - Users can buy packages containing 5 cards by spending virtual coins.
  - Each user starts with 20 coins to buy packages.

- **Deck Building:**
  - Users select the best 4 cards from their collection to form a deck.
  - Decks are used in battles against other players.

- **Battle System:**
  - Battles involve both monster and spell cards.
  - Element types affect the effectiveness of attacks.
  - The battle logic is detailed and includes various card interactions.

- **Scoreboard and Stats:**
  - Scoreboard displays a sorted list of ELO values.
  - Users have editable profiles and detailed stats, including ELO values.

- **Security:**
  - Token-based authentication ensures user actions are performed by the corresponding user.

- **Persistence:**
  - Data is stored in a PostgreSQL database.

## Battle Logic

Battles involve rounds where cards are randomly chosen from decks. Each round compares the damage of the selected cards, and the winner takes the defeated card. The battle ends when a draw occurs or after a maximum of 100 rounds.

## Trading Deals

Users can initiate trading deals by offering a card in the store with specific requirements for the desired card in return.

## Optional Features

- Trading system: Trade cards vs coins.
- Additional card classes (e.g., trap cards, passive spells).
- Further element types (e.g., ice, wind).
- Friends List: Play against friends and manage friends by username.
- Card descriptions.
- Extended Scoreboard with ELO or WHR.
- Ability to add virtual coins with stats on spent coins.
- Transaction history.
- Win/lose ratio in user stats.

## How to Run

1. Clone the repository.
2. Set up a PostgreSQL database and update the connection string in the configuration.
3. Build and run the application.
4. Test with the provided curl script (will be added later).
