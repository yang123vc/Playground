@echo off

if "%1"=="Debug" exit

echo ######## Copying to Output ########
cd %~dp0
copy "..\bin\Release\*.dll" "..\..\Output"
del "..\..\Output\sciter.dll"