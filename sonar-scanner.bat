@echo off
setlocal EnableDelayedExpansion
GOTO :main

:sonar_scanner
echo First parameter is %~1
echo Second parameter is %~2
SonarScanner.MSBuild.exe begin ^
  /n:".net-core-client" ^
  /k:"net-core-client" ^
  /d:sonar.host.url="https://sonarqube.split-internal.com" ^
  /d:sonar.login=%SONAR_LOGIN% ^
  /d:sonar.ws.timeout="300" ^
  /d:sonar.links.ci="https://travis-ci.com/splitio/.net-core-client" ^
  /d:sonar.links.scm="https://github.com/splitio/.net-core-client" ^
  %*
EXIT /B 0

:main
IF NOT "%APPVEYOR_PULL_REQUEST_NUMBER%"=="" (
  echo Pull Request number %APPVEYOR_PULL_REQUEST_NUMBER%
  CALL :sonar_scanner ^
    /d:sonar.pullrequest.provider="GitHub", ^
    /d:sonar.pullrequest.github.repository="splitio/.net-core-client", ^
    /d:sonar.pullrequest.key=%APPVEYOR_PULL_REQUEST_NUMBER%, ^
    /d:sonar.pullrequest.branch=%APPVEYOR_PULL_REQUEST_HEAD_REPO_BRANCH%, ^
    /d:sonar.pullrequest.base=%APPVEYOR_REPO_BRANCH%
) ELSE (
    IF "%APPVEYOR_REPO_BRANCH%"=="master" (
      echo "Master branch."
      CALL :sonar_scanner ^
        /d:sonar.branch.name=%APPVEYOR_REPO_BRANCH%
      ) ELSE (
          IF "%APPVEYOR_REPO_BRANCH%"=="development" (
            echo "Development branch."
            SET "TARGET_BRANCH=master"
            ) ELSE (
                echo "Feature branch."
                SET "TARGET_BRANCH=development"
              )
        echo "Not a pull request or long lived branch."
        echo Branch Name is %APPVEYOR_REPO_BRANCH%
        echo Target Branch is !TARGET_BRANCH!
        CALL :sonar_scanner ^
          "/d:sonar.branch.name=""%APPVEYOR_REPO_BRANCH%""", ^
          "/d:sonar.branch.target=""!TARGET_BRANCH!"""
        )
  )
