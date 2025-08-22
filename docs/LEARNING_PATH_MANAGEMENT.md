# 📚 Learning Path Management System

## Overview

The Learning Path Management System is a comprehensive educational platform feature that allows educators to create structured learning paths and track student progress through personalized learning journeys.

## Features

### 🎯 Learning Path Management
- **Create & Manage Paths**: Full CRUD operations for learning paths
- **Categorization**: Support for different categories (Frontend, Backend, FullStack, DevOps, etc.)
- **Difficulty Levels**: Beginner, Intermediate, Advanced, Expert
- **Module Organization**: Sequential modules with estimated time and rich content

### 📈 Student Progress Tracking
- **Real-time Progress**: Track completion percentage and time spent
- **Module Completions**: Record individual achievements with grades and feedback
- **Status Management**: InProgress, Completed, Paused, Abandoned states
- **Learning Analytics**: Comprehensive analytics and personalized recommendations

### 🔍 Analytics & Insights
- **Student Analytics**: Individual learning patterns and performance metrics
- **Path Statistics**: Completion rates, average times, and module performance
- **Class Performance**: Aggregate analytics for educator insights
- **Recommendations**: AI-powered learning path suggestions

## API Endpoints

### Learning Paths
```http
GET    /api/learningpath                           # List all learning paths
GET    /api/learningpath/category/{category}       # Filter by category
GET    /api/learningpath/difficulty/{difficulty}   # Filter by difficulty
GET    /api/learningpath/{id}                      # Get specific path
POST   /api/learningpath                           # Create new path
PUT    /api/learningpath/{id}                      # Update path
DELETE /api/learningpath/{id}                      # Delete path
PATCH  /api/learningpath/{id}/toggle-status        # Toggle active/inactive
```

### Modules
```http
GET    /api/learningpath/{pathId}/modules          # Get modules for a path
GET    /api/learningpath/modules/{moduleId}        # Get specific module
POST   /api/learningpath/modules                   # Create new module
PUT    /api/learningpath/modules/{moduleId}        # Update module
DELETE /api/learningpath/modules/{moduleId}        # Delete module
PUT    /api/learningpath/{pathId}/modules/reorder  # Reorder modules
```

### Student Progress
```http
GET    /api/learningpath/progress/{userId}/{pathId}                    # Get student progress
GET    /api/learningpath/progress/user/{userId}                        # Get all user progress
POST   /api/learningpath/progress/{userId}/{pathId}/start              # Start learning path
POST   /api/learningpath/progress/{userId}/{pathId}/modules/{moduleId}/complete  # Complete module
```

### Analytics
```http
GET    /api/learningpath/analytics/{userId}        # Get learning analytics
GET    /api/learningpath/recommendations/{userId}  # Get recommended paths
GET    /api/learningpath/{pathId}/statistics       # Get path statistics
```

## Data Models

### LearningPath
```csharp
public class LearningPath
{
    public string Id { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public LearningPathCategory Categoria { get; set; }
    public LearningPathDifficulty Dificuldade { get; set; }
    public int OrdemSequencia { get; set; }
    public int DuracaoEstimadaHoras { get; set; }
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
    public List<LearningModule> Modulos { get; set; }
}
```

### LearningModule
```csharp
public class LearningModule
{
    public string Id { get; set; }
    public string LearningPathId { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public string ConteudoMarkdown { get; set; }
    public int Ordem { get; set; }
    public bool ObrigatorioParaProgresso { get; set; }
    public int TempoEstimadoMinutos { get; set; }
    public string? RecursosAdicionais { get; set; }
}
```

### StudentProgress
```csharp
public class StudentProgress
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string LearningPathId { get; set; }
    public DateTime IniciadoEm { get; set; }
    public DateTime? ConcluidoEm { get; set; }
    public LearningProgressStatus Status { get; set; }
    public double ProgressoPercentual { get; set; }
    public int TempoGastoMinutos { get; set; }
    public DateTime UltimaAtividadeEm { get; set; }
}
```

## Example Usage

### Creating a Learning Path
```json
POST /api/learningpath
{
  "nome": "React Fundamentals",
  "descricao": "Learn React from basics to advanced concepts",
  "categoria": "Frontend",
  "dificuldade": "Intermediate",
  "ordemSequencia": 2,
  "duracaoEstimadaHoras": 35,
  "ativo": true
}
```

### Starting a Learning Path
```json
POST /api/learningpath/progress/user123/path456/start
```

### Completing a Module
```json
POST /api/learningpath/progress/user123/path456/modules/mod789/complete
{
  "timeSpentMinutes": 45,
  "grade": 8.5,
  "feedback": "Great content, very helpful!"
}
```

## Sample Data

The system comes with pre-configured sample learning paths:

### JavaScript Fundamentals
- **Category**: Frontend
- **Difficulty**: Beginner
- **Duration**: 20 hours
- **Modules**:
  - Variables and Data Types (45 min)
  - Functions (60 min)

### React Fundamentals
- **Category**: Frontend
- **Difficulty**: Intermediate
- **Duration**: 35 hours
- **Modules**:
  - Introduction to React (90 min)

## Integration with Existing Features

The Learning Path Management system seamlessly integrates with existing Tutor Copiloto features:

- **AI Analysis**: Learning paths can utilize the existing AI analysis for code review and suggestions
- **Chat System**: Students can ask for help during their learning journey
- **Assessment System**: Module completions are tracked alongside existing assessment features
- **User Management**: Uses the existing user authentication and authorization system

## Database Schema

The system adds four new tables to the existing database:
- `LearningPaths`: Main learning path entities
- `LearningModules`: Individual modules within paths
- `StudentProgresses`: Tracking student progress through paths
- `ModuleCompletions`: Recording individual module completions

All tables are properly indexed for optimal query performance and include appropriate foreign key relationships.

## Authorization

All endpoints require authentication via JWT Bearer token. Users can only access their own progress data, while educators can access class-wide analytics through appropriate role-based permissions.

## Error Handling

The API includes comprehensive error handling with appropriate HTTP status codes:
- `200 OK`: Successful requests
- `201 Created`: Successful creation
- `400 Bad Request`: Invalid input data
- `401 Unauthorized`: Authentication required
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server errors with logging

## Logging

All operations are logged with structured logging using Serilog, including:
- Learning path creation and updates
- Student progress milestones
- Module completions
- Error conditions

This comprehensive logging enables monitoring, debugging, and analytics.