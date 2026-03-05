# OpenTelemetry .NET Example

[![CI](https://img.shields.io/github/actions/workflow/status/devops-thiago/otel-core-example/ci.yml?branch=main&label=CI)](https://github.com/devops-thiago/otel-core-example/actions)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/github/license/devops-thiago/otel-core-example)](LICENSE)
[![Codecov](https://img.shields.io/codecov/c/github/devops-thiago/otel-core-example?label=coverage)](https://app.codecov.io/gh/devops-thiago/otel-core-example)
[![Sonar Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=devops-thiago_otel-core-example&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=devops-thiago_otel-core-example)
[![Sonar Coverage](https://sonarcloud.io/api/project_badges/measure?project=devops-thiago_otel-core-example&metric=coverage)](https://sonarcloud.io/summary/new_code?id=devops-thiago_otel-core-example)
[![OpenTelemetry](https://img.shields.io/badge/OpenTelemetry-enabled-blue?logo=opentelemetry)](https://opentelemetry.io)
[![Docker](https://img.shields.io/badge/Docker-ready-blue?logo=docker)](https://www.docker.com)
[![Docker Hub](https://img.shields.io/docker/v/thiagosg/otel-crud-api-net-core?logo=docker&label=Docker%20Hub)](https://hub.docker.com/r/thiagosg/otel-crud-api-net-core)
[![Docker Pulls](https://img.shields.io/docker/pulls/thiagosg/otel-crud-api-net-core)](https://hub.docker.com/r/thiagosg/otel-crud-api-net-core)

A production-ready .NET REST API example demonstrating user CRUD operations with OpenTelemetry integration and Alloy for observability.

## Features

- **User Management**: Complete CRUD operations for users
- **In-Memory Database**: Entity Framework Core with In-Memory database
- **OpenTelemetry Integration**: Automatic instrumentation for traces, metrics, and logs
- **Alloy Integration**: Sends telemetry data to Grafana Alloy
- **Docker Support**: Easy deployment with Docker and Docker Compose
- **API Documentation**: Swagger/OpenAPI documentation
- **Health Checks**: Built-in health check endpoint

## API Endpoints

- `GET /api/user` - Get all users
- `GET /api/user/{id}` - Get user by ID
- `POST /api/user` - Create new user
- `PUT /api/user/{id}` - Update user
- `DELETE /api/user/{id}` - Delete user
- `GET /health` - Health check

## Quick Start

### Full Observability Stack

Start the complete observability stack with one command:

```bash
# Start the complete stack
docker-compose up -d
```

This provides:
- **API**: http://localhost:8080 - Your .NET Core API
- **Grafana**: http://localhost:3000 - Dashboards and visualization (admin/admin)
- **Alloy UI**: http://localhost:12345 - Telemetry collection status
- **MinIO Console**: http://localhost:9001 - Object storage (admin/password123)
- **Tempo**: http://localhost:3200 - Distributed tracing backend
- **Mimir**: http://localhost:9009 - Metrics storage backend
- **Loki**: http://localhost:3100 - Log aggregation backend

### Generate Test Data

```bash
# Create test data
curl -X POST http://localhost:8080/api/user \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Test","lastName":"User","email":"test@example.com","phoneNumber":"+1234567890"}'

# Make some requests to generate traces
curl http://localhost:8080/api/user
curl http://localhost:8080/api/user/1
```

### Option 3: Pre-built Docker Image

Using the published Docker image:

```bash
# Pull and run the latest image
docker run -p 8080:8080 thiagosg/otel-crud-api-net-core:latest

# Or with basic Alloy for observability
docker run -d --name alloy -p 4320:4320 -p 4321:4321 -p 12345:12345 grafana/alloy:latest
docker run -p 8080:8080 --link alloy -e OpenTelemetry__OtlpEndpoint=http://alloy:4320 thiagosg/otel-crud-api-net-core:latest
```

### Manual Setup

1. **Prerequisites**:
   - .NET 9 SDK
   - Docker (for Alloy)

2. **Start Alloy**:
```bash
docker run -d --name alloy -p 4320:4320 -p 4321:4321 -p 12345:12345 \
  -v $(pwd)/alloy.config:/.config/alloy/config.alloy \
  grafana/alloy:latest run --server.http.listen-addr=0.0.0.0:12345 \
  --storage.path=/var/lib/alloy/data /.config/alloy/config.alloy
```

3. **Run the API**:
```bash
dotnet restore
dotnet run
```

The API will be available at:
- API: http://localhost:5000
- Swagger UI: http://localhost:5000

## 🔍 Observability Features

This project demonstrates enterprise-grade observability using the **OpenTelemetry** standard with **Grafana's LGTM stack** (Loki, Grafana, Tempo, Mimir).

### 📊 **What's Included:**

#### **Distributed Tracing** (Tempo)
- ✅ Automatic HTTP request tracing
- ✅ Database operation tracing with EF Core
- ✅ Custom activity sources for business operations
- ✅ Exception tracking and error correlation
- ✅ Cross-service trace correlation

#### **Metrics Collection** (Mimir)
- ✅ HTTP request metrics (duration, status codes, throughput)
- ✅ Database operation metrics
- ✅ .NET runtime metrics (GC, memory, CPU, threads)
- ✅ Process metrics (uptime, resource usage)
- ✅ Custom business metrics

#### **Structured Logging** (Loki)
- ✅ Structured logging with Serilog
- ✅ Log correlation with traces (trace/span IDs)
- ✅ Automatic log forwarding to Alloy
- ✅ JSON structured output for better parsing
- ✅ Log levels and filtering

#### **Health Monitoring**
- ✅ `/health` - Detailed health check with database status
- ✅ `/info` - Service information (version, uptime, environment)
- ✅ `/metrics` - Prometheus-compatible metrics endpoint

### 🏗️ **Architecture Overview**

This project implements the **LGTM Stack** (Loki, Grafana, Tempo, Mimir) for comprehensive observability:

```
┌─────────────┐    ┌─────────────┐
│  ASP.NET    │    │   Grafana   │
│    Core     │───▶│   Alloy     │
│    App      │    │ Collector   │
└─────────────┘    └─────────────┘
                          │
            ┌─────────────┼─────────────┐
            ▼             ▼             ▼
    ┌─────────────┐ ┌─────────────┐ ┌─────────────┐
    │   Tempo     │ │   Mimir     │ │    Loki     │
    │  (Traces)   │ │ (Metrics)   │ │   (Logs)    │
    └─────────────┘ └─────────────┘ └─────────────┘
            │             │             │
            └─────────────┼─────────────┘
                          ▼
                  ┌─────────────┐
                  │   MinIO     │
                  │ (Storage)   │
                  └─────────────┘
```

### **Components**

| Component | Purpose | Port | UI |
|-----------|---------|------|-----|
| **ASP.NET Core App** | Main application | 8080 | http://localhost:8080 |
| **Grafana Alloy** | Telemetry collector | 4320 (gRPC), 4321 (HTTP) | http://localhost:12345 |
| **Tempo** | Distributed tracing | 3200, 4317, 4318 | - |
| **Mimir** | Metrics storage | 9009 | - |
| **Loki** | Log aggregation | 3100 | - |
| **MinIO** | Object storage | 9000 | http://localhost:9001 |
| **Grafana** | Visualization | 3000 | http://localhost:3000 |

### 🚀 **Key Features:**

| Feature | Implementation | Status |
|---------|----------------|---------|
| **Framework** | .NET 9.0 | ✅ |
| **Language** | C# 13 | ✅ |
| **OpenTelemetry** | 1.12.0 | ✅ |
| **Tracing** | Auto + Manual | ✅ |
| **Metrics** | OpenTelemetry | ✅ |
| **Logging** | Serilog + OTEL | ✅ |
| **Health Checks** | ASP.NET Health Checks | ✅ |
| **Database** | EF Core In-Memory | ✅ |
| **Container** | Docker | ✅ |
| **Concurrency** | async/await | ✅ |

### 🚀 **Performance Characteristics:**

- **Container Size**: ~120MB
- **Startup Time**: ~1-2s
- **Memory Usage**: ~80MB
- **Throughput**: High performance with async/await
- **Resource Efficiency**: Optimized for cloud deployments

## Configuration

### Environment Variables

- `OpenTelemetry__OtlpEndpoint`: Alloy endpoint (default: http://localhost:4320)
- `ASPNETCORE_ENVIRONMENT`: Environment (Development/Production)

### Alloy Configuration

The `alloy.config` file configures Alloy to:
- Receive OTLP data on ports 4320 (gRPC) and 4321 (HTTP)
- Process and batch telemetry data
- Export to various backends (configurable)

## Sample Data

The application includes sample users for testing:
- John Doe (john.doe@example.com)
- Jane Smith (jane.smith@example.com)

## 🧪 Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run tests with coverage and generate report
./scripts/test.sh

# Run tests with specific coverage threshold
./scripts/test.sh Release 85
```

### Test Coverage

The project maintains **80%+ code coverage** with comprehensive test suites:

- **Unit Tests**: Service layer testing with mocked dependencies
- **Integration Tests**: Full API endpoint testing with test database
- **Test Utilities**: Helper classes for test data generation
- **AutoFixture**: Automated test data generation
- **FluentAssertions**: Readable test assertions

### Test Structure

```
UserApi.Tests/
├── Controllers/
│   └── UserControllerIntegrationTests.cs
├── Services/
│   └── UserServiceTests.cs
├── TestConfiguration.cs
├── TestUtilities.cs
└── GlobalUsings.cs
```

## Testing the API

### Using curl

```bash
# Get all users
curl http://localhost:8080/api/user

# Create a new user
curl -X POST http://localhost:8080/api/user \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Alice","lastName":"Johnson","email":"alice.johnson@example.com","phoneNumber":"+1234567890"}'

# Get user by ID
curl http://localhost:8080/api/user/1

# Update user
curl -X PUT http://localhost:8080/api/user/1 \
  -H "Content-Type: application/json" \
  -d '{"firstName":"John","lastName":"Doe Updated","email":"john.doe.updated@example.com"}'

# Delete user
curl -X DELETE http://localhost:8080/api/user/1
```

### Using Swagger UI

Visit http://localhost:8080 to access the interactive API documentation.

## Observability

### Alloy UI
Access the Alloy UI at http://localhost:12345 to view:
- Configuration status
- Component health
- Telemetry flow

### Logs
All logs are structured and include:
- Trace correlation IDs
- Service information
- Request/response details
- Error context

### Metrics
Key metrics include:
- HTTP request duration and count
- Database operation metrics
- Application performance metrics
- System resource usage

## 🛠️ Development

### Code Quality

This project enforces high code quality standards:

- **EditorConfig**: Consistent code formatting across editors
- **.NET Analyzers**: Built-in code analysis and best practices
- **SonarQube**: Comprehensive code quality analysis
- **dotnet format**: Automated code formatting
- **TreatWarningsAsErrors**: All warnings treated as errors
- **Nullable Reference Types**: Enabled for better null safety

### Running Code Quality Checks

```bash
# Format code
dotnet format

# Verify formatting (CI check)
dotnet format --verify-no-changes

# Run linting
dotnet build --verbosity normal --configuration Release /p:TreatWarningsAsErrors=true

# Run SonarCloud analysis locally (example)
dotnet sonarscanner begin \
  /k:"devops-thiago_otel-core-example" \
  /o:"devops-thiago" \
  /d:sonar.host.url="https://sonarcloud.io" \
  /d:sonar.token="your-token"
dotnet build
dotnet test --collect:"XPlat Code Coverage"
dotnet sonarscanner end /d:sonar.token="your-token"
```

## Project Structure

```
├── Controllers/
│   └── UserController.cs           # REST API controller
├── Data/
│   └── UserDbContext.cs            # Entity Framework context
├── DTOs/
│   └── UserDto.cs                  # Data transfer objects
├── Models/
│   └── User.cs                     # User entity model
├── Services/
│   └── UserService.cs              # Business logic layer
├── UserApi.Tests/                  # Test project
│   ├── Controllers/
│   │   └── UserControllerIntegrationTests.cs
│   ├── Services/
│   │   └── UserServiceTests.cs
│   ├── TestConfiguration.cs
│   ├── TestUtilities.cs
│   └── GlobalUsings.cs
├── config/                         # Observability configurations
│   ├── alloy.alloy                # Alloy configuration
│   ├── tempo.yaml                 # Tempo configuration
│   ├── mimir.yaml                 # Mimir configuration
│   ├── loki.yaml                  # Loki configuration
│   └── grafana/                   # Grafana configurations
├── scripts/                       # Build and utility scripts
├── docker-compose.yml             # Full stack deployment
├── Dockerfile                     # Application container
├── Program.cs                     # Application entry point
├── UserApi.csproj                 # Project file
└── README.md                      # This file
```

### CI/CD Pipeline

The project includes a comprehensive GitHub Actions workflow:

1. **Build & Test**: Compile, run tests, generate coverage
2. **Code Quality**: Formatting, linting, security scanning
3. **SonarQube Analysis**: Code quality and security analysis
4. **Docker Build**: Container image creation and publishing
5. **Deployment**: Automated deployment to staging

### Dependency Management

This project uses **Dependabot** for automated dependency updates:

#### **Automated Updates**:
- 🔄 **NuGet Packages**: Weekly updates every Monday at 9 AM EST
- 🔄 **GitHub Actions**: Weekly updates every Monday at 10 AM EST
- 🔄 **Docker Images**: Weekly updates every Tuesday at 9 AM EST

#### **Intelligent Grouping**:
- **OpenTelemetry**: All OpenTelemetry packages updated together
- **Microsoft**: Microsoft packages grouped for consistency
- **Testing**: Test-related packages updated as a group

#### **Features**:
- ✅ Automatic PR creation with detailed changelogs
- ✅ Automatic reviewer assignment (@devops-thiago)
- ✅ Proper labeling and commit message formatting
- ✅ Limited concurrent PRs to avoid overwhelming
- ✅ Security and compatibility checks via CI pipeline

#### **Manual Dependency Checks**:
```bash
# Check for outdated packages
dotnet list package --outdated

# Check for vulnerable packages
dotnet list package --vulnerable --include-transitive

# Update specific package
dotnet add package PackageName --version x.x.x
```

### Adding Custom Metrics

```csharp
using System.Diagnostics.Metrics;

// In your service
private static readonly Meter Meter = new("UserApi");
private static readonly Counter<int> UserCreatedCounter = Meter.CreateCounter<int>("users_created_total");

// Increment counter
UserCreatedCounter.Add(1, new KeyValuePair<string, object?>("user.email", email));
```

### Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Run tests and quality checks (`./scripts/test.sh`)
5. Commit your changes (`git commit -m 'Add amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

### Pull Request Requirements

- ✅ All tests must pass
- ✅ Code coverage must be 80%+
- ✅ No linting errors or warnings
- ✅ Code must be properly formatted
- ✅ SonarQube quality gate must pass
- ✅ Security scan must pass
- ✅ Documentation updated if needed

## Troubleshooting

1. **Alloy not receiving data**: Check if Alloy is running and accessible on the configured ports
2. **Database issues**: The in-memory database resets on restart
3. **Port conflicts**: Ensure ports 8080, 4320, 4321, and 12345 are available

## License

This project is licensed under the MIT License.