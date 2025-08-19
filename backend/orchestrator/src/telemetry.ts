import type { TelemetryEvent } from '@tutor-copiloto/schemas';

export class TelemetryService {
  private events: TelemetryEvent[] = [];

  async logEvent(event: TelemetryEvent): Promise<void> {
    // Em produção, enviaria para OpenTelemetry/Prometheus
    this.events.push(event);
    
    // Log estruturado para stdout (pode ser coletado por logging driver)
    console.log(JSON.stringify({
      timestamp: event.timestamp.toISOString(),
      level: 'info',
      type: 'telemetry',
      event_type: event.eventType,
      conversation_id: event.conversationId,
      user_id: event.userId,
      data: event.data,
      cost: event.cost,
      tokens: event.tokens,
    }));

    // Simula envio para collector
    if (process.env.OTEL_EXPORTER_OTLP_ENDPOINT) {
      // Aqui enviaria para OpenTelemetry Collector
      await this.sendToOtelCollector(event);
    }
  }

  private async sendToOtelCollector(event: TelemetryEvent): Promise<void> {
    // Implementação real enviaria spans/metrics para OTEL
    // Por enquanto só logga
    console.log('📊 Enviando telemetria para OpenTelemetry:', event.eventType);
  }

  // Métricas agregadas para dashboards
  getMetrics(timeRange?: { start: Date; end: Date }): any {
    let filteredEvents = this.events;
    
    if (timeRange) {
      filteredEvents = this.events.filter(e => 
        e.timestamp >= timeRange.start && e.timestamp <= timeRange.end
      );
    }

    const totalRequests = filteredEvents.filter(e => e.eventType === 'request').length;
    const totalCost = filteredEvents.reduce((sum, e) => sum + (e.cost || 0), 0);
    const totalTokens = filteredEvents.reduce((sum, e) => {
      if (e.tokens) {
        return sum + e.tokens.input + e.tokens.output;
      }
      return sum;
    }, 0);

    const errors = filteredEvents.filter(e => e.eventType === 'error').length;
    const errorRate = totalRequests > 0 ? (errors / totalRequests) * 100 : 0;

    return {
      total_requests: totalRequests,
      total_cost: totalCost,
      total_tokens: totalTokens,
      error_count: errors,
      error_rate: errorRate,
      avg_cost_per_request: totalRequests > 0 ? totalCost / totalRequests : 0,
    };
  }
}
