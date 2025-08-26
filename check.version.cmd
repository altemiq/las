dotnet tool update altemiq.semanticversioning --global
dotnet build src
dotnet semver diff solution src^
  --output BreakingChanges^
  --direct-download^
  --version-suffix personal^
  --force
