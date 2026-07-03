# Day 1 Context: chadgreen.com

## Project Summary
Personal website for Chad Green focused on speaking portfolio, community activity, meetup involvement, blog content, and profile/contact information.

## Project Context
- **Owner:** Chad Green
- **Requester:** Chad Green
- **Stack:** Astro 5, TypeScript, Markdown content collections, Azure Static Web Apps, Azure Functions
- **Operating mode:** Content-first, schema-validated updates

## My Role (Content/Data Integrity Specialist)
- Validate cross-collection links and slug relationships.
- Ensure schema and enum compliance for markdown frontmatter.
- Identify and remediate data mismatches before merge.

## Working Notes
- Use `src/content.config.ts` as the authority for collection contracts.
- Keep fixes surgical and aligned with existing routing/ownership conventions.
- Capture recurring integrity checks and lessons learned in this file.
