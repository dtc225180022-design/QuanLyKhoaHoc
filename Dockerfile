# ── Stage 1: Build ────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore dependencies trước (tận dụng Docker layer cache)
COPY ["QuanLyKhoaHoc/QuanLyKhoaHoc.csproj", "QuanLyKhoaHoc/"]
RUN dotnet restore "QuanLyKhoaHoc/QuanLyKhoaHoc.csproj"

# Copy toàn bộ source và build
COPY . .
WORKDIR "/src/QuanLyKhoaHoc"
RUN dotnet publish "QuanLyKhoaHoc.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: Runtime ───────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Tạo thư mục uploads (Railway filesystem ephemeral, nhưng cần tồn tại)
RUN mkdir -p wwwroot/uploads/avatars

COPY --from=build /app/publish .

# Railway inject $PORT tự động — app đọc qua env var PORT
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet", "QuanLyKhoaHoc.dll"]
