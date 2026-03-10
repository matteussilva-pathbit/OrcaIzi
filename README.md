# OrcaIzi - Sistema de Gestão de Orçamentos

OrcaIzi é uma solução completa para gestão de orçamentos, composta por uma **Web API** robusta e um **Frontend** moderno. O projeto utiliza **Clean Architecture**, **DDD (Domain-Driven Design)** e práticas profissionais de desenvolvimento .NET.

## 🚀 Tecnologias Utilizadas

### Backend (.NET 10)
- **ASP.NET Core Web API**: Framework principal.
- **Entity Framework Core**: ORM com SQL Server.
- **Identity**: Gestão de Usuários e Autenticação JWT.
- **Serilog**: Logging estruturado (Console e Arquivo).
- **FluentValidation**: Validação robusta de DTOs.
- **QuestPDF**: Geração de documentos PDF profissionais.
- **xUnit & Moq**: Testes Unitários.
- **Microsoft.AspNetCore.Mvc.Testing**: Testes de Integração (com In-Memory DB).
- **Docker**: Containerização do SQL Server.

### Frontend (ASP.NET Core)
- **Razor Pages**: Framework para construção de UI server-side.
- **Bootstrap 5**: Framework CSS para layout responsivo.
- **HTML5 & CSS3**: Estrutura e estilização.
- **Chart.js**: Visualização de dados e gráficos interativos.

## 🏗️ Arquitetura da Solução

A solução `OrcaIzi.sln` é organizada em projetos seguindo a Clean Architecture:

1.  **OrcaIzi.Domain**: Camada central. Entidades (`Budget`, `Customer`, `User`), Interfaces e Regras de Negócio. Sem dependências externas.
2.  **OrcaIzi.Application**: Camada de aplicação. Serviços (`AppServices`), DTOs, Validadores e Interfaces.
3.  **OrcaIzi.Infrastructure**: Camada de infraestrutura. Implementação de Repositórios, `DbContext`, Migrations e serviços externos (PDF, WhatsApp).
4.  **OrcaIzi.WebAPI**: Camada de apresentação (API). Controllers, Middleware de Tratamento de Erros Global, Configuração de DI e Swagger.
5.  **OrcaIzi.Web**: Camada de apresentação (Frontend). Aplicação Razor Pages para interação do usuário final.
6.  **OrcaIzi.Tests**: Testes Unitários focados na lógica de negócio e serviços.
7.  **OrcaIzi.IntegrationTests**: Testes de Integração que validam o fluxo completo da API (Controller -> Service -> Repo -> DB em Memória).

## 🛠️ Como Rodar o Projeto

### Pré-requisitos
- [Docker Desktop](https://www.docker.com/products/docker-desktop) instalado.
- [.NET SDK 10](https://dotnet.microsoft.com/download) instalado.

### Passo a Passo

1.  **Banco de Dados**
    Inicie o SQL Server via Docker:
    ```bash
    docker-compose up -d
    ```

2.  **Backend (API)**
    Aplique as migrations e inicie a API:
    ```bash
    dotnet ef database update --project src/OrcaIzi.Infrastructure --startup-project src/OrcaIzi.WebAPI
    dotnet run --project src/OrcaIzi.WebAPI/OrcaIzi.WebAPI.csproj
    ```
    Acesse o Swagger: `http://localhost:5012/swagger`

3.  **Frontend (Web)**
    Em outro terminal, inicie a aplicação Web:
    ```bash
    dotnet run --project src/OrcaIzi.Web/OrcaIzi.Web.csproj
    ```
    Acesse no navegador: `http://localhost:5086` (a porta pode variar dependendo do seu ambiente, verifique o console).

4.  **Rodar Testes**
    Para garantir a integridade do sistema, execute a suíte de testes:
    ```bash
    dotnet test
    ```

## ⚡ Comandos Rápidos (Cheat Sheet)

| Ação | Comando |
|------|---------|
| **Subir Banco** | `docker-compose up -d` |
| **Parar Banco** | `docker-compose down` |
| **Rodar API** | `dotnet run --project src/OrcaIzi.WebAPI` |
| **Rodar Web** | `dotnet run --project src/OrcaIzi.Web` |
| **Testar** | `dotnet test` |
| **Criar Migration** | `dotnet ef migrations add NomeMigration --project src/OrcaIzi.Infrastructure --startup-project src/OrcaIzi.WebAPI` |
| **Aplicar Migration** | `dotnet ef database update --project src/OrcaIzi.Infrastructure --startup-project src/OrcaIzi.WebAPI` |
| **Listar Migrations** | `dotnet ef migrations list --project src/OrcaIzi.Infrastructure --startup-project src/OrcaIzi.WebAPI` |

## ✅ Funcionalidades Implementadas

- **CRUD Completo**: Clientes, Orçamentos e Modelos de Orçamento (templates).
- **Paginação**: Endpoints otimizados com paginação (`PagedResult`).
- **Autenticação Segura**: Registro e Login com JWT.
- **Dashboard Interativo**: Visão geral do negócio com gráficos e estatísticas em tempo quase real.
- **Exportação PDF**: Geração de orçamentos em PDF com layout profissional (v2), incluindo dados da empresa, cliente, itens e condição de pagamento.
- **Pagamento Pix (Mercado Pago)**:
  - Geração de cobrança Pix diretamente do orçamento.
  - Exibição de QR Code, código “copia e cola” e link do boleto Pix.
  - Sincronização de status via API e webhook Mercado Pago.
  - Atualização automática do status do orçamento para **Paid** quando aprovado.
- **Link Público do Orçamento**:
  - Geração de link único de acesso sem login.
  - Página pública com visualização do orçamento, botão de pagamento Pix e formulário de **aprovar/rejeitar**.
  - Registro de “assinatura” simples (nome + documento) quando o cliente aprova ou rejeita.
- **Validação**: Dados de entrada validados automaticamente.
- **Tratamento de Erros**: Middleware global (RFC 7807 Problem Details).
- **Testes**: Cobertura com testes unitários e de integração (setup com `WebApplicationFactory` e EF In-Memory).
- **Logging**: Rastreamento de operações com Serilog.

## 💳 Configuração de Pagamentos (Mercado Pago)

No projeto `OrcaIzi.WebAPI`, a configuração de pagamentos fica em:

- [appsettings.Development.json](./src/OrcaIzi.WebAPI/appsettings.Development.json)

Chaves utilizadas:

```json
"Payments": {
  "WebhookToken": "",
  "MercadoPago": {
    "AccessToken": "",
    "NotificationUrl": ""
  }
}
```

### AccessToken
- Configure via **user-secrets** (recomendado para desenvolvimento) ou variáveis de ambiente em produção:

```bash
dotnet user-secrets init --project src/OrcaIzi.WebAPI
dotnet user-secrets set "Payments:MercadoPago:AccessToken" "SEU_TOKEN_AQUI" --project src/OrcaIzi.WebAPI
```

## 🗄️ Banco de Dados e Migrations

- O repositório inclui um `docker-compose.yml` para subir o SQL Server localmente.
- Para atualizar o schema, rode:

```bash
dotnet ef database update --project src/OrcaIzi.Infrastructure --startup-project src/OrcaIzi.WebAPI
```

- Se estiver em desenvolvimento, também é possível deixar a API aplicar migrations automaticamente via:
  - `Database:ApplyMigrationsOnStartup` em `appsettings.Development.json`.

### Webhook Mercado Pago
- Defina `NotificationUrl` com a URL pública do webhook:

```json
"Payments": {
  "WebhookToken": "opcional_token_segredo",
  "MercadoPago": {
    "AccessToken": "",
    "NotificationUrl": "https://seu-dominio.com/api/Payments/mercadopago/webhook"
  }
}
```

- No painel do Mercado Pago, configure o webhook para apontar para essa URL.
- Se `WebhookToken` estiver preenchido, envie o header `X-Webhook-Token` com o mesmo valor em todas as chamadas.

### Endpoints principais de pagamento

- `POST /api/Budgets/{id}/payment/pix` → Gera o pagamento Pix.
- `GET  /api/Budgets/{id}/payment` → Consulta pagamento salvo no orçamento.
- `POST /api/Budgets/{id}/payment/sync` → Sincroniza status no provedor.
- `POST /api/Payments/mercadopago/webhook` → Recebe notificações do Mercado Pago.

## 🔗 Link Público do Orçamento

Fluxo:

1. Usuário logado abre **Detalhes do Orçamento** no frontend (`/Budgets/Details/{id}`).
2. Clica em **Link Público**:
   - Chama `POST /api/Budgets/{id}/share`, que gera um `PublicShareId`.
   - O frontend monta o link:  
     `https://seu-site/Public/Budgets/{shareId}` e copia para o clipboard.
3. O cliente abre o link:
   - `GET /api/public/budgets/{shareId}` → carrega dados.
   - Página pública Razor: `/Public/Budgets/{shareId}`.
4. Na página pública o cliente pode:
   - Visualizar o orçamento completo.
   - Clicar **Pagar com Pix** (usa `PaymentLink` gerado pelo gateway, quando existir).
   - **Aprovar** → `POST /api/public/budgets/{shareId}/approve`.
   - **Rejeitar** → `POST /api/public/budgets/{shareId}/reject`.

Isso permite que o usuário envie somente um link para o cliente final, que consegue ver, aprovar e pagar o orçamento sem precisar criar conta.

## 🧪 Exemplos de Payload (API)

### Autenticação
**POST /api/Auth/login**
```json
{ "username": "admin", "password": "Password123!" }
```

### Orçamentos
**POST /api/Budgets**
```json
{
  "title": "Projeto Web",
  "description": "Desenvolvimento de site institucional",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "expirationDate": "2026-12-31T00:00:00Z",
  "items": [
    { "name": "Design", "description": "Layout Figma", "quantity": 1, "unitPrice": 1500 },
    { "name": "Frontend", "description": "Implementação React", "quantity": 1, "unitPrice": 3000 }
  ]
}
```

**PUT /api/Budgets/{id}**
```json
{
  "title": "Projeto Web (Atualizado)",
  "description": "Adicionado módulo Admin",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "expirationDate": "2027-01-15T00:00:00Z",
  "items": [
    { "name": "Design", "description": "Layout Figma", "quantity": 1, "unitPrice": 1500 },
    { "name": "Frontend Completo", "description": "React + Admin", "quantity": 1, "unitPrice": 5000 }
  ]
}
```
