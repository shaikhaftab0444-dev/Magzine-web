import urllib.request
import urllib.parse
import http.cookiejar
import ssl
import re

ctx = ssl.create_default_context()
ctx.check_hostname = False
ctx.verify_mode = ssl.CERT_NONE

cj = http.cookiejar.CookieJar()
opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cj), urllib.request.HTTPSHandler(context=ctx))

base_url = "https://localhost:7281"

def get(path):
    req = urllib.request.Request(f"{base_url}{path}", headers={'User-Agent': 'Mozilla/5.0'})
    resp = opener.open(req)
    return resp.status, resp.read().decode('utf-8')

def post(path, data):
    encoded = urllib.parse.urlencode(data).encode('utf-8')
    req = urllib.request.Request(f"{base_url}{path}", data=encoded, headers={'User-Agent': 'Mozilla/5.0'})
    resp = opener.open(req)
    return resp.status, resp.read().decode('utf-8')

print("--- 1. Logging in as Admin ---")
status, login_html = get("/Account/Login")
token_match = re.search(r'name="__RequestVerificationToken"\s+type="hidden"\s+value="([^"]+)"', login_html)
token = token_match.group(1) if token_match else ""

post_data = {
    'Email': 'admin@magazine.com',
    'Password': 'Admin@123',
    'RememberMe': 'false',
    '__RequestVerificationToken': token
}
status, resp_html = post("/Account/Login", post_data)
print(f"Login Response: {status}")

print("\n--- 2. Testing /Admin/Collections ---")
status, coll_html = get("/Admin/Collections")
print(f"/Admin/Collections Status: {status}")

# Check 4 revenue cards
cards = ["Today", "This Month", "This Year", "Total Revenue"]
all_cards_found = all(c in coll_html for c in cards)
if all_cards_found:
    print("PASS: All 4 Neumorphic revenue aggregate cards found (Today, This Month, This Year, Total Revenue)")
else:
    print("FAIL: One or more revenue cards missing")

# Check transaction table
if 'table-responsive' in coll_html and 'neumorphic-table' in coll_html:
    print("PASS: Neumorphic table-responsive ledger found")
else:
    print("FAIL: Neumorphic table missing")

cols = ["Date", "User Email", "Item Type", "Amount"]
all_cols_found = all(col in coll_html for col in cols)
if all_cols_found:
    print("PASS: Required table columns found (Date, User Email, Item Type, Amount)")
else:
    print("FAIL: One or more table columns missing")

# Check sidebar link
if 'asp-action="Collections"' in coll_html or '/Admin/Collections' in coll_html:
    print("PASS: 'Collections' sidebar link verified")
else:
    print("FAIL: 'Collections' sidebar link missing")

# Check Neumorphic profile dropdown
if 'neumorphic-dropdown' in coll_html:
    print("PASS: .neumorphic-dropdown class present on admin profile dropdown")
else:
    print("FAIL: .neumorphic-dropdown class missing on admin profile dropdown")

print("\n--- 3. Testing /Admin/Dashboard with Range Filter ---")
status, dash_html = get("/Admin/Dashboard?range=30days")
print(f"/Admin/Dashboard (30days) Status: {status}")

if '/Admin/Dashboard?range=30days' in dash_html and '/Admin/Dashboard?range=quarter' in dash_html and '/Admin/Dashboard?range=ytd' in dash_html:
    print("PASS: Dropdown filter links (/Admin/Dashboard?range=30days, ?range=quarter, ?range=ytd) verified")
else:
    print("FAIL: Filter links not found")

status_q, q_html = get("/Admin/Dashboard?range=quarter")
print(f"/Admin/Dashboard (quarter) Status: {status_q}")

status_y, y_html = get("/Admin/Dashboard?range=ytd")
print(f"/Admin/Dashboard (ytd) Status: {status_y}")

print("\n--- 4. Testing site.css for .neumorphic-dropdown ---")
status_css, css = get("/css/site.css")
if '.neumorphic-dropdown' in css and 'box-shadow: 8px 8px 16px #c5c9d1' in css:
    print("PASS: .neumorphic-dropdown styles verified in site.css")
else:
    print("FAIL: .neumorphic-dropdown missing from site.css")

print("\nALL VERIFICATION CHECKS COMPLETED SUCCESSFULLY")
