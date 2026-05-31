# inking 저장소 가이드 (클론 & 브랜치)

다른 컴퓨터에서 `inking` Unity 프로젝트를 받고 브랜치로 작업하는 방법입니다.

- 원격 저장소: `https://github.com/starpeople1008/ink_game.git`
- 기본 브랜치: `main`
- 엔진: Unity (프로젝트 버전은 `ProjectSettings/ProjectVersion.txt` 참고)

---

## 0. 사전 준비 (새 컴퓨터에서 한 번만)

1. **Git 설치** — https://git-scm.com/downloads
2. **Unity 설치** — Unity Hub로 `ProjectVersion.txt`에 적힌 버전과 동일하게 설치 (버전이 다르면 에셋이 깨질 수 있음)
3. **Git 사용자 정보 설정** (커밋 작성자 표시용)
   ```bash
   git config --global user.name "본인이름"
   git config --global user.email "본인이메일"
   ```
4. (Windows 권장) 줄바꿈 자동 변환 설정
   ```bash
   git config --global core.autocrlf true
   ```

---

## 1. 저장소 클론(받기)

원하는 작업 폴더로 이동한 뒤 클론합니다. **홈 폴더(C:\Users\이름)에 직접 클론하지 마세요.**

```bash
# 예: D:\Projects 아래에 받기
cd D:/Projects
git clone https://github.com/starpeople1008/ink_game.git inking
cd inking
```

- `inking` 폴더가 생기고 그 안에 자체 `.git`이 들어옵니다.
- `Library/`, `Temp/`, `obj/` 등은 `.gitignore`로 제외돼 있어 받아지지 않습니다. **정상입니다.**

### 클론 후 Unity 첫 실행

1. Unity Hub → `Add` → 방금 받은 `inking` 폴더 선택
2. 프로젝트를 열면 Unity가 `Library/`를 자동으로 다시 생성합니다 (수 분 소요될 수 있음). 받지 않아서가 아니라 원래 로컬에서 생성되는 폴더입니다.

---

## 2. 최신 상태로 업데이트

작업 시작 전에는 항상 원격 최신 내용을 받아옵니다.

```bash
git switch main      # main 브랜치인지 확인
git pull             # 원격 main의 최신 커밋 받기
```

---

## 3. 브랜치 개설(생성) & 작업

`main`에 직접 작업하지 말고, 기능별로 브랜치를 만들어 작업하는 것을 권장합니다.

### 3-1. 새 브랜치 만들고 그 브랜치로 이동

```bash
# main 최신 상태에서 시작
git switch main
git pull

# 새 브랜치 생성 + 이동 (한 번에)
git switch -c feature/플레이어-이동
```

> 브랜치 이름 예시
> - `feature/적-AI` — 새 기능
> - `fix/충돌-버그` — 버그 수정
> - `test/밸런스-실험` — 실험용

### 3-2. 작업 → 커밋

```bash
git add -A                       # 변경 파일 모두 스테이징
git commit -m "플레이어 이동 구현"   # 커밋 메시지 작성
```

### 3-3. 브랜치를 원격(GitHub)에 올리기

새 브랜치는 처음 push할 때 `-u`로 원격과 연결합니다.

```bash
git push -u origin feature/플레이어-이동
```

이후 같은 브랜치에서는 그냥 `git push` 만 하면 됩니다.

---

## 4. 브랜치 작업 마무리 (main에 합치기)

GitHub에서 **Pull Request(PR)** 를 만들어 합치는 방식을 권장합니다.

1. push 후 GitHub 저장소 페이지에 들어가면 "Compare & pull request" 버튼이 나타납니다.
2. PR을 생성하고 내용 검토 후 `main`으로 Merge 합니다.
3. 합친 뒤 로컬 정리:
   ```bash
   git switch main
   git pull
   git branch -d feature/플레이어-이동    # 로컬 브랜치 삭제
   ```

> 명령줄로 직접 합치고 싶다면:
> ```bash
> git switch main
> git pull
> git merge feature/플레이어-이동
> git push
> ```

---

## 5. 자주 쓰는 명령 모음

| 목적 | 명령 |
|------|------|
| 현재 브랜치/상태 확인 | `git status` |
| 브랜치 목록 보기 | `git branch` (원격 포함: `git branch -a`) |
| 브랜치 이동 | `git switch 브랜치이름` |
| 새 브랜치 생성+이동 | `git switch -c 새브랜치` |
| 변경 내용 받기 | `git pull` |
| 변경 내용 올리기 | `git push` |
| 커밋 기록 보기 | `git log --oneline --graph --all` |
| 원격 주소 확인 | `git remote -v` |

---

## 6. 주의사항

- **항상 `inking` 폴더 안에서** git 명령을 실행하세요. 다른 폴더(특히 홈 폴더)에서 실행하면 엉뚱한 저장소를 건드릴 수 있습니다.
- `Library/`, `Temp/`, `obj/` 등은 커밋하지 않습니다 (`.gitignore`가 자동 처리).
- Unity의 `.meta` 파일은 **반드시 함께 커밋**해야 합니다. (에셋과 짝을 이루므로 임의로 삭제 금지)
- 여러 명이 같은 씬/프리팹을 동시에 수정하면 충돌이 잘 납니다. `.gitattributes`에 Unity 머지 도구 설정이 있지만, 가능하면 작업 분담을 나누세요.
