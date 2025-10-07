#!/bin/bash
# Seed demo data for QuokkaPack by calling API endpoints
# This script creates a demo user and populates categories, items, and trips

API_URL="${1:-https://localhost:7045}"
EMAIL="${2:-demo@quokkapack.com}"
PASSWORD="${3:-Demo123!}"

set -e

echo "==========================================================="
echo "QuokkaPack Demo Data Seeder"
echo "==========================================================="
echo ""

# Step 1: Register the demo user
echo "[1/4] Registering demo user ($EMAIL)..."

REGISTER_RESPONSE=$(curl -k -s -w "\n%{http_code}" -X POST "$API_URL/api/auth/register" \
    -H "Content-Type: application/json" \
    -d "{\"email\":\"$EMAIL\",\"password\":\"$PASSWORD\"}")

HTTP_CODE=$(echo "$REGISTER_RESPONSE" | tail -n1)
RESPONSE_BODY=$(echo "$REGISTER_RESPONSE" | sed '$d')

if [ "$HTTP_CODE" = "200" ] || [ "$HTTP_CODE" = "201" ]; then
    TOKEN=$(echo "$RESPONSE_BODY" | grep -o '"token":"[^"]*' | grep -o '[^"]*$')
    echo "  ✓ User registered successfully"
elif [ "$HTTP_CODE" = "400" ]; then
    echo "  ⚠ User already exists, attempting login..."

    LOGIN_RESPONSE=$(curl -k -s -X POST "$API_URL/api/auth/login" \
        -H "Content-Type: application/json" \
        -d "{\"email\":\"$EMAIL\",\"password\":\"$PASSWORD\"}")

    TOKEN=$(echo "$LOGIN_RESPONSE" | grep -o '"token":"[^"]*' | grep -o '[^"]*$')
    echo "  ✓ Logged in successfully"
else
    echo "  ✗ Registration failed with HTTP $HTTP_CODE"
    exit 1
fi

# Step 2: Create categories
echo ""
echo "[2/4] Creating categories..."

CATEGORIES=("Toiletries" "Clothing" "Electronics" "Outdoor Gear" "Snacks" "Documents")
declare -A CATEGORY_IDS

for CATEGORY in "${CATEGORIES[@]}"; do
    RESPONSE=$(curl -k -s -X POST "$API_URL/api/categories" \
        -H "Authorization: Bearer $TOKEN" \
        -H "Content-Type: application/json" \
        -d "{\"name\":\"$CATEGORY\"}")

    ID=$(echo "$RESPONSE" | grep -o '"id":[0-9]*' | grep -o '[0-9]*$')
    CATEGORY_IDS["$CATEGORY"]=$ID
    echo "  ✓ Created: $CATEGORY"
done

# Step 3: Create items for each category
echo ""
echo "[3/4] Creating items..."

create_items() {
    local CATEGORY="$1"
    shift
    local ITEMS=("$@")
    local CATEGORY_ID="${CATEGORY_IDS[$CATEGORY]}"

    for ITEM in "${ITEMS[@]}"; do
        curl -k -s -X POST "$API_URL/api/items" \
            -H "Authorization: Bearer $TOKEN" \
            -H "Content-Type: application/json" \
            -d "{\"name\":\"$ITEM\",\"categoryId\":$CATEGORY_ID}" > /dev/null
    done
}

create_items "Toiletries" "Toothbrush" "Toothpaste" "Shampoo" "Deodorant" "Razor" "Face Wash" "Floss"
create_items "Clothing" "T-shirts" "Jeans" "Sweater" "Raincoat" "Socks" "Underwear" "Pajamas" "Hat"
create_items "Electronics" "Phone Charger" "Power Bank" "Headphones" "Laptop" "Kindle" "USB Cable"
create_items "Outdoor Gear" "Hiking Boots" "Tent" "Sleeping Bag" "Flashlight" "Water Bottle" "Backpack"
create_items "Snacks" "Granola Bars" "Trail Mix" "Jerky" "Fruit Snacks" "Protein Bars"
create_items "Documents" "Passport" "Boarding Pass" "Travel Insurance" "Itinerary" "ID Card"

echo "  ✓ Created 43 items across 6 categories"

# Step 4: Create sample trips
echo ""
echo "[4/4] Creating sample trips..."

TRIPS=(
    "Tokyo|2025-04-10|2025-04-24"
    "Yosemite|2025-06-01|2025-06-08"
    "Paris|2025-07-15|2025-07-30"
    "Banff|2025-09-10|2025-09-20"
    "New York City|2025-12-20|2025-12-27"
)

for TRIP in "${TRIPS[@]}"; do
    IFS='|' read -r DESTINATION START_DATE END_DATE <<< "$TRIP"

    curl -k -s -X POST "$API_URL/api/trips" \
        -H "Authorization: Bearer $TOKEN" \
        -H "Content-Type: application/json" \
        -d "{\"destination\":\"$DESTINATION\",\"startDate\":\"$START_DATE\",\"endDate\":\"$END_DATE\"}" > /dev/null

    echo "  ✓ Created trip: $DESTINATION"
done

echo ""
echo "==========================================================="
echo "✓ Demo data seeded successfully!"
echo "==========================================================="
echo ""
echo "Login credentials:"
echo "  Email:    $EMAIL"
echo "  Password: $PASSWORD"
echo ""
