#!/usr/bin/env bash
set -euo pipefail

###############################################################################
# Déploiement HiLoGame ASP.NET vers le VPS avec Docker
# - Transfert des fichiers via rsync
# - Build et déploiement de l'image Docker
# - Lancement du conteneur
#
# Prérequis :
# - rsync et ssh disponibles en local
# - Accès SSH sans mot de passe (clé autorisée) vers le VPS
# - Docker installé sur le VPS
###############################################################################

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Paramètres VPS
REMOTE_USER="root"
REMOTE_HOST="217.65.145.248"
REMOTE_DIR="/opt/hilogame"
CONTAINER_NAME="hilogame"
IMAGE_NAME="hilogame:latest"
DOCKER_PORT="5000"
HOST_PORT="5000"

# Couleurs pour les messages
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Vérifier si rsync et ssh sont installés
if ! command -v rsync &> /dev/null; then
    print_error "rsync n'est pas installé. Installez-le avec: brew install rsync"
    exit 1
fi

if ! command -v ssh &> /dev/null; then
    print_error "ssh n'est pas installé. Installez-le avec: brew install openssh"
    exit 1
fi

# Optionnel : chemin de clé SSH dédiée
SSH_KEY="${SSH_KEY:-$HOME/.ssh/id_ed25519_scacode}"
RSYNC_RSH="ssh -i ${SSH_KEY}"

print_status "🚀 Déploiement de HiLoGame en cours..."

# Vérifier que le Dockerfile existe
if [ ! -f "${PROJECT_DIR}/Dockerfile" ]; then
    print_error "Dockerfile non trouvé dans ${PROJECT_DIR}"
    exit 1
fi

# Exclure certains fichiers/dossiers lors du transfert
EXCLUDE_PATTERNS=(
    "--exclude=.git"
    "--exclude=bin"
    "--exclude=obj"
    "--exclude=.vs"
    "--exclude=.vscode"
    "--exclude=.idea"
    "--exclude=*.user"
    "--exclude=.DS_Store"
    "--exclude=node_modules"
)

print_status "Transfert des fichiers vers le VPS..."
print_status "Host: ${REMOTE_HOST}"
print_status "Target directory: ${REMOTE_DIR}"
print_status "Container port: ${DOCKER_PORT} -> Host port: ${HOST_PORT}"

# Synchroniser les fichiers (sauf bin/obj)
if rsync -az --delete "${EXCLUDE_PATTERNS[@]}" -e "${RSYNC_RSH}" "${PROJECT_DIR}/" "${REMOTE_USER}@${REMOTE_HOST}:${REMOTE_DIR}/"; then
    print_success "✓ Fichiers transférés"
else
    print_error "Erreur lors du transfert"
    exit 1
fi

print_status "Arrêt et suppression de l'ancien conteneur (s'il existe)..."
ssh -i "${SSH_KEY}" "${REMOTE_USER}@${REMOTE_HOST}" "docker stop ${CONTAINER_NAME} 2>/dev/null || true" || true
ssh -i "${SSH_KEY}" "${REMOTE_USER}@${REMOTE_HOST}" "docker rm ${CONTAINER_NAME} 2>/dev/null || true" || true

print_status "Construction de l'image Docker..."
if ssh -i "${SSH_KEY}" "${REMOTE_USER}@${REMOTE_HOST}" "cd ${REMOTE_DIR} && docker build -t ${IMAGE_NAME} ."; then
    print_success "✓ Image Docker construite"
else
    print_error "Erreur lors de la construction de l'image Docker"
    exit 1
fi

# Nettoyer les anciennes images (optionnel, pour économiser l'espace)
print_status "Nettoyage des anciennes images Docker..."
ssh -i "${SSH_KEY}" "${REMOTE_USER}@${REMOTE_HOST}" "docker image prune -f" || true

print_status "Lancement du conteneur Docker..."
if ssh -i "${SSH_KEY}" "${REMOTE_USER}@${REMOTE_HOST}" "docker run -d \
    --name ${CONTAINER_NAME} \
    --restart unless-stopped \
    -p ${HOST_PORT}:${DOCKER_PORT} \
    -e ASPNETCORE_ENVIRONMENT=Production \
    -e PORT=${DOCKER_PORT} \
    ${IMAGE_NAME}"; then
    print_success "✓ Conteneur lancé"
else
    print_error "Erreur lors du lancement du conteneur"
    exit 1
fi

# Vérifier que le conteneur tourne
sleep 2
if ssh -i "${SSH_KEY}" "${REMOTE_USER}@${REMOTE_HOST}" "docker ps | grep ${CONTAINER_NAME} > /dev/null"; then
    print_success "✓ Conteneur en cours d'exécution"
else
    print_error "Le conteneur ne semble pas démarré. Vérifiez les logs avec:"
    echo "  ssh -i ${SSH_KEY} ${REMOTE_USER}@${REMOTE_HOST} 'docker logs ${CONTAINER_NAME}'"
    exit 1
fi

print_success "Déploiement terminé avec succès ! 🎉"
print_success "Votre application est accessible sur: http://${REMOTE_HOST}:${HOST_PORT}"
print_status ""
print_status "Commandes utiles:"
print_status "  - Voir les logs: ssh -i ${SSH_KEY} ${REMOTE_USER}@${REMOTE_HOST} 'docker logs -f ${CONTAINER_NAME}'"
print_status "  - Arrêter: ssh -i ${SSH_KEY} ${REMOTE_USER}@${REMOTE_HOST} 'docker stop ${CONTAINER_NAME}'"
print_status "  - Redémarrer: ssh -i ${SSH_KEY} ${REMOTE_USER}@${REMOTE_HOST} 'docker restart ${CONTAINER_NAME}'"
