# Cash Register System

A secure, role-based cash management system for banking operations with maker-checker workflow and comprehensive audit trails.

## Overview

The Cash Register System provides daily cash tracking for bank branches with a secure maker-checker approval workflow. The system enforces data isolation by branch, maintains denomination-level tracking, and provides both web and API interfaces for flexible integration.

## Key Features

- **Maker-Checker Workflow**: Draft, Submit, Approve/Reject workflow ensures all entries are reviewed before finalization
- **Role-Based Access Control**: Four distinct roles (Inputer, Authorizer, Viewer, Admin) with granular permissions
- **Branch Data Isolation**: Users only access data for their assigned branch, with system-wide access for Admins and Viewers
- **Denomination Tracking**: Track 11 currency denominations (1000, 500, 200, 100, 50, 20, 10, 5, 2, 1, coins) for complete cash accountability
- **Daily Balance Carryforward**: Opening balances automatically populate from previous day's closing vault balance
- **Comprehensive Admin Panel**: Full CRUD operations for users, branches, and entries with status override capabilities
- **Dual Interface**: Web application for daily operations and REST API for system integrations
- **Production-Ready**: Fully containerized with Docker for seamless deployment

## Architecture

Built with Clean Architecture principles using ASP.NET Core 9.0:

- **Web Application**: Server-side Razor Pages with cookie-based authentication
- **REST API**: JWT-authenticated endpoints with Swagger/OpenAPI documentation
- **Database**: Microsoft SQL Server with Entity Framework Core
- **Security**: PBKDF2 password hashing, role-based authorization, branch-level data isolation

## Quick Start

### Prerequisites

- Docker and Docker Compose
- .NET 9.0 SDK (for local development)

### Running with Docker

1. Configure environment variables in `.env` file
2. Start all services:
   ```bash
   docker-compose up -d
   ```
3. Access the applications:
   - Web App: http://localhost:5200
   - REST API: http://localhost:5100 (Swagger UI)

### Default Credentials

**System seeded with sample users:**
- Admin: `Admin` / `Admin123!`
- Branch Inputter: `BRN001Inputter` / `Password123!`
- Branch Authorizer: `BRN001Authorizer` / `Password123!`
- System Viewer: `Management` / `Password123!`

## User Roles

**Inputer**: Creates and edits cash entries for their branch. Submits entries for approval.

**Authorizer**: Reviews and approves/rejects submitted entries for their branch.

**Viewer**: Read-only access to approved entries across all branches for reporting and auditing.

**Admin**: Full system access including user management, branch management, and entry oversight with status override capabilities.

## Technology Stack

- ASP.NET Core 9.0 (Razor Pages & Web API)
- Entity Framework Core 9.0
- Microsoft SQL Server 2022
- Docker & Docker Compose
- Bootstrap 5 (UI)
- JWT Authentication (API)
- Swagger/OpenAPI (API Documentation)

## License

Proprietary. All rights reserved.
# CashRegister
