# job-shop-collection-api
API of the [Job Shop Collection website](https://job-shop-collection.michael-yin.net).

## How to run locally
1. Clone repo
2. Open job-shop-collection-api.sln in Visual Studio 2019 or above, with IIS Express and localDb installation
3. Create database with localDb
    1. In Package Manager Console
    2. Have `job-shop-collection-api` as startup project
    3. Have `job-shop-collection-api.Data` as default project of console
    4. Run `Update-Database`
4. Run (F5) with IIS Express

The api is now running locally, make calls e.g. with postman or browser to e.g.
```
GET http://localhost:55758/api/job-sets
```
Check /job-shop-collection-api/Properties/launchSettings.json for the URL.\
Check the controller for API endpoints and request/response format.

### Run locally with React app job-shop-collection-web

Serve the React app locally with `"proxy": "https://localhost:44383"` in `package.json`.

CORS is set to allow any origin in development, to prevent problems of serving API and react app separately.

## Hosted on Linode
The solution `job-shop-collection-api.sln` is hosted with on a Linode job-shop-collection-api.

The database is hosted with SQL Server 2019 Express Edition on a Linode job-shop-collection-database.

![Current Linode setup](JobShopCollection_Linodes_Current_Setup.svg)

<details>
<Summary>Alternative Setup (Not in use)</summary>
To have HTTPS between web and api, we could add a Nginx reverse proxy in front of the Api application, so that it is easy to configure SSL certificates in Nginx configurations.

Using Nginx for the certificates would be easier than keeping the Api application's Kestrel Server as the public facing Edge Server.

![Alternative Linode setup](JobShopCollection_Linodes_Alternative_Setup.svg)

### SSL certificate for https from reverse proxy to api server
1. generate rootCA.key
```
openssl genrsa -out rootCA.key 4096
```

2. generate rootCA.crt
```
openssl req -x509 -new -nodes -key rootCA.key -sha256 -days 36500 -out rootCA.crt
```

3. generate webproxy.key
```
openssl genrsa -out webproxy.key 2048
```

4. generate webproxy.csr
```
openssl req -new -key webproxy.key -out webproxy.csr
```
with `job-shop-collection.michael-yin.net` as Common Name

5. generate webproxy.crt
```
openssl x509 -req -in webproxy.csr -CA rootCA.crt -CAkey rootCA.key -CAcreateserial -out webproxy.crt -days 36500 -sha256
```

6. generate api.key
```
openssl genrsa -out api.key 2048
```

7. generate api.csr
```
openssl req -new -key api.key -out api.csr
```
with `job-shop-collection.michael-yin.net` as Common Name

8. generate api.crt
```
openssl x509 -req -in api.csr -CA rootCA.crt -CAkey rootCA.key -CAcreateserial -out api.crt -days 36500 -sha256
```
</details>

### Setup job-shop-collection-database Linode
- https://www.linode.com/docs/guides/getting-started/
    - Skip hostname and hosts file
- https://www.linode.com/docs/guides/securing-your-server/
- https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-ubuntu?view=sql-server-ver15
    - Follow through and install SQL Server 2019, choose express edition when asked
- https://stackoverflow.com/questions/1601186/sql-server-script-to-create-a-new-user\
    - Add user

Some commnads
```
// check status
systemctl status mssql-server --no-pager

// allow port in firewall
sudo ufw allow 1433

// check the network connection
nc -zv YOUR_SERVER_NAME_OR_IP 1433

// enter sql command mode for that user, and specify to use job-shop-collection database
sqlcmd -S . -U SA -P '<YourPassword>'
use [job-shop-collection]
```

The connection string is
```
Data Source=tcp:192.53.169.244,1433;Initial Catalog=job-shop-collection;Persist Security Info=False;User ID=jobshopadmin;Password={your_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;
```

#### Update database
The database is updated using the connection string. New migrations committed are continuously deployed in the Github Action of Linode job-shop-collection-api.

### Setup job-shop-collection-api Linode
- https://www.linode.com/docs/guides/getting-started/
    - Hostname job-shop-collection-api
    - in hosts file, associate the public ip addresses with the domain name job-shop-collection.michael-yin.net
- https://www.linode.com/docs/guides/securing-your-server/

In the the current setup, job-shop-collection-api does not have a reverse proxy in front of the ASP.NET Web Api Kestrel server.

- Follow https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-3.1

- Install .NET SDK 3.1 and ASP.NET Core Runtime 3.1 with commands from https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu#2004-

#### Continuous Deployment by Github Actions (Linode)

- Setup FolderProfile publish profile, and check-in the `FolderProfile.pubxml` file

- Check the Github actions file https://github.com/michaelyinopen/job-shop-collection-api/blob/main/.github/workflows/main_linode.yml
    - In step `build_and_update_database`, note that
        - set `runs-on: ubuntu-20.04`
        - publish with `PublishProfile=FolderProfile`
        - Updates database
    - In step `deploy`
        - rsync files to Linode
        - restarts the service
- These Github Secrets are used
    - LINODE_DIRECTORY
    - LINODE_HOST
    - LINODE_PORT
    - LINODE_SQL_CONNECTION_STRING
    - LINODE_SSH_PRIVATE_KEY
    - LINODE_USER

The Github Actions workflow will fail without the following setup.

#### Monitor the app with `systemd`
After the published files are copied to the directory in Linode, create the unit file `/etc/systemd/system/kestrel-job-shop-collection-api.service`. In the file add environment variables
- ConnectionStrings__JobShopCollectionConnectionString
    - generated with `systemd-escape "<value-to-escape>"`
- ASPNETCORE_URLS=http://*:5000
    - cannot use production https because missing certificate

Useful commands
```
// Check status
sudo systemctl status kestrel-job-shop-collection-api.service

// After changing the unit file
sudo systemctl daemon-reload

// Restart
sudo systemctl restart kestrel-job-shop-collection-api.service

// Check logs
sudo journalctl -u kestrel-job-shop-collection-api -r
```

#### Configure iptables to use port 80 and 443
```
// setup with these two commands
sudo iptables -t nat -A PREROUTING -p tcp --dport 80 -j REDIRECT --to-port 5000
sudo iptables -t nat -A PREROUTING -p tcp --dport 443 -j REDIRECT --to-port 5001

// Check with
sudo iptables -t nat --line-numbers -n -L

// Delete with
iptables -t nat -D PREROUTING <the number to delete>
```

#### Allow user to restart the systemd without password (for Github Actions)
```
sudo visudo -f /etc/sudoers.d/restartnopassword
```
This opens a utility to edit the file. Add the following line and save
```
michael ALL=NOPASSWD: /usr/bin/systemctl restart kestrel-job-shop-collection-api.service
```
The user is the same as LINODE_USER in Github Secrets.

Re-run the Github Actions workflow, and it should succeed.
