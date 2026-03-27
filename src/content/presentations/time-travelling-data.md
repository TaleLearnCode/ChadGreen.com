---
title: "Time Travelling Data: A Quick Overview of SQL Server Temporal Tables"
description: "A quick, practical overview of how temporal tables bring first‑class, system‑versioned history to SQL Server and simplify your data architecture."
type: "session"
level: "introductory"
durations: [45, 60, 75]
tags: ["SQL Server", "Azure SQL", "Data", "Entity Framework", ".NET", "Database", "Architecture"]
featured: false
heroImage: "/images/presentations/time-travelling-data.webp"
status: "active"

learningObjectives:
  - "Understand the key scenarios around the use of SQL Server temporal tables"
  - "Understand how to get started using temporal tables"
  - "Understand best practices and limitations of temporal tables"

resources:
  - title: "Slides"
    url: "https://github.com/TaleLearnCode/TimeTravellingData/tree/main/Presentations"
    type: "slides"
  - title: "Demos"
    url: "https://github.com/TaleLearnCode/TimeTravellingData/tree/main/Demos"
    type: "code"
---

Have you built complicated triggers and procedures to track the history of data in your databases? What if SQL Server or Azure SQL could take care of all that for you and you just had to change a couple of settings? Starting with SQL Server 2016, there is support for system-versioned temporal tables as a database feature that brings built-in support for providing information about data stored in a table at any point in time rather than only the data that is correct at the current moment in time. During this session, Chad will explain the key scenarios around the use of Temporal Tables, how system-time works, how to get started, and finish up with a demo to show you Temporal Tables in action, including the easy-to-use T-SQL syntax to implement all of the Temporal goodness. We will also look at how Entity Framework Core makes this even easier now with Temporal Table support.

### Target Audience

- .NET developers and data engineers who need to track historical data changes
- DBAs and architects evaluating built-in SQL Server auditing alternatives
- Anyone curious about system-versioned temporal tables in SQL Server and Azure SQL

### Prerequisites

- Basic familiarity with SQL Server or Azure SQL
- Some experience with T-SQL queries
