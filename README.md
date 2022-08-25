# A55 Aws Extensions for .NET [WIP]

## `A55.Aws.SecretsManager`

Ains to read configuration keys from Aws SecretsManager into your app configuration

On your `Program.cs` just add

```cs
var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddEnvironmentVariables()
    .AddSecretManager();

```
---

### Getting Start

Set those keys on root of your `appsettings.json`

```json5
{
    "ReadSettingsFromSecretManager": true,
    "SecretManagerProjectKey": "appName",
    "ACCESS_KEY_ID_SHARED": "",
    "SECRET_ACCESS_KEY_SHARED": "",
    "SESSION_TOKEN_SHARED": "",
    "REGION_SHARED": "us-east-1",
}
```
is recommended to have an `appsettings` file per environment, like `appsettings.Staging.json` and `appsettings.Production.json`, and set `ReadSettingsFromSecretManager=true`  for non dev environments

** important ** : you dont need do set the aws credentials manually, if you add the `.AddEnvironmentVariables()` on the configuration *before* the `.AddSecretManager()` this extension will fill this credentials values from the environment variables with same name, which are:

```sh
ACCESS_KEY_ID_SHARED
SECRET_ACCESS_KEY_SHARED
SESSION_TOKEN_SHARED
REGION_SHARED
```
---

You also can set the appname on the configuration Extension:

```cs
builder.Configuration
    .AddEnvironmentVariables()
    .AddSecretManager("appName");

```

The appname is important to set which keys this extension will load in your configuration
It will load the configuration on this fixed key paths and subpaths:

```
/settings/shared
/settings/{projectName}/shared/
/settings/{projectName}/{envAlias}/
```

the `envAlias` is mapped from your `ASPNETCORE_ENVIRONMENT`:

```
Development => dev
Staging => stg
Production => prd
```

## Postgres ConnectionString

To load the database credentials you need to have a key on SecretsManager as `/settings/{projectName}/{envAlias}/db`, this extension will map the database credentials to a valid connection string on `ConnectionStrings:DefaultConnection`
The expected secret value fields to map the ConnectionString are:

```json
{
	"name": "db_app_env",
	"user": "app_db_user",
	"host": "db-datasources-env.a55.local",
	"port": "5432",
	"password": "#senhaSecreta@123"
}

```


