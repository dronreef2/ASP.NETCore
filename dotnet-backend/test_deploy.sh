#!/bin/bash
echo "Iniciando servidor..."
dotnet run --urls=http://localhost:5000 &
SERVER_PID=$!
sleep 5

echo "Testando deploy manual..."
curl -X POST http://localhost:5000/api/webhook/manual-deploy \
  -H "Content-Type: application/json" \
  -d '{"repositoryUrl": "https://github.com/example/test-repo", "branch": "main", "author": "Test User"}' \
  -w "\nStatus: %{http_code}\nTime: %{time_total}s\n"

echo "Aguardando processamento..."
sleep 3

echo "Verificando deployments..."
curl -X GET http://localhost:5000/api/webhook/deployments

echo "Parando servidor..."
kill $SERVER_PID
