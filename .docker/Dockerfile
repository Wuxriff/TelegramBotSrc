FROM mcr.microsoft.com/dotnet/runtime-deps:7.0.11-bookworm-slim-arm64v8

WORKDIR /App
COPY Service /App

RUN chmod 777 /App && \
    apt-get update && apt-get install -y \
    tzdata \
    locales \
    locales-all && \
    apt-get clean \
	&& apt-get autoremove -y \
		software-properties-common \
	&& apt-get autoclean \
	&& apt-get clean \
	&& apt-get autoremove \
	&& rm -rf /var/lib/apt/lists/*

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV	LC_ALL en_GB.UTF-8
ENV	LANG en_GB.UTF-8
ENV	LANGUAGE en_GB.UTF-8
ENV	TZ=Europe/Minsk 

RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

ENTRYPOINT ["./ReminderBot.Service"]