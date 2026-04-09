# MajdataPlay 设置同步 — 服务端数据库 Migration 与存储指南

## 一、概述

本文档指导 GeoDanceClub 后端（Django）实现 MajdataPlay 游戏设置的云端存储与同步。

### 同步范围

客户端 `GameSetting` 包含以下顶级分组：

| 分组 | 类名 | 是否同步 | 说明 |
|------|-------|----------|------|
| Game | `GameOptions` | ✅ 同步 | 游戏核心设置 |
| Judge | `JudgeOptions` | ✅ 同步 | 判定偏移 |
| Display | `DisplayOptions` | ✅ 同步 | 显示与皮肤 |
| Audio | `SoundOptions` | ✅ 同步 | 音频设置（含音量子组） |
| Debug | `DebugOptions` | ✅ 同步 | 调试与性能参数 |
| Mod | `ModOptions` | ❌ 不同步 | 临时游戏修改器（客户端标记 `[JsonIgnore]`） |
| Online | `OnlineOptions` | ❌ 不同步 | 服务器连接配置，设备特定 |
| IO | `IOOptions` | ❌ 不同步 | 硬件设备配置（InputDevice + OutputDevice），设备特定 |

### 存储策略

**推荐采用 JSON Blob 存储**，而非将每个字段拆成独立列。原因：

1. 客户端设置字段频繁变动（有大量平台条件编译 `#if UNITY_ANDROID` 等），逐列存储维护成本极高
2. 客户端本身就以 JSON 格式序列化/反序列化设置
3. 服务端不需要对单个设置字段做查询或索引
4. 前后端 schema 一致性天然保证

---

## 二、数据库设计

### Model 定义

```python
# models.py
import uuid
from django.db import models
from django.conf import settings


class UserGameSettings(models.Model):
    """
    每用户一份游戏设置记录。
    设置以 JSON blob 存储，字段结构与客户端 GameSetting 对齐。
    """
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    user = models.OneToOneField(
        settings.AUTH_USER_MODEL,
        on_delete=models.CASCADE,
        related_name='game_settings',
        db_index=True,
    )

    # --- 同步的设置分组，每组一个 JSON 字段 ---
    game = models.JSONField(default=dict, blank=True, help_text="GameOptions")
    judge = models.JSONField(default=dict, blank=True, help_text="JudgeOptions")
    display = models.JSONField(default=dict, blank=True, help_text="DisplayOptions")
    audio = models.JSONField(default=dict, blank=True, help_text="SoundOptions")
    debug = models.JSONField(default=dict, blank=True, help_text="DebugOptions")

    # --- 版本控制 ---
    version = models.PositiveBigIntegerField(
        default=0,
        help_text="单调递增版本号，每次写入 +1，用于冲突检测"
    )
    updated_at = models.DateTimeField(auto_now=True, db_index=True)
    created_at = models.DateTimeField(auto_now_add=True)

    class Meta:
        db_table = 'majplay_user_game_settings'
        verbose_name = '用户游戏设置'
        verbose_name_plural = '用户游戏设置'

    def __str__(self):
        return f"Settings({self.user}) v{self.version}"
```

### 为什么分组存储而不是单个大 JSON

将 5 个顶级分组拆成 5 个独立 `JSONField`，而不是一整个 `settings_blob`：

1. **按需同步**：客户端可以只拉取/推送特定分组，减少传输量
2. **部分失败隔离**：一个分组的 JSON 损坏不影响其他分组
3. **日后可独立扩展**：某个分组如果需要加字段白名单校验，可以独立处理
4. **Django admin 可读性**：每个分组可独立查看和调试

---

## 三、Migration

```bash
python manage.py makemigrations
python manage.py migrate
```

生成的 migration 大致如下（自动生成即可，这里仅作参考）：

```python
# migrations/XXXX_create_user_game_settings.py
from django.db import migrations, models
import django.db.models.deletion
import uuid


class Migration(migrations.Migration):

    dependencies = [
        ('auth', '0012_alter_user_first_name_max_length'),
        # 或你的自定义 user app
    ]

    operations = [
        migrations.CreateModel(
            name='UserGameSettings',
            fields=[
                ('id', models.UUIDField(
                    default=uuid.uuid4, editable=False, primary_key=True, serialize=False
                )),
                ('game', models.JSONField(blank=True, default=dict)),
                ('judge', models.JSONField(blank=True, default=dict)),
                ('display', models.JSONField(blank=True, default=dict)),
                ('audio', models.JSONField(blank=True, default=dict)),
                ('debug', models.JSONField(blank=True, default=dict)),
                ('version', models.PositiveBigIntegerField(default=0)),
                ('updated_at', models.DateTimeField(auto_now=True, db_index=True)),
                ('created_at', models.DateTimeField(auto_now_add=True)),
                ('user', models.OneToOneField(
                    on_delete=django.db.models.deletion.CASCADE,
                    related_name='game_settings',
                    to='auth.user',  # 替换为实际的 AUTH_USER_MODEL
                )),
            ],
            options={
                'db_table': 'majplay_user_game_settings',
                'verbose_name': '用户游戏设置',
                'verbose_name_plural': '用户游戏设置',
            },
        ),
    ]
```

---

## 四、各分组 JSON Schema 参考

以下是每个 JSONField 内存储的字段结构。客户端序列化时使用 `camelCase`。

### 4.1 `game` — GameOptions

```jsonc
{
  "tapSpeed": 7.5,              // float, 默认 7.5
  "touchSpeed": 7.5,            // float, 默认 7.5
  "slideFadeInOffset": 0.0,     // float
  "backgroundDim": 0.8,         // float [0, 1]
  "starRotation": true,         // bool
  "bgInfo": "Combo",            // enum: CPCombo|PCombo|Combo|Achievement_101|Achievement_100|Achievement|AchievementClassical|AchievementClassical_100|DXScore|DXScoreRank|S_Border|SS_Border|SSS_Border|MyBest|Diff|None
  "topInfo": "None",            // enum: None|Judge|Timing|TimingGauge
  "trackSkip": true,            // bool
  "fastRetry": true,            // bool
  "mirror": "Off",              // enum: Off|LRMirror|UDMirror
  "rotation": 0,                // int [-7, 7]
  "slideSkipping": true,        // bool
  "random": "Disabled",         // enum: Disabled|RANDOM|S_RANDOM

  // --- 仅 Android/iOS ---
  "buttonRingForTouch": true,   // bool (仅移动端存在)

  // --- 仅 Standalone ---
  "recordMode": "Disable"       // enum: Disable|OBSTrigger (仅桌面端存在)
}
```

> **平台条件字段处理规则**：服务端应当宽容存储。客户端上传时只包含自身平台的字段；拉取时忽略不认识的字段。服务端只做透传存储，不做字段校验过滤。

### 4.2 `judge` — JudgeOptions

```jsonc
{
  "audioOffset": 0.0,       // float
  "judgeOffset": 0.0,       // float
  "answerOffset": 0.0,      // float
  "touchPanelOffset": 0.0,  // float
  "mode": "Modern"          // enum: Classic|Modern
}
```

### 4.3 `display` — DisplayOptions

```jsonc
{
  "language": "",                     // string
  "skin": "default",                  // string
  "displayCriticalPerfect": false,    // bool
  "displayBreakScore": true,          // bool
  "fastLateType": "Disable",         // enum: All|BelowCP|BelowP|BelowGR|MissOnly|Disable
  "noteJudgeType": "All",            // enum (同上)
  "touchJudgeType": "All",           // enum (同上)
  "slideJudgeType": "All",           // enum (同上)
  "breakJudgeType": "All",           // enum (同上)
  "breakFastLateType": "Disable",    // enum (同上)
  "slideSortOrder": "Modern",         // enum: Classic|Modern
  "outerJudgeDistance": 1.0,          // float [0, 1]
  "innerJudgeDistance": 1.0,          // float [0, 1]
  "displayHoldHeadJudgeResult": false, // bool
  "tapScale": 1.0,                    // float [0, 2]
  "holdScale": 1.0,                   // float [0, 2]
  "touchScale": 1.0,                  // float [0, 2]
  "slideScale": 1.0,                  // float [0, 2]
  "touchFeedback": "Outer_Only",      // enum: All|Outer_Only|Inner_Only|Disable
  "mainScreenTransform": false,       // bool (Android/iOS 默认 true)
  "mainScreenScale": 1.0,             // float [0.05, 1.5]
  "mainScreenOffset": 1.0,            // float [-1, 1]
  "mainScreenCachedScreenCenterY": 960.0, // float (HideInSettingUI)
  "subDisplayOffset": 0.0,            // float [-5, 5]
  "subDisplayScale": 1.0,             // float
  "renderQuality": "Low",             // enum: VeryLow|Low|Medium|High|VeryHight|Ultra
  "fpsLimit": 120,                    // int >= -1
  "skipVideoDownload": false,         // bool

  // --- 仅 Standalone ---
  "resolution": "1080x1920",          // string (HideInSettingUI)
  "topmost": false,                   // bool (HideInSettingUI)
  "vSync": true                       // bool (非 Android/iOS)
}
```

### 4.4 `audio` — SoundOptions

```jsonc
{
  "forceMono": false,          // bool
  "volume": {                  // SFXVolume 子对象
    "global": 0.3,             // float [0, 1]
    "bgm": 1.0,               // float [0, 1]
    "track": 1.0,              // float [0, 1]
    "answer": 0.8,             // float [0, 1]
    "tap": 0.3,                // float [0, 1]
    "ex": 0.3,                 // float [0, 1]
    "break": 0.3,              // float [0, 1]
    "slide": 0.3,              // float [0, 1]
    "touch": 0.3,              // float [0, 1]
    "hanabi": 0.3,             // float [0, 1]
    "voice": 1.0               // float [0, 1]
  },
  "backend": "Wasapi",        // enum: Unity|Wasapi|Asio|BassSimple

  // --- 仅 Standalone ---
  "wasapi": {                  // WasapiOptions
    "exclusive": true,
    "rawMode": true,
    "bufferSize": 0.02,
    "period": 0.005
  },
  "asio": {                    // AsioOptions
    "deviceIndex": 0,
    "sampleRate": 44100
  },
  "channel": {                 // ChannelOptions
    "frontVolume": 1.0,
    "centerAndLFEVolume": 1.0,
    "sideVolume": 1.0,
    "rearVolume": 1.0
  },

  // --- 仅 Android/iOS ---
  "mobile": {                  // MobileAudioOptions
    "enableAAudio": true,      // 仅 Android
    "bufferLengthMs": 128,
    "updatePeriodMs": 16,
    "deviceBufferLengthMs": 32,
    "deviceUpdatePeriodMs": 4
  }
}
```

### 4.5 `debug` — DebugOptions

```jsonc
{
  "displaySensor": false,          // bool
  "touchSimulationRadius": 0.5,    // float [0, 2]
  "touchAAreaExtraRadius": 0.0,    // float [0, 2]
  "touchBAreaExtraRadius": 0.0,    // float [0, 2]
  "touchCAreaExtraRadius": 0.25,   // float [0, 2]
  "touchDAreaExtraRadius": 0.2,    // float [0, 2]
  "touchEAreaExtraRadius": 0.10,   // float [0, 2]
  "touchRadiusAdjust": 0.0,        // float [0, 2]
  "displayFPS": true,              // bool
  "menuOptionIterationSpeed": 45,  // int (HideInSettingUI)
  "displayOffset": 0.0,            // float >= 0
  "noteAppearRate": 0.265,         // float
  "offsetUnit": "Frame",           // enum: Second|Frame
  "noteFolding": true,             // bool (HideInSettingUI, iOS 下 JsonIgnore)
  "djAutoPolicy": "Strict",        // enum: Strict|Permissive
  "maxQueuedFrames": 2,            // int (HideInSettingUI)
  "tapPoolCapacity": 96,           // int (HideInSettingUI, 移动端默认 48)
  "holdPoolCapacity": 96,          // int (HideInSettingUI, 移动端默认 48)
  "touchPoolCapacity": 64,         // int (HideInSettingUI)
  "touchHoldPoolCapacity": 64,     // int (HideInSettingUI)
  "eachLinePoolCapacity": 48,      // int (HideInSettingUI, 移动端默认 24)
  "debugLevel": "Info",            // enum: Debug|Info|Warning|Error|Fatal (HideInSettingUI)

  // --- 仅 Standalone ---
  "fullScreen": true,              // bool (HideInSettingUI)
  "hideCursorInGame": true         // bool (HideInSettingUI)
}
```

---

## 五、API 设计

### 5.1 端点

基于现有 MajPlay API 约定，在 GeoDanceClub endpoint 下新增：

| Method | Path | 说明 |
|--------|------|------|
| `GET` | `account/settings` | 拉取当前登录用户的设置 |
| `PUT` | `account/settings` | 上传/覆盖当前登录用户的设置 |

> 复用现有 cookie/session 认证，与 `account/info`、`account/scores` 同级。

### 5.2 GET `account/settings`

**Response 200:**

```json
{
  "version": 42,
  "updatedAt": "2026-04-09T10:30:00Z",
  "game": { ... },
  "judge": { ... },
  "display": { ... },
  "audio": { ... },
  "debug": { ... }
}
```

**Response 401:** 未登录

**Response 404:** 用户没有已保存的设置（首次使用）

### 5.3 PUT `account/settings`

**Request Body:**

```json
{
  "version": 42,
  "game": { ... },
  "judge": { ... },
  "display": { ... },
  "audio": { ... },
  "debug": { ... }
}
```

- `version`: 客户端从上次 GET 获得的版本号。服务端校验 `request.version == db.version` 才允许写入（乐观锁）。
- 分组可以只提交部分（例如只改了 `game`，其他字段可省略）。

**Response 200:** 写入成功

```json
{
  "version": 43,
  "updatedAt": "2026-04-09T10:31:00Z"
}
```

**Response 409 Conflict:** 版本冲突（另一台设备已修改）

```json
{
  "error": "version_conflict",
  "serverVersion": 43,
  "message": "Settings have been modified by another device"
}
```

**Response 401:** 未登录

### 5.4 Django View 参考实现

```python
# views.py
import json
from django.http import JsonResponse
from django.views import View
from django.utils.decorators import method_decorator
from django.views.decorators.csrf import csrf_exempt

from .models import UserGameSettings

SYNCED_GROUPS = ['game', 'judge', 'display', 'audio', 'debug']


@method_decorator(csrf_exempt, name='dispatch')
class UserSettingsView(View):

    def get(self, request):
        if not request.user.is_authenticated:
            return JsonResponse({'error': 'unauthorized'}, status=401)

        try:
            settings_obj = UserGameSettings.objects.get(user=request.user)
        except UserGameSettings.DoesNotExist:
            return JsonResponse({'error': 'not_found'}, status=404)

        data = {
            'version': settings_obj.version,
            'updatedAt': settings_obj.updated_at.isoformat(),
        }
        for group in SYNCED_GROUPS:
            data[group] = getattr(settings_obj, group)

        return JsonResponse(data)

    def put(self, request):
        if not request.user.is_authenticated:
            return JsonResponse({'error': 'unauthorized'}, status=401)

        try:
            body = json.loads(request.body)
        except (json.JSONDecodeError, ValueError):
            return JsonResponse({'error': 'invalid_json'}, status=400)

        client_version = body.get('version', 0)

        settings_obj, created = UserGameSettings.objects.get_or_create(
            user=request.user
        )

        # 乐观锁：首次创建或版本匹配才可写入
        if not created and settings_obj.version != client_version:
            return JsonResponse({
                'error': 'version_conflict',
                'serverVersion': settings_obj.version,
                'message': 'Settings have been modified by another device',
            }, status=409)

        for group in SYNCED_GROUPS:
            if group in body:
                setattr(settings_obj, group, body[group])

        settings_obj.version += 1
        settings_obj.save()

        return JsonResponse({
            'version': settings_obj.version,
            'updatedAt': settings_obj.updated_at.isoformat(),
        })
```

### 5.5 URL 配置

```python
# urls.py
from django.urls import path
from .views import UserSettingsView

urlpatterns = [
    # ... 现有 MajPlay API 路由 ...
    path('account/settings', UserSettingsView.as_view(), name='user-settings'),
]
```

---

## 六、客户端同步时序

### 登录时拉取设置

```
客户端                          服务端
  |                               |
  |-- POST account/login -------->|  (已有)
  |<---- 200 + Set-Cookie --------|
  |                               |
  |-- GET account/info ---------->|  (已有)
  |<---- 200 UserSummary ---------|
  |                               |
  |-- GET account/settings ------>|  ★ 新增
  |<---- 200 { version, game, ... } |
  |                               |
  [客户端将拉取到的设置覆盖到     ]
  [MajEnv.Settings 对应分组，     ]
  [并触发 GameManager.RequestSave ]
  [写回本地 settings.json         ]
```

### 修改设置后上传

```
客户端                          服务端
  |                               |
  [用户修改设置，退出设置页面]     
  [SettingManager.OnDestroy()     ]
  [触发 GameManager.RequestSave() ]
  |                               |
  |-- PUT account/settings ------>|  ★ 新增
  |   { version, game, judge, ...}|
  |<---- 200 { version: N+1 } ---|
  |                               |
  [客户端更新本地 version 缓存]    
```

---

## 七、冲突策略

| 场景 | 策略 |
|------|------|
| 首次登录，服务端无设置 (404) | 客户端以本地设置为准，立即执行一次 PUT 上传 |
| 首次登录，服务端有设置 | 拉取远端设置，覆盖本地，保存到 `settings.json` |
| 正常使用中修改设置 | 退出设置页后 PUT 上传，version 乐观锁 |
| PUT 返回 409 冲突 | 客户端重新 GET 最新设置，以远端为准覆盖本地 |

---

## 八、注意事项

1. **平台条件字段**：客户端设置包含大量 `#if UNITY_ANDROID` 等条件编译字段。服务端应视 JSON blob 为不透明数据，存什么就取什么，不做字段过滤或校验。

2. **camelCase 序列化**：客户端使用 Newtonsoft.Json 的 `CamelCaseNamingStrategy` 进行 API 通信（见 `Online.cs` 中的 `DEFAULT_JSON_SERIALIZER_SETTINGS`），JSON 字段名是 `camelCase`。但本地 `settings.json` 使用 `PascalCase`（`UserJsonReaderOption` 不含 `CamelCaseNamingStrategy`）。服务端存储应保存 `camelCase` 版本（即 API 传输格式）。

3. **枚举值存储为字符串**：客户端序列化枚举为字符串（`StringEnumConverter`），服务端 JSON 中枚举值应以字符串形式存储，不要转为整数。

4. **版本号语义**：`version` 是单调递增整数，每次写入 +1。客户端在本地应缓存当前 version 值用于后续 PUT。

5. **不同步 Mod**：`ModOptions` 在客户端标记为 `[JsonIgnore]`，不序列化到 `settings.json`，也不参与云端同步。这些是临时游戏修改器，仅在内存中生效。
