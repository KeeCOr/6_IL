# Project Instructions

## Project Identity

`6_IL / Snowfield`는 중세 판타지 모바일 생존 게임이다. 낮(사냥/탐험)과 밤(방어)의 2사이클 루프, 영구사망 동료 시스템이 핵심 정체성이다. **플랫폼: Android (우선) / iOS (macOS 필요)**.

## Authoritative Stack

- React + TypeScript + Vite + Capacitor v7 (v0.1.0)
- 빌드: `npm run build` → `npm run cap:sync` → Android Studio
- 테스트: `npx vitest run`
- 타입 체크: `npm run typecheck`
- 린트: `npm run lint`
- 경로: `C:/Users/bada/6_IL`

## Structure

- `src/`: TypeScript + React 게임 소스
- `android/`: Capacitor 안드로이드 플랫폼 (gitignored — `npx cap add android`로 재생성)
- `capacitor.config.ts`: Capacitor 설정 (`appId: com.stoic.snowfield`)
- `docs/`: GDD, 출시 가이드, 아트 리소스

## Build And Verification

```powershell
# 웹 빌드 + Capacitor 동기화
cd C:/Users/bada/6_IL && npm run build && npx cap sync android

# Android Studio 열기
npx cap open android
```

- android/ 폴더는 gitignored — 커밋하지 않는다
- iOS 빌드는 macOS에서만 가능 (`npx cap add ios`)
- `npm run typecheck`로 TypeScript 오류 선제 확인

## Documentation Rules

- `docs/store-description.md` (모바일 스토어용) 최신 유지
- `docs/mobile-release-guide.md`에 서명 + 배포 절차 기록
- 낮/밤 사이클 변경은 GDD와 동기화

## AI-Assisted Workflow

1. Plan: 낮 사이클, 밤 사이클, 마을 건설, UI 중 어떤 부분인지 정한다
2. Split: 게임 로직, UI/React 컴포넌트, Capacitor 플러그인, 테스트를 분리한다
3. Build: TypeScript 타입 안정성을 유지하며 수정한다
4. Verify: `typecheck` → `vitest run` → `cap:sync` → Android Studio에서 확인
5. Reflect: 영구사망 규칙이나 사이클 타이밍 변경은 GDD에 남긴다

## Do Not

- `android/` 폴더를 git에 커밋하지 않는다
- Steam/Electron 관련 설정을 추가하지 않는다 (모바일 전용)
- 영구사망(permadeath) 정체성을 선택적으로 만들지 않는다
- Capacitor 없이 네이티브 앱 기능을 모방하지 않는다
