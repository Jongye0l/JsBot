# JsBot
### Jongyeol Server Discord Bot
![C#](https://img.shields.io/badge/Lang-C%23-c9c8e4.svg?&logo=c#)
![.NET](https://img.shields.io/badge/Framework-.NET_10-c9c8e4.svg?&logo=dotnet)
![Discord](https://img.shields.io/badge/Lib-NetCord-c9c8e4.svg?&logo=discord)
![Rider](https://img.shields.io/badge/IDE-Rider-c9c8e4.svg?&logo=rider)

---

### 소개
* [NetCord](https://netcord.dev) 기반의 모드 배포/공지용 Discord 봇입니다.
* 기존 Java(Eris) 기반 [JsBot-java](https://github.com/Jongye0l/JsBot-java)를 .NET으로 재작성한 프로젝트입니다.
* 모드 데이터 저장 및 웹 API 서버 역할을 하는 [JALib.Server](https://github.com/Jongye0l/JALib.Server) 서브모듈과 같은 프로세스에서 함께 동작합니다.

---

### 주요 기능 및 특징
* **모드 릴리즈/근황 자동 공지:** 지정 채널에 `!release`, `!beta`, `!progress` 형식으로 메시지를 작성하면 버전 정보, 다운로드/소스 코드/모드 적용 버튼이 포함된 공지 메시지로 자동 변환하여 게시
* **모드별 채널·역할 자동 생성:** `/addmod` 시 릴리즈/근황/베타 역할과 전용 채널을 자동 생성하고, 최신 버전 이동 및 핑 알림 구독 버튼을 제공
* **다른 서버로의 공지 미러링:** `/mod addchannel`로 등록한 다른 길드 채널에도 릴리즈/근황 공지를 동일하게 미러링하고, 필요 시 기존 히스토리를 백필
* **역할 자가 구독:** 버튼 클릭으로 릴리즈/근황/베타 핑 역할을 스스로 토글
* **모니터링/모더레이션 명령어:** `/editmoddata`, `/removemod`, `/timeout` 등 모드 데이터 편집 및 서버 운영용 슬래시 커맨드 제공
* **감사 로그:** 모든 명령어 실행, 버튼 상호작용, 서버 입장/퇴장/부스트 이벤트를 지정 로그 채널에 Embed로 기록

---

### 기술 스택
* **Language / Runtime:** C# / .NET 10
* **Discord Library:** [NetCord](https://netcord.dev) (Gateway, Application Commands, Hosting)
* **Backend:** [JALib.Server](https://github.com/Jongye0l/JALib.Server) (ASP.NET Core, 모드 데이터/웹 API, git submodule)
* **Build Tool:** .NET SDK (`dotnet build` / `dotnet publish`)

---

### 라이선스
이 프로젝트는 **BSD 3-Clause License**를 따릅니다. 자세한 내용은 [LICENSE](./LICENSE) 파일을 참고해 주세요.

---
# [Join My Discord!](https://discord.jongyeol.kr)
