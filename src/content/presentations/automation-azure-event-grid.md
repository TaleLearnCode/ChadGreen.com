---
title: "Automation Using Azure Event Grid"
description: "Learn how to quickly build an automated image thumbnail generator with little code using the Azure Functions Event Grid trigger."
type: "session"
durations: [45, 60, 75]
level: "intermediate"
tags:
  - "Azure"
  - "Azure Functions"
  - "Event Grid"
  - "Automation"
  - "Serverless"
  - "C#"
  - "Microsoft"
heroImage: "/images/presentations/automation-azure-event-grid.webp"
status: "active"
featured: false

learningObjectives:
  - "Understand Azure Functions, Azure Event Grid, Azure Storage, and how they can work together"
  - "Learn from first-hand experiences of building an automated thumbnail generation utility using serverless offerings"
  - "Understand the different between the Azure Functions Blob trigger and the Azure Functions Event Grid trigger"

resources:
  - type: "github"
    title: "AutomationUsingAzureEventGrid Repository"
    url: "https://github.com/TaleLearnCode/AutomationUsingAzureEventGrid"
    description: "Slides and session materials"
---

The Azure Functions Event Grid trigger allows for the automation of tasks when certain events occur within the Azure ecosystem, such as when uploading images to a blob storage account. When a subscribed topic occurs, Event Grid detects the event and triggers the associated Azure Function. The Azure Function can then perform a specified task, such as creating a thumbnail of an uploaded image. This enables the automatic and efficient processing of pictures as soon as they are uploaded without requiring manual intervention. By using the Event Grid trigger in combination with Azure Functions, organizations can easily automate tasks related to image processing and streamline their workflows.

Come learn how to quickly build an automated image thumbnail generator with little code using the Azure Functions Event Grid trigger.

### Target Audience

- Developers and architects looking to automate workflows in Azure
- .NET and C# developers interested in serverless event-driven solutions
- Teams seeking to streamline image processing pipelines without manual intervention

### Prerequisites

- Basic familiarity with Azure and the Azure portal
- Basic understanding of C# or .NET development
- No prior Azure Functions or Event Grid experience required
