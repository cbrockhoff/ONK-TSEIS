param([switch]$builddbs, [switch]$buildservices, [switch]$deploy, [switch]$clean)

function deploydatabase
{
	param($name, $ip)

	Write-Host "Deploying $name @ $ip" -foreground "magenta"
	kubectl-delete -name $name -deployment -service
	kubectl-run -name $name -port 5432
	kubectl-expose -name $name -ip $ip
}

function deployrestapi
{
	param($name, $ip, $replicas=1)

	Write-Host "Deploying $name @ $ip" -foreground "magenta"
	kubectl-delete -name $name -deployment -service
	kubectl-run -name $name -port 5000 -replicas $replicas
	kubectl-expose -name $name -ip $ip
}

function deployexternal
{
	param($name, $replicas=1)

	Write-Host "Deploying $name" -foreground "magenta"
	kubectl-delete -name $name -deployment -service
	kubectl-run -name $name -port 80 -replicas $replicas
	kubectl-expose -name $name -external
}

function deployservice
{
	param($name, $replicas=1)

	Write-Host "Deploying $name" -foreground "magenta"
	kubectl-delete -name $name -deployment
	kubectl-run -name $name -replicas $replicas
}

function kubectl-delete
{
	param($name, [switch]$deployment, [switch]$service)
	
	if($deployment)
	{
		$i = 0
		while($i -lt 15)
		{
			$s = ((kubectl delete deployment $name) 2>&1).ToString()
			if($s.Contains('connectex') -or $s.Contains('unexpected EOF'))
			{
				Start-Sleep -s 1
			}
			else
			{
				if(!$s.Contains('NotFound'))
				{
					Write-Host $s
				}
				break
			}
			$i++
		}
	}
	if($service)
	{
		$i = 0
		while($i -lt 15)
		{
			$s = ((kubectl delete service $name) 2>&1).ToString()
			if($s.Contains('connectex') -or $s.Contains('unexpected EOF'))
			{
				Start-Sleep -s 1
			}
			else
			{
				if(!$s.Contains('NotFound'))
				{
					Write-Host $s
				}
				break
			}
			$i++
		}
	}
}

function kubectl-run
{
	param($name, $replicas = 1, $port = 0)
	$i = 0
	while($i -lt 15)
	{
		if($port -ne 0)
		{
			$s = ((kubectl run $name --image=$name --replicas=$replicas --port=$port --image-pull-policy=Never) 2>&1).ToString()
		}
		else
		{
			$s = ((kubectl run $name --image=$name --replicas=$replicas --image-pull-policy=Never) 2>&1).ToString()
		}
		if($s.Contains('connectex' -or $s.Contains('unexpected EOF')))
		{
			Start-Sleep -s 1
		}
		else
		{
			Write-Host $s
			break
		}
		$i++
	}
}

function kubectl-expose
{
	param($name, [switch]$external, $ip)
	$i = 0
	while($i -lt 15)
	{
		if($external)
		{
			$s = ((kubectl expose deployment $name --type="NodePort") 2>&1).ToString()
		}
		else
		{
			$s = ((kubectl expose deployment $name --cluster-ip=$ip) 2>&1).ToString()
		}
		if($s.Contains('connectex') -or $s.Contains('unexpected EOF'))
		{
			Start-Sleep -s 1
		}
		else
		{
			Write-Host $s
			break
		}
		$i++
	}
}

if(!$builddbs -and !$buildservices -and !$deploy -and !$clean) {return}

if($clean)
{
	Write-Host "Ignore '(NotFound)' errors from Kubectl - removing services and deployments even though they don't exist" -foreground "red"
	Write-Host "Deleting services..." -foreground "magenta"
	kubectl delete service ownercontrol-restapi; kubectl delete deployment ownercontrol-restapi
	kubectl delete service provider-restapi; kubectl delete deployment provider-restapi
	kubectl delete service requester-restapi; kubectl delete deployment requester-restapi
	kubectl delete service apigateway-restapi; kubectl delete deployment apigateway-restapi
	kubectl delete deployment broker-service
	kubectl delete deployment tobintaxer-service

	Write-Host "Deleting databases..." -foreground "magenta"
	kubectl delete service logging-db; kubectl delete deployment logging-db
	kubectl delete service broker-db; kubectl delete deployment broker-db
	kubectl delete service ownercontrol-db; kubectl delete deployment ownercontrol-db
	kubectl delete service requester-db; kubectl delete deployment requester-db
	kubectl delete service apigateway-db; kubectl delete deployment apigateway-db

	Write-Host "Finished cleaning... Exiting."
	return
}

$oldPath = pwd
cd $PSScriptRoot

minikube docker-env | invoke-expression

if($builddbs)
{
	Write-Host "Building docker image apigateway-db"
	docker build -t apigateway-db .\ApiGateway\Persistence

	Write-Host "Building docker image broker-db"
	docker build -t broker-db .\Broker\Persistence

	Write-Host "Building docker image logging-db"
	docker build -t logging-db .\Logging\Persistence

	Write-Host "Building docker image ownercontrol-db"
	docker build -t ownercontrol-db .\OwnerControl\Persistence

	Write-Host "Building docker image requester-db"
	docker build -t requester-db .\Requester\Persistence
}

if($buildservices)
{
	$env:DOTNET_CLI_TELEMETRY_OPTOUT = 1 # because fuck Microsoft Telemetry

	Write-Host "Building broker-service" -foreground "magenta"
	dotnet publish .\Broker\Service -o obj\Docker\publish -v m
	docker build -t broker-service .\Broker\Service

	Write-Host "Building ownercontrol-restapi" -foreground "magenta"
	dotnet publish .\OwnerControl\RestAPI -o obj\Docker\publish -v m
	docker build -t ownercontrol-restapi .\OwnerControl\RestAPI

	Write-Host "Building provider-restapi" -foreground "magenta"
	dotnet publish .\Provider\RestAPI -o obj\Docker\publish -v m
	docker build -t provider-restapi .\Provider\RestAPI

	Write-Host "Building requester-restapi" -foreground "magenta"
	dotnet publish .\Requester\RestAPI -o obj\Docker\publish -v m
	docker build -t requester-restapi .\Requester\RestAPI

	Write-Host "Building tobintaxer-service" -foreground "magenta"
	dotnet publish .\TobinTaxer\Service -o obj\Docker\publish -v m
	docker build -t tobintaxer-service .\TobinTaxer\Service	
	
	Write-Host "Building apigateway-restapi" -foreground "magenta"
	minikube docker-env -u | invoke-expression
	docker build -t apigateway-restapi .\ApiGateway\RestAPI
	Write-Host "This might take a while... Pulling image from Docker host to local machine, and pushing it to Minikube" -foreground "magenta"
	docker save -o C:\docker_images\apigateway-restapi apigateway-restapi
	minikube docker-env | invoke-expression
	docker load -i C:\docker_images\apigateway-restapi
}

if($deploy)
{
	Write-Host "Assuming RabbitMQ is already running"

	deploydatabase -name "logging-db"      -ip "10.0.0.50"
	deploydatabase -name "broker-db"       -ip "10.0.0.90"
	deploydatabase -name "ownercontrol-db" -ip "10.0.0.91"
	deploydatabase -name "requester-db"    -ip "10.0.0.92"
	deploydatabase -name "apigateway-db"   -ip "10.0.0.93"

	Write-Host "Waiting 15 seconds for databases to be ready"
	Start-Sleep -s 15

	deployrestapi -name "ownercontrol-restapi" -ip "10.0.0.110"
	deployrestapi -name "provider-restapi"     -ip "10.0.0.111"
	deployrestapi -name "requester-restapi"    -ip "10.0.0.112"

	deployservice -name "broker-service"
	deployservice -name "tobintaxer-service"

	deployexternal -name "apigateway-restapi"
}

minikube docker-env -u | invoke-expression

cd $oldPath