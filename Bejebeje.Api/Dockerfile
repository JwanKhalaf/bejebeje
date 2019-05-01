# set base image as the dotnet 2.2 SDK.
FROM microsoft/dotnet:2.2-sdk AS build-env

# set the working directory for any RUN, CMD, ENTRYPOINT, COPY and ADD
# instructions that follows the WORKDIR instruction.
WORKDIR /app

# our current working directory within the container is /app
# we now copy all the files (from local machine) to /app (in the container).
COPY . ./

# again, on the container (we are in /app folder)
# we now publish the project into a folder called 'out'.
RUN dotnet publish Bejebeje.Api/Bejebeje.Api.csproj -c Release -o out

# set base image as the dotnet 2.2 runtime.
FROM microsoft/dotnet:2.2-aspnetcore-runtime AS runtime

# local variable
ARG APP_PORT

# temporary test to make sure the variable is coming through.
RUN echo "ASPNETCORE_URLS -> $APP_PORT"

# telling the application what port to run on.
ENV ASPNETCORE_URLS=$APP_PORT

# set the working directory for any RUN, CMD, ENTRYPOINT, COPY and ADD
# instructions that follows the WORKDIR instruction.
WORKDIR /app

# copy the contents of /app/out in the `build-env` and paste it in the
# `/app` directory of the new runtime container.
COPY --from=build-env /app/Bejebeje.Api/out .

# set the entry point into the application.
ENTRYPOINT ["dotnet", "Bejebeje.Api.dll", "-seed"]