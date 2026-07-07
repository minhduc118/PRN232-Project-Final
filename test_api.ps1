$ErrorActionPreference = "Stop"
$baseUrl = "http://localhost:5000/api"

try {
    # 1. Login to get token
    $loginPayload = @{
        email = "customer@sportcourt.vn"
        password = "customer123"
    } | ConvertTo-Json
    $loginRes = Invoke-WebRequest -Uri "$baseUrl/auth/login" -Method Post -Body $loginPayload -ContentType "application/json" -UseBasicParsing
    $loginData = $loginRes.Content | ConvertFrom-Json
    $token = $loginData.data.accessToken

    # 2. Call Create Tournament API
    $tournamentPayload = @"
    {
      "TournamentName": "Test Tournament",
      "Description": "Test",
      "BookingDate": "2026-07-15T00:00:00",
      "CourtSelections": [
        {
          "CourtId": 1,
          "SlotIds": [1, 2]
        }
      ],
      "Services": [],
      "PromotionCode": "",
      "Note": ""
    }
"@
    $res = Invoke-WebRequest -Uri "$baseUrl/bookings/tournament" -Method Post -Body $tournamentPayload -ContentType "application/json" -Headers @{ Authorization = "Bearer $token" } -UseBasicParsing
    "Success! Status Code: $($res.StatusCode) | Response: $($res.Content)" | Out-File -FilePath "api_test_result.txt"
}
catch {
    $errResp = $_.Exception.Response
    if ($errResp -ne $null) {
        $reader = New-Object System.IO.StreamReader($errResp.GetResponseStream())
        $errBody = $reader.ReadToEnd()
        "Failed! Status Code: $($errResp.StatusCode.value__) | Error: $errBody" | Out-File -FilePath "api_test_result.txt"
    } else {
        "Failed! Exception: $($_.Exception.Message)" | Out-File -FilePath "api_test_result.txt"
    }
}
