---
title: "Serverless Beyond the Happy Path: What Actually Happens in Production"
description: "Session details and resources for KCDC 2026: Serverless Beyond the Happy Path: What Actually Happens in Production."
eventSlug: "kcdc-2026"
presentationSlug: "serverless-beyond-the-happy-path"
sessionSlug: "serverless-beyond-the-happy-path"
sessionTitle: "Serverless Beyond the Happy Path: What Actually Happens in Production"
timeZone: "CDT"
canonicalPath: "speaking-session"
---

Thanks for attending **Serverless Beyond the Happy Path: What Actually Happens in Production** at **KCDC 2026**!

## Going Schema-less: How to migrate a relational database to a NoSQL database

Most serverless talks end at the demo. This one starts where production begins.

Serverless platforms promise simplicity and scale, yet many teams discover the real complexity only after launch. Retry storms, poison messages, cold‑start amplification, runaway costs, and brittle deployments often manifest weeks or months later, when traffic increases, or the original team has moved on. These failures aren’t edge cases; they’re the result of designing for the happy path.

This session focuses on what actually happens when serverless systems run in production. Rather than walking through services or frameworks, it examines execution behavior: how functions retry, overlap, re‑enter, and fail under real load. Attendees will explore why common mental models break down, how idempotency must be treated as a system‑level concern, and why naïve concurrency assumptions lead to cascading failures.

The session then connects execution semantics to operating reality, showing how observability and cost signals reveal system behavior—not just errors—and how to detect retry storms, poison messages, and cold‑start side effects before they become incidents.

The final portion is a live walkthrough that hardens a “reasonable but wrong” serverless Function app using concrete patterns for failure containment, versioning, deployment safety, and ownership.

### Learning Objectives

- "**Reason about serverless execution under real production conditions:** Understanding how retries, concurrency, and failure actually behave at runtime, and how to design idempotency and failure boundaries that hold up under load."
- "**Operate serverless workloads using signals that reveal system behavior:** Instrumenting observability and cost data to detect retry storms, poison messages, cold‑start amplification, and runaway spend before they become incidents"
- "**Evolve a working serverless application into a durable production system:** Applying concrete patterns for versioning, deployment, and ownership that survive real traffic, team turnover, and long‑term operation."
