// SignalR connection
let connection = null;
let currentRoomId = null;
let currentRoomName = null;
let currentPlayerName = null;
let myConnectionId = null;
let hasGuessedThisRound = false;
let currentRound = 1;
let totalGames = 1;
let gamesPlayed = 0;
let myAttempts = [];

// Initialize SignalR connection
function initializeConnection() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/gamehub")
        .withAutomaticReconnect()
        .build();

    // Event handlers
    connection.on("RoomCreated", (data) => {
        console.log("Room created:", data);
        // Add the new room to the list if we're on the room selection screen
        if (document.getElementById("roomSelection").style.display !== "none") {
            addRoomToList(data);
        }
    });

    connection.on("PlayerJoined", (data) => {
        console.log("Player joined:", data);
        updatePlayerCount(data.playersCount);
        showNotification(`${data.playerName} has joined the room!`, "success");
    });

    connection.on("GameStarted", (data) => {
        console.log("Game started:", data);
        document.getElementById("gameOverScreen").style.display = "none";
        document.getElementById("gameScreen").style.display = "block";
        hideWaitingForNextGame();
        hasGuessedThisRound = false;
        currentRound = 1;
        totalGames = data.totalGames ?? 1;
        gamesPlayed = data.gamesPlayed ?? 0;
        myAttempts = [];
        if (data.roomName) {
            currentRoomName = data.roomName;
        }
        startGame(data);
    });

    connection.on("PlayerGuessed", (data) => {
        console.log("Player guessed:", data);
        if (data.playerName !== currentPlayerName) {
            showNotification(`${data.playerName} made a guess!`, "info");
        }
    });

    connection.on("WaitingForOtherPlayer", (data) => {
        console.log("Waiting for other player:", data);
        showNotification(data.message, "waiting");
        hasGuessedThisRound = true;
        disableGuessForm();
    });

    connection.on("RoundCompleted", (data) => {
        console.log("Round completed:", data);
        // Server sends the current round number (same mystery number)
        currentRound = data.roundNumber ?? 1;
        hasGuessedThisRound = false;
        document.getElementById("roundNumber").textContent = currentRound;
        showNotification(data.message || "New round!", "success");
        enableGuessForm();
    });

    connection.on("GameCompleted", (data) => {
        console.log("Game completed:", data);
        showGameOver(data);
    });

    connection.on("PlayersReadyForNextGame", (data) => {
        console.log("Players ready for next game:", data);
        updateWaitingForNextGame(data);
    });

    connection.on("PlayerLeft", (data) => {
        console.log("Player left:", data);
        showNotification(data.message, "info");
        setTimeout(() => {
            window.location.reload();
        }, 3000);
    });

    // Start connection
    connection.start()
        .then(() => {
            console.log("Connected to SignalR hub");
            myConnectionId = connection.connectionId;
            loadAvailableRooms();
        })
        .catch(err => {
            console.error("Error connecting to SignalR hub:", err);
            showErrorMessage("Failed to connect to server. Please refresh the page.");
        });
}

// Load available rooms
async function loadAvailableRooms() {
    try {
        const result = await connection.invoke("GetAvailableRooms");
        if (result.success) {
            displayRooms(result.rooms);
        } else {
            showErrorMessage("Failed to load rooms: " + result.error);
        }
    } catch (err) {
        console.error("Error loading rooms:", err);
        showErrorMessage("Failed to load rooms");
    }
}

// Display rooms list
function displayRooms(rooms) {
    const roomsList = document.getElementById("roomsList");
    
    if (rooms.length === 0) {
        roomsList.innerHTML = '<p class="info-message">No rooms available. Create one!</p>';
        return;
    }

    roomsList.innerHTML = rooms.map(room => createRoomHTML(room)).join('');
}

// Create HTML for a single room
function createRoomHTML(room) {
    const games = room.totalGames != null ? room.totalGames : 1;
    return `
        <div class="room-item" data-room-id="${room.roomId}">
            <div class="room-info">
                <span class="room-name-text">${room.roomName}</span>
                <span class="room-range">[${room.minNumber} - ${room.maxNumber}]</span>
                <span class="room-rounds">${games} game(s)</span>
                <span class="room-players">${room.playersCount}/${room.maxPlayers} players</span>
            </div>
            <button class="btn-join" onclick="joinRoom('${room.roomId}')">Join</button>
        </div>
    `;
}

// Add a new room to the list
function addRoomToList(room) {
    const roomsList = document.getElementById("roomsList");
    
    // Remove "no rooms" message if present
    const infoMessage = roomsList.querySelector('.info-message');
    if (infoMessage) {
        roomsList.innerHTML = '';
    }
    
    // Check if room already exists
    const existingRoom = roomsList.querySelector(`[data-room-id="${room.roomId}"]`);
    if (!existingRoom) {
        roomsList.insertAdjacentHTML('afterbegin', createRoomHTML(room));
    }
}

// Create room
async function createRoom() {
    const roomName = document.getElementById("roomName").value.trim();
    const minNumber = parseInt(document.getElementById("minNumber").value);
    const maxNumber = parseInt(document.getElementById("maxNumber").value);
    const totalGamesInput = document.getElementById("totalGames");
    const totalGamesParam = totalGamesInput ? parseInt(totalGamesInput.value) || 1 : 1;

    if (!roomName) {
        showErrorMessage("Please enter a room name");
        return;
    }

    if (isNaN(minNumber) || isNaN(maxNumber)) {
        showErrorMessage("Please enter valid min and max numbers");
        return;
    }

    if (minNumber >= maxNumber) {
        showErrorMessage("Max must be greater than Min");
        return;
    }

    if (totalGamesParam < 1 || totalGamesParam % 2 === 0) {
        showErrorMessage("Number of games must be odd (1, 3, 5, 7, 9)");
        return;
    }

    try {
        const result = await connection.invoke("CreateRoom", roomName, currentPlayerName, minNumber, maxNumber, totalGamesParam);
        if (result.success) {
            currentRoomId = result.roomId;
            currentRoomName = result.roomName;
            showWaitingRoom();
        } else {
            showErrorMessage("Failed to create room: " + result.error);
        }
    } catch (err) {
        console.error("Error creating room:", err);
        showErrorMessage("Failed to create room");
    }
}

// Join room
async function joinRoom(roomId) {
    try {
        const result = await connection.invoke("JoinRoom", roomId, currentPlayerName);
        if (result.success) {
            currentRoomId = result.roomId;
            currentRoomName = result.roomName;
            
            if (result.playersCount === result.maxPlayers) {
                // Game will start automatically
                showNotification("Room full! Starting game...", "success");
            } else {
                showWaitingRoom(result.playersCount);
            }
        } else {
            showErrorMessage("Failed to join room: " + result.error);
        }
    } catch (err) {
        console.error("Error joining room:", err);
        showErrorMessage("Failed to join room");
    }
}

// Start game (game 1 or next game after Play Again)
function startGame(data) {
    document.getElementById("roomSelection").style.display = "none";
    document.getElementById("waitingRoom").style.display = "none";
    document.getElementById("gameScreen").style.display = "block";

    document.getElementById("gameRoomName").textContent = currentRoomName;
    document.getElementById("displayMinNumber").textContent = data.minNumber;
    document.getElementById("displayMaxNumber").textContent = data.maxNumber;

    const guessInput = document.getElementById("guessInput");
    guessInput.min = data.minNumber;
    guessInput.max = data.maxNumber;
    guessInput.value = "";
    guessInput.disabled = false;
    document.getElementById("guessBtn").disabled = false;

    // Display players
    const playersList = document.getElementById("playersList");
    playersList.innerHTML = data.players.map(p =>
        `<span class="player-badge ${p.name === currentPlayerName ? 'current-player' : ''}">${p.name}</span>`
    ).join('');

    document.getElementById("roundNumber").textContent = currentRound;
    const currentGameNum = (data.gamesPlayed ?? 0) + 1;
    document.getElementById("currentGameNumber").textContent = currentGameNum;
    document.getElementById("totalGamesDisplay").textContent = data.totalGames ?? 1;
    if (data.totalGames) totalGames = data.totalGames;
    if (data.gamesPlayed != null) gamesPlayed = data.gamesPlayed;

    // Reset UI for new game: hide last guess result, clear attempts display, hide error
    const lastGuessResult = document.getElementById("lastGuessResult");
    if (lastGuessResult) {
        lastGuessResult.style.display = "none";
    }
    updateMyAttemptsDisplay();
    const errorMessage = document.getElementById("errorMessage");
    if (errorMessage) {
        errorMessage.style.display = "none";
        errorMessage.textContent = "";
    }

    updateGameStatus();
    showNotification("<i class='fas fa-play-circle'></i> Game started! Good luck!", "success");
}

// Update game status
function updateGameStatus() {
    // Status is now handled by notification bar and last guess result
    // No need for separate turn indicator
}

// Make guess
async function makeGuess(event) {
    event.preventDefault();

    if (hasGuessedThisRound) {
        showErrorMessage("You already guessed this round. Wait for the other player.");
        return;
    }

    const guess = parseInt(document.getElementById("guessInput").value);
    
    if (isNaN(guess)) {
        showErrorMessage("Please enter a valid number");
        return;
    }

    try {
        const result = await connection.invoke("MakeGuess", currentRoomId, guess);
        
        if (result.success) {
            document.getElementById("guessInput").value = "";
            
            // Add to my attempts history
            myAttempts.push({
                attemptNumber: result.attempts,
                guessNumber: guess,
                result: result.result,
                message: result.message
            });
            updateMyAttemptsDisplay();
            
            if (result.isWinner) {
                showNotification("You won!", "success");
                showLastGuessResult(result.message, result.result);
                hasGuessedThisRound = true;
                disableGuessForm();
            } else {
                // Show HI/LO result (always visible like solo)
                showLastGuessResult(result.message, result.result);
                // The WaitingForOtherPlayer event will handle the rest
            }
        } else {
            showErrorMessage(result.error);
        }
    } catch (err) {
        console.error("Error making guess:", err);
        showErrorMessage("Failed to make guess");
    }
}

// Update my attempts display
function updateMyAttemptsDisplay() {
    const attemptsHistory = document.getElementById("myAttemptsHistory");
    const attemptsList = document.getElementById("myAttemptsList");
    
    if (myAttempts.length === 0) {
        attemptsHistory.style.display = "none";
        return;
    }
    
    attemptsHistory.style.display = "block";
    
    attemptsList.innerHTML = myAttempts.slice().reverse().map(attempt => {
        const resultClass = attempt.result === 'Higher' ? 'attempt-higher' : 
                           attempt.result === 'Lower' ? 'attempt-lower' : 'attempt-win';
        const resultText = attempt.result === 'Higher' ? 'HI' : 
                          attempt.result === 'Lower' ? 'LO' : 'WIN';
        
        return `
            <div class="attempt-item ${resultClass}">
                <span class="attempt-number">#${attempt.attemptNumber}</span>
                <span class="attempt-guess">${attempt.guessNumber}</span>
                <span class="attempt-result">
                    <span class="result-text">${resultText}</span>
                </span>
            </div>
        `;
    }).join('');
}

// Show game over screen
function showGameOver(data) {
    document.getElementById("gameScreen").style.display = "none";
    document.getElementById("gameOverScreen").style.display = "block";

    const isWinner = data.winnerName === currentPlayerName;
    const winnerNameElement = document.getElementById("winnerName");
    
    if (isWinner) {
        winnerNameElement.innerHTML = '<i class="fas fa-trophy"></i> You won this game!';
        winnerNameElement.className = "winner";
    } else {
        winnerNameElement.innerHTML = '<i class="fas fa-times-circle"></i> You lost this game';
        winnerNameElement.className = "loser";
    }
    
    document.getElementById("revealedNumber").textContent = data.mysteryNumber;
    document.getElementById("gamesPlayedText").textContent = data.gamesPlayed ?? 0;
    document.getElementById("totalGamesText").textContent = data.totalGames ?? 1;

    const finalScoresEl = document.getElementById("finalScoresSummary");
    if (finalScoresEl && data.scores && data.scores.length) {
        finalScoresEl.style.display = "block";
        finalScoresEl.innerHTML = `
            <h3>Score (games won)</h3>
            <div class="scores-list">
                ${data.scores.map(s => `
                    <div class="score-line ${s.name === currentPlayerName ? 'current-player' : ''}">
                        <span class="score-name">${s.name}</span>
                        <span class="score-value">${s.wins} win(s)</span>
                    </div>
                `).join('')}
            </div>
        `;
    } else {
        finalScoresEl.style.display = "none";
    }

    const canPlayAgain = data.canPlayAgain === true;
    const playAgainBtn = document.getElementById("playAgainBtn");
    const backToMenuLink = document.getElementById("backToMenuLink");
    const grandGagnantEl = document.getElementById("grandGagnantText");
    if (playAgainBtn) {
        playAgainBtn.style.display = canPlayAgain ? "inline-block" : "none";
        playAgainBtn.textContent = "Play Again";
        playAgainBtn.disabled = false;
    }
    if (backToMenuLink) {
        backToMenuLink.style.display = canPlayAgain ? "none" : "inline-block";
    }
    hideWaitingForNextGame();
    // When all games are played: show overall winner (most wins)
    if (grandGagnantEl) {
        if (!canPlayAgain && data.scores && data.scores.length >= 2) {
            const sorted = [...data.scores].sort((a, b) => (b.wins || 0) - (a.wins || 0));
            if (sorted[0].wins > sorted[1].wins) {
                grandGagnantEl.textContent = "Overall winner: " + sorted[0].name;
                grandGagnantEl.style.display = "block";
                grandGagnantEl.className = "grand-gagnant";
            } else {
                grandGagnantEl.textContent = "Tie!";
                grandGagnantEl.style.display = "block";
                grandGagnantEl.className = "grand-gagnant tie";
            }
        } else {
            grandGagnantEl.style.display = "none";
        }
    }

    const playersResults = document.getElementById("playersResults");
    playersResults.innerHTML = `
        <h3>This game – statistics</h3>
        ${(data.players || []).map(player => `
            <div class="player-result ${player.isWinner ? 'winner-result' : 'loser-result'}">
                <h4>${player.name} ${player.isWinner ? '<i class="fas fa-trophy"></i>' : ''}</h4>
                <p>Attempts: ${player.attempts}</p>
                <div class="attempts-history">
                    <h5>Guess history:</h5>
                    <div class="attempts-list">
                        ${(player.guessAttempts || []).map(attempt => `
                            <div class="attempt-item attempt-${(attempt.result || '').toLowerCase()}">
                                <span class="attempt-number">#${attempt.attemptNumber}</span>
                                <span class="attempt-guess">${attempt.guessNumber}</span>
                                <span class="attempt-result">
                                    <span class="result-text">${attempt.result === 'Higher' ? 'HI' : attempt.result === 'Lower' ? 'LO' : 'WIN'}</span>
                                </span>
                            </div>
                        `).join('')}
                    </div>
                </div>
            </div>
        `).join('')}
    `;
}

async function playAgain() {
    if (!currentRoomId || !connection) return;
    const playAgainBtn = document.getElementById("playAgainBtn");
    if (playAgainBtn) playAgainBtn.disabled = true;
    try {
        const result = await connection.invoke("StartNextGame", currentRoomId);
        if (result.success) {
            if (result.allReady) {
                showNotification("New game!", "success");
                hideWaitingForNextGame();
            } else {
                const waiting = result.waitingPlayerNames || [];
                showWaitingForNextGameUI("You're ready. Waiting for " + (waiting.length ? waiting.join(", ") : "other player") + "...");
            }
        } else {
            showErrorMessage(result.error || "Cannot play again");
            if (playAgainBtn) playAgainBtn.disabled = false;
        }
    } catch (err) {
        console.error("Error starting next game:", err);
        showErrorMessage("Cannot play again");
        if (playAgainBtn) playAgainBtn.disabled = false;
    }
}

function updateWaitingForNextGame(data) {
    if (data.allReady) return;
    const waiting = data.waitingPlayerNames || [];
    const ready = data.readyPlayerNames || [];
    const msg = waiting.length
        ? "Waiting for " + waiting.join(", ") + " to click Play Again..."
        : "Everyone is ready! Starting...";
    showWaitingForNextGameUI(msg);
}

function showWaitingForNextGameUI(message) {
    const el = document.getElementById("waitingForNextGameText");
    if (el) {
        el.textContent = message;
        el.style.display = "block";
    }
}

function hideWaitingForNextGame() {
    const el = document.getElementById("waitingForNextGameText");
    if (el) el.style.display = "none";
}

// Show waiting room
function showWaitingRoom(playersCount = 1) {
    document.getElementById("roomSelection").style.display = "none";
    document.getElementById("waitingRoom").style.display = "block";
    document.getElementById("currentRoomName").textContent = currentRoomName;
    document.getElementById("playerCount").textContent = playersCount;
}

// Update player count in waiting room
function updatePlayerCount(count) {
    const playerCountElement = document.getElementById("playerCount");
    if (playerCountElement) {
        playerCountElement.textContent = count;
    }
}

// Notification bar (for game events)
function showNotification(message, type = "info") {
    const notificationBar = document.getElementById("notificationBar");
    const notificationText = document.getElementById("notificationText");
    
    if (notificationBar && notificationText) {
        notificationText.innerHTML = message;
        notificationBar.className = `notification-bar ${type}`;
    }
}

// Show last guess result (HI/LO - always visible like solo)
function showLastGuessResult(message, result) {
    const lastGuessResult = document.getElementById("lastGuessResult");
    if (lastGuessResult) {
        lastGuessResult.textContent = message;
        
        // Apply style based on result (like solo)
        if (result === 'Higher') {
            lastGuessResult.className = "feedback-message hint-higher";
        } else if (result === 'Lower') {
            lastGuessResult.className = "feedback-message hint-lower";
        } else {
            lastGuessResult.className = "feedback-message hint";
        }
        
        lastGuessResult.style.display = "block";
    }
}

function showErrorMessage(message) {
    const errorDiv = document.getElementById("errorMessage");
    if (errorDiv) {
        errorDiv.textContent = message;
        errorDiv.style.display = "block";
        setTimeout(() => {
            errorDiv.style.display = "none";
        }, 5000);
    }

    // Also show in room selection if visible
    const roomsList = document.getElementById("roomsList");
    if (roomsList && document.getElementById("roomSelection").style.display !== "none") {
        roomsList.innerHTML = `<p class="error-message">${message}</p>`;
    }
}

function disableGuessForm() {
    document.getElementById("guessInput").disabled = true;
    document.getElementById("guessBtn").disabled = true;
    updateGameStatus();
}

function enableGuessForm() {
    if (!hasGuessedThisRound) {
        document.getElementById("guessInput").disabled = false;
        document.getElementById("guessBtn").disabled = false;
        document.getElementById("guessInput").focus();
    }
    updateGameStatus();
}

// Event listeners
document.addEventListener("DOMContentLoaded", () => {
    // Load player name from localStorage
    const savedName = localStorage.getItem('hiLoPlayerName');
    if (savedName) {
        currentPlayerName = savedName;
        document.getElementById("displayPlayerName").textContent = savedName;
        initializeConnection();
    } else {
        // Redirect to home if no player name is set
        window.location.href = '/';
        return;
    }

    document.getElementById("createRoomBtn").addEventListener("click", createRoom);
    document.getElementById("refreshRoomsBtn").addEventListener("click", loadAvailableRooms);
    document.getElementById("guessForm").addEventListener("submit", makeGuess);
    
    const leaveRoomBtn = document.getElementById("leaveRoomBtn");
    if (leaveRoomBtn) {
        leaveRoomBtn.addEventListener("click", () => {
            window.location.reload();
        });
    }
    const playAgainBtn = document.getElementById("playAgainBtn");
    if (playAgainBtn) {
        playAgainBtn.addEventListener("click", playAgain);
    }
});

