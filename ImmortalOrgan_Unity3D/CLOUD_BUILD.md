# 불멸의 기관 — 갤럭시용 Unity Cloud Build 설정

목적: Unity 6 프로젝트를 **Android APK**로 빌드하여 갤럭시에 직접 설치해 테스트합니다.

## 저장소

- GitHub에는 ZIP 파일 자체가 아닌, 이 디렉터리의 **Assets**, **Packages**, **ProjectSettings** 및 나머지 파일을 올립니다.
- `Assets`, `Packages`, `ProjectSettings`가 GitHub 저장소의 루트에 있으면 `Project subfolder path`는 비웁니다.
- 상위 폴더 `ImmortalOrgan_Unity3D` 안에 올렸다면 `Project subfolder path`에 `ImmortalOrgan_Unity3D`를 지정합니다.
- 소스가 담긴 저장소는 **Private**을 권장합니다.

## Unity Dashboard

1. https://cloud.unity.com/home 로그인 → 프로젝트 선택 → **DevOps > Build Automation**.
2. **Settings > Source control**에서 GitHub 저장소와 기본 브랜치(`main` 등)를 연결합니다.
3. **Configurations > New build configuration**에서 플랫폼 `Android`를 선택합니다.
4. **Unity version**을 `ProjectSettings/ProjectVersion.txt`의 `6000.0.23f1`과 일치시킵니다. 이 버전이 서비스에서 지원되지 않으면 호환 버전을 확인한 뒤 프로젝트 버전과 패키지를 함께 점검해야 합니다.
5. Android 설정에서 **Build app bundles (.aab) instead of an APK**를 끕니다.
6. Advanced settings → **Pre-export method**:
   `ImmortalOrgan.EditorTools.CloudBuildHooks.PreExport`
7. 개발·자가 테스트용 Android 서명을 설정합니다. 디버그 APK도 유효한 서명이 있어야 설치 가능합니다. 자격 증명과 키스토어는 소스 저장소에 커밋하지 마세요.
8. **Build** 실행 → **Build History**에서 결과 확인 → 성공 결과의 `.ZIP` 아티팩트를 다운로드해 `.apk`를 꺼냅니다.

## 이 프로젝트 특이사항

- `Assets/Scenes/ImmortalOrgan_Prototype.unity`는 처음부터 저장되어 있지 않습니다.
- Unity Editor용 코드가 최초 로드/빌드 전에 씬을 생성해 빌드 목록에 등록합니다.
- 클라우드 빌드의 **Pre-export method**를 반드시 위 이름으로 설정하는 편이 안전합니다.
- 이 프로젝트는 에디터와 Android 실제 기기에서 **컴파일·실행 미검증** 상태입니다. 빌드 실패 시 Build History의 오류 로그가 필요합니다.

## 참고 공식 문서

- [Unity Build Automation 구성](https://docs.unity.com/en-us/build-automation/basic-build-configuration/overview)
- [빌드 전 사용자 스크립트 실행](https://docs.unity.com/en-us/build-automation/advanced-build-configuration/run-custom-scripts-during-the-build-process)
- [빌드 결과 다운로드](https://docs.unity.com/en-us/build-automation/check-build-results/overview)
