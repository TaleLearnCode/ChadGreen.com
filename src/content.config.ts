import { defineCollection, reference } from 'astro:content';
import { glob, file } from 'astro/loaders';
import { z } from 'astro/zod';

// ========================
// PRESENTATIONS COLLECTION
// ========================
const presentations = defineCollection({
    loader: glob({ pattern: "**/*.md", base: "./src/content/presentations" }),
    schema: z.object({
        title: z.string(),
        slug: z.string().optional(),
        description: z.string(),
        type: z.enum([
            'session',
            'workshop',
            'lightning-talk',
            'keynote',
            'panel',
            'webinar'
        ]).default('session'),
        // Support multiple presentation lengths (e.g., 45, 60, 75 minute versions)
        durations: z.array(z.number()).optional(),
        level: z.enum(['introductory', 'intermediate', 'advanced', 'all']).default('all'),

        // Learning objectives
        learningObjectives: z.array(z.string()).optional(),

        // Categorization
        tags: z.array(z.string()).default([]),

        // Related presentations
        relatedPresentations: z.array(z.string()).optional(),

        // Resources
        resources: z.array(z.object({
            type: z.enum(['slides', 'video', 'github', 'code', 'demo', 'blog', 'download', 'documentation', 'other']),
            title: z.string(),
            url: z.string(),
            description: z.string().optional(),
        })).optional(),

        // Media
        heroImage: z.string().optional(),

        // Status
        status: z.enum(['active', 'retired', 'in-development']).default('active'),
        featured: z.boolean().default(false),
    }),
});

// ========================
// SPEAKING EVENTS COLLECTION
// ========================
const events = defineCollection({
    loader: glob({ pattern: "**/*.md", base: "./src/content/events" }),
    schema: z.object({
        title: z.string(),
        slug: z.string().optional(),
        eventType: z.enum([
            'conference',
            'meetup',
            'webinar',
            'podcast',
            'workshop',
            'user-group',
            'corporate',
            'other'
        ]).default('conference'),
        description: z.string().optional(),

        // Event details
        startDate: z.coerce.date(),
        endDate: z.coerce.date().optional(),
        location: z.object({
            venue: z.string().optional(),
            city: z.string(),
            state: z.string().optional(),
            country: z.string(),
        }),

        // Linked presentation(s)
        presentations: z.array(z.union([
            z.string(), // Simple presentation ID for backward compatibility
            z.object({
                id: z.string(),
                sessionName: z.string().optional(), // Custom session name for this event
                date: z.string().optional(), // e.g., "February 27, 2026"
                time: z.string().optional(), // e.g., "10:00 AM" or "10:00 AM - 11:00 AM"
                timeZone: z.string().optional(), // e.g., "EST", "PST", "UTC"
                room: z.string().optional(), // e.g., "ST-Laurent 6"
                sessionUrl: z.string().url().optional(), // Link to the session on event website
            })
        ])).default([]),

        // Links
        website: z.string().url().optional(),

        // Status
        featured: z.boolean().default(false),

        // Media
        heroImage: z.string().optional(),
    }),
});

// ========================
// ENGAGEMENT PRESENTATIONS COLLECTION
// ========================
const engagementPresentations = defineCollection({
    loader: glob({ pattern: "**/*.md", base: "./src/content/engagementPresentations" }),
    schema: z.object({
        title: z.string(),
        description: z.string(),
        eventSlug: z.string(),
        presentationSlug: z.string(),
        sessionSlug: z.string(),
        sessionTitle: z.string().optional(),
        date: z.coerce.date().optional(),
        time: z.string().optional(),
        timeZone: z.string().optional(),
        room: z.string().optional(),
        sessionUrl: z.string().url().optional(),
        thumbnail: z.string().optional(),
        heroImage: z.string().optional(),
        links: z.array(z.object({
            type: z.enum(['slides', 'video', 'github', 'code', 'demo', 'blog', 'download', 'documentation', 'lab', 'other']),
            title: z.string(),
            url: z.string().url(),
            description: z.string().optional(),
        })).default([]),
        canonicalPath: z.enum(['presentation-event', 'speaking-session']).default('presentation-event'),
    }),
});

// ========================
// MEETUP GROUPS COLLECTION
// ========================
const meetupGroups = defineCollection({
    loader: glob({ pattern: "**/*.md", base: "./src/content/meetupGroups" }),
    schema: z.object({
        title: z.string(),
        slug: z.string(),
        description: z.string(),

        // Location
        city: z.string(),
        state: z.string().optional(),
        country: z.string(),

        // Links
        website: z.string().url().optional(),

        // Role
        role: z.string().optional(),

        // Status
        featured: z.boolean().default(false),

        // Media
        heroImage: z.string().default('/images/meetups/default-meetup.svg'),
    }),
});

// ========================
// MEETUP EVENTS COLLECTION
// ========================
const meetupEvents = defineCollection({
    loader: glob({ pattern: "**/*.md", base: "./src/content/meetupEvents" }),
    schema: z.object({
        title: z.string(),
        slug: z.string().optional(),
        description: z.string(),
        shortDescription: z.string().optional(),
        meetupGroup: z.string(), // Reference to meetup group slug

        // Date and time
        date: z.coerce.date(),
        time: z.string().optional(),

        // Links
        eventUrl: z.string().url().optional(),

        // Speaker information
        speaker: z.object({
            name: z.string(),
            bio: z.string().optional(),
            title: z.string().optional(), // Job title/role
            company: z.string().optional(),
            photo: z.string().optional(),
            social: z.object({
                website: z.string().url().optional(),
                linkedin: z.string().url().optional(),
                twitter: z.string().optional(),
                github: z.string().url().optional(),
                youtube: z.string().url().optional(),
                sessionize: z.string().url().optional(),
                bluesky: z.string().url().optional(),
            }).optional(),
        }).optional(),

        // Resources
        resources: z.array(z.object({
            type: z.enum(['slides', 'video', 'github', 'code', 'demo', 'blog', 'download', 'documentation', 'other']),
            title: z.string(),
            url: z.string(),
            description: z.string().optional(),
        })).optional(),

        // Media
        thumbnail: z.string().optional(),
        heroImage: z.string().optional(),

        // Location
        location: z.object({
            venue: z.string().optional(),
            address: z.string().optional(),
            city: z.string().optional(),
            state: z.string().optional(),
        }).optional(),

        // Status
        status: z.enum(['upcoming', 'past']).optional(),
    }),
});

// ========================
// BLOG COLLECTION
// ========================
const blog = defineCollection({
    loader: glob({ pattern: "**/*.md", base: "./src/content/blog" }),
    schema: z.object({
        title: z.string(),
        description: z.string(),

        // Author
        author: z.string().default('Chad Green'),

        // Dates
        pubDate: z.coerce.date(),
        updatedDate: z.coerce.date().optional(),

        // Categorization
        category: z.enum([
            'Technical',
            'Speaking',
            'Community',
            'Career',
            'Tutorial',
            'Announcement',
            'Personal',
            'Other'
        ]).default('Other'),
        tags: z.array(z.string()).default([]),

        // Related content
        relatedPresentations: z.array(z.string()).optional(),

        // Media
        heroImage: z.string().default('/images/blog/default-hero.svg'),
        heroImageAlt: z.string().optional(),

        // Status
        draft: z.boolean().default(false),
        featured: z.boolean().default(false),

        // SEO
        canonicalUrl: z.string().url().optional(),
    }),
});

// ========================
// AUTHORS COLLECTION
// ========================
const authors = defineCollection({
    loader: glob({ pattern: "**/*.md", base: "./src/content/authors" }),
    schema: z.object({
        name: z.string(),
        tagline: z.string().optional(),
        bio: z.string().optional(),
        shortBio: z.string().optional(),
        avatar: z.string().default('/images/authors/default-avatar.svg'),
        email: z.string().email().optional(),
        social: z.object({
            twitter: z.string().optional(),
            linkedin: z.string().url().optional(),
            github: z.string().url().optional(),
            youtube: z.string().url().optional(),
            website: z.string().url().optional(),
            sessionize: z.string().url().optional(),
        }).optional(),
    }),
});

// ========================
// SITE DATA COLLECTION
// ========================
const siteData = defineCollection({
    loader: glob({ pattern: "**/*.md", base: "./src/content/siteData" }),
    schema: z.object({
        name: z.string(),
        tagline: z.string(),
        description: z.string(),
        email: z.string().email().optional(),
        socialLinks: z.array(z.object({
            platform: z.string(),
            url: z.string().url(),
            icon: z.string(),
        })).optional(),
    }),
});

export const collections = {
    presentations,
    events,
    engagementPresentations,
    meetupGroups,
    meetupEvents,
    blog,
    authors,
    siteData,
};
