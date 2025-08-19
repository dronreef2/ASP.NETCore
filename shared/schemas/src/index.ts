import { z } from 'zod';

// Tool Schemas
export const RunTestsInputSchema = z.object({
  repoPath: z.string(),
  testCmd: z.string(),
  timeoutSec: z.number().optional().default(120),
});

export const ReadRepoInputSchema = z.object({
  repoPath: z.string(),
  paths: z.array(z.string()).optional(),
  lines: z.object({
    start: z.number(),
    end: z.number(),
  }).optional(),
  commitHash: z.string().optional(),
});

export const StaticCheckInputSchema = z.object({
  repoPath: z.string(),
  linter: z.enum(['eslint', 'semgrep', 'sonarjs']).default('eslint'),
  paths: z.array(z.string()).optional(),
});

export const SearchDocsInputSchema = z.object({
  query: z.string(),
  maxResults: z.number().optional().default(5),
  threshold: z.number().optional().default(0.75),
});

export const ExplainDiffInputSchema = z.object({
  oldContent: z.string(),
  newContent: z.string(),
  filePath: z.string().optional(),
});

// Tool Result Schemas
export const ToolResultSchema = z.object({
  success: z.boolean(),
  data: z.any(),
  error: z.string().optional(),
  duration: z.number(),
  metadata: z.record(z.any()).optional(),
});

// Claude Tool Use Schemas
export const ClaudeToolSchema = z.object({
  name: z.string(),
  description: z.string(),
  input_schema: z.object({
    type: z.literal('object'),
    properties: z.record(z.any()),
    required: z.array(z.string()).optional(),
  }),
});

export const ClaudeToolUseSchema = z.object({
  type: z.literal('tool_use'),
  id: z.string(),
  name: z.string(),
  input: z.record(z.any()),
});

export const ClaudeToolResultSchema = z.object({
  type: z.literal('tool_result'),
  tool_use_id: z.string(),
  content: z.string(),
  is_error: z.boolean().optional(),
});

// Message Schemas
export const ChatMessageSchema = z.object({
  id: z.string(),
  role: z.enum(['user', 'assistant', 'system']),
  content: z.string(),
  timestamp: z.date(),
  metadata: z.record(z.any()).optional(),
});

export const ConversationSchema = z.object({
  id: z.string(),
  userId: z.string(),
  messages: z.array(ChatMessageSchema),
  context: z.record(z.any()).optional(),
  createdAt: z.date(),
  updatedAt: z.date(),
});

// RAG Schemas
export const DocumentChunkSchema = z.object({
  id: z.string(),
  content: z.string(),
  source: z.string(),
  sourcePath: z.string(),
  sourceType: z.enum(['markdown', 'code', 'pdf', 'html']),
  embedding: z.array(z.number()).optional(),
  metadata: z.record(z.any()),
  score: z.number().optional(),
});

export const RAGQuerySchema = z.object({
  query: z.string(),
  maxResults: z.number().optional().default(5),
  threshold: z.number().optional().default(0.75),
  filters: z.record(z.any()).optional(),
});

// Telemetry Schemas
export const TelemetryEventSchema = z.object({
  eventType: z.enum(['request', 'tool_use', 'error', 'cost']),
  conversationId: z.string(),
  userId: z.string(),
  timestamp: z.date(),
  data: z.record(z.any()),
  cost: z.number().optional(),
  tokens: z.object({
    input: z.number(),
    output: z.number(),
  }).optional(),
});

// Assessment Schemas
export const RubricItemSchema = z.object({
  criterion: z.string(),
  description: z.string(),
  maxPoints: z.number(),
  weight: z.number().optional().default(1),
});

export const AssessmentResultSchema = z.object({
  userId: z.string(),
  taskId: z.string(),
  score: z.number(),
  maxScore: z.number(),
  feedback: z.string(),
  rubricResults: z.array(z.object({
    criterion: z.string(),
    points: z.number(),
    feedback: z.string(),
  })),
  timestamp: z.date(),
});

// Type exports
export type RunTestsInput = z.infer<typeof RunTestsInputSchema>;
export type ReadRepoInput = z.infer<typeof ReadRepoInputSchema>;
export type StaticCheckInput = z.infer<typeof StaticCheckInputSchema>;
export type SearchDocsInput = z.infer<typeof SearchDocsInputSchema>;
export type ExplainDiffInput = z.infer<typeof ExplainDiffInputSchema>;
export type ToolResult = z.infer<typeof ToolResultSchema>;
export type ClaudeTool = z.infer<typeof ClaudeToolSchema>;
export type ClaudeToolUse = z.infer<typeof ClaudeToolUseSchema>;
export type ClaudeToolResult = z.infer<typeof ClaudeToolResultSchema>;
export type ChatMessage = z.infer<typeof ChatMessageSchema>;
export type Conversation = z.infer<typeof ConversationSchema>;
export type DocumentChunk = z.infer<typeof DocumentChunkSchema>;
export type RAGQuery = z.infer<typeof RAGQuerySchema>;
export type TelemetryEvent = z.infer<typeof TelemetryEventSchema>;
export type RubricItem = z.infer<typeof RubricItemSchema>;
export type AssessmentResult = z.infer<typeof AssessmentResultSchema>;
