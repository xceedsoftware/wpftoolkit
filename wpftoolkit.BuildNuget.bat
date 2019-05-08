echo off

REM navigate to current directory
cd /d %~dp0

REM take file name of batch without extension
set project=%~n0
REM replace ".BuildNuget" with ""
set project=%project:.BuildNuget=%
set nuget=tools\nuget.exe

echo .
echo nuget pack %project% to %output%
echo .
echo !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
echo !!! Please double check:                                         !!!
echo !!!  - Compiled you project in Release?                          !!!
echo !!!  - Increased assembly version?                               !!!
echo !!!  - Created and pushed a git tag?                             !!!
echo !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
echo .

pause

echo on
%nuget% update -self
%nuget% pack %project%.nuspec -Prop Configuration=Release

pause