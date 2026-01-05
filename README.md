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
└── Online-Learning-Platform-Ass1.sln       # Solution file
```

## 🛠️ Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- A code editor (Visual Studio, VS Code, or Rider)

## 📦 Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Online-Learning-Platform-Ass1
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
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
   - Open your browser and navigate to `https://localhost:5001` or `http://localhost:5000`
   - The exact ports will be displayed in the terminal output

## 🔧 Development

### Configuration

Application settings can be modified in:
- `appsettings.json` - Production settings
- `appsettings.Development.json` - Development environment settings

### Running in Development Mode

```bash
dotnet run --environment Development
```

### Building for Production

```bash
dotnet publish -c Release -o ./publish
```

## 📝 Features

- MVC architecture for clean separation of concerns
- Responsive design using Bootstrap
- Client-side validation with jQuery
- Error handling and logging

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👥 Authors

- Your Name - Initial work

## 🙏 Acknowledgments

- ASP.NET Core documentation
- Bootstrap team
- Contributors and maintainers
