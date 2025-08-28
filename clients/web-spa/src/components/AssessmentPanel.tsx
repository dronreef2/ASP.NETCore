import React from 'react';
import { BarChart3, TrendingUp, Users, Activity } from 'lucide-react';

export const AssessmentPanel: React.FC = () => {
  return (
    <div className="h-full p-6">
      <div className="max-w-6xl mx-auto">
        <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-6">
          Painel de Avaliação
        </h2>

        {/* Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 border dark:border-gray-700">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-600 dark:text-gray-400">Usuários Ativos</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white">1,234</p>
              </div>
              <Users className="w-8 h-8 text-blue-500" />
            </div>
          </div>

          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 border dark:border-gray-700">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-600 dark:text-gray-400">Deployments</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white">89</p>
              </div>
              <Activity className="w-8 h-8 text-green-500" />
            </div>
          </div>

          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 border dark:border-gray-700">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-600 dark:text-gray-400">Taxa de Sucesso</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white">94.2%</p>
              </div>
              <TrendingUp className="w-8 h-8 text-purple-500" />
            </div>
          </div>

          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 border dark:border-gray-700">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-gray-600 dark:text-gray-400">Análises IA</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white">156</p>
              </div>
              <BarChart3 className="w-8 h-8 text-orange-500" />
            </div>
          </div>
        </div>

        {/* Charts Placeholder */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 border dark:border-gray-700">
            <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
              Performance de Deployments
            </h3>
            <div className="h-64 flex items-center justify-center text-gray-500">
              <BarChart3 className="w-16 h-16" />
              <span className="ml-4">Gráfico será implementado</span>
            </div>
          </div>

          <div className="bg-white dark:bg-gray-800 rounded-lg p-6 border dark:border-gray-700">
            <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
              Análises por Tipo
            </h3>
            <div className="h-64 flex items-center justify-center text-gray-500">
              <TrendingUp className="w-16 h-16" />
              <span className="ml-4">Gráfico será implementado</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
