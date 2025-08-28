import React from 'react';
import { Upload, FileText } from 'lucide-react';

export const CodeAnalyzer: React.FC = () => {
  return (
    <div className="h-full flex flex-col">
      {/* Header */}
      <div className="bg-white dark:bg-gray-800 border-b dark:border-gray-700 p-6">
        <h2 className="text-xl font-bold text-gray-900 dark:text-white">
          Analisador de Código
        </h2>
        <p className="text-gray-600 dark:text-gray-400 mt-1">
          Faça upload do seu código para análise inteligente
        </p>
      </div>

      {/* Content */}
      <div className="flex-1 p-6">
        <div className="max-w-2xl mx-auto">
          {/* Upload Area */}
          <div className="border-2 border-dashed border-gray-300 dark:border-gray-600 rounded-lg p-12 text-center">
            <Upload className="w-16 h-16 mx-auto mb-4 text-gray-400" />
            <h3 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
              Arraste e solte seus arquivos aqui
            </h3>
            <p className="text-gray-600 dark:text-gray-400 mb-4">
              ou clique para selecionar arquivos
            </p>
            <button className="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700">
              Selecionar Arquivos
            </button>
          </div>

          {/* Recent Analyses */}
          <div className="mt-8">
            <h4 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
              Análises Recentes
            </h4>
            <div className="space-y-3">
              <div className="bg-white dark:bg-gray-800 border dark:border-gray-700 rounded-lg p-4">
                <div className="flex items-center space-x-3">
                  <FileText className="w-8 h-8 text-blue-500" />
                  <div>
                    <h5 className="font-medium text-gray-900 dark:text-white">
                      App.tsx
                    </h5>
                    <p className="text-sm text-gray-600 dark:text-gray-400">
                      Análise concluída • 2 problemas encontrados
                    </p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
