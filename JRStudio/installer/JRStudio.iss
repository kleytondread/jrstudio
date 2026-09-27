; Script do instalador do J.R Studio (Inno Setup 6).
; Como gerar um novo instalador (ex: depois de uma nova versão): ver Jess.Docs/Guia_Implementacao_IA.md.
;
; AppId NUNCA deve mudar entre versões — é o que permite ao Inno Setup reconhecer uma instalação já
; existente e atualizar por cima dela (em vez de instalar do zero numa pasta separada / duplicada).

#define MyAppName "J.R Studio"
#define MyAppVersion "0.1.0"
#define MyAppPublisher "J.R Studio"
#define MyAppExeName "Jess.Desktop.exe"
#define MyPublishDir "..\src\Jess.Desktop\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish"

[Setup]
AppId={{52685F9C-28BE-4C5A-9FB8-DD3DB2A57A98}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
VersionInfoVersion={#MyAppVersion}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#MyAppExeName}
SetupIconFile=..\src\Jess.Desktop\app.ico
OutputDir=Output
OutputBaseFilename=Setup-JRStudio-{#MyAppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checkedonce

[Files]
Source: "{#MyPublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "MicrosoftEdgeWebview2Setup.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{tmp}\MicrosoftEdgeWebview2Setup.exe"; Parameters: "/silent /install"; StatusMsg: "Instalando componente Microsoft Edge WebView2..."; Check: not WebView2Instalado
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// Detecção oficial da Microsoft pra saber se o WebView2 Runtime já está presente — evita baixar/
// reinstalar à toa em máquinas Windows 10/11 atualizadas, que já vêm com ele de fábrica na maioria
// dos casos. Checa os três lugares onde o instalador "evergreen" do WebView2 pode ter registrado a
// própria versão (WOW6432Node de 64 bits, chave nativa de 64 bits, e a instalação por usuário).
function LerVersaoWebView2(RootKey: Integer; SubKeyName: String; var Versao: String): Boolean;
begin
  Result := RegQueryStringValue(RootKey, SubKeyName, 'pv', Versao) and (Versao <> '') and (Versao <> '0.0.0.0');
end;

function WebView2Instalado: Boolean;
var
  Versao: String;
  ChaveCliente: String;
begin
  ChaveCliente := 'SOFTWARE\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}';
  Result :=
    LerVersaoWebView2(HKLM64, 'SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', Versao) or
    LerVersaoWebView2(HKLM64, ChaveCliente, Versao) or
    LerVersaoWebView2(HKCU, ChaveCliente, Versao);
end;
