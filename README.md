# Crane

Crane is a home server tool designed to be runned as a system.d service that watches out your docker-compose containers and keeps them up to date.

You can define services through environment variables using the following syntax:

```bash
CRANE_BLOG_PATH=/var/www/html
CRANE_MINECRAFT_PATH=/var/game_servers/minecraft
CRANE_MEDIA_PATH=/usr/media/services
CRANE_MEDIA_FILE_NAME=services.docker-compose.yml
```

Rules:

- `CRANE_{SERVICE}_PATH` is required for each configured service
- `CRANE_{SERVICE}_FILE_NAME` is optional
- `{SERVICE}` is the service identifier used to pair the variables for the same compose project

Example systemd service:

```ini
[Unit]
Description=Crane Docker Compose watcher
After=network-online.target docker.service
Wants=network-online.target
Requires=docker.service

[Service]
Type=simple
ExecStart=/usr/bin/crane
Restart=always
RestartSec=10
Environment=CRANE_BLOG_PATH=/var/www/html
Environment=CRANE_MINECRAFT_PATH=/var/game_servers/minecraft
Environment=CRANE_MEDIA_PATH=/usr/media/services
Environment=CRANE_MEDIA_FILE_NAME=services.docker-compose.yml

[Install]
WantedBy=multi-user.target
```

The same example is available in [crane.service](crane.service).

To install it on a Linux host:

```bash
sudo cp crane.service /etc/systemd/system/crane.service
sudo systemctl daemon-reload
sudo systemctl enable --now crane.service
```

Crane is configured for Native AOT publishing and is built as a single executable file. A typical publish command is:

```bash
dotnet publish src/Crane/Crane.csproj -c Release -r linux-x64
```

The resulting binary can then be installed as `/usr/bin/crane`.