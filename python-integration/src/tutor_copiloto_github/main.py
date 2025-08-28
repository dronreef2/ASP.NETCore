"""
Tutor Copiloto - GitHub Chat MCP Integration
Integração do GitHub Chat MCP com o sistema Tutor Copiloto
"""

import os
import asyncio
import logging
from typing import Dict, List, Optional, Any
from contextlib import asynccontextmanager

from fastapi import FastAPI, HTTPException, BackgroundTasks
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field
import httpx
import uvicorn
from dotenv import load_dotenv

# Carregar variáveis de ambiente
load_dotenv()

# Configuração de logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Configurações
GITHUB_API_KEY = os.getenv("GITHUB_API_KEY", "")
GITHUB_CHAT_API_BASE = "https://api.github-chat.com"
MCP_SERVER_HOST = os.getenv("MCP_SERVER_HOST", "localhost")
MCP_SERVER_PORT = int(os.getenv("MCP_SERVER_PORT", "8001"))

# Modelos Pydantic
class RepositoryIndexRequest(BaseModel):
    """Request para indexar um repositório"""
    repo_url: str = Field(..., description="URL do repositório GitHub")
    branch: str = Field("main", description="Branch do repositório")

class RepositoryQueryRequest(BaseModel):
    """Request para consultar um repositório"""
    repo_url: str = Field(..., description="URL do repositório GitHub")
    question: str = Field(..., description="Pergunta sobre o repositório")
    conversation_history: Optional[List[Dict[str, str]]] = Field(None, description="Histórico da conversa")

class RepositoryAnalysisResponse(BaseModel):
    """Response da análise de repositório"""
    success: bool
    message: str
    data: Optional[Dict[str, Any]] = None
    error: Optional[str] = None

# Cliente MCP
class GitHubChatMCPClient:
    """Cliente para se comunicar com o servidor GitHub Chat MCP"""

    def __init__(self, base_url: str = f"http://{MCP_SERVER_HOST}:{MCP_SERVER_PORT}"):
        self.base_url = base_url
        self.client = httpx.AsyncClient(timeout=30.0)

    async def index_repository(self, repo_url: str) -> Dict[str, Any]:
        """Indexa um repositório para análise"""
        try:
            response = await self.client.post(
                f"{self.base_url}/tools/index_repository",
                json={"repo_url": repo_url}
            )
            response.raise_for_status()
            return response.json()
        except Exception as e:
            logger.error(f"Erro ao indexar repositório: {e}")
            raise HTTPException(status_code=500, detail=f"Erro ao indexar repositório: {str(e)}")

    async def query_repository(self, repo_url: str, question: str, conversation_history: Optional[List[Dict[str, str]]] = None) -> Dict[str, Any]:
        """Faz uma pergunta sobre um repositório indexado"""
        try:
            payload = {
                "repo_url": repo_url,
                "question": question
            }
            if conversation_history:
                payload["conversation_history"] = conversation_history

            response = await self.client.post(
                f"{self.base_url}/tools/query_repository",
                json=payload
            )
            response.raise_for_status()
            return response.json()
        except Exception as e:
            logger.error(f"Erro ao consultar repositório: {e}")
            raise HTTPException(status_code=500, detail=f"Erro ao consultar repositório: {str(e)}")

    async def close(self):
        """Fecha o cliente HTTP"""
        await self.client.aclose()

# Instância global do cliente MCP
mcp_client = GitHubChatMCPClient()

# FastAPI app
@asynccontextmanager
async def lifespan(app: FastAPI):
    """Gerenciamento do ciclo de vida da aplicação"""
    logger.info("Iniciando integração GitHub Chat MCP...")
    yield
    logger.info("Finalizando integração GitHub Chat MCP...")
    await mcp_client.close()

app = FastAPI(
    title="Tutor Copiloto - GitHub Integration API",
    description="API de integração do GitHub Chat MCP com o Tutor Copiloto",
    version="1.0.0",
    lifespan=lifespan
)

# Configuração CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Em produção, especifique os domínios permitidos
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.get("/")
async def root():
    """Endpoint raiz"""
    return {"message": "Tutor Copiloto - GitHub Chat MCP Integration", "status": "running"}

@app.get("/health")
async def health_check():
    """Verificação de saúde da API"""
    return {"status": "healthy", "service": "github-integration"}

@app.post("/api/github/index", response_model=RepositoryAnalysisResponse)
async def index_repository_endpoint(request: RepositoryIndexRequest, background_tasks: BackgroundTasks):
    """Indexa um repositório GitHub para análise"""
    try:
        logger.info(f"Indexando repositório: {request.repo_url}")

        # Executa a indexação em background para não bloquear a resposta
        background_tasks.add_task(mcp_client.index_repository, request.repo_url)

        return RepositoryAnalysisResponse(
            success=True,
            message=f"Indexação do repositório {request.repo_url} iniciada com sucesso",
            data={"repo_url": request.repo_url, "status": "indexing"}
        )
    except Exception as e:
        logger.error(f"Erro na indexação: {e}")
        return RepositoryAnalysisResponse(
            success=False,
            message="Erro ao iniciar indexação",
            error=str(e)
        )

@app.post("/api/github/query", response_model=RepositoryAnalysisResponse)
async def query_repository_endpoint(request: RepositoryQueryRequest):
    """Consulta um repositório GitHub indexado"""
    try:
        logger.info(f"Consultando repositório: {request.repo_url}")

        # Primeiro, tenta indexar o repositório se não estiver indexado
        try:
            await mcp_client.index_repository(request.repo_url)
        except Exception as e:
            logger.warning(f"Erro ao indexar repositório (pode já estar indexado): {e}")

        # Faz a consulta
        result = await mcp_client.query_repository(
            request.repo_url,
            request.question,
            request.conversation_history
        )

        return RepositoryAnalysisResponse(
            success=True,
            message="Consulta realizada com sucesso",
            data=result
        )
    except Exception as e:
        logger.error(f"Erro na consulta: {e}")
        return RepositoryAnalysisResponse(
            success=False,
            message="Erro ao consultar repositório",
            error=str(e)
        )

@app.get("/api/github/status")
async def get_status():
    """Retorna o status da integração"""
    return {
        "service": "github-chat-mcp-integration",
        "status": "active",
        "mcp_server": f"{MCP_SERVER_HOST}:{MCP_SERVER_PORT}",
        "github_api_configured": bool(GITHUB_API_KEY)
    }

if __name__ == "__main__":
    uvicorn.run(
        "main:app",
        host="0.0.0.0",
        port=8001,
        reload=True,
        log_level="info"
    )
