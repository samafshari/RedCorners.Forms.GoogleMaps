@echo off
call :Clean RedCorners.Forms.GoogleMaps RedCorners.Forms.GoogleMaps
call :Clean . RedCorners.Forms.GoogleMaps.Demo

exit /b %ERRORLEVEL%

:Clean
@echo Cleaning %~1 
del /q /s %~1\%~2.Droid\bin
del /q /s %~1\%~2.Droid\obj
del /q /s %~1\%~2.Droid\*.user

del /q /s %~1\%~2.iOS\bin
del /q /s %~1\%~2.iOS\obj
del /q /s %~1\%~2.iOS\*.user