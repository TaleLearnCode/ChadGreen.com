# Day 1 Context: chadgreen.com

## Project Summary
Personal portfolio and speaking engagement site for Chad Green. Showcases presentations, speaking events, community involvement (meetups), and blog content.

## Content Collections Overview
1. **presentations** - Reusable talks with metadata (status: active/retired/in-development, featured: bool)
2. **events** - Speaking events (conference, meetup, webinar, etc.)
3. **engagementPresentations** - Event-specific sessions linking events → presentations
4. **meetupGroups** - Meetup group info and role
5. **meetupEvents** - Individual meetup session details
6. **blog** - Blog posts (category: Technical/Speaking/Community/etc., draft: bool)
7. **authors** - Author profiles
8. **siteData** - Global site config

## QA Checklist
- Frontmatter matches schema (esp. enums: type, status, eventType, category)
- Required fields are present
- Dates are valid (use ISO format in frontmatter: 2026-03-26)
- External links are active and correct
- Internal links resolve (e.g., event → presentation references)
- No typos or inconsistencies in naming
- Featured/status fields control visibility correctly

## Testing Process
1. Run locally: `npm run build && npm run preview`
2. Browse the site; click through navigation
3. Check all new/updated content renders correctly
4. Verify links work (both internal and external)
5. Check that featured items appear in lists
6. Verify draft posts don't show up in production

## My Role (QA)
- Review content before merge
- Validate schema compliance
- Test links and rendering
- Sign off on quality
