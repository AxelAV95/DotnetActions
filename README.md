
# 🧪 TodoApi - CI con GitHub Actions y pruebas de integración

Este repositorio contiene una API en ASP.NET Core y su configuración completa para ejecutar **pruebas de integración automáticamente en GitHub Actions** con una base de datos SQL Server real.

---

## 📁 Estructura del proyecto

```
.
├── TodoApp.sln               # Solución principal
├── TodoApi/                  # Proyecto de la API
│   ├── Controllers/          # Controladores (endpoints)
│   ├── Data/                 # DbContext y configuración de EF Core
│   ├── Models/               # Entidades
│   ├── Migrations/           # Generadas por EF Core (crear con dotnet ef)
│   ├── Program.cs            # Setup de la app
│   └── appsettings.json      # Cadena de conexión
│
├── TodoApi.Tests/            # Proyecto de pruebas de integración
│   └── TodoItemsTests.cs     # Pruebas HTTP con WebApplicationFactory
│
└── .github/
    └── workflows/
        └── test-api.yml      # Configuración del workflow de GitHub Actions
```

---

## ⚙️ Configuración Inicial (Local)

### 1. Instalar la herramienta de EF Core
```bash
dotnet tool install --global dotnet-ef
```

### 2. Crear migraciones
Dentro de la carpeta `TodoApi/`:

```bash
dotnet ef migrations add InitialCreate
```

Esto crea la carpeta `Migrations/` con los archivos necesarios para crear la base de datos.

### 3. Aplicar migraciones al iniciar la app

En `Program.cs`, asegúrate de tener:
```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
```

---

## 🤖 GitHub Actions - Integración continua

### 🔹 Archivo: `.github/workflows/test-api.yml`

```yaml
name: Run Tests for TodoApi

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build-and-test:
    runs-on: ubuntu-latest

    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          SA_PASSWORD: "Your_password123"
          ACCEPT_EULA: "Y"
        ports:
          - 1433:1433
        options: >-
          --health-cmd "exit 0"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 3

    steps:
    - name: Checkout code
      uses: actions/checkout@v3

    - name: Setup .NET 9
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '9.0.x'

    - name: Restore dependencies
      run: dotnet restore

    - name: Build solution
      run: dotnet build --no-restore

    - name: Run tests
      env:
        ConnectionStrings__DefaultConnection: "Server=localhost,1433;Database=TodoDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;"
      run: dotnet test --no-build --verbosity normal
```

---

## 🧪 Pruebas de integración

Usamos `WebApplicationFactory<Program>` para levantar la API en memoria y hacer llamadas reales:

```csharp
public class TodoItemsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TodoItemsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostAndGetTodoItem_ShouldSucceed()
    {
        var item = new { Title = "Learn Testing", IsComplete = true };
        var response = await _client.PostAsJsonAsync("/api/todoitems", item);
        response.EnsureSuccessStatusCode();
    }
}
```

---

## ✅ Consideraciones importantes

- La base de datos se crea **cada vez que corre el pipeline**.
- La tabla `TodoItems` debe existir gracias a las migraciones (`InitialCreate`).
- GitHub detecta los tests automáticamente desde la solución `.sln`.
- El contenedor de SQL Server usa `SA_PASSWORD` y está disponible en `localhost:1433`.

---

## 🔐 Mejores prácticas

- Reemplazar la contraseña en texto plano con un **GitHub Secret**.
- Agregar validaciones negativas en los tests (400, 404, etc.).
- Dividir las pruebas en carpetas `IntegrationTests`, `UnitTests`.
- Considerar SQLite InMemory si quieres pruebas más rápidas en local.

---

## 🎯 Resultado final

Cada push activa:

- 🧱 Build completo de la solución
- 🧪 Pruebas reales contra la API
- 🗄 Base de datos en contenedor SQL Server
- ✅ Validación inmediata con reporte en GitHub

---

