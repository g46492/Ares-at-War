set SEInstallDir="E:\SteamLibrary\steamapps\common\SpaceEngineers"
for %%I in (.) do set ParentDirName=%%~nxI
%SEInstallDir%\Bin64\SEWorkshopTool.exe --upload --compile --mods "%ParentDirName%" --exclude .bat .psd .md --description description.md --message patch_notes.md --tags mod "Respawn Ship"
pause