FROM mcr.microsoft.com/dotnet/runtime:6.0

ADD build/output /opt/nuntium

VOLUME ["/data"]

WORKDIR /data

ENTRYPOINT ["/opt/nuntium/Ae.Nuntium"]
