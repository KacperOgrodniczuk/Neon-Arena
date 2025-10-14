# Neon Arena
A fast-paced, online multiplayer third-person arena shooter where players compete in short, timed matches.

## Overview
3D Multiplayer arena shooter, inspired by Tron and The Tower. Players shoot primitively shaped projectiles. The project was used for learning real-time networking and multiplayer game programming, with smooth syncronisation and client-side prediction.

## My Role
Sole developer, responsible for networking and core gameplay systems design and implementation.
- Player movement, actions and synchronisation.
- Core networking using FidhNet.
- Lobby and match-making systems.

## Technologies Used
- *Engine:* Unity [6000.2.5f1]
- *Networking Solution:* FishNet (Free Tier)
- *Language:* C#
- *Tools:* Visual Studio

## Key Features
- *Real-Time Multiplayer:* Players connect to a server to compete in timed, fast-paced matches with all movement, actions and game data synchronised across clients.
- *FishNet Network Synchronisation:* Use of FishNet's Network Prefabs, RPCs, and SyncVars.
- *Client-Side Prediction:* Character movement and actions such as shooting feel responsive with minimal perceived lag.

## How to Run
- Clone the repository.
- Open the project using Unity Editor [6000.2.6f2].
- Run the game in the editor or build and run the game.

## Future Plans
- Implement lag compensation to ensure fair gameplay regardless of network latency.
- Develop lobby and matchmaking system.
- Add more game modes, drops, upgrades and abilities.
