@echo off
set /p version="Enter Version Number to Build With: "

@echo on
dotnet pack ".\TomLonghurst.Events.NotifyContextChanged\TomLonghurst.Events.NotifyContextChanged.csproj"  --configuration Release /p:Version=%version%

pause