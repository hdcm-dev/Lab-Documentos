# Receta del contenedor de compilación de la demo (P11, «cómo se corre»: receta versionada en Demo/, sin imágenes
# locales). scripts/contenedor.sh la construye la primera vez y la etiqueta como fc-demo-build:<huella de este archivo>.
#
# Trae todo lo que la máquina de quien corre la demo no necesita tener:
#   - SDK de .NET 10.0.400 (la imagen base, fijada por versión y digest; global.json pide el mismo SDK);
#   - workload maui-android del workload set 10.0.400.1 (= workloadVersion de global.json; MAUI 10.0.20);
#   - JDK 17 y el SDK de Android (plataformas 35 y 36, build-tools), que pide net10.0-android;
#   - python3 y openssl, que usan «guion.sh humo» y «guion.sh tls».
#
# No trae adb para el teléfono ni el emulador: el guion usa el adb que indique FC_ADB_CONTENEDOR o el del sistema.
# Origen: es la receta de lab-e2e-maui-dev (Lab-E2E.WebBlazor/.devcontainer/Dockerfile) sin lo que la demo no usa
# (Appium, Node.js, sudo, usuario propio) y con las versiones fijadas.
FROM mcr.microsoft.com/dotnet/sdk:10.0.400-noble@sha256:4beef5b8919dcaa2dc924233bd069257e883cc7a061e09088a97d152d6a48510

ENV DEBIAN_FRONTEND=noninteractive

RUN apt-get update && apt-get install -y --no-install-recommends \
        openjdk-17-jdk-headless \
        unzip zip curl git ca-certificates \
        libicu-dev locales \
        python3 openssl \
    && rm -rf /var/lib/apt/lists/*

# Los mensajes de dotnet salen en español, como en las corridas que cita la guía.
RUN sed -i 's/^# es_AR.UTF-8/es_AR.UTF-8/; s/^# en_US.UTF-8/en_US.UTF-8/' /etc/locale.gen && locale-gen

ENV JAVA_HOME=/usr/lib/jvm/java-17-openjdk-amd64 \
    ANDROID_HOME=/opt/android-sdk \
    ANDROID_SDK_ROOT=/opt/android-sdk

# SDK de Android: herramientas de línea de comandos fijadas por número de build.
RUN mkdir -p ${ANDROID_HOME}/cmdline-tools \
    && curl -fsSL -o /tmp/cmdline.zip \
        https://dl.google.com/android/repository/commandlinetools-linux-13114758_latest.zip \
    && unzip -q /tmp/cmdline.zip -d ${ANDROID_HOME}/cmdline-tools \
    && mv ${ANDROID_HOME}/cmdline-tools/cmdline-tools ${ANDROID_HOME}/cmdline-tools/latest \
    && rm /tmp/cmdline.zip

ENV PATH=${ANDROID_HOME}/cmdline-tools/latest/bin:${ANDROID_HOME}/platform-tools:${PATH}

RUN yes | sdkmanager --licenses > /dev/null \
    && sdkmanager --install \
        "platform-tools" \
        "platforms;android-36" \
        "platforms;android-35" \
        "build-tools;36.0.0" \
        "build-tools;35.0.0" \
    && rm -rf ${ANDROID_HOME}/.temp \
    && chmod -R a+rX ${ANDROID_HOME}

# El workload set fijo: el mismo número que workloadVersion de global.json.
RUN dotnet workload install maui-android --version 10.0.400.1 --skip-sign-check \
    && dotnet nuget locals all --clear

# contenedor.sh corre como el usuario de quien llama (-u uid:gid), con HOME en /tmp/home.
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_NOLOGO=1 \
    LANG=es_AR.UTF-8
WORKDIR /work
