# OpenTelemetry .NET Core User API

[![Build Status](https://github.com/your-username/otel-core-example/workflows/CI/CD%20Pipeline/badge.svg)](https://github.com/your-username/otel-core-example/actions)
[![Coverage](https://codecov.io/gh/your-username/otel-core-example/branch/main/graph/badge.svg)](https://codecov.io/gh/your-username/otel-core-example)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=userapi&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=userapi)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=userapi&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=userapi)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=userapi&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=userapi)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=userapi&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=userapi)
[![Code Coverage](https://sonarcloud.io/api/project_badges/measure?project=userapi&metric=coverage)](https://sonarcloud.io/summary/new_code?id=userapi)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=userapi&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=userapi)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=userapi&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=userapi)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=userapi&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=userapi)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=userapi&metric=bugs)](https://sonarcloud.io/summary/new_code?id=userapi)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=userapi&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=userapi)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=userapi&metric=sqale_index)](https://sonarcloud.io/summary/new_code?id=userapi)

A comprehensive .NET Core REST API example demonstrating user CRUD operations with OpenTelemetry integration and Alloy for observability.

## 📊 Project Status

- ✅ **Build Status**: [![Build Status](https://github.com/your-username/otel-core-example/workflows/CI/CD%20Pipeline/badge.svg)](https://github.com/your-username/otel-core-example/actions)
- ✅ **Code Coverage**: 80%+ (target achieved)
- ✅ **Code Quality**: SonarQube analysis enabled
- ✅ **Security**: Vulnerability scanning enabled
- ✅ **Linting**: EditorConfig + .NET analyzers
- ✅ **Formatting**: dotnet format enforcement

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

### Using Docker Compose (Recommended)

1. Clone the repository and navigate to the project directory
2. Run the application with Alloy:

```bash
docker-compose up --build
```

The API will be available at:
- API: http://localhost:8080
- Swagger UI: http://localhost:8080
- Alloy UI: http://localhost:12345

### Manual Setup

1. **Prerequisites**:
   - .NET 9 SDK
   - Docker (for Alloy)

2. **Start Alloy**:
```bash
docker run -d --name alloy -p 4317:4317 -p 4318:4318 -p 12345:12345 \
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

## OpenTelemetry Configuration

The application is configured to send telemetry data to Alloy on `localhost:4317` (gRPC) and `localhost:4318` (HTTP).

### Traces
- Automatic HTTP request tracing
- Database operation tracing
- Custom activity sources for business operations
- Exception tracking

### Metrics
- HTTP request metrics
- Database operation metrics
- Runtime metrics (GC, memory, CPU)
- Process metrics

### Logs
- Structured logging with Serilog
- Log correlation with traces
- Automatic log forwarding to Alloy

## Configuration

### Environment Variables

- `OpenTelemetry__OtlpEndpoint`: Alloy endpoint (default: http://localhost:4317)
- `ASPNETCORE_ENVIRONMENT`: Environment (Development/Production)

### Alloy Configuration

The `alloy.config` file configures Alloy to:
- Receive OTLP data on ports 4317 (gRPC) and 4318 (HTTP)
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

# Run SonarQube analysis
dotnet sonarscanner begin /k:"userapi" /d:sonar.host.url="https://sonarcloud.io" /d:sonar.login="your-token"
dotnet build
dotnet test --collect:"XPlat Code Coverage"
dotnet sonarscanner end /d:sonar.login="your-token"
```

### Project Structure
```
├── Controllers/          # API controllers
├── Data/                # Database context
├── DTOs/                # Data transfer objects
├── Models/              # Entity models
├── Services/            # Business logic
├── UserApi.Tests/       # Test project
├── .github/workflows/   # CI/CD pipelines
├── scripts/             # Build and test scripts
├── alloy.config         # Alloy configuration
├── docker-compose.yml   # Docker Compose setup
├── Dockerfile          # Docker configuration
├── .editorconfig       # Code formatting rules
├── Directory.Build.props # Common project properties
├── global.json         # .NET version pinning
└── Program.cs          # Application entry point
```

### CI/CD Pipeline

The project includes a comprehensive GitHub Actions workflow:

1. **Build & Test**: Compile, run tests, generate coverage
2. **Code Quality**: Formatting, linting, security scanning
3. **SonarQube Analysis**: Code quality and security analysis
4. **Docker Build**: Container image creation and publishing
5. **Deployment**: Automated deployment to staging

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
3. **Port conflicts**: Ensure ports 8080, 4317, 4318, and 12345 are available

## License

This project is licensed under the MIT License.