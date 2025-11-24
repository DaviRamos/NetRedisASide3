# NetRedisASide3 - Setup Manual (Passo a Passo)

## 📋 Comandos para Linux/MacOS (Bash)

### 1. Criar Estrutura de Pastas

```bash
# Criar pasta raiz e navegar
mkdir -p NetRedisASide3
cd NetRedisASide3

# Criar todas as pastas de uma vez
mkdir -p Models Data Repositories Services Validators Endpoints Configuration scripts keycloak postman wwwroot

echo "✅ Estrutura de pastas criada!"
```

### 2. Criar Projeto .NET 9

```bash
# Criar novo projeto Web API
dotnet new web -n NetRedisASide3 -f net9.0

# Inicializar Git
git init
git add .
git commit -m "Initial commit"

echo "✅ Projeto .NET 9 criado!"
```

### 3. Instalar Pacotes NuGet

```bash
# Entity Framework Core + PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore --version 9.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.0
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 9.0.0

# Redis
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis --version 9.0.0
dotnet add package StackExchange.Redis --version 2.8.16

# Authentication
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 9.0.0

# Validation
dotnet add package FluentValidation --version 11.10.0
dotnet add package FluentValidation.DependencyInjectionExtensions --version 11.10.0

# Health Checks
dotnet add package AspNetCore.HealthChecks.Redis --version 9.0.1
dotnet add package AspNetCore.HealthChecks.Npgsql --version 9.0.1
dotnet add package AspNetCore.HealthChecks.Uris --version 9.0.1

# Swagger
dotnet add package Swashbuckle.AspNetCore --version 7.2.0
dotnet add package Microsoft.AspNetCore.OpenApi --version 9.0.0

echo "✅ Pacotes NuGet instalados!"
```

### 4. Criar Arquivo .gitignore

```bash
cat > .gitignore << 'EOF'
## .NET
bin/
obj/
*.user
*.suo
*.cache
*.dll
*.exe
*.pdb

## Visual Studio
.vs/
.vscode/
*.csproj.user

## Rider
.idea/

## Environment
.env
*.env.local
.env.production

## User Secrets
secrets.json

## Backups
backups/
*.bak

## OS
.DS_Store
Thumbs.db

## Logs
logs/
*.log
EOF

echo "✅ Arquivo .gitignore criado!"
```

### 5. Criar Arquivo .env.example

```bash
cat > .env.example << 'EOF'
# PostgreSQL
POSTGRES_ADMIN_USER=postgres
POSTGRES_ADMIN_PASSWORD=postgres_admin_pass_2025

# Databases
ASSUNTO_DB_NAME=assuntos_db
ASSUNTO_DB_USER=assunto_user
ASSUNTO_DB_PASSWORD=assunto_pass_secure_2025

MOVIMENTACAO_DB_NAME=movimentacoes_db
MOVIMENTACAO_DB_USER=movimentacao_user
MOVIMENTACAO_DB_PASSWORD=movimentacao_pass_secure_2025

TIPODOCUMENTO_DB_NAME=tipos_documentos_db
TIPODOCUMENTO_DB_USER=tipo_doc_user
TIPODOCUMENTO_DB_PASSWORD=tipo_doc_pass_secure_2025

# Redis
REDIS_PASSWORD=redis_secure_pass_2025

# Keycloak
KEYCLOAK_ADMIN=admin
KEYCLOAK_ADMIN_PASSWORD=admin_keycloak_pass_2025
KEYCLOAK_CLIENT_SECRET=your-client-secret-here-change-me
EOF

echo "✅ Arquivo .env.example criado!"
```

### 6. Configurar User Secrets

```bash
# Inicializar User Secrets
dotnet user-secrets init

# Adicionar connection strings
dotnet user-secrets set "ConnectionStrings:AssuntoDb" "Host=localhost;Port=5432;Database=assuntos_db;Username=assunto_user;Password=assunto_pass_secure_2025"

dotnet user-secrets set "ConnectionStrings:MovimentacaoDb" "Host=localhost;Port=5432;Database=movimentacoes_db;Username=movimentacao_user;Password=movimentacao_pass_secure_2025"

dotnet user-secrets set "ConnectionStrings:TipoDocumentoDb" "Host=localhost;Port=5432;Database=tipos_documentos_db;Username=tipo_doc_user;Password=tipo_doc_pass_secure_2025"

# Verificar secrets
dotnet user-secrets list

echo "✅ User Secrets configurados!"
```

---

## 📝 Próximos Passos (Todos os SO)

### 7. Copiar Arquivos de Código

Agora você precisa criar os arquivos de código nas pastas apropriadas. Use os artifacts fornecidos anteriormente:

```bash
# Estrutura esperada:
NetRedisASide3/
├── Models/
│   ├── Assunto.cs
│   ├── Movimentacao.cs
│   └── TipoDocumento.cs
├── Data/
│   └── AppDbContext.cs
├── Repositories/
│   ├── IRepository.cs
│   ├── AssuntoRepository.cs
│   ├── MovimentacaoRepository.cs
│   └── TipoDocumentoRepository.cs
├── Services/
│   ├── AssuntoService.cs
│   ├── MovimentacaoService.cs
│   └── TipoDocumentoService.cs
├── Validators/
│   ├── AssuntoValidator.cs
│   ├── MovimentacaoValidator.cs
│   └── TipoDocumentoValidator.cs
├── Endpoints/
│   ├── AssuntoEndpoints.cs
│   ├── MovimentacaoEndpoints.cs
│   └── TipoDocumentoEndpoints.cs
├── Configuration/
│   └── KeycloakSettings.cs
└── Program.cs
```

### 8. Criar Arquivos de Infraestrutura

**docker-compose.yml** (na raiz do projeto)
**Dockerfile** (na raiz do projeto)
**scripts/create-databases.sh**
**scripts/download-ollama-models.sh**
**keycloak/realm-export.json**
**postman/NetRedisASide3.postman_collection.json**

### 9. Preparar Ambiente

```bash
# Copiar .env
cp .env.example .env

# Editar .env com suas credenciais (opcional)
nano .env  # ou vim .env, ou code .env

# Tornar scripts executáveis (Linux/Mac)
chmod +x scripts/*.sh
```

### 10. Subir Infraestrutura

```bash
# Subir todos os serviços Docker
docker-compose up -d

# Verificar status
docker-compose ps

# Ver logs
docker-compose logs -f
```

### 11. Aplicar Migrations

```bash
# Criar migration inicial
dotnet ef migrations add InitialCreate

# Aplicar no banco
dotnet ef database update

# Verificar
dotnet ef migrations list
```

### 12. Executar Aplicação

```bash
# Executar em modo desenvolvimento
dotnet run

# Ou com hot reload
dotnet watch run
```

### 13. Testar

Abra o navegador:
- **Swagger**: https://localhost:7001/swagger
- **Health Check**: https://localhost:7001/health
- **Keycloak**: http://localhost:8080

---

## 🔍 Verificação Rápida

```bash
# Verificar estrutura criada
tree -L 2 -I 'bin|obj'

# Verificar pacotes instalados
dotnet list package

# Verificar User Secrets
dotnet user-secrets list

# Verificar containers Docker
docker-compose ps

# Verificar bancos criados
docker exec -it postgres psql -U postgres -c "\l"

# Verificar cache Redis
docker exec -it redis redis-cli -a redis_secure_pass_2025 PING
```

---

## 🆘 Solução de Problemas

### Erro: "dotnet command not found"
```bash
# Instalar .NET 9 SDK
# https://dotnet.microsoft.com/download/dotnet/9.0
```

### Erro: "docker-compose command not found"
```bash
# Instalar Docker Desktop
# https://www.docker.com/products/docker-desktop
```

### Erro: Porta já em uso
```bash
# Verificar portas em uso
netstat -tuln | grep LISTEN  # Linux/Mac
netstat -ano | findstr LISTENING  # Windows

# Parar containers conflitantes
docker ps
docker stop <container_id>
```

### Erro: Migrations não aplicam
```bash
# Limpar e recriar
dotnet ef database drop --force
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Erro: User Secrets não funcionam
```bash
# Verificar se projeto tem UserSecretsId
cat NetRedisASide3.csproj | grep UserSecretsId

# Se não tiver, adicionar manualmente
dotnet user-secrets init

# Listar para confirmar
dotnet user-secrets list
```

---

## 🎯 Checklist Completo

Marque cada item conforme completar:

- [x] ✅ Estrutura de pastas criada
- [x] ✅ Projeto .NET 9 criado
- [x] ✅ Pacotes NuGet instalados
- [x] ✅ .gitignore criado
- [x] ✅ .env.example criado
- [x] ✅ User Secrets configurados
- [X] ✅ Arquivos de código copiados (Models, Repositories, etc.)
- [X] ✅ Program.cs criado
- [X] ✅ docker-compose.yml criado
- [X] ✅ Dockerfile criado
- [X] ✅ Scripts shell criados
- [X] ✅ realm-export.json criado
- [X] ✅ Collection Postman criada
- [X] ✅ .env configurado
- [X] ✅ Docker Compose rodando
- [X] ✅ Migrations aplicadas
- [X] ✅ Aplicação executando
- [x] ✅ Swagger acessível
- [ ] ✅ Health checks passando
- [ ] ✅ Postman testado

---

## 📚 Comandos Úteis Pós-Setup

### Desenvolvimento Diário

```bash
# Iniciar ambiente
docker-compose up -d
dotnet watch run

# Parar ambiente
docker-compose down
```

### Gerenciar Migrations

```bash
# Criar nova migration
dotnet ef migrations add NomeDaMigration

# Aplicar migrations
dotnet ef database update

# Reverter para migration anterior
dotnet ef database update PreviousMigrationName

# Remover última migration
dotnet ef migrations remove

# Gerar script SQL
dotnet ef migrations script
```

### Gerenciar Docker

```bash
# Ver logs de um serviço
docker-compose logs -f postgres
docker-compose logs -f redis
docker-compose logs -f keycloak
docker-compose logs -f ollama

# Reiniciar um serviço
docker-compose restart postgres

# Rebuild e reiniciar
docker-compose up -d --build

# Limpar tudo
docker-compose down -v
docker system prune -a --volumes
```

### Testes

```bash
# Executar testes (quando criar)
dotnet test

# Com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Executar teste específico
dotnet test --filter FullyQualifiedName~AssuntoTests
```

### Build e Publish

```bash
# Build Release
dotnet build -c Release

# Publish
dotnet publish -c Release -o ./publish

# Criar imagem Docker
docker build -t netredisaside3:latest .

# Executar container
docker run -d -p 8080:8080 netredisaside3:latest
```

---

## 📖 Referências Rápidas

### Estrutura Final Esperada

```
NetRedisASide3/
├── Configuration/
│   └── KeycloakSettings.cs
├── Data/
│   └── AppDbContext.cs
├── Endpoints/
│   ├── AssuntoEndpoints.cs
│   ├── MovimentacaoEndpoints.cs
│   └── TipoDocumentoEndpoints.cs
├── keycloak/
│   └── realm-export.json
├── Models/
│   ├── Assunto.cs
│   ├── Movimentacao.cs
│   └── TipoDocumento.cs
├── postman/
│   └── NetRedisASide3.postman_collection.json
├── Repositories/
│   ├── AssuntoRepository.cs
│   ├── IRepository.cs
│   ├── MovimentacaoRepository.cs
│   └── TipoDocumentoRepository.cs
├── scripts/
│   ├── create-databases.sh
│   └── download-ollama-models.sh
├── Services/
│   ├── AssuntoService.cs
│   ├── MovimentacaoService.cs
│   └── TipoDocumentoService.cs
├── Validators/
│   ├── AssuntoValidator.cs
│   ├── MovimentacaoValidator.cs
│   └── TipoDocumentoValidator.cs
├── wwwroot/
├── .env
├── .env.example
├── .gitignore
├── docker-compose.yml
├── Dockerfile
├── NetRedisASide3.csproj
├── Program.cs
└── README.md
```

### Portas Utilizadas

| Serviço | Porta | URL |
|---------|-------|-----|
| API .NET | 7001 (HTTPS) / 5001 (HTTP) | https://localhost:7001 |
| PostgreSQL | 5432 | localhost:5432 |
| Redis | 6379 | localhost:6379 |
| Keycloak | 8080 | http://localhost:8080 |
| Ollama | 11434 | http://localhost:11434 |
| Weaviate | 8081 | http://localhost:8081 |

### Credenciais Padrão (Desenvolvimento)

**PostgreSQL:**
- User: `postgres`
- Password: `postgres_admin_pass_2025`

**Redis:**
- Password: `redis_secure_pass_2025`

**Keycloak:**
- Admin User: `admin`
- Admin Password: `admin_keycloak_pass_2025`
- Test User: `admin` / `admin123`

**⚠️ IMPORTANTE:** Altere todas as senhas em produção!

---

## 🚀 Setup Automatizado (Alternativa)

Se preferir, use o script automatizado:

```bash
# Baixar e executar
curl -O https://raw.githubusercontent.com/seu-usuario/NetRedisASide3/main/SETUP.sh
chmod +x SETUP.sh
./SETUP.sh
```

Ou para Windows PowerShell:

```powershell
# Baixar e executar
Invoke-WebRequest -Uri "https://raw.githubusercontent.com/seu-usuario/NetRedisASide3/main/SETUP.ps1" -OutFile "SETUP.ps1"
.\SETUP.ps1
```

---

## 💡 Dicas Finais

1. **Use o VS Code ou Rider** para melhor experiência de desenvolvimento
2. **Instale extensões úteis:**
   - C# Dev Kit
   - Docker
   - GitLens
   - REST Client
3. **Configure o debugger** no launch.json
4. **Use o Swagger** para testar APIs rapidamente
5. **Monitore os logs** com `docker-compose logs -f`
6. **Faça commits frequentes** durante o desenvolvimento
7. **Documente mudanças** no código

---

## 📞 Suporte

Se encontrar problemas:

1. Verifique os logs: `docker-compose logs`
2. Verifique o status: `docker-compose ps`
3. Consulte a documentação completa nos artifacts
4. Abra uma issue no GitHub

---

**✅ Pronto! Você agora tem um guia completo para setup manual do projeto NetRedisASide3.**