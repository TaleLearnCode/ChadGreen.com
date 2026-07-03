export const integritySeverities = ['error', 'warning', 'info'] as const;
export type IntegritySeverity = (typeof integritySeverities)[number];

export const integrityScopes = [
    'schema-frontmatter',
    'internal-reference',
    'external-link',
    'media-path',
] as const;
export type IntegrityScope = (typeof integrityScopes)[number];

export interface IntegrityFindingLocation {
    collection: string;
    filePath: string;
    entryId?: string;
    field?: string;
    path?: string;
    line?: number;
    column?: number;
}

export interface IntegrityRemediation {
    summary: string;
    suggestedChange?: string;
    autoFixAvailable?: boolean;
    docsPath?: string;
}

export interface IntegrityFinding {
    ruleId: string;
    code?: string;
    severity: IntegritySeverity;
    scope: IntegrityScope;
    message: string;
    location: IntegrityFindingLocation;
    file?: string;
    fieldOrPath?: string;
    remediation: IntegrityRemediation;
    blocksSave: boolean;
}

export interface IntegrityRuleDefinition {
    id: string;
    name: string;
    description: string;
    scope: IntegrityScope;
    defaultSeverity: IntegritySeverity;
    blocksSave: boolean;
    enabledByDefault: boolean;
}
