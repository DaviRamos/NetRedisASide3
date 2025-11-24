#!/bin/bash

# Script de setup completo do projeto NetRedisASide3
# Execute: chmod +x SETUP.sh && ./SETUP.sh

set -e

echo "=========================================="
echo "🚀 NetRedisASide3 - Setup Completo"
echo "=========================================="
echo ""

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Função para imprimir mensagens coloridas
print_success() {
    echo -e "${GREEN}✓${NC} $1"
}

print_error() {
    echo -e "${RED}✗${NC} $1"
}

print_info() {
    echo -e "${BLUE}ℹ${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}⚠${NC} $1"
}

# Verificar pré-requisitos
echo "Verificando pré-requisitos..."
echo ""

check_command() {
    if command -v $1 &> /dev/null; then
        print_success "$1 instalado"
        return 0
    else
        print_error "$1 não encontrado"
        return 1
    fi
}

MISSING_DEPS=0

check_command "dotnet" || MISSING_DEPS=1
check_command "docker" || MISSING_DEPS=1
check_command "docker-compose" || MISSING_DEPS=1
check_command "git" || MISSING_DEPS=1

echo ""

if [ $MISSING_DEPS -eq 1 ]; then
    print_error "Alguns pré-requisitos estão faltando. Por favor, instale-os antes de continuar."
    exit 1
fi

# Verificar versão do .NET
DOTNET_VERSION=$(dotnet --version | cut -d'.' -f1)
if [ "$DOTNET_VERSION" -lt "9" ]; then
    print_warning ".NET 9 é recomendado. Versão atual: $(dotnet --version)"
else
    print_success ".NET 9+ detectado"
fi

echo ""
echo "=========================================="
echo "📁 Criando estrutura de pastas..."
echo "=========================================="
echo ""

# Nome do projeto
PROJECT_NAME="NetRedisASide3"

# Criar diretório principal se não existir
if [ ! -d "$PROJECT_NAME" ]; then
    mkdir "$PROJECT_NAME"
    print_success "Pasta raiz criada: $PROJECT_NAME"
else
    print_info "Pasta $PROJECT_NAME já existe"
fi

cd "$PROJECT_NAME"

# Criar estrutura de diretórios
echo ""
print_info "Criando estrutura de diretórios..."

mkdir -p Models
mkdir -p Data
mkdir -p Repositories
mkdir -p Services
mkdir -p Validators
mkdir -p Endpoints
mkdir -p Configuration
mkdir -p scripts
mkdir -p keycloak
mkdir -p postman
mkdir -p wwwroot

print_success "Estrutura de diretórios criada"

echo ""
echo "=========================================="
echo "🔧 Criando projeto .NET 9..."
echo "=========================================="
echo ""

# Criar projeto .NET se não existir
if [ ! -f "$PROJECT_NAME.csproj" ]; then
    dotnet new web -n "$PROJECT_NAME" -f net9.0
    rm Program.cs # Vamos criar um customizado
    print_success "Projeto .NET 9 criado"
else
    print_info "Arquivo .csproj já existe"
fi

echo ""
echo "=========================================="
echo "📦 Instalando pacotes NuGet..."
echo "=========================================="
echo ""

# Entity Framework Core
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

print_success "Pacotes NuGet instalados"

echo ""
echo "=========================================="
echo "📝 Criando arquivos de configuração..."
echo "=========================================="
echo ""

# Criar .env.example
cat > .env.example << 'EOF'
# PostgreSQL Admin Credentials
POSTGRES_ADMIN_USER=postgres
POSTGRES_ADMIN_PASSWORD=postgres_admin_pass_2025

# Database específico: Assuntos
ASSUNTO_DB_NAME=assuntos_db
ASSUNTO_DB_USER=assunto_user
ASSUNTO_DB_PASSWORD=assunto_pass_secure_2025

# Database específico: Movimentações
MOVIMENTACAO_DB_NAME=movimentacoes_db
MOVIMENTACAO_DB_USER=movimentacao_user
MOVIMENTACAO_DB_PASSWORD=movimentacao_pass_secure_2025

# Database específico: Tipos de Documento
TIPODOCUMENTO_DB_NAME=tipos_documentos_db
TIPODOCUMENTO_DB_USER=tipo_doc_user
TIPODOCUMENTO_DB_PASSWORD=tipo_doc_pass_secure_2025

# Redis
REDIS_PASSWORD=redis_secure_pass_2025

# Keycloak
KEYCLOAK_ADMIN=admin
KEYCLOAK_ADMIN_PASSWORD=admin_keycloak_pass_2025
KEYCLOAK_REALM=netredisaside3
KEYCLOAK_CLIENT_ID=netredisaside3-api
KEYCLOAK_CLIENT_SECRET=your-client-secret-here-change-me

# Ollama
OLLAMA_HOST=ollama
OLLAMA_PORT=11434

# Weaviate
WEAVIATE_HOST=weaviate
WEAVIATE_PORT=8080
EOF

print_success "Arquivo .env.example criado"

# Criar .gitignore
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
*.userprefs
*.pidb
*.vspscc
*_i.c
*_p.c
*.ilk
*.meta
*.obj
*.pch
*.pgc
*.pgd
*.rsp
*.sbr
*.tlb
*.tli
*.tlh
*.tmp
*.log
*.vspscc
*.vssscc
.builds

## Visual Studio
.vs/
.vscode/
*.csproj.user
*.dbmdl

## Rider
.idea/
*.sln.iml

## Docker
.dockerignore

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

## Publish
publish/
EOF

print_success "Arquivo .gitignore criado"

# Criar README.md básico
cat > README.md << 'EOF'
# NetRedisASide3

Sistema de Gestão Enterprise com .NET 9

## Início Rápido

```bash
# 1. Configurar variáveis de ambiente
cp .env.example .env

# 2. Configurar User Secrets
./scripts/setup-secrets.sh

# 3. Tornar scripts executáveis
chmod +x scripts/*.sh

# 4. Subir infraestrutura
docker-compose up -d

# 5. Aplicar migrations
dotnet ef database update

# 6. Executar aplicação
dotnet run
```

## Acesso

- **API**: https://localhost:7001
- **Swagger**: https://localhost:7001/swagger
- **Keycloak**: http://localhost:8080

Veja a documentação completa nos arquivos gerados.
EOF

print_success "README.md criado"

echo ""
echo "=========================================="
echo "🔨 Criando scripts auxiliares..."
echo "=========================================="
echo ""

# Script de setup de secrets
cat > scripts/setup-secrets.sh << 'EOF'
#!/bin/bash

echo "🔐 Configurando User Secrets..."

dotnet user-secrets init

dotnet user-secrets set "ConnectionStrings:AssuntoDb" "Host=localhost;Port=5432;Database=assuntos_db;Username=assunto_user;Password=assunto_pass_secure_2025"

dotnet user-secrets set "ConnectionStrings:MovimentacaoDb" "Host=localhost;Port=5432;Database=movimentacoes_db;Username=movimentacao_user;Password=movimentacao_pass_secure_2025"

dotnet user-secrets set "ConnectionStrings:TipoDocumentoDb" "Host=localhost;Port=5432;Database=tipos_documentos_db;Username=tipo_doc_user;Password=tipo_doc_pass_secure_2025"

echo "✅ User Secrets configurados!"
echo ""
echo "Para visualizar: dotnet user-secrets list"
EOF

chmod +x scripts/setup-secrets.sh
print_success "Script setup-secrets.sh criado"

# Script de criação de databases
cat > scripts/create-databases.sh << 'EOF'
#!/bin/bash
set -e

echo "===================================="
echo "Criando databases e usuários..."
echo "===================================="

create_database() {
    local db_name=$1
    local db_user=$2
    local db_password=$3
    
    echo "Criando database: $db_name"
    
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
        DO \$\$
        BEGIN
            IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = '$db_user') THEN
                CREATE USER $db_user WITH PASSWORD '$db_password';
            END IF;
        END
        \$\$;
        
        SELECT 'CREATE DATABASE $db_name OWNER $db_user'
        WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = '$db_name')\gexec
        
        GRANT ALL PRIVILEGES ON DATABASE $db_name TO $db_user;
        
        \c $db_name
        
        GRANT ALL ON SCHEMA public TO $db_user;
        ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO $db_user;
        ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO $db_user;
EOSQL
    
    echo "✓ Database $db_name criado com sucesso!"
    echo ""
}

if [ -n "$ASSUNTO_DB_NAME" ]; then
    create_database "$ASSUNTO_DB_NAME" "$ASSUNTO_DB_USER" "$ASSUNTO_DB_PASSWORD"
fi

if [ -n "$MOVIMENTACAO_DB_NAME" ]; then
    create_database "$MOVIMENTACAO_DB_NAME" "$MOVIMENTACAO_DB_USER" "$MOVIMENTACAO_DB_PASSWORD"
fi

if [ -n "$TIPODOCUMENTO_DB_NAME" ]; then
    create_database "$TIPODOCUMENTO_DB_NAME" "$TIPODOCUMENTO_DB_USER" "$TIPODOCUMENTO_DB_PASSWORD"
fi

echo "Criando database para Keycloak..."
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    SELECT 'CREATE DATABASE keycloak'
    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'keycloak')\gexec
    
    GRANT ALL PRIVILEGES ON DATABASE keycloak TO $POSTGRES_USER;
EOSQL

echo "===================================="
echo "Todos os databases foram criados!"
echo "===================================="
EOF

chmod +x scripts/create-databases.sh
print_success "Script create-databases.sh criado"

# Script de download de modelos Ollama
cat > scripts/download-ollama-models.sh << 'EOF'
#!/bin/bash
set -e

echo "===================================="
echo "Baixando modelos do Ollama..."
echo "===================================="

echo "Aguardando Ollama iniciar..."
max_attempts=30
attempt=0

while ! curl -s http://localhost:11434/api/tags > /dev/null 2>&1; do
    attempt=$((attempt + 1))
    if [ $attempt -eq $max_attempts ]; then
        echo "❌ Timeout aguardando Ollama iniciar"
        exit 1
    fi
    echo "Tentativa $attempt/$max_attempts..."
    sleep 10
done

echo "✓ Ollama está pronto!"
echo ""

download_model() {
    local model_name=$1
    echo "Baixando modelo: $model_name"
    ollama pull $model_name
    if [ $? -eq 0 ]; then
        echo "✓ Modelo $model_name baixado com sucesso!"
    else
        echo "❌ Erro ao baixar modelo $model_name"
        return 1
    fi
    echo ""
}

download_model "llama2"
download_model "all-minilm"
download_model "mxbai-embed-large"

echo "===================================="
echo "Todos os modelos foram baixados!"
echo "===================================="

echo ""
echo "Modelos disponíveis:"
ollama list

exit 0
EOF

chmod +x scripts/download-ollama-models.sh
print_success "Script download-ollama-models.sh criado"

echo ""
echo "=========================================="
echo "✅ Setup Concluído!"
echo "=========================================="
echo ""
print_success "Estrutura de pastas criada"
print_success "Projeto .NET 9 configurado"
print_success "Pacotes NuGet instalados"
print_success "Scripts auxiliares criados"
echo ""
echo "=========================================="
echo "📋 Próximos Passos:"
echo "=========================================="
echo ""
echo "1. ${BLUE}Copiar arquivos de código${NC}"
echo "   - Copie os arquivos .cs para as respectivas pastas"
echo "   - Models/, Repositories/, Services/, etc."
echo ""
echo "2. ${BLUE}Configurar ambiente${NC}"
echo "   cd $PROJECT_NAME"
echo "   cp .env.example .env"
echo "   ./scripts/setup-secrets.sh"
echo ""
echo "3. ${BLUE}Criar arquivos restantes${NC}"
echo "   - docker-compose.yml"
echo "   - Dockerfile"
echo "   - keycloak/realm-export.json"
echo "   - postman/NetRedisASide3.postman_collection.json"
echo ""
echo "4. ${BLUE}Iniciar desenvolvimento${NC}"
echo "   docker-compose up -d"
echo "   dotnet ef migrations add InitialCreate"
echo "   dotnet ef database update"
echo "   dotnet run"
echo ""
echo "=========================================="
echo "📁 Estrutura Criada:"
echo "=========================================="
echo ""
tree -L 2 -a

echo ""
print_success "Setup completo! 🎉"
echo ""