#Requires -Version 5.1
<#
.SYNOPSIS
    Verify FR-RCP-004 PUT /api/v1/recipes/{id} cases.
.DESCRIPTION
    401/404/422 verifiable anonymously. 200/409/403 need -Token (TV1 JWT).
.EXAMPLE
    powershell -ExecutionPolicy Bypass -File scripts/verify-recipe-update.ps1
    powershell -ExecutionPolicy Bypass -File scripts/verify-recipe-update.ps1 -Token "eyJ..."
#>
param(
    [string]$BaseUrl = "http://localhost:5000",
    [string]$Token = ""
)

$ErrorActionPreference = "Stop"
$script:Pass = 0
$script:Fail = 0

function Invoke-PutRecipe([string]$Id, [hashtable]$Body, [string]$AuthToken) {
    $headers = @{ "Content-Type" = "application/json" }
    if ($AuthToken -ne "") { $headers["Authorization"] = "Bearer $AuthToken" }
    $json = ($Body | ConvertTo-Json -Depth 5 -Compress)
    try {
        $res = Invoke-RestMethod -Uri "$BaseUrl/api/v1/recipes/$Id" -Method Put `
            -Headers $headers -Body $json -TimeoutSec 15
        return @{ Status = 200; Body = $res }
    } catch {
        $resp = $_.Exception.Response
        if ($null -eq $resp) { return @{ Status = -1; Body = $_.Exception.Message } }
        $code = [int]$resp.StatusCode
        $text = ""
        try {
            $sr = New-Object System.IO.StreamReader($resp.GetResponseStream())
            $text = $sr.ReadToEnd()
            $sr.Close()
        } catch { }
        return @{ Status = $code; Body = $text }
    }
}

function Assert-Status([string]$Name, [int]$Actual, [int]$Expected) {
    if ($Actual -eq $Expected) {
        Write-Host "PASS: $Name (HTTP $Actual)" -ForegroundColor Green
        $script:Pass++
    } else {
        Write-Host "FAIL: $Name (expected $Expected, got $Actual)" -ForegroundColor Red
        $script:Fail++
    }
}

# 0. Seed data: lay 1 recipe published + rowVersion
$list = Invoke-RestMethod -Uri "$BaseUrl/api/v1/recipes?page=1&pageSize=1" -TimeoutSec 15
if ($list.items.Count -eq 0 -and $list.Count -eq 0) { throw "DB chua seed: khong co recipe nao." }
$slug = $list.items[0].slug
if (-not $slug) { $slug = $list[0].slug }
$detail = Invoke-RestMethod -Uri ("$BaseUrl/api/v1/recipes/" + [uri]::EscapeDataString($slug)) -TimeoutSec 15
$realId = $detail.id
$rowVersion = $detail.rowVersion
$catId = $detail.categoryId
Write-Host "Mau: id=$realId slug=$slug"

$validBody = @{
    title = "Cap nhat verify ABCDEF"
    description = $detail.description
    instructions = $detail.instructions
    categoryId = $catId
    prepTimeMinutes = $detail.prepTimeMinutes
    cookTimeMinutes = $detail.cookTimeMinutes
    servings = $detail.servings
    difficulty = 1
    rowVersion = $rowVersion
}

# 1. Anonymous -> 401 (authz work 4)
$r = Invoke-PutRecipe $realId $validBody ""
Assert-Status "PUT anonymous -> 401" $r.Status 401

# 2. Sai id -> 404 (check ton tai chay truoc authz)
$r = Invoke-PutRecipe "00000000-0000-0000-0000-000000000000" $validBody $Token
Assert-Status "PUT sai id -> 404" $r.Status 404

# 3. Title qua ngan -> 422 (ValidationBehavior chay truoc handler)
$bad = $validBody.Clone(); $bad.title = "abc"; $bad.rowVersion = $rowVersion
$r = Invoke-PutRecipe $realId $bad ""
Assert-Status "PUT title ngan -> 422" $r.Status 422

# 4. RowVersion khong phai base64 -> 422
$bad2 = $validBody.Clone(); $bad2.rowVersion = "not-base64!!!"
$r = Invoke-PutRecipe $realId $bad2 ""
Assert-Status "PUT rowVersion xau -> 422" $r.Status 422

# 5-7. Can JWT (TV1)
if ($Token -eq "") {
    Write-Host "SKIP: 200/409/403 (chay lai voi -Token khi TV1 xong JWT)." -ForegroundColor Yellow
} else {
    $stale = $validBody.Clone(); $stale.rowVersion = "AAAAAAAAAAAAAAAAAAAAAA=="
    $r = Invoke-PutRecipe $realId $stale $Token
    Assert-Status "PUT rowVersion cu -> 409" $r.Status 409

    $r = Invoke-PutRecipe $realId $validBody $Token
    Assert-Status "PUT hop le -> 200" $r.Status 200

    Write-Host "NOTE: 403 can token cua user khac (khong phai chu/Admin) - kiem tra tay." -ForegroundColor Yellow
}

Write-Host "`nKet qua: $($script:Pass) PASS, $($script:Fail) FAIL"
if ($script:Fail -gt 0) { exit 1 }
