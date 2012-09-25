echo "Make sure you've built the project in release mode first!"
pause

lib\NuGet.exe pack src\StatsdClient\StatsdClient.csproj -Prop Configuration=Release

echo "Publish the .nupkg file"
pause