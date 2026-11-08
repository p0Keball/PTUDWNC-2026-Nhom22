FROM postgres:16-alpine

RUN apk add --no-cache gzip

COPY backup.sh /usr/local/bin/backup.sh
COPY crontab /etc/crontabs/root
RUN chmod +x /usr/local/bin/backup.sh

# crond cần quyền root để đọc /etc/crontabs/root và ghi pid file.
USER root

CMD ["crond", "-f", "-l", "2"]
