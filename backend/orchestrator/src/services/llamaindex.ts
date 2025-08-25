import axios, { AxiosInstance } from 'axios';

export interface EmbeddingResponse {
  embeddings: number[][];
  model: string;
  usage: {
    total_tokens: number;
  };
}

export interface QueryResponse {
  results: Array<{
    id: string;
    content: string;
    metadata: Record<string, any>;
    score: number;
  }>;
  usage: {
    total_tokens: number;
  };
}

export interface DocumentChunk {
  id: string;
  content: string;
  source: string;
  metadata: Record<string, any>;
  embedding?: number[];
}

/**
 * Serviço para integração com LlamaIndex Cloud
 * Responsável por embeddings, indexação e busca semântica
 */
export class LlamaIndexService {
  /**
   * Gera explicação/refatoração contextualizada usando OpenAI
   * Busca contexto relevante no índice e gera resposta com LLM
   */
  async generateExplanation(query: string, indexName: string, openAiApiKey: string, model = 'gpt-4'): Promise<string> {
    // Busca contexto relevante
    const searchResult = await this.semanticSearch(query, indexName, { topK: 3 });
    const context = searchResult.results.map(r => r.content).join('\n---\n');

    // Monta prompt para LLM
    const prompt = `Contexto extraído:\n${context}\n\nPergunta: ${query}\n\nExplique ou refatore conforme solicitado.`;

    // Chama OpenAI
    try {
      const response = await axios.post('https://api.openai.com/v1/chat/completions', {
        model,
        messages: [
          { role: 'system', content: 'Você é um especialista em código e documentação.' },
          { role: 'user', content: prompt }
        ],
        max_tokens: 512,
        temperature: 0.7
      }, {
        headers: {
          'Authorization': `Bearer ${openAiApiKey}`,
          'Content-Type': 'application/json'
        }
      });
      return response.data.choices[0].message.content;
    } catch (error) {
      console.error('Erro ao gerar explicação/refatoração:', error);
      throw new Error('Falha ao gerar explicação/refatoração');
    }
  }
  private client: AxiosInstance;
  private readonly baseUrl = 'https://api.llamaindex.ai/v1';

  constructor() {
    const apiKey = process.env.LLAMAINDEX_API_KEY;
    if (!apiKey) {
      throw new Error('LLAMAINDEX_API_KEY não configurada');
    }

    this.client = axios.create({
      baseURL: this.baseUrl,
      headers: {
        'Authorization': `Bearer ${apiKey}`,
        'Content-Type': 'application/json',
      },
      timeout: 30000,
    });
  }

  /**
   * Gera embeddings para textos
   */
  async createEmbeddings(texts: string[], model = 'text-embedding-ada-002'): Promise<EmbeddingResponse> {
    try {
      const response = await this.client.post('/embeddings', {
        input: texts,
        model,
      });

      return {
        embeddings: response.data.data.map((item: any) => item.embedding),
        model: response.data.model,
        usage: response.data.usage,
      };
    } catch (error) {
      console.error('Erro ao criar embeddings:', error);
      throw new Error(`Falha ao gerar embeddings: ${error}`);
    }
  }

  /**
   * Indexa documentos no LlamaIndex
   */
  async indexDocuments(documents: DocumentChunk[], indexName: string): Promise<void> {
    try {
      // Gera embeddings para todos os documentos
      const texts = documents.map(doc => doc.content);
      const embeddingResponse = await this.createEmbeddings(texts);

      // Prepara documentos com embeddings
      const documentsWithEmbeddings = documents.map((doc, index) => ({
        ...doc,
        embedding: embeddingResponse.embeddings[index],
      }));

      // Indexa no LlamaIndex
      await this.client.post(`/indexes/${indexName}/documents`, {
        documents: documentsWithEmbeddings,
      });

      console.log(`✅ Indexados ${documents.length} documentos no índice ${indexName}`);
    } catch (error) {
      console.error('Erro ao indexar documentos:', error);
      throw new Error(`Falha ao indexar documentos: ${error}`);
    }
  }

  /**
   * Busca semântica no índice
   */
  async semanticSearch(
    query: string, 
    indexName: string, 
    options: {
      topK?: number;
      threshold?: number;
      includeMetadata?: boolean;
    } = {}
  ): Promise<QueryResponse> {
    const { topK = 5, threshold = 0.7, includeMetadata = true } = options;

    try {
      // Gera embedding da query
      const queryEmbedding = await this.createEmbeddings([query]);

      // Busca no índice
      const response = await this.client.post(`/indexes/${indexName}/query`, {
        query_embedding: queryEmbedding.embeddings[0],
        top_k: topK,
        threshold,
        include_metadata: includeMetadata,
      });

      return {
        results: response.data.results.map((result: any) => ({
          id: result.id,
          content: result.content,
          metadata: result.metadata || {},
          score: result.score,
        })),
        usage: response.data.usage || { total_tokens: 0 },
      };
    } catch (error) {
      console.error('Erro na busca semântica:', error);
      throw new Error(`Falha na busca semântica: ${error}`);
    }
  }

  /**
   * Cria um novo índice
   */
  async createIndex(indexName: string, config: any = {}): Promise<void> {
    try {
      await this.client.post('/indexes', {
        name: indexName,
        config: {
          dimension: 1536, // Dimensão para text-embedding-ada-002
          metric: 'cosine',
          ...config,
        },
      });

      console.log(`✅ Índice ${indexName} criado com sucesso`);
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 409) {
        console.log(`ℹ️ Índice ${indexName} já existe`);
        return;
      }
      console.error('Erro ao criar índice:', error);
      throw new Error(`Falha ao criar índice: ${error}`);
    }
  }

  /**
   * Lista índices disponíveis
   */
  async listIndexes(): Promise<string[]> {
    try {
      const response = await this.client.get('/indexes');
      return response.data.indexes.map((index: any) => index.name);
    } catch (error) {
      console.error('Erro ao listar índices:', error);
      throw new Error(`Falha ao listar índices: ${error}`);
    }
  }

  /**
   * Deleta um índice
   */
  async deleteIndex(indexName: string): Promise<void> {
    try {
      await this.client.delete(`/indexes/${indexName}`);
      console.log(`✅ Índice ${indexName} deletado com sucesso`);
    } catch (error) {
      console.error('Erro ao deletar índice:', error);
      throw new Error(`Falha ao deletar índice: ${error}`);
    }
  }

  /**
   * Busca híbrida (semântica + keyword)
   */
  async hybridSearch(
    query: string,
    indexName: string,
    options: {
      topK?: number;
      alpha?: number; // 0 = apenas keyword, 1 = apenas semântica
      threshold?: number;
    } = {}
  ): Promise<QueryResponse> {
    const { topK = 5, alpha = 0.7, threshold = 0.5 } = options;

    try {
      const response = await this.client.post(`/indexes/${indexName}/hybrid-search`, {
        query,
        top_k: topK,
        alpha,
        threshold,
      });

      return {
        results: response.data.results,
        usage: response.data.usage || { total_tokens: 0 },
      };
    } catch (error) {
      console.error('Erro na busca híbrida:', error);
      // Fallback para busca semântica simples
      return this.semanticSearch(query, indexName, { topK, threshold });
    }
  }

  /**
   * Verifica saúde da conexão
   */
  async healthCheck(): Promise<boolean> {
    try {
      const response = await this.client.get('/health');
      return response.status === 200;
    } catch (error) {
      console.error('LlamaIndex health check falhou:', error);
      return false;
    }
  }
}
