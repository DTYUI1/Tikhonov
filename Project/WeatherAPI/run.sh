#!/bin/bash

echo "🚀 Starting Weather API..."

# Останавливаем предыдущие контейнеры
docker-compose down -v

# Собираем и запускаем
docker-compose up --build -d

echo "⏳ Waiting for services to start..."
sleep 15

# Проверяем health
echo "🏥 Checking health..."
curl -s http://localhost:5000/health | jq .

echo ""
echo "✅ Weather API is running!"
echo "📚 Swagger UI: http://localhost:5000/swagger"
echo ""
echo "🔑 Test credentials:"
echo "   Admin: admin@weather.api / Admin123!"
echo "   Manager: manager@weather.api / Manager123!"
echo "   User: user@weather.api / User123!"
