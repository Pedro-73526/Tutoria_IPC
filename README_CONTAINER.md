# Guia de Containerização e Configuração

Este guia explica como containerizar a aplicação `TutoringApp` e configurar as variáveis de ambiente no servidor de destino.

## 1. Pré-requisitos

Certifique-se de que o **Docker** e o **Docker Compose** estão instalados no servidor de destino.

## 2. Estrutura dos Ficheiros

Foram criados dois ficheiros na raiz do projeto:
- `Dockerfile`: Define como a imagem da aplicação é construída.
- `docker-compose.yml`: Define como o contentor é executado e injeta as configurações.

## 3. Como Construir e Executar

No servidor de destino (ou na sua máquina para testar), navegue até à pasta onde estão estes ficheiros e execute:

```bash
docker-compose up -d --build
```

Isto irá:
1. Construir a imagem da aplicação.
2. Iniciar o contentor em background.
3. Mapear a porta 80 do contentor para a porta 8080 do servidor (pode alterar isto no `docker-compose.yml`).

A aplicação ficará acessível em `http://localhost:8080` (ou no IP do servidor).

## 4. Como Configurar no Servidor de Destino

O .NET Core permite substituir as definições do `appsettings.json` através de variáveis de ambiente.

No ficheiro `docker-compose.yml`, existe uma secção `environment`. É aqui que pode alterar as configurações sem precisar de recompilar a aplicação.

### Exemplo de Configuração

Abra o ficheiro `docker-compose.yml` no servidor e edite os valores:

```yaml
environment:
  # Para chaves aninhadas (como ConnectionStrings), use duplo underscore (__)
  - ConnectionStrings__DefaultConnection=Server=NOVO_SERVIDOR;Database=...
  
  # Para chaves simples
  - WS_GetDocente=https://novo-url.com/api/
  - Quovadis_Password=nova_password
```

Depois de alterar o ficheiro, reinicie o contentor para aplicar as mudanças:

```bash
docker-compose up -d
```

## Resumo das Variáveis

| Variável de Ambiente | Chave no appsettings.json |
|----------------------|---------------------------|
| `ConnectionStrings__DefaultConnection` | `ConnectionStrings:DefaultConnection` |
| `ConnectionStrings__TutoringAppContextConnection` | `ConnectionStrings:TutoringAppContextConnection` |
| `WS_GetDocente` | `WS_GetDocente` |
| ... | ... |

Desta forma, quem gere o servidor de destino tem total controlo sobre as configurações apenas editando o ficheiro de texto `docker-compose.yml`.

## 5. Como Publicar no Docker Hub

Para levar a aplicação para outro servidor sem precisar de copiar o código fonte, pode usar o Docker Hub.

### Passo 1: Criar Repositório
Crie uma conta e um repositório no [Docker Hub](https://hub.docker.com/) (ex: `meu-utilizador/tutoringapp`).

### Passo 2: Login e Build
No seu computador (onde tem o código):

```bash
# 1. Login no Docker Hub
docker login

# 2. Construir a imagem com o nome do repositório
docker build -t SEU_UTILIZADOR/tutoringapp:latest -f Dockerfile .

# 3. Enviar para o Docker Hub
docker push SEU_UTILIZADOR/tutoringapp:latest
```

### Passo 3: Usar no Servidor de Destino
No servidor de destino, **não precisa do Dockerfile nem do código fonte**. Precisa apenas do `docker-compose.yml` com uma pequena alteração.

Substitua a secção `build` pela imagem:

```yaml
version: '3.4'
services:
  tutoringapp:
    image: SEU_UTILIZADOR/tutoringapp:latest  # <--- Alterado aqui
    # build: ... (remover a secção build)
    ports:
      - "8080:80"
    environment:
      - ConnectionStrings__DefaultConnection=...
      # ... (restante configuração igual)
```

Depois, no servidor, basta correr:
```bash
docker-compose up -d
```
O Docker irá descarregar automaticamente a imagem do Docker Hub e iniciar a aplicação com as configurações definidas.
