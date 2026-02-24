# FU News Management System

![Version](https://img.shields.io/badge/version-1.0.0-blue)
![License](https://img.shields.io/badge/license-MIT-green)
![Build](https://img.shields.io/badge/build-passing-success)
![Platform](https://img.shields.io/badge/platform-asp.net%20core%208.0-purple)

A scalable, microservices-based News Management Platform designed for high perfromance and enterprise-grade content delivery.

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Architecture](#-architecture)
- [Features](#-features)
- [Prerequisites](#-prerequisites)
- [Installation](#-installation)
- [Running the Application](#-running-the-application)
- [Configuration](#-configuration)
- [Testing Guide](#-testing-guide)
- [API Reference](#-api-reference)
- [Troubleshooting](#-troubleshooting)

---

## 🔭 Overview

The **FU News Management System** is a distributed application that facilitates the creation, management, and consumption of digital content. It separates concerns into distinct microservices, ensuring modularity and scalability. Key architectural decisions include **Clean Architecture**, **CQRS-lite patterns** (via OData), and **In-Memory Caching** for high-read scenarios.

Target Users:
- **Public Readers**: Access news, search via advanced filters (OData), receive real-time updates.
- **Staff (Reporters)**: Content creation, AI-assisted tagging, analytics monitoring.
- **Admin**: System configuration, user management, master data control.

---

## 🏗 Architecture

The system adopts a **Microservices-oriented Architecture** composed of 4 core services:

| Service | Port | Description | Tech Stack |
| :--- | :--- | :--- | :--- |
| **Core Backend** | `5000` | Central Business Logic, CRUD, IAM | WebAPI, EF Core |
| **Frontend** | `5001` | User Interface, Dashboards | MVC, Razor, Bootstrap 5 |
| **Analytics Service** | `5100` | Views Tracking, Trending Calculation | WebAPI, SignalR |
| **AI Tagging Service** | `5200` | Content Analysis, Auto-tagging | WebAPI, ML.NET (concept) |

### Key Technologies
- **Framework**: .NET 8.0 (C# 12)
- **Database**: SQL Server 2019+
- **ORM**: Entity Framework Core 8.0 (Code-First)
- **Caching**: LazyCache (IMemoryCache wrapper)
- **API Standard**: OData v4 (Open Data Protocol)
- **Real-time**: SignalR Core

---

## ✨ Features

### 1. Content Delivery (Public)
- **Dynamic News Feed**: Infinite scroll/pagination of latest articles.
- **Advanced Search (Explore)**: Filter by Date Range, Author, Category, Tags using OData query syntax.
- **Real-time Notifications**: New articles appear instantly without page refresh.

### 2. Content Management (Staff)
- **Rich Text Editor**: Create articles with formatted content.
- **AI Tag Suggestions**: Automatically generate relevant tags based on article body.
- **Version History**: Track changes and revisions.
- **Live Analytics**: View real-time readership statistics per article.

### 3. System Administration (Admin)
- **User Management**: Create accounts for Staff and Lecturers.
- **Category Management**: Full lifecycle management with **Instant Cache Invalidation**.
- **System Reports**: Aggregate view of system health and content metrics.

### 4. Enterprise Capabilities
- **High Performance Caching**: Categories cached for 10 mins (90% database load reduction).
- **Security & Stability**: 
  - `MaxTop=100` limit on all APIs to prevent DoS.
  - Role-Based Access Control (RBAC).

---

## � Prerequisites

Ensure the following tools are installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express or Developer edition)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (Recommended) or VS Code

---

## ⚙ Installation

### 1. Clone Repository
```bash
git clone https://github.com/your-repo/FUNewsManagement.git
cd FUNewsManagement
```

### 2. Configure Database
Update the connection string in `PHAMVIETDUNG_SE1885_A02_BE/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(local);Database=FUNewsManagement;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 3. Apply Migrations
Initialize the database schema:

```powershell
cd PHAMVIETDUNG_SE1885_A02_BE
dotnet restore
dotnet ef database update
```

---

## ▶ Running the Application

To run the full system, you must start all 4 services.

### Option A: Visual Studio (Recommended)
1. Open `PHAMVIETDUNG_SE1885_A02.sln`.
2. Right-click Solution > **Set Startup Projects**.
3. Select **Multiple startup projects**.
4. Set Action to **Start** for:
   - `PHAMVIETDUNG_SE1885_A02_BE`
   - `PHAMVIETDUNG_SE1885_A02_AnalyticsAPI`
   - `PHAMVIETDUNG_SE1885_A02_AiAPI`
   - `PHAMVIETDUNG_SE1885_A02_FE`
5. Press **F5**.

### Option B: CLI (Terminal)
Open 4 terminal tabs:

```bash
# Tab 1: Backend
dotnet run --project PHAMVIETDUNG_SE1885_A02_BE --urls="http://localhost:5000"

# Tab 2: Analytics
dotnet run --project PHAMVIETDUNG_SE1885_A02_AnalyticsAPI --urls="http://localhost:5100"

# Tab 3: AI Service
dotnet run --project PHAMVIETDUNG_SE1885_A02_AiAPI --urls="http://localhost:5200"

# Tab 4: Frontend
dotnet run --project PHAMVIETDUNG_SE1885_A02_FE --urls="http://localhost:5001"
```

Access the application at **[http://localhost:5001](http://localhost:5001)**.

---

## 🧪 Testing Guide

### Default Accounts

| Role | Email | Password |
| :--- | :--- | :--- |
| **Admin** | `admin@FUNewsManagementSystem.org` | `@@abc123@@` |
| **Staff** | *(Create via Admin Dashboard)* | `@1` |

### Manual Verification Scenarios

#### 1. Performance (Caching)
- **Step**: GET `http://localhost:5000/api/category`
- **Expectation**: 
  - 1st Request: ~100ms (Cache Miss).
  - 2nd Request: **<10ms** (Cache Hit).
  - After Create/Update: Cache invalidates, next request is Miss.

#### 2. Security (Pagination)
- **Step**: GET `http://localhost:5000/api/newsarticle?$top=500`
- **Expectation**: Response contains exactly **100 records** (MaxTop limit).

#### 3. AI Service
- **Step**: Create Article > Click "Suggest Tags".
- **Expectation**: Tags are returned from Service running on port `5200`.

---

## � API Reference

The system exposes OData v4 endpoints.

- **Base URL**: `http://localhost:5000/api`
- **Metadata**: `http://localhost:5000/api/$metadata`

### Common Queries
- **Select Fields**: `?$select=NewsTitle,CreatedDate`
- **Filter**: `?$filter=NewsStatus eq true and CategoryId eq 1`
- **Expand Relations**: `?$expand=Category,NewsTags($expand=Tag)`
- **Pagination**: `?$top=10&$skip=20&$count=true`

---

## ❓ Troubleshooting

**Issue**: "SQL Network Interfaces, error: 26 - Error Locating Server/Instance Specified"
- **Fix**: Check `appsettings.json`. Ensure Server name matches your SSMS instance (e.g., `(localdb)\MSSQLLocalDB` or `.\SQLEXPRESS`).

**Issue**: "Frontend shows 404 on API calls"
- **Fix**: Ensure Backend is running on port `5000`. If running on https, accept the SSL certificate (`dotnet dev-certs https --trust`).

---

**© 2026 FU News Management System**. Maintained by PHAMVIETDUNG_SE1885.
