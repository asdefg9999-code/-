# 불멸의 기관 — Unity 3D 액션 RPG 프로토타입

원작 소설 《불멸의 기관》을 기반으로 한 **독창적인 3인칭 모바일 액션 RPG 프로젝트**입니다.

프로토타입 이름: **제1장 · 침식의 숲**  
권장 버전: **Unity 6 (6000.0 이상)**  
개발 단계: **프로토타입 0.1 — Unity 에디터 실행 및 디버그 APK 빌드용 소스**

## 1. Unity에서 실행

1. Unity Hub에서 Unity 6 버전을 설치합니다. 스마트폰 빌드를 위해 **Android Build Support**, Android SDK & NDK Tools, OpenJDK 모듈도 선택합니다.
2. 이 ZIP을 압축 해제합니다. `Assets`, `Packages`, `ProjectSettings`가 같은 폴더에 있는 상태여야 합니다.
3. Unity Hub → **Add project from disk** → 압축을 해제한 `ImmortalOrgan_Unity3D` 폴더 선택.
4. 처음 열 때 Unity가 필요한 패키지를 설치하고 스크립트를 컴파일합니다. 연결 상태에 따라 몇 분 걸릴 수 있습니다.
5. 메뉴 **Immortal Organ → 1 · Open playable scene**을 클릭합니다.
6. Unity 상단 **Play ▶** 버튼을 누르고 게임에서 `탐험 시작`을 선택합니다.

최초 프로젝트 로딩 시 에디터 스크립트가 `Assets/Scenes/ImmortalOrgan_Prototype.unity` 장면을 자동 생성하고 빌드 목록에도 추가합니다. 씬에 게임오브젝트를 수동으로 배치할 필요는 없습니다.

## 2. 조작법

| 기능 | PC | 모바일 |
|---|---|---|
| 이동 | WASD / 방향키 | 왼쪽 가상 조이스틱 |
| 카메라 회전 | 마우스 오른쪽 버튼 드래그 | 화면 오른쪽 빈 공간 스와이프 |
| 점프 | Space | 점프 버튼 |
| 대시 · 회피 | 왼쪽 Shift | 회피 버튼 |
| 기본 공격 | 마우스 왼쪽 / J | 공격 버튼 |
| 혈류 개방 | Q | 혈류 개방 버튼 |
| 세포 재생 | R | 재생 버튼 |
| L-Cell 잔향 수집 | E | 조사 버튼 |
| 일시정지 | Esc | 메뉴 버튼 |

스마트폰은 가로 화면을 기본으로 사용합니다.

## 3. 플레이 목표

1. 폐쇄 연구소가 있는 수목원을 탐험합니다.
2. **L-Cell 잔향 3개**를 찾아 수집합니다. 아이템 근처에서 `조사`를 누릅니다.
3. 일반 거머리형 침식체들을 처치해 혈액 공명을 모읍니다.
4. 세 개의 잔향이 공명하면 연구소 앞에 **코어 잔향 수호자**가 나타납니다.
5. 보스를 처치하면 제1장 체험판이 완료됩니다.

이 보스는 실제 코어를 드롭하는 개체가 아닌, 실험시설에 남아 있는 **코어 정보의 잔향**입니다. 원작의 코어 획득 설정을 변경하지 않습니다.

## 4. 독자적인 전투 시스템

- **혈흔 베기:** 기본 공격. 피해를 주면서 L-Cell 공명 에너지를 충전하고 침식도를 일부 낮춥니다.
- **혈류 개방:** 공명 에너지 35 소비. 반경 4m 광역 피해. 침식도 상승.
- **세포 재생:** 공명 에너지 26 소비. 체력 28 회복. 침식도 상승.
- **회피 대시:** 공명 에너지 8 소비. 순간 이동 속도 상승, 짧은 피격 무적.
- **침식도 과부하:** 침식도가 100에 도달하면 체력을 잃고 4초간 혈류 개방을 사용할 수 없습니다.

L-Cell은 단순한 마나가 아니라 **재생과 침식 사이에서 위험을 감수하는 자원**입니다.

## 5. Android APK 만들기

1. Unity Hub에서 해당 에디터 버전의 **Android Build Support** 모듈이 설치되어 있는지 확인합니다.
2. Unity → **File → Build Profiles**에서 Android 플랫폼을 선택하고 전환합니다.
3. 메뉴 **Immortal Organ → 2 · Build Android debug APK**를 클릭합니다.
4. 성공하면 프로젝트의 `Builds/ImmortalOrgan_Android_Debug.apk` 파일이 생성됩니다.
5. 테스트용 APK를 본인 스마트폰으로 옮겨 설치합니다. Android의 설치 권한 안내가 필요할 수 있습니다.

이것은 테스트 APK 제작 기능이며 Google Play 스토어 배포용 서명·최적화·심사 절차는 별도입니다.

## 6. 주요 파일

| 파일 | 역할 |
|---|---|
| `GameRuntime.cs` | 게임 초기화·승리·사망 상태 |
| `WorldFactory.cs` | 숲·연구소·몬스터·캐릭터를 3D로 자동 생성 |
| `PlayerMotor.cs` | 3인칭 이동, 점프, 대시 |
| `OrbitCamera.cs` | 추적 카메라, 충돌 회피, 회전/줌 |
| `BloodCombat.cs` | L-Cell·침식도·혈액 전투 |
| `EnemyAgent.cs` | 적 탐지·추적·공격·체력 |
| `QuestDirector.cs` | 잔향 3개 수집, 보스 개방, 챕터 완료 |
| `AnimeCharacter.cs` | 임시 3D 최민호 캐릭터와 간단한 움직임 |
| `GameHud.cs` | 체력·공명·퀘스트 HUD와 모바일 버튼 |
| `GameInput.cs` | Unity Input System 및 레거시 입력 호환 |
| `PrototypeSceneSetup.cs` | 최초 씬 생성 및 Android 디버그 빌드 메뉴 |

`Assets/Resources/MinhoConcept.png`는 게임의 **오리지널 애니풍 캐릭터 원화**입니다. 현재 필드에서 움직이는 캐릭터는 별도로 생성한 **임시 3D 조형**이므로, 최종 작품에서는 이 원화를 기반으로 한 리깅된 3D 모델과 애니메이션으로 교체해야 합니다.

## 7. 현 단계의 제한

- 완성된 대규모 오픈월드가 아닌 **1개 탐험 지역**입니다.
- 임시 3D 프리미티브를 사용하며, 고품질 셀셰이딩·전용 애니메이션·음성·음악은 아직 없습니다.
- 몬스터 행동은 간단한 추적/근접 공격 AI이며, NavMesh·멀티플레이·서버 저장은 구현되지 않았습니다.
- 이 프로젝트를 작성한 환경에는 Unity 에디터가 없어 **Unity 컴파일·Android 실제 빌드·휴대폰 실기 검증은 수행하지 못했습니다.** 프로젝트를 열어 오류가 발견되면 해당 Unity Console 로그를 기준으로 수정하면 됩니다.

## 8. 참고 문서

PC 없이 클라우드에서 Android APK를 빌드하려면 [CLOUD_BUILD.md](CLOUD_BUILD.md)를 참고하세요.

- [Unity 6 CharacterController.Move](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/CharacterController.Move.html)
- [Unity 6 Android Build Settings](https://docs.unity3d.com/6000.0/Documentation/Manual/android-build-settings.html)
- [Unity Input System 1.11 설치](https://docs.unity.cn/Packages/com.unity.inputsystem@1.11/manual/Installation.html)

기존 모바일 웹 체험판은 별도로 유지됩니다. Unity 프로젝트는 네이티브 3D 제작 단계의 소스입니다.
