# Steam Price Map API

Backend API pro vyhledávání Steam her a získávání cenových informací napříč různými regiony a měnami.

## 🚀 Funkce

- **Vyhledávání her** - Rychlé vyhledávání v databázi Steam her
- **Informace o hrách** - Detailní informace o hrách včetně vývojářů, vydavatelů, kategorií
- **Mezinárodní ceny** - Ceny her ve více než 40 zemích/regionech
- **Konverze měn** - Automatická konverze cen do libovolné měny
- **Caching** - Inteligentní cachování pro rychlé odpovědi
- **Background aktualizace** - Automatická aktualizace dat každou půlnoc
- **Input validace** - Kompletní validace všech vstupních parametrů

## 📋 Požadavky

- .NET 8.0 SDK
- API klíč pro Exchange Rate API (pro konverzi měn)

## 🔧 Instalace a spuštění

1. **Klonování repozitáře**
```bash
git clone <repository-url>
cd SOC_SteamPM-BE
```

2. **Konfigurace API klíčů** (DŮLEŽITÉ)
```bash
# Pro development použijte User Secrets
dotnet user-secrets init
dotnet user-secrets set "CurrencyApi:ApiKey" "your-api-key-here"
```

3. **Spuštění aplikace**
```bash
dotnet restore
dotnet build
dotnet run
```

4. **Otevřete Swagger UI**
```
https://localhost:5001/swagger
```

## 📚 API Endpointy

### 1. Vyhledávání her
```
GET /api/GameSearch/searchGame?search={searchTerm}
```

**Parametry:**
- `search` (string, 1-200 znaků) - Hledaný termín

**Příklad:**
```bash
GET /api/GameSearch/searchGame?search=Counter-Strike
```

**Odpověď:**
```json
{
  "games": [
    {
      "appId": 730,
      "name": "Counter-Strike: Global Offensive"
    }
  ],
  "totalCount": 1,
  "searchTerm": "Counter-Strike"
}
```

### 2. Informace o hře s cenami
```
GET /api/PriceMap/game/{appId}?currency={currencyCode}
```

**Parametry:**
- `appId` (int, > 0) - Steam Application ID
- `currency` (string, 3 znaky, uppercase) - Kód měny (např. EUR, USD, CZK)

**Příklad:**
```bash
GET /api/PriceMap/game/730?currency=CZK
```

**Odpověď:**
```json
{
  "name": "Counter-Strike: Global Offensive",
  "appId": 730,
  "shortDescription": "...",
  "releaseDate": "2012-08-21",
  "developers": ["Valve"],
  "publishers": ["Valve"],
  "categories": ["Multi-player", "Steam Achievements"],
  "genres": ["Action", "Free to Play"],
  "headerImage": "https://...",
  "priceOverview": {
    "USD": {
      "discountPercent": 0,
      "initial": 0.00,
      "final": 0.00,
      "convertedInitial": 0.00,
      "convertedFinal": 0.00
    }
  }
}
```

### 3. Status engine
```
GET /api/Engine/status
```

**Odpověď:**
```json
{
  "status": "ready",
  "lastUpdated": "2024-01-15T10:30:00",
  "gameCount": 125000,
  "errorMessage": null,
  "updateAttempts": 0
}
```

### 4. Manuální refresh dat
```
GET /api/Engine/refresh
```

## ✅ Validace vstupů

API obsahuje kompletní validaci všech vstupních parametrů:

### AppId validace
- ✅ Musí být kladné číslo (> 0)
- ❌ `0`, `-1` jsou neplatné

### Currency validace
- ✅ Přesně 3 znaky
- ✅ Pouze velká písmena (A-Z)
- ✅ Příklady: `EUR`, `USD`, `CZK`, `GBP`
- ❌ `eur` (lowercase), `EURO` (příliš dlouhé), `E$R` (speciální znaky)

### Search term validace
- ✅ 1-200 znaků
- ❌ Prázdný string vrací prázdné výsledky (ne chybu)
- ❌ Více než 200 znaků je odmítnuto

### Příklad chybové odpovědi
```json
{
  "error": "Validation Failed",
  "message": "Invalid request parameters.",
  "errors": [
    "Currency code must be exactly 3 uppercase letters (e.g., EUR, USD, CZK)."
  ]
}
```

## 🏗️ Architektura

```
Controllers/          # API endpointy s validací
├── PriceMapController.cs
├── GameSearchController.cs
└── EngineController.cs

Services/            # Business logika
├── PriceMap/
├── GameSearch/
├── Steam/
└── Currencies/

Managers/            # State management
└── EngineDataManager.cs

Middleware/          # Custom middleware
└── EngineStatusMiddleware.cs

Utils/              # Pomocné třídy
├── DataFactory.cs
└── ValidationConstants.cs

Models/             # Data modely
Exceptions/         # Custom výjimky
```

## 🔒 Bezpečnost

- ✅ **API klíče v User Secrets** - Nikdy v kódu!
- ✅ **Input validace** - Všechny vstupy jsou validovány
- ✅ **Rate limiting** - Ochrana proti abuse
- ✅ **Structured logging** - Bezpečné logování bez citlivých dat
- ✅ **Custom exceptions** - Kontrolované error handling

## 📊 Caching strategie

- **Herní data** - Cached po dobu 24 hodin
- **Měnové kurzy** - Cached po dobu 24 hodin
- **Seznam her** - V paměti, aktualizace každou půlnoc

## 🧪 Testování

1. Použijte soubor `SOC_SteamPM-BE-validation-tests.http` pro testování validace
2. Otevřete v Visual Studio nebo Rider
3. Spusťte jednotlivé requesty

## 🛠️ Konfigurace

Konfigurace v `appsettings.json`:

```json
{
  "SteamApi": {
    "AllGamesUrl": "https://api.steampowered.com/ISteamApps/GetAppList/v2/",
    "GameAllInfo": "https://store.steampowered.com/api/appdetails?appids={APPID}&cc={CC}&l=english",
    "GamePriceInfo": "https://store.steampowered.com/api/appdetails?appids={APPID}&cc={CC}&filters=price_overview"
  },
  "DataStorage": {
    "FolderPath": "startupData",
    "LoadDataFromFileOnStartup": false
  },
  "CurrencyApi": {
    "BaseUrl": "https://v6.exchangerate-api.com/v6/{API-KEY}/latest/"
  }
}
```

## 📝 Logování

Aplikace používá strukturované logování:
- `Information` - Normální operace
- `Warning` - Neočekávané situace (hra nenalezena, neplatná měna)
- `Error` - Chyby v operacích
- `Critical` - Kritické chyby (inicializace selhala)

## 🚦 Status kódy

- `200 OK` - Úspěšný request
- `400 Bad Request` - Neplatné vstupní parametry
- `404 Not Found` - Hra nenalezena
- `500 Internal Server Error` - Neočekávaná chyba serveru
- `503 Service Unavailable` - Služba se načítá/aktualizuje

## 🤝 Vývoj

### Přidání nové validace

1. Přidejte konstanty do `Utils/ValidationConstants.cs`
2. Použijte Data Annotations na parametrech controlleru
3. Přidejte XML dokumentaci
4. Otestujte pomocí HTTP requestů

## 📄 Licence

...