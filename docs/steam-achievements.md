# Snowfield — Store Achievements

> **Platform Note:** Snowfield targets Android and iOS via Capacitor. Steam achievements are not applicable for mobile releases.
> 
> For **Google Play**, use the Play Games Services Achievements API.  
> For **App Store**, use GameKit Achievements.

---

## Google Play Achievements (Planned)

| ID (internal) | EN Name | KO Name | How to Unlock |
|----------------|---------|---------|---------------|
| `ACH_FIRST_HUNT` | First Hunt | 첫 번째 사냥 | Complete your first day cycle hunt |
| `ACH_FIRST_NIGHT` | Night Defender | 밤의 수호자 | Survive your first night wave |
| `ACH_VILLAGE_BUILD` | Village Founder | 마을 창건자 | Build 5 village structures |
| `ACH_FIRST_DEATH` | Permadeath Lesson | 죽음의 교훈 | Lose a companion permanently |
| `ACH_BLIZZARD` | Blizzard Survivor | 눈보라 생존자 | Survive a blizzard event |
| `ACH_DEEP_EXPLORE` | Into the White | 하얀 심연으로 | Explore past the fog boundary |
| `ACH_RECRUIT_5` | Full Party | 완전한 파티 | Recruit 5 wanderers total |
| `ACH_VILLAGE_MAX` | Snowfield Settlement | 설원 정착지 | Upgrade the village to max level |
| `ACH_WEEK_SURVIVE` | Week in the Cold | 추위 속 일주일 | Survive 7 in-game days |
| `ACH_PERFECT_NIGHT` | Perfect Defense | 완벽한 밤 | Survive a night wave without any companion dying |

---

## Implementation References

- **Google Play Games SDK**: `com.google.android.gms:play-services-games`
- **Unlock method**: `GamesClient.unlockAchievement(achievementId)`
- **Display**: `GamesClient.getAchievementsIntent()`
- Setup in Google Play Console → Achievements section before publishing

## iOS (GameKit)

```swift
GKAchievement(identifier: "ACHIEVEMENT_ID")
achievement.percentComplete = 100
GKAchievement.report([achievement])
```
