@echo off

pushd "%~dp0"

cd InMemCached
..\Libraries\NuGet\NuGet.exe pack -Prop Configuration=Release

pause

popd
