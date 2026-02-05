# Server Lifecycle Management

Scripts for starting/stopping backend and frontend servers during E2E tests.

## Find Available Ports

Avoid conflicts with running services:

```bash
find_port() {
  node -e "const s=require('net').createServer();s.listen(0,()=>{console.log(s.address().port);s.close()})"
}

export E2E_BACKEND_PORT=$(find_port)
export E2E_FRONTEND_PORT=$(find_port)
echo "Using ports: Backend=$E2E_BACKEND_PORT, Frontend=$E2E_FRONTEND_PORT"
```

## Start Backend

```bash
cd backend/Api && dotnet run --urls "http://localhost:$E2E_BACKEND_PORT" &
BACKEND_PID=$!

# Wait for ready (max 60s)
for i in {1..60}; do
  curl -s "http://localhost:$E2E_BACKEND_PORT/health" && break
  sleep 1
done
```

## Start Frontend

```bash
cd frontend && npm run dev -- --port $E2E_FRONTEND_PORT &
FRONTEND_PID=$!

# Wait for ready (max 30s)
for i in {1..30}; do
  curl -s "http://localhost:$E2E_FRONTEND_PORT" && break
  sleep 1
done
```

## Run Tests

```bash
cd frontend && npx playwright test --reporter=html,json
```

Playwright reads `E2E_BACKEND_PORT` and `E2E_FRONTEND_PORT` from environment.

## Teardown

```bash
kill $BACKEND_PID $FRONTEND_PID 2>/dev/null || true
```
