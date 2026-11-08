#!/bin/sh
set -eu

CERT_DIR="/etc/nginx/certs"
CERT_FILE="${CERT_DIR}/self-signed.crt"
KEY_FILE="${CERT_DIR}/self-signed.key"

if [ -f "${CERT_FILE}" ] && [ -f "${KEY_FILE}" ]; then
    echo "TLS certificate đã tồn tại, bỏ qua sinh mới."
    exit 0
fi

mkdir -p "${CERT_DIR}"
echo "Sinh chứng chỉ TLS self-signed cho môi trường local..."
openssl req -x509 -nodes -newkey rsa:2048 -days 825 \
    -keyout "${KEY_FILE}" \
    -out "${CERT_FILE}" \
    -subj "/C=VN/ST=HCM/L=HoChiMinh/O=FoodBlog/OU=Dev/CN=localhost" \
    -addext "subjectAltName=DNS:localhost,IP:127.0.0.1"

chmod 600 "${KEY_FILE}"
echo "Đã sinh chứng chỉ TLS tại ${CERT_DIR}."
