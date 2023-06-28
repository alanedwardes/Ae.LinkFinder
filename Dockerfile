FROM mcr.microsoft.com/dotnet/runtime:6.0

ADD build/output /opt/aelinkfinder

VOLUME ["/data"]

WORKDIR /data

ENTRYPOINT ["/opt/aelinkfinder/Ae.Nuntium"]
