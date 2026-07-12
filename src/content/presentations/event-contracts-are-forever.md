---
title: 'Event Contracts Are Forever: Designing Schemas That Survive Change'
slug: event-contracts-are-forever
description: Event‑driven systems promise loose coupling, but in practice, teams freeze event schemas out of fear. Once an event is published, it becomes a long‑lived public contract with unknown consumers, delayed failure modes, and no safe rollback. Versioning that works for REST APIs often collapses under fan‑out, shared streams, and organizational reality.
type: session
durations:
- 45
- 60
- 75
level: all
learningObjectives:
- Explain why familiar “non‑breaking” change strategies fail in event‑driven systems, including how fan‑out, delayed consumers, and shared streams turn small schema changes into long‑term production risks.
- Design event contracts that survive change over time, applying practical evolution strategies such as additive change, envelopes, metadata, and compatibility boundaries with a clear understanding of their tradeoffs.
- Apply governance and ownership patterns that enable safe evolution without gridlock, balancing consumer protection, versioning discipline, and organizational reality to confidently say “yes” to change.
tags: []
relatedPresentations: []
resources: []
status: in-development
featured: false
validated: false
---
Event‑driven systems promise loose coupling, but in practice, teams freeze event schemas out of fear. Once an event is published, it becomes a long‑lived public contract with unknown consumers, delayed failure modes, and no safe rollback. Versioning that works for REST APIs often collapses under fan‑out, shared streams, and organizational reality.

This session reframes events as durable public APIs and shows how to design contracts that survive change. We’ll start by examining why “non‑breaking” changes break production months later, and why familiar instincts fail in event‑driven systems. From there, we’ll explore schema evolution strategies that actually work in practice, additive change, envelopes, and metadata, along with the tradeoffs teams rarely discuss.

We’ll then look beyond schemas to the social side of contracts: ownership, review, and governance patterns that protect shared event streams without creating central bottlenecks or version explosions. Finally, we’ll walk through a live refactor of a real event, comparing a seemingly reasonable change that breaks consumers with an alternative that evolves safely.

Attendees will leave with concrete patterns, decision frameworks, and the confidence to deliberately evolve event contracts, saying “yes” to change without fear, rewrites, or governance gridlock.