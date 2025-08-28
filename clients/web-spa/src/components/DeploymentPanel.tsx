import React, { useState, useEffect } from 'react';
import { Play, CheckCircle, XCircle, Clock, AlertTriangle, Zap, Eye, RefreshCw } from 'lucide-react';
import axios from 'axios';

interface Deployment {
  id: string;
  repositoryUrl: string;
  branch: string;
  status: 'Pending' | 'Running' | 'Success' | 'Failed';
  createdAt: string;
  deployedAt?: string;
  duration?: string;
}

interface DeploymentAnalysis {
  deploymentId: string;
  aiAnalysis: string;
  successProbability: number;
  anomalyDetected: boolean;
  securityAssessment: string;
  optimizationSuggestions: string;
  analyzedAt: string;
}

export const DeploymentPanel: React.FC = () => {
  const [deployments, setDeployments] = useState<Deployment[]>([]);
  const [selectedDeployment, setSelectedDeployment] = useState<Deployment | null>(null);
  const [analysis, setAnalysis] = useState<DeploymentAnalysis | null>(null);
  const [loading, setLoading] = useState(false);
  const [deployLoading, setDeployLoading] = useState(false);
  const [connectionStatus, setConnectionStatus] = useState<string>('Conectando ao servidor...');
  const [repositoryUrl, setRepositoryUrl] = useState<string>('https://github.com/dronreef2/ASP.NETCore');
  const [branch, setBranch] = useState<string>('main');

  // Carregar deployments
  const loadDeployments = async () => {
    try {
      setLoading(true);
      setConnectionStatus('Conectando ao servidor...');
      console.log('Tentando conectar ao backend...');
      const response = await axios.get('http://localhost:5000/api/webhook/deployments');
      console.log('Conexão bem-sucedida:', response.data);
      setConnectionStatus('Conectado com sucesso');
      setDeployments(response.data);
    } catch (error: any) {
      console.error('Erro ao carregar deployments:', error);
      if (error.code === 'ECONNREFUSED') {
        setConnectionStatus('❌ Erro de conexão. Verifique se o servidor está rodando.');
        console.error('Servidor não está respondendo. Verifique se o backend está rodando na porta 5000.');
      } else if (error.response) {
        setConnectionStatus(`Erro do servidor: ${error.response.status}`);
        console.error('Erro do servidor:', error.response.status, error.response.data);
      } else {
        setConnectionStatus('Erro de rede. Tentando reconectar...');
        console.error('Erro de rede:', error.message);
      }
    } finally {
      setLoading(false);
    }
  };

  // Carregar análise de um deployment específico
  const loadAnalysis = async (deploymentId: string) => {
    try {
      const response = await axios.get(`http://localhost:5000/api/webhook/deployments/${deploymentId}/analysis`);
      setAnalysis(response.data);
    } catch (error) {
      console.error('Erro ao carregar análise:', error);
      setAnalysis(null);
    }
  };

  // Executar deploy manual
  const executeManualDeploy = async () => {
    try {
      setDeployLoading(true);
      const response = await axios.post('http://localhost:5000/api/webhook/manual-deploy', {
        repositoryUrl: repositoryUrl,
        branch: branch,
        author: 'Frontend User'
      });

      const newDeploymentId = response.data.deploymentId;

      // Recarregar deployments após um breve delay
      setTimeout(() => {
        loadDeployments();
        // Selecionar o novo deployment
        setTimeout(() => {
          const newDeployment = deployments.find(d => d.id === newDeploymentId);
          if (newDeployment) {
            setSelectedDeployment(newDeployment);
            loadAnalysis(newDeploymentId);
          }
        }, 1000);
      }, 2000);

    } catch (error) {
      console.error('Erro ao executar deploy:', error);
    } finally {
      setDeployLoading(false);
    }
  };

  useEffect(() => {
    // Aguardar um pouco para o backend inicializar
    const timer = setTimeout(() => {
      loadDeployments();
      // Atualizar a cada 5 segundos
      const interval = setInterval(loadDeployments, 5000);
      return () => clearInterval(interval);
    }, 1000);

    return () => clearTimeout(timer);
  }, []);

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'Success':
        return <CheckCircle className="w-5 h-5 text-green-500" />;
      case 'Failed':
        return <XCircle className="w-5 h-5 text-red-500" />;
      case 'Running':
        return <Clock className="w-5 h-5 text-blue-500 animate-spin" />;
      default:
        return <Clock className="w-5 h-5 text-gray-400" />;
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Success':
        return 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300';
      case 'Failed':
        return 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300';
      case 'Running':
        return 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300';
      default:
        return 'bg-gray-100 text-gray-800 dark:bg-gray-900 dark:text-gray-300';
    }
  };

  return (
    <div className="h-full flex">
      {/* Lista de Deployments */}
      <div className="w-1/3 border-r dark:border-gray-700 p-6">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h2 className="text-xl font-bold text-gray-900 dark:text-white">
              Deployments
            </h2>
            <p className="text-sm text-gray-600 dark:text-gray-400 mt-1">
              {connectionStatus}
            </p>
          </div>
        </div>

        {/* Formulário de Deploy Manual */}
        <div className="bg-white dark:bg-gray-800 p-6 rounded-lg border border-gray-200 dark:border-gray-700 mb-6">
          <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
            Deploy Manual
          </h3>
          <p className="text-sm text-gray-600 dark:text-gray-400 mb-4">
            Dica: Você pode usar qualquer repositório GitHub público! Cole a URL completa do repositório que deseja analisar.
          </p>
          
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                URL do Repositório GitHub * (público ou privado)
              </label>
              <input
                type="url"
                value={repositoryUrl}
                onChange={(e) => setRepositoryUrl(e.target.value)}
                placeholder="https://github.com/dronreef2/ASP.NETCore"
                className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
                required
              />
              <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                Exemplos: microsoft/vscode, facebook/react, seu-usuario/projeto
              </p>
            </div>
            
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                Branch
              </label>
              <input
                type="text"
                value={branch}
                onChange={(e) => setBranch(e.target.value)}
                placeholder="main"
                className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 dark:bg-gray-700 dark:text-white"
              />
            </div>
          </div>
        </div>

        <div className="flex space-x-2">
          <button
            onClick={loadDeployments}
            className="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700"
            disabled={loading}
          >
            <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
          </button>
          <button
            onClick={executeManualDeploy}
            disabled={deployLoading || !repositoryUrl.trim()}
            className="flex items-center px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
          >
            {deployLoading ? (
              <RefreshCw className="w-4 h-4 mr-2 animate-spin" />
            ) : (
              <Play className="w-4 h-4 mr-2" />
            )}
            Deploy Manual
          </button>
        </div>

        <div className="space-y-3">
          {deployments.map((deployment) => (
            <div
              key={deployment.id}
              onClick={() => {
                setSelectedDeployment(deployment);
                loadAnalysis(deployment.id);
              }}
              className={`
                p-4 rounded-lg border cursor-pointer transition-colors
                ${selectedDeployment?.id === deployment.id
                  ? 'border-blue-500 bg-blue-50 dark:bg-blue-900/20'
                  : 'border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-800'
                }
              `}
            >
              <div className="flex items-center justify-between mb-2">
                <div className="flex items-center space-x-2">
                  {getStatusIcon(deployment.status)}
                  <span className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(deployment.status)}`}>
                    {deployment.status}
                  </span>
                </div>
                <span className="text-xs text-gray-500">
                  {new Date(deployment.createdAt).toLocaleTimeString()}
                </span>
              </div>

              <div className="text-sm">
                <p className="font-medium text-gray-900 dark:text-white truncate">
                  {deployment.repositoryUrl.split('/').pop()}
                </p>
                <p className="text-gray-600 dark:text-gray-400">
                  Branch: {deployment.branch}
                </p>
                {deployment.duration && (
                  <p className="text-gray-500 text-xs">
                    Duração: {deployment.duration}
                  </p>
                )}
              </div>
            </div>
          ))}

          {deployments.length === 0 && !loading && (
            <div className="text-center py-8 text-gray-500">
              <Zap className="w-12 h-12 mx-auto mb-4 opacity-50" />
              <p>Nenhum deployment encontrado</p>
              <p className="text-sm">Clique em "Deploy Manual" para começar</p>
            </div>
          )}
        </div>
      </div>

      {/* Detalhes e Análise */}
      <div className="flex-1 p-6">
        {selectedDeployment ? (
          <div className="space-y-6">
            {/* Cabeçalho do Deployment */}
            <div className="bg-white dark:bg-gray-800 rounded-lg p-6 border dark:border-gray-700">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-xl font-bold text-gray-900 dark:text-white">
                  Deployment #{selectedDeployment.id.slice(-8)}
                </h3>
                <div className="flex items-center space-x-2">
                  {getStatusIcon(selectedDeployment.status)}
                  <span className={`px-3 py-1 rounded-full text-sm font-medium ${getStatusColor(selectedDeployment.status)}`}>
                    {selectedDeployment.status}
                  </span>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <span className="font-medium text-gray-700 dark:text-gray-300">Repositório:</span>
                  <p className="text-gray-900 dark:text-white">{selectedDeployment.repositoryUrl}</p>
                </div>
                <div>
                  <span className="font-medium text-gray-700 dark:text-gray-300">Branch:</span>
                  <p className="text-gray-900 dark:text-white">{selectedDeployment.branch}</p>
                </div>
                <div>
                  <span className="font-medium text-gray-700 dark:text-gray-300">Criado em:</span>
                  <p className="text-gray-900 dark:text-white">
                    {new Date(selectedDeployment.createdAt).toLocaleString()}
                  </p>
                </div>
                {selectedDeployment.deployedAt && (
                  <div>
                    <span className="font-medium text-gray-700 dark:text-gray-300">Finalizado em:</span>
                    <p className="text-gray-900 dark:text-white">
                      {new Date(selectedDeployment.deployedAt).toLocaleString()}
                    </p>
                  </div>
                )}
              </div>
            </div>

            {/* Análise Automática da IA */}
            {analysis ? (
              <div className="space-y-6">
                <h4 className="text-lg font-bold text-gray-900 dark:text-white flex items-center">
                  <Zap className="w-5 h-5 mr-2 text-blue-500" />
                  Análise Automática da IA
                </h4>

                {/* Probabilidade de Sucesso */}
                <div className="bg-white dark:bg-gray-800 rounded-lg p-6 border dark:border-gray-700">
                  <div className="flex items-center justify-between mb-4">
                    <h5 className="font-semibold text-gray-900 dark:text-white">
                      Probabilidade de Sucesso
                    </h5>
                    <span className={`px-3 py-1 rounded-full text-sm font-medium ${
                      analysis.successProbability >= 0.8 ? 'bg-green-100 text-green-800' :
                      analysis.successProbability >= 0.6 ? 'bg-yellow-100 text-yellow-800' :
                      'bg-red-100 text-red-800'
                    }`}>
                      {(analysis.successProbability * 100).toFixed(1)}%
                    </span>
                  </div>
                  <div className="w-full bg-gray-200 rounded-full h-2">
                    <div
                      className={`h-2 rounded-full ${
                        analysis.successProbability >= 0.8 ? 'bg-green-500' :
                        analysis.successProbability >= 0.6 ? 'bg-yellow-500' :
                        'bg-red-500'
                      }`}
                      style={{ width: `${analysis.successProbability * 100}%` }}
                    ></div>
                  </div>
                </div>

                {/* Análise da IA */}
                <div className="bg-white dark:bg-gray-800 rounded-lg p-6 border dark:border-gray-700">
                  <h5 className="font-semibold text-gray-900 dark:text-white mb-3">
                    📊 Análise Inteligente
                  </h5>
                  <p className="text-gray-700 dark:text-gray-300 leading-relaxed">
                    {analysis.aiAnalysis}
                  </p>
                </div>

                {/* Detecção de Anomalias */}
                <div className="bg-white dark:bg-gray-800 rounded-lg p-6 border dark:border-gray-700">
                  <div className="flex items-center justify-between mb-3">
                    <h5 className="font-semibold text-gray-900 dark:text-white">
                      🔍 Detecção de Anomalias
                    </h5>
                    {analysis.anomalyDetected ? (
                      <AlertTriangle className="w-5 h-5 text-red-500" />
                    ) : (
                      <CheckCircle className="w-5 h-5 text-green-500" />
                    )}
                  </div>
                  <p className={`text-sm ${
                    analysis.anomalyDetected
                      ? 'text-red-700 dark:text-red-300'
                      : 'text-green-700 dark:text-green-300'
                  }`}>
                    {analysis.anomalyDetected
                      ? 'Anomalias detectadas no deployment'
                      : 'Nenhuma anomalia detectada'
                    }
                  </p>
                </div>

                {/* Avaliação de Segurança */}
                <div className="bg-white dark:bg-gray-800 rounded-lg p-6 border dark:border-gray-700">
                  <h5 className="font-semibold text-gray-900 dark:text-white mb-3">
                    🔒 Avaliação de Segurança
                  </h5>
                  <p className="text-gray-700 dark:text-gray-300">
                    {analysis.securityAssessment}
                  </p>
                </div>

                {/* Sugestões de Otimização */}
                <div className="bg-white dark:bg-gray-800 rounded-lg p-6 border dark:border-gray-700">
                  <h5 className="font-semibold text-gray-900 dark:text-white mb-3">
                    💡 Sugestões de Otimização
                  </h5>
                  <div className="space-y-2">
                    {analysis.optimizationSuggestions.split(', ').map((suggestion, index) => (
                      <div key={index} className="flex items-start space-x-2">
                        <div className="w-2 h-2 bg-blue-500 rounded-full mt-2 flex-shrink-0"></div>
                        <p className="text-gray-700 dark:text-gray-300">{suggestion}</p>
                      </div>
                    ))}
                  </div>
                </div>

                {/* Timestamp da Análise */}
                <div className="text-xs text-gray-500 text-center">
                  Análise realizada em {new Date(analysis.analyzedAt).toLocaleString()}
                </div>
              </div>
            ) : (
              <div className="text-center py-12">
                <Eye className="w-16 h-16 mx-auto mb-4 text-gray-400" />
                <h4 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
                  Selecione um deployment
                </h4>
                <p className="text-gray-600 dark:text-gray-400">
                  Clique em um deployment para ver sua análise automática da IA
                </p>
              </div>
            )}
          </div>
        ) : (
          <div className="text-center py-12">
            <Zap className="w-16 h-16 mx-auto mb-4 text-gray-400" />
            <h4 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
              Bem-vindo ao painel de deployments
            </h4>
            <p className="text-gray-600 dark:text-gray-400 mb-6">
              Aqui você pode executar deployments manuais e ver as análises automáticas da IA
            </p>
            <button
              onClick={executeManualDeploy}
              disabled={deployLoading}
              className="inline-flex items-center px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
            >
              {deployLoading ? (
                <RefreshCw className="w-5 h-5 mr-2 animate-spin" />
              ) : (
                <Play className="w-5 h-5 mr-2" />
              )}
              Executar Primeiro Deploy
            </button>
          </div>
        )}
      </div>
    </div>
  );
};
