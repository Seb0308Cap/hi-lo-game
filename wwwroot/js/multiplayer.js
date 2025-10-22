// SignalR connection
let connection = null;
let currentRoomId = null;
let currentRoomName = null;
let currentPlayerName = null;
let myConnectionId = null;
let hasGuessedThisRound = false;
let currentRound = 1;
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
        hasGuessedThisRound = false;
        currentRound = 1;
        myAttempts = [];
        // Update room name from server data
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
        currentRound = data.roundNumber;
        hasGuessedThisRound = false;
        document.getElementById("roundNumber").textContent = currentRound;
        showNotification(data.message, "success");
        enableGuessForm();
    });

    connection.on("GameCompleted", (data) => {
        console.log("Game completed:", data);
        showGameOver(data);
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
    return `
        <div class="room-item" data-room-id="${room.roomId}">
            <div class="room-info">
                <span class="room-name-text">${room.roomName}</span>
                <span class="room-range">[${room.minNumber} - ${room.maxNumber}]</span>
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

    try {
        const result = await connection.invoke("CreateRoom", roomName, currentPlayerName, minNumber, maxNumber);
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

// Start game
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

    // Display players
    const playersList = document.getElementById("playersList");
    playersList.innerHTML = data.players.map(p => 
        `<span class="player-badge ${p.name === currentPlayerName ? 'current-player' : ''}">${p.name}</span>`
    ).join('');

    document.getElementById("roundNumber").textContent = currentRound;
    
    // Clear last guess result on game start
    const lastGuessResult = document.getElementById("lastGuessResult");
    if (lastGuessResult) {
        lastGuessResult.style.display = "none";
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

    // Personalized message based on if you won or lost
    const isWinner = data.winnerName === currentPlayerName;
    const winnerNameElement = document.getElementById("winnerName");
    
    if (isWinner) {
        winnerNameElement.innerHTML = '<i class="fas fa-trophy"></i> You won!';
        winnerNameElement.className = "winner";
    } else {
        winnerNameElement.innerHTML = '<i class="fas fa-times-circle"></i> You lost!';
        winnerNameElement.className = "loser";
    }
    
    document.getElementById("revealedNumber").textContent = data.mysteryNumber;

    const playersResults = document.getElementById("playersResults");
    playersResults.innerHTML = `
        <h3>Game Statistics</h3>
        ${data.players.map(player => `
            <div class="player-result ${player.isWinner ? 'winner-result' : 'loser-result'}">
                <h4>${player.name} ${player.isWinner ? '<i class="fas fa-trophy"></i>' : ''}</h4>
                <p>Attempts: ${player.attempts}</p>
                <div class="attempts-history">
                    <h5>Guess History:</h5>
                    <div class="attempts-list">
                        ${player.guessAttempts.map(attempt => `
                            <div class="attempt-item attempt-${attempt.result.toLowerCase()}">
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
});

