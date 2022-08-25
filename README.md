# A55 Aws Extensions for .NET [WIP]

## `A55.Aws.SecretsManager`

Ains to read configuration keys from Aws SecretsManager into your app configuration

On your `Program.cs` just add

```cs

```


It will load the configuration on this fixed key paths:

```
"/settings/shared"
"/settings/{projectName}/shared
"/settings/{projectName}/{envAlias}
```

the `envAlias` is mapped from your `ASPNETCORE_ENVIRONMENT`:

```
Development => dev
Staging => stg
Production => prd
```

