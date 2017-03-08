# Get Player by Gamertag

Get information about a player by gamertag.

## Request

See Common parameters and headers that are used by all requests related to the Leaderboard Building Block.

Method  | Request URI
------- | -----------
GET     | `/api/admin/players/{gamerTag}`

### Request Parameters
Name        | Required |   Type   | Description
------------|----------|----------|------------
gamerTag|Yes|String|Tag of the player

### JSON Body

Empty body

### Response

| Status Code | Description |
|-------------|-------------|
|200|Success|
|404|Player not found|

### JSON Body

```json
{
  "player": {
    "gamertag": "string",
    "country": "string",
    "customTag": "string"
  }
}
```

Element name        | Required  | Type       | Description
------------------- | --------- | ---------  | -----------
gamertag            | Yes       | String     | Tag of the player
country             | Yes       | String     | Country of the player
customtag           | No        | String     | Custom tag
