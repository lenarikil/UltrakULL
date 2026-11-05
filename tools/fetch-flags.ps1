# Downloads the embedded flag set for the UltrakULL translation browser from
# flagcdn.com and packs it into UltrakULL/flags/flags.zip, which is committed to
# the repository and embedded into the mod DLL at build time.
#
# Source of the artwork: https://flagcdn.com (public domain).
# Size: w40 (40 px wide PNG, ~1-2 KB per flag) - plenty for the ~20-24 px card
# overlay, tiny enough to embed all ~254 flags (~300 KB total).
#
# This script is NOT part of the mod or its build: it is run once when the flag
# set needs to be refreshed. The mod itself never downloads flags at runtime.
#
# Usage:  powershell -ExecutionPolicy Bypass -File tools\fetch-flags.ps1

$ErrorActionPreference = 'Stop'

# All 249 officially assigned ISO 3166-1 alpha-2 codes...
$codes = @(
    'ad','ae','af','ag','ai','al','am','ao','ar','as','at','au','aw','ax','az',
    'ba','bb','bd','be','bf','bg','bh','bi','bj','bl','bm','bn','bo','bq','br','bs','bt','bv','bw','by','bz',
    'ca','cc','cd','cf','cg','ch','ci','ck','cl','cm','cn','co','cr','cu','cv','cw','cx','cy','cz',
    'de','dj','dk','dm','do','dz',
    'ec','ee','eg','eh','er','es','et',
    'fi','fj','fk','fm','fo','fr',
    'ga','gb','gd','ge','gf','gg','gh','gi','gl','gm','gn','gp','gq','gr','gs','gt','gu','gw','gy',
    'hk','hm','hn','hr','ht','hu',
    'id','ie','il','im','in','io','iq','ir','is','it',
    'je','jm','jo','jp',
    'ke','kg','kh','ki','km','kn','kp','kr','kw','ky','kz',
    'la','lb','lc','li','lk','lr','ls','lt','lu','lv','ly',
    'ma','mc','md','me','mf','mg','mh','mk','ml','mm','mn','mo','mp','mq','mr','ms','mt','mu','mv','mw','mx','my','mz',
    'na','nc','ne','nf','ng','ni','nl','no','np','nr','nu','nz',
    'om',
    'pa','pe','pf','pg','ph','pk','pl','pm','pn','pr','ps','pt','pw','py',
    'qa',
    're','ro','rs','ru','rw',
    'sa','sb','sc','sd','se','sg','sh','si','sj','sk','sl','sm','sn','so','sr','ss','st','sv','sx','sy','sz',
    'tc','td','tf','tg','th','tj','tk','tl','tm','tn','to','tr','tt','tv','tw','tz',
    'ua','ug','um','us','uy','uz',
    'va','vc','ve','vg','vi','vn','vu',
    'wf','ws',
    'ye','yt',
    'za','zm','zw'
)

# ...plus Kosovo (user-assigned, widely used) and the UK constituent flags.
$extraCodes = @('xk', 'gb-eng', 'gb-nir', 'gb-sct', 'gb-wls')

$allCodes = $codes + $extraCodes

$rootDir = Split-Path -Parent $PSScriptRoot
$flagsDir = Join-Path $rootDir 'UltrakULL\flags'
$pngDir = Join-Path $flagsDir 'png'
$zipPath = Join-Path $flagsDir 'flags.zip'

if (Test-Path $pngDir) { Remove-Item -Recurse -Force $pngDir }
New-Item -ItemType Directory -Path $pngDir | Out-Null

$failed = @()
foreach ($code in $allCodes) {
    $url = "https://flagcdn.com/w40/$code.png"
    $target = Join-Path $pngDir "$code.png"
    try {
        Invoke-WebRequest -Uri $url -OutFile $target -UseBasicParsing -TimeoutSec 30
        Write-Host "OK  $code"
    }
    catch {
        $failed += $code
        Write-Warning "FAIL $code : $($_.Exception.Message)"
    }
}

if ($failed.Count -gt 0) {
    Write-Warning "Failed codes: $($failed -join ', ')"
}

# Pack every png into flags.zip with flat "<code>.png" entry names.
if (Test-Path $zipPath) { Remove-Item -Force $zipPath }
Compress-Archive -Path (Join-Path $pngDir '*.png') -DestinationPath $zipPath -CompressionLevel Optimal

$count = (Get-ChildItem $pngDir -Filter *.png).Count
$sizeKb = [math]::Round((Get-Item $zipPath).Length / 1KB)
Write-Host ""
Write-Host "Fetched $count flags; packed into $zipPath ($sizeKb KB)."
Write-Host "The png folder is intermediate output and can be deleted."

if ($failed.Count -gt 0) { exit 1 }
