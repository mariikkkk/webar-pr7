# Задание 7 — Unity WebAR: перемещение и масштабирование

**Демонстрация:** https://mariikkkk.github.io/webar-pr7/

Откройте ссылку в Chrome на Android с поддержкой WebXR, дождитесь загрузки,
нажмите «Начать AR» и разрешите доступ к камере. Для просмотра редактор Unity не нужен.
На компьютере доступен обычный 3D-предпросмотр.

## Управление

- Один палец: захватить куб и перетащить.
- Удерживая куб, добавить второй палец и свести или развести пальцы: масштаб 40–300%.
- «Передо мной»: вернуть куб перед камерой и восстановить масштаб.
- На компьютере: перетаскивание мышью, масштаб колесом при удержании куба.
- Синий цвет — свободный объект, бирюзовый — наведение, оранжевый — захват.

Куб размещается в метре перед камерой один раз, после чего сохраняет положение
в пространстве AR-сессии. При движении телефона меняются ракурс и видимые грани.
Распознавание плоскостей и сохранение положения между сессиями здесь не реализованы.

## Как устроено

Три скрипта сцены в `UnityProject/Assets/Scripts`:

- `ScreenGestureInput` переводит экранные касания в луч и передаёт его мосту.
- `BridgeRayInput` связывает Input System с `XRRayInteractor`.
- `InteractiveObject` задаёт масштаб и цвет обратной связи.

Цепочка ввода: DOM overlay → адаптированный webxr-xrit-bridge → Input System →
XR Ray Interactor → XR Grab Interactable. Перемещением занимается XR Interaction Toolkit.
Положение камеры обновляет `TrackedPoseDriver`; `WebXRCamera` управляет AR-камерами и проекцией.
Также подключены лучи LeftHand/RightHand для XR-контроллеров; физические контроллеры не тестировались.

## Исходники и сборка

`UnityProject/` — проект редактора. Корень репозитория — готовая WebGL-сборка для GitHub Pages.
Откройте `UnityProject` через Unity Hub в **Unity 6000.6.2f1** с установленным модулем WebGL.

Версии зависимостей: WebXR Export **0.25.0**, XR Interaction Toolkit **3.7.0**,
Input System **1.20.1**, Legacy Input Helpers **3.0.1**.
WebXR Export и адаптированный bridge **0.1.2** встроены в `UnityProject/Packages`;
остальные пакеты Unity восстановит из `manifest.json` и `packages-lock.json`.

Автоматическая сборка (путь к исполняемому файлу редактора зависит от ОС):

```sh
Unity -batchmode -nographics -quit -projectPath /absolute/path/to/UnityProject \
  -buildTarget WebGL -executeMethod BuildHomework.Run -logFile /absolute/path/to/build.log
```

`BuildHomework.Run` создаёт сцену и настройки, затем собирает её в соседнюю папку
`unity-pr7-build`. Перед запуском сохраните свои изменения сцены: метод пересоздаёт её.
Для обновления сайта перенесите содержимое этой сборки в корень репозитория и отправьте
коммит в `main`; GitHub Pages автоматически опубликует его.

Настройки: WebGL, Linear, Compression Disabled, Decompression Fallback выключен,
Threads Support выключен, AR reference space `local`, обязательный DOM overlay.
Вход в AR требует HTTPS и браузера с поддержкой immersive-ar и DOM overlay.

## Проверка

После очистки служебного кода и публикации на GitHub Pages выполнена повторная
ручная проверка на OnePlus 15 (Android 17, Chrome 156.0.8078.47).
Пользователь подтвердил: куб остаётся в пространстве при движении телефона,
перетаскивается и масштабируется двумя пальцами.
Также проверены WebGL-сборка и браузерные сценарии мыши и двух касаний.

## Сторонний код

Мост основан на [someone-s/webxr-xri-toolkit-bridge](https://github.com/someone-s/webxr-xri-toolkit-bridge).
Исходный мост не предоставлял готового управления мобильным AR: добавлен экранный виртуальный
контроллер и обновлена совместимость. Изменения описаны в
`UnityProject/Packages/com.someone-s.webxr-xrit-bridge/AR-ADAPTATION.md`.

[WebXR Export](https://github.com/De-Panther/unity-webxr-export) и bridge распространяются
под Apache-2.0; лицензии сохранены внутри пакетов. К компонентам движка применяются условия Unity.
