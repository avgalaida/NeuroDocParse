# NeuroDocParse

**NeuroDocParse** — это микросервисное решение для автоматического распознавания документов и извлечения данных с использованием технологий компьютерного зрения. Проект включает в себя несколько микросервисов, взаимодействующих между собой с помощью **Kafka**, использует **YOLOv8** для обнаружения объектов и **EasyOCR** для распознавания текста.

## Извлечениие данных

Отправляем изображение, получаем JSON.

![Пример](https://github.com/avgalaida/NeuroDocParse/blob/main/sample.avif)

## 📊 Архитектура

Проект построен на микросервисной архитектуре, взаимодействующей через брокер сообщений и включающей различные сервисы для обнаружения документов, полей, а также распознавания текста.

![Архитектура](https://github.com/avgalaida/NeuroDocParse/blob/main/architecture.avif)

## 🛠 Технологии

- **Docker**: Контейнеризация всех микросервисов.
- **Vue.js**: Фронтенд для взаимодействия с пользователями.
- **GraphQL**: API для связи клиента с микросервисами.
- **Keycloak**: Управление аутентификацией и авторизацией пользователей.
- **Kafka**: Брокер сообщений для взаимодействия между микросервисами.
- **MinIO**: Объектное хранилище для данных.
- **YOLOv8**: Модель для обнаружения объектов на изображениях.
- **EasyOCR**: Инструмент для оптического распознавания символов.
- **.NET**: Основная платформа для разработки микросервисов.
- **PostgreSQL**: База данных для хранения результатов обработки документов и данных пользователей.

## 📂 Структура проекта

- **/config**: Конфигурационные файлы проекта.
- **/services**: Микросервисы, которые обеспечивают работу системы:
  - **/frontend**: Веб-интерфейс, построенный на Vue.js.
  - **/gateway**: Шлюз, который управляет запросами от фронтенда и направляет их к соответствующим сервисам.
  - **/collector**: Сервис для сбора данных.
  - **/doc_detection**: Сервис для обнаружения документов на изображениях.
  - **/field_detection**: Сервис для обнаружения полей, соответствующих типу документа.
  - **/text_recog**: Сервис для распознавания текста на основе EasyOCR.
- **docker-compose.yml**: Конфигурационный файл для оркестрации контейнеров.

## 🧑‍💻 Вклад

Если у вас есть предложения по улучшению или оптимизации проекта, пожалуйста, создайте **Issue** или отправьте **Pull Request**.

---

Обученные модели в репозиторие отсутствуют из-за ограничений GitHub по размеру файлов.
