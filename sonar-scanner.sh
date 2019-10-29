#/bin/bash -e

sonar_scanner() {
  local params=$@

  sonar-scanner \
    -Dsonar.host.url='https://sonarqube.split-internal.com' \
    -Dsonar.login="$SONAR_TOKEN" \
    -Dsonar.ws.timeout='300' \
    -Dsonar.sources='src' \
    -Dsonar.projectName='.net-core-client' \
    -Dsonar.projectKey='net-core-client' \
    -Dsonar.links.ci='https://travis-ci.com/splitio/.net-core-client' \
    -Dsonar.links.scm='https://github.com/splitio/.net-core-client' \
    ${params}

  return $?
}

if [ "$TRAVIS_PULL_REQUEST" != "false" ]; then
  sonar_scanner \
    -Dsonar.pullrequest.provider='GitHub' \
    -Dsonar.pullrequest.github.repository='splitio/.net-core-client' \
    -Dsonar.pullrequest.key=$TRAVIS_PULL_REQUEST \
    -Dsonar.pullrequest.branch=$TRAVIS_PULL_REQUEST_BRANCH \
    -Dsonar.pullrequest.base=$TRAVIS_BRANCH
else
  if [ "$TRAVIS_BRANCH" == 'master' ]; then
    sonar_scanner \
      -Dsonar.branch.name=$TRAVIS_BRANCH
  else
    if [ "$TRAVIS_BRANCH" == 'development' ]; then
      TARGET_BRANCH='master'
    else
      TARGET_BRANCH='development'
    fi
    sonar_scanner \
      -Dsonar.branch.name=$TRAVIS_BRANCH \
      -Dsonar.branch.target=$TARGET_BRANCH
  fi
fi
