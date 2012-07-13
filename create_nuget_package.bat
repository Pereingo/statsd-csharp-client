echo "Make sure you build the project in release mode first!"

lib\NuGet.exe pack StatsdClient\StatsdClient.csproj -Prop Configuration=Release

echo "Publish the .nupkg file"
pause