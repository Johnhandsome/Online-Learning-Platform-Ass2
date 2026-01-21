
# Online Learning Platform

An ASP.NET Core MVC web application for online learning management.

## 🚀 Technology Stack

- **.NET 9.0** - Latest .NET framework
- **ASP.NET Core MVC** - Web application framework
- **C#** - Primary programming language
- **Bootstrap** - Frontend CSS framework
- **jQuery** - JavaScript library

## 📁 Project Structure

```

Online-Learning-Platform-Ass1/
├── Online-Learning-Platform-Ass1.Web/     # Main web application
│   ├── Controllers/                        # MVC Controllers
│   ├── Models/                             # Data models and view models
│   ├── Views/                              # Razor views
│   ├── wwwroot/                            # Static files (CSS, JS, images)
│   └── Program.cs                          # Application entry point
├── Online-Learning-Platform-Ass1.Service/  # Business logic layer
├── Online-Learning-Platform-Ass1.Data/     # Data access & EF Core
└── Online-Learning-Platform-Ass1.sln       # Solution file

````

## 🛠️ Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- A code editor (Visual Studio, VS Code, or Rider)
- SQL Server (LocalDB or full SQL Server)

## 🧭 Quick Start (Recommended Order)

Follow **this order** to avoid runtime and database errors:

| Step | Action |
|----|------|
| 1 | Clone repository |
| 2 | Configure connection string |
| 3 | Restore packages |
| 4 | Build solution |
| 5 | Run EF database update |
| 6 | Run the web application |

## 📦 Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Online-Learning-Platform-Ass1
2. **Restore dependencies**

   ```bash
   dotnet restore
   ```

3. **Build the solution**

   ```bash
   dotnet build
   ```

## 🗄️ Database & Entity Framework Core (Code First)

This project uses **Entity Framework Core (Code First)**.
The database schema is generated directly from C# entity classes.

### ⚙️ Configure Connection String

Before running EF commands, update the connection string in:

* `appsettings.json`
* or `appsettings.Development.json`

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=OnlineLearningPlatformDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### 📌 Install EF Core CLI (One-time)

```bash
dotnet tool install --global dotnet-ef
```

Verify:

```bash
dotnet ef --version
```

### 🚀 Create / Update Database

Apply existing migrations to create the database and tables:

```bash
dotnet ef database update
```

If `DbContext` is in a separate project:

```bash
dotnet ef database update \
  --project Online-Learning-Platform-Ass1.Data \
  --startup-project Online-Learning-Platform-Ass1.Web
```

This will:

* Create the database if it does not exist
* Create tables from entities
* Apply all pending migrations

### 🧱 Working with Entities & Migrations

When you **add or modify entity classes**:

1. Create a migration

   ```bash
   dotnet ef migrations add AddLessonProgress
   ```

2. Apply it to database

   ```bash
   dotnet ef database update
   ```

With separated projects:

```bash
dotnet ef migrations add AddLessonProgress \
  --project Online-Learning-Platform-Ass1.Data \
  --startup-project Online-Learning-Platform-Ass1.Web
```

### 🔄 Common EF Commands

| Purpose               | Command                           |
| --------------------- | --------------------------------- |
| Add migration         | `dotnet ef migrations add <Name>` |
| Update database       | `dotnet ef database update`       |
| Remove last migration | `dotnet ef migrations remove`     |
| List migrations       | `dotnet ef migrations list`       |

---

### ⚠️ Notes

* Do **not** manually edit migration files unless you know what you're doing
* Always commit migration files to source control
* If schema breaks during development:

  ```bash
  dotnet ef database drop
  dotnet ef database update
  ```

## ▶️ Running the Application

1. **Navigate to the web project**

   ```bash
   cd Online-Learning-Platform-Ass1.Web
   ```

2. **Run the application**

   ```bash
   dotnet run
   ```

3. **Access the application**

   * `https://localhost:5001`
   * `http://localhost:5000`
     (Ports are shown in terminal output)

## 📝 Features

* MVC architecture with clean separation of concerns
* Responsive UI using Bootstrap
* Client-side interactions with jQuery
* Entity Framework Core (Code First)
* Authentication & course enrollment flow
* AI-assisted lesson features (summary & Q&A)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License.

## 👥 Contributors

See the full list of contributors on GitHub:
https://github.com/PRN222-Group-1/Online-Learning-Platform-Ass1/graphs/contributors

## 🙏 Acknowledgments

* ASP.NET Core documentation
* Entity Framework Core
* Bootstrap team
