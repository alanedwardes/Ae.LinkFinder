FROM selenium/standalone-chrome:114.0

ADD build/output /opt/aelinkfinder

VOLUME ["/data"]

WORKDIR /data

ENTRYPOINT ["/opt/aelinkfinder/Ae.LinkFinder"]
