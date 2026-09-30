#!/bin/bash
# Funciona em macOS e Linux.
# Sobe a infra (Docker) e os 3 projetos, cada um em uma janela própria do navegador.
# Ctrl+C encerra os 3 projetos (a infra Docker continua no ar).
cd "$(dirname "$0")/../.." || exit 1

APPS=(
  "Identity|projects/Identity/Identity.WebApi|5191"
  "Transaction|projects/Transaction/Transaction.WebApi|5107"
  "BalanceProjection|projects/BalanceProjection/BalanceProjection.WebApi|5192"
)

show_urls() {
  echo
  echo "URLs:"
  for app in "${APPS[@]}"; do
    IFS='|' read -r name dir port <<< "$app"
    printf "  %-18s http://localhost:%s\n" "$name" "$port"
  done
  printf "  %-18s http://localhost:15672\n" "RabbitMQ"
  echo
}

open_window() {
  if [ "$(uname)" = "Darwin" ]; then
    if [ -d "/Applications/Google Chrome.app" ]; then
      open -a "Google Chrome" --args --new-window "$1"
    else
      open "$1"
    fi
  elif command -v google-chrome > /dev/null; then
    google-chrome --new-window "$1" > /dev/null 2>&1 &
  elif command -v chromium > /dev/null; then
    chromium --new-window "$1" > /dev/null 2>&1 &
  else
    xdg-open "$1" > /dev/null 2>&1 &
  fi
}

command -v docker > /dev/null || { echo "Docker não encontrado."; exit 1; }
command -v dotnet > /dev/null || { echo ".NET SDK não encontrado."; exit 1; }

show_urls
mkdir -p run/logs
docker compose up -d postgres rabbitmq || exit 1

pids=()
trap 'echo; echo "Encerrando..."; kill "${pids[@]}" 2>/dev/null; exit 0' INT TERM

for app in "${APPS[@]}"; do
  IFS='|' read -r name dir port <<< "$app"
  ASPNETCORE_ENVIRONMENT=Development dotnet run --project "$dir" --urls "http://localhost:$port" \
    > "run/logs/$name.log" 2>&1 &
  pids+=($!)
done

for app in "${APPS[@]}"; do
  IFS='|' read -r name dir port <<< "$app"
  printf "Aguardando %s (porta %s)" "$name" "$port"
  until curl -fs "http://localhost:$port/" > /dev/null 2>&1; do
    printf "."; sleep 1
  done
  echo " ok"
  open_window "http://localhost:$port/"
  sleep 1
done

echo "Tudo no ar. Logs em run/logs/. Ctrl+C para encerrar."
show_urls
wait
