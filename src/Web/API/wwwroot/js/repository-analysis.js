/**
 * JavaScript para a interface de análise de repositórios GitHub
 * Gerencia a interação com a API e atualização da UI
 */

class RepositoryAnalysisUI {
    constructor() {
        this.currentPage = 1;
        this.currentFilters = {};
        this.repositories = [];
        this.isLoading = false;

        this.initializeEventListeners();
        this.loadInitialData();
    }

    initializeEventListeners() {
        // Botão de busca
        $('#searchBtn').on('click', () => this.searchRepositories());

        // Botão de analisar todos
        $('#analyzeAllBtn').on('click', () => this.analyzeAllRepositories());

        // Botão de atualizar
        $('#refreshBtn').on('click', () => this.refreshData());

        // Filtros
        $('#language, #sortBy').on('change', () => this.searchRepositories());

        // Modal de detalhes
        $('#repositoryModal').on('show.bs.modal', (event) => {
            const button = $(event.relatedTarget);
            const repoId = button.data('repo-id');
            this.loadRepositoryDetails(repoId);
        });

        // Botão de analisar no modal
        $('#analyzeBtn').on('click', () => this.analyzeRepositoryFromModal());
    }

    loadInitialData() {
        this.loadPlatformStats();
        this.loadRepositories();
    }

    async loadPlatformStats() {
        try {
            const response = await $.get('/api/repositoryanalysis/stats');
            this.updatePlatformStats(response);
        } catch (error) {
            console.error('Erro ao carregar estatísticas:', error);
            this.showError('Erro ao carregar estatísticas da plataforma');
        }
    }

    updatePlatformStats(stats) {
        $('#totalRepos').text(stats.totalRepositories || 0);
        $('#totalAnalyses').text(stats.analyzedRepositories || 0);
        $('#totalIssues').text(stats.totalIssues || 0);
        $('#avgQuality').text(`${(stats.averageQualityScore || 0).toFixed(1)}%`);
    }

    async searchRepositories() {
        if (this.isLoading) return;

        this.isLoading = true;
        this.showLoading(true);

        const searchQuery = $('#searchQuery').val().trim();
        const language = $('#language').val();
        const sortBy = $('#sortBy').val();

        const params = {
            q: searchQuery,
            language: language || undefined,
            sort: sortBy,
            page: 1,
            per_page: 20
        };

        try {
            const response = await $.get('/api/repositoryanalysis/search', params);
            this.repositories = response.items || [];
            this.renderRepositoriesTable();
            this.updatePagination(response);
        } catch (error) {
            console.error('Erro na busca:', error);
            this.showError('Erro ao buscar repositórios');
        } finally {
            this.isLoading = false;
            this.showLoading(false);
        }
    }

    async loadRepositories(page = 1) {
        if (this.isLoading) return;

        this.isLoading = true;
        this.showLoading(true);

        try {
            const params = {
                page: page,
                pageSize: 20,
                ...this.currentFilters
            };

            const response = await $.get('/api/repositoryanalysis/repositories', params);
            this.repositories = response || [];
            this.renderRepositoriesTable();
            this.updatePagination({ hasNextPage: this.repositories.length === 20 });
        } catch (error) {
            console.error('Erro ao carregar repositórios:', error);
            this.showError('Erro ao carregar lista de repositórios');
        } finally {
            this.isLoading = false;
            this.showLoading(false);
        }
    }

    renderRepositoriesTable() {
        const tbody = $('#repositoriesTableBody');
        tbody.empty();

        if (this.repositories.length === 0) {
            tbody.append(`
                <tr>
                    <td colspan="6" class="text-center py-4">
                        <i class="fas fa-inbox fa-2x text-muted mb-2"></i>
                        <br>
                        Nenhum repositório encontrado
                    </td>
                </tr>
            `);
            return;
        }

        this.repositories.forEach(repo => {
            const row = this.createRepositoryRow(repo);
            tbody.append(row);
        });
    }

    createRepositoryRow(repo) {
        const statusBadge = this.getStatusBadge(repo.status);
        const qualityBadge = this.getQualityBadge(repo.lastQualityScore);
        const lastAnalyzed = repo.lastAnalyzedAt
            ? new Date(repo.lastAnalyzedAt).toLocaleDateString('pt-BR')
            : 'Nunca';

        return `
            <tr>
                <td>
                    <div class="d-flex align-items-center">
                        <img src="${repo.owner?.avatar_url || '/images/default-avatar.png'}"
                             class="rounded-circle me-2" width="32" height="32" alt="Avatar">
                        <div>
                            <strong>${repo.name}</strong>
                            <br>
                            <small class="text-muted">${repo.owner?.login || repo.owner}</small>
                        </div>
                    </div>
                </td>
                <td>
                    <span class="badge bg-light text-dark">${repo.description || 'Sem descrição'}</span>
                </td>
                <td>
                    <span class="badge bg-secondary">${repo.language || 'N/A'}</span>
                </td>
                <td>
                    <i class="fas fa-star text-warning"></i> ${repo.stars?.toLocaleString() || 0}
                </td>
                <td>${statusBadge}</td>
                <td>
                    <div class="btn-group btn-group-sm">
                        <button class="btn btn-outline-primary" data-repo-id="${repo.id}"
                                data-bs-toggle="modal" data-bs-target="#repositoryModal">
                            <i class="fas fa-eye"></i> Detalhes
                        </button>
                        <button class="btn btn-outline-success" onclick="ui.analyzeRepository('${repo.full_name}')">
                            <i class="fas fa-play"></i> Analisar
                        </button>
                    </div>
                </td>
            </tr>
        `;
    }

    getStatusBadge(status) {
        switch (status) {
            case 'Analyzed':
                return '<span class="badge bg-success">Analisado</span>';
            case 'Analyzing':
                return '<span class="badge bg-warning">Analisando</span>';
            case 'Failed':
                return '<span class="badge bg-danger">Falhou</span>';
            default:
                return '<span class="badge bg-secondary">Não Analisado</span>';
        }
    }

    getQualityBadge(score) {
        if (!score || score === 0) return '<span class="badge bg-secondary">N/A</span>';

        const classes = {
            'success': score >= 80,
            'info': score >= 60,
            'warning': score >= 40,
            'danger': score < 40
        };

        const badgeClass = Object.keys(classes).find(cls => classes[cls]) || 'secondary';
        return `<span class="badge bg-${badgeClass}">${score.toFixed(1)}%</span>`;
    }

    updatePagination(response) {
        const nav = $('#paginationNav');
        const pagination = $('#pagination');

        if (!response.hasNextPage && this.currentPage === 1) {
            nav.hide();
            return;
        }

        nav.show();
        pagination.empty();

        // Anterior
        if (this.currentPage > 1) {
            pagination.append(`
                <li class="page-item">
                    <a class="page-link" href="#" onclick="ui.loadRepositories(${this.currentPage - 1})">
                        Anterior
                    </a>
                </li>
            `);
        }

        // Página atual
        pagination.append(`
            <li class="page-item active">
                <span class="page-link">${this.currentPage}</span>
            </li>
        `);

        // Próximo
        if (response.hasNextPage) {
            pagination.append(`
                <li class="page-item">
                    <a class="page-link" href="#" onclick="ui.loadRepositories(${this.currentPage + 1})">
                        Próximo
                    </a>
                </li>
            `);
        }
    }

    async loadRepositoryDetails(repoId) {
        try {
            const response = await $.get(`/api/repositoryanalysis/repositories/${repoId}`);
            this.renderRepositoryModal(response);
        } catch (error) {
            console.error('Erro ao carregar detalhes:', error);
            this.showError('Erro ao carregar detalhes do repositório');
        }
    }

    renderRepositoryModal(repo) {
        const modalTitle = $('#repositoryModalTitle');
        const modalBody = $('#repositoryModalBody');

        modalTitle.text(`${repo.name} - Detalhes`);

        modalBody.html(`
            <div class="row">
                <div class="col-md-8">
                    <h5>${repo.full_name}</h5>
                    <p class="text-muted">${repo.description || 'Sem descrição disponível'}</p>

                    <div class="row mt-3">
                        <div class="col-6">
                            <strong>Linguagem:</strong> ${repo.language || 'N/A'}
                        </div>
                        <div class="col-6">
                            <strong>Estrelas:</strong> ${repo.stars?.toLocaleString() || 0}
                        </div>
                        <div class="col-6">
                            <strong>Forks:</strong> ${repo.forks?.toLocaleString() || 0}
                        </div>
                        <div class="col-6">
                            <strong>Issues:</strong> ${repo.open_issues?.toLocaleString() || 0}
                        </div>
                    </div>

                    <div class="mt-3">
                        <strong>URL:</strong>
                        <a href="${repo.html_url}" target="_blank" class="ms-2">
                            ${repo.html_url}
                            <i class="fas fa-external-link-alt ms-1"></i>
                        </a>
                    </div>
                </div>
                <div class="col-md-4">
                    <img src="${repo.owner?.avatar_url || '/images/default-avatar.png'}"
                         class="img-fluid rounded" alt="Avatar do proprietário">
                    <p class="mt-2 text-center">
                        <strong>${repo.owner?.login || repo.owner}</strong>
                    </p>
                </div>
            </div>

            ${repo.lastAnalysis ? `
                <div class="mt-4">
                    <h6>Última Análise</h6>
                    <div class="row">
                        <div class="col-6">
                            <strong>Score de Qualidade:</strong>
                            ${this.getQualityBadge(repo.lastAnalysis.qualityScore)}
                        </div>
                        <div class="col-6">
                            <strong>Data:</strong>
                            ${new Date(repo.lastAnalysis.analyzedAt).toLocaleString('pt-BR')}
                        </div>
                    </div>
                </div>
            ` : ''}
        `);
    }

    async analyzeRepository(repositoryName) {
        if (!confirm(`Deseja analisar o repositório ${repositoryName}?`)) return;

        try {
            const response = await $.post('/api/repositoryanalysis/analyze', {
                repositoryUrl: `https://github.com/${repositoryName}`,
                forceReanalysis: false
            });

            this.showSuccess('Análise iniciada com sucesso!');
            this.refreshData();
        } catch (error) {
            console.error('Erro na análise:', error);
            this.showError('Erro ao iniciar análise do repositório');
        }
    }

    async analyzeRepositoryFromModal() {
        const repoId = $('#analyzeBtn').data('repo-id');
        if (!repoId) return;

        await this.analyzeRepository(repoId);
        $('#repositoryModal').modal('hide');
    }

    async analyzeAllRepositories() {
        if (this.repositories.length === 0) {
            this.showWarning('Nenhum repositório para analisar');
            return;
        }

        if (!confirm(`Deseja analisar todos os ${this.repositories.length} repositórios?`)) return;

        try {
            const response = await $.post('/api/repositoryanalysis/analyze-batch', {
                repositoryUrls: this.repositories.map(r => r.html_url)
            });

            this.showSuccess('Análise em lote iniciada com sucesso!');
            this.refreshData();
        } catch (error) {
            console.error('Erro na análise em lote:', error);
            this.showError('Erro ao iniciar análise em lote');
        }
    }

    async refreshData() {
        await Promise.all([
            this.loadPlatformStats(),
            this.loadRepositories(this.currentPage)
        ]);
    }

    showLoading(show) {
        const btn = $('#searchBtn');
        const icon = btn.find('i');

        if (show) {
            btn.prop('disabled', true);
            icon.removeClass('fa-search').addClass('fa-spinner fa-spin');
        } else {
            btn.prop('disabled', false);
            icon.removeClass('fa-spinner fa-spin').addClass('fa-search');
        }
    }

    showSuccess(message) {
        this.showToast(message, 'success');
    }

    showError(message) {
        this.showToast(message, 'error');
    }

    showWarning(message) {
        this.showToast(message, 'warning');
    }

    showToast(message, type) {
        // Implementação simples de toast - pode ser substituída por uma biblioteca como Toastify
        const toastClass = {
            success: 'alert-success',
            error: 'alert-danger',
            warning: 'alert-warning'
        }[type] || 'alert-info';

        const toast = $(`
            <div class="alert ${toastClass} alert-dismissible fade show position-fixed"
                 style="top: 20px; right: 20px; z-index: 9999; min-width: 300px;">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `);

        $('body').append(toast);

        setTimeout(() => {
            toast.alert('close');
        }, 5000);
    }
}

// Inicializar quando o documento estiver pronto
$(document).ready(() => {
    window.ui = new RepositoryAnalysisUI();
});
