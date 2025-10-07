# Seed demo data for QuokkaPack by calling API endpoints
# This script creates a demo user and populates categories, items, and trips

param(
    [string]$ApiUrl = "https://localhost:7045",
    [string]$Email = "demo@quokkapack.com",
    [string]$Password = "Demo123!"
)

$ErrorActionPreference = "Stop"

Write-Host "===========================================================" -ForegroundColor Cyan
Write-Host "QuokkaPack Demo Data Seeder" -ForegroundColor Cyan
Write-Host "===========================================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Register the demo user
Write-Host "[1/4] Registering demo user ($Email)..." -ForegroundColor Yellow

try {
    $registerResponse = Invoke-RestMethod -Uri "$ApiUrl/api/auth/register" `
        -Method POST `
        -Body (@{ email = $Email; password = $Password } | ConvertTo-Json) `
        -ContentType "application/json" `
        -SkipCertificateCheck

    $token = $registerResponse.token
    Write-Host "  ✓ User registered successfully" -ForegroundColor Green
}
catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "  ⚠ User already exists, attempting login..." -ForegroundColor Yellow

        $loginResponse = Invoke-RestMethod -Uri "$ApiUrl/api/auth/login" `
            -Method POST `
            -Body (@{ email = $Email; password = $Password } | ConvertTo-Json) `
            -ContentType "application/json" `
            -SkipCertificateCheck

        $token = $loginResponse.token
        Write-Host "  ✓ Logged in successfully" -ForegroundColor Green
    }
    else {
        throw
    }
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Step 2: Create categories
Write-Host ""
Write-Host "[2/4] Creating categories..." -ForegroundColor Yellow

$categories = @(
    "Toiletries", "Clothing", "Electronics",
    "Outdoor Gear", "Snacks", "Documents"
)

$categoryIds = @{}

foreach ($categoryName in $categories) {
    $response = Invoke-RestMethod -Uri "$ApiUrl/api/categories" `
        -Method POST `
        -Headers $headers `
        -Body (@{ name = $categoryName } | ConvertTo-Json) `
        -SkipCertificateCheck

    $categoryIds[$categoryName] = $response.id
    Write-Host "  ✓ Created: $categoryName" -ForegroundColor Green
}

# Step 3: Create items for each category
Write-Host ""
Write-Host "[3/4] Creating items..." -ForegroundColor Yellow

$itemsData = @{
    "Toiletries" = @("Toothbrush", "Toothpaste", "Shampoo", "Deodorant", "Razor", "Face Wash", "Floss")
    "Clothing" = @("T-shirts", "Jeans", "Sweater", "Raincoat", "Socks", "Underwear", "Pajamas", "Hat")
    "Electronics" = @("Phone Charger", "Power Bank", "Headphones", "Laptop", "Kindle", "USB Cable")
    "Outdoor Gear" = @("Hiking Boots", "Tent", "Sleeping Bag", "Flashlight", "Water Bottle", "Backpack")
    "Snacks" = @("Granola Bars", "Trail Mix", "Jerky", "Fruit Snacks", "Protein Bars")
    "Documents" = @("Passport", "Boarding Pass", "Travel Insurance", "Itinerary", "ID Card")
}

$itemCount = 0
foreach ($categoryName in $itemsData.Keys) {
    $categoryId = $categoryIds[$categoryName]

    foreach ($itemName in $itemsData[$categoryName]) {
        Invoke-RestMethod -Uri "$ApiUrl/api/items" `
            -Method POST `
            -Headers $headers `
            -Body (@{ name = $itemName; categoryId = $categoryId } | ConvertTo-Json) `
            -SkipCertificateCheck | Out-Null

        $itemCount++
    }
}

Write-Host "  ✓ Created $itemCount items across $($categories.Count) categories" -ForegroundColor Green

# Step 4: Create sample trips
Write-Host ""
Write-Host "[4/4] Creating sample trips..." -ForegroundColor Yellow

$trips = @(
    @{ destination = "Tokyo"; startDate = "2025-04-10"; endDate = "2025-04-24" }
    @{ destination = "Yosemite"; startDate = "2025-06-01"; endDate = "2025-06-08" }
    @{ destination = "Paris"; startDate = "2025-07-15"; endDate = "2025-07-30" }
    @{ destination = "Banff"; startDate = "2025-09-10"; endDate = "2025-09-20" }
    @{ destination = "New York City"; startDate = "2025-12-20"; endDate = "2025-12-27" }
)

foreach ($trip in $trips) {
    Invoke-RestMethod -Uri "$ApiUrl/api/trips" `
        -Method POST `
        -Headers $headers `
        -Body ($trip | ConvertTo-Json) `
        -SkipCertificateCheck | Out-Null

    Write-Host "  ✓ Created trip: $($trip.destination)" -ForegroundColor Green
}

Write-Host ""
Write-Host "===========================================================" -ForegroundColor Cyan
Write-Host "✓ Demo data seeded successfully!" -ForegroundColor Green
Write-Host "===========================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Login credentials:" -ForegroundColor White
Write-Host "  Email:    $Email" -ForegroundColor White
Write-Host "  Password: $Password" -ForegroundColor White
Write-Host ""
