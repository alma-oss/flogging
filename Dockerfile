FROM dcreg.service.consul/prod/development-dotnet-core-sdk-common:latest

# build scripts
COPY ./fake.sh /flogging/
COPY ./build.fsx /flogging/
COPY ./paket.dependencies /flogging/
COPY ./paket.references /flogging/
COPY ./paket.lock /flogging/

# sources
COPY ./Logging.fsproj /flogging/
COPY ./src /flogging/src

WORKDIR /flogging

RUN \
    ./fake.sh build target Build no-clean

CMD ["./fake.sh", "build", "target", "Tests", "no-clean"]
