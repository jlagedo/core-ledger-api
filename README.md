<img width="1280" height="640" alt="core-fund-accounting-social-card" src="https://github.com/user-attachments/assets/525189ff-20bb-43fc-a2a2-8c6f3d315e79" />

# Core Ledger API

Uma API REST .NET 10 pronta para produção para **contabilidade de fundos (Fund Accounting)** voltada ao mercado brasileiro de fundos de investimento, implementando princípios de Clean Architecture com foco em segurança, auditabilidade e conformidade regulatória. Este sistema serve como a fonte autoritativa de dados contábeis (ABOR - Accounting Book of Records) para fundos de investimento.

## Visão Geral do Sistema

O Core Ledger API é um **Portfolio Management System / Fund Administration System** capaz de:

- Processar transações de compra e venda de ativos do mercado financeiro
- Consolidar posições na carteira diária
- Calcular o NAV (Net Asset Value) para processamento de aplicação e resgate
- Atender às exigências regulatórias da CVM, ANBIMA e demais órgãos

### Referências Regulatórias

| Norma | Descrição |
|-------|-----------|
| **Resolução CVM 175/2022** | Marco regulatório de fundos de investimento |
| **Instrução CVM 577/2016** | Plano Contábil dos Fundos de Investimento (COFI) |
| **Código ANBIMA** | Autorregulação de fundos |
| **Lei 11.033/2004** | Tributação de fundos de investimento |

---

## Arquitetura

Este projeto segue **Clean Architecture** (Arquitetura Hexagonal) com clara separação de responsabilidades:

```
CoreLedger.API/              # Camada de Apresentação (Controllers, Middleware, Hubs SignalR)
CoreLedger.Application/      # Camada de Aplicação (Use Cases, DTOs, Validators)
CoreLedger.Domain/           # Camada de Domínio (Entities, Value Objects, Interfaces)
CoreLedger.Infrastructure/   # Camada de Infraestrutura (EF Core, Repositories, Cache, Mensageria)
CoreLedger.Workers/          # Background Workers (Processamentos Assíncronos)
CoreLedger.Shared/           # Utilitários e Constantes
CoreLedger.UnitTests/        # Testes Unitários (xUnit + NSubstitute)
CoreLedger.IntegrationTests/ # Testes de Integração (Testcontainers + xUnit)
CoreLedger.FunctionalTests/  # Testes Funcionais (E2E)
```

### Padrões Arquiteturais Principais

- **CQRS**: Command Query Responsibility Segregation usando MediatR
- **Domain-Driven Design**: Modelos de domínio ricos com lógica de negócio
- **Clean Architecture**: Separação de responsabilidades por camadas
- **Domain Events**: Comunicação entre agregados
- **Outbox Pattern**: Garantia de entrega de mensagens
- **Pipeline de Middleware**: Responsabilidades transversais (tratamento de exceções, logging, IDs de correlação)

### Schemas de Banco de Dados

```sql
CREATE SCHEMA cadastros;      -- Fundos, ativos, cotistas
CREATE SCHEMA carteira;       -- Operações, posições
CREATE SCHEMA passivo;        -- Movimentações de cotistas
CREATE SCHEMA cota;           -- PL, cotas, fechamento
CREATE SCHEMA pricing;        -- Preços e indexadores
CREATE SCHEMA audit;          -- Logs e histórico
```

---

## Stack Tecnológica

### Framework Principal

| Camada | Tecnologia | Versão |
|--------|------------|--------|
| **Backend API** | .NET | 10 |
| **Backend Workers** | .NET | 10 |
| **Real-Time** | SignalR | 8+ |
| **Banco de Dados** | PostgreSQL | 16+ |
| **Cache** | Redis | 7+ |
| **Message Queue** | RabbitMQ | 3.12+ |
| **Identity Provider** | Auth0 / Microsoft Entra ID | - |

### Pacotes e Bibliotecas (.NET)

#### API e Web
| Pacote | Finalidade |
|--------|------------|
| **Microsoft.AspNetCore.OpenApi** | Documentação OpenAPI/Swagger |
| **Swashbuckle.AspNetCore** | UI do Swagger |
| **FluentValidation.AspNetCore** | Validação de requests |
| **Serilog.AspNetCore** | Logging estruturado |
| **AspNetCoreRateLimit** | Rate limiting |
| **Microsoft.AspNetCore.Authentication.JwtBearer** | Validação de tokens JWT |
| **Microsoft.AspNetCore.SignalR** | Comunicação real-time |

#### Acesso a Dados
| Pacote | Finalidade |
|--------|------------|
| **Npgsql.EntityFrameworkCore.PostgreSQL** | EF Core para PostgreSQL |
| **Dapper** | Queries de alta performance (relatórios) |
| **EFCore.BulkExtensions** | Operações em lote |

#### Cache e Mensageria
| Pacote | Finalidade |
|--------|------------|
| **StackExchange.Redis** | Cliente Redis |
| **Microsoft.Extensions.Caching.StackExchangeRedis** | Cache distribuído |
| **RabbitMQ.Client** | Cliente RabbitMQ |

#### Utilitários e Cálculos
| Pacote | Finalidade |
|--------|------------|
| **AutoMapper** | Mapeamento objeto-objeto |
| **MediatR** | Padrão CQRS e Mediator |
| **Polly** | Resiliência e retry policies |
| **MathNet.Numerics** | Cálculos matemáticos e estatísticos |
| **NodaTime** | Manipulação precisa de datas/horários |
| **CsvHelper** | Importação/exportação CSV |
| **ClosedXML** | Geração de Excel |

---

## Módulos do Sistema

### Priorização para MVP

| Prioridade | Módulo | Justificativa | Criticidade |
|------------|--------|---------------|-------------|
| 1 | Cadastros | Base para tudo funcionar | Bloqueante |
| 2 | Gestão de Carteira | Registro e liquidação de operações | Bloqueante |
| 3 | Precificação | MtM diário obrigatório | Bloqueante |
| 4 | Cálculo de Cota | Core do sistema — sem isso não há fundo | Bloqueante |
| 5 | Gestão de Passivo | Aplicações e resgates de cotistas | Bloqueante |
| 6 | Enquadramento | Obrigação regulatória | Alta |
| 7 | Obrigações CVM | Informe diário é obrigatório | Alta |
| 8 | Contabilidade | Pode ser simplificada inicialmente | Média |
| 9 | Tesouraria | Pode ser manual inicialmente | Média |
| 10 | Relatórios | Pode ser básico inicialmente | Média |

### Módulo de Cadastros (Core Data)

| Feature | Descrição |
|---------|-----------|
| **Cadastro de Fundos** | CNPJ, nome, tipo, parâmetros de cota, multiclasse (CVM 175) |
| **Classes e Subclasses** | Estrutura multiclasse conforme Resolução CVM 175 |
| **Parâmetros de Cota** | Casas decimais, horário de corte |
| **Taxas do Fundo** | Administração, gestão, custódia, performance (% a.a.) |
| **Cadastro de Ativos** | Código, tipo, emissor, vencimento, indexador |
| **Cadastro de Cotistas** | PF/PJ, CPF/CNPJ, dados bancários, suitability |
| **Emissores/Contrapartes** | Dados cadastrais, rating de crédito, limites |
| **Contas Bancárias** | Banco, agência, conta do fundo |
| **Indexadores** | CDI, Selic, IPCA, IGP-M (valores históricos) |
| **Calendário** | Feriados e dias úteis (B3, Anbima) |

### Tipos de Ativos Suportados

#### MVP Fase 1
| Categoria | Ativos |
|-----------|--------|
| Títulos Públicos | LFT, LTN, NTN-B, NTN-B Principal |
| Títulos Privados Bancários | CDB, LCI, LCA |
| Operações | Compromissadas |
| Cotas | Cotas de outros fundos (FI, FIC, FIDC, FII) |
| Renda Variável | Ações (ON/PN), ETFs |

#### Expansão Fase 2
| Categoria | Ativos |
|-----------|--------|
| Derivativos | Futuros, Opções |
| Títulos Privados | Debêntures, CRI, CRA |

#### Expansão Fase 3
| Categoria | Ativos |
|-----------|--------|
| Derivativos Complexos | Swaps, NDF, Termos |
| Exterior | Stocks, Bonds, ETFs Internacionais |
| Outros | Criptoativos, Créditos de Carbono |

### Módulo de Gestão de Carteira (Portfolio)

| Feature | Descrição |
|---------|-----------|
| **Boletagem Manual** | Interface para registro de operações |
| **Compra de Ativos** | Registro de compras com custo |
| **Venda de Ativos** | Registro de vendas, cálculo de resultado |
| **Liquidação** | Controle de D+0, D+1, D+2, D+3 |
| **Posição de Carteira** | Quantidade, custo médio, valor de mercado |
| **Eventos Corporativos** | Dividendos, JCP, cupons, vencimentos, bonificações |
| **Cálculo de Custo Médio** | FIFO ou Custo Médio Ponderado |

### Módulo de Precificação (MtM)

| Feature | Descrição |
|---------|-----------|
| **Importação de Preços** | ANBIMA, B3, manual |
| **Histórico de Preços** | Série histórica por ativo |
| **Cálculo de Valor de Mercado** | PU × Quantidade |
| **Validação de Preços** | Alerta de variação atípica |
| **Marcação a Mercado** | Conforme manual ANBIMA/CVM |

### Módulo de Gestão de Passivo (Cotistas)

| Feature | Descrição |
|---------|-----------|
| **Aplicação** | Registro de novas aplicações |
| **Resgate Total/Parcial** | Liquidação total ou parcial da posição |
| **Cotização** | Conversão valor → cotas pela cota do dia |
| **Posição de Cotistas** | Cotas, valor, custo médio por cotista |
| **Cálculo de IR** | IR sobre ganho de capital no resgate (tabela regressiva) |
| **Cálculo de IOF** | IOF regressivo (resgates < 30 dias) |
| **Come-cotas** | Antecipação semestral de IR |

### Módulo de Cálculo de Cota (NAV)

| Feature | Descrição |
|---------|-----------|
| **Cálculo de PL** | Patrimônio Líquido = Ativos - Passivos |
| **Cálculo de Cota** | Cota = PL / Quantidade de Cotas |
| **Provisão de Taxas** | Taxa de administração pro-rata diária |
| **Fechamento Diário** | Checklist, validação, aprovação |
| **Histórico de Cotas** | Série histórica de valores |

---

## Workers de Processamento Assíncrono

| Worker | Responsabilidade |
|--------|------------------|
| **PricingWorker** | Importação e validação de preços |
| **SettlementWorker** | Liquidação de operações |
| **NAVCalculationWorker** | Cálculo de cota em batch |
| **EventProcessingWorker** | Processamento de eventos corporativos |
| **ReportWorker** | Geração de relatórios |

---

## Ciclo Diário do Administrador

```
┌─────────────────────────────────────────────────────────────────┐
│                     CICLO DIÁRIO MÍNIMO                         │
├─────────────────────────────────────────────────────────────────┤
│  1. ABERTURA DO DIA                                             │
│     └── Verificar calendário, feriados, dia útil                │
│                                                                 │
│  2. IMPORTAR DADOS EXTERNOS                                     │
│     ├── Preços (ANBIMA, B3)                                     │
│     ├── Indexadores (CDI, Selic, IPCA)                          │
│     └── Extratos bancários                                      │
│                                                                 │
│  3. PROCESSAR OPERAÇÕES                                         │
│     ├── Boletagem / importação de operações                     │
│     ├── Liquidação de operações D+0                             │
│     └── Conciliação com custódia                                │
│                                                                 │
│  4. PROCESSAR EVENTOS                                           │
│     ├── Eventos corporativos (dividendos, juros, etc.)          │
│     └── Vencimentos, cupons e amortizações                      │
│                                                                 │
│  5. PROCESSAR PASSIVO                                           │
│     ├── Aplicações do dia                                       │
│     ├── Resgates (cotização e liquidação)                       │
│     └── Cálculo de IR/IOF                                       │
│                                                                 │
│  6. PROVISIONAR DESPESAS                                        │
│     ├── Taxa de administração                                   │
│     ├── Taxa de custódia                                        │
│     └── Taxa de gestão                                          │
│                                                                 │
│  7. PRECIFICAR CARTEIRA                                         │
│     └── Marcar a mercado todos os ativos                        │
│                                                                 │
│  8. VERIFICAR ENQUADRAMENTO                                     │
│     └── Checar limites e regras CVM                             │
│                                                                 │
│  9. CALCULAR COTA                                               │
│     ├── Calcular PL                                             │
│     ├── Calcular quantidade de cotas                            │
│     ├── Calcular valor da cota                                  │
│     └── Validar e aprovar                                       │
│                                                                 │
│  10. FECHAR DIA                                                 │
│      └── Travar data, publicar cota                             │
└─────────────────────────────────────────────────────────────────┘
```

---

## API REST

### Padrões de API

| Item | Especificação |
|------|---------------|
| Versão | v1 (ex: /api/v1/fundos) |
| Formato | JSON |
| Autenticação | OAuth 2.0 / JWT (Auth0 ou Microsoft Entra ID) |
| Rate Limiting | 1000 req/min (Starter), 5000 req/min (Pro) |
| Paginação | Offset-based (page, pageSize) |
| Ordenação | sort=campo:asc|desc |
| Filtros | filter[campo]=valor (RFC-8040) |

### Endpoints Principais

#### Fundos
```yaml
GET    /api/v1/fundos                    # Lista fundos (paginado)
POST   /api/v1/fundos                    # Cria fundo
GET    /api/v1/fundos/{id}               # Obtém fundo por ID
PUT    /api/v1/fundos/{id}               # Atualiza fundo
DELETE /api/v1/fundos/{id}               # Exclui fundo (soft delete)
GET    /api/v1/fundos/{id}/classes       # Lista classes do fundo
GET    /api/v1/fundos/{id}/taxas         # Lista taxas do fundo
GET    /api/v1/fundos/{id}/prazos        # Lista prazos
```

#### Ativos
```yaml
GET    /api/v1/ativos                    # Lista ativos
POST   /api/v1/ativos                    # Cria ativo
GET    /api/v1/ativos/{id}               # Obtém ativo
PUT    /api/v1/ativos/{id}               # Atualiza ativo
DELETE /api/v1/ativos/{id}               # Exclui ativo (soft delete)
GET    /api/v1/ativos/{id}/precos        # Histórico de preços
GET    /api/v1/ativos/{id}/eventos       # Eventos do ativo
POST   /api/v1/ativos/importar           # Importação em lote
```

#### Preços
```yaml
POST   /api/v1/precos/importar           # Importar preços do dia
GET    /api/v1/precos                    # Consultar preços de uma data
GET    /api/v1/precos/pendentes          # Ativos sem preço
```

#### Operações (Carteira)
```yaml
GET    /api/v1/operacoes                 # Lista operações
POST   /api/v1/operacoes                 # Cria operação (boleta)
GET    /api/v1/operacoes/{id}            # Obtém operação
PUT    /api/v1/operacoes/{id}            # Atualiza operação
POST   /api/v1/operacoes/{id}/liquidar   # Liquida operação
POST   /api/v1/operacoes/{id}/cancelar   # Cancela operação
```

#### Passivo (Movimentações)
```yaml
GET    /api/v1/movimentacoes             # Lista movimentações
POST   /api/v1/movimentacoes/aplicacao   # Nova aplicação
POST   /api/v1/movimentacoes/resgate     # Novo resgate
GET    /api/v1/cotistas/{id}/posicao     # Posição do cotista
GET    /api/v1/cotistas/{id}/extrato     # Extrato do cotista
```

#### Cota
```yaml
POST   /api/v1/cotas/calcular            # Calcula cota do dia
POST   /api/v1/cotas/fechar              # Fecha dia
GET    /api/v1/cotas/historico           # Histórico de cotas
GET    /api/v1/cotas/pl                  # Composição do PL
```

### Eventos SignalR (Real-time)

| Evento | Notificação |
|--------|-------------|
| **operacao.liquidada** | Atualização de posição de carteira |
| **precos.importados** | Liberação para fechamento |
| **cota.calculada** | Valor da cota disponível |
| **desenquadramento** | Alerta para gestor |
| **movimentacao.processada** | Confirmação para operador |

---

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (para PostgreSQL, Redis e RabbitMQ)
- [PostgreSQL 16+](https://www.postgresql.org/download/) (ou use Docker)
- [Redis 7+](https://redis.io/download) (ou use Docker)
- [RabbitMQ 3.12+](https://www.rabbitmq.com/download.html) (ou use Docker)
- IDE: [Visual Studio 2025](https://visualstudio.microsoft.com/), [Rider](https://www.jetbrains.com/rider/), ou [VS Code](https://code.visualstudio.com/)

---

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
```

### 3. Inicie os Serviços com Docker Compose

```bash
# Inicie os containers
docker-compose up -d

# Verifique se os containers estão rodando
docker ps
```

Os seguintes serviços estarão disponíveis:
- **PostgreSQL**: localhost:5432
- **Redis**: localhost:6379
- **RabbitMQ AMQP**: localhost:5672
- **RabbitMQ Management UI**: http://localhost:15672 (guest/guest)
- **Seq (Logs)**: http://localhost:8081

### 4. Aplique as Migrations do Banco de Dados

```bash
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

---

## Docker Compose (Desenvolvimento)

```yaml
version: '3.8'
services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_DB: fund_accounting
      POSTGRES_USER: fund_admin
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data

  rabbitmq:
    image: rabbitmq:3.12-management
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: fund_user
      RABBITMQ_DEFAULT_PASS: ${RABBITMQ_PASSWORD}

  seq:
    image: datalust/seq:latest
    ports:
      - "5341:5341"
      - "8081:80"
    environment:
      ACCEPT_EULA: Y

volumes:
  postgres_data:
  redis_data:
```

---

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

---

## Regras de Negócio Essenciais

### Cálculo de Custo Médio

```
Custo Médio = (Quantidade Anterior × Custo Anterior + Quantidade Nova × Preço Compra) 
              / (Quantidade Anterior + Quantidade Nova)
```

### Cálculo de IR no Resgate (Fundos Longo Prazo)

```
Base de Cálculo = (Cota Resgate - Custo Médio Aquisição) × Quantidade Cotas

Alíquotas:
- Até 180 dias: 22,5%
- 181 a 360 dias: 20%
- 361 a 720 dias: 17,5%
- Acima de 720 dias: 15%
```

### Taxa de Administração Pro-Rata

```
Provisão Diária = (PL × Taxa Anual) / 252

Onde:
- PL = Patrimônio Líquido do dia anterior
- Taxa Anual = Taxa de administração em decimal (ex: 0.02 para 2% a.a.)
- 252 = Dias úteis no ano
```

---

## Requisitos Não-Funcionais

| Requisito | Especificação |
|-----------|---------------|
| Disponibilidade | 99.9% em horário comercial (8h-20h) |
| Performance | Fechamento de cota < 5 min por fundo |
| Escalabilidade | Suporte a 500+ fundos simultâneos |
| Precisão | 8 casas decimais para cotas, 15 para cálculos internos |
| Auditoria | Retenção de logs por 5 anos (exigência regulatória) |
| Backup | RPO < 1 hora, RTO < 4 horas |
| Segurança | Criptografia em trânsito e repouso, MFA |
| Tempo de resposta API | < 200ms (p95) |

---

## Diretrizes de Desenvolvimento

### Padrões de Código

- **Tipos de Referência Anuláveis**: Habilitado em todo o projeto
- **Warnings como Erros**: Compilação estrita
- **Documentação XML**: Obrigatória para todas as APIs públicas
- **Convenções de Nomenclatura**: Seguir as diretrizes Microsoft .NET

### Estratégia de Identificadores (IDs)

| Tipo | Uso | Justificativa |
|------|-----|---------------|
| **UUID** | Entidades expostas via API pública | Não expõe volume, geração distribuída |
| **BIGINT** | Tabelas de alto volume operacional | Performance de índice B-tree |
| **INTEGER** | Tabelas de parâmetros/lookup | Simplicidade, baixo volume |

### Padrões de Testes

- **Cobertura de Testes Unitários**: Mínimo de 80% para camadas de Application e Domain
- **Nomenclatura de Testes**: `NomeDoMetodo_Cenario_ComportamentoEsperado`
- **Arrange-Act-Assert**: Seguir padrão AAA em todos os testes

### Diretrizes de Banco de Dados

- **Migrations**: Sempre usar migrations explícitas
- **Transações**: Transações explícitas para operações contábeis multi-etapas
- **Concorrência**: Tokens de concorrência otimista para dados contábeis críticos
- **Tratamento de Valores Monetários**: Usar `decimal` com precisão/escala explícitas

---

## Health Checks

- **Saúde Geral**: `GET /health`
- **Prontidão**: `GET /health/ready`
- **Vivacidade**: `GET /health/live`

---

## Logging

Os logs são escritos em:
- **Console**: Saída estruturada com timestamps
- **Seq**: Centralização de logs (http://localhost:8081)
- **Arquivo**: `logs/core-ledger-{Date}.log` (retido por 30 dias)

### Níveis de Log
- **Information**: Eventos de alto nível (transação registrada, cota calculada)
- **Warning**: Anomalias recuperáveis
- **Error**: Falhas que requerem atenção
- **Critical**: Falhas em nível de sistema

### IDs de Correlação

Cada requisição recebe um ID de correlação (via header `X-Correlation-ID` ou gerado automaticamente) para rastreamento de ponta a ponta.

---

## Glossário

| Termo | Definição |
|-------|-----------|
| **NAV** | Net Asset Value - Valor Patrimonial Líquido |
| **PL** | Patrimônio Líquido |
| **MtM** | Mark-to-Market - Marcação a Mercado |
| **Cota** | Fração ideal do patrimônio do fundo |
| **Cotização** | Conversão de valor financeiro em cotas |
| **Come-cotas** | Antecipação semestral de IR em fundos |
| **Enquadramento** | Verificação de limites regulatórios |
| **Boletagem** | Registro de operações no sistema |
| **D+X** | Prazo em dias úteis após a data de referência |
| **ABOR** | Accounting Book of Records |

---

## Roadmap de Implementação

### Sprint 0 - Setup (1 semana)
- [ ] Estrutura de projetos .NET
- [ ] Docker Compose com PostgreSQL, Redis, RabbitMQ
- [ ] CI/CD básico
- [ ] Configuração de logging (Serilog + Seq)

### Sprint 1-2 - Fundação (2 semanas)
- [ ] Entidades de domínio
- [ ] EF Core DbContext e migrations
- [ ] Autenticação JWT
- [ ] Cadastros básicos (CRUD)

### Sprint 3-5 - Core Features (3 semanas)
- [ ] Boletagem de operações
- [ ] Precificação
- [ ] Workers de processamento
- [ ] Integração com Redis e RabbitMQ

### Sprint 6-8 - NAV e Passivo (3 semanas)
- [ ] Gestão de passivo
- [ ] Cálculo de cota
- [ ] Fechamento diário

### Sprint 9-10 - Refinamento (2 semanas)
- [ ] Testes de integração
- [ ] Performance tuning
- [ ] Documentação de API

---

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

---

## Licença

[Especifique sua licença aqui - MIT, Apache 2.0, Proprietária, etc.]

## Autor

- **Dev Solo** — Um desenvolvedor curioso explorando o mundo de Fund Accounting com ajuda de IAs

---

## Referências

- [Resolução CVM nº 175/2022](https://www.gov.br/cvm/pt-br) - Marco Regulatório de Fundos de Investimento
- [Instrução CVM nº 577/2016](https://www.gov.br/cvm/pt-br) - Plano Contábil dos Fundos de Investimento (COFI)
- [Código ANBIMA](https://www.anbima.com.br) - Regulação e Melhores Práticas para Fundos de Investimento
- [Manual de Marcação a Mercado ANBIMA](https://www.anbima.com.br)

---

**Para suporte ou dúvidas, por favor abra uma issue no GitHub.**

---

## ⚠️ Disclaimer (Leia antes de sair investindo!)

**Calma aí, investidor!** 🛑

Este projeto é **100% educacional** e está sendo desenvolvido como um laboratório de testes para explorar as capacidades de **Inteligência Artificial no desenvolvimento de software**. Ou seja: nenhum fundo foi administrado, nenhuma cota foi calculada, e nenhum cotista foi prejudicado na produção deste README.

### 🤖 A Jornada dos AI Coding Assistants

Durante o desenvolvimento, testei várias ferramentas de IA para coding:

| Ferramenta | Veredicto |
|------------|-----------|
| **Windsurf** | Bom, mas limitado por estar disponível apenas como IDE separada ou plugin do JetBrains. Não oferece um pacote completo que dê pra usar fora disso. |
| **GitHub Copilot** | É bom, mas cada dia que passa está ficando mais integrado no mundo Microsoft/GitHub. Mesmo usando .NET, eu queria algo diferente do ecossistema Microsoft *(sim, a ironia de fazer o projeto em .NET não me passou despercebida 😅)* |
| **Claude Code** | 🏆 **O escolhido!** |

### 💜 Por que Claude Code é diferente?

Eu adorei o **Claude Code** principalmente porque ele **fica fora da IDE**. Uso mais pelo terminal e, de início, parece loucura, mas isso separa meu contexto mental do "modo prompt" do "modo código".

**Confissão:** Eu *odeio* estar programando e ter uma IA tentando me ajudar o tempo todo. Perco o foco completamente. Aquelas sugestões inline que ficam piscando enquanto você tenta pensar? Tortura.

Com o Claude Code no terminal, eu:
1. **Codifico em paz** — Sem interrupções, sem sugestões não solicitadas
2. **Mudo de contexto conscientemente** — Quando preciso de ajuda, abro o terminal e converso
3. **Mantenho o foco** — Cada coisa no seu lugar

Além disso:

- **Entende contexto de verdade** — Você explica o domínio de Fund Accounting uma vez e ele *entende*. Não precisa repetir 47 vezes que "cota não é a mesma coisa que ação".

- **Gera código que funciona** — E não aquele código que "quase funciona" e você passa 3 horas debugando um ponto-e-vírgula.

- **Não inventa biblioteca que não existe** — Sabe aquele momento constrangedor quando a IA sugere um pacote NuGet que simplesmente... não existe? Pois é, aqui não.

- **Claude AI Projects** — Essa funcionalidade é sensacional. Você joga os documentos de especificação, regulamentação da CVM, e o Claude mantém tudo como contexto persistente. Este README mesmo foi gerado no Projects, com todos os docs do projeto disponíveis pra consulta. Game changer.

*(Inserir qualquer jabá adicional do Claude aqui — eles merecem depois de me aguentar perguntando sobre come-cotas às 2h da manhã)*

### 🎯 O Objetivo Real

Este projeto serve para:

1. **Aprender** sobre o mercado brasileiro de fundos de investimento
2. **Testar** até onde as IAs conseguem ajudar em projetos complexos de domínio específico
3. **Documentar** as melhores práticas de desenvolvimento com assistentes de IA
4. **Se divertir** (sim, programar pode ser divertido quando você tem um bom copiloto)

### ⚖️ Aviso Legal (O Chato, mas Necessário)

- Este código **NÃO** deve ser usado em produção para administrar fundos reais
- **NÃO** nos responsabilizamos se você usar isso e a CVM bater na sua porta
- As regras de negócio são baseadas na regulamentação vigente, mas podem conter erros (somos humanos... e IAs)
- Se você é da CVM e está lendo isso: oi! 👋 É só um projeto de estudo, juro!

### 🚀 Quer Testar Também?

Se você está curioso sobre desenvolvimento assistido por IA, recomendo fortemente dar uma chance ao [Claude Code](https://claude.ai). Não, eles não estão me pagando (mas deveriam, depois dessa propaganda toda 😄).

---

*"O futuro do desenvolvimento de software é colaborativo: humanos definindo o 'o quê' e 'por quê', e IAs ajudando com o 'como'."*

— Um dev solo que passou muitas horas conversando com uma IA sobre contabilidade de fundos

---

*Documento atualizado em Janeiro/2026*  
*Feito com ☕, muita curiosidade, muita solidão (é só eu aqui, galera), e uma quantidade saudável de prompts bem elaborados*
