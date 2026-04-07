# EimmyTool

EimmyTool is a modern desktop management solution designed to streamline sales, inventory tracking, and business reporting. Built with the latest .NET technologies, it provides a high-performance, native Windows experience for small to medium-sized retail operations.

## 🚀 Features

- **Sales Management:** Efficiently process transactions and manage customer records.
- **Inventory Control:** Real-time tracking of stock levels with automated reporting.
- **Data-Driven Insights:** Comprehensive reporting modules to analyze business performance.
- **Secure Authentication:** Integrated user management with hashed password security.
- **Local Data Storage:** Fast and reliable data persistence using SQLite.

## 🛠️ Built With

- **[WinUI 3](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/):** The latest UI framework for Windows desktop apps.
- **[.NET 8+](https://dotnet.microsoft.com/):** For a fast and modern development runtime.
- **[SQLite](https://www.sqlite.org/):** Lightweight and robust local database engine.
- **XAML:** For creating a responsive and fluid user interface.

## 📋 Prerequisites

Before running the project, ensure you have the following installed:

- [Visual Studio 2022](https://visualstudio.microsoft.com/) (version 17.x or later)
- **Windows App SDK** workload
- **.NET Desktop Development** workload

## ⚙️ Getting Started

1. **Clone the repository:**
   ```bash
   git clone [https://github.com/lpenapin/EimmyTool.git](https://github.com/lpenapin/EimmyTool.git)
2. **Open the solution:**
Open EimmyTool.sln in Visual Studio.

3. **Restore Packages:**
The NuGet packages should restore automatically on build.

4. **Run the Application:**
Set the startup project to EimmyTool (Package) and press F5.

## 🔒 Security

This application implements secure authentication practices, including:

Hashed password storage for local user accounts.

Role-based access control for sensitive inventory and reporting modules.

## 🏗️ Architecture

EimmyTool follows modern desktop development patterns:

Clean UI: Utilizing WinUI 3 controls for a native Windows 11 look and feel.

Database Integration: SQLite integration for offline-first reliability.

Modular Design: Segregated logic for inventory, sales, and user management.
