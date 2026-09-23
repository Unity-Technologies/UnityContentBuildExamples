<#
.SYNOPSIS
Generates the card images used by the Phrasebook example.

.DESCRIPTION
Draws one PNG per phrase and language into Assets/Cards/<Phrase>/<Phrase>.<lang>.png: a rounded
rectangle in the phrase's colour with the word centred in white and the language code in the corner.
The images are committed to the repository, so this script only needs to run again to change a word,
add a phrase, or add a language.

Uses System.Drawing, so it runs on Windows PowerShell or PowerShell 7 on Windows.

.EXAMPLE
pwsh ./Tools/New-PhraseCards.ps1
#>
[CmdletBinding()]
param(
    [string]$OutputRoot = (Join-Path $PSScriptRoot "..\Assets\Cards"),
    [int]$Width = 512,
    [int]$Height = 256
)

Add-Type -AssemblyName System.Drawing

# Phrase id -> colour, and the word in each language.  Add a language by adding a key to each phrase.
$phrases = [ordered]@{
    Hello    = @{ Color = "#2A9D8F"; Words = [ordered]@{ en = "Hello";     fr = "Bonjour";   es = "Hola" } }
    Goodbye  = @{ Color = "#7B4B94"; Words = [ordered]@{ en = "Goodbye";   fr = "Au revoir"; es = "Adiós" } }
    ThankYou = @{ Color = "#E09F3E"; Words = [ordered]@{ en = "Thank you"; fr = "Merci";     es = "Gracias" } }
    Welcome  = @{ Color = "#5C6BC0"; Words = [ordered]@{ en = "Welcome";   fr = "Bienvenue"; es = "Bienvenido" } }
}

function New-Card {
    param([string]$Path, [string]$Word, [string]$Language, [System.Drawing.Color]$Fill)

    $bitmap = New-Object System.Drawing.Bitmap $Width, $Height
    $g = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
        $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
        $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
        $g.Clear([System.Drawing.Color]::Transparent)

        # Rounded rectangle, inset slightly so the anti-aliased edge is not clipped.
        $radius = 32
        $rect = New-Object System.Drawing.RectangleF 2, 2, ($Width - 4), ($Height - 4)
        $shape = New-Object System.Drawing.Drawing2D.GraphicsPath
        $d = $radius * 2
        $shape.AddArc($rect.X, $rect.Y, $d, $d, 180, 90)
        $shape.AddArc($rect.Right - $d, $rect.Y, $d, $d, 270, 90)
        $shape.AddArc($rect.Right - $d, $rect.Bottom - $d, $d, $d, 0, 90)
        $shape.AddArc($rect.X, $rect.Bottom - $d, $d, $d, 90, 90)
        $shape.CloseFigure()
        $brush = New-Object System.Drawing.SolidBrush $Fill
        $g.FillPath($brush, $shape)

        # The word, centred.  Shrink the font until the longest word fits with a margin.
        $family = [System.Drawing.FontFamily]::GenericSansSerif
        $size = 72
        do {
            $font = New-Object System.Drawing.Font $family, $size, ([System.Drawing.FontStyle]::Bold), ([System.Drawing.GraphicsUnit]::Pixel)
            $measured = $g.MeasureString($Word, $font)
            if ($measured.Width -le $Width - 64) { break }
            $font.Dispose()
            $size -= 4
        } while ($size -gt 24)

        $format = New-Object System.Drawing.StringFormat
        $format.Alignment = [System.Drawing.StringAlignment]::Center
        $format.LineAlignment = [System.Drawing.StringAlignment]::Center
        $g.DrawString($Word, $font, [System.Drawing.Brushes]::White, $rect, $format)

        # Language code, bottom right, small.
        $tagFont = New-Object System.Drawing.Font $family, 20, ([System.Drawing.FontStyle]::Regular), ([System.Drawing.GraphicsUnit]::Pixel)
        $tagFormat = New-Object System.Drawing.StringFormat
        $tagFormat.Alignment = [System.Drawing.StringAlignment]::Far
        $tagFormat.LineAlignment = [System.Drawing.StringAlignment]::Far
        $tagRect = New-Object System.Drawing.RectangleF 0, 0, ($Width - 24), ($Height - 18)
        $tagBrush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(200, 255, 255, 255))
        $g.DrawString($Language.ToUpperInvariant(), $tagFont, $tagBrush, $tagRect, $tagFormat)

        $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally {
        $g.Dispose()
        $bitmap.Dispose()
    }
}

foreach ($phraseId in $phrases.Keys) {
    $phrase = $phrases[$phraseId]
    $folder = Join-Path $OutputRoot $phraseId
    New-Item -ItemType Directory -Force $folder | Out-Null
    $fill = [System.Drawing.ColorTranslator]::FromHtml($phrase.Color)

    foreach ($lang in $phrase.Words.Keys) {
        $file = Join-Path $folder "$phraseId.$lang.png"
        New-Card -Path $file -Word $phrase.Words[$lang] -Language $lang -Fill $fill
        Write-Output "Wrote $file"
    }
}
