# Hi-Lo Game - ASP.NET Core

A Hi-Lo guessing game implementation with both solo and real-time multiplayer modes, built with ASP.NET Core 9.0 and SignalR.

## Game Concept

The system chooses a mystery number within a configurable range. Players guess numbers and receive hints (HI/LO) until they find it. The goal is to discover the mystery number in minimum attempts.

## Features

### Solo Mode
- Configurable range [Min-Max]
- Attempt tracking and history
- Visual feedback with color-coded hints
- Game statistics and leaderboard

### Multiplayer Mode (SignalR)
- Real-time 2-player gameplay
- Create or join game rooms
- Simultaneous round-based gameplay
- Custom range configuration per room
- Live notifications and updates
- Complete game statistics at the end

## Architecture

Clean architecture following SOLID principles:

```
┌─ Presentation Layer
│  ├─ Razor Pages (Solo.cshtml, Multiplayer.cshtml)
│  └─ SignalR Hub (GameHub) - Orchestration only
│
├─ Service Layer (Business Logic)
│  ├─ IGameService / GameService
│  ├─ IMultiplayerGameService / MultiplayerGameService
│  └─ ILoggerService / BrowserLoggerService
│
├─ Repository Layer (Data Access)
│  ├─ IGameRepository / SessionGameRepository
│  ├─ IRoomRepository / MemoryCacheRoomRepository
│  └─ IGameHistoryRepository / MemoryCacheGameHistoryRepository
│
└─ Domain Layer
   ├─ Models (Entities, ValueObjects, Events, Responses, etc.)
   ├─ Mappers (GameMapper, GameRoomMapper)
   ├─ Factories (MessageFactory)
   └─ Utils (RandomNumberGenerator)
```

## Technologies

- **ASP.NET Core 9.0** - Web framework
- **Razor Pages** - Server-side rendering
- **SignalR** - Real-time communication
- **C# 12** - Latest language features
- **Dependency Injection** - Native IoC container
- **Session State** - Solo game persistence
- **Memory Cache** - Multiplayer rooms & history

## Getting Started

### Prerequisites
- .NET 9.0 SDK

### Run the application
```bash
dotnet run
```

Then open: **http://localhost:5000**

### Quick start scripts
```bash
./start.sh  # Start with port check
./stop.sh   # Stop all instances
```

## How to Play

### Solo Mode
1. Enter your name on the home screen
2. Select "Solo" mode
3. Configure min/max range
4. Start guessing!

### Multiplayer Mode
1. Enter your name on the home screen
2. Select "Multiplayer" mode
3. Create a room or join an existing one
4. Both players guess simultaneously each round
5. First to find the number wins!

## Design Patterns

- **Repository Pattern** - Data access abstraction
- **Service Layer Pattern** - Business logic isolation
- **Dependency Injection** - Loose coupling
- **Factory Pattern** - Object creation
- **Mapper Pattern** - Domain/DTO separation
- **Result Pattern** - Error handling (`OperationResult<T>`)
- **Record Types** - Immutable data structures for Events & Responses

## Project Structure

```
HiLoGame/
├── Pages/              # Razor Pages (UI)
├── Hubs/               # SignalR Hubs (Real-time communication)
├── Services/           # Business logic layer
├── Repositories/       # Data access layer
├── Models/             # Domain layer (organized by type)
│   ├── Constants/      # Game & message constants
│   ├── Enums/          # Type enumerations (GameMode, GameResult, ErrorCode)
│   ├── Events/         # SignalR event models (8 real-time events)
│   ├── Exceptions/     # Custom domain exceptions
│   ├── Responses/      # Hub response DTOs (4 response types)
│   ├── Results/        # Operation result pattern
│   └── *.cs            # Core entities (Game, Player, GameRoom, etc.)
├── Mappers/            # Domain/DTO transformations
├── Factories/          # Object creation (MessageFactory)
├── Utils/              # Utilities (RandomNumberGenerator)
└── wwwroot/            # Static files (CSS, JS)
```

### Models Organization

The `Models/` folder follows domain-driven design principles with clear separation:

**Events/** (SignalR Real-time Events - C# Records)
- `RoomCreatedEvent` - Room creation broadcast
- `PlayerJoinedEvent` - Player joins notification
- `GameStartedEvent` - Game start signal
- `PlayerGuessedEvent` - Guess notification
- `GameCompletedEvent` - Win announcement
- `RoundCompletedEvent` - Round transition
- `WaitingForOtherPlayerEvent` - Waiting state
- `PlayerLeftEvent` - Disconnection notification

**Responses/** (Hub Method Returns - C# Records)
- `CreateRoomResponse` - Room creation result
- `JoinRoomResponse` - Join operation result
- `MakeGuessResponse` - Guess processing result
- `GetAvailableRoomsResponse` - Rooms list with `RoomInfo`

**Enums/**
- `GameMode` - Solo/Multiplayer
- `GameResult` - Higher/Lower/Win
- `ErrorCode` - Domain error codes (12 types)

**Core Entities** (Root level)
- `Game`, `GameRoom`, `Player` - Main aggregates
- `GameRange`, `GuessResult` - Value objects
- `PlayerGameState`, `RoomPlayer` - State tracking
- `GameHistory`, `GuessAttempt` - Historical data

## Deployment

### Deploy to Render.com

This application is ready to deploy on Render.com using Docker:

1. **Push your code to GitHub**
   ```bash
   git push -u origin main
   ```

2. **Create a new Web Service on Render.com**
   - Go to [Render Dashboard](https://dashboard.render.com/)
   - Click "New +" → "Web Service"
   - Connect your GitHub repository

3. **Configure the service**
   - **Runtime**: Docker
   - **Plan**: Free (or your preferred plan)
   - **Environment**: Production
   - The `render.yaml` file will auto-configure the rest

4. **Deploy**
   - Click "Create Web Service"
   - Render will automatically build and deploy your app
   - Your app will be available at `https://your-app-name.onrender.com`

**Note**: The free tier on Render.com will spin down after inactivity and may take ~30 seconds to wake up.

## License

This project was created as a technical assessment.

---

Built with ASP.NET Core 9.0
