#!/bin/sh
set -eu

# cron chạy với PATH tối giản; pg_dump nằm ở /usr/local/bin trong image postgres.
export PATH="/usr/local/bin:/usr/bin:/bin:${PATH:-}"

BACKUP_DIR="${BACKUP_DIR:-/backups}"
RETENTION_DAYS="${BACKUP_RETENTION_DAYS:-30}"
STAMP="$(date -u +%Y%m%d_%H%M%S)"
TARGET="${BACKUP_DIR}/foodblog_${STAMP}.sql.gz"
TEMP="${BACKUP_DIR}/foodblog_${STAMP}.sql"

mkdir -p "${BACKUP_DIR}"
trap 'rm -f "${TEMP}"' EXIT

echo "[$(date -u +%Y-%m-%dT%H:%M:%SZ)] Bắt đầu sao lưu ${PGDATABASE}@${PGHOST} -> ${TARGET}"

# Ghi ra file tạm trước (set -e sẽ dừng nếu pg_dump lỗi), sau đó mới nén,
# tránh tạo file .gz rỗng/dở dang khi pg_dump thất bại giữa pipeline.
pg_dump --no-owner --no-privileges --file="${TEMP}"
gzip -9 -c "${TEMP}" > "${TARGET}"
rm -f "${TEMP}"

# Kiểm tra file không rỗng trước khi coi là thành công.
if [ ! -s "${TARGET}" ]; then
  echo "Sao lưu thất bại: file rỗng." >&2
  rm -f "${TARGET}"
  exit 1
fi

# Giữ tối đa RETENTION_DAYS ngày.
find "${BACKUP_DIR}" -type f -name 'foodblog_*.sql.gz' -mtime "+${RETENTION_DAYS}" -delete
echo "[$(date -u +%Y-%m-%dT%H:%M:%SZ)] Sao lưu hoàn tất (${TARGET})."
