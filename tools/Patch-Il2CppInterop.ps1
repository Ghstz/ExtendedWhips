# Patch-Il2CppInterop.ps1
# Fixes Il2CppInterop issue #183 (Sequence contains more than one element in GenericMethod_GetMethod_Hook)

Write-Host "Running Il2CppInterop Patcher..." -ForegroundColor Cyan
dotnet run --project "$PSScriptRoot\FixIl2Cpp\FixIl2Cpp.csproj"

