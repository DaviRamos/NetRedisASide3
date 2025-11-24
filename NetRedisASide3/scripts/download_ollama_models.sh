#!/bin/bash
set -e

echo "===================================="
echo "Todos os modelos foram baixados!"
echo "===================================="

# Listar modelos instalados
echo ""
echo "Modelos disponíveis:"
ollama list

exit 0===================================="
echo "Baixando modelos do Ollama..."
echo "===================================="

# Aguardar o serviço Ollama estar pronto
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

# Função para baixar modelo
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

# Baixar modelos
download_model "llama2"
download_model "all-minilm"
download_model "mxbai-embed-large"

echo "