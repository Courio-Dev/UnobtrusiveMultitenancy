REM Create a 'GeneratedReports' folder if it does not exist
RMDIR /S /Q %~dp0GeneratedReports
if not exist "%~dp0GeneratedReports" mkdir "%~dp0GeneratedReports"
if not exist "%~dp0\GeneratedReports\artifacts\Coverage" mkdir "%~dp0\GeneratedReports\artifacts\Coverage"
 if not exist "%~dp0\GeneratedReports\ReportGenerator Output" mkdir "%~dp0\GeneratedReports\ReportGenerator Output"

REM Remove any previously created test output directories
REM CD %~dp0GeneratedReports
REM FOR /D /R %%X IN (%USERNAME%*) DO RD /S /Q "%%X"
REM FOR /D /R c:\FOLDERLOCATION %%X IN (*.tmp) DO RMDIR /S /Q "%%X"
 

REM Run the tests against the targeted output
call :RunOpenCoverUnitTestMetrics
 
REM Generate the report output based on the test results
if %errorlevel% equ 0 (
 call :RunReportGeneratorOutput
)
 
REM Launch the report
if %errorlevel% equ 0 (
 call :RunLaunchReport
)
exit /b %errorlevel%
 
:RunOpenCoverUnitTestMetrics
"%~dp0\tools\OpenCover.4.6.519\tools\OpenCover.Console.exe" ^
-target:"C:\Program Files\dotnet\dotnet.exe" ^
-targetargs:"test \"%~dp0\test\PuzzleCMS.UnitsTests\PuzzleCMS.UnitsTests.csproj\"" ^
-filter:"+[*]* -[*.UnitsTests]*" ^
-skipautoprops ^
-oldStyle ^
-mergeoutput ^
-register:user ^
-mergebyhash ^
-output:"%~dp0\GeneratedReports\artifacts\Coverage\coverage.xml"
exit /b %errorlevel%
 
:RunReportGeneratorOutput
"%~dp0\tools\ReportGenerator.3.1.2\tools\ReportGenerator.exe" ^
-reports:"%~dp0\GeneratedReports\artifacts\Coverage\coverage.xml" ^
-targetdir:"%~dp0\GeneratedReports\ReportGenerator Output"
exit /b %errorlevel%
 
:RunLaunchReport
start "report" "%~dp0\GeneratedReports\ReportGenerator Output\index.htm"
exit /b %errorlevel%

pause