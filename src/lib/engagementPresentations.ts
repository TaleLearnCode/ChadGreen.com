import type { CollectionEntry } from 'astro:content';

export type EngagementPresentationEntry = CollectionEntry<'engagementPresentations'>;

export function getSpeakingSessionPath(entry: EngagementPresentationEntry): string {
    return `/speaking/${entry.data.eventSlug}/presentations/${entry.data.sessionSlug}`;
}

export function getPresentationEventPath(entry: EngagementPresentationEntry): string {
    return `/presentations/${entry.data.presentationSlug}/events/${entry.data.eventSlug}`;
}

export function getCanonicalPath(entry: EngagementPresentationEntry): string {
    if (entry.data.canonicalPath === 'speaking-session') {
        return getSpeakingSessionPath(entry);
    }

    return getPresentationEventPath(entry);
}

export function getResourceIcon(type: string): string {
    const icons: Record<string, string> = {
        slides: '📊',
        video: '🎬',
        github: '💻',
        code: '⌨️',
        demo: '🚀',
        blog: '📝',
        download: '⬇️',
        documentation: '📚',
        lab: '🧪',
        other: '🔗',
    };

    return icons[type] || '🔗';
}
