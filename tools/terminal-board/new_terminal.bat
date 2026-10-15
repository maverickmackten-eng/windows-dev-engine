@echo off
REM Opens a new named terminal.  Usage:  new_terminal.bat Maverick
cd /d "%~dp0"
python launcher.py new %1
