param
(
    [string] $SERVICE_VERSION,
    [string] $SAVEPATH
)

function Get-ParameterError
{
    $parameterError = $false
    
    if (-Not $SERVICE_VERSION)
    {
        Write-Host "-SERVICE_VERSION [string] parameter must be populated with a value"
        Write-Host "     for example: -SERVICE_VERSION `"1.0`""
        Write-Host ""
        $parameterError = $true
    } 

    if (-Not $SAVEPATH -Or -Not(Test-Path $SAVEPATH))
    {
        Write-Host "-SERVICE_VERSION [string] parameter must be populated with a value"
        Write-Host "     for example: -SAVEPATH `"C:\Temp\`""
        Write-Host ""
        $parameterError = $true
    } 

    return $parameterError
}

if ( (Get-ParameterError) )
{
   throw "Exiting due to parameter errors"
}

$servicename = 'reminderbotservice'
$fullservicename = $servicename + ':' + $SERVICE_VERSION
$fullsavepath = Join-Path $SAVEPATH  ($servicename + '.tar')

docker rmi $fullservicename
docker build --platform linux/arm64 --force-rm -t $fullservicename -f Dockerfile .
docker save $fullservicename -o $fullsavepath