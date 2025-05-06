dotnet build --framework net8.0 -c Release HotfixAOT/
dotnet build --framework net8.0 -c Release_Patched HotfixAOT/
dotnet build --framework net8.0 -c Release PatchTool/
cd PatchTool/bin/Release/net8.0
PatchTool -i -h ../../../../HotfixAOT/Patched/HotfixAOT.hash -o ../../../../HotfixAOT/Patched/HotfixAOT.dll ../../../../HotfixAOT/bin/Release/net8.0/HotfixAOT.dll
PatchTool -p -o ../../../../HotfixAOT/Patched/HotfixAOT.patch ../../../../HotfixAOT/Patched/HotfixAOT.hash ../../../../HotfixAOT/bin/Release_Patched/net8.0/HotfixAOT.dll

pause