---
title: "Time Travelling Data: A Quick Overview of SQL Server Temporal Tables"
slug: "time-travelling-data-lightning"
description: "A quick, practical overview of how temporal tables bring first‑class, system‑versioned history to SQL Server and simplify your data architecture."
type: "lightning-talk"
level: "introductory"
durations: [20]
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

Often, customers will ask what the data looked like on a particular date, and you might have built complex triggers and procedures to track that history. But SQL Server and Azure SQL already have a built-in solution, and it's straightforward to use. In this quick overview, you will learn what Temporal Tables are, key scenarios for their use, and how to use built-in query syntax to retrieve the values of database records at a point in time. We will also look at how Entity Framework Core makes this even easier now with Temporal Table support.
