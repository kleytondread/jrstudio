# Troubleshooting — Tela em branco / erro do Blazor no WPF (WebView2)

*Registro técnico de um problema enfrentado na Fase 1 (setembro/2026), para consulta futura caso ele volte a aparecer — por exemplo, ao atualizar pacotes ou o SDK do .NET.*

---

## Sintoma

Ao rodar `Jess.Desktop` (via `dotnet run`, executável direto, ou debug do Visual Studio), um ou mais dos seguintes:

- Processo encerra sozinho logo na abertura, sem UI nenhuma (às vezes com um dump em `%LOCALAPPDATA%\CrashDumps\Jess.Desktop.exe.<pid>.dmp`)
- Janela abre mas fica travada em "Carregando...", com a mensagem "Ocorreu um erro inesperado. Recarregar ✕" sempre visível
- Janela renderiza o conteúdo, mas sem nenhum estilo (sem paleta de cores, sem a fonte Inter/Space Grotesk, ícones SVG gigantes e desproporcionais — como se o `app.css` não tivesse sido aplicado)

Eram **4 bugs empilhados e independentes**. Cada um mascarava o diagnóstico do seguinte, então vale ler os quatro mesmo que só um sintoma esteja acontecendo agora.

---

## Contexto necessário: o que é o BlazorWebView

Um app "Blazor Hybrid" não roda num navegador nem num servidor Kestrel — ele roda como processo WPF nativo, com um controle **WebView2** (o motor do Edge/Chromium embarcado como `Control`, tipo um `TextBox` ou `Grid`) dentro da janela. O `BlazorWebView` (`Microsoft.AspNetCore.Components.WebView.Wpf`) sobe um host HTTP falso (`https://0.0.0.0/`) só pra esse WebView2 navegar, intercepta as "requisições" servindo arquivos do `wwwroot` local ou fazendo interop JS↔.NET, e roda o runtime do Blazor dentro dessa página — sem Kestrel/ASP.NET Core hospedando de verdade por trás. Por isso mecanismos que funcionam "de graça" em Blazor Server/WASM (Static Web Assets, middleware de arquivos estáticos) são reimplementados de forma mais simples e mais frágil nesse modo.

---

## Bug #1 — Crash nativo no `WebView2CompositionControl`

**Sintoma:** processo morre na abertura. Event Viewer do Windows (Log de Aplicativos) mostra:

```
Provider: .NET Runtime, Id 1026
Exception Info: System.IO.FileNotFoundException: Could not load file or assembly
'Microsoft.Windows.SDK.NET, Version=10.0.17763.10, ...'
   at Microsoft.Web.WebView2.Wpf.WebView2CompositionControl.TryInitializeD3DImage()
   at ... Window.Show() ... Jess.Desktop.App.OnStartup(...)
```

Também aparece como `Application Error` genérico com módulo `CoreMessaging.dll`, código `0xc0000602`.

**Causa:** a partir do .NET 10, `Microsoft.AspNetCore.Components.WebView.Wpf` passou a hospedar o WebView2 através do `WebView2CompositionControl` (baseado em *visual composition* — entra na árvore de renderização do WPF como uma `Visual`, resolvendo o problema de *airspace* de controles nativos) em vez do `WebView2` clássico (baseado em HWND). A composição exige interop com `Windows.UI.Composition` (WinRT), que depende da assembly de projeção `Microsoft.Windows.SDK.NET.dll` — só corretamente referenciada/copiada com um `TargetFramework` específico do SDK do Windows, algo mal documentado pela própria Microsoft. É um **bug confirmado da plataforma**, não do nosso código:

- Issue oficial: https://github.com/MicrosoftEdge/WebView2Feedback/issues/5436
- Confirmação de um engenheiro da Microsoft (comentário de 2026-08-06): mesmo aplicando o workaround de TFM, o `WebView2CompositionControl` ainda tem outros bugs abertos (tela branca durante init — #5566, conteúdo borrado — #5205, drag-drop — #5237, cliques quebrados dentro de containers com transform/scale do WPF).

**Correção aplicada:** fixar os pacotes Blazor/WebView na linha estável 9.0.x, que ainda usa o `WebView2` clássico:

```xml
<!-- src/Jess.Desktop/Jess.Desktop.csproj -->
<PackageReference Include="Microsoft.AspNetCore.Components.WebView.Wpf" Version="9.0.120" />
```
```xml
<!-- src/Jess.UI/Jess.UI.csproj -->
<PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="9.0.11" />
```

Perdemos a composição "moderna" (não é um problema pra nós — não empilhamos popups/overlays por cima do WebView2), mas ganhamos estabilidade.

**⚠️ Revisitar no futuro:** essa é a mudança que efetivamente resolveu o crash. Antes de fazer qualquer upgrade de pacotes `Microsoft.AspNetCore.Components.*`/`Microsoft.Web.WebView2.*` no projeto, conferir o status do issue #5436 acima — se a Microsoft tiver lançado uma correção definitiva, os pacotes podem voltar a acompanhar a versão do .NET (10.0.x) normalmente.

---

## Bug #2 — Blazor nunca inicializava (`autostart="false"`)

**Sintoma:** tela eternamente presa em "Carregando..." (texto placeholder de `<div id="app">`, que só é substituído quando o Blazor monta a árvore de componentes).

**Causa:** `src/Jess.Desktop/wwwroot/index.html` tinha:

```html
<script src="_framework/blazor.webview.js" autostart="false"></script>
```

`autostart="false"` é uma opção legítima do Blazor pra quando se quer chamar `Blazor.start()` manualmente (ex.: esperar alguma config assíncrona antes de montar). Ficou no scaffolding inicial (Fase 0) sem nenhum `Blazor.start()` correspondente em lugar nenhum — então o runtime nunca era iniciado.

**Correção:** removido o atributo (volta ao padrão, que já auto-inicia).

---

## Bug #3 — Banner de erro sempre visível (não era uma exceção real)

**Sintoma:** "Ocorreu um erro inesperado. Recarregar ✕" aparecendo mesmo sem nenhuma exceção acontecer — o que atrapalhou bastante o diagnóstico, fazendo parecer um bug de renderização de componente quando na real não havia exceção nenhuma disparando naquele momento.

**Causa:** `#blazor-error-ui` é uma `<div>` sempre presente no HTML por convenção do template padrão do Blazor; o runtime só a torna visível via JS quando ocorre um erro fatal de verdade. Ela fica oculta por padrão via uma regra CSS `display: none` — que nunca foi escrita (copiamos a estrutura HTML do template, mas não o CSS que acompanha).

**Correção:** adicionada a regra em `src/Jess.UI/wwwroot/css/app.css`:

```css
#blazor-error-ui {
  display: none;
  /* ...demais estilos... */
}
```

---

## Bug #4 — CSS retornando 404 (`_content/...`)

**Sintoma:** depois de corrigir os 3 bugs acima, o app renderizava, mas sem nenhum estilo — ícones SVG enormes, sem paleta de cores nem fonte Inter/Space Grotesk.

**Causa:** `index.html` referenciava o CSS pelo esquema padrão de *Static Web Assets* de Razor Class Library:

```html
<link rel="stylesheet" href="_content/Jess.UI/css/app.css" />
```

`_content/{PackageId}/{caminho}` é resolvido, num host ASP.NET Core de verdade, por middleware que lê um manifesto gerado no build (`{Assembly}.staticwebassets.runtime.json` / `.staticwebassets.endpoints.json`). No `BlazorWebView` essa resolução é feita por uma implementação própria, mais simples, dentro do pacote `Microsoft.AspNetCore.Components.WebView`. Hipótese mais provável (consistente com os sintomas, não 100% confirmada): a versão **9.0.120** desse pacote (fixada no Bug #1) não interpreta corretamente o formato de manifesto mais novo gerado pelo SDK do .NET 10 instalado (10.0.401) — ou seja, um efeito colateral do próprio downgrade que resolveu o Bug #1.

**Correção:** parar de depender desse mecanismo para esse arquivo. O CSS-fonte continua em `Jess.UI/wwwroot/css/app.css` (mantendo a estrutura de camadas da Seção 7.1 da especificação), mas `Jess.Desktop.csproj` agora copia/linka esse mesmo arquivo físico para dentro do seu próprio `wwwroot` no build:

```xml
<!-- src/Jess.Desktop/Jess.Desktop.csproj -->
<Content Include="..\Jess.UI\wwwroot\css\app.css" Link="wwwroot\css\app.css" CopyToOutputDirectory="PreserveNewest" />
```

E `index.html` passou a referenciar `css/app.css` (relativo a si mesmo) em vez de `_content/Jess.UI/css/app.css`. Isso contorna o mecanismo de Static Web Assets inteiro — o arquivo passa a ser servido do mesmo jeito que o próprio `index.html` já era (arquivo estático dentro do `wwwroot` do app, nunca dependeu de manifesto).

**⚠️ Se no futuro for adicionado mais algum arquivo estático vindo de `Jess.UI`** (ex.: uma imagem, um `.js` colocation de componente), ele vai esbarrar no mesmo problema se referenciado via `_content/Jess.UI/...`. Ou aplicar o mesmo truque de `Content Include ... Link=...` no `Jess.Desktop.csproj`, ou revisitar isso quando os pacotes voltarem para a linha 10.0.x (ver Bug #1).

---

## Ferramentas de diagnóstico que ajudaram (e as que não ajudaram)

- **Ajudou de verdade:** Visor de Eventos do Windows (`Get-WinEvent -FilterHashtable @{LogName='Application'}` via PowerShell), filtrando por `Jess.Desktop` — foi onde apareceu o stack trace real do Bug #1. Também os dumps em `%LOCALAPPDATA%\CrashDumps\`.
- **Não ajudou nesse caso (mas vale manter):** o `ErrorBoundary` com logging que foi adicionado em `Jess.UI/Layout/MainLayout.razor` (via `Jess.UI/Components/LoggingErrorBoundary.cs`) e o `services.AddBlazorWebViewDeveloperTools()` (Debug only) em `App.xaml.cs`. Nenhum dos dois pegou o Bug #1 porque ele acontece numa camada nativa/WinRT, abaixo até do `AppDomain.UnhandledException` — só serve pra exceções de renderização de componentes Razor (bugs "normais" de aplicação, não bugs de plataforma). Ficam no código porque são baratos e vão ajudar em bugs futuros desse segundo tipo.
- **Cache do WebView2:** `%LOCALAPPDATA%\Jess.Desktop.WebView2\` guarda respostas antigas (inclusive 404) entre execuções. Se uma mudança em `wwwroot` não parecer refletir, apagar essa pasta antes de desconfiar de outra coisa.

---

## Referências

- https://github.com/MicrosoftEdge/WebView2Feedback/issues/5436 — issue principal (regressão .NET 9 → 10)
- https://github.com/MicrosoftEdge/WebView2Feedback/issues/5702 e /5350 — mesmo erro relatado por outros usuários
- https://learn.microsoft.com/en-us/aspnet/core/blazor/hybrid/static-files — documentação oficial de Static Web Assets em Blazor Hybrid
