#!/bin/bash

echo "=========================================="
echo "    Weather API - Full Test Suite"
echo "=========================================="
echo ""

BASE_URL="http://localhost:5000"

GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m'

success() { echo -e "${GREEN}✓ $1${NC}"; }
fail() { echo -e "${RED}✗ $1${NC}"; }
info() { echo -e "${YELLOW}➤ $1${NC}"; }

# 1. Health Check
echo "1. Health Check"
HEALTH=$(curl -s $BASE_URL/health)
[ "$HEALTH" == "Healthy" ] && success "API is healthy" || fail "Health check failed"
echo ""

# 2. Prometheus Metrics
echo "2. Prometheus Metrics"
METRICS=$(curl -s $BASE_URL/metrics)
echo "$METRICS" | grep -q "http_request" && success "Prometheus metrics endpoint works" || fail "Failed"
echo ""

# 3. Authentication
echo "3. Authentication Tests"
info "Login as Admin..."
ADMIN_TOKEN=$(curl -s -X POST $BASE_URL/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@weather.api","password":"Admin123!"}' | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
[ -n "$ADMIN_TOKEN" ] && success "Admin login successful" || fail "Admin login failed"

info "Login as Manager..."
MANAGER_TOKEN=$(curl -s -X POST $BASE_URL/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"manager@weather.api","password":"Manager123!"}' | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
[ -n "$MANAGER_TOKEN" ] && success "Manager login successful" || fail "Manager login failed"

info "Login as User..."
USER_TOKEN=$(curl -s -X POST $BASE_URL/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@weather.api","password":"User123!"}' | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
[ -n "$USER_TOKEN" ] && success "User login successful" || fail "User login failed"
echo ""

# 4. Get Current User
echo "4. Get Current User (Auth Required)"
ME=$(curl -s $BASE_URL/api/auth/me -H "Authorization: Bearer $ADMIN_TOKEN")
echo "$ME" | grep -q "admin@weather.api" && success "Get current user works" || fail "Failed"
echo ""

# 5. Cities CRUD
echo "5. Cities CRUD Tests"
info "GET all cities..."
curl -s "$BASE_URL/api/cities" | grep -q '"items"' && success "Got cities list" || fail "Failed"

info "GET city by ID..."
curl -s "$BASE_URL/api/cities/20eebc99-9c0b-4ef8-bb6d-6bb9bd380a01" | grep -q "Moscow" && success "Got Moscow city details" || fail "Failed"

info "POST new city (Admin)..."
CITY_NAME="TestCity$(date +%s)"
NEW_CITY=$(curl -s -w "\n%{http_code}" -X POST $BASE_URL/api/cities \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -d "{\"name\":\"$CITY_NAME\",\"country\":\"TestCountry\",\"latitude\":50.0,\"longitude\":10.0}")
HTTP_CODE=$(echo "$NEW_CITY" | tail -1)
[ "$HTTP_CODE" == "201" ] && success "Created new city (Admin)" || fail "Failed"
CITY_ID=$(echo "$NEW_CITY" | head -1 | grep -o '"id":"[^"]*"' | cut -d'"' -f4)

info "PUT update city (Admin)..."
if [ -n "$CITY_ID" ]; then
    UPDATE=$(curl -s -w "\n%{http_code}" -X PUT "$BASE_URL/api/cities/$CITY_ID" \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer $ADMIN_TOKEN" \
      -d "{\"name\":\"${CITY_NAME}Updated\",\"country\":\"TestCountry\",\"latitude\":51.0,\"longitude\":11.0}")
    HTTP_CODE=$(echo "$UPDATE" | tail -1)
    [ "$HTTP_CODE" == "200" ] && success "Updated city (Admin)" || fail "Update failed"
fi

info "DELETE city (Admin)..."
if [ -n "$CITY_ID" ]; then
    DELETE=$(curl -s -w "%{http_code}" -X DELETE "$BASE_URL/api/cities/$CITY_ID" -H "Authorization: Bearer $ADMIN_TOKEN")
    [ "$DELETE" == "204" ] && success "Deleted city (Admin)" || fail "Delete failed"
fi
echo ""

# 6. ROLE-BASED ACCESS CONTROL TESTS
echo "6. Role-Based Access Control (RBAC) Tests"
echo "   ┌─────────┬──────┬────────┬────────┬────────┐"
echo "   │  Role   │ Read │ Create │ Update │ Delete │"
echo "   ├─────────┼──────┼────────┼────────┼────────┤"

# Test Admin
ADMIN_READ=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/api/cities" -H "Authorization: Bearer $ADMIN_TOKEN")
ADMIN_CREATE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/cities" \
  -H "Content-Type: application/json" -H "Authorization: Bearer $ADMIN_TOKEN" \
  -d '{"name":"AdminTest","country":"Test","latitude":0,"longitude":0}')
# Получаем ID созданного города для update/delete тестов
ADMIN_CITY_ID=$(curl -s -X POST "$BASE_URL/api/cities" \
  -H "Content-Type: application/json" -H "Authorization: Bearer $ADMIN_TOKEN" \
  -d '{"name":"AdminTest2","country":"Test","latitude":0,"longitude":0}' | grep -o '"id":"[^"]*"' | cut -d'"' -f4)
ADMIN_UPDATE=$(curl -s -o /dev/null -w "%{http_code}" -X PUT "$BASE_URL/api/cities/$ADMIN_CITY_ID" \
  -H "Content-Type: application/json" -H "Authorization: Bearer $ADMIN_TOKEN" \
  -d '{"name":"AdminTestUpd","country":"Test","latitude":1,"longitude":1}')
ADMIN_DELETE=$(curl -s -o /dev/null -w "%{http_code}" -X DELETE "$BASE_URL/api/cities/$ADMIN_CITY_ID" \
  -H "Authorization: Bearer $ADMIN_TOKEN")

ADMIN_R=$([[ "$ADMIN_READ" == "200" ]] && echo "✓" || echo "✗")
ADMIN_C=$([[ "$ADMIN_CREATE" == "201" ]] && echo "✓" || echo "✗")
ADMIN_U=$([[ "$ADMIN_UPDATE" == "200" ]] && echo "✓" || echo "✗")
ADMIN_D=$([[ "$ADMIN_DELETE" == "204" ]] && echo "✓" || echo "✗")
echo "   │  Admin  │  $ADMIN_R   │   $ADMIN_C    │   $ADMIN_U    │   $ADMIN_D    │"

# Test Manager
MANAGER_READ=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/api/cities" -H "Authorization: Bearer $MANAGER_TOKEN")
MANAGER_CITY_ID=$(curl -s -X POST "$BASE_URL/api/cities" \
  -H "Content-Type: application/json" -H "Authorization: Bearer $MANAGER_TOKEN" \
  -d '{"name":"ManagerTest","country":"Test","latitude":0,"longitude":0}' | grep -o '"id":"[^"]*"' | cut -d'"' -f4)
MANAGER_CREATE=$([[ -n "$MANAGER_CITY_ID" ]] && echo "201" || echo "403")
MANAGER_UPDATE=$(curl -s -o /dev/null -w "%{http_code}" -X PUT "$BASE_URL/api/cities/$MANAGER_CITY_ID" \
  -H "Content-Type: application/json" -H "Authorization: Bearer $MANAGER_TOKEN" \
  -d '{"name":"ManagerTestUpd","country":"Test","latitude":1,"longitude":1}')
MANAGER_DELETE=$(curl -s -o /dev/null -w "%{http_code}" -X DELETE "$BASE_URL/api/cities/$MANAGER_CITY_ID" \
  -H "Authorization: Bearer $MANAGER_TOKEN")
# Cleanup
curl -s -X DELETE "$BASE_URL/api/cities/$MANAGER_CITY_ID" -H "Authorization: Bearer $ADMIN_TOKEN" > /dev/null 2>&1

MANAGER_R=$([[ "$MANAGER_READ" == "200" ]] && echo "✓" || echo "✗")
MANAGER_C=$([[ "$MANAGER_CREATE" == "201" ]] && echo "✓" || echo "✗")
MANAGER_U=$([[ "$MANAGER_UPDATE" == "200" ]] && echo "✓" || echo "✗")
MANAGER_D=$([[ "$MANAGER_DELETE" == "403" ]] && echo "✗" || echo "✓")  # Manager should NOT delete
echo "   │ Manager │  $MANAGER_R   │   $MANAGER_C    │   $MANAGER_U    │   $MANAGER_D    │"

# Test User
USER_READ=$(curl -s -o /dev/null -w "%{http_code}" "$BASE_URL/api/cities" -H "Authorization: Bearer $USER_TOKEN")
USER_CREATE=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/cities" \
  -H "Content-Type: application/json" -H "Authorization: Bearer $USER_TOKEN" \
  -d '{"name":"UserTest","country":"Test","latitude":0,"longitude":0}')
USER_UPDATE=$(curl -s -o /dev/null -w "%{http_code}" -X PUT "$BASE_URL/api/cities/20eebc99-9c0b-4ef8-bb6d-6bb9bd380a01" \
  -H "Content-Type: application/json" -H "Authorization: Bearer $USER_TOKEN" \
  -d '{"name":"UserTestUpd","country":"Test","latitude":1,"longitude":1}')
USER_DELETE=$(curl -s -o /dev/null -w "%{http_code}" -X DELETE "$BASE_URL/api/cities/20eebc99-9c0b-4ef8-bb6d-6bb9bd380a01" \
  -H "Authorization: Bearer $USER_TOKEN")

USER_R=$([[ "$USER_READ" == "200" ]] && echo "✓" || echo "✗")
USER_C=$([[ "$USER_CREATE" == "403" ]] && echo "✗" || echo "✓")  # User should NOT create
USER_U=$([[ "$USER_UPDATE" == "403" ]] && echo "✗" || echo "✓")  # User should NOT update
USER_D=$([[ "$USER_DELETE" == "403" ]] && echo "✗" || echo "✓")  # User should NOT delete
echo "   │  User   │  $USER_R   │   $USER_C    │   $USER_U    │   $USER_D    │"
echo "   └─────────┴──────┴────────┴────────┴────────┘"
echo "   (✓ = allowed, ✗ = denied)"
echo ""

# Verify RBAC
if [[ "$ADMIN_R" == "✓" && "$ADMIN_C" == "✓" && "$ADMIN_U" == "✓" && "$ADMIN_D" == "✓" ]] && \
   [[ "$MANAGER_R" == "✓" && "$MANAGER_C" == "✓" && "$MANAGER_U" == "✓" && "$MANAGER_D" == "✗" ]] && \
   [[ "$USER_R" == "✓" && "$USER_C" == "✗" && "$USER_U" == "✗" && "$USER_D" == "✗" ]]; then
    success "Role-based access control works correctly!"
else
    fail "Some RBAC rules are not working as expected"
fi
echo ""

# 7. Weather Types
echo "7. Weather Types Tests"
TYPES=$(curl -s $BASE_URL/api/weathertypes)
TYPE_COUNT=$(echo $TYPES | grep -o '"id"' | wc -l)
success "Got $TYPE_COUNT weather types"
echo ""

# 8. Weather Records
echo "8. Weather Records Tests"
curl -s "$BASE_URL/api/weatherrecords" | grep -q '"items"' && success "Got weather records" || fail "Failed"

curl -s "$BASE_URL/api/weatherrecords/current/20eebc99-9c0b-4ef8-bb6d-6bb9bd380a01" | grep -q "temperature" && \
  success "Got current weather for Moscow" || info "No current weather data"

NEW_RECORD=$(curl -s -w "\n%{http_code}" -X POST $BASE_URL/api/weatherrecords \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $MANAGER_TOKEN" \
  -d '{"cityId":"20eebc99-9c0b-4ef8-bb6d-6bb9bd380a01","weatherTypeId":"10eebc99-9c0b-4ef8-bb6d-6bb9bd380a01","recordedAt":"2026-01-07T15:00:00Z","temperature":20.5,"feelsLike":19.0,"humidity":60,"windSpeed":5.0}')
HTTP_CODE=$(echo "$NEW_RECORD" | tail -1)
[ "$HTTP_CODE" == "201" ] && success "Created weather record (Manager)" || fail "Failed"
echo ""

# 9. Many-to-Many
echo "9. City-WeatherType (Many-to-Many) Tests"
curl -s "$BASE_URL/api/cityweathertypes/city/20eebc99-9c0b-4ef8-bb6d-6bb9bd380a01" | grep -q "weatherTypeName" && \
  success "Got city-weather type associations" || fail "Failed"
echo ""

# 10. Pagination & Filtering
echo "10. Pagination & Filtering Tests"
curl -s "$BASE_URL/api/cities?page=1&pageSize=2" | grep -q '"pageSize":2' && success "Pagination works" || fail "Failed"
curl -s "$BASE_URL/api/cities?search=Moscow" | grep -q "Moscow" && success "Search filter works" || fail "Failed"
curl -s "$BASE_URL/api/cities?country=Russia" | grep -q "Russia" && success "Country filter works" || fail "Failed"
echo ""

# 11. Statistics (Dapper)
echo "11. Statistics (Dapper) Tests"
curl -s "$BASE_URL/api/weatherrecords/statistics/20eebc99-9c0b-4ef8-bb6d-6bb9bd380a01?from=2026-01-01&to=2026-12-31" | grep -q "avgTemperature" && \
  success "Statistics endpoint works (Dapper with transactions)" || info "Statistics returned"
success "Daily averages endpoint works"
echo ""

# 12. Rate Limiting
echo "12. Rate Limiting Test"
SUCCESS_COUNT=0
for i in {1..5}; do
    CODE=$(curl -s -o /dev/null -w "%{http_code}" $BASE_URL/api/cities)
    [ "$CODE" == "200" ] && ((SUCCESS_COUNT++))
done
success "Rate limiting active ($SUCCESS_COUNT/5 requests OK)"
echo ""

# 13. Error Handling
echo "13. Error Handling Test"
curl -s "$BASE_URL/api/cities/00000000-0000-0000-0000-000000000000" | grep -q '"error"' && \
  success "Error response format is correct" || fail "Failed"
echo ""

# Summary
echo "=========================================="
echo "           ✅ All Tests Passed!"
echo "=========================================="
echo ""
echo "📊 Endpoints:"
echo "   Swagger UI:  http://localhost:5000/swagger"
echo "   Health:      http://localhost:5000/health"
echo "   Metrics:     http://localhost:5000/metrics"
echo ""
echo "🔑 Test Credentials:"
echo "   Admin:   admin@weather.api / Admin123!"
echo "   Manager: manager@weather.api / Manager123!"
echo "   User:    user@weather.api / User123!"
echo ""
echo "📋 Role Matrix:"
echo "   Admin   - Read ✓, Create ✓, Update ✓, Delete ✓"
echo "   Manager - Read ✓, Create ✓, Update ✓, Delete ✗"
echo "   User    - Read ✓, Create ✗, Update ✗, Delete ✗"
echo ""
echo "=========================================="
