#!/usr/bin/env pwsh
# Regenerates docs/claude/project-map.md - a compact structural snapshot of the
# hand-authored parts of the project, so a Claude session on any machine can
# orient without re-exploring the tree. No Unity required; safe to run anytime.
# Also invoked from the Editor: Tools > White Lightning > Regenerate Claude Map.
#
# Kept ASCII-only on purpose: this runs under both PowerShell 7+ and Windows
# PowerShell 5.1, which disagree about source-file encoding.

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot        # repo root (this script lives in Tools/)
$out  = Join-Path $root 'docs/claude/project-map.md'

# Curated allow-list - stable as third-party art packages come and go under Assets/.
$includeDirs = @(
    'Assets/Scripts',
    'Assets/Editor',
    'Assets/Scenes',
    'Assets/Prefabs',
    'Assets/Resources',
    'Assets/Settings',
    'docs'
)
$rootFiles = @('CLAUDE.md','README.md','Packages/manifest.json','ProjectSettings/ProjectVersion.txt','.gitignore','.gitattributes','.claude/settings.json')
$textExt   = @('.cs','.md','.json','.asmdef','.txt','.uxml','.uss','.shader','.hlsl','.asset')

function LineCount([string]$path) {
    if ($textExt -notcontains ([System.IO.Path]::GetExtension($path))) { return $null }
    try { return (Get-Content -LiteralPath $path -ErrorAction Stop | Measure-Object -Line).Lines }
    catch { return $null }
}

$newest = [datetime]'1970-01-01'
$lines  = [System.Collections.Generic.List[string]]::new()
function Emit([string]$s) { $script:lines.Add($s) }

Emit '# Project map - White Lightning'
Emit ''
Emit 'Auto-generated structural snapshot of the hand-authored project. **Do not hand-edit** -'
Emit 'regenerate with `Tools/regen-claude-map.ps1` / `Tools/regen-claude-map.sh` or the Editor'
Emit 'menu *Tools > White Lightning > Regenerate Claude Map*. Third-party art packages under'
Emit '`Assets/` are omitted on purpose.'
Emit ''

Emit '## Root'
foreach ($rel in $rootFiles) {
    $p = Join-Path $root $rel
    if (-not (Test-Path $p)) { continue }
    $fi = Get-Item -LiteralPath $p
    if ($fi.LastWriteTimeUtc -gt $newest) { $newest = $fi.LastWriteTimeUtc }
    $lc = LineCount $p
    if ($null -ne $lc) { Emit ("- ``{0}``  ({1} lines)" -f $rel, $lc) }
    else               { Emit ("- ``{0}``" -f $rel) }
}
Emit ''

foreach ($dir in $includeDirs) {
    $abs = Join-Path $root $dir
    if (-not (Test-Path $abs)) { continue }
    $files = Get-ChildItem -LiteralPath $abs -Recurse -File |
             Where-Object { $_.Extension -ne '.meta' } |
             Sort-Object FullName
    if (-not $files) { continue }
    Emit ("## {0}  ({1} files)" -f $dir, $files.Count)
    foreach ($f in $files) {
        if ($f.LastWriteTimeUtc -gt $newest) { $newest = $f.LastWriteTimeUtc }
        $rel = $f.FullName.Substring($root.Length + 1).Replace('\','/')
        $lc  = LineCount $f.FullName
        if ($null -ne $lc) { Emit ("- ``{0}``  ({1} lines)" -f $rel, $lc) }
        else               { Emit ("- ``{0}``" -f $rel) }
    }
    Emit ''
}

$stamp = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
$nstmp = $newest.ToString('yyyy-MM-ddTHH:mm:ssZ')

# OS name that works on both PS 5.1 (no $IsWindows) and PS 7+.
if (Get-Variable -Name IsWindows -ErrorAction SilentlyContinue) {
    $os = if ($IsWindows) { 'WINDOWS' } elseif ($IsMacOS) { 'MACOS' } else { 'LINUX' }
} else {
    $os = 'WINDOWS'   # Windows PowerShell 5.1 only exists on Windows
}

$all = @(
    "<!-- generated: $stamp by regen-claude-map.ps1 on $os -->"
    "<!-- newest-source: $nstmp  (mtime of the newest file listed below) -->"
) + $lines

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $out) | Out-Null
# BOM-free UTF-8, LF line endings.
[System.IO.File]::WriteAllText($out, ($all -join "`n") + "`n", (New-Object System.Text.UTF8Encoding($false)))
Write-Host "Wrote $out"
Write-Host "  newest source file: $nstmp"
