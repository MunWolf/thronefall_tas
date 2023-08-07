try {
    $sln_path = $Args[0]
    $input_path = $Args[1]

    $config = Get-Content (Join-Path $sln_path install.cfg) | ConvertFrom-StringData

    function CopyDll {
        param (
            $dll
        )

        Write-Output "Installing $(Join-Path $config.InstallPath $dll)"
        $null = New-Item -ItemType File -Path (Join-Path $config.InstallPath $dll) -Force
        $null = Copy-Item (Join-Path $input_path $dll) (Join-Path $config.InstallPath $dll) -Force
    }

    $dlls = (
        'ThronefallTAS.dll',
        'UniverseLib.Mono.dll'
    )

    $dlls | ForEach-Object {
        CopyDll -dll $_
    }
} catch {
    throw 1
}
