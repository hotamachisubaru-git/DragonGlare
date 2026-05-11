$ErrorActionPreference = 'Stop'
$projectDir = 'D:\Dev\DragonGlareAlpha'
$excludeDirs = @('bin', 'obj', '.codex-build')

# Collect all .cs files
$files = Get-ChildItem -Path $projectDir -Recurse -Filter '*.cs' | Where-Object {
    $path = $_.FullName
    $skip = $false
    foreach ($ex in $excludeDirs) {
        if ($path -match "\\$ex\\") { $skip = $true; break }
    }
    -not $skip
}

# Regex patterns
$classRegex = '^\s*(?:public|internal|private|protected|file)?\s*(?:static|sealed|abstract)?\s*class\s+(\w+)'
$methodRegex = '^\s*(?:public|internal|private|protected)?\s*(?:static|virtual|override|abstract|async)?\s*[\w<>,\[\]]+\s+(\w+)\s*\('
$propRegex = '^\s*(?:public|internal|private|protected)?\s*(?:static|virtual|override)?\s*[\w<>,\[\]]+\s+(\w+)\s*[{]'
$fieldRegex = '^\s*(?:public|internal|private|protected|readonly)?\s*(?:static|readonly)?\s*[\w<>,\[\]]+\s+(\w+)\s*;'

$definitions = @()
$allText = @{}

foreach ($file in $files) {
    $content = Get-Content -Path $file.FullName -Raw
    $allText[$file.FullName] = $content
    $lines = $content -split "`r?`n"
    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ($line -match $classRegex) {
            $definitions += [PSCustomObject]@{ Type='Class'; Name=$matches[1]; File=$file.FullName; Line=($i+1) }
        }
        if ($line -match $methodRegex) {
            $definitions += [PSCustomObject]@{ Type='Method'; Name=$matches[1]; File=$file.FullName; Line=($i+1) }
        }
        if ($line -match $propRegex) {
            $definitions += [PSCustomObject]@{ Type='Property'; Name=$matches[1]; File=$file.FullName; Line=($i+1) }
        }
        if ($line -match $fieldRegex) {
            $definitions += [PSCustomObject]@{ Type='Field'; Name=$matches[1]; File=$file.FullName; Line=($i+1) }
        }
    }
}

# Check references (simple text search across all files)
$unused = @()
foreach ($def in $definitions) {
    $name = $def.Name
    # Skip common patterns
    if ($name -in @('Main', 'ToString', 'Equals', 'GetHashCode', 'Dispose', 'Finalize', 'Clone', 'CompareTo', 'GetEnumerator', 'Deconstruct', 'Invoke', 'MoveNext', 'Current', 'Reset', 'Value', 'HasValue', 'GetType', 'MemberwiseClone')) { continue }
    if ($name -match '^<') { continue } # compiler generated
    if ($name -match '^_') { continue } # private fields often used via reflection or partial
    if ($def.Type -eq 'Method' -and ($name -match '^get_' -or $name -match '^set_' -or $name -match '^add_' -or $name -match '^remove_')) { continue }
    if ($def.Type -eq 'Method' -and $name -eq $def.Name -and $name -eq 'Draw' -or $name -eq 'Update' -or $name -eq 'Initialize' -or $name -eq 'LoadContent' -or $name -eq 'UnloadContent') { continue }

    $refCount = 0
    foreach ($filePath in $allText.Keys) {
        $text = $allText[$filePath]
        # Simple word boundary check
        $pattern = '\b' + [regex]::Escape($name) + '\b'
        $matches = [regex]::Matches($text, $pattern)
        foreach ($m in $matches) {
            # Determine if it's a definition line by checking context
            $lineStart = $text.LastIndexOf("`n", $m.Index) + 1
            $lineEnd = $text.IndexOf("`n", $m.Index)
            if ($lineEnd -lt 0) { $lineEnd = $text.Length }
            $lineText = $text.Substring($lineStart, $lineEnd - $lineStart)
            # Exclude comment lines and string literals (rough)
            if ($lineText -match '^\s*//') { continue }
            # If same file and line matches definition, skip
            if ($filePath -eq $def.File -and $lineText -match [regex]::Escape($name)) {
                # This is likely the definition; check if there are other occurrences
                continue
            }
            $refCount++
        }
    }
    if ($refCount -eq 0) {
        $unused += $def
    }
}

$unused | Format-Table -AutoSize
Write-Host "Total unused symbols: $($unused.Count)"
