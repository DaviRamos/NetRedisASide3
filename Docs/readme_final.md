# NetRedisASide3

## 🚀 Sistema de Gestão Enterprise com .NET 9

Aplicação completa desenvolvida em .NET 9 com arquitetura moderna, implementando **Cache Distribuído Redis**, **PostgreSQL**, **Autenticação Keycloak**, integração com **IA (Ollama)** e **Banco Vetorial (Weaviate)**.

---

## 📋 Índice

- [Características](#-características)
- [Pré-requisitos](#-pré-requisitos)
- [Instalação Rápida](#-instalação-rápida)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Configuração](#-configuração)
- [Uso](#-uso)
- [Testes com Postman](#-testes-com-postman)
- [Segurança](#-segurança)
- [Troubleshooting](#-troubleshooting)

---

## ✨ Características

### Stack Tecnológico
- ✅ **.NET 9** - Framework moderno e performático
- ✅ **PostgreSQL 16** - Banco de dados relacional com múltiplos schemas
- ✅ **Redis 7** - Cache distribuído (IDistributedCache)
- ✅ **Keycloak 23** - Identity Provider (OAuth 2.0/OpenID Connect)
- ✅ **Ollama** - Modelos de IA locais com suporte GPU NVIDIA
- ✅ **Weaviate** - Banco de dados vetorial para embeddings

### Arquitetura e Padrões
- ✅ **Repository Pattern** - Abstração de acesso a dados
- ✅ **Service Layer** - Lógica de negócio isolada
- ✅ **Minimal APIs** - Endpoints HTTP modernos e performáticos
- ✅ **FluentValidation** - Validação robusta de entrada
- ✅ **Health Checks** - Monitoramento de todos os serviços
- ✅ **Cache-Aside Pattern** - Estratégia de cache inteligente
- ✅ **User Secrets** - Proteção de credenciais sensíveis

### Segurança
- 🔒 Autenticação JWT via Keycloak
- 🔒 Proteção contra SQL Injection (EF Core parametrizado)
- 🔒 HTTPS obrigatório em produção
- 🔒 Validação de entrada com FluentValidation
- 🔒 Secrets management com User Secrets e .env
- 🔒 CORS configurado
- 🔒 Rate Limiting

---

## 🔧 Pré-requisitos

Certifique-se de ter instalado:

- **Docker Desktop** (com suporte a containers Linux)
- **Docker Compose** 
- **.NET 9 SDK** - [Download aqui](https://dotnet.microsoft.com/download/dotnet/9.0)
- **NVIDIA Docker** (para suporte GPU no Ollama) - [Guia de instalação](https://docs.nvidia.com/datacenter/cloud-native/container-toolkit/install-guide.html)
- **Git**

### Verificar Instalações

```bash
# Verificar Docker
docker --version
docker-compose --version

# Verificar .NET 9
dotnet --version

# Verificar NVIDIA Docker (se tiver GPU)
docker run --rm --gpus all nvidia/cuda:11.8.0-base-ubuntu22.04 nvidia-smi
```

---

## 🚀 Instalação Rápida

### 1. Clonar o Repositório

```bash
git clone https://github.com/seu-usuario/NetRedisASide3.git
cd NetRedisASide3
```

### 2. Configurar Variáveis de Ambiente

```bash
cp .env.example .env
```

Edite o arquivo `.env` com suas credenciais (ou use os valores padrão para desenvolvimento).

### 3. Configurar User Secrets

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:AssuntoDb" "Host=localhost;Port=5432;Database=assuntos_db;Username=assunto_user;Password=assunto_pass_secure_2025"
dotnet user-secrets set "ConnectionStrings:MovimentacaoDb" "Host=localhost;Port=5432;Database=movimentacoes_db;Username=movimentacao_user;Password=movimentacao_pass_secure_2025"
dotnet user-secrets set "ConnectionStrings:TipoDocumentoDb" "Host=localhost;Port=5432;Database=tipos_documentos_db;Username=tipo_doc_user;Password=tipo_doc_pass_secure_2025"
```

### 4. Tornar Scripts Executáveis

```bash
chmod +x scripts/*.sh
```

### 5. Subir Serviços com Docker Compose

```bash
docker-compose up -d
```

Isso iniciará todos os serviços:
- PostgreSQL (porta 5432)
- Redis (porta 6379)
- Keycloak (porta 8080)
- Ollama com GPU (porta 11434)
- Weaviate (porta 8081)

### 6. Aguardar Inicialização dos Serviços

```bash
# Verificar status dos containers
docker-compose ps

# Acompanhar logs de um serviço específico
docker-compose logs -f ollama    # Para ver download dos modelos IA
docker-compose logs -f keycloak  # Para ver importação do realm
docker-compose logs -f postgres  # Para ver criação dos databases
```

**Importante:** O download dos modelos do Ollama pode levar alguns minutos na primeira execução (2-5GB de dados).

### 7. Aplicar Migrations do Entity Framework

```bash
# Criar migration inicial
dotnet ef migrations add InitialCreate

# Aplicar migrations no banco
dotnet ef database update
```

### 8. Executar a Aplicação

```bash
dotnet run
```

A aplicação estará disponível em:
- **API**: https://localhost:7001
- **Swagger UI**: https://localhost:7001/swagger
- **Keycloak Admin**: http://localhost:8080 (admin/admin_keycloak_pass_2025)

---

## 📁 Estrutura do Projeto

```
NetRedisASide3/
├── Models/                      # Modelos de domínio
│   ├── Assunto.cs
│   ├── Movimentacao.cs
│   └── TipoDocumento.cs
├── Data/                        # Contexto do banco de dados
│   └── AppDbContext.cs
├── Repositories/                # Camada de acesso a dados
│   ├── IRepository.cs
│   ├── AssuntoRepository.cs
│   ├── MovimentacaoRepository.cs
│   └── TipoDocumentoRepository.cs
├── Services/                    # Lógica de negócio com cache
│   ├── AssuntoService.cs
│   ├── MovimentacaoService.cs
│   └── TipoDocumentoService.cs
├── Validators/                  # Validação de entrada
│   ├── AssuntoValidator.cs
│   ├── MovimentacaoValidator.cs
│   └── TipoDocumentoValidator.cs
├── Endpoints/                   # Minimal APIs
│   ├── AssuntoEndpoints.cs
│   ├── MovimentacaoEndpoints.cs
│   └── TipoDocumentoEndpoints.cs
├── Configuration/               # Configurações
│   └── KeycloakSettings.cs
├── scripts/                     # Scripts de inicialização
│   ├── create-databases.sh
│   ├── import-keycloak-realm.sh
│   └── download-ollama-models.sh
├── keycloak/                    # Configuração do Keycloak
│   └── realm-export.json
├── postman/                     # Collection para testes
│   └── NetRedisASide3.postman_collection.json
├── docker-compose.yml           # Orquestração de containers
├── Dockerfile                   # Build da aplicação
├── .env.example                 # Template de variáveis de ambiente
├── appsettings.json
├── appsettings.Development.json
├── NetRedisASide3.csproj
├── Program.cs                   # Ponto de entrada da aplicação
└── README.md
```

---

## ⚙️ Configuração

### Variáveis de Ambiente (.env)

As principais variáveis configuráveis:

| Variável | Descrição | Valor Padrão |
|----------|-----------|--------------|
| `POSTGRES_ADMIN_USER` | Usuário admin do PostgreSQL | postgres |
| `POSTGRES_ADMIN_PASSWORD` | Senha do admin | postgres_admin_pass_2025 |
| `REDIS_PASSWORD` | Senha do Redis | redis_secure_pass_2025 |
| `KEYCLOAK_ADMIN` | Usuário admin do Keycloak | admin |
| `KEYCLOAK_ADMIN_PASSWORD` | Senha do Keycloak | admin_keycloak_pass_2025 |
| `KEYCLOAK_CLIENT_SECRET` | Secret do cliente OAuth | netredisaside3-secret... |

### User Secrets (Desenvolvimento)

Connection strings são armazenadas em **User Secrets** para maior segurança:

```bash
# Listar secrets configurados
dotnet user-secrets list

# Remover um secret
dotnet user-secrets remove "ConnectionStrings:AssuntoDb"

# Limpar todos os secrets
dotnet user-secrets clear
```

### Keycloak - Usuários Padrão

O realm vem com 2 usuários pré-configurados:

| Usuário | Senha | Roles |
|---------|-------|-------|
| admin | admin123 | api_admin, api_user |
| user | user123 | api_user |

**⚠️ IMPORTANTE:** Altere essas senhas em produção!

---

## 🎯 Uso

### Autenticação

Todas as APIs (exceto `/health` e `/`) exigem autenticação via **Bearer Token JWT**.

#### Obter Token via Postman ou cURL:

```bash
curl -X POST http://localhost:8080/realms/netredisaside3/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=netredisaside3-api" \
  -d "client_secret=netredisaside3-secret-change-in-production" \
  -d "username=admin" \
  -d "password=admin123"
```

Resposta:
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expires_in": 300,
  "refresh_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer"
}
```

Use o `access_token` no header `Authorization: Bearer {token}` nas requisições.

### Endpoints Disponíveis

#### **Assuntos**

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/assuntos` | Lista todos os assuntos |
| GET | `/api/assuntos/{id}` | Busca por ID |
| POST | `/api/assuntos` | Cria novo assunto |
| PUT | `/api/assuntos/{id}` | Atualiza assunto |
| DELETE | `/api/assuntos/{id}` | Remove assunto |

**Exemplo de criação:**
```bash
curl -X POST https://localhost:7001/api/assuntos \
  -H "Authorization: Bearer {seu-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Arquitetura de Software",
    "descricao": "Padrões e práticas de arquitetura empresarial"
  }'
```

#### **Movimentações**

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/movimentacoes` | Lista todas as movimentações |
| GET | `/api/movimentacoes/{id}` | Busca por ID |
| POST | `/api/movimentacoes` | Cria nova movimentação |
| PUT | `/api/movimentacoes/{id}` | Atualiza movimentação |
| DELETE | `/api/movimentacoes/{id}` | Remove movimentação |

#### **Tipos de Documento**

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/tipos-documento` | Lista todos os tipos |
| GET | `/api/tipos-documento/{id}` | Busca por ID |
| POST | `/api/tipos-documento` | Cria novo tipo |
| PUT | `/api/tipos-documento/{id}` | Atualiza tipo |
| DELETE | `/api/tipos-documento/{id}` | Remove tipo |

#### **Health Checks**

| Endpoint | Descrição |
|----------|-----------|
| `/health` | Status completo de todos os serviços |
| `/health/ready` | Verifica se app está pronta (DB + Cache) |
| `/health/live` | Verifica se app está viva |

**Exemplo:**
```bash
curl https://localhost:7001/health
```

---

## 🧪 Testes com Postman

### Importar Collection

1. Abra o Postman
2. Clique em **Import**
3. Selecione o arquivo `postman/NetRedisASide3.postman_collection.json`

### Configurar Variáveis

A collection já vem com variáveis pré-configuradas:
- `base_url`: https://localhost:7001
- `keycloak_url`: http://localhost:8080
- `client_id`: netredisaside3-api
- `client_secret`: netredisaside3-secret-change-in-production

### Workflow de Teste

1. **Obter Token**: Execute a requisição `Authentication > Get Access Token`
   - O token será automaticamente salvo na variável `access_token`
   
2. **Testar Endpoints**: Todas as outras requisições usarão automaticamente o token

3. **Health Checks**: Verifique o status dos serviços em `Health Checks`

### Exemplo de Fluxo Completo

```
1. Authentication > Get Access Token (Password Grant)
2. Assuntos > Create Assunto
3. Assuntos > List All Assuntos (verifica cache)
4. Assuntos > Get Assunto By ID
5. Assuntos > Update Assunto
6. Assuntos > List All Assuntos (verifica invalidação do cache)
7. Assuntos > Delete Assunto
```

---

## 🔒 Segurança

### Boas Práticas Implementadas

✅ **Autenticação e Autorização**
- OAuth 2.0 / OpenID Connect via Keycloak
- Tokens JWT com validade de 5 minutos
- Refresh tokens para renovação

✅ **Proteção de Dados**
- User Secrets para credenciais em desenvolvimento
- Variáveis de ambiente (.env) para configuração
- Secrets nunca commitados no repositório

✅ **Proteção contra Vulnerabilidades**
- SQL Injection: EF Core com queries parametrizadas
- XSS: Validação de entrada com FluentValidation
- CSRF: Tokens JWT stateless
- CORS: Configurado para origens específicas

✅ **Monitoramento**
- Health checks de todos os serviços
- Logs estruturados
- Auditoria de operações

### Checklist de Segurança para Produção

- [ ] Alterar todas as senhas padrão no `.env`
- [ ] Alterar `KEYCLOAK_CLIENT_SECRET`
- [ ] Habilitar HTTPS obrigatório (`RequireHttpsMetadata: true`)
- [ ] Configurar certificados SSL/TLS válidos
- [ ] Ativar rate limiting por IP/usuário
- [ ] Configurar backup automático dos bancos de dados
- [ ] Implementar rotação de secrets
- [ ] Ativar logs de auditoria
- [ ] Configurar firewall e regras de rede
- [ ] Desabilitar usuários de teste (admin/user)

---

## 🔍 Cache Distribuído (Redis)

### Estratégia Cache-Aside

A aplicação implementa o padrão **Cache-Aside (Lazy Loading)**:

1. **Leitura:**
   - Verifica se existe no cache
   - Se não existir, busca no banco
   - Salva no cache para próximas leituras

2. **Escrita:**
   - Cria/Atualiza no banco
   - Invalida o cache relacionado

3. **TTL (Time-To-Live):**
   - Padrão: 5 minutos
   - Configurável em `Services/*.Service.cs`

### Chaves de Cache

```
Formato: {entidade}:{id} ou {entidade}s:all

Exemplos:
- assunto:1
- assuntos:all
- movimentacao:5
- movimentacoes:all
- tipodocumento:3
- tiposdocumento:all
```

### Verificar Cache no Redis

```bash
# Acessar Redis CLI
docker exec -it redis redis-cli -a redis_secure_pass_2025

# Listar todas as chaves
KEYS NetRedisASide3:*

# Ver valor de uma chave
GET NetRedisASide3:assuntos:all

# Verificar TTL
TTL NetRedisASide3:assunto:1

# Invalidar manualmente
DEL NetRedisASide3:assuntos:all
```

---

## 🤖 Integração com IA (Ollama)

### Modelos Disponíveis

Após a inicialização, os seguintes modelos estarão disponíveis:

| Modelo | Tamanho | Uso |
|--------|---------|-----|
| llama2 | ~3.8GB | Geração de texto, chat |
| all-minilm | ~23MB | Embeddings leves e rápidos |
| mxbai-embed-large | ~670MB | Embeddings de alta qualidade |

### Testar Ollama

```bash
# Listar modelos instalados
curl http://localhost:11434/api/tags

# Gerar texto com llama2
curl http://localhost:11434/api/generate -d '{
  "model": "llama2",
  "prompt": "Explique Clean Architecture em 3 parágrafos",
  "stream": false
}'

# Gerar embedding
curl http://localhost:11434/api/embeddings -d '{
  "model": "all-minilm",
  "prompt": "Arquitetura de software empresarial"
}'
```

### Usar em C#

```csharp
using System.Net.Http;
using System.Text.Json;

var client = new HttpClient();
var response = await client.PostAsync(
    "http://localhost:11434/api/generate",
    new StringContent(JsonSerializer.Serialize(new {
        model = "llama2",
        prompt = "O que é DDD?",
        stream = false
    }), Encoding.UTF8, "application/json")
);

var result = await response.Content.ReadAsStringAsync();
```

---

## 🗄️ Weaviate (Banco Vetorial)

### Verificar Status

```bash
curl http://localhost:8081/v1/.well-known/ready
```

### Criar Schema Exemplo

```bash
curl -X POST http://localhost:8081/v1/schema \
  -H "Content-Type: application/json" \
  -d '{
    "class": "Documento",
    "vectorizer": "text2vec-ollama",
    "moduleConfig": {
      "text2vec-ollama": {
        "model": "all-minilm",
        "apiEndpoint": "http://ollama:11434"
      }
    },
    "properties": [
      {
        "name": "titulo",
        "dataType": ["string"]
      },
      {
        "name": "conteudo",
        "dataType": ["text"]
      }
    ]
  }'
```

### Inserir Documento

```bash
curl -X POST http://localhost:8081/v1/objects \
  -H "Content-Type: application/json" \
  -d '{
    "class": "Documento",
    "properties": {
      "titulo": "Clean Architecture",
      "conteudo": "Clean Architecture é um padrão que separa as responsabilidades..."
    }
  }'
```

### Busca Semântica

```bash
curl -X POST http://localhost:8081/v1/graphql \
  -H "Content-Type: application/json" \
  -d '{
    "query": "{
      Get {
        Documento(
          nearText: {
            concepts: [\"arquitetura de software\"]
          }
          limit: 5
        ) {
          titulo
          conteudo
          _additional {
            distance
          }
        }
      }
    }"
  }'
```

---

## 🐛 Troubleshooting

### Problema: Ollama não baixa modelos

**Sintoma:** Container `ollama` reinicia continuamente

**Solução:**
```bash
# Verificar logs
docker logs ollama

# Baixar modelos manualmente
docker exec -it ollama bash
ollama pull llama2
ollama pull all-minilm
ollama pull mxbai-embed-large
```

### Problema: Keycloak não importa realm

**Sintoma:** Realm `netredisaside3` não aparece no Keycloak

**Solução:**
```bash
# Parar containers
docker-compose down

# Remover volumes
docker volume rm netredisaside3_keycloak_data

# Recriar
docker-compose up -d keycloak
```

### Problema: Erro de conexão com PostgreSQL

**Sintoma:** `Npgsql.PostgresException: Connection refused`

**Solução:**
```bash
# Verificar se o container está rodando
docker ps | grep postgres

# Verificar logs
docker logs postgres

# Verificar se databases foram criados
docker exec -it postgres psql -U postgres -c "\l"

# Recriar databases
docker-compose restart postgres
```

### Problema: GPU não detectada no Ollama

**Sintoma:** Ollama roda em CPU mesmo com GPU disponível

**Solução:**
```bash
# Verificar NVIDIA Docker
docker run --rm --gpus all nvidia/cuda:11.8.0-base-ubuntu22.04 nvidia-smi

# Se falhar, instalar nvidia-container-toolkit
distribution=$(. /etc/os-release;echo $ID$VERSION_ID)
curl -s -L https://nvidia.github.io/nvidia-docker/gpgkey | sudo apt-key add -
curl -s -L https://nvidia.github.io/nvidia-docker/$distribution/nvidia-docker.list | sudo tee /etc/apt/sources.list.d/nvidia-docker.list

sudo apt-get update && sudo apt-get install -y nvidia-container-toolkit
sudo systemctl restart docker
```

### Problema: Erro 401 Unauthorized nas APIs

**Sintoma:** Todas as requisições retornam 401

**Solução:**
```bash
# Verificar se Keycloak está rodando
curl http://localhost:8080/realms/netredisaside3/.well-known/openid-configuration

# Obter novo token
curl -X POST http://localhost:8080/realms/netredisaside3/protocol/openid-connect/token \
  -d "grant_type=password" \
  -d "client_id=netredisaside3-api" \
  -d "client_secret=netredisaside3-secret-change-in-production" \
  -d "username=admin" \
  -d "password=admin123"

# Verificar se o token está válido (não expirou)
# Tokens JWT expiram em 5 minutos por padrão
```

### Problema: Cache não está funcionando

**Sintoma:** Todas as requisições vão ao banco

**Solução:**
```bash
# Verificar se Redis está rodando
docker ps | grep redis

# Testar conexão com Redis
docker exec -it redis redis-cli -a redis_secure_pass_2025 PING

# Verificar logs da aplicação
dotnet run | grep -i "cache"

# Limpar cache
docker exec -it redis redis-cli -a redis_secure_pass_2025 FLUSHALL
```

---

## 📊 Performance

### Benchmarks Esperados

| Operação | Com Cache | Sem Cache |
|----------|-----------|-----------|
| GET (item único) | < 10ms | < 50ms |
| GET (lista) | < 15ms | < 100ms |
| POST | N/A | < 100ms |
| PUT | N/A | < 100ms |
| DELETE | N/A | < 50ms |

### Otimizações Implementadas

✅ Async/Await em todas operações I/O  
✅ Connection pooling (EF Core)  
✅ Redis cache distribuído  
✅ Queries otimizadas (AsNoTracking)  
✅ Índices no banco de dados  
✅ DTOs para reduzir payload  

---

## 🤝 Contribuição

Contribuições são bem-vindas! Por favor:

1. Faça fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

---

## 📝 Licença

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes.

---

## 📧 Suporte

- **Issues**: [GitHub Issues](https://github.com/seu-usuario/netredisaside3/issues)
- **Discussions**: [GitHub Discussions](https://github.com/seu-usuario/netredisaside3/discussions)
- **Email**: dev@netredisaside3.com

---

**Desenvolvido com ❤️ usando .NET 9, Redis, PostgreSQL, Keycloak, Ollama e Weaviate**