#!/bin/bash

DOCKER_TAG='latest'
DOCKER_DEVELOP_TAG='latest-develop'

# log into docker hub.
docker login -u $DOCKER_USERNAME -p $DOCKER_PASSWORD

# build the docker image.
docker build -f ./Dockerfile -t bejebeje/api:travis . --no-cache

# tag the docker image with build number.
docker tag bejebeje/api:$DOCKER_TAG $DOCKER_USERNAME/bejebeje/api:$TRAVIS_BUILD_NUMBER

echo "The travis branch variable is:"

echo $TRAVIS_BRANCH

if [ "$TRAVIS_BRANCH" == "develop" ]; then
  echo "we're in true branch!"
  echo "the develop tag is:"
  echo $DOCKER_DEVELOP_TAG
  # tag the docker image with latest develop tag.
  docker tag bejebeje/api:$DOCKER_TAG $DOCKER_USERNAME/bejebeje/api:$DOCKER_DEVELOP_TAG

  echo "docker image is tagged with $DOCKER_DEVELOP_TAG"

  # push the docker image (tagged latest) to docker hub.
  docker push bejebeje/api:$DOCKER_DEVELOP_TAG
else
  echo "we're in false branch!"
  # tag the docker image with latest.
  docker tag bejebeje/api:$DOCKER_TAG $DOCKER_USERNAME/bejebeje/api:$DOCKER_TAG

  # push the docker image (tagged latest) to docker hub.
  docker push bejebeje/api:$DOCKER_TAG
fi

# push the docker image (tagged with build number) to docker hub.
docker push bejebeje/api:$TRAVIS_BUILD_NUMBER