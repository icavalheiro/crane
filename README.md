# Crane

Crane is a home server tool designed to be runned as a system.d service that whats out your docker-compose containers and keeps them up to date.

You can define a list of paths to your docker-compose folders in /etc/crane/config.json with the following sintax:

```json
[
    {
        "path": "/var/html"
    },
    {
        "path": "/usr/admin/services",
        "fileName": "services.docker-compose.yml"
    },
    ...
]
```