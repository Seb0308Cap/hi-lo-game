#!/bin/bash

# Script de démarrage Hi-Lo Game
# Usage: ./start.sh

echo "Hi-Lo Game - Starting..."

# Tuer les processus existants sur le port 5000
echo "Checking port 5000..."
if lsof -ti:5000 > /dev/null 2>&1; then
    echo "Port 5000 is in use, killing existing process..."
    lsof -ti:5000 | xargs kill -9 2>/dev/null
    sleep 1
fi

# Démarrer l'application
echo "Starting application on http://localhost:5000"
dotnet run

echo ""
echo "Application stopped."

