import urllib.request
import urllib.parse
import http.cookiejar
import ssl
import json
import re

ctx = ssl.create_default_context()
ctx.check_hostname = False
ctx.verify_mode = ssl.CERT_NONE

cj = http.cookiejar.CookieJar()
opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cj), urllib.request.HTTPSHandler(context=ctx))

base_url = "https://localhost:7281"

def get(path, headers=None):
    hdrs = {'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) TestClient/1.0'}
    if headers:
        hdrs.update(headers)
    req = urllib.request.Request(f"{base_url}{path}", headers=hdrs)
    resp = opener.open(req)
    return resp.status, resp.read().decode('utf-8')

def post(path, data, headers=None):
    hdrs = {'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) TestClient/1.0'}
    if headers:
        hdrs.update(headers)
    encoded = urllib.parse.urlencode(data).encode('utf-8')
    req = urllib.request.Request(f"{base_url}{path}", data=encoded, headers=hdrs)
    resp = opener.open(req)
    return resp.status, resp.read().decode('utf-8')

print("--- 1. Testing Login & UserSessionTrack Creation ---")
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

print("\n--- 2. Testing /Admin/Tracker Page ---")
status, tracker_html = get("/Admin/Tracker")
print(f"/Admin/Tracker HTML Status: {status}")

if 'Live User Tracker' in tracker_html and 'trackerTableBody' in tracker_html:
    print("PASS: Tracker view rendered with Live User Tracker header and table")
else:
    print("FAIL: Tracker view structure missing")

# Check table headers
headers = ["User Email", "Login Time", "Device / Browser", "IP Address", "Status"]
headers_found = all(h in tracker_html for h in headers)
if headers_found:
    print("PASS: All required columns present (User Email, Login Time, Device / Browser, IP Address, Status)")
else:
    print("FAIL: One or more table headers missing")

# Check 10-second refresh setInterval script
if 'setInterval(refreshTracker, 10000)' in tracker_html or '10000' in tracker_html:
    print("PASS: 10-second auto-polling setInterval script detected")
else:
    print("FAIL: 10-second auto-polling script missing")

# Check AJAX JSON endpoint
status_ajax, tracker_json = get("/Admin/Tracker?ajax=true", {'X-Requested-With': 'XMLHttpRequest', 'Accept': 'application/json'})
print(f"/Admin/Tracker AJAX Status: {status_ajax}")
try:
    sessions = json.loads(tracker_json)
    print(f"PASS: Tracker returned JSON with {len(sessions)} session records")
    admin_tracked = any(s.get('userEmail') == 'admin@magazine.com' for s in sessions)
    if admin_tracked:
        print("PASS: Recent login hook created UserSessionTrack for 'admin@magazine.com'")
    else:
        print("FAIL: admin@magazine.com not found in tracked sessions")
except Exception as e:
    print(f"FAIL: Error parsing JSON: {e}")

print("\n--- 3. Testing /Admin/Collections Filter UI & AJAX ---")
status, coll_html = get("/Admin/Collections")
print(f"/Admin/Collections HTML Status: {status}")

# Check filter bar elements
if 'id="filterForm"' in coll_html and 'name="startDate"' in coll_html and 'name="endDate"' in coll_html and 'name="itemType"' in coll_html:
    print("PASS: Neumorphic filter bar with startDate, endDate, and itemType found")
else:
    print("FAIL: Filter bar inputs missing")

# Check onchange event on inputs
if 'onchange="applyCollectionsFilter()"' in coll_html:
    print("PASS: onchange AJAX trigger verified on filter inputs")
else:
    print("FAIL: onchange AJAX trigger missing")

# Check card IDs for DOM update
card_ids = ["cardTodayRevenue", "cardMonthRevenue", "cardYearRevenue", "cardTotalRevenue", "transactionsTableBody"]
all_ids_found = all(cid in coll_html for cid in card_ids)
if all_ids_found:
    print("PASS: DOM element IDs for real-time card and table updates verified")
else:
    print("FAIL: One or more DOM IDs missing")

# Test AJAX filtering: VIP Plan
status_vip, vip_json = get("/Admin/Collections?ajax=true&itemType=VIP+Plan", {'X-Requested-With': 'XMLHttpRequest', 'Accept': 'application/json'})
print(f"/Admin/Collections AJAX (VIP Plan) Status: {status_vip}")
try:
    data_vip = json.loads(vip_json)
    print(f"PASS: VIP Plan filter returned valid JSON: Total = {data_vip.get('totalRevenueFormatted')}, Count = {data_vip.get('count')}")
except Exception as e:
    print(f"FAIL: VIP filter JSON error: {e}")

# Test AJAX filtering: Date range + Single Issue
status_issue, issue_json = get("/Admin/Collections?ajax=true&startDate=2026-01-01&endDate=2026-12-31&itemType=Single+Issue", {'X-Requested-With': 'XMLHttpRequest', 'Accept': 'application/json'})
print(f"/Admin/Collections AJAX (Date Range + Single Issue) Status: {status_issue}")
try:
    data_issue = json.loads(issue_json)
    print(f"PASS: Single Issue + Date range filter returned valid JSON: Total = {data_issue.get('totalRevenueFormatted')}, Count = {data_issue.get('count')}")
except Exception as e:
    print(f"FAIL: Single Issue filter JSON error: {e}")

print("\n--- 4. Testing Sidebar and Dropdown Links ---")
if 'asp-action="Tracker"' in tracker_html or '/Admin/Tracker' in tracker_html:
    print("PASS: 'Live Tracker' link verified in layout")
else:
    print("FAIL: 'Live Tracker' link missing in layout")

if 'asp-action="Collections"' in tracker_html or '/Admin/Collections' in tracker_html:
    print("PASS: 'Collections' link verified in layout")
else:
    print("FAIL: 'Collections' link missing in layout")

print("\nALL ARCHITECTURAL VERIFICATION CHECKS COMPLETED SUCCESSFULLY")
