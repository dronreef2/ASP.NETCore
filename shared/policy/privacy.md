# Política de Privacidade e Uso - Tutor Copiloto

## 1. Coleta e Uso de Dados

### Dados Coletados
- **Código-fonte**: Apenas trechos selecionados pelo usuário para análise
- **Mensagens de chat**: Conversas educacionais com o assistente IA
- **Metadados**: Timestamps, duração de sessões, uso de ferramentas
- **Identificadores**: IDs únicos de sessão e usuário (anonimizados)

### Dados NÃO Coletados
- **Código completo** não solicitado pelo usuário
- **Informações pessoais** além do necessário para funcionamento
- **Credenciais** ou segredos do repositório
- **Dados sensíveis** ou PII sem consentimento explícito

## 2. Armazenamento e Retenção

### Políticas de Retenção
- **Conversas**: 30 dias (configurável por organização)
- **Código analisado**: Não persistido após análise
- **Logs de sistema**: 90 dias para debugging
- **Métricas agregadas**: Indefinidamente (anonimizadas)

### Segurança
- Criptografia em trânsito (TLS 1.3)
- Criptografia em repouso (AES-256)
- Acesso baseado em roles (RBAC)
- Auditoria completa de acessos

## 3. Compartilhamento de Dados

### Proibições
- **Nunca** compartilhamos código proprietário
- **Nunca** enviamos dados para terceiros sem consentimento
- **Nunca** usamos dados para treinar modelos sem opt-in explícito

### Casos Permitidos
- Análise agregada para melhorias do produto (anonimizada)
- Debugging técnico com dados mascarados
- Compliance legal quando exigido por lei

## 4. Controles do Usuário

### Direitos do Usuário
- **Visualizar** todos os dados coletados
- **Deletar** conversas e histórico
- **Exportar** dados em formato legível
- **Opt-out** de coleta de métricas

### Configurações Disponíveis
```json
{
  "privacy": {
    "data_retention_days": 30,
    "collect_metrics": false,
    "share_anonymized_data": false,
    "export_conversations": true
  }
}
```

## 5. Uso Educacional vs Comercial

### Ambiente Educacional
- Políticas mais permissivas para aprendizado
- Maior retenção para acompanhamento pedagógico
- Análise de progresso permitida

### Ambiente Comercial
- Políticas mais restritivas
- Dados empresariais nunca deixam a organização
- Opção de deployment on-premises

## 6. Compliance

### Regulamentações Suportadas
- **GDPR** (Europa): Consentimento, direito ao esquecimento
- **LGPD** (Brasil): Transparência, finalidade específica
- **COPPA** (EUA): Proteção de menores
- **SOC 2**: Controles de segurança auditados

## 7. Contato e Violações

### Reportar Problemas
- Email: privacy@tutor-copiloto.com
- Sistema: reportar via /api/privacy/report
- Tempo de resposta: 72 horas

### Canal para DPO (Data Protection Officer)
- dpo@tutor-copiloto.com
- Telefone: +55 11 9999-9999

---

**Última atualização**: 19/08/2025  
**Versão**: 1.0  
**Revisão**: Anual ou conforme mudanças significativas
