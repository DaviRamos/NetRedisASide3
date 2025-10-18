#!/bin/bash
set -e

# Script para criar múltiplos databases no PostgreSQL com usuários individuais
# Cada database terá suas próprias credenciais de acesso

echo "===================================="
echo "Criando databases e usuários..."
echo "===================================="

# Função para criar database e usuário
create_database() {
    local db_name=$1
    local db_user=$2
    local db_password=$3
    
    echo "Criando database: $db_name"
    
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
        -- Criar usuário se não existir
        DO \$\$
        BEGIN
            IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = '$db_user') THEN
                CREATE USER $db_user WITH PASSWORD '$db_password';
            END IF;
        END
        \$\$;
        
        -- Criar database se não existir
        SELECT 'CREATE DATABASE $db_name OWNER $db_user'
        WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = '$db_name')\gexec
        
        -- Conceder privilégios
        GRANT ALL PRIVILEGES ON DATABASE $db_name TO $db_user;
        
        -- Conectar ao database e configurar schema
        \c $db_name
        
        GRANT ALL ON SCHEMA public TO $db_user;
        ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO $db_user;
        ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON SEQUENCES TO $db_user;
EOSQL
    
    echo "✓ Database $db_name criado com sucesso!"
    echo ""
}

# Criar database para Assuntos
if [ -n "$ASSUNTO_DB_NAME" ] && [ -n "$ASSUNTO_DB_USER" ] && [ -n "$ASSUNTO_DB_PASSWORD" ]; then
    create_database "$ASSUNTO_DB_NAME" "$ASSUNTO_DB_USER" "$ASSUNTO_DB_PASSWORD"
fi

# Criar database para Movimentações
if [ -n "$MOVIMENTACAO_DB_NAME" ] && [ -n "$MOVIMENTACAO_DB_USER" ] && [ -n "$MOVIMENTACAO_DB_PASSWORD" ]; then
    create_database "$MOVIMENTACAO_DB_NAME" "$MOVIMENTACAO_DB_USER" "$MOVIMENTACAO_DB_PASSWORD"
fi

# Criar database para Tipos de Documento
if [ -n "$TIPODOCUMENTO_DB_NAME" ] && [ -n "$TIPODOCUMENTO_DB_USER" ] && [ -n "$TIPODOCUMENTO_DB_PASSWORD" ]; then
    create_database "$TIPODOCUMENTO_DB_NAME" "$TIPODOCUMENTO_DB_USER" "$TIPODOCUMENTO_DB_PASSWORD"
fi

# Criar database para Keycloak
echo "Criando database para Keycloak..."
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    SELECT 'CREATE DATABASE keycloak'
    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'keycloak')\gexec
    
    GRANT ALL PRIVILEGES ON DATABASE keycloak TO $POSTGRES_USER;
EOSQL
echo "✓ Database keycloak criado com sucesso!"
echo ""

echo "===================================="
echo "Todos os databases foram criados!"
echo "===================================="