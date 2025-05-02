<#
.SYNOPSIS
    Recursively find all C# files that reference Newtonsoft.Json.

.DESCRIPTION
    Scans every .cs file under the script’s folder, looking for any mention of "Newtonsoft.Json".
    Outputs the file path, line number, and matching line for your review.
#>

# Change to the script’s directory (so dot-sourcing or running from elsewhere still works)
Push-Location $PSScriptRoot

# Main pipeline: 
# 1. Find all .cs files
# 2. Search them for “Newtonsoft.Json”
# 3. Group matches by file
# 4. Print file header + each matching line with its number
Get-ChildItem -Path . -Recurse -Filter *.cs |
  Select-String -Pattern 'Newtonsoft\.Json' |
  Group-Object -Property Path |
  ForEach-Object {
      # File header
      Write-Host "`n=== $($_.Name) ===" -ForegroundColor Cyan

      # Each match in that file
      $_.Group |
        Sort-Object LineNumber |
        ForEach-Object {
            Write-Host ("  {0,4}: {1}" -f $_.LineNumber, $_.Line.Trim())
        }
  }

# Return to original directory
Pop-Location