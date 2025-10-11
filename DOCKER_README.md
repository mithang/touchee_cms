# Docker Compose Setup for Purest Admin

This document explains how to run the Purest Admin backend system using Docker Compose.

## Prerequisites

- Docker Engine 20.10.0 or later
- Docker Compose 1.29.0 or later

## Services Included

1. **MySQL Database** (db): MySQL 8.0 database with pre-loaded schema
2. **API Service** (api): .NET 8.0 backend API

## Quick Start

1. Navigate to the project root directory:
   ```bash
   cd /path/to/touchee_cms
   ```

2. Start all services:
   ```bash
   docker-compose up -d
   ```

3. Access the services:
   - API: http://localhost:8080
   - Database: localhost:3306 (root password: Aa@123456)

## Configuration

### Database
- Database name: `purest`
- Root password: `Aa@123456`
- User: `root`
- Port: `3306`

### API Service
- Port: `8080`
- Environment: Container
- Connection to database is automatically configured

## Useful Commands

### Start services
```bash
docker-compose up -d
```

### View logs
```bash
# View all logs
docker-compose logs

# View specific service logs
docker-compose logs api
docker-compose logs db
```

### Stop services
```bash
docker-compose down
```

### Rebuild services
```bash
docker-compose up -d --build
```

## Notes

1. The database is initialized with the schema from `relationship-model/admin-mysql.sql`
2. Data is persisted in a Docker volume named `db_data`
3. The API service waits for the database to be healthy before starting
4. CORS is configured to allow common development ports (5173, 3000, 8080)

## Troubleshooting

### Database Connection Issues
If the API cannot connect to the database, ensure:
1. The database service is running: `docker-compose ps`
2. The database is healthy: `docker-compose logs db`
3. The connection string in appsettings.Container.json is correct

### API Service Issues
If the API service fails to start:
1. Check the logs: `docker-compose logs api`
2. Ensure all project files are present
3. Verify the Dockerfile builds correctly: `docker build ./api`

### Clean Start
To start fresh (warning: this will delete all data):
```bash
docker-compose down -v
docker-compose up -d --build
```