import type { IntegrityFinding, IntegrityRuleDefinition } from './contracts';

export type IntegrityValidationSource = 'editor-save' | 'manual' | 'ci';

export interface SlugMutationRequestDto {
    collection: string;
    from: string;
    to: string;
    autoCascadeReferences: boolean;
}

export interface IntegrityValidationRequestDto {
    requestId?: string;
    source: IntegrityValidationSource;
    collection: string;
    filePath: string;
    entryId?: string;
    frontmatter?: Record<string, unknown>;
    body?: string;
    changedFields?: string[];
    slugMutation?: SlugMutationRequestDto;
    includeExternalLinkChecks?: boolean;
}

export interface ReferenceCascadeUpdateDto {
    collection: string;
    filePath: string;
    entryId?: string;
    fieldPath: string;
    oldValue: string;
    newValue: string;
    autoApplied: boolean;
}

export interface IntegrityValidationSummaryDto {
    errors: number;
    warnings: number;
    infos: number;
    blocksSave: boolean;
}

export interface IntegrityValidationResultDto {
    requestId: string;
    evaluatedAt: string;
    summary: IntegrityValidationSummaryDto;
    findings: IntegrityFinding[];
    rulesEvaluated: IntegrityRuleDefinition[];
    cascadeUpdates: ReferenceCascadeUpdateDto[];
}

export type SlugMutationRequest = SlugMutationRequestDto;
export type IntegrityValidationRequest = IntegrityValidationRequestDto;
export type ReferenceCascadeUpdate = ReferenceCascadeUpdateDto;
export type IntegrityValidationSummary = IntegrityValidationSummaryDto;
export type IntegrityValidationResult = IntegrityValidationResultDto;
