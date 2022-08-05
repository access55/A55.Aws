# A55.Extensions

[![CI](https://github.com/access55/A55.Extensions/actions/workflows/push.yml/badge.svg)](https://github.com/access55/akrualizador/actions/workflows/push.yml)
![](https://access55.github.io/A55.Extensions/lines_badge.svg)
![](https://access55.github.io/A55.Extensions/test_report_badge.svg)
![](https://access55.github.io/A55.Extensions/badge_linecoverage.svg)
![](https://access55.github.io/A55.Extensions/badge_branchcoverage.svg)

![](https://access55.github.io/A55.Extensions/dotnet_version_badge.svg)
![](https://img.shields.io/badge/Lang-C%23-green)
![https://editorconfig.org/](https://img.shields.io/badge/style-EditorConfig-black)
[![pre-commit](https://img.shields.io/badge/pre--commit-enabled-brightgreen?&logoColor=white)](build/Helpers/GitHooks.cs)


## 🛠️ Tarefas

Este projeto utiliza  **[NUKE](https://nuke.build/docs/introduction/)** 🏗️
 * TL;DR: _é um gestor de tarefas e builds, parecido com  Make, só que com C#_
 * Você pode listar as tarefas disponíveis usando `./build` ou `dotnet nuke`
> **⚠️ Atenção**: _voce deve usar a versão do `./build` para seu sistema operacional, no windows `./build.cmd` outros `./build.sh`_

---
## ⚙️ Setup (Development)

O que precisa ter instalado? ( **Dependências** ):

* [.NET](https://dotnet.microsoft.com/en-us/download)  ![](https://access55.github.io/akrualizador/dotnet_version_badge.svg)
* [Docker](https://docs.docker.com/get-docker/)
* Algum Editor ou IDE ([VSCode](https://code.visualstudio.com)
  / [Visual Studio](https://visualstudio.microsoft.com/pt-br/)
  / [VS4Mac🍎](https://visualstudio.microsoft.com/pt-br/vs/mac/) / [Rider](https://www.jetbrains.com/pt-br/rider/))


Tendo as dependências instaladas, rode o comando para fazer o setup básico, isso vai configurar um container `postgres`, instalar um [hook de pre-commit](https://git-scm.com/docs/githooks) de formatação, e garantir a instalação dos pacotes e [tools](https://docs.microsoft.com/pt-br/dotnet/core/tools/global-tools) locais da aplicação:

```shell
./build.sh setup
```
Ou restore e rode o setup via [Nuke](https://nuke.build):

```shell
dotnet tool restore
dotnet nuke setup
```

> **💡 Dica**: _Todo lugar que é usado o `./build` pode ser substituído por `dotnet nuke` com a vantagem que pode ser rodado de qualquer pasta do repositório desde que tenha feito pelo menos uma vez o `dotnet tool restore`_

---
## 📏 Code Style / Formatação / Linter

Para garantir uma formação consistente esse projeto reforça o estilo de código utilizando [EditorConfig](https://editorconfig.org), vale verificar se seu editor ou IDE possui [suporte nativo](https://editorconfig.org/#pre-installed) ou [precisa de plugin](https://editorconfig.org/#download) de formatação.
Tanto no [CI](.github/workflows/push.yml) quanto no pre-commit é utilizado o [dotnet format](https://github.com/dotnet/format) que usa o [`.editorconfig`](.editorconfig) e os analyzers instalados no projeto como configuração.

> **💅 Dica**: Recomendamos a font [JetBrains Mono](https://www.jetbrains.com/pt-br/lp/mono) e que habilite as ligatures no seu editor🎨

## 🔨 Formatação automática:

Basta rodar o comando:
```
./build format
```
> **💡 Dica**: _Nem todo erro de formatação e warning é passível de ser resolvido automaticamente, veja na saída do comando para saber se ocorreu tudo bem_

## 🛫 Rodar

Esse comando ira iniciar o banco de dados no docker, aplicar as migrations e iniciar a API
```
./build run
```

> **📌 OpenAPI**: Com a API de pé é possível acessar o [Swagger](https://swagger.io) na rota `/swagger`

## 🧪 Testes

Voce pode gerar um relatório de coverage usando

```shell
./build report
```
- _por baixo dos panos utiliza [reportgenerator](https://github.com/danielpalme/ReportGenerator) e [coverlet](https://github.com/coverlet-coverage/coverlet)_

Para rodar todos os testes basta rodar
```shell
./build test
```


## 🚀 Publish

Este projeto utiliza de **[Trunk Based Development](http://trunkbaseddevelopment.com/)**, logo só possui uma branch de vida longa ativa.


___
## 🚪🚶Fim...
