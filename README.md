# EimmyTool

EimmyTool is a modern desktop management solution designed to streamline sales, inventory tracking, and business reporting. Built with the latest .NET technologies, it provides a high-performance, native Windows experience for small to medium-sized retail operations.

<img width="466" height="318" alt="image" src="https://github.com/user-attachments/assets/b05bbea1-972d-43b8-85ce-07cb9eadeae0" />

## 🚀 Features

- **Sales Management:** Efficiently process transactions and manage customer records.
- **Inventory Control:** Real-time tracking of stock levels with automated reporting.
- **Data-Driven Insights:** Comprehensive reporting modules to analyze business performance.
- **Secure Authentication:** Integrated user management with hashed password security.
- **Local Data Storage:** Fast and reliable data persistence using SQLite.

## 🏗️ Project Structure

The project is organized to maintain a clear separation between the UI, business logic, and data access layers.

         ├── EimmyTool (Project Root)
         │   ├── Assets             # App icons and static resources
         │   ├── Common             # Shared constants, helpers, and utilities
         │   ├── Contracts          # Interfaces for services and repositories
         │   ├── Data               # SQLite database context and migrations
         │   ├── Models             # Data entities and business objects
         │   ├── Services           # Core logic (Auth, Inventory, Sales, Reporting)
         │   ├── ViewModels         # MVVM logic for UI interaction
         │   ├── Views              # XAML Windows and Pages
         │   │   ├── Controls       # Custom reusable UI components
         │   │   ├── Dialogs        # Modal windows for user input
         │   │   └── Shell          # Main window and navigation structure
         │   └── App.xaml           # Application lifecycle and global resources
         └── EimmyTool.sln          # Visual Studio Solution file

## 🧩 Core Logic & Services

The application follows a Service-Repository pattern to handle business logic and database interactions. Below are the primary classes and methods that drive the core functionality:

<img width="390" height="484" alt="image" src="https://github.com/user-attachments/assets/e746568d-e820-4eb0-a4b9-4bcaa6972ea8" />

### 🔐 Authentication & Security
* **`UserService.cs`**: Manages user profiles and access control.
    * `GetByUser(string username)`: Retrieves user data for login validation.
    * `Insert(User user)`: Handles new user registration.
* **`PasswordHasher.cs`**: A security utility that ensures passwords are never stored in plain text using industry-standard hashing.

### 📦 Inventory & Providers
* **`ProductService.cs`**: The engine for inventory management.
    * `GetAll()` / `GetByCode(string code)`: Methods to retrieve stock information.
    * `UpdateStock(int productId, int quantity)`: Adjusts inventory levels after a sale or restock.
* **`ProviderService.cs`**: Manages the relationship with supply chain partners.

### 💰 Sales & Invoicing
* **`InvoiceService.cs`**: Handles the generation and persistence of sales records.
    * `GenerateInvoice(Invoice invoice, List<InvoiceItem> items)`: A transactional method that saves the header, saves individual items, and updates product stock in a single flow.
    * `GetInvoicesByDate(DateTime start, DateTime end)`: Pulls data specifically for financial reporting.

### 👥 Client Management
* **`ClientService.cs`**: 
    * `GetByDni(string dni)`: Quickly identifies returning customers during the checkout process to apply loyalty data or specific terms.

## 🏗️ Architecture

EimmyTool follows modern desktop development patterns:

Clean UI: Utilizing WinUI 3 controls for a native Windows 11 look and feel.

Database Integration: SQLite integration for offline-first reliability.

Modular Design: Segregated logic for inventory, sales, and user management.

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




