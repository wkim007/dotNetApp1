@echo off
rem Sets Tom Sawyer license environment variables for the current cmd.exe session.
rem Usage:
rem   1. Edit the values below.
rem   2. Run: set-tomsawyer-license.bat
rem   3. Start your app from the same command prompt.

set TS_LICENSE_PROTOCOL=http
set TS_LICENSE_HOST=https://server.licensing.tomsawyer.com/WRUV5HNWN0TLGS53WJ4E00STO
set TS_LICENSE_PORT=8080
set TS_LICENSE_PATH=/flexnet/services
set TS_LICENSE_NAME=Quest Software

echo Tom Sawyer license environment variables set:
echo   TS_LICENSE_PROTOCOL=%TS_LICENSE_PROTOCOL%
echo   TS_LICENSE_HOST=%TS_LICENSE_HOST%
echo   TS_LICENSE_PORT=%TS_LICENSE_PORT%
echo   TS_LICENSE_PATH=%TS_LICENSE_PATH%
echo   TS_LICENSE_NAME=%TS_LICENSE_NAME%
