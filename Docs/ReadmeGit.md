# Comandos Úteis - NetRedisASide3

## 🚀 Início Rápido

```bash
# Clone e configure
git clone https://github.com/seu-usuario/NetRedisASide3.git
cd NetRedisASide3
cp .env.example .env
chmod +x scripts/*.sh

# Configure secrets
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:AssuntoDb" "Host=localhost;Port=5432;Database=assuntos_db;Username=assunto_user;Password=assunto_pass_secure_2025"

# Suba os serviços
docker-compose up -d

# Execute a aplicação
dotnet run
```

---

## 🐳 Docker & Docker Compose

```bash
# Subir todos os serviços
docker-compose up -d

# Subir com rebuild
docker-compose up -d --build

# Ver logs de todos os serviços
docker-compose logs -f

# Ver logs de um serviço específico
docker-compose logs -f postgres
docker-compose logs -f redis
docker-compose logs -f keycloak
docker-compose logs -f ollama
docker-compose logs -f weaviate

# Parar todos os serviços
docker-compose stop

# Parar e remover containers
docker-compose down

# Parar e remover containers + volumes (⚠️ APAGA DADOS)
docker-compose down -v

# Reiniciar um serviço específico
docker-compose restart postgres

# Ver status dos containers
docker-compose ps

# Executar comando em container
docker-compose exec postgres psql -U postgres
docker-compose exec redis redis-cli -a redis_secure_pass_2025
```

---

## 🗄️ PostgreSQL

```bash
# Conectar ao PostgreSQL
docker exec -it postgres psql -U postgres

# Listar databases
docker exec -it postgres psql -U postgres -c "\l"

# Conectar a um database específico
docker exec -it postgres psql -U postgres -d assuntos_db

# Ver tabelas
docker exec -it postgres psql -U postgres -d assuntos_db -c "\dt"

# Executar query
docker exec -it postgres psql -U postgres -d assuntos_db -c "SELECT * FROM assuntos;"

# Backup de um database
docker exec -it postgres pg_dump -U postgres assuntos_db > backup_assuntos.sql

# Restore de um backup
docker exec -i postgres psql -U postgres assuntos_db < backup_assuntos.sql

# Ver conexões ativas
docker exec -it postgres psql -U postgres -c "SELECT * FROM pg_stat_activity;"

# Matar conexões de um database
docker exec -it postgres psql -U postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = 'assuntos_db';"
```

---

## 🔴 Redis

```bash
# Conectar ao Redis
docker exec -it redis redis-cli -a redis_secure_pass_2025

# Listar todas as chaves
docker exec -it redis redis-cli -a redis_secure_pass_2025 KEYS "NetRedisASide3:*"

# Ver valor de uma chave
docker exec -it redis redis-cli -a redis_secure_pass_2025 GET "NetRedisASide3:assuntos:all"

# Ver TTL de uma chave
docker exec -it redis redis-cli -a redis_secure_pass_2025 TTL "NetRedisASide3:assunto:1"

# Deletar uma chave
docker exec -it redis redis-cli -a redis_secure_pass_2025 DEL "NetRedisASide3:assuntos:all"

# Limpar todas as chaves (⚠️ CUIDADO)
docker exec -it redis redis-cli -a redis_secure_pass_2025 FLUSHALL

# Ver informações do Redis
docker exec -it redis redis-cli -a redis_secure_pass_2025 INFO

# Monitorar comandos em tempo real
docker exec -it redis redis-cli -a redis_secure_pass_2025 MONITOR

# Ver memória usada
docker exec -it redis redis-cli -a redis_secure_pass_2025 MEMORY STATS
```

---

## 🔐 Keycloak

```bash
# Acessar admin console
# http://localhost:8080
# Usuário: admin
# Senha: admin_keycloak_pass_2025

# Exportar realm
docker exec -it keycloak /opt/keycloak/bin/kc.sh export --dir /tmp --realm netredisaside3

# Importar realm
docker exec -it keycloak /opt/keycloak/bin/kc.sh import --dir /opt/keycloak/data/import

# Ver logs
docker logs -f keycloak

# Obter token via cURL
curl -X POST http://localhost:8080/realms/netredisaside3/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=netredisaside3-api" \
  -d "client_secret=netredisaside3-secret-change-in-production" \
  -d "username=admin" \
  -d "password=admin123"

# Validar token
curl -X POST http://localhost:8080/realms/netredisaside3/protocol/openid-connect/token/introspect \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=netredisaside3-api" \
  -d "client_secret=netredisaside3-secret-change-in-production" \
  -d "token=SEU_TOKEN_AQUI"
```

---

## 🤖 Ollama

```bash
# Listar modelos instalados
docker exec -it ollama ollama list

# Baixar um modelo
docker exec -it ollama ollama pull llama2

# Remover um modelo
docker exec -it ollama ollama rm llama2

# Testar geração de texto
curl http://localhost:11434/api/generate -d '{
  "model": "llama2",
  "prompt": "O que é Clean Architecture?",
  "stream": false
}'

# Testar embeddings
curl http://localhost:11434/api/embeddings -d '{
  "model": "all-minilm",
  "prompt": "Arquitetura de software"
}'

# Ver tags disponíveis
curl http://localhost:11434/api/tags

# Ver informações de um modelo
docker exec -it ollama ollama show llama2

# Executar chat interativo
docker exec -it ollama ollama run llama2
```

---

## 🧠 Weaviate

```bash
# Verificar status
curl http://localhost:8081/v1/.well-known/ready

# Verificar health
curl http://localhost:8081/v1/.well-known/live

# Ver schema atual
curl http://localhost:8081/v1/schema

# Criar classe
curl -X POST http://localhost:8081/v1/schema \
  -H "Content-Type: application/json" \
  -d '{
    "class": "Documento",
    "vectorizer": "text2vec-ollama"
  }'

# Deletar classe
curl -X DELETE http://localhost:8081/v1/schema/Documento

# Ver meta informações
curl http://localhost:8081/v1/meta
```

---

## 🔧 .NET & Entity Framework

```bash
# Restaurar dependências
dotnet restore

# Compilar projeto
dotnet build

# Executar aplicação
dotnet run

# Executar com hot reload
dotnet watch run

# Executar testes
dotnet test

# Limpar build
dotnet clean

# Adicionar pacote NuGet
dotnet add package NomeDoPacote

# Listar pacotes instalados
dotnet list package

# Verificar atualizações de pacotes
dotnet list package --outdated

# Entity Framework - Criar migration
dotnet ef migrations add NomeDaMigration

# EF - Aplicar migrations
dotnet ef database update

# EF - Reverter migration
dotnet ef database update PreviousMigrationName

# EF - Remover última migration
dotnet ef migrations remove

# EF - Listar migrations
dotnet ef migrations list

# EF - Gerar script SQL
dotnet ef migrations script

# EF - Ver conexões
dotnet ef dbcontext info

# EF - Recriar banco (⚠️ APAGA DADOS)
dotnet ef database drop --force
dotnet ef database update

# User Secrets - Inicializar
dotnet user-secrets init

# User Secrets - Adicionar
dotnet user-secrets set "Key" "Value"

# User Secrets - Listar
dotnet user-secrets list

# User Secrets - Remover
dotnet user-secrets remove "Key"

# User Secrets - Limpar todos
dotnet user-secrets clear
```

---

## 📊 Monitoramento e Debug

```bash
# Ver uso de memória dos containers
docker stats

# Ver logs da aplicação .NET
dotnet run | tee app.log

# Ver apenas erros
dotnet run 2>&1 | grep -i error

# Verificar portas em uso
netstat -tuln | grep LISTEN

# Testar conectividade com serviços
curl -k https://localhost:7001/health
curl http://localhost:8080/health/ready
curl http://localhost:11434/api/tags
curl http://localhost:8081/v1/.well-known/ready

# Verificar processos .NET rodando
ps aux | grep dotnet

# Kill processo .NET (se necessário)
pkill -f dotnet

# Ver uso de disco
df -h

# Limpar logs do Docker
docker system prune -a --volumes
```

---

## 🧪 Testes com cURL

### Health Checks

```bash
# Health geral
curl -k https://localhost:7001/health | jq

# Health ready
curl -k https://localhost:7001/health/ready | jq

# Health live
curl -k https://localhost:7001/health/live | jq

# Root endpoint
curl -k https://localhost:7001/ | jq
```

### Autenticação

```bash
# Obter token e salvar em variável
export TOKEN=$(curl -X POST http://localhost:8080/realms/netredisaside3/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=netredisaside3-api" \
  -d "client_secret=netredisaside3-secret-change-in-production" \
  -d "username=admin" \
  -d "password=admin123" \
  | jq -r '.access_token')

echo $TOKEN
```

### CRUD - Assuntos

```bash
# Listar todos (usando token)
curl -k https://localhost:7001/api/assuntos \
  -H "Authorization: Bearer $TOKEN" | jq

# Buscar por ID
curl -k https://localhost:7001/api/assuntos/1 \
  -H "Authorization: Bearer $TOKEN" | jq

# Criar novo
curl -k -X POST https://localhost:7001/api/assuntos \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Arquitetura de Software",
    "descricao": "Padrões e práticas de arquitetura empresarial"
  }' | jq

# Atualizar
curl -k -X PUT https://localhost:7001/api/assuntos/1 \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "nome": "Arquitetura de Software Atualizado",
    "descricao": "Nova descrição"
  }' | jq

# Deletar
curl -k -X DELETE https://localhost:7001/api/assuntos/1 \
  -H "Authorization: Bearer $TOKEN"
```

### CRUD - Movimentações

```bash
# Criar movimentação
curl -k -X POST https://localhost:7001/api/movimentacoes \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Transferência Interna",
    "descricao": "Movimentação de recursos entre departamentos"
  }' | jq

# Listar todas
curl -k https://localhost:7001/api/movimentacoes \
  -H "Authorization: Bearer $TOKEN" | jq
```

### CRUD - Tipos de Documento

```bash
# Criar tipo de documento
curl -k -X POST https://localhost:7001/api/tipos-documento \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Nota Fiscal Eletrônica",
    "descricao": "Documento fiscal eletrônico modelo 55 (NF-e)"
  }' | jq

# Listar todos
curl -k https://localhost:7001/api/tipos-documento \
  -H "Authorization: Bearer $TOKEN" | jq
```

---

## 🔍 Debug e Troubleshooting

### Verificar conectividade entre containers

```bash
# Ping entre containers
docker exec postgres ping -c 3 redis
docker exec keycloak ping -c 3 postgres
docker exec ollama ping -c 3 weaviate

# Verificar DNS interno
docker exec postgres nslookup redis
docker exec redis nslookup postgres
```

### Ver configuração da aplicação

```bash
# Ver variáveis de ambiente
dotnet run --environment=Development | grep -i connection

# Ver configuração do appsettings
cat appsettings.json | jq
cat appsettings.Development.json | jq

# Ver user secrets
dotnet user-secrets list
```

### Limpar e recomeçar

```bash
# Parar tudo
docker-compose down -v
docker system prune -a --volumes -f

# Limpar build .NET
dotnet clean
rm -rf bin/ obj/

# Recriar tudo
docker-compose up -d
dotnet restore
dotnet build
dotnet ef database update
dotnet run
```

---

## 📦 Build e Deploy

### Build com Docker

```bash
# Build da imagem
docker build -t netredisaside3:latest .

# Build com tag específica
docker build -t netredisaside3:1.0.0 .

# Executar container
docker run -d \
  --name netredisaside3-app \
  -p 8080:8080 \
  --network netredisaside3-network \
  -e ASPNETCORE_ENVIRONMENT=Production \
  netredisaside3:latest

# Ver logs do container
docker logs -f netredisaside3-app
```

### Publish

```bash
# Publish para produção
dotnet publish -c Release -o ./publish

# Publish para Windows
dotnet publish -c Release -r win-x64 --self-contained true -o ./publish/win

# Publish para Linux
dotnet publish -c Release -r linux-x64 --self-contained true -o ./publish/linux

# Executar aplicação publicada
cd publish
./NetRedisASide3
```

---

## 🧹 Manutenção

### Backup completo

```bash
# Criar diretório de backup
mkdir -p backups/$(date +%Y%m%d)

# Backup PostgreSQL
docker exec postgres pg_dump -U postgres assuntos_db > backups/$(date +%Y%m%d)/assuntos_db.sql
docker exec postgres pg_dump -U postgres movimentacoes_db > backups/$(date +%Y%m%d)/movimentacoes_db.sql
docker exec postgres pg_dump -U postgres tipos_documentos_db > backups/$(date +%Y%m%d)/tipos_documentos_db.sql

# Backup Redis (RDB)
docker exec redis redis-cli -a redis_secure_pass_2025 BGSAVE
docker cp redis:/data/dump.rdb backups/$(date +%Y%m%d)/redis_dump.rdb

# Backup Keycloak realm
docker exec keycloak /opt/keycloak/bin/kc.sh export \
  --dir /tmp \
  --realm netredisaside3
docker cp keycloak:/tmp/netredisaside3-realm.json backups/$(date +%Y%m%d)/

# Compactar backups
tar -czf backups/backup_$(date +%Y%m%d_%H%M%S).tar.gz backups/$(date +%Y%m%d)
```

### Restore de backup

```bash
# Restore PostgreSQL
docker exec -i postgres psql -U postgres assuntos_db < backups/20250116/assuntos_db.sql

# Restore Redis
docker cp backups/20250116/redis_dump.rdb redis:/data/dump.rdb
docker-compose restart redis
```

### Limpeza de recursos

```bash
# Remover imagens não utilizadas
docker image prune -a

# Remover volumes órfãos
docker volume prune

# Remover networks não utilizadas
docker network prune

# Limpeza completa (⚠️ CUIDADO)
docker system prune -a --volumes
```

---

## 📈 Performance

### Medir tempo de resposta

```bash
# Com curl
time curl -k https://localhost:7001/api/assuntos \
  -H "Authorization: Bearer $TOKEN"

# Com Apache Bench (100 requests, 10 concurrent)
ab -n 100 -c 10 \
  -H "Authorization: Bearer $TOKEN" \
  https://localhost:7001/api/assuntos

# Com wrk (1 minuto, 10 threads, 100 conexões)
wrk -t10 -c100 -d60s \
  -H "Authorization: Bearer $TOKEN" \
  https://localhost:7001/api/assuntos
```

### Monitorar recursos

```bash
# Uso de CPU e memória
docker stats --no-stream

# Uso contínuo
docker stats

# Apenas um container
docker stats postgres

# Ver processos dentro do container
docker top postgres
```

---

## 🔐 Segurança

### Scan de vulnerabilidades

```bash
# Scan da imagem Docker
docker scan netredisaside3:latest

# Trivy scan
trivy image netredisaside3:latest

# Scan de dependências .NET
dotnet list package --vulnerable
```

### Atualizar dependências

```bash
# Atualizar todos os pacotes
dotnet outdated -u

# Atualizar pacote específico
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 9.0.1
```

---

## 📝 Logs

### Coletar logs

```bash
# Logs de todos os containers
docker-compose logs > logs_completos.txt

# Logs da aplicação
dotnet run > app.log 2>&1

# Logs apenas de erros
dotnet run 2>&1 | grep -i "error\|exception\|fail" > errors.log

# Logs com timestamp
docker-compose logs --timestamps > logs_com_timestamp.txt
```

### Análise de logs

```bash
# Contar erros
grep -i error logs_completos.txt | wc -l

# Ver top 10 erros
grep -i error logs_completos.txt | sort | uniq -c | sort -rn | head -10

# Ver logs de um período
docker-compose logs --since "2025-01-16T10:00:00" --until "2025-01-16T12:00:00"
```

---

## 🎯 Shortcuts Úteis

```bash
# Aliases úteis (adicionar ao ~/.bashrc ou ~/.zshrc)
alias dc='docker-compose'
alias dcu='docker-compose up -d'
alias dcd='docker-compose down'
alias dcl='docker-compose logs -f'
alias dps='docker ps'
alias dr='dotnet run'
alias db='dotnet build'
alias dt='dotnet test'
alias dw='dotnet watch run'

# Usar
dc up -d
dcl postgres
```

---

## 📚 Recursos Adicionais

### Documentação Oficial

- [.NET 9 Docs](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-9)
- [EF Core](https://learn.microsoft.com/ef/core/)
- [PostgreSQL](https://www.postgresql.org/docs/)
- [Redis](https://redis.io/docs/)
- [Keycloak](https://www.keycloak.org/documentation)
- [Ollama](https://github.com/ollama/ollama)
- [Weaviate](https://weaviate.io/developers/weaviate)

### Ferramentas Recomendadas

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Postman](https://www.postman.com/)
- [DBeaver](https://dbeaver.io/) - Client SQL universal
- [RedisInsight](https://redis.io/insight/) - GUI para Redis
- [JetBrains Rider](https://www.jetbrains.com/rider/) ou [VS Code](https://code.visualstudio.com/)

---

**💡 Dica:** Adicione este arquivo aos favoritos para consulta rápida!