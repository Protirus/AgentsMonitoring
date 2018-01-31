@echo off

set ans=Altiris.NS
set adb=Altiris.Database

set gac=C:\Windows\Microsoft.NET\assembly\GAC_MSIL
set csc=@c:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe

if "%1"=="7.6" goto build-7.6
if "%1"=="7.5" goto build-7.5

:default build path
set build=-8.0

set ver1=v4.0_8.0.2298.0__d516cb311cfb6e4f

goto build

:build-7.6
set build=-7.6

set ver1=v4.0_7.6.1383.0__d516cb311cfb6e4f

goto build


:build-7.5
set build=-7.5

set gac=C:\Windows\Assembly\GAC_MSIL
set csc=@c:\Windows\Microsoft.NET\Framework\v2.0.50727\csc.exe

set ver1=7.5.3153.0__d516cb311cfb6e4f


:build
: Build AgentsMonitoring.exe
cmd /c %csc% /reference:%gac%\%ans%\%ver1%\%ans%.dll /reference:%gac%\%adb%\%ver1%\%adb%.dll /out:AgentsMonitoring%build%.exe AgentsMonitoring.cs APIWrapper.cs
