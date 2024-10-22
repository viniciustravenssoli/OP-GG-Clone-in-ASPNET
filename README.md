# OP.GG Clone - Simplified Version

Este projeto é um clone simplificado do OP.GG, desenvolvido em ASP.NET. Ele permite que os usuários pesquisem informações sobre jogadores, visualizem estatísticas de partidas e acompanhem seu desempenho. O objetivo é fornecer uma plataforma básica de análise para jogadores de games competitivos, similar ao OP.GG.

## 📋 Índice

- [Tecnologias](#-tecnologias)
- [Funcionalidades](#-funcionalidades)
- [Pré-requisitos](#-pré-requisitos)
- [Instalação](#-instalação)
- [Uso](#-uso)
- [Licença](#-licença)

## 🛠 Tecnologias

- ASP.NET Core
- Entity Framework Core
- SQL Server
- HTML, CSS, (para o frontend)
- Bootstrap (para estilização)

## 🚀 Funcionalidades

- **Pesquisa de Jogadores:** Busque jogadores por nome de usuário e veja suas estatísticas.
- **Visualização de Partidas:** Exiba um histórico de partidas recentes de um jogador.
- **Análise de Desempenho:** Estatísticas resumidas para cada partida.
- **Perfis de Jogadores:** Detalhes do perfil com informações adicionais sobre o jogador.

## 📋 Pré-requisitos

Antes de começar, certifique-se de ter instalado:

- [.NET SDK](https://dotnet.microsoft.com/download)
- [Git](https://git-scm.com/)

## 📦 Instalação

1. Clone o repositório:

    ```bash
    git clone https://github.com/viniciustravenssoli/OP-GG-Clone-in-ASPNET
    ```
    
2. Restaure os pacotes NuGet:

    ```bash
    dotnet restore
    ```
    
3. Configure a string de conexão com o banco de dados no arquivo `appsettings.json`.

4. Crie o banco de dados e aplique as migrações:

    ```bash
    dotnet ef database update
    ```

## ▶️ Uso

1. Inicie o projeto:

    ```bash
    dotnet run
    ```

2. Abra o navegador e vá para `http://localhost:` para acessar o aplicativo.


## 📝 Licença

Este projeto está sob a licença MIT. Consulte o arquivo [LICENSE](./LICENSE) para obter mais detalhes.
