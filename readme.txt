================================================================================
SMART TICKETING SYSTEM - TICKET VENDOR MACHINE (TVM)
Software Engineering Lab - Group Assignment 1 | Group 14
================================================================================

1. GitHub Repository Link:
https://github.com/seuquay56/SmartTicketingSystem_TVM.git

2. Figma UI/UX Design Link (MetroLink - Riverside Line [01]):
https://www.figma.com/design/LbKhf6mQOvEtakarW24O3d/Vending-Machine-UI--Community-?node-id=2004-20053

3. Solution Architecture & How to Run:
- Solution File: SmartTicketingSystem_TVM.sln (Visual Studio 2022)
- Framework: .NET 8.0 (Windows Desktop WinForms) & C# 12
- Projects in Solution:
  * SmartTicketing.Core: Domain models, peripheral abstractions, state controller.
  * SmartTicketing.Data: MSSQL data access (ADO.NET) & In-memory auto-sync fallback.
  * SmartTicketing.UI: High-fidelity touch kiosk UI matching Figma screens S01-S11.
- Execution:
  1. Open 'SmartTicketingSystem_TVM.sln' in Visual Studio 2022.
  2. Set 'SmartTicketing.UI' as Startup Project.
  3. Press F5 (or Ctrl+F5) to run.
  4. Alternatively, execute via terminal:
     dotnet run --project src/SmartTicketing.UI/SmartTicketing.UI.csproj

4. Database Script & Setup Instructions:
- Database Engine: Microsoft SQL Server (MSSQL 2022 / LocalDB)
- Script Path: /Database/Script.sql
- Execution Steps:
  1. Open SQL Server Management Studio (SSMS) or Azure Data Studio.
  2. Connect to your local server instance: (localdb)\MSSQLLocalDB or localhost.
  3. Open and execute 'Database/Script.sql' to generate database 'SmartTicketingDB',
     tables (Stations, Tickets, PaymentTransactions, TVMMachines, AuditLogs),
     and seed data for the 7 Riverside Line stations.
  4. (Optional) Adjust connection string in SmartTicketing.Data/DatabaseHelper.cs.
  * NOTE: The application features an automatic in-memory seed fallback. If SQL Server 
    is not running on the grading machine, the app runs smoothly with zero errors.

5. Member Contribution Breakdown:
1. 524H0057 - On Gia Khiem: 30%
2. 524H0069 - Vo Nguyen Phuc Thinh: 40%
3. 524H0052 - Nguyen Nhat Huy: 30%
Total: 100%
================================================================================
