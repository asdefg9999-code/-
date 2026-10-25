# v0.2 최민호 3D 캐릭터 제작·연동 명세

상태: **원화/정면·측면·후면 시트 제작 완료, 실제 FBX/리깅/Animator/Unity 빌드는 미완료**.

## 기준 디자인

- 원본: `Assets/Resources/MinhoConcept.png`
- 신규 3면도: `Docs/Art/Minho_Turnaround_v0.2.png`
- 주인공: 한국인 청년 최민호. 검은 머리와 은빛 앞머리, 적색 눈, 검정·남색 롱코트와 버건디 안감, 왼팔의 혈관형 침식 문양을 유지.
- 장식 체인은 모바일 최적화를 위해 일부 메시/텍스처로 단순화 가능하나 실루엣·전면 흉부 문양·코트의 앞뒤 구성을 우선 보존.
- 이 문서와 3면도는 제작 참조 자료. **3면도는 3D 모델, 스킨, 리그 또는 애니메이션 파일이 아니다.**

## 외부 3D 모델 제작 납품 조건

1. 원점은 두 발 사이 바닥 `y=0`. 정면은 Unity 로컬 `+Z` 방향, 키 약 `1.82m`, 월드 스케일 1.
2. 휴머노이드 본·스킨 웨이트와 눈에 띄는 메시 교차 없는 A-포즈. 코트 자락이 다리와 충돌하지 않도록 점검.
3. 게임용 FBX 및 텍스처. 가능한 한 머티리얼 수, 투명/발광 레이어, 실시간 천 물리, 본 수를 줄여 Galaxy S21 Ultra에서 성능 시험.
4. 필요한 애니메이션: `Idle`, `Walk`, `Run`(후속), `Jump`, `Fall`(후속), `Dash`, `Slash`, `Hit`(후속), `Death`(후속).
5. Unity Humanoid Avatar 검사; 루트 모션은 끄고 `PlayerMotor`의 CharacterController를 이동의 단일 소스로 사용.
6. 타인 게임/애니의 모델, 리그, 의상, 애니메이션을 무단 추출하거나 복제하지 않음. 에셋 라이선스 추적.

## Unity 연결 방법

1. 실제 제작된 FBX 및 텍스처를 Unity 프로젝트 `Assets/Characters/Minho/`에 임포트.
2. `Rig` 탭의 Humanoid Avatar 설정, 애니메이션 클립, 본·텍스처·셰이더가 정상인지 검사.
3. `Animator Controller`를 만들고 기본 상태 `Idle`, 이동 상태 `Walk`, 공격 상태 `Slash`, 대시 상태 `Dash` 등을 구성.
4. Controller의 선택적 파라미터는 아래 표를 기준으로 연결. `AnimeCharacter`는 존재하는 파라미터만 구동함.
5. FBX 모델 루트가 바닥 원점과 정면 `+Z`를 따르는 **Prefab**을 만든 뒤 **정확히** `Assets/Resources/Characters/MinhoRigged.prefab`으로 저장.
6. Android APK를 새로 빌드하고 Galaxy 실기기에서 모델 표시·이동·카메라·공격·대시·프레임·셰이더를 시험.

| Animator 파라미터 | 유형 | 전달값 | 연결 기준 |
| --- | --- | --- | --- |
| `Speed` | Float | 정지 0, 이동 1, 대시 2 | Idle/Walk/Dash 블렌드 |
| `Grounded` | Bool | CharacterController가 지면에 있는지 | Jump/Fall 상태 |
| `Dash` | Bool | 대시 진행 중 | 회피 상태 |
| `Slash` | Trigger | `BloodCombat`의 공격 시 호출 | 공격 애니메이션 |

`AnimeCharacter.Build()`는 Prefab을 먼저 시도한다. 없거나 `Animator` 및 Controller가 없으면 **기존 프로시저럴 최민호로 자동 복귀**한다. 따라서 새 모델이 준비되지 않아도 기존 플레이 경험을 보존한다. 단, 잘못된 모델의 재질·스케일·상태 전이는 Animator 존재만으로 검증되지 않으므로 APK 실기기 테스트가 필요하다.

## 0.2 작업 체크리스트

- [x] 기존 캐릭터 원화 확인 및 3면도 제작
- [x] 선택적 rigged Prefab 연결 지점 작성 및 기존 캐릭터 fallback 유지
- [ ] 실제 게임용 3D 모델/UV/텍스처
- [ ] 휴머노이드 리깅과 애니메이션 클립
- [ ] Unity Animator Controller 및 `MinhoRigged.prefab` 배치
- [ ] Unity 에디터 컴파일·씬 테스트
- [ ] Android Cloud Build 및 Galaxy 실기기 테스트

