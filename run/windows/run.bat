@echo off
REM Duplo clique para iniciar. Chama o run.ps1 sem exigir mudança de política do PowerShell.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0run.ps1"
pause
