#define Repository     "./Installer"
#define AppName      "IPA BCFier Revit Plugin"
#define AppPublisher "Dangl IT GmbH"
#define AppURL       "https://www.dangl-it.com"

#define RevitAddinFolder "{sd}\ProgramData\Autodesk\Revit\Addins"
#define RevitAddin24  RevitAddinFolder+"\2024\"
#define RevitAddin23  RevitAddinFolder+"\2023\"
#define RevitAddin22  RevitAddinFolder+"\2022\"
#define RevitAddin21  RevitAddinFolder+"\2021\"

[Setup]
AppId="cfd740a5-8089-4323-a6e5-c86cfee9cca2"
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
DefaultDirName={#RevitAddin24}
DisableDirPage=yes
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
DisableWelcomePage=no
OutputDir={#Repository}\output
OutputBaseFilename=IpaBcfierRevitPlugin
SetupIconFile={#Repository}\InstallerAssets\BCF.ico
Compression=lzma
SolidCompression=yes
WizardImageFile={#Repository}\InstallerAssets\banner.bmp
ChangesAssociations=yes

[Components]
Name: revit24; Description: Addin for Autodesk Revit 2024;  Types: full
Name: revit23; Description: Addin for Autodesk Revit 2023;  Types: full
Name: revit22; Description: Addin for Autodesk Revit 2022;  Types: full
Name: revit21; Description: Addin for Autodesk Revit 2021;  Types: full

[Files]

;REVIT 2024
Source: "{#Repository}\Release-2024\IPA.Bcfier.Revit.addin"; DestDir: "{#RevitAddin24}"; Flags: ignoreversion; Components: revit24
Source: "{#Repository}\Release-2024\DecimalEx.dll"; DestDir: "{#RevitAddin24}\Ipa.BCFier"; Flags: ignoreversion; Components: revit24
Source: "{#Repository}\Release-2024\Dangl.BCF.dll"; DestDir: "{#RevitAddin24}\Ipa.BCFier"; Flags: ignoreversion; Components: revit24
Source: "{#Repository}\Release-2024\IPA.Bcfier.dll"; DestDir: "{#RevitAddin24}\Ipa.BCFier"; Flags: ignoreversion; Components: revit24
Source: "{#Repository}\Release-2024\IPA.Bcfier.Revit.dll"; DestDir: "{#RevitAddin24}\Ipa.BCFier"; Flags: ignoreversion; Components: revit24
Source: "{#Repository}\bcfier-app\*"; DestDir: "{#RevitAddin24}\Ipa.BCFier\ipa-bcfier-app"; Flags: ignoreversion recursesubdirs; Components: revit24

;REVIT 2023
Source: "{#Repository}\Release-2023\IPA.Bcfier.Revit.addin"; DestDir: "{#RevitAddin23}"; Flags: ignoreversion; Components: revit23
Source: "{#Repository}\Release-2023\DecimalEx.dll"; DestDir: "{#RevitAddin23}\Ipa.BCFier"; Flags: ignoreversion; Components: revit23
Source: "{#Repository}\Release-2023\Dangl.BCF.dll"; DestDir: "{#RevitAddin23}\Ipa.BCFier"; Flags: ignoreversion; Components: revit23
Source: "{#Repository}\Release-2023\IPA.Bcfier.dll"; DestDir: "{#RevitAddin23}\Ipa.BCFier"; Flags: ignoreversion; Components: revit23
Source: "{#Repository}\Release-2023\IPA.Bcfier.Revit.dll"; DestDir: "{#RevitAddin23}\Ipa.BCFier"; Flags: ignoreversion; Components: revit23
Source: "{#Repository}\bcfier-app\*"; DestDir: "{#RevitAddin23}\Ipa.BCFier\ipa-bcfier-app"; Flags: ignoreversion recursesubdirs; Components: revit23

;REVIT 2022
Source: "{#Repository}\Release-2022\IPA.Bcfier.Revit.addin"; DestDir: "{#RevitAddin22}"; Flags: ignoreversion; Components: revit22
Source: "{#Repository}\Release-2022\DecimalEx.dll"; DestDir: "{#RevitAddin22}\Ipa.BCFier"; Flags: ignoreversion; Components: revit22
Source: "{#Repository}\Release-2022\Dangl.BCF.dll"; DestDir: "{#RevitAddin22}\Ipa.BCFier"; Flags: ignoreversion; Components: revit22
Source: "{#Repository}\Release-2022\IPA.Bcfier.dll"; DestDir: "{#RevitAddin22}\Ipa.BCFier"; Flags: ignoreversion; Components: revit22
Source: "{#Repository}\Release-2022\IPA.Bcfier.Revit.dll"; DestDir: "{#RevitAddin22}\Ipa.BCFier"; Flags: ignoreversion; Components: revit22
Source: "{#Repository}\bcfier-app\*"; DestDir: "{#RevitAddin22}\Ipa.BCFier\ipa-bcfier-app"; Flags: ignoreversion recursesubdirs; Components: revit22

;REVIT 2021
Source: "{#Repository}\Release-2021\IPA.Bcfier.Revit.addin"; DestDir: "{#RevitAddin21}"; Flags: ignoreversion; Components: revit21
Source: "{#Repository}\Release-2021\DecimalEx.dll"; DestDir: "{#RevitAddin21}\Ipa.BCFier"; Flags: ignoreversion; Components: revit21
Source: "{#Repository}\Release-2021\Dangl.BCF.dll"; DestDir: "{#RevitAddin21}\Ipa.BCFier"; Flags: ignoreversion; Components: revit21
Source: "{#Repository}\Release-2021\IPA.Bcfier.dll"; DestDir: "{#RevitAddin21}\Ipa.BCFier"; Flags: ignoreversion; Components: revit21
Source: "{#Repository}\Release-2021\IPA.Bcfier.Revit.dll"; DestDir: "{#RevitAddin21}\Ipa.BCFier"; Flags: ignoreversion; Components: revit21
Source: "{#Repository}\bcfier-app\*"; DestDir: "{#RevitAddin21}\Ipa.BCFier\ipa-bcfier-app"; Flags: ignoreversion recursesubdirs; Components: revit21
