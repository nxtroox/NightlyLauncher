#define MyAppName "NightlyLauncher"
#define MyAppVersion GetVersionNumbersString("..\publish\NightlyLauncher.exe")
#define MyAppPublisher "nxtroox"
#define MyAppURL "https://github.com/nxtroox/NightlyLauncher"
#define MyAppExeName "NightlyLauncher.exe"
#define DoubleAmp(Value) StringChange(Value, "&", "&&")
#call EmitLanguagesSection

[Setup]
AppId={{1A77CE7D-DED5-4C3E-9BF6-6EA7D1FF5112}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
AppUpdatesURL={#MyAppURL}/releases
DefaultDirName={autopf}\{#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
SetupArchitecture=x64
DisableProgramGroupPage=yes
LicenseFile=..\LICENSE
PrivilegesRequiredOverridesAllowed=dialog
OutputBaseFilename={#MyAppName}Setup-win-x64
SolidCompression=yes
WizardStyle=modern dynamic
SetupIconFile=..\src\NightlyLauncher\Assets\NightlyLauncher.ico

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Excludes: "*.pdb"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#DoubleAmp(MyAppName)}}"; Flags: nowait postinstall skipifsilent
