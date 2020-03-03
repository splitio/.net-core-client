@echo off

:sonar_scanner
SonarScanner.MSBuild.exe begin ^
  /k:".net-core-client" ^
  /d:"sonar.host.url=https://sonarqube.split-internal.com" ^
  /d:sonar.login=%SONAR_LOGIN% ^
  /d:sonar.ws.timeout="300" ^
  /d:sonar.sources="." ^
  /d:sonar.links.ci="https://travis-ci.com/splitio/.net-core-client" ^
  /d:sonar.links.scm="https://github.com/splitio/.net-core-client" ^
  %*
EXIT /B 0

IF NOT %APPVEYOR_PULL_REQUEST_NUMBER%="" (
  CALL :sonar_scanner ^
    /d:sonar.pullrequest.provider="GitHub", ^
    /d:sonar.pullrequest.github.repository="splitio/.net-core-client", ^
    /d:sonar.pullrequest.key=%APPVEYOR_PULL_REQUEST_NUMBER%, ^
    /d:sonar.pullrequest.branch=%APPVEYOR_PULL_REQUEST_HEAD_REPO_BRANCH%, ^
    /d:sonar.pullrequest.base=%APPVEYOR_REPO_BRANCH%
  ) ELSE (
      IF %APPVEYOR_REPO_BRANCH%="master" (
        CALL :sonar_scanner ^
          /d:sonar.branch.name=%APPVEYOR_REPO_BRANCH%
        ) ELSE (
            IF %APPVEYOR_REPO_BRANCH%="development" (
              SET TARGET_BRANCH="master"
              ) ELSE (
                  SET TARGET_BRANCH="development"
                )
          CALL :sonar_scanner ^
            /d:sonar.branch.name=%APPVEYOR_REPO_BRANCH%,^
            /d:sonar.branch.target=%TARGET_BRANCH%
          )
    )
