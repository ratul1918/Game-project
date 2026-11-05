#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUT="$ROOT/docs/load-test"
mkdir -p "$OUT"
docker compose -f "$ROOT/docker-compose.yml" up -d --wait postgres
(cd "$ROOT/server" && mvn -q package -DskipTests && exec java -jar target/lighthouse-server-0.1.0.jar > "$OUT/server.log" 2>&1) &
SERVER_PID=$!
trap 'kill "$SERVER_PID" 2>/dev/null || true; docker compose -f "$ROOT/docker-compose.yml" down' EXIT
for i in {1..60}; do curl -fsS http://localhost:8080/actuator/health >/dev/null && break; sleep 2; done

for bots in 10 50 100 200 500; do
  csv="../docs/load-test/run-${bots}.csv"
  if ! (cd "$ROOT/bot-client" && mvn -q exec:java -Dexec.mainClass=com.thelighthouse.bot.BotClient \
      -Dexec.args="--url http://localhost:8080 --bots $bots --duration ${DURATION_SECONDS:-30} --ramp-up ${RAMP_SECONDS:-10} --csv $csv"); then
    echo "stopped at $bots bots (bot client failed)" | tee "$OUT/stop.txt"
    break
  fi
  curl -fsS -H "Authorization: Bearer ${ADMIN_DEV_TOKEN:-dev-admin-token}" http://localhost:8080/admin/stats > "$OUT/stats-${bots}.json"
done
