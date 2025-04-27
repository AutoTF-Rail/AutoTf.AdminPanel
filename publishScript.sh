# This script is used on the server that hosts the central servers and admin panel.
dotnet publish -c Release -o out
docker login 172.17.0.3:5001
docker build -t 172.17.0.3:5001/repository/docker-autotf/autotf.adminpanel:latest .
docker push 172.17.0.3:5001/repository/docker-autotf/autotf.adminpanel:latest
