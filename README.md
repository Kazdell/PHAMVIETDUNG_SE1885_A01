
# FUNews Management System - Advanced Edition (V2)

A modern, distributed news management system built with **ASP.NET Core**, featuring OData, Microservices-style architecture, JWT Authentication, and AI integrations.

## 🏗 Architecture

The solution consists of 4 distinct services:

1.  **Frontend (FE)** (`_FE`):
    *   **Port**: `http://localhost:5001`
    *   **Tech**: ASP.NET Core MVC
    *   **Features**: Public View, Staff Dashboard, Admin Panel. Uses `IHttpClientFactory` with **Polly** for resilient API calls.
2.  **Core API (BE)** (`_BE`):
    *   **Port**: `http://localhost:5000`
    *   **Tech**: ASP.NET Core WebAPI, OData, EF Core, JWT
    *   **Features**: Central CRUD, Authentication, Business Logic.
3.  **Analytics API** (`_AnalyticsAPI`):
    *   **Port**: `http://localhost:5100`
    *   **Purpose**: Provides statistical data for the Admin Dashboard (Charts).
4.  **AI API** (`_AiAPI`):
    *   **Port**: `http://localhost:5200`
    *   **Purpose**: Provides "Smart Tag Suggestions" for Staff when writing articles.

## 🚀 Prerequisites

*   **.NET 8.0 SDK** (or later)
*   **SQL Server** (LocalDB or MSSQL)

## � Installation & Setup

### 1. Database Configuration
1.  Open **SQL Server Management Studio (SSMS)**.
2.  Create a new database named `FUNewsManagement`.
3.  Run the **Full Creation Script** (provided in your assignment package) to create Tables and Data.
    *   *Note: If you already have the DB, just ensure `SystemAccount` has `RefreshToken` column and `AuditLog` table exists.*
4.  Update the connection string in `PHAMVIETDUNG_SE1885_A01_BE\appsettings.json`:
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "server=YOUR_SERVER_NAME;database=FUNewsManagement;uid=sa;pwd=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true"
    }
    ```

### 2. Running the Solution
You need to run **ALL 4 Projects** simultaneously.

**Option A: Visual Studio**
1.  Right-click the **Solution** -> **Properties**.
2.  Select **Multiple Startup Projects**.
3.  Set the following to **Start**:
    *   `PHAMVIETDUNG_SE1885_A01_BE` (Core API)
    *   `PHAMVIETDUNG_SE1885_A01_FE` (Frontend)
    *   `PHAMVIETDUNG_SE1885_A01_AnalyticsAPI` (Stats)
    *   `PHAMVIETDUNG_SE1885_A01_AiAPI` (AI)
4.  Press **F5**.

**Option B: CLI**
Open 4 different terminal windows and run:
```powershell
# Term 1: Core API
dotnet run --project PHAMVIETDUNG_SE1885_A01_BE --urls "http://localhost:5000"

# Term 2: Analytics
dotnet run --project PHAMVIETDUNG_SE1885_A01_AnalyticsAPI --urls "http://localhost:5100"

# Term 3: AI Service
dotnet run --project PHAMVIETDUNG_SE1885_A01_AiAPI --urls "http://localhost:5200"

# Term 4: Frontend
dotnet run --project PHAMVIETDUNG_SE1885_A01_FE --urls "http://localhost:5001"
```

## 🔑 Default Accounts

| Role | Email | Password |
| :--- | :--- | :--- |
| **Admin** | `admin@FUNewsManagementSystem.org` | `@@abc123@@` |
| **Staff** | *(Use admin to create one OR check database)* | *(Table: SystemAccount)* |

## ✅ Key Features to Test

1.  **Admin Login**: Access `http://localhost:5001/Account/Login`. Log in as Admin.
    *   Check **Dashboard** (Charts powered by AnalyticsAPI).
    *   **Manage Accounts** (Create/Edit Users).
    *   **Report** (View Audit Logs).
2.  **Staff Features**: Log in as a Staff account.
    *   **Manage News**: Create/Edit Articles.
    *   **Smart Tags**: When creating news, type content and click **"Suggest Tags (AI)"** to see auto-suggestions (Powered by AiAPI).
    *   **History**: View your own created articles.
3.  **Public User**: Access `http://localhost:5001`.
    *   View News.
    *   **Advanced Search**: Filter by Date, Category, etc.

## ⚠️ Troubleshooting
*   **"Connection Refused"**: Ensure all 4 projects are running. FE crashes if Core API (5000) is down.
*   **"Login Failed"**: Check `appsettings.json` connection string and ensure Database is running.

---
*Assignment Submission by PHAMVIETDUNG_SE1885*
