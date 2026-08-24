# Container deployment

`ImageMapper.AppHost` via Aspire CLI controls Docker Compose artifact generation:

- `aspire publish`
- `aspire do prepare-compose --environment <Staging|Production>`
- `aspire deploy --environment <Staging|Production>`

Compose environment defaults are configured in `src/ImageMapper.AppHost/appsettings.<environment>.json` under `ComposeDefaults` instead of `appsettings.json` within the API project.
Additionally the ports the container images listen on can be configured here. These are the internal ports.

```json
{
  "ComposeDefaults": {
    "ImageFolders": [
        "/path/to/your/images1",
        "/path/to/your/images2"
    ],
    "ApiPort": 8081,
    "WebPort": 8080
  }
}
```

Replace the `CHANGE_ME_*` default values in the files with the folder paths for each deployment environment.

You will likely need to add a bind mount for the image folders to resolve them outside of the container, which you can do by creating a `docker-compose.override.yaml` file in `src\ImageMapper.AppHost\aspire-output`, for example:

```yaml
services:
  imagemapper-api:
    volumes:
      - "/path/to/your/images:/data/images:ro"
```

Make sure relevant permissions exist for the source folder. When using Docker Desktop on Windows, you may need to add the source folder to the list of shared drives in Docker Desktop settings.

### Issues

You also may need to start with the 'aspire do prepare-compose' command to generate the compose files, add the override file and then run
`docker compose --env-file .\.env.staging up -d --remove-orphans` from `src\ImageMapper.AppHost\aspire-output`.

Also may need to initially run `aspire deploy` to get all the images built correctly, then delete the container it creates and run the `docker compose up` command to create a container
picking up the override file. `aspire deploy` appears to fail to pick up the override, but `aspire do prepare-compose` fails to correctly tag images or create the dashboard image.

Issues with Aspire generating bind/volume mounts on project resources in the compose file from code, discussed here https://github.com/microsoft/aspire/issues/4359
