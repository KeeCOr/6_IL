# InisLand — GDD (코드 검증판)

- 작성일: 2026-09-11 (KST)
- 문서 유형: 게임 디자인 문서 (GDD)
- 문서 버전: 2.0 (전면 재작성 — 코드/데이터 기준)
- 근거 신뢰도: 중~높음 (Unity C# 소스, ScriptableObject 기본값, 기존 기획서·구현분석·빌드검증 문서를 직접 열람해 작성)
- 이 문서가 대체하는 이전 버전: 문서 버전 1.0 (전 항목 "[미확인]" 위주로 작성됨)

## 0. 버전 표기 — 실제로 확인된 불일치 (있는 그대로 기록)

여러 출처의 버전 표기가 서로 다르며, 이번 세션에서는 이 불일치를 해소하지 않고 사실만 나열한다.

| 출처 | 값 | 비고 |
|---|---|---|
| `package.json` (`version`) | 0.4.0 | 웹(Vite/Phaser) 트랙. AGENTS.md 규정상 Unity가 권위이므로 게임 콘텐츠 버전의 근거로 쓰지 않음 |
| `docs/InisLand_기획서.md` 상단 헤더 | v0.3.0 (2026-07-01) | 본문 변경 이력은 2026-09-08까지 계속 추가됨(헤더 갱신 안 됨) |
| `docs/InisLand_기획서.md` 내 "Release / Verification (2026-09-08)" | "v0.3.0의 게임 콘텐츠와 버전은 유지했다" | portable exe SHA-256 `5BB92BD9EAFBFF38C7A37E51204BD072BC740CEBC628ABB9585B36FEA4006B0C`, 99,534,448 bytes |
| `docs/InisLand_BuildValidation_2026-09-08.md` | "Source version: 0.5.0. Existing portable: 0.3.0." | 같은 날짜 문서인데 0.5.0을 소스 버전이라 명시, 새 빌드는 만들지 못함(Unity 2022.3.62f3 실행 파일 부재) |
| `ProjectSettings/ProjectVersion.txt` | Unity 에디터 2022.3.62f3 | 게임 버전이 아니라 Unity 에디터 버전 |
| 루트 실행파일명 | `InisLand_v0.3.0_portable.exe`, `InisLand_v0.2.0_portable_Data/` 폴더 잔존 | 실제 파일명 기준 |

**결론(사실만)**: 단일한 "현재 버전" 숫자를 코드에서 신뢰성 있게 확정할 수 없다. 가장 최근에 검증까지 이어진 표기는 v0.3.0 portable(2026-09-08 20초 생존 확인)이며, 소스 트리에는 v0.4.0(자원 HUD 아이콘화, 2026-09-07)과 v0.5.0 표기(BuildValidation)가 섞여 있다. 본 GDD는 이 상태를 "**버전 정합 미해결**"로 명시하고, 이후 섹션은 소스 코드(가장 최신 커밋 상태) 기준으로 작성한다.

---

## 1. 기획 품질 6요소 (기존 GDD 프레임워크 계승: 문제정의 / 페르소나 / 핵심 재미·디자인 필러 / 핵심 루프 / MVP 가설·KPI / 구현 상태)

### 1-1. 문제 정의 — [사실: 기획서 본문]
`docs/InisLand_기획서.md` 1장: "혼자서도 짧은 세션 안에 생존과 경영 두 재미를 모두 느낄 수 있는 게임이 없다." 낮에는 탐험가, 밤에는 마을 수호자로 한 세션 안에서 역할이 전환되는 것을 목표로 삼는다.

### 1-2. 페르소나 — [사실: 기획서 본문]
이진혁, 27세 직장인. 자투리 시간(20~30분), 로그라이크/생존류 선호, 복잡한 조작 부담, "오늘 밤은 버텼다"는 성취감 지향. (`docs/InisLand_구현분석_v0.2.0.md`에 동일 페르소나로 플레이 시뮬레이션 존재)

### 1-3. 핵심 재미와 왜 재미있는가 — [사실: 코드로 뒷받침]
핵심 재미는 **"짧은 낮의 준비가 밤의 생존 결과로 즉시 되돌아오는 긴장·해소 사이클"**이다. 근거:
- `Assets/Scripts/Cycle/DayNightController.cs` — Day→Evening→Night→Dawn 4단계 EventBus 순환이 실제로 구현되어 있고 낮의 자원(채집)이 밤의 방어 수단(건물)으로 직결된다.
- `Assets/Scripts/Enemies/NightController.cs` — 밤마다 웨이브가 누적 증가(`BaseWaveCount + (day-1)*PerDayIncrement`)하고, 25킬 또는 5일 주기로 보스가 강제 소환돼(`BossKillThreshold=25`, `day % 5 == 0`) 밤 안에서도 별도의 클라이맥스가 존재한다.
- `Assets/Scripts/Combat/PlayerProgression.cs` — 룬 3종 시너지(독/얼음/번개 중 2종 마스터 시 +50%)가 "이번엔 다른 조합" 재도전 동기를 코드 수준에서 제공한다.

### 1-4. 디자인 필러 — [제안: 코드 근거로 재구성]
1. **낮/밤 역할 전환**: 채집·탐험(낮) ↔ 방어·전투(밤)를 같은 캐릭터가 수행 (근거: `DayNightController`, `GatherController`, `NightController`)
2. **모닥불 = 마을의 심장**: 화상 오라(현재 DPS 2.5, `BalanceConfig.BonfireDamagePerSec`)이자 온기 유지 수단으로, 좀비 방어와 마을 생존이 한 오브젝트에 응축됨 (`Assets/Scripts/Combat/CampfireAura.cs`)
3. **선택은 룬 3장 중 1장**: 레벨업마다 12종 효과 룬 풀에서 3장 제시, 매 판 다른 빌드 (`PlayerProgression.PickThreeOffer`)
4. **점진적 압박**: 날짜가 오를수록 좀비 HP/데미지/속도/변종이 동시에 강화되어 정체된 플레이를 허용하지 않음 (`NightController.SpawnOne` 일자별 배율)

### 1-5. MVP 가설과 KPI — [사실: 기획서 본문, 실측 데이터는 미확인]
`기획서.md` 5장·7장의 가설 4개, 목표 KPI(Day 3 생존율 50%+, 세션 15~25분, 밤 격퇴 성취감 7/10+)는 문서화만 되어 있고, 실측 플레이테스트 로그/애널리틱스 구현은 이번 세션에서 코드 내 발견하지 못함 → **미검증**.

### 1-6. 구현 상태 — [사실: 코드 직접 확인, 3장 참조]
아래 3장의 "구현됨/계획됨/제안" 매트릭스로 판정.

---

## 2. 컴포넌트 표: 역할 / 선택 / 입력→판단→피드백 / 상태

| 컴포넌트 | 역할(role) | 선택(choice) | 입력→판단→피드백 | 상태(status) |
|---|---|---|---|---|
| 플레이어(`PlayerController`, `PlayerAttackController`) | 낮엔 탐험가, 밤엔 방어자 | 이동 방향, 채집 대상, 근접 공격 타이밍 | WASD 입력→충돌/사거리 판정→즉시 이동·타격 반응 | 구현됨 |
| 모닥불(`CampfireAura`) | 화상 오라 + 마을 온기 유지, 연료 소모(HP drain 0.5/s, `CampfireHpDrainPerSec`) | 어디에 배치하고 언제 방어를 집중할지 | 좀비가 반경(128) 진입→오라 데미지(2.5/s) 적용→체력바·범위 시각 피드백 | 구현됨 |
| 웨이브 스포너(`NightController`) | 밤마다 압박 수준 결정 | 없음(자동), 플레이어는 대응만 | 일자 경과→웨이브 수·변종·보스 결정→화면 색상(Lerp), 카메라 흔들림(`CameraFollow.Shake`)으로 피드백 | 구현됨 |
| 룬 선택(`PlayerProgression`) | 성장 방향 결정 | 레벨업 시 3장 중 1장 | XP 누적→레벨업 이벤트→룬 3장 제시→선택 즉시 수치 반영 | 구현됨 |
| 건물(`Building`, `BuildingFactory`) | 방어/생산/수용 인프라 | 어떤 건물을 어디에 지을지(자원 소비) | 자원 지불→건설 완료→HP·기능 활성화 | 구현됨(9종, 아래 3장) |
| NPC 영입(`RecruitableNpc`, `Companion`) | 동료 확보(전투/생산 보조) | 만난 NPC를 영입할지, 인구 한도 내에서 | F키 입력(사거리 1.8u 이내)→`CanRecruit()` 판정(마을 수용 인원)→즉시 동료 전환 및 색 변화 | 구현됨 |
| HUD 가이던스(`HudGuidanceText`) | 현재 목표/위험/행동 3슬롯 안내 | 없음(정보 제공) | 페이즈/블리자드/식량 상태→우선순위 규칙 매칭→OBJECTIVE/RISK/ACTION 3줄 텍스트 | 구현됨 |
| 업적(`AchievementManager`) | 세션 내 마일스톤 인지 | 없음(자동 추적) | 0.4초 폴링→9종 조건 검사→토스트 큐 적재 | 구현됨(영구 저장 아님, 세션 한정) |

---

## 3. 콘텐츠 수량·해금조건·공급량·보상·변형 — 코드 검증

모두 소스 파일과 줄 단위로 대조. "미검증" 표기 없는 항목은 코드에서 직접 읽은 값이다.

### 3-1. 낮/밤 사이클
- `Assets/Scripts/Config/BalanceConfig.cs` 현재 기본값: `DayDurationSec=180f`, `NightDurationSec=180f`, `EveningTransitionSec=30f`, `DawnTransitionSec=30f`.
  - 기획서 원안(40초)과도, 구현분석 v0.2.0 시점 값(540초)과도 다른 **제3의 값(180초)**으로, 현재 소스가 두 문서 사이 어딘가에서 조정되었음을 뜻한다.
- 레벨 스케일: `PlayerProgression.GrantXp`에서 레벨업마다 `LevelDurationMul = 1 + (Level-1) * 0.25` — 기획서의 "레벨-1" 스케일 공식과 일치하되, 적용 트리거가 "일자(day)"가 아니라 "플레이어 레벨"이다(기획서는 레벨 표기, 구현분석은 재확인 안 함).

### 3-2. 자원 (`Assets/Scripts/Resources/ResourceStore.cs`)
- 종류 5종: Wood, Meat, Food, Frostbloom, Stone.
- 저장 상한: Wood/Stone/Meat/Food = 100, Frostbloom = 50 (건물로 캡 증가 가능, `IncreaseCap`).
- 시작 자원(`BalanceConfig`): Wood 15, Stone 5, Meat 0, Food 5, Frostbloom 0.
- 채집: 나무 4초/+3, 돌 6초/+2, 사슴 3초/+2(고기) — 전부 `BalanceConfig` 필드로 확인.

### 3-3. 건물 — 9종 (`Assets/Scripts/Village/Building.cs` enum `BuildingKind`)
Campfire, Barricade, Fence, House, Storage, Farm, Watchtower, Infirmary, HuntersHut.
- `BuildingFactory.cs`에는 Barricade/House/Storage/Farm/Watchtower/Infirmary/HuntersHut 7종의 스폰 함수가 존재(Campfire/Fence는 다른 경로에서 생성되는 것으로 추정 — **미검증**, 이번 세션에서 해당 스폰 지점을 특정하지 못함).
- HP: Campfire 280, Barricade 280, Fence 14 (`BalanceConfig`). Storage/House/Farm/Watchtower/Infirmary/HuntersHut의 HP 기본값은 `Building.cs` 나머지 부분에서 확인 필요 — **미검증**(이번 세션에서 전체 파일 미열람).
- 건물 성장 보너스(기존 건물 수에 따라 신규 건물 HP +12%, 최대 200%)는 `InisLand_구현분석_v0.2.0.md`에 기술되어 있으나 이번 세션에서 해당 코드 라인은 직접 대조하지 못함 — **미검증(문서 근거만 있음)**.

### 3-4. 룬 시스템 — 17종 enum, 12종 실제 드롭 (`PlayerProgression.cs`)
- `RuneKind` enum 총 17개 값: 레거시 5종(DamageUp/FireRateUp/HpUp/RangeUp/MoveSpeedUp, 구 세이브 호환용으로 enum만 보존) + 효과 기반 12종(PoisonBlade, IceArrow, MultiShot, Detonator, LightningStrike, SummonDog, SummonHawk, Vampirism, Thorns, Pierce, AllyBoost, ResourceGift).
- `PickThreeOffer`는 레거시 5종을 제외한 12종 풀에서만 3장을 뽑는다(가중치 각 2, 최대 스택 도달 시 풀에서 제외).
- 스택: 룬당 최대 3스택(1=기본, 2=강화, 3=마스터). 원소 룬(독/얼음/번개) 중 2종 이상 마스터 시 전역 시너지 배율 1.5배(`ElementalMasterSynergy`).
- 즉시 효과 룬: SummonDog/SummonHawk(펫 소환, 스택별 마리 수·데미지 증가), ResourceGift(스택 1/2/3 = Wood 10/20/40, Stone 5/10/20, Food 5/10/20, Frostbloom 0/0/1 즉시 지급).

### 3-5. 좀비 웨이브 — 변형 4종 + 보스 4형태 (`Assets/Scripts/Enemies/NightController.cs`)
- 변종 4종(`Variant` enum): Normal(기본), Fast(day3+ 확률 30%, HP×0.6, 속도×1.6), Archer(day4+ 확률 20%, 원거리 6+dmg, 사거리 7u), Tank(day7+ 확률 18%, HP×2.2, 속도×0.6).
- 일자별 강화: 매일 HP +12%, 데미지 +1, 속도 +2%(최대 +50% 캡).
- 웨이브 규모: `BaseWaveCount`(기본 8) + `(day-1) * PerDayIncrement`(기본 6), 블리자드 발생 시 ×2. 블리자드 확률 20%(`BlizzardChance`), 모닥불 반경 밖에서 초당 2 데미지 추가.
- 보스: 25킬(`BossKillThreshold`) 도달 또는 5일 주기(`day % 5 == 0`) 중 먼저 오는 조건으로 소환, HP `100 + day*100`, 데미지 보너스 `18 + (day-1)*2`. 스프라이트는 day 1=FrostZombie, 2=WinterKnight, 3=IronGiant, 4일 이후는 모두 FrostLich(고유 보스는 3종+반복 1종, 기획서의 "4형태"는 스프라이트 종류 수 기준으로 일치).

### 3-6. NPC/동료 — 5종 아키타입 (`Assets/Scripts/World/ProceduralSpawner.cs` 약 492~545행)
사냥꾼(전투4/농사2), 전사(전투5/농사1), 농부(전투1/농사5), 아이(전투0/농사2), 노인(전투1/농사5). 기획서의 "치유사/무사"는 코드에는 없고 "농부/전사"로 명명되어 있어 `InisLand_구현분석_v0.2.0.md`의 명칭 변경 지적과 일치함을 재확인.
- 영입 한도: `FreeCapacity=12`(그레이스), House 1채당 `CapacityPerHouse=4` 추가. 일일 한도는 제거됨(코드 주석: "월드 동시 NPC 캡으로 자연 제한").

### 3-7. 무기 — 코드-주석 불일치 발견 (`Assets/Scripts/Combat/WeaponCatalog.cs`)
- 클래스 주석은 "6종 기본 무기 풀"이라 적혀 있으나, `Build()`가 실제로 생성하는 무기는 **2종뿐**: Longsword(근접, dmg 28, range 1.8, cd 1.0), Bow(원거리, dmg 6, range 7.0, cd 0.55, 투사체 속도 12). 나머지 4종은 코드에 존재하지 않는다 — **주석과 실제 콘텐츠 수량 불일치, 사실로 확인**.

### 3-8. 업적 — 9종 (`Assets/Scripts/Progression/AchievementManager.cs`)
first_kill(첫 처치), kills_25, kills_100, first_comp(첫 동료), five_comp(동시 5인), survive_3/7/15(일자 생존), score_300. 세션 한정(영구 저장 아님, 주석으로 명시).

### 3-9. 미검증 항목 총괄 목록
1. Campfire/Fence 스폰 코드 위치
2. Storage/House/Farm/Watchtower/Infirmary/HuntersHut 각 건물 HP 기본값
3. "건물 수 증가 시 HP+12%(최대 200%)" 보너스의 현재 코드 존재 여부
4. 실측 플레이테스트 KPI 데이터(Day3 생존율, 세션 길이 실측)
5. 온보딩 슬라이드가 실제 조작키(E 통합)와 일치하는지 UI 렌더 스크린샷 대조 (코드 상 `OnboardingController` 문구는 "E 키 나무/돌 채집"으로 이미 통합 반영되어 있음을 확인했으나, 실제 빌드 화면 대조는 미실시)
6. 접근성 기능(색맹 모드, 자막, 리매핑) 유무
7. 애널리틱스/이벤트 로깅 인프라 유무

---

## 4. 세션 길이별 경험

- **30초**: `OnboardingController` 2슬라이드(낮 조작 안내 → 밤 위협 안내) 통과 또는 스킵 직후, `HudGuidanceText.Build`가 "Explore and stockpile / No immediate threat / Gather wood, stone, or scout" 기본 문구를 즉시 노출해 첫 행동을 지시한다.
- **5분**: 낮 페이즈 1회(현재 기본값 180초) 동안 나무/돌/사슴 채집, NPC 조우 시 F키 영입 시도, 첫 건물(Barricade/Campfire류) 배치 판단까지 도달 가능한 시간대.
- **30분**: 현재 `DayDurationSec+NightDurationSec+Evening+Dawn = 180+180+30+30 = 420초(7분)`가 한 사이클이므로, 30분이면 약 4사이클 내외 진행 — 룬 3~4회 선택, 웨이브 난이도 상승(변종 Fast/Archer 등장 구간 day3~4)을 체감하는 시점.
- **장기 세션(수 사이클 이상)**: day 7 이후 Tank 변종 등장, 5일 주기 보스 반복, 업적 survive_7/survive_15·kills_100·score_300 도달 여부가 성장 체감의 축. 저장/불러오기는 `Assets/Scripts/Persistence/SaveLoad.cs` 존재로 세션 간 이어하기가 구현되어 있음(내용 상세는 이번 세션에서 미열람 — **미검증**).

---

## 5. 재미의 연쇄 (Fun Chain)

`채집(낮, 즉시 자원 획득) → 건설/자원 배분(판단) → 밤 방어(모닥불·바리케이드로 결과 확인) → 처치 XP·룬 선택(성장 체감) → 다음 낮(레벨업으로 늘어난 사이클 길이/난이도 자각) → 반복`

각 연결 고리는 코드로 확인된 이벤트 기반 트리거로 연결된다: `EventBus`의 `DayStartedPayload`/`NightStartedPayload`/`DawnStartedPayload`가 페이즈 전환을 구독자(`NightController`, `HudGuidanceText` 호출부 등)에 전파하며, `PlayerProgression.GrantXp`가 레벨업 시 다음 사이클 길이(`LevelDurationMul`)를 즉시 갱신해 "성장이 다음 도전 강도에 반영된다"는 인과를 코드 수준에서 보장한다.

---

## 6. 실제 플레이 예시 2개 (코드/데이터 기반)

**예시 1 — Day 1 첫 밤(보스 없음, 코드값 대입)**
플레이어가 낮 180초 동안 나무 3그루(각 4초, +3씩=+9), 돌 1개(6초, +2) 채집. 밤 시작 시 `NightController.StartNight(1)`이 실행되어 `basePending = 8 + 0*6 = 8`마리 예약, 블리자드 확률 20% 불발 가정 시 8마리 좀비가 `BetweenSpawnsSec=0.6초` 간격으로 스폰된다. Day1이므로 변종 확률 조건(Fast는 day>=3)이 모두 거짓이라 전부 Normal 변종(HP 20, 데미지 8). 25킬 미만이면 보스 미소환(`day % 5 == 0` 조건도 거짓)이라 이 밤은 보스 없이 종료된다.

**예시 2 — Day 5 밤(보스 강제 소환 + 블리자드 겹침)**
`day=5`이므로 `IsBossNight = (5 % 5 == 0) = true`가 되어 밤 시작과 동시에 `BossWarningThenSpawn` 코루틴이 3초 경고 후 보스를 소환한다(스프라이트는 day=5→4일 이후 분기로 `BossFrostLich`). 동시에 `_rng.Next() < 0.2`가 참이면 블리자드가 겹쳐 웨이브 수가 2배(`basePending*2`)가 되고 스폰 간격도 0.6초×0.6=0.36초로 단축된다. 이 조합에서는 플레이어가 모닥불 반경(128) 밖으로 나가면 블리자드 추가 데미지(초당 2)까지 받으므로, "모닥불 근처에서 방어" 전략이 강제된다 — 디자인 필러 2("모닥불 = 마을의 심장")가 코드 규칙으로 직접 구현된 사례.

---

## 7. 피로도/실패 완화 장치 — [사실: 코드 확인]

- 새벽 회복: `기획서.md`에 명시된 "HP 복구·건물 +25%HP·NPC 1명 스폰"은 `구현분석_v0.2.0.md`에서 "정확히 구현"으로 표기되었으나, 이번 세션에서 해당 코드(`DayNightController`/`Building` 새벽 처리부)를 줄 단위로 재대조하지 못함 — **부분 미검증**.
- 모닥불 반경 내 보호: `NightController.ApplyBlizzardDamage`가 플레이어가 `CampfireAura.Radius` 안에 있으면 블리자드 데미지를 면제한다(코드로 확인).
- 룬 즉시 자원 보급(ResourceGift)이 실패 후 회복 수단으로 기능.
- 영입 인구 한도가 그레이스 12명으로 시작해 집이 없어도 최소 동료 확보가 가능(`FreeCapacity=12`).

---

## 8. 경제/성장/밸런스 레버 — [사실: `BalanceConfig.cs` 필드 전체]

| 레버 | 현재 값 |
|---|---|
| 낮/밤/전환 시간 | 180 / 180 / 30 / 30 초 |
| 시야 반경 | 낮 10 / 밤 6 / 메가블리자드 3 타일 |
| 채집 속도·수확량 | 나무 4s→+3, 돌 6s→+2, 사슴 3s→+2 |
| 플레이어 | HP 100, 이속 180, 리스폰 3s, 방어 0 |
| 좀비 기본 | HP 20, 이속 60, 공격 8, 쿨다운 1s, 사거리 36 |
| 웨이브 | 기본 8마리, 일당 +4(주: `WavePerDay`는 필드로 존재하되 `NightController`의 실제 증가값은 `PerDayIncrement=6`으로 별도 관리 — **두 값이 다른 곳에 이원화되어 있음, 사실로 확인**), 최대 300 |
| 건물 HP | Campfire 280, Barricade 280, Fence 14 |
| 건물 비용 | Campfire 5W, Barricade 5W |
| 모닥불 오라 | DPS 2.5, 반경 128, 공격버프 +15%, HP 드레인 0.5/s |

이 레버들은 전부 `Resources/BalanceConfig.asset`으로 배포되며 Inspector에서 재조정 가능한 단일 소스로 설계되어 있다(코드 주석 확인).

---

## 9. 온보딩 / UI-HUD 5가지 상태 / 접근성 / 오디오-비주얼

### 온보딩 — 구현됨
`OnboardingController.cs`: 2슬라이드(낮 조작+모닥불 경고, 밤 웨이브+모닥불 방어 규칙) IMGUI 튜토리얼, Enter/Space/클릭으로 진행, 완료 시 `SnowfieldScene` 로드.

### HUD 5가지 상태 — 코드에서 실제로 분기되는 4가지 확인 + 1가지는 구조적으로 존재
`HudGuidanceText.Build(phase, activeZombies, wavePending, isBlizzard, foodShortage)`가 아래 4가지 안내 상태를 반환한다(전부 코드로 확인):
1. 밤 + 블리자드: "Survive the blizzard night" / 위험(danger) / "모닥불로 복귀"
2. 밤(블리자드 아님): "Hold the village line" / 위험(danger) / "정문 방어"
3. 낮 + 식량 부족: "Restore food supplies" / 경고(warning) / "채집/사냥 우선"
4. 낮 기본(위협 없음): "Explore and stockpile" / 안전(safe) / "자원 수집·정찰"
- 5번째 상태로 볼 수 있는 것은 온보딩 슬라이드 오버레이(별도 UI, `OnboardingController`)로, `HudGuidance` 구조체 자체가 아니라 씬 전환 이전 단계라는 점에서 이전 GDD가 제시한 "가이던스 툴팁 오버레이"와는 다른 컴포넌트다 — 5상태 프레임을 유지하되 실체는 "온보딩 오버레이 + 4가지 인게임 가이던스"로 정정.

### 접근성 — 미검증
색맹 모드, 자막, 컨트롤 리매핑 관련 코드나 설정 파일을 이번 세션에서 찾지 못함. 라이트 플리커(`LightFlicker.cs`) 강도 조절 옵션도 확인되지 않음 — **미검증, 제안 단계 유지**.

### 오디오-비주얼 — [사실: 기획서 본문 + `Assets/Scripts/Audio`, `Assets/Scripts/UI/Sfx.cs`, `Music.cs` 존재]
BGM 1곡(Kenney CC0 루프), SFX 6종(결정론적 CC0 WAV, 입력/액션/위험/전환/성공/실패), 권장 믹스 BGM 0.28 / SFX 0.70(기획서 명시). `RuntimeAudioDirector.cs`와 EditMode 테스트(`RuntimeAudioDirectorTests.cs`)가 존재해 오디오 방향성 로직에 대한 자동 테스트가 있음을 확인.

---

## 10. 구현됨 / 계획됨 / 제안 구분

**구현됨(코드로 직접 확인)**
- 낮/밤/저녁/새벽 4단계 사이클과 EventBus 연동
- 채집 3종(나무/돌/사슴), 자원 5종 + 상한
- 건물 9종 enum, 그중 7종 스폰 팩토리 함수
- 룬 17종 enum / 12종 드롭 풀 / 3스택 / 원소 시너지
- 좀비 변종 4종 + 보스 4스프라이트, 일자별 강화 수식
- NPC 5종 아키타입, 영입 인구 한도 로직
- HUD 가이던스 4상태, 온보딩 2슬라이드
- 업적 9종(세션 한정)
- 무기 2종(Longsword, Bow) — **주석상 "6종"과 불일치**

**계획됨/부분 미검증(문서에는 있으나 코드 재확인 안 됨)**
- 새벽 회복의 "건물 +25%HP, NPC 1명 스폰" 정확도
- 건물별 개별 HP 기본값(Campfire/Barricade/Fence 외)
- 건물 수 기반 HP 성장 보너스(+12%, 최대 200%)
- 저장/불러오기(`SaveLoad.cs`) 상세 동작

**제안(이번 GDD 작성자 제안, 미구현/미검증)**
- 접근성 옵션(색맹 모드, 라이트 플리커 강도 조절, 자막)
- 실제 플레이테스트 기반 KPI 계측 인프라
- 무기 카탈로그를 주석대로 6종까지 확장하거나, 주석을 2종에 맞게 수정

---

## 11. 다음 우선순위 3가지

1. **버전 정합**: package.json(0.4.0) / 기획서 헤더(v0.3.0) / BuildValidation(0.5.0) / portable exe 파일명(v0.3.0) 4곳의 불일치를 하나의 소스오브트루스로 정리한다.
2. **무기 카탈로그 주석-코드 불일치 해소**: `WeaponCatalog.cs`가 "6종"이라 주석했지만 실제로는 2종만 구현되어 있으므로, 4종을 추가 구현하거나 주석/기획 문서를 2종 기준으로 수정한다.
3. **건물 HP·성장 보너스 재검증**: Storage/House/Farm/Watchtower/Infirmary/HuntersHut의 HP 기본값과 "건물 수 기반 HP+12%" 보너스가 현재 코드에 실재하는지 `Building.cs` 전체와 `VillageStarter.cs`를 대조해 확정한다.

---

## 12. 근거(Evidence) 노트 — 섹션별 참조 파일

- 버전 표기: `package.json`, `docs/InisLand_기획서.md`(헤더 및 "Release/Verification" 절), `docs/InisLand_BuildValidation_2026-09-08.md`, `ProjectSettings/ProjectVersion.txt`, 루트 `InisLand_v0.3.0_portable.exe` 파일명
- 핵심 재미/디자인 필러: `Assets/Scripts/Cycle/DayNightController.cs`, `Assets/Scripts/Enemies/NightController.cs`, `Assets/Scripts/Combat/PlayerProgression.cs`, `Assets/Scripts/Combat/CampfireAura.cs`
- 컴포넌트 표: 위 파일들 + `Assets/Scripts/Village/Building.cs`, `Assets/Scripts/Village/BuildingFactory.cs`, `Assets/Scripts/Companions/RecruitableNpc.cs`, `Assets/Scripts/UI/HudGuidanceText.cs`, `Assets/Scripts/Progression/AchievementManager.cs`
- 콘텐츠 수량: `Assets/Scripts/Config/BalanceConfig.cs`, `Assets/Scripts/Resources/ResourceStore.cs`, `Assets/Scripts/Village/Building.cs`(enum), `Assets/Scripts/Village/BuildingFactory.cs`, `Assets/Scripts/Combat/PlayerProgression.cs`, `Assets/Scripts/Enemies/NightController.cs`, `Assets/Scripts/World/ProceduralSpawner.cs`(492~545행), `Assets/Scripts/Combat/WeaponCatalog.cs`, `Assets/Scripts/Progression/AchievementManager.cs`
- 세션 길이별 경험/재미의 연쇄/플레이 예시: 위 `BalanceConfig`, `NightController`, `PlayerProgression`, `EventBus`/`GameEvents` 값을 직접 계산해 산출(추정 아닌 산술 대입)
- 온보딩/HUD 상태: `Assets/Scripts/UI/OnboardingController.cs`, `Assets/Scripts/UI/HudGuidanceText.cs`
- 오디오: `docs/InisLand_기획서.md`("오디오 시스템" 절), `Assets/Scripts/Audio/RuntimeAudioDirector.cs`, `Assets/Scripts/UI/Sfx.cs`, `Assets/Scripts/UI/Music.cs`, `Assets/Tests/EditMode/RuntimeAudioDirectorTests.cs`
- 기존 기획서/구현분석 대조: `docs/InisLand_기획서.md`, `docs/InisLand_구현분석_v0.2.0.md`, `docs/InisLand_페르소나_플레이_피드백_v0.2.0.md`(이번 세션에서 미열람 — 존재만 확인), `docs/next_improvement_instruction.md`
- 미검증 항목: 이번 세션에서 열람하지 못한 파일(예: `Building.cs` 전체, `VillageStarter.cs`, `SaveLoad.cs` 상세, `Assets/Scripts/Persistence/SaveLoad.cs`)은 본문에 명시적으로 "미검증"으로 표기

---

## 부록: 대표 이미지 (기존 참조 보존)

![InisLand gameplay preview](./InisLand_gameplay_preview.png)
