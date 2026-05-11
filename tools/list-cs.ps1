Get-ChildItem -Path . -Filter *.cs -Recurse | Where-Object { $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\.codex-build\\' } | ForEach-Object { $_.FullName }
