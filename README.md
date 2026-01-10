# Core Ledger API

Uma API REST .NET 10 pronta para produção para **contabilidade de fundos ABOR (Accounting Book of Records)** projetada para clientes financeiros institucionais, implementando princípios de Clean Architecture com foco em segurança, auditabilidade e manutenibilidade. Este sistema serve como a fonte autoritativa de dados contábeis para fundos de investimento.

## Arquitetura

Este projeto segue **Clean Architecture** (Arquitetura Hexagonal) com clara separação de responsabilidades:

```
CoreLedger.API/              # Camada de Apresentação (Controllers, Middleware, Extensions)
CoreLedger.Application/      # Camada de Aplicação (Use Cases, DTOs, Validators)
CoreLedger.Domain/           # Camada de Domínio (Entities, Value Objects, Interfaces)
CoreLedger.Infrastructure/   # Camada de Infraestrutura (EF Core, Repositories, Persistence)
CoreLedger.UnitTests/        # Testes Unitários (xUnit + NSubstitute)
CoreLedger.IntegrationTests/ # Testes de Integração (Testcontainers + xUnit)
```

### Padrões Arquiteturais Principais

- **CQRS**: Command Query Responsibility Segregation usando MediatR
- **Uso Direto do DbContext**: Entity Framework Core DbSet<T> como padrão de acesso a dados
- **Query Services**: Serviços da camada de infraestrutura para operações complexas de filtragem RFC-8040
- **Injeção de Dependência**: Injeção por construtor em toda a aplicação
- **Domain-Driven Design**: Modelos de domínio ricos com lógica de negócio
- **Pipeline de Middleware**: Responsabilidades transversais (tratamento de exceções, logging, IDs de correlação)

## Stack Tecnológica

### Framework Principal
- **.NET 10.0** - Framework .NET mais recente
- **ASP.NET Core** - Framework para Web API
- **C# 13** - Recursos mais recentes da linguagem com tipos de referência anuláveis

### Banco de Dados & ORM
- **PostgreSQL 18** - Banco de dados principal
- **Entity Framework Core 10** - ORM
- **Npgsql** - Provider PostgreSQL

### Padrões de Aplicação
- **MediatR 14** - Padrão CQRS e mediator
- **AutoMapper 16** - Mapeamento objeto-para-objeto
- **FluentValidation 12** - Validação de entrada

### Mensageria & Autenticação
- **RabbitMQ** - Message broker para processamento assíncrono e comunicação com workers
- **Auth0** - Autenticação JWT Bearer e gerenciamento de usuários

### Logging & Monitoramento
- **Serilog** - Logging estruturado
- **Health Checks** - Monitoramento de saúde da aplicação
- **IDs de Correlação** - Rastreamento de requisições

### Documentação da API
- **Swagger/OpenAPI** - Documentação interativa da API
- **Documentação XML** - Documentação em nível de código

### Testes
- **xUnit** - Framework de testes
- **NSubstitute** - Biblioteca de mocking
- **Testcontainers** - Testes de integração com containers Docker descartáveis

### DevOps
- **Docker** - Containerização
- **Docker Compose** - Ambiente de desenvolvimento local

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (para PostgreSQL e RabbitMQ local)
- [PostgreSQL 18](https://www.postgresql.org/download/) (ou use Docker)
- [RabbitMQ](https://www.rabbitmq.com/download.html) (ou use Docker)
- IDE: [Visual Studio 2025](https://visualstudio.microsoft.com/), [Rider](https://www.jetbrains.com/rider/), ou [VS Code](https://code.visualstudio.com/)

## Começando

### 1. Clone o Repositório

```bash
git clone https://github.com/your-org/core-ledger-api.git
cd core-ledger-api
```

### 2. Configure as Variáveis de Ambiente

```bash
# Copie o template de ambiente
cp .env.template .env

# Edite .env com sua configuração
# Para desenvolvimento local, os padrões devem funcionar com Docker Compose
```

### 3. Inicie o PostgreSQL e RabbitMQ com Docker

```bash
# Inicie os containers PostgreSQL e RabbitMQ
docker-compose up -d

# Verifique se os containers estão rodando
docker ps
```

Os seguintes serviços estarão disponíveis:
- **PostgreSQL**: localhost:5432
- **RabbitMQ AMQP**: localhost:5672
- **RabbitMQ Management UI**: http://localhost:15672 (guest/guest)

### 4. Aplique as Migrations do Banco de Dados

```bash
# A partir da raiz da solução
dotnet ef database update --project CoreLedger.Infrastructure --startup-project CoreLedger.API
```

### 5. Execute a Aplicação

```bash
# Modo desenvolvimento com hot reload
dotnet watch run --project CoreLedger.API

# Ou execução padrão
dotnet run --project CoreLedger.API
```

A API estará disponível em:
- **HTTPS**: https://localhost:7001
- **HTTP**: http://localhost:5001
- **Swagger UI**: https://localhost:7001/swagger

## Testes

### Executar Todos os Testes

```bash
dotnet test
```

### Executar Apenas Testes Unitários

```bash
dotnet test CoreLedger.UnitTests/CoreLedger.UnitTests.csproj
```

### Executar Apenas Testes de Integração

```bash
dotnet test CoreLedger.IntegrationTests/CoreLedger.IntegrationTests.csproj
```

### Cobertura de Código

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Documentação da API

### Swagger UI

Ao executar em modo Development, acesse a documentação interativa da API em:

**https://localhost:7001/swagger**

### Health Checks

- **Saúde Geral**: `GET /health`
- **Prontidão**: `GET /health/ready`
- **Vivacidade**: `GET /health/live`

### Exemplos de Endpoints

#### API de ToDos

```bash
# Obter todos os ToDos
GET /api/todos

# Obter ToDo por ID
GET /api/todos/{id}

# Criar novo ToDo
POST /api/todos
Content-Type: application/json
{
  "description": "Completar a documentação do projeto"
}

# Atualizar ToDo
PUT /api/todos/{id}
Content-Type: application/json
{
  "description": "Descrição atualizada",
  "isCompleted": true
}

# Excluir ToDo
DELETE /api/todos/{id}
```

## Estrutura do Projeto

```
core-ledger-api/
├── CoreLedger.API/
│   ├── Controllers/          # Controllers da API
│   ├── Extensions/           # Extensões de configuração de serviços
│   ├── Middleware/           # Middleware customizado
│   └── Program.cs            # Ponto de entrada da aplicação
├── CoreLedger.Application/
│   ├── DTOs/                 # Data Transfer Objects
│   ├── Mappings/             # Profiles do AutoMapper
│   ├── UseCases/             # Commands e Queries (CQRS)
│   └── Validators/           # Validadores FluentValidation
├── CoreLedger.Domain/
│   ├── Entities/             # Entidades de domínio
│   ├── Exceptions/           # Exceções de domínio
│   ├── Interfaces/           # Interfaces da aplicação (IApplicationDbContext)
│   └── ValueObjects/         # Value objects
├── CoreLedger.Infrastructure/
│   ├── Persistence/          # DbContext e configurações de entidades
│   │   └── Migrations/       # Migrations do EF Core
│   └── Services/
│       ├── QueryServices/    # Filtragem e paginação RFC-8040
│       └── External/         # Integrações com serviços externos
├── CoreLedger.Worker/
│   ├── Consumers/            # Consumidores de mensagens RabbitMQ
│   └── Program.cs            # Ponto de entrada do worker service
├── CoreLedger.UnitTests/
│   ├── Application/          # Testes da camada de aplicação
│   └── Domain/               # Testes da camada de domínio
├── CoreLedger.IntegrationTests/
│   └── API/                  # Testes de integração da API
└── docs/
    └── archive/              # Documentação arquivada
```

## Diretrizes de Desenvolvimento

### Padrões de Código

- **Tipos de Referência Anuláveis**: Habilitado em todo o projeto (`<Nullable>enable</Nullable>`)
- **Warnings como Erros**: Compilação estrita (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`)
- **Documentação XML**: Obrigatória para todas as APIs públicas
- **Convenções de Nomenclatura**: Seguir as diretrizes Microsoft .NET

### Padrões de Testes

- **Cobertura de Testes Unitários**: Mínimo de 80% para camadas de Application e Domain
- **Nomenclatura de Testes**: `NomeDoMetodo_Cenario_ComportamentoEsperado`
- **Arrange-Act-Assert**: Seguir padrão AAA em todos os testes
- **Mocking**: Usar NSubstitute para mocking baseado em interfaces

### Melhores Práticas de Segurança

- **Gerenciamento de Secrets**: Usar User Secrets (dev) ou Azure Key Vault (prod)
- **Validação de Entrada**: FluentValidation para todos os commands e queries
- **Tratamento de Erros**: Middleware global de exceções previne vazamento de detalhes internos
- **HTTPS**: Obrigatório em ambientes não-desenvolvimento
- **Trilhas de Auditoria**: Logging estruturado com IDs de correlação

### Diretrizes de Banco de Dados

- **Migrations**: Sempre usar migrations explícitas; nunca `EnsureCreated()`
- **Transações**: Transações explícitas para operações contábeis multi-etapas
- **Concorrência**: Tokens de concorrência otimista para dados contábeis críticos
- **Tratamento de Valores Monetários**: Usar `decimal` com precisão/escala explícitas para todos os valores monetários

## Migrations do Banco de Dados

### Criar uma Nova Migration

```bash
dotnet ef migrations add NomeDaMigration --project CoreLedger.Infrastructure --startup-project CoreLedger.API
```

### Aplicar Migrations

```bash
dotnet ef database update --project CoreLedger.Infrastructure --startup-project CoreLedger.API
```

### Reverter Migration

```bash
dotnet ef database update NomeDaMigrationAnterior --project CoreLedger.Infrastructure --startup-project CoreLedger.API
```

### Remover Última Migration (se não aplicada)

```bash
dotnet ef migrations remove --project CoreLedger.Infrastructure --startup-project CoreLedger.API
```

## Build para Produção

### Build Release

```bash
dotnet build --configuration Release
```

### Publicar

```bash
dotnet publish CoreLedger.API/CoreLedger.API.csproj --configuration Release --output ./publish
```

### Build Docker (Futuro)

```bash
# TODO: Adicionar Dockerfile
docker build -t core-ledger-api:latest .
```

## Logging

Os logs são escritos em:
- **Console**: Saída estruturada com timestamps
- **Arquivo**: `logs/core-ledger-{Date}.log` (retido por 30 dias)

### Níveis de Log
- **Information**: Eventos de alto nível (transação registrada, lançamento contábil postado, NAV calculado)
- **Warning**: Anomalias recuperáveis
- **Error**: Falhas que requerem atenção
- **Critical**: Falhas em nível de sistema

### IDs de Correlação

Cada requisição recebe um ID de correlação (via header `X-Correlation-ID` ou gerado automaticamente) para rastreamento de ponta a ponta.

## Contribuindo

1. Faça fork do repositório
2. Crie uma branch de feature (`git checkout -b feature/funcionalidade-incrivel`)
3. Faça commit das suas alterações (`git commit -m 'Adiciona funcionalidade incrível'`)
4. Faça push para a branch (`git push origin feature/funcionalidade-incrivel`)
5. Abra um Pull Request

### Requisitos para PR

- Todos os testes passam (`dotnet test`)
- Código segue as convenções do projeto
- Documentação XML para APIs públicas
- Testes unitários para novas funcionalidades
- Sem warnings ou erros

## Licença

[Especifique sua licença aqui - MIT, Apache 2.0, Proprietária, etc.]

## Autores

- **Equipe de Desenvolvimento** - [Sua Organização]

## Agradecimentos

- Construído como um sistema de contabilidade de fundos ABOR para clientes financeiros institucionais
- Segue padrões de segurança e conformidade de nível empresarial
- Projetado para auditabilidade, integridade de dados e comportamento determinístico
- Serve como o livro contábil autoritativo de registros para fundos de investimento

---

**Para suporte ou dúvidas, por favor abra uma issue no GitHub.**
