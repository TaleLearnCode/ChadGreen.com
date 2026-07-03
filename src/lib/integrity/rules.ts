import type { IntegrityRuleDefinition } from './contracts';

export const schemaFrontmatterComplianceRule: IntegrityRuleDefinition = {
    id: 'schema-frontmatter-compliance',
    name: 'Schema and frontmatter compliance',
    description: 'Validates frontmatter against content schema and required field rules.',
    scope: 'schema-frontmatter',
    defaultSeverity: 'error',
    blocksSave: true,
    enabledByDefault: true,
};

export const internalReferenceRule: IntegrityRuleDefinition = {
    id: 'internal-reference-check',
    name: 'Internal reference checks',
    description: 'Validates local references and cross-collection slug/id links resolve correctly.',
    scope: 'internal-reference',
    defaultSeverity: 'error',
    blocksSave: true,
    enabledByDefault: true,
};

export const externalLinkWarningRule: IntegrityRuleDefinition = {
    id: 'external-link-warning',
    name: 'External link warning checks',
    description: 'Validates external links and emits warnings for dead or unreachable targets.',
    scope: 'external-link',
    defaultSeverity: 'warning',
    blocksSave: false,
    enabledByDefault: true,
};

export const mediaPathExistenceRule: IntegrityRuleDefinition = {
    id: 'media-path-existence',
    name: 'Media path existence checks',
    description: 'Validates local media paths exist in the repository and are resolvable at runtime.',
    scope: 'media-path',
    defaultSeverity: 'error',
    blocksSave: true,
    enabledByDefault: true,
};

export const phase0IntegrityRules: readonly IntegrityRuleDefinition[] = [
    schemaFrontmatterComplianceRule,
    internalReferenceRule,
    externalLinkWarningRule,
    mediaPathExistenceRule,
];

export const phase0IntegrityRuleMap: Readonly<Record<string, IntegrityRuleDefinition>> = Object.freeze(
    Object.fromEntries(phase0IntegrityRules.map((rule) => [rule.id, rule])),
);

export const integrityRuleDefinitions = phase0IntegrityRules;
export const integrityRuleDefinitionsById = phase0IntegrityRuleMap;
