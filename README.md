# FUNews Management System - Advanced Edition (V2)

![Dashboard Preview](docs/dashboard-preview.png)
*(Fig 1. Real-time Dashboard with Analytics and SignalR)*

## 📖 Introduction
Welcome to **FUNews Management System V2**, a modern, distributed news platform built with enterprise-grade .NET technologies.
This project demonstrates a microservices-style architecture where AI and Analytics are decoupled from the core business logic.

**Key Highlights:**
- **Smart AI Tagging:** Auto-suggests tags for articles based on content content (NLP).
- **Real-time Analytics:** Admin Dashboard updates live without refreshing (SignalR).
- **Offline Mode:** Works even when the internet disconnects or the backend server goes down.
- **High Performance:** Optimized with Caching (LazyCache) and OData querying.

---

## 🏗 System Architecture

The solution is composed of 4 key services running simultaneously:

| Service | Port | Description | Tech Stack |
| :--- | :--- | :--- | :--- |
| **Frontend (FE)** | `http://localhost:5001` | User Interface for Readers, Staff, and Admins. | ASP.NET Core MVC, Bootstrap 5, jQuery |
| **Core API (BE)** | `http://localhost:5000` | Central Logic, Database access, Auth. | WebAPI, EF Core, JWT, OData |
| **Analytics API** | `http://localhost:5100` | Processes logs & delivers chart data. | WebAPI, Background Services |
| **AI API** | `http://localhost:5200` | Intelligent Tagging & Recommendations. | ML.NET / Text Analysis |

---

## 🚀 Setup Guide (For Beginners)

### Prerequisites
- **Visual Studio 2022** (or VS Code + .NET CLI)
- **.NET 8.0 SDK**
- **SQL Server** (Any version)

### Step 1: Database Setup
1. Open **SQL Server Management Studio (SSMS)**.
2. Create a database named `FUNewsManagement`.
3. Open the file `script.sql` (found in the root folder) and run it to create Tables and Dummy Data.
4. Update the **ConnectionString** in `PHAMVIETDUNG_SE1885_A01_BE/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "server=(local);database=FUNewsManagement;uid=sa;pwd=YourPassword123;TrustServerCertificate=True"
   }
   ```
   *(Replace `YourPassword123` with your actual SQL password)*

### Step 2: Run the System
You need to start **ALL 4 Projects**.

**Using Visual Studio:**
1. Right-click **Solution** > **Properties** > **Multiple Startup Projects**.
2. Set all 4 projects (`_BE`, `_FE`, `_AnalyticsAPI`, `_AiAPI`) to **Start**.
3. Press **F5**.

**Using Terminal (CLI):**
Open 4 separate terminals and run:
```powershell
# Term 1: Core API
dotnet run --project PHAMVIETDUNG_SE1885_A01_BE --urls "http://localhost:5000"

# Term 2: Analytics
dotnet run --project PHAMVIETDUNG_SE1885_A01_AnalyticsAPI --urls "http://localhost:5100"

# Term 3: AI
dotnet run --project PHAMVIETDUNG_SE1885_A01_AiAPI --urls "http://localhost:5200"

# Term 4: Frontend
dotnet run --project PHAMVIETDUNG_SE1885_A01_FE --urls "http://localhost:5001"
```

---

## 🔑 Test Accounts

| Role | Email | Password | Access Rights |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@FUNewsManagementSystem.org` | `@@abc123@@` | Full System Access, Dashboard, Reports |
| **Staff** | `staff@news.com` | `@@abc123@@` | Create/Edit News, View History |
| **User** | *(No login required)* | - | View Public News, Search |

---

## 🧪 Features to Test

### 1. AI Tag Suggestion (Smart Tagging)
*Use this to see the AI in action.*

1. Login as **Staff**.
2. Go to **Manage News** > **Create News**.
3. Type some content (e.g., *"Bitcoin price hit a new record high today due to market surge..."*).
4. Click the **"Suggest Tags (AI)"** button.
5. **Result:** The system analyzes your text and auto-selects related tags like `#Finance`, `#Crypto`, `#Money`.

![AI Tag Suggestion](docs/ai-tagging.png)
*(Fig 2. AI automatically suggesting tags based on content)*

### 2. Real-time Dashboard
*Use this to see live analytics without refreshing.*

1. Login as **Admin**.
2. Open the **Dashboard**.
3. Open a **Private/Incognito Window** and access the homepage as a normal user.
4. Read a few articles.
5. **Result:** Switch back to the Admin Dashboard. You will see the **Total Views** and **Active Users** count jump up instantly! 

![Dashboard](docs/dashboard-charts.png)
*(Fig 3. Admin Dashboard with Live Data)*

### 3. API Fault Tolerance (Offline Mode)
*Use this to see the system's robustness.*

1. While browsing the **Staff** page, turn off the **Core API** (Close the terminal running port 5000).
2. Try to click **"Create News"**.
3. **Result:** A **Yellow Banner** appears saying "Offline Mode - Server Unreachable". The button is disabled to prevent errors.
4. Turn the API back on. The system auto-recovers!

---

## 🛠 API Endpoints (For Developers)

- **Swagger Documentation:**
  - Core API: `http://localhost:5000/swagger`
  - Analytics API: `http://localhost:5100/swagger`
  - AI API: `http://localhost:5200/swagger`

- **Health Checks:**
  - `http://localhost:5000/health`
  - `http://localhost:5100/health`

---

*Project developed by PHAMVIETDUNG_SE1885 for PRN232 Assignment.*
