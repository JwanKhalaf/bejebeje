#!/bin/bash

# define tag variable
DOCKER_TAG='latest'

# log into docker hub.
docker login -u $DOCKER_USERNAME -p $DOCKER_PASSWORD

# build the docker image.
docker build -f ./Dockerfile -t bejebeje/mvc:$DOCKER_TAG -t bejebeje/mvc:$TRAVIS_BUILD_NUMBER . --no-cache

# tag the docker image with latest.
docker tag bejebeje/mvc:$DOCKER_TAG $DOCKER_USERNAME/bejebeje/mvc:$DOCKER_TAG

# tag the docker image with build number.
docker tag bejebeje/mvc:$DOCKER_TAG $DOCKER_USERNAME/bejebeje/mvc:$TRAVIS_BUILD_NUMBER

# push the docker image (tagged latest) to docker hub.
docker push bejebeje/mvc:$DOCKER_TAG

# push the docker image (tagged with build number) to docker hub.
docker push bejebeje/mvc:$TRAVIS_BUILD_NUMBER