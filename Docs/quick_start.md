# NetRedisASide3 - Quick Start Guide 🚀

## ⚡ Início em 5 Minutos

### Opção 1: Script Automatizado (Recomendado)

#### Linux/MacOS:
```bash
curl -O https://raw.githubusercontent.com/seu-usuario/NetRedisASide3/main/SETUP.sh
chmod +x SETUP.sh
./SETUP.sh
```

#### Windows PowerShell:
```powershell
Invoke-WebRequest -Uri "https://github.com/seu-usuario/NetRedisASide3/raw/main/SETUP.ps1" -OutFile "SETUP.ps1"
.\SETUP.ps1
```

### Opção 2: Manual Rápido

```bash
# 1. Criar estrutura
mkdir -p NetRedisASide3/{Models,Data,Repositories,Services,Validators,Endpoints,Configuration,scripts,keycloak,postman,wwwroot}
cd NetRedisASide3

# 2. Criar projeto
dotnet new web -n NetRedisASide3 -f net9.0

# 3. Instalar pacotes essenciais
dotnet add package Microsoft.EntityFrameworkCore --version 9.0.0
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 9.0.0
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis --version 9.0.0
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 9.0.0
dotnet add package FluentValidation.DependencyInjectionExtensions --version 11.10.0

# 4. Configurar secrets
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:AssuntoDb" "Host=localhost;Port=5432;Database=assuntos_db;Username=assunto_user;Password=assunto_pass"
```

---

## 📝 Checklist Mínimo Viável

Para ter a aplicação rodando, você precisa **no mínimo**:

### Arquivos Essenciais:

- [ ] `NetRedisASide3.csproj` (gerado automaticamente)
- [ ] `Program.cs` ⭐ **ESSENCIAL**
- [ ] `docker-compose.yml` ⭐ **ESSENCIAL**
- [ ] `.env` (cópia do `.env.example`)
- [ ] Models: `Assunto.cs`, `Movimentacao.cs`, `TipoDocumento.cs`
- [ ] Data: `AppDbContext.cs`
- [ ] Pelo menos 1 Repository, Service, Validator e Endpoint

### Comandos de Inicialização:

```bash
# 1. Subir infraestrutura
docker-compose up -d

# 2. Aguardar serviços (30-60 segundos)
docker-compose ps

# 3. Aplicar migrations
dotnet ef migrations add InitialCreate
dotnet ef database update

# 4. Rodar aplicação
dotnet run
```

---

## 🎯 Ordem de Criação Recomendada

### Fase 1: Estrutura Básica (5 min)
```bash
1. ✅ Criar pastas
2. ✅ Criar projeto .NET
3. ✅ Instalar pacotes
4. ✅ Configurar .env
```

### Fase 2: Modelos e Banco (10 min)
```bash
5. ✅ Criar Models (Assunto.cs, etc)
6. ✅ Criar AppDbContext.cs
7. ✅ Criar docker-compose.yml
8. ✅ Subir PostgreSQL
9. ✅ Aplicar migrations
```

### Fase 3: Lógica de Negócio (15 min)
```bash
10. ✅ Criar Repositories
11. ✅ Criar Services (com cache)
12. ✅ Criar Validators
13. ✅ Subir Redis
```

### Fase 4: APIs (10 min)
```bash
14. ✅ Criar Endpoints
15. ✅ Criar Program.cs
16. ✅ Testar com dotnet run
17. ✅ Acessar Swagger
```

### Fase 5: Autenticação (15 min)
```bash
18. ✅ Criar realm-export.json
19. ✅ Subir Keycloak
20. ✅ Configurar authentication
21. ✅ Testar com token
```

### Fase 6: IA (Opcional - 20 min)
```bash
22. ✅ Subir Ollama
23. ✅ Baixar modelos
24. ✅ Subir Weaviate
25. ✅ Testar integração
```

---

## 🔥 Comandos Ultra-Rápidos

### Teste Rápido (sem Docker)
```bash
# Apenas a aplicação .NET (sem BD, sem cache)
dotnet run --launch-profile http
```

### Build Rápido
```bash
dotnet build -c Release --no-restore
```

### Limpar e Rebuild
```bash
dotnet clean
dotnet restore
dotnet build
```

---

## 📦 Pacotes Mínimos vs Completos

### Mínimo (MVP):
```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Swashbuckle.AspNetCore
```

### Completo (Recomendado):
Use o script `SETUP.sh` ou `SETUP.ps1` que instala tudo.

---

## 🐳 Docker Compose Mínimo

Se quiser começar apenas com PostgreSQL:

```yaml
version: '3.8'
services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
```

---

## ⚡ Atalhos Úteis

### Criar tudo de uma vez (após ter os arquivos):
```bash
# Bash one-liner
docker-compose up -d && dotnet ef database update && dotnet run
```

### Recomeçar do zero:
```bash
docker-compose down -v
dotnet ef database drop --force
dotnet ef migrations remove
rm -rf bin/ obj/
dotnet restore && dotnet build
```

---

## 🎓 Fluxo de Aprendizado

### Dia 1: Básico
- Criar projeto
- Entender Models e DbContext
- Rodar migrations
- Testar CRUD básico

### Dia 2: Intermediário
- Adicionar Repositories
- Implementar Services
- Adicionar Validators
- Testar APIs com Swagger

### Dia 3: Avançado
- Adicionar Cache (Redis)
- Implementar Authentication (Keycloak)
- Health Checks
- Postman Collection

### Dia 4: Expert
- Integrar Ollama (IA)
- Integrar Weaviate (Busca Semântica)
- Implementar features avançadas
- Deploy

---

## 🆘 Troubleshooting Rápido

### Erro: "Porta 5432 já em uso"
```bash
# Parar PostgreSQL local
sudo systemctl stop postgresql  # Linux
brew services stop postgresql   # Mac
```

### Erro: "Não consigo conectar ao banco"
```bash
# Verificar se PostgreSQL está rodando
docker-compose ps
docker logs postgres

# Testar conexão
docker exec -it postgres psql -U postgres -c "SELECT 1"
```

### Erro: "Migration falha"
```bash
# Limpar e recriar
dotnet ef database drop --force
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Erro: "Pacote não encontrado"
```bash
# Limpar cache NuGet
dotnet nuget locals all --clear
dotnet restore --force
```

---

## 📊 Métricas de Sucesso

Você saberá que está tudo funcionando quando:

✅ `docker-compose ps` mostra todos os containers **healthy**  
✅ `dotnet run` inicia sem erros  
✅ https://localhost:7001/swagger abre o Swagger UI  
✅ https://localhost:7001/health retorna status **Healthy**  
✅ Consegue criar um registro via Swagger  
✅ Postman consegue obter token do Keycloak  

---

## 🎯 Metas por Tempo

### 15 Minutos:
- ✅ Projeto criado
- ✅ Pacotes instalados
- ✅ PostgreSQL rodando
- ✅ Aplicação compilando

### 30 Minutos:
- ✅ Models criados
- ✅ DbContext configurado
- ✅ Migrations aplicadas
- ✅ CRUD básico funcionando

### 1 Hora:
- ✅ Repositories implementados
- ✅ Services com cache
- ✅ Validators funcionando
- ✅ APIs testadas no Swagger

### 2 Horas:
- ✅ Authentication configurada
- ✅ Keycloak integrado
- ✅ Health checks ativos
- ✅ Postman collection funcionando

### 3 Horas:
- ✅ Ollama rodando
- ✅ Weaviate configurado
- ✅ Integração IA testada
- ✅ Projeto completo funcionando

---

## 💻 Comandos para Diferentes IDEs

### Visual Studio Code
```bash
# Abrir projeto
code .

# Debug
# Pressione F5 ou use "Run and Debug"
```

### JetBrains Rider
```bash
# Abrir projeto
rider NetRedisASide3.sln

# Debug
# Clique no botão de debug ou Shift+F10
```

### Visual Studio 2022
```bash
# Abrir solução
start NetRedisASide3.sln

# Debug
# F5 ou Ctrl+F5 (sem debug)
```

---

## 🔗 Links Rápidos

| Recurso | URL | Descrição |
|---------|-----|-----------|
| API Local | https://localhost:7001 | Endpoint principal |
| Swagger | https://localhost:7001/swagger | Documentação interativa |
| Health | https://localhost:7001/health | Status dos serviços |
| Keycloak | http://localhost:8080 | Identity Provider |
| PostgreSQL | localhost:5432 | Banco de dados |
| Redis | localhost:6379 | Cache distribuído |
| Ollama | http://localhost:11434 | Modelos de IA |
| Weaviate | http://localhost:8081 | Banco vetorial |

---

## 📚 Documentação Completa

Para mais detalhes, consulte:

1. **README.md** - Visão geral e documentação completa
2. **MANUAL_SETUP.md** - Comandos passo a passo
3. **COMMANDS.md** - Comandos úteis do dia a dia
4. **EXAMPLES.md** - Exemplos de código avançado
5. **PRODUCTION.md** - Guia de deploy em produção

---

## 🎁 Bônus: Aliases Úteis

Adicione ao seu `.bashrc` ou `.zshrc` (Linux/Mac):

```bash
# NetRedisASide3 Aliases
alias nra-up='docker-compose up -d'
alias nra-down='docker-compose down'
alias nra-logs='docker-compose logs -f'
alias nra-run='dotnet run'
alias nra-watch='dotnet watch run'
alias nra-test='dotnet test'
alias nra-build='dotnet build'
alias nra-clean='dotnet clean && rm -rf bin/ obj/'
alias nra-migrate='dotnet ef migrations add'
alias nra-update='dotnet ef database update'
alias nra-status='docker-compose ps && dotnet --version'
```

Para PowerShell (Windows), adicione ao seu `$PROFILE`:

```powershell
# NetRedisASide3 Aliases
function nra-up { docker-compose up -d }
function nra-down { docker-compose down }
function nra-logs { docker-compose logs -f }
function nra-run { dotnet run }
function nra-watch { dotnet watch run }
function nra-test { dotnet test }
function nra-build { dotnet build }
function nra-clean { dotnet clean; Remove-Item -Recurse -Force bin/, obj/ }
function nra-status { docker-compose ps; dotnet --version }
```

Depois basta executar:
```bash
nra-up      # Sobe infraestrutura
nra-run     # Executa aplicação
nra-logs    # Ver logs
```

---

## 🌟 Dica Final

**Não tente fazer tudo de uma vez!**

Siga a ordem recomendada:
1. Estrutura básica primeiro
2. Banco de dados funcionando
3. CRUD básico
4. Cache e autenticação
5. Features avançadas (IA, etc)

Cada fase deve estar **100% funcional** antes de prosseguir para a próxima.

---

## 📞 Precisa de Ajuda?

1. ✅ Verifique os logs: `docker-compose logs`
2. ✅ Consulte TROUBLESHOOTING.md
3. ✅ Revise os exemplos em EXAMPLES.md
4. ✅ Teste com a collection do Postman
5. ✅ Abra uma issue no GitHub

---

## 🎉 Pronto para Começar?

Escolha seu método:

**Rápido:** Execute `./SETUP.sh` (Linux/Mac) ou `.\SETUP.ps1` (Windows)

**Manual:** Siga o passo a passo em `MANUAL_SETUP.md`

**Intermediário:** Use este guia Quick Start

---

**Boa sorte! 🚀**

*Desenvolvido com ❤️ usando .NET 9*