# Steam Price Map - Backend

Backendová část aplikace pro interaktivní vizualizaci regionálních cenových rozdílů her na platformě Steam.

## Požadavky

- .NET 8.0 SDK
- API klíč pro Exchange Rate API
- API klíč pro Steam API

## Konfigurace API klíčů
Aplikace je závislá na využívání externích API služeb a bez správného nastavení API klíčů nebude aplikace správně fungovat.

### Kde získat API klíče?

- **Steam Web API:** https://steamcommunity.com/dev/apikey
- **Exchange Rate API:** https://www.exchangerate-api.com/

### Pro vývoj
Vytvořte soubor `.env` v kořenovém adresáři projektu podle šablony `.env.example`, do které doplníte API klíče.


### Pro nasazení do produkce
V rámci serverového prostředí, na kterém bude aplikace fungovat, je zapotřebí vložit do proměnných systému následující hodnoty:
- `SteamApi__ApiKey=___váš_steam_api_klíč___`
- `CurrencyApi__ApiKey=___váš_exchangeRates_api_klíč___`

## Produkční build

```bash
make build
```

Alternativně:

```bash
dotnet build
```

Zkompiluje projekt, nainstaluje závislosti a vytvoří spustitelné DLL soubory ve složce . `bin/Debug/net8.0/`

## Vývojové prostředí
```bash
make run
```

Alternativně:

```bash
dotnet run
```

Spustí vývojový server na adrese `http://localhost:5000`.

Swagger dokumentace je k dispozici na adrese `http://localhost:5000/swagger`.