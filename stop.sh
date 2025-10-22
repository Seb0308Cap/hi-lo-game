#!/bin/bash

# Script d'arrêt Hi-Lo Game
# Usage: ./stop.sh

echo "Stopping Hi-Lo Game..."

# Tuer tous les processus dotnet HiLoGame
pkill -f "dotnet.*HiLoGame" 2>/dev/null

# Tuer tout ce qui utilise le port 5000
if lsof -ti:5000 > /dev/null 2>&1; then
    echo "Killing process on port 5000..."
    lsof -ti:5000 | xargs kill -9 2>/dev/null
fi

echo "All processes stopped."

