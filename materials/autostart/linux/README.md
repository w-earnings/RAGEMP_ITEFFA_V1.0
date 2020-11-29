Установка автозапуска:
- Создаем файл: /etc/systemd/system/ragemp-server.service
- Копируем содержимое из и ragemp-server.service заменяем на свои данные.
-- After -> старт сетевых интерфейсов
-- WorkingDirectory -> Указываем рабочую папку
-- User -> Указываем своего пользователя (root)
-- ExecStart -> указываем команду для запуска
- Сохраняем файл, перезапускаем daemon systemd командой:
  # systemctl daemon-reload
- Команды для быстрого и удобного управления своим сервером RAGE:MP:
  # service ragemp-server start
  # service ragemp-server stop
  # service ragemp-server restart
  # service ragemp-server status





