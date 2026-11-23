FROM nginx:1.27-alpine

RUN apk add --no-cache openssl

COPY nginx.conf /etc/nginx/conf.d/default.conf
# Script trong /docker-entrypoint.d được chạy trước khi nginx khởi động.
COPY generate-cert.sh /docker-entrypoint.d/40-generate-cert.sh
# Chuẩn hóa LF phòng trường hợp file bị checkout CRLF trên Windows.
RUN sed -i 's/\r$//' /docker-entrypoint.d/40-generate-cert.sh \
    && chmod +x /docker-entrypoint.d/40-generate-cert.sh

EXPOSE 80 443
