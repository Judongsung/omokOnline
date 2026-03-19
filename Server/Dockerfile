# 1. 빌드 환경 (C# 12 / .NET 8.0)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# 2. 프로젝트 파일 복사 및 의존성 설치
COPY *.csproj ./
RUN dotnet restore

# 3. 나머지 코드 복사 및 배포용으로 빌드
COPY . ./
RUN dotnet publish -c Release -o out

# 4. 실제 실행할 가벼운 런타임 환경
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# 5. 8080 포트 열고, 서버 실행 (OmokServer.dll 이름이 맞는지 확인해 주세요!)
EXPOSE 8080
ENTRYPOINT ["dotnet", "OmokServer.dll"]