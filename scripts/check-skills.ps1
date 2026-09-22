$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$skills = @(Get-ChildItem (Join-Path $root '.agents/skills') -Directory)
if ($skills.Count -eq 0) { throw 'No project skills found.' }
foreach ($skill in $skills) {
    $entry = Join-Path $skill.FullName 'SKILL.md'
    $text = Get-Content -LiteralPath $entry -Raw -Encoding UTF8
    if ($text -notmatch '(?s)\A---\r?\nname: ([a-z0-9-]+)\r?\ndescription: ([^\r\n]+)\r?\n---') {
        throw "Invalid discovery metadata: $entry"
    }
    if ($Matches[1] -ne $skill.Name -or $skill.Name.Length -gt 64) { throw "Invalid skill name: $entry" }
    if ($text -notmatch '\]\((\.\./\.\./\.\./\.github/skills/[^)]+/SKILL\.md)\)') {
        throw "Missing maintained source link: $entry"
    }
    $source = [IO.Path]::GetFullPath((Join-Path $skill.FullName $Matches[1]))
    if (!(Test-Path -LiteralPath $source -PathType Leaf)) { throw "Missing skill source: $source" }
    if ((Get-Content -LiteralPath $source -Raw -Encoding UTF8) -notmatch '(?m)^name:\s*'+[regex]::Escape($skill.Name)+'\s*$') {
        throw "Source name differs from discovery name: $source"
    }
}
Write-Host "Validated $($skills.Count) skill entrypoints and their maintained sources."
